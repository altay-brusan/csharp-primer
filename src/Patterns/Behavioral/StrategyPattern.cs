namespace Patterns.Behavioral;

/// <summary>
/// Demonstrates the Strategy pattern — define a family of algorithms,
/// encapsulate each one, and make them interchangeable at runtime.
///
/// Modern C# implementations prefer delegates / Func&lt;T, TResult&gt; over heavyweight
/// interface hierarchies for simple strategies. Use interfaces when strategies
/// have multiple methods or need to carry state.
/// </summary>
public static class StrategyPattern
{
    // ── Interface-based strategy ──────────────────────────────────────────────
    public interface ISortStrategy<T>
    {
        void Sort(List<T> items);
    }

    public class AscendingSort<T> : ISortStrategy<T> where T : IComparable<T>
    {
        public void Sort(List<T> items) => items.Sort();
    }

    public class DescendingSort<T> : ISortStrategy<T> where T : IComparable<T>
    {
        public void Sort(List<T> items) => items.Sort((a, b) => b.CompareTo(a));
    }

    public class Sorter<T>
    {
        private ISortStrategy<T> _strategy;
        public Sorter(ISortStrategy<T> strategy) => _strategy = strategy;
        public void SetStrategy(ISortStrategy<T> strategy) => _strategy = strategy;
        public void Sort(List<T> items) => _strategy.Sort(items);
    }

    // ── Delegate-based strategy (lighter weight) ──────────────────────────────
    // A simple Func captures the strategy without a class hierarchy.
    public class OrderProcessor
    {
        private readonly Func<decimal, decimal> _discountStrategy;
        private readonly Func<decimal, decimal> _taxStrategy;

        public OrderProcessor(
            Func<decimal, decimal> discountStrategy,
            Func<decimal, decimal> taxStrategy)
        {
            _discountStrategy = discountStrategy;
            _taxStrategy      = taxStrategy;
        }

        public decimal CalculateTotal(decimal subtotal)
        {
            decimal afterDiscount = subtotal - _discountStrategy(subtotal);
            return afterDiscount + _taxStrategy(afterDiscount);
        }
    }

    // Pre-defined strategies as static lambdas (zero allocation).
    public static readonly Func<decimal, decimal> NoDiscount   = _ => 0m;
    public static readonly Func<decimal, decimal> TenPercent   = amount => amount * 0.10m;
    public static readonly Func<decimal, decimal> FifteenPercent = amount => amount * 0.15m;

    public static readonly Func<decimal, decimal> UsTax  = amount => amount * 0.08m;
    public static readonly Func<decimal, decimal> EuVat  = amount => amount * 0.21m;
    public static readonly Func<decimal, decimal> NoTax  = _ => 0m;

    // ── Composable strategy (chain of responsibility) ─────────────────────────
    // Multiple strategies can be combined with function composition.
    public static Func<decimal, decimal> Combine(params Func<decimal, decimal>[] strategies) =>
        amount => strategies.Sum(s => s(amount));
}
