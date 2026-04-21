using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace Streams.Pipelines;

/// <summary>
/// Demonstrates System.IO.Pipelines — a high-throughput I/O primitive that
/// separates producers and consumers with a built-in back-pressure buffer.
///
/// Pipelines excel at:
/// • Parsing network streams line-by-line or record-by-record.
/// • Avoiding large intermediate byte[] allocations.
/// • Enabling concurrent read (producer) and parse (consumer).
///
/// The core types:
///   PipeWriter — producer writes into a leased buffer.
///   PipeReader — consumer reads what was written, marking consumed bytes.
///   Pipe       — pairs a PipeWriter and PipeReader.
/// </summary>
public static class PipelineExamples
{
    // ── Write text lines to a Pipe, then read and count lines ─────────────────
    public static async Task<int> CountLinesAsync(
        IEnumerable<string> lines, CancellationToken ct = default)
    {
        var pipe = new Pipe();

        Task writing  = WriteLinesToPipeAsync(pipe.Writer, lines, ct);
        Task<int> counting = CountLinesFromPipeAsync(pipe.Reader, ct);

        await Task.WhenAll(writing, counting).ConfigureAwait(false);
        return await counting;
    }

    private static async Task WriteLinesToPipeAsync(
        PipeWriter writer, IEnumerable<string> lines, CancellationToken ct)
    {
        foreach (var line in lines)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(line + "\n");
            await writer.WriteAsync(encoded, ct).ConfigureAwait(false);
        }
        await writer.CompleteAsync().ConfigureAwait(false);
    }

    private static async Task<int> CountLinesFromPipeAsync(
        PipeReader reader, CancellationToken ct)
    {
        int lineCount = 0;

        while (true)
        {
            ReadResult result = await reader.ReadAsync(ct).ConfigureAwait(false);
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (TryReadLine(ref buffer, out _))
                lineCount++;

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted) break;
        }

        await reader.CompleteAsync().ConfigureAwait(false);
        return lineCount;
    }

    private static bool TryReadLine(
        ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        SequencePosition? position = buffer.PositionOf((byte)'\n');
        if (position is null)
        {
            line = default;
            return false;
        }
        line   = buffer.Slice(0, position.Value);
        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
        return true;
    }

    // ── Parse key=value pairs from a pipe ────────────────────────────────────
    public static async Task<Dictionary<string, string>> ParseKeyValuesAsync(
        IEnumerable<string> rawLines, CancellationToken ct = default)
    {
        var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var pipe    = new Pipe();

        var writeTask = WriteLinesToPipeAsync(pipe.Writer, rawLines, ct);
        var parseTask = Task.Run(async () =>
        {
            while (true)
            {
                ReadResult result = await pipe.Reader.ReadAsync(ct).ConfigureAwait(false);
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (TryReadLine(ref buffer, out var lineSeq))
                {
                    string raw = Encoding.UTF8.GetString(lineSeq);
                    int eq = raw.IndexOf('=');
                    if (eq > 0)
                        results[raw[..eq].Trim()] = raw[(eq + 1)..].Trim();
                }

                pipe.Reader.AdvanceTo(buffer.Start, buffer.End);
                if (result.IsCompleted) break;
            }
            await pipe.Reader.CompleteAsync().ConfigureAwait(false);
        }, ct);

        await Task.WhenAll(writeTask, parseTask).ConfigureAwait(false);
        return results;
    }
}
