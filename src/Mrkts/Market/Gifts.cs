using System.Text.Json;

namespace Mrkts.Market;

public sealed class Gifts
{
    private readonly MrktClient _client;

    internal Gifts(MrktClient client) => _client = client;

    public Task<JsonElement> SalingAsync(
        int count = 20,
        string cursor = "",
        IReadOnlyList<string>? collectionNames = null,
        IReadOnlyList<string>? modelNames = null,
        IReadOnlyList<string>? backdropNames = null,
        IReadOnlyList<string>? symbolNames = null,
        int? number = null,
        bool? isNew = null,
        bool? isPremarket = null,
        bool? luckyBuy = null,
        string? giftType = null,
        bool? craftable = null,
        bool? isCrafted = null,
        bool? tgCanBeCraftedFrom = null,
        bool? removeSelfSales = null,
        bool? isTransferable = null,
        bool? availableForStaking = null,
        bool? forGame = null,
        long? minPrice = null,
        long? maxPrice = null,
        string ordering = "Price",
        bool lowToHigh = true,
        string? query = null,
        IDictionary<string, object?>? extra = null,
        CancellationToken cancellationToken = default)
    {
        collectionNames ??= [];
        modelNames ??= [];
        backdropNames ??= [];
        symbolNames ??= [];
        return _client.PostAsync("gifts/saling", Payload.Build(new Dictionary<string, object?>
        {
            ["count"] = count,
            ["cursor"] = cursor,
            ["collectionNames"] = collectionNames,
            ["modelNames"] = modelNames,
            ["backdropNames"] = backdropNames,
            ["symbolNames"] = symbolNames,
            ["number"] = number,
            ["isNew"] = isNew,
            ["isPremarket"] = isPremarket,
            ["luckyBuy"] = luckyBuy,
            ["giftType"] = giftType,
            ["craftable"] = craftable,
            ["isCrafted"] = isCrafted,
            ["tgCanBeCraftedFrom"] = tgCanBeCraftedFrom,
            ["removeSelfSales"] = removeSelfSales,
            ["isTransferable"] = isTransferable,
            ["availableForStaking"] = availableForStaking,
            ["forGame"] = forGame,
            ["minPrice"] = minPrice,
            ["maxPrice"] = maxPrice,
            ["ordering"] = ordering,
            ["lowToHigh"] = lowToHigh,
            ["query"] = query,
        }, extra, dropNulls: true), cancellationToken);
    }

    public Task<JsonElement> BuyAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken = default) =>
        _client.PostAsync("gifts/buy", new Dictionary<string, object?> { ["ids"] = ids }, cancellationToken);

    public Task<JsonElement> SaleAsync(IReadOnlyList<string> ids, long price, CancellationToken cancellationToken = default) =>
        _client.PostAsync("gifts/sale", new Dictionary<string, object?> { ["ids"] = ids, ["price"] = price }, cancellationToken);

    public Task<JsonElement> CollectionsAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync("gifts/collections", cancellationToken: cancellationToken);

    public Task<JsonElement> ModelsAsync(IReadOnlyList<string> collections, CancellationToken cancellationToken = default) =>
        _client.PostAsync("gifts/models", new Dictionary<string, object?> { ["collections"] = collections }, cancellationToken);

    public Task<JsonElement> BackdropsAsync(IReadOnlyList<string> collections, CancellationToken cancellationToken = default) =>
        _client.PostAsync("gifts/backdrops", new Dictionary<string, object?> { ["collections"] = collections }, cancellationToken);

    public Task<JsonElement> InventoryAsync(
        bool isListed = false,
        int count = 20,
        string cursor = "",
        IReadOnlyList<string>? collectionNames = null,
        IReadOnlyList<string>? modelNames = null,
        IReadOnlyList<string>? backdropNames = null,
        IReadOnlyList<string>? symbolNames = null,
        int? number = null,
        bool? isNew = null,
        bool? isPremarket = null,
        bool? luckyBuy = null,
        string? giftType = null,
        bool? craftable = null,
        bool? isCrafted = null,
        bool? tgCanBeCraftedFrom = null,
        bool? removeSelfSales = null,
        bool? isTransferable = null,
        bool? availableForStaking = null,
        bool? forGame = null,
        long? minPrice = null,
        long? maxPrice = null,
        string ordering = "None",
        bool lowToHigh = false,
        string? query = null,
        IDictionary<string, object?>? extra = null,
        CancellationToken cancellationToken = default)
    {
        collectionNames ??= [];
        modelNames ??= [];
        backdropNames ??= [];
        symbolNames ??= [];
        return _client.PostAsync("gifts", Payload.Build(new Dictionary<string, object?>
        {
            ["isListed"] = isListed,
            ["count"] = count,
            ["cursor"] = cursor,
            ["collectionNames"] = collectionNames,
            ["modelNames"] = modelNames,
            ["backdropNames"] = backdropNames,
            ["symbolNames"] = symbolNames,
            ["number"] = number,
            ["isNew"] = isNew,
            ["isPremarket"] = isPremarket,
            ["luckyBuy"] = luckyBuy,
            ["giftType"] = giftType,
            ["craftable"] = craftable,
            ["isCrafted"] = isCrafted,
            ["tgCanBeCraftedFrom"] = tgCanBeCraftedFrom,
            ["removeSelfSales"] = removeSelfSales,
            ["isTransferable"] = isTransferable,
            ["availableForStaking"] = availableForStaking,
            ["forGame"] = forGame,
            ["minPrice"] = minPrice,
            ["maxPrice"] = maxPrice,
            ["ordering"] = ordering,
            ["lowToHigh"] = lowToHigh,
            ["query"] = query,
        }, extra, dropNulls: true), cancellationToken);
    }

    public Task<JsonElement> HistoryAsync(int limit = 20, string type = "Gift", IDictionary<string, object?>? extra = null, CancellationToken cancellationToken = default) =>
        _client.GetAsync("history", json: Payload.Build(new Dictionary<string, object?>
        {
            ["limit"] = limit,
            ["type"] = type,
        }, extra, dropNulls: false), cancellationToken: cancellationToken);

    public Task<JsonElement> FeedAsync(
        int count = 20,
        string cursor = "",
        IReadOnlyList<string>? collectionNames = null,
        IReadOnlyList<string>? modelNames = null,
        IReadOnlyList<string>? backdropNames = null,
        int? number = null,
        IReadOnlyList<string>? type = null,
        long? minPrice = null,
        long? maxPrice = null,
        string ordering = "Latest",
        bool lowToHigh = false,
        string? query = null,
        IDictionary<string, object?>? extra = null,
        CancellationToken cancellationToken = default)
    {
        collectionNames ??= [];
        modelNames ??= [];
        backdropNames ??= [];
        type ??= [];
        return _client.PostAsync("feed", Payload.Build(new Dictionary<string, object?>
        {
            ["count"] = count,
            ["cursor"] = cursor,
            ["collectionNames"] = collectionNames,
            ["modelNames"] = modelNames,
            ["backdropNames"] = backdropNames,
            ["number"] = number,
            ["type"] = type,
            ["minPrice"] = minPrice,
            ["maxPrice"] = maxPrice,
            ["ordering"] = ordering,
            ["lowToHigh"] = lowToHigh,
            ["query"] = query,
        }, extra, dropNulls: true), cancellationToken);
    }
}
