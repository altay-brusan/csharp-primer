using Patterns.Creational;
using Patterns.Behavioral;
using Patterns.Structural;
using Patterns.Functional;

namespace Patterns.Tests;

public class FactoryPatternTests
{
    [Theory]
    [InlineData("email", "Email")]
    [InlineData("sms",   "SMS")]
    [InlineData("push",  "Push")]
    public void CreateNotification_ReturnsCorrectChannel(string input, string expected)
    {
        var notification = FactoryPatterns.CreateNotification(input);
        Assert.Equal(expected, notification.Channel);
    }

    [Fact]
    public void CreateNotification_UnknownChannel_Throws()
    {
        Assert.Throws<ArgumentException>(() => FactoryPatterns.CreateNotification("fax"));
    }

    [Fact]
    public void WebFactory_RenderLoginForm_ContainsHtmlTags()
    {
        var html = FactoryPatterns.RenderLoginForm(new FactoryPatterns.WebUiFactory());
        Assert.Contains("<button>", html);
        Assert.Contains("<input",   html);
    }

    [Fact]
    public void WindowsFactory_RenderLoginForm_ContainsWinTags()
    {
        var html = FactoryPatterns.RenderLoginForm(new FactoryPatterns.WindowsUiFactory());
        Assert.Contains("win:Button", html);
        Assert.Contains("win:TextBox", html);
    }
}

public class BuilderPatternTests
{
    [Fact]
    public void Builder_BuildsRequestCorrectly()
    {
        var request = new BuilderPattern.HttpRequestBuilder()
            .WithMethod("POST")
            .WithUrl("/api/resource")
            .WithBearerToken("tok")
            .WithJsonBody("{}")
            .WithTimeout(TimeSpan.FromSeconds(10))
            .Build();

        Assert.Equal("POST",           request.Method);
        Assert.Equal("/api/resource",  request.Url);
        Assert.Equal("{}",             request.Body);
        Assert.Equal(10_000,           request.TimeoutMs);
        Assert.Equal("Bearer tok",     request.Headers["Authorization"]);
    }

    [Fact]
    public void Builder_EmptyUrl_Throws()
    {
        var builder = new BuilderPattern.HttpRequestBuilder().WithUrl("");
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void BuildApiRequest_HelperMethod_SetsAllFields()
    {
        var req = BuilderPattern.BuildApiRequest("mytoken", "https://api.example.com", "{}");
        Assert.Equal("POST", req.Method);
        Assert.Contains("mytoken", req.Headers["Authorization"]);
    }
}

public class StrategyPatternTests
{
    [Fact]
    public void Strategy_AscendingSort_SortsCorrectly()
    {
        var items = new List<int> { 5, 2, 8, 1 };
        new StrategyPattern.Sorter<int>(new StrategyPattern.AscendingSort<int>()).Sort(items);
        Assert.Equal([1, 2, 5, 8], items);
    }

    [Fact]
    public void Strategy_DescendingSort_SortsCorrectly()
    {
        var items = new List<int> { 5, 2, 8, 1 };
        new StrategyPattern.Sorter<int>(new StrategyPattern.DescendingSort<int>()).Sort(items);
        Assert.Equal([8, 5, 2, 1], items);
    }

    [Fact]
    public void Strategy_Delegate_CalculatesTotalCorrectly()
    {
        var processor = new StrategyPattern.OrderProcessor(
            StrategyPattern.TenPercent,
            StrategyPattern.UsTax);

        decimal total = processor.CalculateTotal(100m);
        // 100 - 10% discount = 90; + 8% tax = 97.20
        Assert.Equal(97.20m, total);
    }
}

public class ObserverPatternTests
{
    [Fact]
    public void StockTicker_PriceChanged_FiresEvent()
    {
        var ticker = new ObserverPattern.StockTicker("AAPL", 150m);
        ObserverPattern.StockTicker.PriceChangedArgs? received = null;
        ticker.PriceChanged += (_, e) => received = e;

        ticker.UpdatePrice(160m);

        Assert.NotNull(received);
        Assert.Equal(150m, received!.OldPrice);
        Assert.Equal(160m, received.NewPrice);
    }

    [Fact]
    public void StockTicker_SamePrice_DoesNotFireEvent()
    {
        var ticker    = new ObserverPattern.StockTicker("GOOG", 100m);
        int fireCount = 0;
        ticker.PriceChanged += (_, _) => fireCount++;
        ticker.UpdatePrice(100m);
        Assert.Equal(0, fireCount);
    }

    [Fact]
    public void EventBus_PublishSubscribe_DeliversMessage()
    {
        var bus     = new ObserverPattern.EventBus();
        var received = new List<string>();
        bus.Subscribe<string>(msg => received.Add(msg));
        bus.Publish("hello");
        bus.Publish("world");
        Assert.Equal(["hello", "world"], received);
    }
}

public class DecoratorPatternTests
{
    [Fact]
    public async Task Decorator_LoggingDecorator_LogsMessages()
    {
        var smtp    = new DecoratorPattern.SmtpMessageService();
        var logging = new DecoratorPattern.LoggingMessageService(smtp);

        await logging.SendAsync("alice@example.com", "Hello");

        Assert.Equal(2, logging.Log.Count);
        Assert.Contains("alice@example.com", logging.Log[0]);
    }

