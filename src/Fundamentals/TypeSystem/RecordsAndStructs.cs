namespace Fundamentals.TypeSystem;

/// <summary>
/// Demonstrates records and record structs — concise, value-semantic data carriers.
///
/// Records are reference types with compiler-synthesised equality, hash code, ToString,
/// and a with-expression for non-destructive mutation.
/// Record structs add the same features to value types.
/// </summary>
public static class RecordsAndStructs
{
    // ── Positional record (reference type) ───────────────────────────────────
    // The compiler generates a primary constructor, public init properties, Equals,
    // GetHashCode, ToString, and Deconstruct.
    public record Person(string FirstName, string LastName, DateOnly BirthDate)
    {
        // Computed property added to a positional record.
        public int Age =>
            DateOnly.FromDateTime(DateTime.Today).Year - BirthDate.Year -
            (DateOnly.FromDateTime(DateTime.Today).DayOfYear < BirthDate.DayOfYear ? 1 : 0);

        public string FullName => $"{FirstName} {LastName}";
    }

    // ── With-expressions (non-destructive mutation) ───────────────────────────
    // Records are immutable by default. `with` creates a copy with specific changes.
    public static Person Rename(Person person, string newLastName) =>
        person with { LastName = newLastName };

    // ── Positional record struct (value type) ────────────────────────────────
    // Prefer readonly record struct for small, immutable value objects.
    public readonly record struct Temperature(double Celsius)
    {
        public double Fahrenheit => Celsius * 9 / 5 + 32;
        public double Kelvin     => Celsius + 273.15;

        public static Temperature FromFahrenheit(double f) => new((f - 32) * 5 / 9);
        public static Temperature Boiling => new(100);
        public static Temperature Freezing => new(0);
    }

    // ── Record inheritance ────────────────────────────────────────────────────
    // Records support inheritance. Derived records extend base record equality.
    public abstract record Shape(string Color);
    public record Circle(string Color, double Radius) : Shape(Color)
    {
        public double Area => Math.PI * Radius * Radius;
    }
    public record Rectangle(string Color, double Width, double Height) : Shape(Color)
    {
        public double Area => Width * Height;
    }

    // ── Deconstruction ────────────────────────────────────────────────────────
    // Positional records support tuple-like deconstruction out of the box.
    public static (string first, string last) DeconstructPerson(Person person)
    {
        var (first, last, _) = person;   // uses compiler-generated Deconstruct
        return (first, last);
    }
}
