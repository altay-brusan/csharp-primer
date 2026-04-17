using System.Collections.Concurrent;

namespace Collections.Advanced;

/// <summary>
/// Demonstrates thread-safe collections from System.Collections.Concurrent.
///
/// These collections are safe to read and write from multiple threads simultaneously
/// without external locking. Use them for shared state in concurrent scenarios.
///
/// ConcurrentDictionary  — thread-safe key/value store with atomic operations.
/// ConcurrentQueue&lt;T&gt;   — FIFO; lock-free enqueue/dequeue.
/// ConcurrentBag&lt;T&gt;     — unordered; optimised for same-thread produce/consume.
/// BlockingCollection&lt;T&gt;— bounding + blocking wrapper around IProducerConsumerCollection.
/// </summary>
public static class ConcurrentCollectionExamples
{
    // ── ConcurrentDictionary ──────────────────────────────────────────────────
    // GetOrAdd, AddOrUpdate, and TryUpdate provide atomic operations.
    public class HitCounter
    {
        private readonly ConcurrentDictionary<string, long> _counts = new(StringComparer.OrdinalIgnoreCase);

        // Atomically increment the counter for a URL.
        public long Record(string url) =>
            _counts.AddOrUpdate(url, addValue: 1, updateValueFactory: (_, old) => old + 1);

        public long Get(string url) => _counts.GetValueOrDefault(url, 0);

        public IReadOnlyDictionary<string, long> Snapshot() =>
            new Dictionary<string, long>(_counts);
    }

    // ── ConcurrentQueue<T> ────────────────────────────────────────────────────
    public static async Task<IReadOnlyList<int>> ConcurrentQueueDemoAsync(
        int producerCount = 2, int itemsPerProducer = 5, CancellationToken ct = default)
    {
        var queue   = new ConcurrentQueue<int>();
        var results = new ConcurrentBag<int>();

        // Multiple producers
        var producers = Enumerable.Range(0, producerCount).Select(p =>
            Task.Run(() =>
            {
                for (int i = 0; i < itemsPerProducer; i++)
                    queue.Enqueue(p * 100 + i);
            }, ct)).ToArray();

        await Task.WhenAll(producers).ConfigureAwait(false);

        // Single consumer
        while (queue.TryDequeue(out int item))
            results.Add(item);

        return [.. results];
    }

    // ── BlockingCollection<T> — classic producer/consumer with bounding ───────
    // The producer blocks when the collection is full; the consumer blocks when empty.
    public static async Task<IReadOnlyList<string>> BoundedProducerConsumerAsync(
        IEnumerable<string> inputs, int boundedCapacity = 4, CancellationToken ct = default)
    {
        var collection = new BlockingCollection<string>(boundedCapacity);
        var results    = new List<string>();

        var producer = Task.Run(() =>
        {
            foreach (var item in inputs)
            {
                if (ct.IsCancellationRequested) break;
                collection.Add(item, ct);      // blocks when full
            }
            collection.CompleteAdding();
        }, ct);

        var consumer = Task.Run(() =>
        {
            foreach (var item in collection.GetConsumingEnumerable(ct))
                results.Add($"processed:{item}");
        }, ct);

        await Task.WhenAll(producer, consumer).ConfigureAwait(false);
        return results;
    }
}
