using Collections.BasicCollections;
using Collections.Linq;
using Collections.Advanced;

namespace Collections.Tests;

public class ListAndDictionaryTests
{
    [Fact]
    public void BuildWordFrequency_CountsCorrectly()
    {
        var words = new[] { "apple", "banana", "Apple", "cherry", "banana" };
        var freq  = ListAndDictionaryExamples.BuildWordFrequency(words);

        Assert.Equal(2, freq["apple"]);
        Assert.Equal(2, freq["banana"]);
        Assert.Equal(1, freq["cherry"]);
    }

    [Fact]
    public void BuildWordFrequencyFast_SameResultAsNaive()
    {
        var words = new[] { "x", "y", "x", "z", "y", "x" };
        var naive = ListAndDictionaryExamples.BuildWordFrequency(words);
        var fast  = ListAndDictionaryExamples.BuildWordFrequencyFast(words);

        foreach (var (k, v) in naive)
            Assert.Equal(v, fast[k]);
    }

    [Fact]
    public void GroupByCategory_CreatesCorrectGroups()
    {
        var items = new[]
        {
            new ListAndDictionaryExamples.Item("Fruit",  "Apple",  1.0m),
            new ListAndDictionaryExamples.Item("Fruit",  "Banana", 0.5m),
            new ListAndDictionaryExamples.Item("Veggie", "Carrot", 0.8m),
        };
        var lookup = ListAndDictionaryExamples.GroupByCategory(items);

        Assert.Equal(2, lookup["Fruit"].Count());
        Assert.Single(lookup["Veggie"]);
    }
}

public class SetQueueStackTests
{
    [Fact]
    public void Intersect_FindsCommonElements()
    {
        var result = SetQueueStackExamples.Intersect(
            new[] { 1, 2, 3, 4 }, new[] { 3, 4, 5, 6 });
        Assert.Equal(new[] { 3, 4 }.OrderBy(x => x), result.OrderBy(x => x));
    }

    [Fact]
    public void SymmetricDifference_FindsUniquesInEachSet()
    {
        var result = SetQueueStackExamples.SymmetricDifference(
            new[] { 1, 2, 3 }, new[] { 3, 4, 5 });
        Assert.DoesNotContain(3, result);
        Assert.Contains(1, result);
        Assert.Contains(4, result);
    }

    [Fact]
    public void BreadthFirstTraversal_VisitsLevelByLevel()
    {
        var tree = new SetQueueStackExamples.TreeNode<int>(1,
        [
            new(2, [new(4, []), new(5, [])]),
            new(3, [new(6, [])]),
        ]);
        var result = SetQueueStackExamples.BreadthFirstTraversal(tree);
        Assert.Equal([1, 2, 3, 4, 5, 6], result);
    }

    [Fact]
    public void ProcessByPriority_OrdersByPriority()
    {
        var tasks = new (string, int)[]
        {
            ("low", 3), ("critical", 1), ("medium", 2)
        };
        var result = SetQueueStackExamples.ProcessByPriority(tasks);
        Assert.Equal("critical", result[0]);
        Assert.Equal("medium",   result[1]);
        Assert.Equal("low",      result[2]);
    }

    [Fact]
    public void UniqueOrdered_PreservesInsertionOrder()
    {
        var result = SetQueueStackExamples.UniqueOrdered(
            new[] { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3 });
        Assert.Equal([3, 1, 4, 5, 9, 2, 6], result);
    }
}

public class AdvancedLinqTests
{
    private static readonly AdvancedLinq.Employee[] Employees =
    [
        new(1, "Alice",   "Engineering", 90_000, 5),
        new(2, "Bob",     "Engineering", 80_000, 3),
        new(3, "Carol",   "Marketing",   70_000, 7),
        new(4, "Dave",    "Marketing",   65_000, 2),
        new(5, "Eve",     "Engineering", 95_000, 8),
    ];

