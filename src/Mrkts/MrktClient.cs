using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Mrkts.Market;
using Mrkts.Steam;

namespace Mrkts;

public sealed class MrktClient : IAsyncDisposable
{
    private static readonly Uri BaseUrl = new("https://api.tgmrkt.io/api/v1/");
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(5);

    private readonly IAccessTokenProvider? _tokenProvider;
    private readonly IReadOnlyDictionary<string, string>? _headers;
    private readonly HttpMessageHandler? _handler;
    private HttpClient? _http;

    public MrktClient(string? token = null, string? proxy = null, IReadOnlyDictionary<string, string>? headers = null, HttpMessageHandler? handler = null)
        : this(token, null, proxy, headers, handler)
    {
    }

    public MrktClient(IAccessTokenProvider tokenProvider, string? proxy = null, IReadOnlyDictionary<string, string>? headers = null, HttpMessageHandler? handler = null)
        : this(null, tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider)), proxy, headers, handler)
    {
    }

    private MrktClient(string? token, IAccessTokenProvider? tokenProvider, string? proxy, IReadOnlyDictionary<string, string>? headers, HttpMessageHandler? handler)
    {
        Token = token;
        _tokenProvider = tokenProvider;
        Proxy = proxy;
        _headers = headers;
        _handler = handler;
        Market = new MarketClient(this);
        Steam = new SteamClient(this);
        User = new User(this);
    }

    public string? Token { get; private set; }
    public string? Proxy { get; }
    public bool IsStarted => _http is not null;
    public HttpClient Http => _http ?? throw new InvalidOperationException("MrktClient is not started. Call StartAsync() first.");
    public MarketClient Market { get; }
    public SteamClient Steam { get; }
    public User User { get; }
    public Gifts Gifts => Market.Gifts;
    public Orders Orders => Market.Orders;
    public Offers Offers => Market.Offers;

    public async Task<MrktClient> StartAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Token) && _tokenProvider is not null)
            Token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(Token) && _http is null)
            CreateHttp();
        return this;
    }

    public async Task<string> AuthAsync(string tgWebAppData, CancellationToken cancellationToken = default)
    {
        _http ??= CreateHttp();
        var data = await PostAsync("auth", new Dictionary<string, object?> { ["data"] = tgWebAppData }, cancellationToken).ConfigureAwait(false);
        string? token = data.ValueKind == JsonValueKind.Object && data.TryGetProperty("token", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Auth response did not contain a token");
        return token;
    }

    public void SetToken(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        Token = token;
        if (_http is null)
        {
            CreateHttp();
            return;
        }
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _http.DefaultRequestHeaders.Remove("cookie");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("cookie", $"access_token={token}");
    }

    public ValueTask DisposeAsync()
    {
        _http?.Dispose();
        _http = null;
        return ValueTask.CompletedTask;
    }

    internal async Task<JsonElement> GetAsync(string uri, string? query = null, object? json = null, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, query is null ? uri : $"{uri}?{query}", json, cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
    }

    internal async Task<JsonElement> PostAsync(string uri, object? json = null, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, uri, json, cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
    }

    internal Task<HttpResponseMessage> SendAsync(HttpMethod method, string uri, object? json, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, uri);
        if (json is not null)
        {
            var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(json, json.GetType()));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = content;
        }
        return Http.SendAsync(request, cancellationToken);
    }

    internal static IReadOnlyList<KeyValuePair<string, string>> BuildHeaders(string? token)
    {
        int chrome = Random.Shared.Next(120, 146), edge = Random.Shared.Next(120, 146);
        var headers = new List<KeyValuePair<string, string>>
        {
            new("accept", "application/json, text/plain, */*"),
            new("accept-encoding", "gzip, deflate"),
            new("accept-language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7"),
            new("content-type", "application/json"),
            new("origin", "https://cdn.tgmrkt.io"),
            new("priority", "u=1, i"),
            new("referer", "https://cdn.tgmrkt.io/"),
            new("sec-ch-ua", $"\"Not=A?Brand\";v=\"8\", \"Chromium\";v=\"{chrome}\", \"Microsoft Edge\";v=\"{edge}\""),
            new("sec-ch-ua-mobile", "?0"),
            new("sec-ch-ua-platform", Random.Shared.Next(3) switch { 0 => "\"Windows\"", 1 => "\"macOS\"", _ => "\"Linux\"" }),
            new("sec-fetch-dest", "empty"),
            new("sec-fetch-mode", "cors"),
            new("sec-fetch-site", "same-site"),
            new("sec-fetch-storage-access", "active"),
            new("user-agent", $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{chrome}.0.0.0 Safari/537.36 Edg/{edge}.0.0.0"),
        };
        if (!string.IsNullOrEmpty(token))
        {
            headers.Add(new("authorization", $"Bearer {token}"));
            headers.Add(new("cookie", $"access_token={token}"));
        }
        return headers;
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return document.RootElement.Clone();
    }

    private HttpClient CreateHttp()
    {
        var http = new HttpClient(_handler ?? CreateHandler(), disposeHandler: _handler is null)
        {
            BaseAddress = BaseUrl,
            Timeout = RequestTimeout,
        };
        foreach (var (name, value) in BuildHeaders(Token))
            http.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
        if (_headers is not null)
            foreach (var (name, value) in _headers)
            {
                http.DefaultRequestHeaders.Remove(name);
                http.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
            }
        _http = http;
        return http;
    }

    private SocketsHttpHandler CreateHandler() => new()
    {
        UseProxy = Proxy is not null,
        Proxy = Proxy is null ? null : new WebProxy(Proxy),
        UseCookies = false,
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
    };
}
