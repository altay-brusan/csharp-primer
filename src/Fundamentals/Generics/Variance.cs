namespace Fundamentals.Generics;

/// <summary>
/// Demonstrates generic variance: covariance (out), contravariance (in),
/// and how IEnumerable&lt;T&gt; and Action&lt;T&gt; use these in practice.
///
/// Covariance: you can use a more-derived type where a base type is expected.
/// Contravariance: you can use a less-derived type where a more-derived is expected.
/// </summary>
public static class Variance
{
    // ── Covariant interface (out T) ───────────────────────────────────────────
    // T is only produced (returned), never consumed. Allows IProducer<Derived>
    // to be assigned to IProducer<Base>.
    public interface IProducer<out T>
    {
        T Produce();
        IEnumerable<T> ProduceMany(int count);
    }

    public class NumberProducer : IProducer<int>
    {
        private int _next;
        public int          Produce()              => _next++;
        public IEnumerable<int> ProduceMany(int n) => Enumerable.Range(_next, n);
    }

    // IEnumerable<T> is covariant, so IEnumerable<string> IS-A IEnumerable<object>.
    public static int CountDistinctChars(IEnumerable<object> items) =>
        items.Select(o => o.ToString()?.Length ?? 0).Distinct().Count();

    // ── Contravariant interface (in T) ────────────────────────────────────────
    // T is only consumed (accepted as parameter), never produced.
    // IComparer<Base> can be assigned to IComparer<Derived>.
    public interface IConsumer<in T>
    {
        void Consume(T item);
    }

    // Action<T> is contravariant. An Action<object> can serve as Action<string>.
    public static void ApplyToAll<T>(IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
            action(item);
    }

    // ── Practical example: sorting with covariant IComparer ───────────────────
    public abstract record Animal(string Name);
    public record Dog(string Name, string Breed) : Animal(Name);
    public record Cat(string Name, bool IsIndoor) : Animal(Name);

    // IComparer<Animal> can sort both Dog[] and Cat[] because IComparer is
    // contravariant in T (the concrete type parameter flows "in").
    public class ByNameComparer : IComparer<Animal>
    {
        public int Compare(Animal? x, Animal? y) =>
            string.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
    }

    public static IReadOnlyList<Dog> SortedDogs(IEnumerable<Dog> dogs)
    {
        var list = dogs.ToList();
        list.Sort(new ByNameComparer()); // IComparer<Animal> used for List<Dog>
        return list;
    }
}
