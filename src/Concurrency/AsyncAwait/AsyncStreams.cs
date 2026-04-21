namespace Concurrency.AsyncAwait;

/// <summary>
/// Demonstrates async streams (IAsyncEnumerable&lt;T&gt;) introduced in C# 8.
///
/// Use async streams when data arrives asynchronously and you want to process it
/// item-by-item without buffering everything in memory first.
/// The producer uses `yield return` inside an `async` method.
/// The consumer uses `await foreach`.
/// </summary>
public static class AsyncStreams
{
    // ── Producing an async stream ─────────────────────────────────────────────
    // Simulates a paginated API: yields batches of integers with a delay between pages.
    public static async IAsyncEnumerable<int> GeneratePagedAsync(
        int totalItems,
        int pageSize = 5,
        int pageDelayMs = 20,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        for (int page = 0; page * pageSize < totalItems; page++)
        {
            await Task.Delay(pageDelayMs, ct).ConfigureAwait(false);

            int start = page * pageSize;
            int end   = Math.Min(start + pageSize, totalItems);
            for (int i = start; i < end; i++)
                yield return i;
        }
    }

    // ── Consuming an async stream with await foreach ──────────────────────────
    public static async Task<List<int>> CollectAsync(
        IAsyncEnumerable<int> source, CancellationToken ct = default)
    {
        var results = new List<int>();
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
            results.Add(item);
        return results;
    }

    // ── Transforming an async stream ──────────────────────────────────────────
    // Async LINQ-like projection: applies a mapping while preserving lazy evaluation.
    public static async IAsyncEnumerable<TOut> SelectAsync<TIn, TOut>(
        IAsyncEnumerable<TIn> source,
        Func<TIn, Task<TOut>> selector,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
            yield return await selector(item).ConfigureAwait(false);
    }

    // ── Async stream with early termination via CancellationToken ─────────────
    public static async Task<List<int>> TakeUntilAsync(
        IAsyncEnumerable<int> source, int maxItems, CancellationToken ct = default)
    {
        var results = new List<int>(maxItems);
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            results.Add(item);
            if (results.Count >= maxItems) break;
        }
        return results;
    }
}
