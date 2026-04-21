namespace Streams.Memory;

/// <summary>
/// Demonstrates MemoryStream, BinaryReader/Writer, and ArrayPool&lt;byte&gt;
/// for efficient in-memory serialisation and buffer management.
///
/// ArrayPool&lt;byte&gt; avoids repeated heap allocations for temporary byte buffers.
/// Use it whenever you need a short-lived buffer larger than 256 bytes.
/// </summary>
public static class MemoryStreamExamples
{
    // ── Round-trip with MemoryStream ──────────────────────────────────────────
    // Write structured data to a MemoryStream, then read it back.
    public static byte[] SerialiseIntegers(IReadOnlyList<int> values)
    {
        using var ms     = new MemoryStream();
        using var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true);

        writer.Write(values.Count);
        foreach (var v in values)
            writer.Write(v);

        return ms.ToArray();
    }

    public static IReadOnlyList<int> DeserialiseIntegers(byte[] data)
    {
        using var ms     = new MemoryStream(data);
        using var reader = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true);

        int count  = reader.ReadInt32();
        var values = new List<int>(count);
        for (int i = 0; i < count; i++)
            values.Add(reader.ReadInt32());
        return values;
    }

    // ── Rent from ArrayPool to avoid heap pressure ────────────────────────────
    // Always return rented arrays in a finally block.
    public static async Task<int> ProcessWithPooledBufferAsync(
        Stream source, Func<byte[], int, int> process,
        CancellationToken ct = default)
    {
        const int bufferSize = 4096;
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            int total = 0;
            int read;
            while ((read = await source.ReadAsync(buffer.AsMemory(0, bufferSize), ct)
                                       .ConfigureAwait(false)) > 0)
            {
                total += process(buffer, read);
            }
            return total;
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    // ── RecyclableMemoryStream pattern ────────────────────────────────────────
    // In production, use Microsoft.IO.RecyclableMemoryStream instead of plain MemoryStream
    // to pool the underlying buffers and avoid LOH allocations.
    // The example below shows the same API surface as MemoryStream.
    public static async Task<byte[]> CaptureStreamAsync(
        Func<Stream, CancellationToken, Task> writer, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        await writer(ms, ct).ConfigureAwait(false);
        return ms.ToArray();
    }

    // ── Efficient string-to-byte conversion via Encoding ─────────────────────
    public static byte[] EncodeUtf8(string text)
    {
        int maxBytes = System.Text.Encoding.UTF8.GetMaxByteCount(text.Length);
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(maxBytes);
        try
        {
            int written = System.Text.Encoding.UTF8.GetBytes(text, buffer);
            return buffer[..written]; // copy exact slice
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
