using System.IO;
using TL;

namespace Mrkts.Examples.GetToken;

internal sealed class TelegramTokenProvider : IAccessTokenProvider
{
    private readonly string _session;
    private readonly string _workdir;
    private readonly string? _proxy;
    private readonly string _username;
    private readonly Func<string, string?>? _config;

    public TelegramTokenProvider(string session, string workdir = ".", Func<string, string?>? config = null, string? proxy = null, string username = "mrkt")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        _session = session;
        _workdir = workdir;
        _proxy = proxy;
        _username = username;
        _config = config;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var initData = await GetInitDataAsync(cancellationToken).ConfigureAwait(false);
        return await MrktAuth.GetTokenAsync(initData, _proxy, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetInitDataAsync(CancellationToken cancellationToken = default)
    {
        ProtectSessionStore();
        using var client = new WTelegram.Client(CreateConfig());
        using var registration = cancellationToken.Register(client.Dispose);
        await client.LoginUserIfNeeded(reloginOnFailedResume: true).ConfigureAwait(false);
        var resolved = await client.Contacts_ResolveUsername(_username).ConfigureAwait(false);
        var bot = resolved.User ?? throw new InvalidOperationException($"@{_username} is not a user");
        var result = await client.Messages_RequestMainWebView(bot, bot, "Other", start_param: "").ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return result is WebViewResultUrl { url: { Length: > 0 } url } ? ExtractInitData(url) : throw new InvalidOperationException("Telegram did not return a Web App URL");
    }

    internal static string ExtractInitData(string url)
    {
        int fragmentStart = url.IndexOf('#');
        string target = fragmentStart < 0 ? url : url[..fragmentStart];
        int queryStart = target.IndexOf('?');
        var query = queryStart < 0 ? null : ParseQuery(target[(queryStart + 1)..]);
        var fragment = fragmentStart < 0 ? null : ParseQuery(url[(fragmentStart + 1)..]);
        var initData = query is not null && query.TryGetValue("tgWebAppData", out var value) ? value
            : fragment is not null && fragment.TryGetValue("tgWebAppData", out var fragmentValue) ? fragmentValue
            : null;
        return string.IsNullOrEmpty(initData) ? throw new InvalidOperationException("Telegram WebApp URL does not contain tgWebAppData") : initData;
    }

    private string SessionPath => Path.Combine(_workdir, _session + ".wt.session");

    private Func<string, string?> CreateConfig()
    {
        var supplied = new Dictionary<string, string?>();
        return what =>
        {
            var value = what == "session_pathname" ? SessionPath : _config?.Invoke(what);
            if (what is "password" or "verification_code")
            {
                if (value is not null && supplied.TryGetValue(what, out var previous) && previous == value)
                    throw new InvalidOperationException($"Telegram rejected the {what} supplied by the config callback");
                supplied[what] = value;
            }
            return value;
        };
    }

    private void ProtectSessionStore()
    {
        bool created = !Directory.Exists(_workdir);
        Directory.CreateDirectory(_workdir);
        if (OperatingSystem.IsWindows())
            return;
        if (created)
            new DirectoryInfo(_workdir).UnixFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
        if (!File.Exists(SessionPath))
            File.WriteAllBytes(SessionPath, []);
        File.SetUnixFileMode(SessionPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var values = new Dictionary<string, string>();
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            int separator = pair.IndexOf('=');
            var key = Unquote(separator < 0 ? pair : pair[..separator]);
            var value = separator < 0 ? "" : Unquote(pair[(separator + 1)..]);
            if (value.Length > 0)
                values.TryAdd(key, value);
        }
        return values;
    }

    private static string Unquote(string value) => Uri.UnescapeDataString(value.Replace('+', ' '));
}
