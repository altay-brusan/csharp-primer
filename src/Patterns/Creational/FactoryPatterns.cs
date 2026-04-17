namespace Patterns.Creational;

/// <summary>
/// Demonstrates the Factory Method and Abstract Factory patterns.
///
/// Factory Method: a virtual method that subclasses override to create objects.
/// Abstract Factory: an interface for creating families of related objects
///   without specifying their concrete classes.
///
/// Prefer factories when:
/// • Construction logic is non-trivial.
/// • You need to return different subtypes based on parameters.
/// • You want to centralise creation, enabling DI or mocking.
/// </summary>
public static class FactoryPatterns
{
    // ── Product hierarchy ─────────────────────────────────────────────────────
    public interface INotification
    {
        string Channel   { get; }
        Task SendAsync(string recipient, string message, CancellationToken ct = default);
    }

    public class EmailNotification : INotification
    {
        public string Channel => "Email";
        public Task SendAsync(string recipient, string message, CancellationToken ct = default)
        {
            Console.WriteLine($"[Email → {recipient}]: {message}");
            return Task.CompletedTask;
        }
    }

    public class SmsNotification : INotification
    {
        public string Channel => "SMS";
        public Task SendAsync(string recipient, string message, CancellationToken ct = default)
        {
            Console.WriteLine($"[SMS → {recipient}]: {message}");
            return Task.CompletedTask;
        }
    }

    public class PushNotification : INotification
    {
        public string Channel => "Push";
        public Task SendAsync(string recipient, string message, CancellationToken ct = default)
        {
            Console.WriteLine($"[Push → {recipient}]: {message}");
            return Task.CompletedTask;
        }
    }

    // ── Factory Method ────────────────────────────────────────────────────────
    // A static factory method selects and constructs the right product.
    public static INotification CreateNotification(string channel) => channel switch
    {
        "email" or "Email" => new EmailNotification(),
        "sms"   or "SMS"   => new SmsNotification(),
        "push"  or "Push"  => new PushNotification(),
        _ => throw new ArgumentException($"Unknown channel: {channel}", nameof(channel))
    };

    // ── Abstract Factory ──────────────────────────────────────────────────────
    // Produces families of related objects. Switch out the whole factory to change behaviour.
    public interface IUiFactory
    {
        IButton    CreateButton(string label);
        ITextInput CreateTextInput(string placeholder);
    }

    public interface IButton    { string Render(); }
    public interface ITextInput { string Render(); }

    // Windows family
    public record WindowsButton(string Label)    : IButton    { public string Render() => $"<win:Button>{Label}</win:Button>"; }
    public record WindowsTextInput(string Hint)  : ITextInput { public string Render() => $"<win:TextBox hint='{Hint}'/>"; }

    public class WindowsUiFactory : IUiFactory
    {
        public IButton    CreateButton(string label)       => new WindowsButton(label);
        public ITextInput CreateTextInput(string placeholder) => new WindowsTextInput(placeholder);
    }

    // Web family
    public record WebButton(string Label)    : IButton    { public string Render() => $"<button>{Label}</button>"; }
    public record WebTextInput(string Hint)  : ITextInput { public string Render() => $"<input placeholder='{Hint}'/>"; }

    public class WebUiFactory : IUiFactory
    {
        public IButton    CreateButton(string label)       => new WebButton(label);
        public ITextInput CreateTextInput(string placeholder) => new WebTextInput(placeholder);
    }

    // Client code is decoupled from concrete widget classes.
    public static string RenderLoginForm(IUiFactory factory)
    {
        var btn   = factory.CreateButton("Login");
        var input = factory.CreateTextInput("Username");
        return $"{input.Render()}\n{btn.Render()}";
    }
}
