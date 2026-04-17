namespace Patterns.Structural;

/// <summary>
/// Demonstrates the Repository pattern — an abstraction layer between the domain model
/// and the data source.
///
/// Benefits:
/// • Domain code depends on the interface, not on EF Core, Dapper, or any ORM.
/// • The interface can be replaced with an in-memory fake in tests.
/// • Queries are expressed in domain terms, not in raw SQL / LINQ-to-SQL.
///
/// Pair with the Unit-of-Work pattern when you need transactional consistency.
/// </summary>
public static class RepositoryPattern
{
    // ── Domain entity ─────────────────────────────────────────────────────────
    public record Product(int Id, string Name, decimal Price, string Category)
    {
        public static Product Create(int id, string name, decimal price, string category)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentOutOfRangeException.ThrowIfNegative(price);
            return new Product(id, name, price, category);
        }
    }

    // ── Repository interface ──────────────────────────────────────────────────
    public interface IProductRepository
    {
        Task<Product?>             GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> GetByCategoryAsync(string category, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(Product product, CancellationToken ct = default);
        Task UpdateAsync(Product product, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }

    // ── In-memory implementation (also serves as a test double) ───────────────
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly Dictionary<int, Product> _store;

        public InMemoryProductRepository(IEnumerable<Product>? seed = null)
        {
            _store = seed?.ToDictionary(p => p.Id) ?? [];
        }

        public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(_store.GetValueOrDefault(id));

        public Task<IReadOnlyList<Product>> GetByCategoryAsync(
            string category, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Product>>(
                _store.Values
                      .Where(p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase))
                      .ToList());

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Product>>([.. _store.Values]);

        public Task AddAsync(Product product, CancellationToken ct = default)
        {
            if (_store.ContainsKey(product.Id))
                throw new InvalidOperationException($"Product {product.Id} already exists.");
            _store[product.Id] = product;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            if (!_store.ContainsKey(product.Id))
                throw new KeyNotFoundException($"Product {product.Id} not found.");
            _store[product.Id] = product;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id, CancellationToken ct = default)
        {
            _store.Remove(id);
            return Task.CompletedTask;
        }
    }

    // ── Service that uses the repository ─────────────────────────────────────
    public class ProductService(IProductRepository repository)
    {
        public async Task<IReadOnlyList<Product>> GetAffordableAsync(
            string category, decimal maxPrice, CancellationToken ct = default)
        {
            var products = await repository.GetByCategoryAsync(category, ct).ConfigureAwait(false);
            return products.Where(p => p.Price <= maxPrice).OrderBy(p => p.Price).ToList();
        }

        public async Task<Product> UpdatePriceAsync(
            int id, decimal newPrice, CancellationToken ct = default)
        {
            var product = await repository.GetByIdAsync(id, ct).ConfigureAwait(false)
                ?? throw new KeyNotFoundException($"Product {id} not found.");
            var updated = product with { Price = newPrice };
            await repository.UpdateAsync(updated, ct).ConfigureAwait(false);
            return updated;
        }
    }
}
