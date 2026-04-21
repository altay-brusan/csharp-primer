namespace Fundamentals.PatternMatching;

/// <summary>
/// Demonstrates switch expressions and exhaustive pattern matching introduced in C# 8–12.
///
/// Pattern matching lets you branch on the shape and content of data without casting.
/// The switch expression is a concise, expression-form alternative to switch statements.
/// </summary>
public static class SwitchExpressions
{
    // ── Type patterns ─────────────────────────────────────────────────────────
    public abstract record Shape;
    public record Circle(double Radius)       : Shape;
    public record Rectangle(double W, double H) : Shape;
    public record Triangle(double Base, double Height) : Shape;

    // Type pattern + property subpattern in a switch expression.
    public static double Area(Shape shape) => shape switch
    {
        Circle   { Radius: var r }  => Math.PI * r * r,
        Rectangle { W: var w, H: var h } => w * h,
        Triangle { Base: var b, Height: var h } => 0.5 * b * h,
        _ => throw new ArgumentOutOfRangeException(nameof(shape))
    };

    // ── Relational and logical patterns (C# 9) ────────────────────────────────
    public enum RiskLevel { Low, Medium, High, Critical }

    public static RiskLevel ClassifyCvssScore(double score) => score switch
    {
        < 0 or > 10       => throw new ArgumentOutOfRangeException(nameof(score)),
        0                 => RiskLevel.Low,
        > 0 and < 4       => RiskLevel.Low,
        >= 4 and < 7      => RiskLevel.Medium,
        >= 7 and < 9      => RiskLevel.High,
        _                 => RiskLevel.Critical   // 9–10
    };

    // ── Property patterns ─────────────────────────────────────────────────────
    public record Order(decimal Total, bool IsPremiumCustomer, string Region);

    public static decimal CalculateDiscount(Order order) => order switch
    {
        { IsPremiumCustomer: true,  Total: > 500 } => order.Total * 0.15m,
        { IsPremiumCustomer: true  }               => order.Total * 0.10m,
        { Region: "EU", Total: > 200 }             => order.Total * 0.05m,
        _                                          => 0m
    };

    // ── Tuple patterns ────────────────────────────────────────────────────────
    // Match multiple values simultaneously.
    public enum Season { Spring, Summer, Autumn, Winter }

    public static string Describe(Season season, bool isRaining) => (season, isRaining) switch
    {
        (Season.Spring, false) => "Warm and sunny",
        (Season.Spring, true)  => "April showers",
        (Season.Summer, false) => "Hot and dry",
        (Season.Summer, true)  => "Tropical storm",
        (Season.Autumn, _)     => isRaining ? "Misty autumn" : "Golden leaves",
        (Season.Winter, _)     => "Cold",
        _                      => throw new ArgumentOutOfRangeException(nameof(season))
    };

    // ── List patterns (C# 11) ─────────────────────────────────────────────────
    // Match on the structure of a sequence.
    public static string DescribeSequence(int[] items) => items switch
    {
        []           => "empty",
        [var single] => $"single: {single}",
        [var a, var b] => $"pair: {a} and {b}",
        [var head, .., var tail] => $"starts with {head}, ends with {tail}",
    };
}
