using System.IO.Compression;

namespace Streams.Memory;

/// <summary>
/// Demonstrates GZip and Brotli compression using decorator streams.
///
/// Stream composition pattern: wrap a base stream with a compression stream,
/// wrap that with a reader/writer. Data flows through the chain automatically.
///
/// GZip is widely supported (HTTP, gzip files). Brotli achieves better ratios
/// for text but requires .NET 2.1+.
/// </summary>
public static class CompressionExamples
{
    // ── GZip compress ─────────────────────────────────────────────────────────
    public static async Task<byte[]> GZipCompressAsync(
        byte[] data, CancellationToken ct = default)
    {
        using var output    = new MemoryStream();
        await using var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true);
        await gzip.WriteAsync(data, ct).ConfigureAwait(false);
        await gzip.FlushAsync(ct).ConfigureAwait(false);
        return output.ToArray();
    }

    // ── GZip decompress ───────────────────────────────────────────────────────
    public static async Task<byte[]> GZipDecompressAsync(
        byte[] compressed, CancellationToken ct = default)
    {
        using var input     = new MemoryStream(compressed);
        await using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output    = new MemoryStream();
        await gzip.CopyToAsync(output, ct).ConfigureAwait(false);
        return output.ToArray();
    }

    // ── Brotli compress ───────────────────────────────────────────────────────
    public static async Task<byte[]> BrotliCompressAsync(
        byte[] data, CancellationToken ct = default)
    {
        using var output      = new MemoryStream();
        await using var brotli = new BrotliStream(output, CompressionLevel.Optimal, leaveOpen: true);
        await brotli.WriteAsync(data, ct).ConfigureAwait(false);
        await brotli.FlushAsync(ct).ConfigureAwait(false);
        return output.ToArray();
    }

    // ── Brotli decompress ─────────────────────────────────────────────────────
    public static async Task<byte[]> BrotliDecompressAsync(
        byte[] compressed, CancellationToken ct = default)
    {
        using var input       = new MemoryStream(compressed);
        await using var brotli = new BrotliStream(input, CompressionMode.Decompress);
        using var output      = new MemoryStream();
        await brotli.CopyToAsync(output, ct).ConfigureAwait(false);
        return output.ToArray();
    }

    // ── Stream chaining helper (compose decorator chain) ─────────────────────
    // Compress text directly to a target stream without intermediate buffers.
    public static async Task CompressTextToStreamAsync(
        string text, Stream target,
        CompressionLevel level = CompressionLevel.Optimal,
        CancellationToken ct = default)
    {
        await using var gzip   = new GZipStream(target, level, leaveOpen: true);
        await using var writer = new StreamWriter(gzip, System.Text.Encoding.UTF8,
                                                  bufferSize: 4096, leaveOpen: true);
        await writer.WriteAsync(text.AsMemory(), ct).ConfigureAwait(false);
    }
}
