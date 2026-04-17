namespace Collections.Advanced;

/// <summary>
/// Demonstrates Span&lt;T&gt;, ReadOnlySpan&lt;T&gt;, and Memory&lt;T&gt; for high-performance,
/// allocation-free slicing of arrays, strings, and stack-allocated buffers.
///
/// Span&lt;T&gt;:
/// • Stack-only (ref struct) — cannot be stored in class fields or captured by lambdas.
/// • Provides a window into contiguous memory (array, stack allocation, or unmanaged).
///
/// Memory&lt;T&gt;:
/// • Heap-friendly wrapper — can be stored in fields and used with async.
/// • Obtain a Span via memory.Span when inside a synchronous method.
/// </summary>
public static class SpanAndMemoryExamples
{
    // ── Slice a string without allocating a substring ─────────────────────────
    public static int CountDigits(ReadOnlySpan<char> text)
    {
        int count = 0;
        foreach (char c in text)
            if (char.IsDigit(c)) count++;
        return count;
    }

    // Caller passes a slice — no heap allocation.
    public static int CountDigitsInSlice(string text, int start, int length) =>
        CountDigits(text.AsSpan(start, length));

    // ── Stack-allocated buffer with stackalloc ────────────────────────────────
    // stackalloc avoids a heap allocation for small, short-lived buffers.
    public static string ReverseSmallString(string input)
    {
        if (input.Length > 256)
            return new string(input.Reverse().ToArray()); // fall back for large strings

        Span<char> buffer = stackalloc char[input.Length];
        for (int i = 0; i < input.Length; i++)
            buffer[i] = input[input.Length - 1 - i];
        return new string(buffer);
    }

    // ── Parsing structured binary / text without allocation ───────────────────
    // Parse CSV line into fields without creating intermediate string objects.
    public static int ParseCsvFieldCount(ReadOnlySpan<char> line)
    {
        if (line.IsEmpty) return 0;
        int count = 1;
        foreach (char c in line)
            if (c == ',') count++;
        return count;
    }

    // ── Memory<T> for async contexts ──────────────────────────────────────────
    // Store a slice as Memory so it can cross an await boundary.
    public static async Task<int> SumSegmentAsync(
        Memory<int> segment, CancellationToken ct = default)
    {
        await Task.Yield(); // simulate async boundary
        int sum = 0;
        foreach (int v in segment.Span)
            sum += v;
        return sum;
    }

    // ── MemoryMarshal: reinterpret byte spans ─────────────────────────────────
    // Read structured data from raw bytes (e.g., binary file headers) safely.
    public static (int low, int high) SplitInt64(long value)
    {
        Span<byte> bytes = stackalloc byte[8];
        System.Runtime.InteropServices.MemoryMarshal.Write(bytes, in value);
        int low  = System.Runtime.InteropServices.MemoryMarshal.Read<int>(bytes[..4]);
        int high = System.Runtime.InteropServices.MemoryMarshal.Read<int>(bytes[4..]);
        return (low, high);
    }
}
