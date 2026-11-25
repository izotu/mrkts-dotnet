using System.Net;
using System.Text.Json;

namespace Mrkts.Steam.Cs2;

public sealed class Skins
{
    private readonly MrktClient _client;

    internal Skins(MrktClient client) => _client = client;

    public Task<JsonElement> SalingAsync(
        IReadOnlyList<string>? subTypeInternalNames = null,
        IReadOnlyList<string>? modelInternalNames = null,
        IReadOnlyList<string>? exteriorInternalNames = null,
        IReadOnlyList<string>? finishInternalNames = null,
        IReadOnlyList<string>? qualityInternalNames = null,
        IReadOnlyList<string>? rarityInternalNames = null,
        IReadOnlyList<string>? colorNames = null,
        IReadOnlyList<string>? stickerNames = null,
        IReadOnlyList<string>? stickerColorNames = null,
        IReadOnlyList<string>? keychainNames = null,
        IReadOnlyList<string>? collectionNames = null,
        IReadOnlyList<string>? teamNames = null,
        IReadOnlyList<string>? playerNames = null,
        IReadOnlyList<string>? stickerCollectionNames = null,
        IReadOnlyList<string>? stickerTeamNames = null,
        IReadOnlyList<string>? stickerPlayerNames = null,
        IReadOnlyList<string>? tradeItemNames = null,
        int count = 20,
        double? minFloat = null,
        double? maxFloat = null,
        bool? returnable = null,
        bool? luckyBuy = null,
        bool? hasPattern = null,
        int? minPatternIndex = null,
        int? maxPatternIndex = null,
        bool? hasBlueGem = null,
        int? minBlueGem = null,
        int? maxBlueGem = null,
        bool? hasFade = null,
        int? minFade = null,
        int? maxFade = null,
        long? minPrice = null,
        long? maxPrice = null,
        string? ordering = null,
        bool? lowToHigh = null,
        string? query = null,
        bool? hasHighlight = null,
        string? cursor = null,
        string? searchAfterCursor = null,
        IReadOnlyList<string>? excludedIds = null,
        bool? removeSelfSales = null,
        IDictionary<string, object?>? extra = null,
        CancellationToken cancellationToken = default)
    {
        subTypeInternalNames ??= [];
        modelInternalNames ??= [];
        exteriorInternalNames ??= [];
        finishInternalNames ??= [];
        qualityInternalNames ??= [];
        rarityInternalNames ??= [];
        colorNames ??= [];
        stickerNames ??= [];
        stickerColorNames ??= [];
        keychainNames ??= [];
        collectionNames ??= [];
        teamNames ??= [];
        playerNames ??= [];
        stickerCollectionNames ??= [];
        stickerTeamNames ??= [];
        stickerPlayerNames ??= [];
        tradeItemNames ??= [];
        return _client.PostAsync("steam/trade-items/saling", Payload.Build(new Dictionary<string, object?>
        {
            ["subTypeInternalNames"] = subTypeInternalNames,
            ["modelInternalNames"] = modelInternalNames,
            ["exteriorInternalNames"] = exteriorInternalNames,
            ["finishInternalNames"] = finishInternalNames,
            ["qualityInternalNames"] = qualityInternalNames,
            ["rarityInternalNames"] = rarityInternalNames,
            ["colorNames"] = colorNames,
            ["stickerNames"] = stickerNames,
            ["stickerColorNames"] = stickerColorNames,
            ["keychainNames"] = keychainNames,
            ["collectionNames"] = collectionNames,
            ["teamNames"] = teamNames,
            ["playerNames"] = playerNames,
            ["stickerCollectionNames"] = stickerCollectionNames,
            ["stickerTeamNames"] = stickerTeamNames,
            ["stickerPlayerNames"] = stickerPlayerNames,
            ["tradeItemNames"] = tradeItemNames,
            ["count"] = count,
            ["minFloat"] = minFloat,
            ["maxFloat"] = maxFloat,
            ["returnable"] = returnable,
            ["luckyBuy"] = luckyBuy,
            ["hasPattern"] = hasPattern,
            ["minPatternIndex"] = minPatternIndex,
            ["maxPatternIndex"] = maxPatternIndex,
            ["hasBlueGem"] = hasBlueGem,
            ["minBlueGem"] = minBlueGem,
            ["maxBlueGem"] = maxBlueGem,
            ["hasFade"] = hasFade,
            ["minFade"] = minFade,
            ["maxFade"] = maxFade,
            ["minPrice"] = minPrice,
            ["maxPrice"] = maxPrice,
            ["ordering"] = ordering,
            ["lowToHigh"] = lowToHigh,
            ["query"] = query,
            ["hasHighlight"] = hasHighlight,
            ["cursor"] = cursor,
            ["searchAfterCursor"] = searchAfterCursor,
            ["excludedIds"] = excludedIds,
            ["removeSelfSales"] = removeSelfSales,
        }, extra, dropNulls: true), cancellationToken);
    }

