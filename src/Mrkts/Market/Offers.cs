using System.Net;
using System.Text.Json;

namespace Mrkts.Market;

public sealed class Offers
{
    private readonly MrktClient _client;

    internal Offers(MrktClient client) => _client = client;

    public Task<JsonElement> CreateAsync(long price, string giftSaleId, CancellationToken cancellationToken = default) =>
        _client.PostAsync("offers/create", new Dictionary<string, object?> { ["price"] = price, ["giftSaleId"] = giftSaleId }, cancellationToken);

    public Task<JsonElement> CancelAsync(string offerId, CancellationToken cancellationToken = default) =>
        _client.PostAsync($"offers/cancel?offerId={Uri.EscapeDataString(offerId)}", cancellationToken: cancellationToken);

    public Task<JsonElement> MyOffersByGiftAsync(string id, int count = 20, CancellationToken cancellationToken = default) =>
        _client.GetAsync($"offers/by-gift/{Uri.EscapeDataString(id)}?count={count}", cancellationToken: cancellationToken);
}
