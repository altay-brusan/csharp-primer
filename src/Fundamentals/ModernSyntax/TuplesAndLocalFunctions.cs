namespace Fundamentals.ModernSyntax;

/// <summary>
/// Demonstrates C# tuples (ValueTuple), deconstruction, and local functions.
///
/// Tuples provide lightweight, unnamed aggregate types for multi-value returns
/// without introducing a named type. Deconstruction unpacks them into named variables.
/// Local functions are methods defined inside another method; they can close over locals.
/// </summary>
public static class TuplesAndLocalFunctions
{
    // ── Named tuple return ─────────────────────────────────────────────────────
    public static (double Min, double Max, double Mean) Statistics(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
            throw new ArgumentException("Collection must not be empty.", nameof(values));

        double min  = values[0], max = values[0], sum = 0;
        foreach (var v in values)
        {
            if (v < min) min = v;
            if (v > max) max = v;
            sum += v;
        }
        return (min, max, sum / values.Count);
    }

    // ── Deconstruction ────────────────────────────────────────────────────────
    public static string SummarisePair(int a, int b)
    {
        var (small, large) = a < b ? (a, b) : (b, a);   // tuple + deconstruct
        return $"Smaller={small}, Larger={large}, Sum={small + large}";
    }

    // ── Tuple equality ────────────────────────────────────────────────────────
    public static bool IsOrigin((int X, int Y) point) => point == (0, 0);

    // ── Local functions ───────────────────────────────────────────────────────
    // Local functions keep helper logic close to the code that uses it,
    // avoid polluting the class API, and can access the enclosing method's variables.
    public static IReadOnlyList<int> GetFibonacci(int count)
    {
        if (count <= 0) return [];

        var results = new List<int>(count);
        for (int i = 0; i < count; i++)
            results.Add(Fib(i));
        return results;

        // Local recursive function — not visible outside GetFibonacci.
        static int Fib(int n) => n <= 1 ? n : Fib(n - 1) + Fib(n - 2);
    }

    // ── Local functions that capture variables ────────────────────────────────
    public static Func<int, int> MakeAdder(int addend)
    {
        return Add;   // local function used as a delegate

        int Add(int x) => x + addend;   // captures addend from enclosing scope
    }

    // ── Nested deconstruction ─────────────────────────────────────────────────
    public record Point(int X, int Y);

    public static string DescribeDistance(Point from, Point to)
    {
        var (x1, y1) = from;
        var (x2, y2) = to;
        double dist  = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        return $"Distance from {from} to {to} = {dist:F2}";
    }
}
