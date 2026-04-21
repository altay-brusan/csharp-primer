namespace Patterns.Functional;

/// <summary>
/// Demonstrates the Specification pattern — encapsulate a business rule as an object
/// that can be combined, reused, and tested in isolation.
///
/// Specifications:
/// • Express "does this entity satisfy this criterion?"
/// • Can be combined with And, Or, Not for composite rules.
/// • Are reusable across query layers (in-memory LINQ, EF Core predicate, etc.).
/// </summary>
public abstract class Specification<T>
{
    public abstract bool IsSatisfiedBy(T candidate);

    // ── Combinators ───────────────────────────────────────────────────────────
    public Specification<T> And(Specification<T> other)  => new AndSpecification<T>(this, other);
    public Specification<T> Or(Specification<T> other)   => new OrSpecification<T>(this, other);
    public Specification<T> Not()                        => new NotSpecification<T>(this);

    // Operator overloads for syntactic sugar: spec1 && spec2, spec1 || spec2, !spec
    public static Specification<T> operator &(Specification<T> l, Specification<T> r) => l.And(r);
    public static Specification<T> operator |(Specification<T> l, Specification<T> r) => l.Or(r);
    public static Specification<T> operator !(Specification<T> s) => s.Not();
}

file sealed class AndSpecification<T>(Specification<T> left, Specification<T> right)
    : Specification<T>
{
    public override bool IsSatisfiedBy(T candidate) =>
        left.IsSatisfiedBy(candidate) && right.IsSatisfiedBy(candidate);
}

file sealed class OrSpecification<T>(Specification<T> left, Specification<T> right)
    : Specification<T>
{
    public override bool IsSatisfiedBy(T candidate) =>
        left.IsSatisfiedBy(candidate) || right.IsSatisfiedBy(candidate);
}

file sealed class NotSpecification<T>(Specification<T> inner) : Specification<T>
{
    public override bool IsSatisfiedBy(T candidate) => !inner.IsSatisfiedBy(candidate);
}

/// <summary>
/// Concrete specifications for a product catalogue domain.
/// </summary>
public static class ProductSpecifications
{
    public record Product(string Name, decimal Price, string Category, bool InStock, int StockLevel);

    // ── Leaf specifications ───────────────────────────────────────────────────
    public class InStockSpec : Specification<Product>
    {
        public override bool IsSatisfiedBy(Product p) => p.InStock && p.StockLevel > 0;
    }

    public class PriceRangeSpec(decimal min, decimal max) : Specification<Product>
    {
        public override bool IsSatisfiedBy(Product p) => p.Price >= min && p.Price <= max;
    }

    public class CategorySpec(string category) : Specification<Product>
    {
        public override bool IsSatisfiedBy(Product p) =>
            string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase);
    }

    // ── Filtering with specifications ─────────────────────────────────────────
    public static IReadOnlyList<Product> Filter(
        IEnumerable<Product> products, Specification<Product> spec) =>
        products.Where(spec.IsSatisfiedBy).ToList();

    // ── Example: combine specs ────────────────────────────────────────────────
    public static IReadOnlyList<Product> FindAffordableElectronicsInStock(
        IEnumerable<Product> catalogue) =>
        Filter(catalogue,
            new InStockSpec() &
            new CategorySpec("Electronics") &
            new PriceRangeSpec(0, 500));
}