    [Fact]
    public async Task Decorator_Composed_WorksEndToEnd()
    {
        var service = DecoratorPattern.BuildService();
        var result  = await service.SendAsync("bob@example.com", "Test");
        Assert.Contains("bob@example.com", result);
    }
}

public class RepositoryPatternTests
{
    private static RepositoryPattern.InMemoryProductRepository CreateRepo(
        params RepositoryPattern.Product[] seed) => new(seed);

    [Fact]
    public async Task Repository_AddAndGet_Works()
    {
        var repo = CreateRepo();
        var product = RepositoryPattern.Product.Create(1, "Laptop", 999m, "Electronics");
        await repo.AddAsync(product);

        var found = await repo.GetByIdAsync(1);
        Assert.Equal(product, found);
    }

    [Fact]
    public async Task Repository_GetByCategory_FiltersCorrectly()
    {
        var repo = CreateRepo(
            RepositoryPattern.Product.Create(1, "Laptop",  999m,  "Electronics"),
            RepositoryPattern.Product.Create(2, "Phone",   699m,  "Electronics"),
            RepositoryPattern.Product.Create(3, "Desk",    299m,  "Furniture"));

        var electronics = await repo.GetByCategoryAsync("Electronics");
        Assert.Equal(2, electronics.Count);
    }

    [Fact]
    public async Task ProductService_GetAffordable_FiltersAndSorts()
    {
        var repo = CreateRepo(
            RepositoryPattern.Product.Create(1, "Budget Phone",  300m, "Electronics"),
            RepositoryPattern.Product.Create(2, "Flagship Phone",900m, "Electronics"),
            RepositoryPattern.Product.Create(3, "Mid Phone",     500m, "Electronics"));

        var service = new RepositoryPattern.ProductService(repo);
        var result  = await service.GetAffordableAsync("Electronics", 600m);

        Assert.Equal(2, result.Count);
        Assert.Equal("Budget Phone", result[0].Name);
    }
}

public class ResultPatternTests
{
    private static readonly IReadOnlyList<ResultPatternExamples.User> Users =
    [
        new(1, "Alice", "alice@example.com"),
        new(2, "Bob",   "bob@example.com"),
    ];

    [Fact]
    public void FindUser_ExistingId_ReturnsSuccess()
    {
        var result = ResultPatternExamples.FindUser(Users, 1);
        Assert.True(result.IsSuccess);
        Assert.Equal("Alice", result.Value.Name);
    }

    [Fact]
    public void FindUser_NonExistentId_ReturnsFailure()
    {
        var result = ResultPatternExamples.FindUser(Users, 99);
        Assert.True(result.IsFailure);
        Assert.Equal(ResultPatternExamples.UserError.NotFound, result.Error);
    }

    [Fact]
    public void ValidateEmail_InvalidEmail_ReturnsFailure()
    {
        var result = ResultPatternExamples.ValidateEmail("not-an-email");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void UpdateEmail_ValidScenario_Updates()
    {
        var users  = Users.ToList();
        var result = ResultPatternExamples.UpdateEmail(users, 1, "new@email.com");
        Assert.True(result.IsSuccess);
        Assert.Equal("new@email.com", result.Value.Email);
    }

    [Fact]
    public void Result_Map_TransformsValue()
    {
        var result = Result<int, string>.Ok(42).Map(x => x * 2);
        Assert.Equal(84, result.Value);
    }

    [Fact]
    public void Result_Bind_PropagatesError()
    {
        var failure = Result<int, string>.Fail("oops");
        var chained = failure.Bind(x => Result<string, string>.Ok(x.ToString()));
        Assert.True(chained.IsFailure);
        Assert.Equal("oops", chained.Error);
    }
}

public class SpecificationPatternTests
{
    private static readonly IReadOnlyList<ProductSpecifications.Product> Catalogue =
    [
        new("Laptop",  1200m, "Electronics", true,  10),
        new("Phone",    400m, "Electronics", true,   5),
        new("Tablet",   800m, "Electronics", false,  0),
        new("Desk",     350m, "Furniture",   true,   3),
        new("Monitor",  450m, "Electronics", true,   2),
    ];

    [Fact]
    public void FindAffordableElectronicsInStock_ReturnsCorrectProducts()
    {
        var results = ProductSpecifications.FindAffordableElectronicsInStock(Catalogue);
        // Phone (400) and Monitor (450) match; Laptop (1200) too expensive, Tablet out-of-stock
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Equal("Electronics", p.Category));
        Assert.All(results, p => Assert.True(p.InStock));
        Assert.All(results, p => Assert.True(p.Price <= 500m));
    }

    [Fact]
    public void Specification_Not_InvertsResult()
    {
        var inStock    = new ProductSpecifications.InStockSpec();
        var outOfStock = !inStock;
        var results    = ProductSpecifications.Filter(Catalogue, outOfStock);
        Assert.Single(results);
        Assert.Equal("Tablet", results[0].Name);
    }

    [Fact]
    public void Specification_Or_CombinesCorrectly()
    {
        var electronics = new ProductSpecifications.CategorySpec("Electronics");
        var furniture   = new ProductSpecifications.CategorySpec("Furniture");
        var either      = electronics | furniture;
        var results     = ProductSpecifications.Filter(Catalogue, either);
        Assert.Equal(Catalogue.Count, results.Count);
    }
}
