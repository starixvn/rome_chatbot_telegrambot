using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.MemberWinLossLimits;
using TelegramBot.Importer.Models.MemberWinLossLimits;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class MemberWinLossLimitServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly MemberWinLossLimitService memberWinLossLimitService;

    public MemberWinLossLimitServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        memberWinLossLimitService = new MemberWinLossLimitService(dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetMemberWinLossLimits_Always_RetrunsMemberWinLossLimits(int batchSize, long lastLogId, IEnumerable<MemberWinLossLimit> memberWinLossLimits)
    {
        // Arrange
        dynamicRepository.FetchAsync<MemberWinLossLimit>(Arg.Any<GetMemberWinLossLimitCommand>()).Returns(Task.FromResult(memberWinLossLimits));

        // Act
        var result = await memberWinLossLimitService.GetMemberWinLossLimits(batchSize, lastLogId);

        // Assert
        Assert.Same(result.Item2, memberWinLossLimits);
    }
}