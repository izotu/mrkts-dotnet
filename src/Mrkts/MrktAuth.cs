namespace Mrkts;

public static class MrktAuth
{
    public static async Task<string> GetTokenAsync(string tgWebAppData, string? proxy = null, CancellationToken cancellationToken = default)
    {
        await using var client = new MrktClient(proxy: proxy);
        return await client.AuthAsync(tgWebAppData, cancellationToken).ConfigureAwait(false);
    }
}
