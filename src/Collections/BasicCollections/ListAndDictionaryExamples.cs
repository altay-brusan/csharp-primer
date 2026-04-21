namespace Collections.BasicCollections;

/// <summary>
/// Demonstrates the core generic list and dictionary collections.
///
/// Prefer the interface type (IList&lt;T&gt;, IDictionary&lt;…&gt;) in public APIs so callers
/// are not bound to a specific implementation.
/// </summary>
public static class ListAndDictionaryExamples
{
    // ── List<T> ───────────────────────────────────────────────────────────────
    public static IReadOnlyList<string> BuildPriorityList(
        IEnumerable<string> items,
        Comparison<string> comparer)
    {
        var list = new List<string>(items);  // initialise from sequence
        list.Sort(comparer);
        list.RemoveAll(s => string.IsNullOrWhiteSpace(s)); // in-place predicate removal
        return list;
    }

    // Span-based bulk copy when you need a flat array later.
    public static int[] ToSortedArray(IEnumerable<int> values)
    {
        int[] arr = [.. values];   // collection expression (C# 12)
        Array.Sort(arr);
        return arr;
    }

    // ── Dictionary<TKey, TValue> ──────────────────────────────────────────────
    // GetValueOrDefault and TryGetValue avoid KeyNotFoundException.
    public static Dictionary<string, int> BuildWordFrequency(IEnumerable<string> words)
    {
        var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var word in words)
        {
            freq.TryGetValue(word, out int count);    // 0 if not found
            freq[word] = count + 1;
        }
        return freq;
    }

    // CollectionsMarshal.GetValueRefOrAddDefault lets you update in place (no double lookup).
    public static Dictionary<string, int> BuildWordFrequencyFast(IEnumerable<string> words)
    {
        var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var word in words)
        {
            ref int count = ref System.Runtime.InteropServices.CollectionsMarshal
                .GetValueRefOrAddDefault(freq, word, out _);
            count++;
        }
        return freq;
    }

    // ── SortedDictionary keeps keys sorted ────────────────────────────────────
    public static SortedDictionary<DateTime, string> BuildTimeline(
        IEnumerable<(DateTime date, string @event)> events)
    {
        var timeline = new SortedDictionary<DateTime, string>();
        foreach (var (date, @event) in events)
            timeline[date] = @event;
        return timeline;
    }

    // ── ILookup: like a dictionary of lists ──────────────────────────────────
    // Created from LINQ's ToLookup; read-only after creation.
    public record Item(string Category, string Name, decimal Price);

    public static ILookup<string, Item> GroupByCategory(IEnumerable<Item> items) =>
        items.ToLookup(i => i.Category, StringComparer.OrdinalIgnoreCase);
}
