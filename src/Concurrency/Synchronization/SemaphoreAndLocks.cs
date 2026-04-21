namespace Concurrency.Synchronization;

/// <summary>
/// Demonstrates common synchronisation primitives:
/// SemaphoreSlim, lock, Interlocked, and ReaderWriterLockSlim.
///
/// SemaphoreSlim is the preferred async-compatible throttle.
/// Interlocked provides lock-free atomic operations on primitives.
/// lock (Monitor) is fine for short, synchronous critical sections.
/// ReaderWriterLockSlim allows concurrent reads but exclusive writes.
/// </summary>
public static class SemaphoreAndLocks
{
    // ── SemaphoreSlim — limit concurrent async work ───────────────────────────
    // Throttles the number of concurrent downloads / API calls / etc.
    public static async Task<IReadOnlyList<string>> ThrottledFetchAsync(
        IReadOnlyList<string> urls,
        int maxConcurrency = 4,
        CancellationToken ct = default)
    {
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var results = new string[urls.Count];

        var tasks = urls.Select(async (url, index) =>
        {
            await semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                await Task.Delay(10, ct).ConfigureAwait(false); // simulate network
                results[index] = $"fetched:{url}";
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
        return results;
    }

    // ── lock — short synchronous critical section ────────────────────────────
    public class ThreadSafeCounter
    {
        private readonly object _sync = new();
        private int _value;

        public void Increment() { lock (_sync) { _value++; } }
        public void Decrement() { lock (_sync) { _value--; } }
        public int  Value      { get { lock (_sync) { return _value; } } }
    }

    // ── Interlocked — lock-free atomic operations ─────────────────────────────
    public class AtomicCounter
    {
        private long _value;

        public void Increment()               => Interlocked.Increment(ref _value);
        public void Add(long amount)          => Interlocked.Add(ref _value, amount);
        public long Value                     => Interlocked.Read(ref _value);

        // Compare-and-swap: only sets _value to newVal if it equals expected.
        public bool TryUpdate(long expected, long newVal) =>
            Interlocked.CompareExchange(ref _value, newVal, expected) == expected;
    }

    // ── ReaderWriterLockSlim — optimistic concurrent reads ───────────────────
    public class ReadWriteCache<TKey, TValue> where TKey : notnull
    {
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly Dictionary<TKey, TValue> _data = [];

        public TValue? Get(TKey key)
        {
            _lock.EnterReadLock();
            try   { return _data.TryGetValue(key, out var v) ? v : default; }
            finally { _lock.ExitReadLock(); }
        }

        public void Set(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try   { _data[key] = value; }
            finally { _lock.ExitWriteLock(); }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_data.TryGetValue(key, out var existing)) return existing;

                _lock.EnterWriteLock();
                try
                {
                    // Double-check after acquiring write lock.
                    if (!_data.TryGetValue(key, out existing))
                        _data[key] = existing = factory(key);
                    return existing!;
                }
                finally { _lock.ExitWriteLock(); }
            }
            finally { _lock.ExitUpgradeableReadLock(); }
        }
    }
}