    public Task<JsonElement> FeedAsync(
        IReadOnlyList<string>? subTypeInternalNames = null,
        IReadOnlyList<string>? modelInternalNames = null,
        IReadOnlyList<string>? exteriorInternalNames = null,
        IReadOnlyList<string>? finishInternalNames = null,
        IReadOnlyList<string>? qualityInternalNames = null,
        IReadOnlyList<string>? rarityInternalNames = null,
        IReadOnlyList<string>? colorNames = null,
        IReadOnlyList<string>? collectionNames = null,
        IReadOnlyList<string>? teamNames = null,
        IReadOnlyList<string>? playerNames = null,
        int count = 20,
        string? cursor = null,
        IReadOnlyList<string>? stickerNames = null,
        IReadOnlyList<string>? keychainNames = null,
        IReadOnlyList<string>? tradeItemNames = null,
        double? minFloat = null,
        double? maxFloat = null,
        bool? returnable = null,
        bool? luckyBuy = null,
        bool? hasPattern = null,
        int? minPatternIndex = null,
        int? maxPatternIndex = null,
        bool? hasBlueGem = null,
        int? minBlueGem = null,
        int? maxBlueGem = null,
        bool? hasFade = null,
        int? minFade = null,
        int? maxFade = null,
        long? minPrice = null,
        long? maxPrice = null,
        IReadOnlyList<string>? type = null,
        string? ordering = null,
        bool? lowToHigh = null,
        string? query = null,
        bool? hasHighlight = null,
        IDictionary<string, object?>? extra = null,
        CancellationToken cancellationToken = default)
    {
        subTypeInternalNames ??= [];
        modelInternalNames ??= [];
        exteriorInternalNames ??= [];
        finishInternalNames ??= [];
        qualityInternalNames ??= [];
        rarityInternalNames ??= [];
        colorNames ??= [];
        collectionNames ??= [];
        teamNames ??= [];
        playerNames ??= [];
        stickerNames ??= [];
        keychainNames ??= [];
        tradeItemNames ??= [];
        return _client.PostAsync("steam/feed", Payload.Build(new Dictionary<string, object?>
        {
            ["subTypeInternalNames"] = subTypeInternalNames,
            ["modelInternalNames"] = modelInternalNames,
            ["exteriorInternalNames"] = exteriorInternalNames,
            ["finishInternalNames"] = finishInternalNames,
            ["qualityInternalNames"] = qualityInternalNames,
            ["rarityInternalNames"] = rarityInternalNames,
            ["colorNames"] = colorNames,
            ["collectionNames"] = collectionNames,
            ["teamNames"] = teamNames,
            ["playerNames"] = playerNames,
            ["count"] = count,
            ["cursor"] = cursor,
            ["stickerNames"] = stickerNames,
            ["keychainNames"] = keychainNames,
            ["tradeItemNames"] = tradeItemNames,
            ["minFloat"] = minFloat,
            ["maxFloat"] = maxFloat,
            ["returnable"] = returnable,
            ["luckyBuy"] = luckyBuy,
            ["hasPattern"] = hasPattern,
            ["minPatternIndex"] = minPatternIndex,
            ["maxPatternIndex"] = maxPatternIndex,
            ["hasBlueGem"] = hasBlueGem,
            ["minBlueGem"] = minBlueGem,
            ["maxBlueGem"] = maxBlueGem,
            ["hasFade"] = hasFade,
            ["minFade"] = minFade,
            ["maxFade"] = maxFade,
            ["minPrice"] = minPrice,
            ["maxPrice"] = maxPrice,
            ["type"] = type,
            ["ordering"] = ordering,
            ["lowToHigh"] = lowToHigh,
            ["query"] = query,
            ["hasHighlight"] = hasHighlight,
        }, extra, dropNulls: true), cancellationToken);
    }

    public Task<JsonElement> ModelsAsync(IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default) =>
        _client.GetAsync("steam/trade-item-properties/models", Payload.BuildQuery(query), cancellationToken: cancellationToken);

    public Task<JsonElement> VariantsAsync(IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default) =>
        _client.GetAsync("steam/trade-item-properties/variants", Payload.BuildQuery(query), cancellationToken: cancellationToken);

    public Task<JsonElement> ExteriorsAsync(IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default) =>
        _client.GetAsync("steam/trade-item-properties/exteriors", Payload.BuildQuery(query), cancellationToken: cancellationToken);

    public Task<JsonElement> RaritiesAsync(IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default) =>
        _client.GetAsync("steam/trade-item-properties/rarities", Payload.BuildQuery(query), cancellationToken: cancellationToken);

    public Task<JsonElement> FinishesAsync(string? modelInternalName = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default) =>
        _client.GetAsync("steam/trade-item-properties/finishes", Payload.BuildQuery(Payload.Build(new Dictionary<string, object?>
        {
            ["modelInternalName"] = modelInternalName,
        }, query, dropNulls: true)), cancellationToken: cancellationToken);

    public Task<JsonElement> CollectionsAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync("steam/trade-item-properties/collections", cancellationToken: cancellationToken);

    public Task<JsonElement> TypesAsync(IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default) =>
        _client.GetAsync("steam/trade-item-properties/types", Payload.BuildQuery(query), cancellationToken: cancellationToken);

    public Task<JsonElement> QualitiesAsync(IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default) =>
        _client.GetAsync("steam/trade-item-properties/qualities", Payload.BuildQuery(query), cancellationToken: cancellationToken);

    public Task<JsonElement> ItemAsync(string steamId, CancellationToken cancellationToken = default) =>
        _client.GetAsync($"steam/trade-items/item/{Uri.EscapeDataString(steamId)}", cancellationToken: cancellationToken);

    public Task<JsonElement> TradeItemAsync(
        string steamId,
        int count = 20,
        string? cursor = null,
        long? minPrice = null,
        long? maxPrice = null,
        IReadOnlyList<string>? type = null,
        string? ordering = null,
        bool? lowToHigh = null,
        IDictionary<string, object?>? extra = null,
        CancellationToken cancellationToken = default) =>
        _client.PostAsync($"steam/feed/trade-item/{Uri.EscapeDataString(steamId)}", Payload.Build(new Dictionary<string, object?>
        {
            ["count"] = count,
            ["cursor"] = cursor,
            ["minPrice"] = minPrice,
            ["maxPrice"] = maxPrice,
            ["type"] = type,
            ["ordering"] = ordering,
            ["lowToHigh"] = lowToHigh,
        }, extra, dropNulls: true), cancellationToken);
}
