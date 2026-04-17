namespace Fundamentals.TypeSystem;

/// <summary>
/// Demonstrates C# nullable reference types (NRT) and nullable value types (Nullable&lt;T&gt;).
///
/// Enable NRT in the project file with &lt;Nullable&gt;enable&lt;/Nullable&gt;.
/// The compiler then distinguishes T (non-nullable) from T? (nullable) for reference types,
/// mirroring the long-standing Nullable&lt;T&gt; / T? distinction for value types.
/// </summary>
public static class NullableTypes
{
    // ── Nullable value type: Nullable<T> / T? ────────────────────────────────
    // int? is syntactic sugar for Nullable<int>. It has a .HasValue and .Value property.
    public static double AverageOrDefault(IEnumerable<int?> values, double defaultValue = 0)
    {
        var nonNull = values.Where(v => v.HasValue).Select(v => v!.Value).ToList();
        return nonNull.Count > 0 ? nonNull.Average() : defaultValue;
    }

    // Null-coalescing operator (??) returns the left side if non-null, otherwise right.
    public static int Coalesce(int? primary, int? secondary, int fallback) =>
        primary ?? secondary ?? fallback;

    // ── Nullable reference type ───────────────────────────────────────────────
    // string? signals the value may be null; the compiler issues warnings on unsafe use.
    public static string Greet(string? name) =>
        name is null ? "Hello, stranger!" : $"Hello, {name}!";

    // ── Null-conditional operator (?.) ────────────────────────────────────────
    // Returns null instead of throwing NullReferenceException when the operand is null.
    public static int? GetLength(string? text) => text?.Length;

    // ── Null-forgiving operator (!) ───────────────────────────────────────────
    // Tells the compiler "I know this is not null". Use sparingly and document why.
    public static string TrimTrusted(string? value)
    {
        // Contract: callers guarantee non-null in this path.
        return value!.Trim();
    }

    // ── Pattern matching with null ────────────────────────────────────────────
    public record Address(string Street, string? Apartment, string City);

    public static string FormatAddress(Address? address) => address switch
    {
        null                                    => "(no address)",
        { Apartment: null } a                  => $"{a.Street}, {a.City}",
        { Apartment: var apt } a               => $"{a.Street} #{apt}, {a.City}",
    };
}
