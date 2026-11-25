# MRKT .NET Client

An asynchronous C# client for the [MRKT](https://t.me/mrkt) API. `MrktClient` owns the HTTP
transport, and the API is organized into `MarketClient`, `SteamClient`, and `User`. Tokens
come from a string or from any `IAccessTokenProvider`.

This is a C# port of [mrkts](https://github.com/aiofake/mrkts), the asynchronous Python
client for MRKT by [aiofake](https://github.com/aiofake), published under the MIT license.
The API surface and the request formats come from the original project. This port is
independent and is not affiliated with the original author. See
[Differences from the Python client](#differences-from-the-python-client).

## Features

- Access token authentication: a token string or any `IAccessTokenProvider`
- Market methods for gifts, orders, and offers
- Steam methods, including CS2 skins
- User profile, balance, and transaction methods
- Raw requests through `MrktClient.Http`
- Concurrent multi-account operations with `MrktPool`
- Polling workflows: `Poll` and `WatchListings`

## Requirements

The .NET 10 SDK. The library has no package dependencies. The `GetToken` example uses
[WTelegramClient](https://github.com/wiz0u/WTelegramClient).

```bash
dotnet build Mrkts.slnx
```

## Quick Start

```csharp
using Mrkts;

await using var client = new MrktClient("YOUR_ACCESS_TOKEN");
await client.StartAsync();

var collections = await client.Market.Gifts.CollectionsAsync();
var profile = await client.User.MeAsync();
```

`Market`, `Steam`, and `User` are always available. `Gifts`, `Orders`, and `Offers` are
shortcuts for `Market.Gifts`, `Market.Orders`, and `Market.Offers`.

API methods return the decoded JSON response as a `JsonElement`. Response shapes are
defined by the upstream service.

`StartAsync()` creates the HTTP session and `DisposeAsync()` releases it. A client without
a token stays unstarted, and `Http` throws `InvalidOperationException`.

Without an explicit `proxy`, proxy environment variables are ignored and the client
connects directly.

## Authentication

### Token providers

`IAccessTokenProvider` supplies tokens from any source:

```csharp
using Mrkts;

public sealed class FileTokenProvider(string path) : IAccessTokenProvider
{
    public Task<string> GetTokenAsync(CancellationToken cancellationToken = default) =>
        File.ReadAllTextAsync(path, cancellationToken);
}

await using var client = new MrktClient(new FileTokenProvider("/path/to/token"));
await client.StartAsync();
```

`StartAsync()` asks the provider once, when the client has no token. Expired tokens
(`401`/`403`) are not refreshed automatically.

### Telegram session

The library contains no Telegram code. `examples/Mrkts.Examples.GetToken` includes
`TelegramTokenProvider`, an `IAccessTokenProvider` built on WTelegramClient that obtains a
token from a Telegram user session. To use it in another application, copy
`TelegramTokenProvider.cs` from the example.

```bash
dotnet run --project examples/Mrkts.Examples.GetToken -- credentials.json --out token
```

The credentials file holds `api_id`, `api_hash`, `phone_number`, and `workdir`, with
optional `password`, `proxy`, and `session`. The first run asks for the verification code in
the console and saves the session. The example prints a readable reason when Telegram
rejects the password, the code, or the application credentials.

Notes for anyone reusing the provider:

- The session is stored in `{workdir}/{session}.wt.session` with owner-only permissions.
  The format belongs to WTelegramClient and cannot reuse Pyrogram `.session` files.
- WTelegramClient asks for `api_id`, `api_hash`, and `phone_number` through a config
  callback and prompts for other values in the console, for example the verification code.
- A password or verification code that Telegram rejects is not sent again. The provider
  throws instead of retrying, so a wrong value cannot cause `FLOOD_WAIT`.
- When the saved session cannot be resumed, the provider signs in again.
- Use one session file from one client at a time. `proxy` applies to the MRKT token
  exchange only; the connection to Telegram is direct.
- `MrktAuth.GetTokenAsync(initData)` exchanges Telegram Web App init data that you already
  have.

Protect session files and tokens like passwords.

## API Methods

### Market

```csharp
var collections = await client.Market.Gifts.CollectionsAsync();
var listings = await client.Market.Gifts.SalingAsync(
    collectionNames: ["Plush Pepe"],
    count: 20,
    ordering: "Price",
    lowToHigh: true);
var inventory = await client.Market.Gifts.InventoryAsync(isListed: false, count: 20);
var orders = await client.Market.Orders.ListAsync(collectionNames: ["Plush Pepe"], count: 10);
var fills = await client.Market.Orders.GiftsHistoryAsync("ORDER_ID");
```

- `Market.Gifts`: `CollectionsAsync`, `ModelsAsync`, `BackdropsAsync`, `SalingAsync`,
  `InventoryAsync`, `HistoryAsync`, `FeedAsync`, `BuyAsync`, `SaleAsync`.
- `Market.Orders`: `ListAsync`, `CreateAsync`, `GetMyOrdersAsync`, `TopAsync`,
  `AllCollectionTopAsync`, `GiftsHistoryAsync`, `FillAsync`, `CancelAsync`. `CancelAsync`
  returns `false` when the server responds with an HTTP error and `true` on success.
- `Market.Offers`: `CreateAsync`, `CancelAsync`, `MyOffersByGiftAsync`.

`BuyAsync`, `SaleAsync`, and the order and offer methods change account state. Check IDs and
amounts before calling them.

### User

```csharp
var profile = await client.User.MeAsync();
var balance = await client.User.BalanceAsync();
var transactions = await client.User.TransactionsAsync();
```

`User` also provides `WithdrawAsync`, `ActivitiesAsync`, `ConfigsAsync`, `GetLocaleAsync`,
`TasksAsync`, and `NewTasksAsync`. `AutoWithdrawAsync` reads the available balance and the
saved TON wallet before withdrawing.

### Steam and CS2

```csharp
var connected = await client.Steam.ConnectedAsync();
var skins = await client.Steam.Cs2.Skins.SalingAsync(count: 20, ordering: "Price", lowToHigh: true);
```

- `Steam`: `ConnectAsync`, `DisconnectAsync`, `ConnectedAsync`, `SetTradeLinkAsync`,
  `TradeLinkAsync`, `GetTradeLinkAsync`. `ConnectAsync` and `GetTradeLinkAsync` always return
  the response and open the link in the default browser when `openBrowser` is `true`.
- `Steam.Cs2.Skins`: `SalingAsync`, `FeedAsync`, `TradeItemAsync`, `ItemAsync`, and the
  catalog methods `ModelsAsync`, `VariantsAsync`, `ExteriorsAsync`, `RaritiesAsync`,
  `FinishesAsync`, `CollectionsAsync`, `TypesAsync`, `QualitiesAsync`.

The CS2 listing endpoints require every list filter in the payload. `SalingAsync` and
`FeedAsync` send omitted lists as empty.

### Extra parameters

Methods that send a JSON body accept `extra`, a dictionary merged into the payload, so
fields that are not modeled can be sent without a raw request. Catalog methods accept
`query` in the same way. All async methods take a `CancellationToken` as their last
argument.

## Raw HTTP Requests

`client.Http` is an `HttpClient` configured with the API base URL, browser-like headers,
and authentication. Paths are relative to `https://api.tgmrkt.io/api/v1/`:

```csharp
using System.Net.Http.Json;
using System.Text.Json;

using var response = await client.Http.PostAsync(
    "gifts/saling",
    JsonContent.Create(new { count = 10, collectionNames = new[] { "Plush Pepe" } }));
response.EnsureSuccessStatusCode();
var listings = await response.Content.ReadFromJsonAsync<JsonElement>();
```

## Multiple Accounts

`MrktPool` runs a callback for each account on a bounded set of reusable worker clients:

```csharp
using Mrkts;

await using var pool = new MrktPool(["TOKEN_1", "TOKEN_2", "TOKEN_3"], concurrency: 2);
var results = await pool.MapAsync(client => client.User.BalanceAsync());

foreach (var result in results)
{
    if (result.IsSuccess)
        Console.WriteLine(result.Value);
    else
        Console.WriteLine(result.Error!.Message);
}
```

- Results keep the input account order. A callback exception is returned in that account's
  `PoolResult<T>.Error`; other accounts are not affected.
- Workers are created lazily, at most `concurrency` of them, and are reused by every
  `MapAsync` call until the pool is disposed. `unlimited: true` creates one worker per
  account.
- `MapAsync(concurrency: n)` overrides the limit for one call.
- Accounts can also be `IAccessTokenProvider` instances. The pool resolves their tokens at
  `StartAsync()` with the same `concurrency` limit, attempts every account, and throws an
  `InvalidOperationException` naming the first index that failed.
- Await all work inside the callback and do not dispose the worker client: the same worker
  is assigned to another account afterwards.

## Workflows

`Poll` requests a page and yields each response:

```csharp
await foreach (var snapshot in client.Poll(
    (c, cancellationToken) => c.Market.Gifts.FeedAsync(count: 20, cancellationToken: cancellationToken),
    interval: 2.0))
{
    Console.WriteLine(snapshot);
}
```

`WatchListings` polls the listing feed and yields each new listing event once. The first
response is a baseline and yields nothing:

```csharp
await foreach (var listing in client.WatchListings(interval: 2.0, collectionNames: ["Plush Pepe"]))
    Console.WriteLine(listing.GetProperty("gift").GetProperty("name"));
```

The interval must be a finite number greater than zero, and the client must be started.
Request errors propagate to the consumer. Events are yielded in arrival order, which may
differ from event dates, and only the latest page is read.

## API Structure

```text
MrktClient
├── Market
│   ├── Gifts
│   ├── Orders
│   └── Offers
├── Steam
│   └── Cs2
│       └── Skins
└── User
```

## Differences from the Python client

- `preload`, `aliasing`, `aliases`, and `exec` are not ported. All API groups are always
  available, and workflows are extension methods.
- `fetch_feed`, `fetch_orders`, and `fetch_saling` are replaced by the single `Poll`
  workflow, which takes the request as a delegate.
- Session-based constructors and `get_tokens` are replaced by token providers.
- Telegram authentication is not part of the library. In Python it is built in through the
  `kurigram` dependency; here it lives in the `GetToken` example.
- Methods return `JsonElement` instead of dictionaries.

Five methods differ from the Python requests. They follow the official web app
(release 53588); the Python versions are rejected by the current API:

| Method | Python client | This client |
| --- | --- | --- |
| `User.GetLocaleAsync` | `GET` | `POST` |
| `User.WithdrawAsync` | `POST` with JSON | `GET` with query parameters |
| `Orders.GiftsHistoryAsync` | `POST orders/gifts-history` | `GET orders/gifts-history/{orderId}` |
| `Orders.FillAsync` | `POST orders/fill` | `POST orders/fill/` |
| `Steam.Cs2.Skins` | omitted filters are dropped or sent as `null` | lists default to empty, `count` to 20, `null` fields are not sent |

## Examples

- `examples/Mrkts.Examples.GetToken`: obtains an MRKT token from a Telegram session and
  writes it.
- `examples/Mrkts.Examples.PollFeed`: polls the market feed and prints every page, with
  `MRKT_TOKEN` from the environment.
- `examples/Mrkts.Examples.WatchListings`: prints each new listing once. Collection names
  are optional arguments:

  ```bash
  MRKT_TOKEN=<token> dotnet run --project examples/Mrkts.Examples.WatchListings -- "Plush Pepe"
  ```

## Credits

- [aiofake/mrkts](https://github.com/aiofake/mrkts) by aiofake (MIT): the original Python
  client. This library is a port of its API and request formats, and its license and
  copyright notice are retained in `LICENSE`.
- [WTelegramClient](https://github.com/wiz0u/WTelegramClient) by Olivier Marcoux (MIT),
  used by the `GetToken` example.

## License

MIT. See [LICENSE](LICENSE).
