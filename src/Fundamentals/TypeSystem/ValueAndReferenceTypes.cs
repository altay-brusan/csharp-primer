namespace Fundamentals.TypeSystem;

/// <summary>
/// Demonstrates the distinction between value types and reference types.
///
/// Value types (struct, int, bool, DateTime, etc.) are stored on the stack (or inline
/// in the containing object). Copies are independent. Reference types (class, string,
/// array, delegate) live on the heap; copying a variable copies the reference.
/// </summary>
public static class ValueAndReferenceTypes
{
    // ── Value type: struct ────────────────────────────────────────────────────
    // Structs should be small, immutable, and semantically represent a value.
    public readonly struct Point(double x, double y)
    {
        public double X { get; } = x;
        public double Y { get; } = y;

        public double DistanceTo(Point other) =>
            Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));

        public override string ToString() => $"({X}, {Y})";
    }

    // ── Reference type: class ─────────────────────────────────────────────────
    // Classes are allocated on the heap; two variables can point to the same object.
    public class BoundingBox
    {
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }

        public BoundingBox(Point topLeft, Point bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        public double Width  => Math.Abs(BottomRight.X - TopLeft.X);
        public double Height => Math.Abs(BottomRight.Y - TopLeft.Y);
        public double Area   => Width * Height;
    }

    /// <summary>
    /// Shows that assigning a struct copies the value, not the reference.
    /// </summary>
    public static (Point original, Point copy) DemonstrateValueCopy()
    {
        var original = new Point(1, 2);
        var copy     = original;         // full copy — independent
        return (original, copy);
    }

    /// <summary>
    /// Shows that assigning a class copies only the reference.
    /// Modifying through either variable affects the same object.
    /// </summary>
    public static (BoundingBox a, BoundingBox b) DemonstrateReferenceSemantic()
    {
        var a = new BoundingBox(new Point(0, 0), new Point(10, 10));
        var b = a;                       // b points to the same object
        b.TopLeft = new Point(5, 5);     // affects a.TopLeft too
        return (a, b);
    }

    // ── Boxing / Unboxing ─────────────────────────────────────────────────────
    // Storing a value type in an object variable boxes it (heap allocation).
    // Casting back to the value type unboxes it. Avoid in hot paths.
    public static int BoxingRoundTrip(int n)
    {
        object boxed   = n;          // box
        int    unboxed = (int)boxed; // unbox
        return unboxed;
    }
}
