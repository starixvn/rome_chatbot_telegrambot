using Fanex.Caching;
using Fanex.Data.Repository;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class ProductServiceTests
{
    private readonly ICacheService cacheService;
    private readonly IDynamicRepository dynamicRepository;
    private readonly ProductService productService;

    public ProductServiceTests()
    {
        cacheService = Substitute.For<ICacheService>();
        dynamicRepository = Substitute.For<IDynamicRepository>();
        productService = new ProductService(cacheService, dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetOfferedProductIdsByCustomer_HasNOTProductOffered_ReturnsEmptyOfByte(int custId)
    {
        // Arrange
        cacheService.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<IEnumerable<Product>>>>(),
            Arg.Any<CacheItemOptions>())
            .Returns(Task.FromResult<IEnumerable<Product>>(null));
        var expected = Enumerable.Empty<byte>();

        // Act
        var result = await productService.GetOfferedProductIdsByCustomer(custId);

        // Assert
        Assert.Same(result, expected);
    }

    [Theory, AutoData]
    public async Task GetOfferedProductIdsByCustomer_HasProductOffered_ReturnsEmptyOfByte(int custId)
    {
        // Arrange
        IEnumerable<Product> products = new List<Product>{
            new Product{Id=1, Name="Product1", IsHidden = true},
            new Product{Id=2, Name="Product2", IsHidden = true},
            new Product{Id=3, Name="Product3", IsHidden = false}
        };
        cacheService.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<IEnumerable<Product>>>>(),
            Arg.Any<CacheItemOptions>())
            .Returns(Task.FromResult(products));
        IEnumerable<byte> expected = products.Where(p => !p.IsHidden).Select(p => (byte)p.Id);

        // Act
        var result = await productService.GetOfferedProductIdsByCustomer(custId);

        // Assert
        Assert.Equal(result, expected);
    }
}