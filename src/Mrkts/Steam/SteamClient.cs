using System.Diagnostics;
using System.Text.Json;
using Mrkts.Steam.Cs2;

namespace Mrkts.Steam;

public sealed class SteamClient
{
    private const string DefaultRedirectUri = "https://www.mrkt.land/steam-link?return=https%3A%2F%2Ft.me%2Fmrkt%2Fapp%3Fstartapp%3Dopensteamlinked";

    private readonly MrktClient _client;

    internal SteamClient(MrktClient client)
    {
        _client = client;
        Cs2 = new Cs2Client(client);
    }

    public Cs2Client Cs2 { get; }

    public async Task<JsonElement> ConnectAsync(string languageCode = "en", string redirectUri = DefaultRedirectUri, bool openBrowser = false, CancellationToken cancellationToken = default)
    {
        var link = await _client.PostAsync("auth/link/sign-in-link", new Dictionary<string, object?> { ["redirectUri"] = redirectUri, ["languageCode"] = languageCode }, cancellationToken).ConfigureAwait(false);
        if (openBrowser && ReadString(link, "url") is { Length: > 0 } url)
            Process.Start(url);
        return link;
    }

    public Task<JsonElement> DisconnectAsync(CancellationToken cancellationToken = default) =>
        _client.PostAsync("auth/disconnect/steam", cancellationToken: cancellationToken);

    public async Task<bool> ConnectedAsync(CancellationToken cancellationToken = default)
    {
        var user = await _client.User.MeAsync(cancellationToken).ConfigureAwait(false);
        return user.ValueKind == JsonValueKind.Object && user.TryGetProperty("steamUser", out var steamUser) && IsTruthy(steamUser);
    }

    public async Task<bool> SetTradeLinkAsync(string tradeLink, CancellationToken cancellationToken = default)
    {
        await _client.PostAsync("steam/trade-link", new Dictionary<string, object?> { ["tradeLink"] = tradeLink }, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<string> TradeLinkAsync(CancellationToken cancellationToken = default)
    {
        var user = await _client.User.MeAsync(cancellationToken).ConfigureAwait(false);
        return ReadString(ReadProperty(user, "steamUser"), "tradeLink") ?? "";
    }

    public async Task<string> GetTradeLinkAsync(bool openBrowser = false, CancellationToken cancellationToken = default)
    {
        var user = await _client.User.MeAsync(cancellationToken).ConfigureAwait(false);
        var id = ReadProperty(ReadProperty(user, "steamUser"), "id");
        var url = $"https://steamcommunity.com/profiles/{(id.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null ? "" : id.ToString())}/tradeoffers/privacy#trade_offer_access_url";
        if (openBrowser)
            Process.Start(url);
        return url;
    }

    private static JsonElement ReadProperty(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) ? value : default;

    private static string? ReadString(JsonElement parent, string name)
    {
        var value = ReadProperty(parent, name);
        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static bool IsTruthy(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Object => value.EnumerateObject().Any(),
        JsonValueKind.Array => value.GetArrayLength() > 0,
        JsonValueKind.String => value.GetString() is { Length: > 0 },
        JsonValueKind.Number => value.TryGetInt64(out var number) ? number != 0 : value.GetDouble() != 0,
        JsonValueKind.True => true,
        _ => false,
    };
}
