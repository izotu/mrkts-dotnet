using System.Collections;
using System.Globalization;
using System.Text.Json;

namespace Mrkts;

internal static class Payload
{
    public static Dictionary<string, object?> Build(Dictionary<string, object?> payload, IDictionary<string, object?>? extra, bool dropNulls)
    {
        if (extra is not null)
            foreach (var (key, value) in extra)
                payload[key] = value;
        if (dropNulls)
            foreach (var key in payload.Where(pair => pair.Value is null).Select(pair => pair.Key).ToArray())
                payload.Remove(key);
        return payload;
    }

    public static string? BuildQuery(IEnumerable<KeyValuePair<string, object?>>? query)
    {
        if (query is null)
            return null;
        var parts = new List<string>();
        foreach (var (key, value) in query)
        {
            if (value is null)
                continue;
            var name = Uri.EscapeDataString(key);
            if (value is string || value is not IEnumerable items)
                parts.Append(name, value);
            else
                foreach (var item in items)
                    parts.Append(name, item);
        }
        return parts.Count == 0 ? null : string.Join("&", parts);
    }

    private static void Append(this List<string> parts, string name, object? value) =>
        parts.Add($"{name}={Uri.EscapeDataString(Format(value))}");

    private static string Format(object? value) =>
        value is IFormattable formattable ? formattable.ToString(null, CultureInfo.InvariantCulture) : value?.ToString() ?? "";
}
