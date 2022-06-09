using System;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using NSubstitute;
using WLMonitored.RiskControl.Importer.Criteria;
using WLMonitored.RiskControl.Importer.Models;
using WLMonitored.RiskControl.Importer.Services;
using WLMonitored.Shared.Criteria;
using WLMonitored.Shared.Models;
using Xunit;

namespace WLMonitored.RiskControl.Importer.UnitTests.Services;

public class WinLossServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly IWinLossService winLossService;

    public WinLossServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        winLossService = new WinLossService(dynamicRepository);
    }

    [Fact]
    public async Task GetLatestAccumulatedWinLoss_Always_CallsFetchAsync()
    {
        // Arrange
        var lastCheckTime = DateTime.Now;
        const int batchSize = 1000;

        // Act
        await winLossService.GetLatestAccumulatedWinLoss(lastCheckTime, batchSize);

        // Assert
        await dynamicRepository.Received().FetchAsync<AccumulatedWinLoss>(Arg.Is<GetLatestAccumulatedWinLossCriteria>(
            criteria => lastCheckTime == criteria.LastCheckTime
            && batchSize == criteria.BatchNo
        ));
    }

    [Fact]
    public async Task GetWinLossThresholds_Always_CallsFetchAsync()
    {
        // Arrange
        const string custIds = "23003140";

        // Act
        await winLossService.GetWinLossThresholds(custIds, string.Empty, false);

        // Assert
        await dynamicRepository.Received().FetchAsync<WinLossThreshold>(Arg.Is<GetWinLossSettingCriteria>(
            criteria => custIds == criteria.CustList
            && 0 == criteria.SportType
            && !criteria.IsDeposit
        ));
    }

    [Fact]
    public async Task GetChangedWinLossThresholds_Always_CallsFetchAsync()
    {
        // Arrange & Act
        var now = DateTime.Now;
        await winLossService.GetChangedWinLossThresholds(string.Empty, false, now);

        // Assert
        await dynamicRepository.Received().FetchAsync<WinLossThreshold>(Arg.Is<GetChangedWinLossThresholdCriteria>(
            criteria => 0 == criteria.SportType && !criteria.IsDeposit && now == criteria.LastScanDate
        ));
    }

    [Fact]
    public async Task GetAccumulatedWinLoss_Always_CallsFetchAsync()
    {
        // Arrange
        const string custIds = "23003140";
        var currentDate = DateTime.Now;

        // Act
        await winLossService.GetAccumulatedWinLoss(custIds, currentDate);

        // Assert
        await dynamicRepository.Received().FetchAsync<AccumulatedWinLoss>(Arg.Is<GetCreditAccumulatedWinLossCriteria>(
            criteria => custIds == criteria.CustIds
            && currentDate == criteria.CurrentDate
        ));
    }
}