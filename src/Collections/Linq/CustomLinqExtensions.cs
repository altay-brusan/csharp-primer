namespace Collections.Linq;

/// <summary>
/// Demonstrates custom LINQ extension methods using iterator blocks (yield return)
/// and lazy evaluation.
///
/// Custom operators should:
/// • Accept IEnumerable&lt;T&gt; (or IAsyncEnumerable&lt;T&gt; for async variants).
/// • Be lazy: use yield return rather than building a list internally.
/// • Validate arguments eagerly (before the first yield) using a local iterator.
/// </summary>
public static class CustomLinqExtensions
{
    // ── Batch / Windowing ─────────────────────────────────────────────────────
    // Yields non-overlapping batches of exactly `size` (last batch may be smaller).
    public static IEnumerable<IReadOnlyList<T>> Batch<T>(
        this IEnumerable<T> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
        return Core(source, size);

        static IEnumerable<IReadOnlyList<T>> Core(IEnumerable<T> src, int batchSize)
        {
            var batch = new List<T>(batchSize);
            foreach (var item in src)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch.AsReadOnly();
                    batch = new List<T>(batchSize);
                }
            }
            if (batch.Count > 0) yield return batch.AsReadOnly();
        }
    }

    // ── Sliding window ────────────────────────────────────────────────────────
    public static IEnumerable<IReadOnlyList<T>> Window<T>(
        this IEnumerable<T> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
        return Core(source, size);

        static IEnumerable<IReadOnlyList<T>> Core(IEnumerable<T> src, int windowSize)
        {
            var buffer = new Queue<T>(windowSize);
            foreach (var item in src)
            {
                buffer.Enqueue(item);
                if (buffer.Count == windowSize)
                {
                    yield return buffer.ToList().AsReadOnly();
                    buffer.Dequeue();
                }
            }
        }
    }

    // ── Scan (running aggregate) ──────────────────────────────────────────────
    // Like Aggregate but yields each intermediate accumulated value.
    public static IEnumerable<TAccumulate> Scan<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate seed,
        Func<TAccumulate, TSource, TAccumulate> accumulator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(accumulator);
        var current = seed;
        foreach (var item in source)
        {
            current = accumulator(current, item);
            yield return current;
        }
    }

    // ── TakeWhileWithLast ────────────────────────────────────────────────────
    // Like TakeWhile but also includes the first failing element.
    public static IEnumerable<T> TakeUntil<T>(
        this IEnumerable<T> source, Func<T, bool> stopAfter)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(stopAfter);
        foreach (var item in source)
        {
            yield return item;
            if (stopAfter(item)) yield break;
        }
    }

    // ── ForEach (execute action for side-effects) ─────────────────────────────
    // Note: LINQ purists argue against ForEach because it encourages side-effects.
    // Use it only when you intentionally want imperative side-effects.
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);
        foreach (var item in source)
            action(item);
    }
}
