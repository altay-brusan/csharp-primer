namespace Fundamentals.PatternMatching;

/// <summary>
/// Demonstrates is-expression patterns and deconstruction patterns.
///
/// The `is` pattern is useful when you need a conditional check in an if-statement
/// rather than a full switch expression.
/// </summary>
public static class TypePatterns
{
    // ── is type pattern with variable binding ─────────────────────────────────
    // Combine a type check and a variable assignment in a single expression.
    public static string Describe(object obj) =>
        obj is int    n ? $"integer: {n}" :
        obj is string s ? $"string of length {s.Length}" :
        obj is bool   b ? $"boolean: {b}" :
        obj is null     ? "null" :
                          $"unknown: {obj.GetType().Name}";

    // ── Negation pattern ──────────────────────────────────────────────────────
    public static void ProcessIfNotNull(string? value)
    {
        if (value is not null)
        {
            Console.WriteLine(value.ToUpper());
        }
    }

    // ── Combined type + property pattern in if ────────────────────────────────
    public record Product(string Name, decimal Price, bool InStock);

    public static bool IsAffordableAndAvailable(object item, decimal budget) =>
        item is Product { InStock: true, Price: var p } && p <= budget;

    // ── Deconstruction pattern ────────────────────────────────────────────────
    // Works with any type that exposes a Deconstruct method (including tuples and records).
    public static string FormatCoordinate((double Lat, double Lon) coordinate)
    {
        var (lat, lon) = coordinate;
        return $"{Math.Abs(lat):F4}°{(lat >= 0 ? 'N' : 'S')}, " +
               $"{Math.Abs(lon):F4}°{(lon >= 0 ? 'E' : 'W')}";
    }

    // ── var pattern ───────────────────────────────────────────────────────────
    // `var p` always matches and captures any value (including null).
    // Useful to bind the intermediate result of a larger expression.
    public static bool IsInRange(object? obj)
    {
        if (obj is var value && value is int n)
            return n is >= 0 and <= 100;
        return false;
    }

    // ── Recursive / nested property patterns ─────────────────────────────────
    public record Address(string Country, string City);
    public record Customer(string Name, Address? Address);

    public static bool IsEuropeanCustomer(Customer customer) =>
        customer is { Address: { Country: "DE" or "FR" or "ES" or "IT" or "NL" } };
}
