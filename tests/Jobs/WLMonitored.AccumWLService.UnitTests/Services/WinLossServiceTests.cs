using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using NSubstitute;
using WLMonitored.AccumWLService.Criteria;
using WLMonitored.AccumWLService.Models;
using WLMonitored.AccumWLService.Services;
using WLMonitored.Shared.Criteria;
using WLMonitored.Shared.Extensions;
using WLMonitored.Shared.Models;
using Xunit;

namespace WLMonitored.AccumWLService.UnitTests.Services;

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
    public async Task GetWinLossSettings_Always_CallsFetchAsync()
    {
        // Arrange
        const string custIds = "1,2,3,4,5,6";

        // Act
        await winLossService.GetWinLossSettings(custIds, string.Empty, false);

        // Assert
        await dynamicRepository.Received().FetchAsync<WinLossSetting>(Arg.Is<GetWinLossSettingCriteria>(
            criteria => custIds == criteria.CustList
        ));
    }

    [Fact]
    public async Task GetAccumulatedWinLoss_Credit_CallsFetchAsync()
    {
        // Arrange
        const string creditCustIds = "1,2,3,4,5,6";
        const string depositCustIds = "";
        var today = DateTime.Today;

        // Act
        await winLossService.GetAccumulatedWinLoss(creditCustIds, string.Empty, depositCustIds, today);

        // Assert
        await dynamicRepository.Received().FetchAsync<AccumulatedWinLoss>(Arg.Is<GetCreditAccumulatedWinLossCriteria>(
            criteria => creditCustIds == criteria.CustIds
            && today == criteria.CurrentDate
        ));
    }

    [Fact]
    public async Task GetAccumulatedWinLoss_Deposit_CallsFetchAsync()
    {
        // Arrange
        const string creditCustIds = "";
        const string depositCustIds = "7,8,9,";
        var today = DateTime.Today;

        // Act
        await winLossService.GetAccumulatedWinLoss(creditCustIds, string.Empty, depositCustIds, today);

        // Assert
        await dynamicRepository.Received().FetchAsync<AccumulatedWinLoss>(Arg.Is<GetDepositAccumulatedWinLossCriteria>(
            criteria => depositCustIds == criteria.CustIds
            && today == criteria.CurrentDate
        ));
    }

    [Fact]
    public async Task GetAccumulatedWinLoss_CreditAndDeposit_CallsFetchAsync()
    {
        // Arrange
        const string creditCustIds = "1,2,3,4,5,6";
        const string depositCustIds = "7,8,9,";
        var today = DateTime.Today;

        // Act
        await winLossService.GetAccumulatedWinLoss(creditCustIds, string.Empty, depositCustIds, today);

        // Assert
        await dynamicRepository.Received().FetchAsync<AccumulatedWinLoss>(Arg.Is<GetCreditAccumulatedWinLossCriteria>(
            criteria => creditCustIds == criteria.CustIds
            && today == criteria.CurrentDate
        ));
        await dynamicRepository.Received().FetchAsync<AccumulatedWinLoss>(Arg.Is<GetDepositAccumulatedWinLossCriteria>(
            criteria => depositCustIds == criteria.CustIds
            && today == criteria.CurrentDate
        ));
    }

    [Fact]
    public async Task UpdateAccumulatedWinLoss_Always_ExecuteAsync()
    {
        // Arrange
        var now = DateTime.Now;
        var accumulatedWinLossParams = new List<AccumulatedWinLossParam>
        {
            new AccumulatedWinLossParam(1, false, 10, now, string.Empty, 1),
            new AccumulatedWinLossParam(2, true, 20, now, "i", 1)
        };
        const long lastId = 1001;

        // Act
        await winLossService.UpdateAccumulatedWinLoss(accumulatedWinLossParams, lastId, "WLMonitoredAccount");

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<UpdateAccumulateWinLossCommand>(
            criteria => $"<CustIds><Cust CustId=\"1\" AccumWL=\"10\" Type=\"c\" SettlementDate=\"{now.ToSimpleDate()}\" Action=\"\" RoleId=\"1\"/><Cust CustId=\"2\" AccumWL=\"20\" Type=\"d\" SettlementDate=\"{now.ToSimpleDate()}\" Action=\"i\" RoleId=\"1\"/></CustIds>" == criteria.CustWinlost
            && lastId == criteria.LastId
        ));
    }
}