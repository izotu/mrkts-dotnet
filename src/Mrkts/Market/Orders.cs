using System.Net;
using System.Text.Json;

namespace Mrkts.Market;

public sealed class Orders
{
    private readonly MrktClient _client;

    internal Orders(MrktClient client) => _client = client;

    public Task<JsonElement> CreateAsync(
        string? collectionName = null,
        string? modelName = null,
        string? backdropName = null,
        string? symbolName = null,
        long priceMinNanoTONs = 500_000_000,
        long priceMaxNanoTONs = 1_000_000_000,
        int quantity = 1,
        bool isTransferable = false,
        IDictionary<string, object?>? extra = null,
        CancellationToken cancellationToken = default) =>
        _client.PostAsync("orders/create", Payload.Build(new Dictionary<string, object?>
        {
            ["collectionName"] = collectionName,
            ["modelName"] = modelName,
            ["backdropName"] = backdropName,
            ["symbolName"] = symbolName,
            ["priceMinNanoTONs"] = priceMinNanoTONs,
            ["priceMaxNanoTONs"] = priceMaxNanoTONs,
            ["quantity"] = quantity,
            ["isTransferable"] = isTransferable,
        }, extra, dropNulls: false), cancellationToken);

    public async Task<bool> CancelAsync(string orderId, CancellationToken cancellationToken = default)
    {
        using var response = await _client.SendAsync(HttpMethod.Post, $"orders/cancel/{Uri.EscapeDataString(orderId)}", null, cancellationToken).ConfigureAwait(false);
        try
        {
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException error) when (error.StatusCode is not null)
        {
            return false;
        }
    }

    public Task<JsonElement> ListAsync(
        IReadOnlyList<string> collectionNames,
        IReadOnlyList<string>? modelNames = null,
        IReadOnlyList<string>? backdropNames = null,
        IReadOnlyList<string>? symbolNames = null,
        bool? availableForStaking = null,
        double? minPrice = null,
        double? maxPrice = null,
        string ordering = "Price",
        bool lowToHigh = false,
        string? query = null,
        string cursor = "",
        int count = 20,
        IDictionary<string, object?>? extra = null,
        CancellationToken cancellationToken = default)
    {
        modelNames ??= [];
        backdropNames ??= [];
        symbolNames ??= [];
        return _client.PostAsync("orders", Payload.Build(new Dictionary<string, object?>
        {
            ["collectionNames"] = collectionNames,
            ["modelNames"] = modelNames,
            ["backdropNames"] = backdropNames,
            ["symbolNames"] = symbolNames,
            ["availableForStaking"] = availableForStaking,
            ["minPrice"] = minPrice,
            ["maxPrice"] = maxPrice,
            ["ordering"] = ordering,
            ["lowToHigh"] = lowToHigh,
            ["query"] = query,
            ["cursor"] = cursor,
            ["count"] = count,
        }, extra, dropNulls: false), cancellationToken);
    }

    public Task<JsonElement> FillAsync(string orderId, string giftIds, CancellationToken cancellationToken = default) =>
        _client.PostAsync("orders/fill/", new Dictionary<string, object?> { ["orderId"] = orderId, ["giftIds"] = giftIds }, cancellationToken);

    public Task<JsonElement> GetMyOrdersAsync(
        IReadOnlyList<string>? collectionNames = null,
        IReadOnlyList<string>? modelNames = null,
        IReadOnlyList<string>? backdropNames = null,
        IReadOnlyList<string>? symbolNames = null,
        bool? availableForStaking = null,
        double? minPrice = null,
        double? maxPrice = null,
        string ordering = "PublicationTime",
        bool lowToHigh = false,
        string? query = null,
        string cursor = "",
        int count = 20,
        IDictionary<string, object?>? extra = null,
        CancellationToken cancellationToken = default)
    {
        collectionNames ??= [];
        modelNames ??= [];
        backdropNames ??= [];
        symbolNames ??= [];
        return _client.PostAsync("orders/get-my-orders", Payload.Build(new Dictionary<string, object?>
        {
            ["collectionNames"] = collectionNames,
            ["modelNames"] = modelNames,
            ["backdropNames"] = backdropNames,
            ["symbolNames"] = symbolNames,
            ["availableForStaking"] = availableForStaking,
            ["minPrice"] = minPrice,
            ["maxPrice"] = maxPrice,
            ["ordering"] = ordering,
            ["lowToHigh"] = lowToHigh,
            ["query"] = query,
            ["cursor"] = cursor,
            ["count"] = count,
        }, extra, dropNulls: false), cancellationToken);
    }

    public Task<JsonElement> AllCollectionTopAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync("orders/all-collection-top", cancellationToken: cancellationToken);

    public Task<JsonElement> GiftsHistoryAsync(string orderId, string stFrom = "", int limit = 20, IDictionary<string, object?>? extra = null, CancellationToken cancellationToken = default) =>
        _client.GetAsync($"orders/gifts-history/{Uri.EscapeDataString(orderId)}", Payload.BuildQuery(Payload.Build(new Dictionary<string, object?>
        {
            ["from"] = stFrom,
            ["limit"] = limit,
        }, extra, dropNulls: true)), cancellationToken: cancellationToken);

    public Task<JsonElement> TopAsync(string collectionName, string? modelName = null, string? backdropName = null, string? symbolName = null, CancellationToken cancellationToken = default) =>
        _client.PostAsync("orders/top", new Dictionary<string, object?>
        {
            ["collectionName"] = collectionName,
            ["modelName"] = modelName,
            ["backdropName"] = backdropName,
            ["symbolName"] = symbolName,
        }, cancellationToken);
}
