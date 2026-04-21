namespace Collections.BasicCollections;

/// <summary>
/// Demonstrates HashSet&lt;T&gt;, Queue&lt;T&gt;, Stack&lt;T&gt;, and PriorityQueue&lt;TElement, TPriority&gt;.
///
/// HashSet provides O(1) contains/add/remove for unordered unique elements.
/// Queue is FIFO (first in, first out).
/// Stack is LIFO (last in, first out).
/// PriorityQueue dequeues the element with the smallest priority value first.
/// </summary>
public static class SetQueueStackExamples
{
    // ── HashSet<T> ────────────────────────────────────────────────────────────
    public static HashSet<T> Intersect<T>(IEnumerable<T> first, IEnumerable<T> second)
    {
        var set = new HashSet<T>(first);
        set.IntersectWith(second);
        return set;
    }

    public static HashSet<T> SymmetricDifference<T>(IEnumerable<T> a, IEnumerable<T> b)
    {
        var set = new HashSet<T>(a);
        set.SymmetricExceptWith(b);
        return set;
    }

    // ── Queue<T> — breadth-first traversal ───────────────────────────────────
    public record TreeNode<T>(T Value, IReadOnlyList<TreeNode<T>> Children);

    public static IReadOnlyList<T> BreadthFirstTraversal<T>(TreeNode<T> root)
    {
        var result = new List<T>();
        var queue  = new Queue<TreeNode<T>>();
        queue.Enqueue(root);

        while (queue.TryDequeue(out var node))
        {
            result.Add(node.Value);
            foreach (var child in node.Children)
                queue.Enqueue(child);
        }
        return result;
    }

    // ── Stack<T> — depth-first traversal ────────────────────────────────────
    public static IReadOnlyList<T> DepthFirstTraversal<T>(TreeNode<T> root)
    {
        var result = new List<T>();
        var stack  = new Stack<TreeNode<T>>();
        stack.Push(root);

        while (stack.TryPop(out var node))
        {
            result.Add(node.Value);
            // Push children in reverse so leftmost is processed first.
            foreach (var child in node.Children.Reverse())
                stack.Push(child);
        }
        return result;
    }

    // ── PriorityQueue<TElement, TPriority> ───────────────────────────────────
    // Min-heap by default: lowest priority value dequeues first.
    public static IReadOnlyList<string> ProcessByPriority(
        IEnumerable<(string task, int priority)> tasks)
    {
        var pq = new PriorityQueue<string, int>();
        foreach (var (task, priority) in tasks)
            pq.Enqueue(task, priority);

        var results = new List<string>(pq.Count);
        while (pq.TryDequeue(out string? task, out _))
            results.Add(task);
        return results;
    }

    // ── Deduplication preserving insertion order (HashSet + List) ─────────────
    public static IReadOnlyList<T> UniqueOrdered<T>(IEnumerable<T> source)
        where T : notnull
    {
        var seen   = new HashSet<T>();
        var result = new List<T>();
        foreach (var item in source)
            if (seen.Add(item))
                result.Add(item);
        return result;
    }
}
