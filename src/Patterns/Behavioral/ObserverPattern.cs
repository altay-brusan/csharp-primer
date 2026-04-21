namespace Patterns.Behavioral;

/// <summary>
/// Demonstrates the Observer pattern using C# events, IObservable&lt;T&gt;/IObserver&lt;T&gt;,
/// and a simple event bus.
///
/// Events and delegates are C#'s built-in observer mechanism.
/// IObservable/IObserver is the BCL contract used by Reactive Extensions (Rx.NET).
/// A custom event bus decouples publishers from subscribers without shared references.
/// </summary>
public static class ObserverPattern
{
    // ── Event-based observer (idiomatic C#) ───────────────────────────────────
    public class StockTicker
    {
        // EventArgs carries the event data.
        public class PriceChangedArgs(string symbol, decimal oldPrice, decimal newPrice)
            : EventArgs
        {
            public string  Symbol   { get; } = symbol;
            public decimal OldPrice { get; } = oldPrice;
            public decimal NewPrice { get; } = newPrice;
            public decimal Change   => NewPrice - OldPrice;
        }

        // Use EventHandler<TEventArgs> for strongly typed events.
        public event EventHandler<PriceChangedArgs>? PriceChanged;

        private decimal _price;
        public string Symbol { get; }

        public StockTicker(string symbol, decimal initialPrice)
        {
            Symbol = symbol;
            _price = initialPrice;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice == _price) return;
            var args = new PriceChangedArgs(Symbol, _price, newPrice);
            _price = newPrice;
            PriceChanged?.Invoke(this, args);  // null-conditional invocation is thread-safe read
        }
    }

    // ── IObservable / IObserver ───────────────────────────────────────────────
    // The BCL pattern for push-based notification sequences.
    public class SimpleObservable<T>(IEnumerable<T> sequence) : IObservable<T>
    {
        public IDisposable Subscribe(IObserver<T> observer)
        {
            foreach (var item in sequence)
                observer.OnNext(item);
            observer.OnCompleted();
            return NoopDisposable.Instance;
        }
    }

    // Minimal no-op IDisposable for subscriptions that need no teardown.
    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();
        public void Dispose() { }
    }

    public class PrintingObserver<T> : IObserver<T>
    {
        private readonly List<T> _received = [];
        public IReadOnlyList<T> Received => _received;

        public void OnNext(T value)      => _received.Add(value);
        public void OnError(Exception e) => Console.WriteLine($"Error: {e.Message}");
        public void OnCompleted()        => Console.WriteLine("Sequence completed.");
    }

    // ── Lightweight in-process event bus ─────────────────────────────────────
    // Subscribers register a handler for a specific message type.
    // Decouples publishers and subscribers without a shared interface.
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = [];

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var key = typeof(TEvent);
            if (!_handlers.TryGetValue(key, out var list))
                _handlers[key] = list = [];
            list.Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var list))
                list.Remove(handler);
        }

        public void Publish<TEvent>(TEvent @event)
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out var list)) return;
            foreach (var handler in list.ToList()) // copy to allow unsubscribe during publish
                ((Action<TEvent>)handler)(@event);
        }
    }
}
