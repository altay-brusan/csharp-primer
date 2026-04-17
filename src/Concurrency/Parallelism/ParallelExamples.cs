namespace Concurrency.Parallelism;

/// <summary>
/// Demonstrates the Task Parallel Library (TPL): Parallel.ForEach,
/// PLINQ, and Task.Run for CPU-bound work.
///
/// Guidelines:
/// • Use Parallel.ForEach / PLINQ only for CPU-bound work with independent iterations.
/// • Use Task.Run to offload CPU-bound work from the UI / request thread.
/// • Avoid sharing mutable state across parallel iterations; prefer aggregation.
/// </summary>
public static class ParallelExamples
{
    // ── Parallel.ForEach with local state to avoid contention ────────────────
    // Each thread accumulates into a local sum; locals are merged at the end.
    public static long ParallelSum(IList<int> items)
    {
        long total = 0;
        Parallel.ForEach(
            items,
            localInit: () => 0L,
            body: (item, _, localSum) => localSum + item,
            localFinally: localSum => Interlocked.Add(ref total, localSum));
        return total;
    }

    // ── Parallel.For with degree-of-parallelism limit ─────────────────────────
    public static double[] ComputeExpensive(double[] inputs, int maxDegreeOfParallelism = -1)
    {
        var results = new double[inputs.Length];
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism < 1
                ? Environment.ProcessorCount
                : maxDegreeOfParallelism
        };

        Parallel.For(0, inputs.Length, options, i =>
            results[i] = Math.Sqrt(Math.Abs(Math.Sin(inputs[i]) * Math.Cos(inputs[i]))));

        return results;
    }

    // ── PLINQ (Parallel LINQ) ─────────────────────────────────────────────────
    // AsParallel() converts an IEnumerable into a PLINQ query.
    // AsOrdered() preserves order at the cost of some parallelism benefit.
    public static IReadOnlyList<int> PrimesBelowPlinq(int limit)
    {
        return Enumerable
            .Range(2, limit - 1)
            .AsParallel()
            .AsOrdered()
            .Where(IsPrime)
            .ToList();

        static bool IsPrime(int n)
        {
            if (n < 2) return false;
            for (int i = 2; (long)i * i <= n; i++)
                if (n % i == 0) return false;
            return true;
        }
    }

    // ── Task.Run for CPU-bound work ───────────────────────────────────────────
    // Wraps synchronous, CPU-heavy work in a Task so callers can await it.
    public static Task<long> SumRangeAsync(int from, int to, CancellationToken ct = default) =>
        Task.Run(() =>
        {
            long sum = 0;
            for (int i = from; i <= to; i++)
            {
                ct.ThrowIfCancellationRequested();
                sum += i;
            }
            return sum;
        }, ct);

    // ── Parallel.ForEachAsync (C# 6 / .NET 6+) ───────────────────────────────
    // CPU-bounded fan-out that supports async bodies.
    public static async Task<IReadOnlyList<string>> ProcessItemsAsync(
        IEnumerable<int> items,
        int maxDegreeOfParallelism = 4,
        CancellationToken ct = default)
    {
        var results = new System.Collections.Concurrent.ConcurrentBag<string>();
        await Parallel.ForEachAsync(
            items,
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = ct },
            async (item, token) =>
            {
                await Task.Delay(1, token).ConfigureAwait(false); // simulate I/O
                results.Add($"processed-{item}");
            }).ConfigureAwait(false);
        return [.. results];
    }
}
