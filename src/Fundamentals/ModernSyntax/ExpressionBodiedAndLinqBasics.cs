namespace Fundamentals.ModernSyntax;

/// <summary>
/// Demonstrates LINQ query syntax, method syntax, and expression-bodied members.
///
/// Expression-bodied members keep simple members concise while avoiding boilerplate.
/// LINQ queries should be lazy (deferred execution) unless you explicitly materialise
/// them with ToList / ToArray / ToDictionary etc.
/// </summary>
public static class ExpressionBodiedAndLinqBasics
{
    // ── Expression-bodied property, method, and indexer ───────────────────────
    public class Circle(double radius)
    {
        public double Radius     => radius;                     // read-only property
        public double Diameter   => radius * 2;
        public double Area       => Math.PI * radius * radius;
        public double Perimeter  => 2 * Math.PI * radius;
        public override string ToString() => $"Circle(r={radius:F2})";
    }

    // ── LINQ method syntax ────────────────────────────────────────────────────
    public static IReadOnlyList<string> TopThreeLongWords(IEnumerable<string> words) =>
        words
            .Where(w => w.Length > 4)
            .OrderByDescending(w => w.Length)
            .Take(3)
            .ToList();

    // ── LINQ query syntax ─────────────────────────────────────────────────────
    // Equivalent to the method-syntax version above — choose whichever is clearer.
    public static IReadOnlyList<string> TopThreeLongWordsQuery(IEnumerable<string> words) =>
        (from w in words
         where w.Length > 4
         orderby w.Length descending
         select w)
        .Take(3)
        .ToList();

    // ── Projection with anonymous types ──────────────────────────────────────
    public record Product(string Name, decimal Price, string Category);

    public static IReadOnlyList<string> SummariseProducts(IEnumerable<Product> products) =>
        products
            .Where(p => p.Price > 0)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(p => $"[{p.Category}] {p.Name} — ${p.Price:F2}")
            .ToList();

    // ── GroupBy and aggregation ───────────────────────────────────────────────
    public static IReadOnlyDictionary<string, decimal> TotalByCategory(
        IEnumerable<Product> products) =>
        products
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Price));

    // ── Any / All / Count ────────────────────────────────────────────────────
    public static (bool anyExpensive, bool allPositive, int discountedCount)
        QuickStats(IEnumerable<Product> products, decimal threshold) =>
    (
        products.Any(p => p.Price > threshold),
        products.All(p => p.Price >= 0),
        products.Count(p => p.Price < threshold)
    );
}
