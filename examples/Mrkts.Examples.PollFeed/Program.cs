using Mrkts;

var token = Environment.GetEnvironmentVariable("MRKT_TOKEN");
if (string.IsNullOrEmpty(token))
{
    Console.Error.WriteLine("MRKT_TOKEN is not set");
    return 1;
}

await using var client = new MrktClient(token);
await client.StartAsync();
await foreach (var response in client.Poll((c, cancellationToken) => c.Market.Gifts.FeedAsync(count: 20, cancellationToken: cancellationToken), interval: 2.0))
    Console.WriteLine(response);
return 0;
