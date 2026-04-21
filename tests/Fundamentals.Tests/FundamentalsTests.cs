using Fundamentals.TypeSystem;
using Fundamentals.PatternMatching;
using Fundamentals.ModernSyntax;
using Fundamentals.Generics;

namespace Fundamentals.Tests;

public class TypeSystemTests
{
    [Fact]
    public void ValueCopy_IsIndependent()
    {
        var (original, copy) = ValueAndReferenceTypes.DemonstrateValueCopy();
        Assert.Equal(original.X, copy.X);
        Assert.Equal(original.Y, copy.Y);
    }

    [Fact]
    public void ReferenceSemantic_BothVariablesPointToSameObject()
    {
        var (a, b) = ValueAndReferenceTypes.DemonstrateReferenceSemantic();
        Assert.Same(a, b);
        Assert.Equal(5, a.TopLeft.X);
    }

    [Fact]
    public void Boxing_RoundTrip_PreservesValue()
    {
        Assert.Equal(42, ValueAndReferenceTypes.BoxingRoundTrip(42));
    }

    [Theory]
    [InlineData(null, "Hello, stranger!")]
    [InlineData("Alice", "Hello, Alice!")]
    public void Nullable_Greet_ReturnsExpected(string? name, string expected)
    {
        Assert.Equal(expected, NullableTypes.Greet(name));
    }

    [Fact]
    public void Nullable_GetLength_ReturnsNullForNull()
    {
        Assert.Null(NullableTypes.GetLength(null));
        Assert.Equal(5, NullableTypes.GetLength("hello"));
    }

    [Fact]
    public void Nullable_Coalesce_PicksFirstNonNull()
    {
        Assert.Equal(10, NullableTypes.Coalesce(10, 20, 30));
        Assert.Equal(20, NullableTypes.Coalesce(null, 20, 30));
        Assert.Equal(30, NullableTypes.Coalesce(null, null, 30));
    }

    [Fact]
    public void Record_WithExpression_CreatesNewInstance()
    {
        var person  = new RecordsAndStructs.Person("Alice", "Smith", new DateOnly(1990, 1, 1));
        var renamed = RecordsAndStructs.Rename(person, "Jones");

        Assert.Equal("Smith", person.LastName);
        Assert.Equal("Jones", renamed.LastName);
        Assert.Equal("Alice", renamed.FirstName);
    }

    [Fact]
    public void Record_Equality_ByValue()
    {
        var a = new RecordsAndStructs.Temperature(100);
        var b = new RecordsAndStructs.Temperature(100);
        Assert.Equal(a, b);
        Assert.Equal(212, a.Fahrenheit);
    }

    [Fact]
    public void Record_Deconstruct_WorksCorrectly()
    {
        var person = new RecordsAndStructs.Person("Bob", "Builder", new DateOnly(1985, 6, 15));
        var (first, last) = RecordsAndStructs.DeconstructPerson(person);
        Assert.Equal("Bob", first);
        Assert.Equal("Builder", last);
    }
}

public class PatternMatchingTests
{
    [Fact]
    public void SwitchExpression_Circle_ComputesArea()
    {
        var circle = new SwitchExpressions.Circle(5);
        double area = SwitchExpressions.Area(circle);
        Assert.Equal(Math.PI * 25, area, precision: 10);
    }

    [Theory]
    [InlineData(3.5,  SwitchExpressions.RiskLevel.Low)]
    [InlineData(5.0,  SwitchExpressions.RiskLevel.Medium)]
    [InlineData(8.0,  SwitchExpressions.RiskLevel.High)]
    [InlineData(9.5,  SwitchExpressions.RiskLevel.Critical)]
    public void SwitchExpression_CvssScore_ClassifiesCorrectly(double score, SwitchExpressions.RiskLevel expected)
    {
        Assert.Equal(expected, SwitchExpressions.ClassifyCvssScore(score));
    }

    [Fact]
    public void SwitchExpression_ListPattern_Empty()
    {
        Assert.Equal("empty", SwitchExpressions.DescribeSequence([]));
    }

    [Fact]
    public void SwitchExpression_ListPattern_Single()
    {
        Assert.Equal("single: 42", SwitchExpressions.DescribeSequence([42]));
    }

    [Fact]
    public void TypePattern_IsNotNull_WorksCorrectly()
    {
        Assert.Equal("integer: 7", TypePatterns.Describe(7));
        Assert.Equal("null",       TypePatterns.Describe(null!));
        Assert.Equal("string of length 5", TypePatterns.Describe("hello"));
    }

