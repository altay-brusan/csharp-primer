namespace Patterns.Structural;

/// <summary>
/// Demonstrates the Decorator pattern — add behaviour to objects dynamically
/// by wrapping them in decorator objects that share the same interface.
///
/// Prefer composition over inheritance. Decorators are open/closed:
/// add new functionality without modifying existing code.
/// </summary>
public static class DecoratorPattern
{
    // ── Component interface ───────────────────────────────────────────────────
    public interface IMessageService
    {
        Task<string> SendAsync(string recipient, string message, CancellationToken ct = default);
    }

    // ── Concrete component ────────────────────────────────────────────────────
    public class SmtpMessageService : IMessageService
    {
        public Task<string> SendAsync(string recipient, string message, CancellationToken ct = default)
        {
            string result = $"SMTP: sent '{message}' to {recipient}";
            return Task.FromResult(result);
        }
    }

    // ── Base decorator ────────────────────────────────────────────────────────
    // Holds a reference to the wrapped component and delegates by default.
    public abstract class MessageServiceDecorator(IMessageService inner) : IMessageService
    {
        protected readonly IMessageService Inner = inner;

        public virtual Task<string> SendAsync(string recipient, string message, CancellationToken ct = default) =>
            Inner.SendAsync(recipient, message, ct);
    }

    // ── Logging decorator ─────────────────────────────────────────────────────
    public class LoggingMessageService(IMessageService inner) : MessageServiceDecorator(inner)
    {
        private readonly List<string> _log = [];
        public IReadOnlyList<string> Log => _log;

        public override async Task<string> SendAsync(
            string recipient, string message, CancellationToken ct = default)
        {
            _log.Add($"[LOG] Sending to {recipient}");
            string result = await Inner.SendAsync(recipient, message, ct).ConfigureAwait(false);
            _log.Add($"[LOG] Sent: {result}");
            return result;
        }
    }

    // ── Retry decorator ───────────────────────────────────────────────────────
    public class RetryMessageService(IMessageService inner, int maxRetries = 3)
        : MessageServiceDecorator(inner)
    {
        public override async Task<string> SendAsync(
            string recipient, string message, CancellationToken ct = default)
        {
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    return await Inner.SendAsync(recipient, message, ct).ConfigureAwait(false);
                }
                catch when (attempt < maxRetries && !ct.IsCancellationRequested)
                {
                    await Task.Delay(50 * attempt, ct).ConfigureAwait(false);
                }
            }
        }
    }

    // ── Composing decorators ──────────────────────────────────────────────────
    // Order matters: outer decorator executes first.
    // SmtpMessageService → RetryMessageService → LoggingMessageService
    public static IMessageService BuildService() =>
        new LoggingMessageService(
            new RetryMessageService(
                new SmtpMessageService()));
}
