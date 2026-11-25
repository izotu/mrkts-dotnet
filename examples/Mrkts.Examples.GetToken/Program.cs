using System.IO;
using System.Text.Json;
using Mrkts.Examples.GetToken;
using TL;
using WTelegram;
using static System.IO.UnixFileMode;

if (args.Length is < 1 or > 3 || args.Length == 2 || args.Length == 3 && args[1] != "--out")
{
    Console.Error.WriteLine("usage: Mrkts.Examples.GetToken <credentials.json> [--out <token file>]");
    return 2;
}

string[] required = ["api_id", "api_hash", "phone_number", "workdir"];
try
{
    var credentials = JsonDocument.Parse(await File.ReadAllTextAsync(args[0])).RootElement.Clone();
    if (credentials.ValueKind != JsonValueKind.Object)
        throw new InvalidDataException($"Credentials file {args[0]} must contain a JSON object");
    var missing = required.Where(key => string.IsNullOrEmpty(Text(credentials, key))).ToArray();
    if (missing.Length > 0)
        throw new InvalidDataException($"Credentials file {args[0]} is missing: {string.Join(", ", missing)}");

    var workdir = ExpandUser(Text(credentials, "workdir")!);
    Directory.CreateDirectory(workdir);
    SetMode(workdir, UserRead | UserWrite | UserExecute);

    string? Config(string what) => what switch
    {
        "api_id" => Text(credentials, "api_id"),
        "api_hash" => Text(credentials, "api_hash"),
        "phone_number" => Text(credentials, "phone_number"),
        "password" => Text(credentials, "password"),
        _ => null,
    };

    var provider = new TelegramTokenProvider(Text(credentials, "session") ?? "mrkt", workdir, Config, Text(credentials, "proxy"));
    var token = await provider.GetTokenAsync();

    if (args.Length == 3)
    {
        var path = Path.GetFullPath(ExpandUser(args[2]));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        SetMode(Path.GetDirectoryName(path)!, UserRead | UserWrite | UserExecute);
        await File.WriteAllTextAsync(path, token);
        SetMode(path, UserRead | UserWrite);
    }
    else
    {
        Console.WriteLine(token);
    }
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(Errors.Describe(error));
    return 1;
}

static string? Text(JsonElement credentials, string key)
{
    if (!credentials.TryGetProperty(key, out var value))
        return null;
    return value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.GetRawText(),
        _ => null,
    };
}

static string ExpandUser(string path) =>
    path == "~" || path.StartsWith("~/") ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[2..]) : path;

static void SetMode(string path, UnixFileMode mode)
{
    if (!OperatingSystem.IsWindows())
        File.SetUnixFileMode(path, mode);
}

internal static class Errors
{
    public static string Describe(Exception error) => error switch
    {
        InvalidDataException or JsonException or UnauthorizedAccessException => error.Message,
        InvalidOperationException { Message: var rejected } when rejected.Contains("rejected the password") =>
            "Telegram rejected the password. Check `password` in the credentials file.",
        InvalidOperationException { Message: var rejected } when rejected.Contains("rejected the verification_code") =>
            "Telegram rejected the verification code. Run again and enter the code promptly.",
        RpcException { Code: var code, Message: var message } => message switch
        {
            "PHONE_CODE_INVALID" or "PHONE_CODE_EXPIRED" => "Telegram rejected the verification code. Run again and enter the code promptly.",
            "PASSWORD_HASH_INVALID" => "Telegram rejected the password. Check `password` in the credentials file.",
            "API_ID_INVALID" or "API_ID_PUBLISHED_FLOOD" or "API_ID_APP_HASH_INVALID" => $"Telegram rejected the application credentials ({message}). Check api_id and api_hash.",
            _ when message.StartsWith("FLOOD_WAIT_") => $"Telegram limits requests ({message}). Wait before trying again.",
            _ => $"Telegram rejected the request: {message} (code {code}).",
        },
        WTException or InvalidOperationException or IOException or ArgumentException => error.Message,
        _ => $"{error.GetType().Name}: {error.Message}",
    };
}
