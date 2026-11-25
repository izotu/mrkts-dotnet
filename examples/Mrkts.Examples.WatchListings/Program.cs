using System.Text.Json;
using Mrkts;

var token = Environment.GetEnvironmentVariable("MRKT_TOKEN");
if (string.IsNullOrEmpty(token))
{
    Console.Error.WriteLine("usage: MRKT_TOKEN=<token> Mrkts.Examples.WatchListings [\"Plush Pepe\" ...]");
    return 1;
}

await using var client = new MrktClient(token);
await client.StartAsync();
Console.WriteLine(args.Length == 0
    ? "Watching new listings on the whole market. The first page is a baseline. Ctrl+C to stop."
    : $"Watching new listings in {string.Join(", ", args)}. The first page is a baseline. Ctrl+C to stop.");

await foreach (var listing in client.WatchListings(interval: 2.0, collectionNames: args.Length == 0 ? null : args))
    Console.WriteLine(Describe(listing));
return 0;

static string Describe(JsonElement listing)
{
    var gift = listing.ValueKind == JsonValueKind.Object && listing.TryGetProperty("gift", out var value) ? value : default;
    return string.Join("  ", new[]
    {
        Field(listing, "date"),
        Field(gift, "name"),
        Field(gift, "number") is { Length: > 0 } number ? $"#{number}" : "",
        Field(gift, "salePrice") is { Length: > 0 } price ? $"salePrice={price}" : "",
    }.Where(part => part.Length > 0));
}

static string Field(JsonElement element, string name) =>
    element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
        ? value.ToString()
        : "";
