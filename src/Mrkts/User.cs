using System.Text.Json;

namespace Mrkts;

public sealed class User
{
    private readonly MrktClient _client;

    internal User(MrktClient client) => _client = client;

    public Task<JsonElement> MeAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync("me", cancellationToken: cancellationToken);

    public Task<JsonElement> BalanceAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync("balance", cancellationToken: cancellationToken);

    public Task<JsonElement> WithdrawAsync(long nanoTONs, string walletAddress, CancellationToken cancellationToken = default) =>
        WithdrawAsync(JsonSerializer.SerializeToElement(nanoTONs), walletAddress, cancellationToken);

    public Task<JsonElement> ActivitiesAsync(int count = 20, IReadOnlyList<string>? filters = null, CancellationToken cancellationToken = default)
    {
        filters ??= ["Offers", "ChannelOffers", "GiftsCollectionOffers"];
        return _client.GetAsync("activities/with-totals", Payload.BuildQuery(new Dictionary<string, object?>
        {
            ["count"] = count,
            ["isActive"] = "true",
            ["filters"] = filters,
        }), cancellationToken: cancellationToken);
    }

    public Task<JsonElement> TransactionsAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync("transactions/await", cancellationToken: cancellationToken);

    public Task<JsonElement> ConfigsAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync("configs", cancellationToken: cancellationToken);

    public Task<JsonElement> GetLocaleAsync(string locale, CancellationToken cancellationToken = default) =>
        _client.PostAsync("get_locale", new Dictionary<string, object?> { ["locale"] = locale }, cancellationToken);

    public Task<JsonElement> TasksAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync("tasks", cancellationToken: cancellationToken);

    public Task<JsonElement> NewTasksAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync("tasks/new", cancellationToken: cancellationToken);

    /// <summary>Withdraw the available balance to the account's saved TON wallet.</summary>
    public async Task<JsonElement> AutoWithdrawAsync(long? nanoTONs = null, CancellationToken cancellationToken = default)
    {
        var amount = nanoTONs is long value ? JsonSerializer.SerializeToElement(value) : await ReadWithdrawableAsync(cancellationToken).ConfigureAwait(false);
        var profile = await MeAsync(cancellationToken).ConfigureAwait(false);
        string? wallet = profile.ValueKind == JsonValueKind.Object
            && profile.TryGetProperty("wallet", out var walletData) && walletData.ValueKind == JsonValueKind.Object
            && walletData.TryGetProperty("ton", out var ton) && ton.ValueKind == JsonValueKind.String
            ? ton.GetString()
            : null;
        if (string.IsNullOrEmpty(wallet))
            throw new InvalidOperationException("Account response does not contain a saved TON wallet");
        return await WithdrawAsync(amount, wallet, cancellationToken).ConfigureAwait(false);
    }

    private Task<JsonElement> WithdrawAsync(JsonElement nanoTONs, string walletAddress, CancellationToken cancellationToken) =>
        _client.GetAsync("wallet/withdraw/tons", Payload.BuildQuery(new Dictionary<string, object?> { ["nanoTONs"] = nanoTONs, ["wallet"] = walletAddress }), cancellationToken: cancellationToken);

    private async Task<JsonElement> ReadWithdrawableAsync(CancellationToken cancellationToken)
    {
        var balance = await BalanceAsync(cancellationToken).ConfigureAwait(false);
        if (balance.ValueKind == JsonValueKind.Object
            && balance.TryGetProperty("balance", out var balanceData) && balanceData.ValueKind == JsonValueKind.Object
            && balanceData.TryGetProperty("hard", out var hard) && hard.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined))
            return hard.Clone();
        throw new InvalidOperationException("Account response does not contain a withdrawable balance");
    }
}
