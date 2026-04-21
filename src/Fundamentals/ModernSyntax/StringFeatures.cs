namespace Fundamentals.ModernSyntax;

/// <summary>
/// Demonstrates modern C# string features:
/// string interpolation, verbatim strings, raw string literals (C# 11),
/// and string.Create for allocation-efficient formatting.
/// </summary>
public static class StringFeatures
{
    // ── Interpolated strings ──────────────────────────────────────────────────
    public static string FormatOrder(string customer, int quantity, decimal price) =>
        $"Order for {customer}: {quantity} × ${price:F2} = ${quantity * price:F2}";

    // ── Interpolated strings with format specifiers ───────────────────────────
    public static string FormatDate(DateTime dt) =>
        $"Date: {dt:yyyy-MM-dd}, Day: {dt:dddd}";

    // ── Verbatim strings (@"…") ───────────────────────────────────────────────
    // Backslashes are literal; newlines inside the string are preserved.
    public static string WindowsPath() =>
        @"C:\Users\Alice\Documents\report.pdf";

    // ── Raw string literals (C# 11, """ … """) ───────────────────────────────
    // No escaping needed. Leading whitespace matching the closing """ is trimmed.
    public static string JsonTemplate(string name, int age) =>
        $$"""
        {
            "name": "{{name}}",
            "age":  {{age}}
        }
        """;

    // ── String interpolation with conditional expressions ─────────────────────
    public static string PluraliseItem(int count, string noun) =>
        $"{count} {noun}{(count == 1 ? "" : "s")}";

    // ── Span<char>-based parsing (zero allocation) ───────────────────────────
    // ReadOnlySpan<char> lets you slice a string without allocating a substring.
    public static (string key, string value) ParseKeyValue(string line)
    {
        ReadOnlySpan<char> span = line.AsSpan();
        int eq = span.IndexOf('=');
        if (eq < 0) throw new FormatException("Expected 'key=value'");
        return (span[..eq].Trim().ToString(), span[(eq + 1)..].Trim().ToString());
    }

    // ── String.Create (efficient custom formatting) ───────────────────────────
    // Writes directly into a newly-allocated string's buffer.
    public static string ToSnakeCase(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase)) return camelCase;

        // Count transitions to pre-compute output length.
        int extras = 0;
        for (int i = 1; i < camelCase.Length; i++)
            if (char.IsUpper(camelCase[i])) extras++;
        return string.Create(camelCase.Length + extras, camelCase, (span, src) =>
        {
            int pos = 0;
            for (int i = 0; i < src.Length; i++)
            {
                if (char.IsUpper(src[i]) && i > 0)
                    span[pos++] = '_';
                span[pos++] = char.ToLowerInvariant(src[i]);
            }
        });
    }

    // ── Composite formatting and culture-sensitive output ─────────────────────
    public static string FormatCurrency(decimal amount, string cultureName)
    {
        var culture = new System.Globalization.CultureInfo(cultureName);
        return amount.ToString("C", culture);
    }
}
