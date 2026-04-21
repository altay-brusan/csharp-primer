namespace Streams.FileIO;

/// <summary>
/// Demonstrates idiomatic file I/O patterns with System.IO.
///
/// Key practices:
/// • Always use async overloads (ReadAllTextAsync, WriteAllTextAsync, etc.) to avoid
///   blocking the thread pool.
/// • Wrap streams in using declarations so Dispose is called on every exit path.
/// • Pass CancellationToken through every I/O call.
/// • Use FileOptions.Asynchronous when opening a FileStream for async use.
/// • Prefer File.* helpers for simple cases; use FileStream directly for large files.
/// </summary>
public static class FileStreams
{
    // ── Read a whole text file ─────────────────────────────────────────────────
    public static Task<string> ReadTextAsync(string path, CancellationToken ct = default) =>
        File.ReadAllTextAsync(path, ct);

    // ── Write a whole text file ────────────────────────────────────────────────
    public static Task WriteTextAsync(string path, string content, CancellationToken ct = default) =>
        File.WriteAllTextAsync(path, content, ct);

    // ── Append a line ─────────────────────────────────────────────────────────
    public static Task AppendLineAsync(string path, string line, CancellationToken ct = default) =>
        File.AppendAllTextAsync(path, line + Environment.NewLine, ct);

    // ── Stream large files line by line ───────────────────────────────────────
    // Reading everything at once (ReadAllTextAsync) is fine for small files.
    // For large files, stream line-by-line to keep memory usage flat.
    public static async IAsyncEnumerable<string> ReadLinesAsync(
        string path,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        using var reader = new StreamReader(
            new FileStream(path, FileMode.Open, FileAccess.Read,
                           FileShare.Read, bufferSize: 4096,
                           FileOptions.Asynchronous | FileOptions.SequentialScan));

        string? line;
        while ((line = await reader.ReadLineAsync(ct).ConfigureAwait(false)) is not null)
            yield return line;
    }

    // ── Copy a file using streams (efficient, large-file safe) ────────────────
    public static async Task CopyFileAsync(
        string sourcePath, string destPath, CancellationToken ct = default)
    {
        await using var src = new FileStream(
            sourcePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);

        await using var dst = new FileStream(
            destPath, FileMode.Create, FileAccess.Write,
            FileShare.None, 81920, FileOptions.Asynchronous);

        await src.CopyToAsync(dst, ct).ConfigureAwait(false);
    }

    // ── Read binary data ───────────────────────────────────────────────────────
    public static async Task<byte[]> ReadBinaryAsync(string path, CancellationToken ct = default)
    {
        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read,
            FileShare.Read, 4096, FileOptions.Asynchronous);

        var buffer = new byte[stream.Length];
        await stream.ReadExactlyAsync(buffer, ct).ConfigureAwait(false);
        return buffer;
    }

    // ── Write binary data ──────────────────────────────────────────────────────
    public static async Task WriteBinaryAsync(
        string path, ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        await using var stream = new FileStream(
            path, FileMode.Create, FileAccess.Write,
            FileShare.None, 4096, FileOptions.Asynchronous);

        await stream.WriteAsync(data, ct).ConfigureAwait(false);
    }
}
