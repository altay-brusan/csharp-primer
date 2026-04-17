using System.Numerics;

namespace Fundamentals.Generics;

/// <summary>
/// Demonstrates generic constraints and covariance / contravariance.
///
/// Constraints tell the compiler what you guarantee about a type parameter,
/// enabling you to call methods or use features that would otherwise be unavailable.
/// </summary>
public static class GenericConstraints
{
    // ── where T : struct  (value type) ────────────────────────────────────────
    public static T? TryParse<T>(string input) where T : struct, IParsable<T> =>
        T.TryParse(input, null, out T value) ? value : null;

    // ── where T : class, new()  (reference type + default constructor) ────────
    public static T CreateAndConfigure<T>(Action<T> configure) where T : class, new()
    {
        var instance = new T();
        configure(instance);
        return instance;
    }

    // ── where T : IComparable<T> ──────────────────────────────────────────────
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0) return min;
        if (value.CompareTo(max) > 0) return max;
        return value;
    }

    // ── Multiple constraints ───────────────────────────────────────────────────
    public interface IEntity { int Id { get; } }

    public static TEntity? FindById<TEntity>(IEnumerable<TEntity> source, int id)
        where TEntity : class, IEntity =>
        source.FirstOrDefault(e => e.Id == id);

    // ── Generic method with static abstract interface member (C# 11) ──────────
    // Allows arithmetic-style operations without boxing.
    public static T Sum<T>(IEnumerable<T> values) where T : INumber<T> =>
        values.Aggregate(T.Zero, (acc, v) => acc + v);

    // ── Generic repository pattern with constraints ──────────────────────────
    public interface IRepository<T> where T : class, IEntity
    {
        T?           GetById(int id);
        IList<T>     GetAll();
        void         Add(T entity);
        void         Remove(T entity);
    }

    public class InMemoryRepository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly Dictionary<int, T> _store = [];

        public T?       GetById(int id) => _store.GetValueOrDefault(id);
        public IList<T> GetAll()        => [.. _store.Values];
        public void     Add(T entity)   => _store[entity.Id] = entity;
        public void     Remove(T entity)=> _store.Remove(entity.Id);
    }
}
