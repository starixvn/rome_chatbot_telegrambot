using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.ProblemAccounts;
using TelegramBot.Importer.Models.ProblemAccounts;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class ProblemAccountServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly ProblemAccountService problemAccountService;

    public ProblemAccountServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        problemAccountService = new ProblemAccountService(dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetProblemAccounts_Always_RetrunsProblemAccounts(int lastLogId, int batchSize, IEnumerable<ProblemAccount> problemAccounts)
    {
        // Arrange
        dynamicRepository.FetchAsync<ProblemAccount>(Arg.Any<GetProblemAccountCommand>()).Returns(Task.FromResult(problemAccounts));

        // Act
        var result = await problemAccountService.GetProblemAccounts(lastLogId, batchSize);

        // Assert
        Assert.Same(result.Item2, problemAccounts);
    }
}