    [Fact]
    public void TypePattern_NestedProperty_DetectsEuropeanCustomer()
    {
        var eu  = new TypePatterns.Customer("Hans", new TypePatterns.Address("DE", "Berlin"));
        var us  = new TypePatterns.Customer("John", new TypePatterns.Address("US", "NYC"));
        Assert.True(TypePatterns.IsEuropeanCustomer(eu));
        Assert.False(TypePatterns.IsEuropeanCustomer(us));
    }
}

public class ModernSyntaxTests
{
    [Fact]
    public void String_FormatOrder_ProducesCorrectOutput()
    {
        var result = StringFeatures.FormatOrder("Alice", 3, 9.99m);
        Assert.Contains("Alice", result);
        Assert.Contains("3", result);
        Assert.Contains("9.99", result);
    }

    [Fact]
    public void String_ParseKeyValue_ExtractsCorrectly()
    {
        var (key, value) = StringFeatures.ParseKeyValue("  host = localhost  ");
        Assert.Equal("host", key);
        Assert.Equal("localhost", value);
    }

    [Fact]
    public void String_ToSnakeCase_ConvertsCorrectly()
    {
        Assert.Equal("camel_case_string", StringFeatures.ToSnakeCase("CamelCaseString"));
        Assert.Equal("my_variable",       StringFeatures.ToSnakeCase("MyVariable"));
    }

    [Fact]
    public void Tuples_Statistics_ReturnsCorrectValues()
    {
        var (min, max, mean) = TuplesAndLocalFunctions.Statistics([1.0, 2.0, 3.0, 4.0, 5.0]);
        Assert.Equal(1.0, min);
        Assert.Equal(5.0, max);
        Assert.Equal(3.0, mean);
    }

    [Fact]
    public void Tuples_GetFibonacci_ReturnsCorrectSequence()
    {
        var fib = TuplesAndLocalFunctions.GetFibonacci(8);
        Assert.Equal([0, 1, 1, 2, 3, 5, 8, 13], fib);
    }

    [Fact]
    public void LinqBasics_TopThreeLongWords_WorksCorrectly()
    {
        var words = new[] { "hi", "hello", "wonderful", "programming", "cat", "extraordinary" };
        var top   = ExpressionBodiedAndLinqBasics.TopThreeLongWords(words);
        Assert.Equal(3, top.Count);
        Assert.Equal("extraordinary", top[0]);
    }

    [Fact]
    public void LinqBasics_TotalByCategory_AggregatesCorrectly()
    {
        var products = new[]
        {
            new ExpressionBodiedAndLinqBasics.Product("A", 10m, "Books"),
            new ExpressionBodiedAndLinqBasics.Product("B", 20m, "Books"),
            new ExpressionBodiedAndLinqBasics.Product("C", 15m, "Tech"),
        };
        var totals = ExpressionBodiedAndLinqBasics.TotalByCategory(products);
        Assert.Equal(30m, totals["Books"]);
        Assert.Equal(15m, totals["Tech"]);
    }
}

public class GenericTests
{
    [Fact]
    public void Clamp_ClampsCorrectly()
    {
        Assert.Equal(5, GenericConstraints.Clamp(3, 5, 10));
        Assert.Equal(10, GenericConstraints.Clamp(15, 5, 10));
        Assert.Equal(7, GenericConstraints.Clamp(7, 5, 10));
    }

    [Fact]
    public void Sum_Generic_SumsIntegers()
    {
        Assert.Equal(15, GenericConstraints.Sum([1, 2, 3, 4, 5]));
    }

    [Fact]
    public void InMemoryRepository_AddAndGet_WorksCorrectly()
    {
        var repo = new GenericConstraints.InMemoryRepository<TestEntity>();
        var entity = new TestEntity(1, "Test");
        repo.Add(entity);
        Assert.Equal(entity, repo.GetById(1));
        Assert.Single(repo.GetAll());
    }

    private record TestEntity(int Id, string Name) : GenericConstraints.IEntity;

    [Fact]
    public void Variance_SortedDogs_SortsAlphabetically()
    {
        var dogs = new[]
        {
            new Variance.Dog("Zara", "Labrador"),
            new Variance.Dog("Buddy", "Poodle"),
            new Variance.Dog("Max", "Husky"),
        };
        var sorted = Variance.SortedDogs(dogs);
        Assert.Equal("Buddy", sorted[0].Name);
        Assert.Equal("Max",   sorted[1].Name);
        Assert.Equal("Zara",  sorted[2].Name);
    }
}
