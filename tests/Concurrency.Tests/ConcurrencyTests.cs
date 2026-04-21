using Concurrency.AsyncAwait;
using Concurrency.Parallelism;
using Concurrency.Synchronization;

namespace Concurrency.Tests;

public class AsyncBasicsTests
{
    [Fact]
    public async Task FetchMessageAsync_ReturnsMessage()
    {
        var msg = await AsyncBasics.FetchMessageAsync(10);
        Assert.Contains("10", msg);
    }

    [Fact]
    public async Task FetchAllAsync_ReturnsAllResults()
    {
        var results = await AsyncBasics.FetchAllAsync([10, 20, 30]);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task RaceAsync_ReturnsWinnerFirst()
    {
        var winner = await AsyncBasics.RaceAsync([100, 10, 200]);
        Assert.Contains("10", winner);
    }

    [Fact]
    public async Task RetryAsync_RetriesOnFailure()
    {
        int attempts = 0;
        var result = await AsyncBasics.RetryAsync(async ct =>
        {
            attempts++;
            if (attempts < 3) throw new InvalidOperationException("transient");
            await Task.Delay(1, ct);
            return "ok";
        }, maxAttempts: 3, baseDelayMs: 10);

        Assert.Equal("ok", result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task GetCachedAsync_ReturnsSameValue()
    {
        var first  = await AsyncBasics.GetCachedAsync(1);
        var second = await AsyncBasics.GetCachedAsync(1);
        Assert.Equal(first, second);
    }
}

public class AsyncStreamsTests
{
    [Fact]
    public async Task GeneratePagedAsync_YieldsCorrectItems()
    {
        var items = await AsyncStreams.CollectAsync(
            AsyncStreams.GeneratePagedAsync(15, pageSize: 5, pageDelayMs: 5));
        Assert.Equal(Enumerable.Range(0, 15).ToList(), items);
    }

    [Fact]
    public async Task TakeUntilAsync_LimitsResults()
    {
        var items = await AsyncStreams.TakeUntilAsync(
            AsyncStreams.GeneratePagedAsync(100, pageSize: 5, pageDelayMs: 5), 7);
        Assert.Equal(7, items.Count);
    }
}

public class ParallelExamplesTests
{
    [Fact]
    public void ParallelSum_SumsLargeList()
    {
        var items = Enumerable.Range(1, 1000).ToList();
        long expected = items.Sum(x => (long)x);
        Assert.Equal(expected, ParallelExamples.ParallelSum(items));
    }

    [Fact]
    public void PrimesBelowPlinq_FindsCorrectPrimes()
    {
        var primes = ParallelExamples.PrimesBelowPlinq(20);
        Assert.Equal(new[] { 2, 3, 5, 7, 11, 13, 17, 19 }, primes);
    }

    [Fact]
    public async Task SumRangeAsync_ComputesGaussSum()
    {
        long result = await ParallelExamples.SumRangeAsync(1, 100);
        Assert.Equal(5050L, result);
    }

    [Fact]
    public async Task ProcessItemsAsync_ProcessesAll()
    {
        var results = await ParallelExamples.ProcessItemsAsync(Enumerable.Range(0, 10));
        Assert.Equal(10, results.Count);
    }
}

public class CancellationTests
{
    [Fact]
    public async Task LongRunningWork_CompletesWithoutCancellation()
    {
        var results = await CancellationExamples.LongRunningWorkAsync(5, CancellationToken.None);
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public async Task LongRunningWork_CancelsWhenRequested()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CancellationExamples.LongRunningWorkAsync(1000, cts.Token));
    }

    [Fact]
    public async Task WithTimeoutAsync_ThrowsOnTimeout()
    {
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CancellationExamples.WithTimeoutAsync<bool>(
                async ct => { await Task.Delay(5000, ct); return true; },
                timeout: TimeSpan.FromMilliseconds(50)));
    }
}

public class SemaphoreAndLocksTests
{
    [Fact]
    public async Task ThrottledFetchAsync_ReturnsAllResults()
    {
        var urls = Enumerable.Range(1, 8).Select(i => $"url{i}").ToList();
        var results = await SemaphoreAndLocks.ThrottledFetchAsync(urls, maxConcurrency: 3);
        Assert.Equal(8, results.Count);
        Assert.All(results, r => Assert.StartsWith("fetched:", r));
    }

    [Fact]
    public void ThreadSafeCounter_IncrementDecrement_Correctly()
    {
        var counter = new SemaphoreAndLocks.ThreadSafeCounter();
        Parallel.For(0, 100, _ => counter.Increment());
        Parallel.For(0, 40,  _ => counter.Decrement());
        Assert.Equal(60, counter.Value);
    }

    [Fact]
    public void AtomicCounter_IncrementsCorrectly()
    {
        var counter = new SemaphoreAndLocks.AtomicCounter();
        Parallel.For(0, 1000, _ => counter.Increment());
        Assert.Equal(1000L, counter.Value);
    }
}

public class ChannelTests
{
    [Fact]
    public async Task PipelineAsync_TransformsAllItems()
    {
        var source  = Enumerable.Range(1, 10);
        var results = await ChannelExamples.PipelineAsync(source, x => x * x);
        Assert.Equal(10, results.Count);
        Assert.Contains(1,   results);
        Assert.Contains(100, results);
    }

    [Fact]
    public async Task ProduceConsume_RoundTrip()
    {
        var channel = ChannelExamples.CreateUnbounded<int>();
        var items   = Enumerable.Range(0, 5).ToList();

        var produceTask = ChannelExamples.ProduceAsync(channel.Writer, items);
        var results     = await ChannelExamples.ConsumeAsync(channel.Reader);
        await produceTask;

        Assert.Equal(items, results);
    }
}
