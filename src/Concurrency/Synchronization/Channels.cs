using System.Threading.Channels;

namespace Concurrency.Synchronization;

/// <summary>
/// Demonstrates System.Threading.Channels — a high-performance producer/consumer
/// pipeline primitive.
///
/// Use channels when:
/// • You need bounded back-pressure (BoundedChannel).
/// • You want to decouple producers from consumers asynchronously.
/// • You need multiple writers and/or multiple readers (configurable).
///
/// Channel&lt;T&gt; has two ends: a ChannelWriter&lt;T&gt; and a ChannelReader&lt;T&gt;.
/// </summary>
public static class ChannelExamples
{
    // ── Unbounded channel: no back-pressure ──────────────────────────────────
    public static Channel<T> CreateUnbounded<T>() =>
        Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });

    // ── Bounded channel: apply back-pressure ─────────────────────────────────
    public static Channel<T> CreateBounded<T>(int capacity) =>
        Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode    = BoundedChannelFullMode.Wait,  // writer awaits when full
            SingleReader = true,
            SingleWriter = false,
        });

    // ── Producer ──────────────────────────────────────────────────────────────
    public static async Task ProduceAsync<T>(
        ChannelWriter<T> writer,
        IEnumerable<T> items,
        int delayMs = 0,
        CancellationToken ct = default)
    {
        try
        {
            foreach (var item in items)
            {
                if (delayMs > 0)
                    await Task.Delay(delayMs, ct).ConfigureAwait(false);
                await writer.WriteAsync(item, ct).ConfigureAwait(false);
            }
        }
        finally
        {
            writer.Complete(); // signal no more items
        }
    }

    // ── Consumer ──────────────────────────────────────────────────────────────
    public static async Task<List<T>> ConsumeAsync<T>(
        ChannelReader<T> reader,
        CancellationToken ct = default)
    {
        var results = new List<T>();
        await foreach (var item in reader.ReadAllAsync(ct).ConfigureAwait(false))
            results.Add(item);
        return results;
    }

    // ── Pipeline: producer → transform → consumer ─────────────────────────────
    // Chains two channels so the output of one stage feeds the input of the next.
    public static async Task<List<TOut>> PipelineAsync<TIn, TOut>(
        IEnumerable<TIn> source,
        Func<TIn, TOut> transform,
        CancellationToken ct = default)
    {
        var inputChannel  = CreateUnbounded<TIn>();
        var outputChannel = CreateUnbounded<TOut>();

        // Stage 1: produce
        var produceTask = ProduceAsync(inputChannel.Writer, source, ct: ct);

        // Stage 2: transform
        var transformTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var item in inputChannel.Reader.ReadAllAsync(ct))
                    await outputChannel.Writer.WriteAsync(transform(item), ct);
            }
            finally
            {
                outputChannel.Writer.Complete();
            }
        }, ct);

        // Stage 3: consume
        var results = await ConsumeAsync(outputChannel.Reader, ct).ConfigureAwait(false);
        await Task.WhenAll(produceTask, transformTask).ConfigureAwait(false);
        return results;
    }
}
