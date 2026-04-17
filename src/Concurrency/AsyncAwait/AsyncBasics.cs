namespace Concurrency.AsyncAwait;

/// <summary>
/// Demonstrates idiomatic async/await patterns.
///
/// Rules of thumb:
/// • Always use CancellationToken for cancellable work.
/// • Use ConfigureAwait(false) in library code to avoid context capture.
/// • Prefer ValueTask when a method frequently completes synchronously.
/// • Use Task.WhenAll / Task.WhenAny for concurrent fan-out.
/// • Never use Task.Result or Task.Wait() in async code (deadlock risk).
/// </summary>
public static class AsyncBasics
{
    // ── Simple async method ───────────────────────────────────────────────────
    public static async Task<string> FetchMessageAsync(
        int delayMs, CancellationToken ct = default)
    {
        await Task.Delay(delayMs, ct).ConfigureAwait(false);
        return $"Fetched after {delayMs} ms";
    }

    // ── Fan-out with Task.WhenAll ─────────────────────────────────────────────
    // Start all tasks concurrently; await them all at once.
    public static async Task<IReadOnlyList<string>> FetchAllAsync(
        IReadOnlyList<int> delays, CancellationToken ct = default)
    {
        var tasks = delays.Select(d => FetchMessageAsync(d, ct));
        string[] results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results;
    }

    // ── Task.WhenAny — first-one-wins ─────────────────────────────────────────
    public static async Task<string> RaceAsync(
        IReadOnlyList<int> delays, CancellationToken ct = default)
    {
        var tasks = delays.Select(d => FetchMessageAsync(d, ct)).ToList();
        Task<string> winner = await Task.WhenAny(tasks).ConfigureAwait(false);
        return await winner.ConfigureAwait(false);
    }

    // ── Retry with exponential back-off ──────────────────────────────────────
    public static async Task<T> RetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        int maxAttempts = 3,
        int baseDelayMs = 200,
        CancellationToken ct = default)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await operation(ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)
            {
                int delay = baseDelayMs * (1 << (attempt - 1)); // 200, 400, 800 ms …
                await Task.Delay(delay, ct).ConfigureAwait(false);
            }
        }
    }

    // ── Async void: avoid except for event handlers ───────────────────────────
    // Exceptions in async void cannot be caught by callers. Use async Task instead.
    // The following is the CORRECT pattern for event handlers:
    public static void OnButtonClickCorrect(object? sender, EventArgs e) =>
        _ = HandleClickAsync(); // fire-and-forget with discard

    private static async Task HandleClickAsync()
    {
        await Task.Delay(100).ConfigureAwait(false);
        // UI update would go here
    }

    // ── ValueTask: useful when fast-path is synchronous ───────────────────────
    private static readonly Dictionary<int, string> _cache = [];

    public static ValueTask<string> GetCachedAsync(int key, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out string? cached))
            return new ValueTask<string>(cached);          // synchronous path

        return FetchAndCacheAsync(key, ct);                // async path

        async ValueTask<string> FetchAndCacheAsync(int k, CancellationToken token)
        {
            string value = await FetchMessageAsync(50, token).ConfigureAwait(false);
            _cache[k] = value;
            return value;
        }
    }
}
