namespace Concurrency.Synchronization;

/// <summary>
/// Demonstrates CancellationToken and cooperative cancellation.
///
/// CancellationToken is the standard way to signal cancellation in .NET.
/// Pass tokens through the entire call chain; check them at loop boundaries
/// and on I/O calls. Never swallow OperationCanceledException — let it propagate.
/// </summary>
public static class CancellationExamples
{
    // ── Basic cancellable operation ───────────────────────────────────────────
    public static async Task<List<int>> LongRunningWorkAsync(
        int itemCount, CancellationToken ct)
    {
        var results = new List<int>(itemCount);
        for (int i = 0; i < itemCount; i++)
        {
            ct.ThrowIfCancellationRequested();            // cooperative check
            await Task.Delay(10, ct).ConfigureAwait(false); // passes token to I/O
            results.Add(i);
        }
        return results;
    }

    // ── Timeout via CancellationTokenSource ───────────────────────────────────
    // Use CreateLinkedTokenSource to combine a timeout with the caller's token.
    public static async Task<T> WithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        CancellationToken callerToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(callerToken);
        cts.CancelAfter(timeout);
        return await operation(cts.Token).ConfigureAwait(false);
    }

    // ── Checking cancellation without throwing ────────────────────────────────
    public static async IAsyncEnumerable<int> YieldUntilCancelledAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken ct = default)
    {
        int n = 0;
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(5, ct).ConfigureAwait(false);
            yield return n++;
        }
    }

    // ── Registering a clean-up callback ──────────────────────────────────────
    // CancellationToken.Register fires synchronously when cancelled.
    public static async Task<string> CancellableOperationWithCleanupAsync(
        CancellationToken ct)
    {
        string status = "started";
        using var registration = ct.Register(() => status = "cancelled");

        await Task.Delay(50, ct).ConfigureAwait(false);
        return ct.IsCancellationRequested ? status : "completed";
    }

    // ── Linked token sources ──────────────────────────────────────────────────
    // Combine a user-initiated cancellation with a budget/timeout.
    public static CancellationTokenSource CreateBudgetedToken(
        CancellationToken userToken, int budgetMs)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(userToken);
        cts.CancelAfter(budgetMs);
        return cts; // caller disposes
    }
}