    [Fact]
    public void DepartmentStats_ComputesCorrectly()
    {
        var stats = AdvancedLinq.DepartmentStats(Employees);
        var eng   = stats.First(s => s.Dept == "Engineering");
        Assert.Equal(3, eng.Count);
        Assert.Equal((90_000m + 80_000m + 95_000m) / 3, eng.AvgSalary);
    }

    [Fact]
    public void TopEarnerPerDepartment_ReturnsBestPerDept()
    {
        var top = AdvancedLinq.TopEarnerPerDepartment(Employees);
        Assert.Equal(2, top.Count);
        Assert.Contains(top, e => e.Name == "Eve");      // Eng top earner
        Assert.Contains(top, e => e.Name == "Carol");    // Mkt top earner
    }

    [Fact]
    public void Paginate_SplitsIntoCorrectChunks()
    {
        var pages = AdvancedLinq.Paginate(Enumerable.Range(1, 10), 3);
        Assert.Equal(4, pages.Count);
        Assert.Equal(3, pages[0].Length);
        Assert.Single(pages[3]);
    }
}

public class CustomLinqExtensionsTests
{
    [Fact]
    public void Batch_SplitsCorrectly()
    {
        var batches = Enumerable.Range(1, 7).Batch(3).ToList();
        Assert.Equal(3, batches.Count);
        Assert.Equal([1, 2, 3], batches[0]);
        Assert.Equal([7],       batches[2]);
    }

    [Fact]
    public void Window_SlidesCorrectly()
    {
        var windows = Enumerable.Range(1, 5).Window(3).ToList();
        Assert.Equal(3, windows.Count);
        Assert.Equal([1, 2, 3], windows[0]);
        Assert.Equal([3, 4, 5], windows[2]);
    }

    [Fact]
    public void Scan_ComputesRunningSum()
    {
        var running = Enumerable.Range(1, 5)
            .Scan(0, (acc, x) => acc + x)
            .ToList();
        Assert.Equal([1, 3, 6, 10, 15], running);
    }

    [Fact]
    public void TakeUntil_IncludesFirstMatchingElement()
    {
        var result = Enumerable.Range(1, 10)
            .TakeUntil(x => x == 5)
            .ToList();
        Assert.Equal([1, 2, 3, 4, 5], result);
    }
}

public class SpanAndMemoryTests
{
    [Fact]
    public void CountDigits_CountsCorrectly()
    {
        Assert.Equal(3, SpanAndMemoryExamples.CountDigits("abc123".AsSpan()));
        Assert.Equal(0, SpanAndMemoryExamples.CountDigits("hello".AsSpan()));
    }

    [Fact]
    public void ReverseSmallString_ReversesCorrectly()
    {
        Assert.Equal("olleh", SpanAndMemoryExamples.ReverseSmallString("hello"));
        Assert.Equal("",      SpanAndMemoryExamples.ReverseSmallString(""));
    }

    [Fact]
    public async Task SumSegmentAsync_SumsCorrectly()
    {
        int[] arr     = [1, 2, 3, 4, 5];
        int   result  = await SpanAndMemoryExamples.SumSegmentAsync(arr.AsMemory(1, 3));
        Assert.Equal(9, result); // 2 + 3 + 4
    }
}

public class ConcurrentCollectionTests
{
    [Fact]
    public void HitCounter_RecordsHits()
    {
        var counter = new ConcurrentCollectionExamples.HitCounter();
        Parallel.For(0, 100, _ => counter.Record("https://example.com"));
        Assert.Equal(100L, counter.Get("https://example.com"));
    }

    [Fact]
    public async Task ConcurrentQueueDemo_CollectsAllItems()
    {
        var results = await ConcurrentCollectionExamples.ConcurrentQueueDemoAsync(
            producerCount: 3, itemsPerProducer: 4);
        Assert.Equal(12, results.Count);
    }
}
