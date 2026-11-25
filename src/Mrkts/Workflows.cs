using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Mrkts;

public static class Workflows
{
    private static readonly TimeSpan SeenTtl = TimeSpan.FromSeconds(600);

    /// <summary>Poll a request and yield each response.</summary>
    public static IAsyncEnumerable<T> Poll<T>(this MrktClient client, Func<MrktClient, CancellationToken, Task<T>> fetch, double interval = 1.0, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fetch);
        CheckPoll(client, interval, nameof(Poll));
        return PollCore(client, fetch, interval, cancellationToken);
    }

    /// <summary>Poll the listing feed and yield each new listing event once.
    /// <para>The first response is a baseline and yields nothing. Later responses yield unseen events in arrival order, which may differ from event dates. Only the latest page is read, so the API's page size bounds how far back a late event can be noticed.</para>
    /// </summary>
    public static IAsyncEnumerable<JsonElement> WatchListings(
        this MrktClient client,
        double interval = 2.0,
        IReadOnlyList<string>? collectionNames = null,
        IReadOnlyList<string>? modelNames = null,
        IReadOnlyList<string>? backdropNames = null,
        int? number = null,
        long? minPrice = null,
        long? maxPrice = null,
        string? query = null,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default)
    {
        CheckPoll(client, interval, nameof(WatchListings));
        return WatchCore(client, interval, collectionNames, modelNames, backdropNames, number, minPrice, maxPrice, query, timeProvider ?? TimeProvider.System, cancellationToken);
    }

    private static void CheckPoll(MrktClient client, double interval, string workflow)
    {
        ArgumentNullException.ThrowIfNull(client);
        if (!double.IsFinite(interval) || interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), interval, "interval must be a finite number greater than zero");
        if (!client.IsStarted)
            throw new InvalidOperationException($"Start the MrktClient before running {workflow}()");
    }

    private static async IAsyncEnumerable<T> PollCore<T>(MrktClient client, Func<MrktClient, CancellationToken, Task<T>> fetch, double interval, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (true)
        {
            yield return await fetch(client, cancellationToken).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken).ConfigureAwait(false);
        }
    }

    private static async IAsyncEnumerable<JsonElement> WatchCore(
        MrktClient client,
        double interval,
        IReadOnlyList<string>? collectionNames,
        IReadOnlyList<string>? modelNames,
        IReadOnlyList<string>? backdropNames,
        int? number,
        long? minPrice,
        long? maxPrice,
        string? query,
        TimeProvider timeProvider,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var seen = new Dictionary<string, long>();
        var baseline = true;
        while (true)
        {
            var response = await client.Market.Gifts.FeedAsync(
                collectionNames: collectionNames,
                modelNames: modelNames,
                backdropNames: backdropNames,
                number: number,
                type: ["listing"],
                minPrice: minPrice,
                maxPrice: maxPrice,
                ordering: "Latest",
                lowToHigh: false,
                query: query,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            long now = timeProvider.GetTimestamp();
            foreach (var (eventId, firstSeen) in seen.ToArray())
                if (timeProvider.GetElapsedTime(firstSeen, now) > SeenTtl)
                    seen.Remove(eventId);
            if (response.ValueKind == JsonValueKind.Object && response.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var listing in items.EnumerateArray())
                {
                    var eventId = EventId(listing);
                    if (seen.ContainsKey(eventId))
                        continue;
                    seen[eventId] = now;
                    if (!baseline)
                        yield return listing;
                }
            }
            baseline = false;
            await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken).ConfigureAwait(false);
        }
    }

    private static string EventId(JsonElement listing)
    {
        if (listing.ValueKind != JsonValueKind.Object || !listing.TryGetProperty("id", out var id) || id.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return "Undefined:";
        var value = id.ValueKind == JsonValueKind.Number
            ? id.TryGetInt64(out var integer) ? integer.ToString(CultureInfo.InvariantCulture) : id.GetDouble().ToString("R", CultureInfo.InvariantCulture)
            : id.GetRawText();
        return $"{id.ValueKind}:{value}";
    }
}
