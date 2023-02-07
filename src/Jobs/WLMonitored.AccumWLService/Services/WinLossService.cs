using System.Text;
using Fanex.Data.Repository;
using WLMonitored.AccumWLService.Criteria;
using WLMonitored.AccumWLService.Models;
using WLMonitored.Shared.Criteria;
using WLMonitored.Shared.Models;

namespace WLMonitored.AccumWLService.Services;

public interface IWinLossService
{
    Task<IEnumerable<WinLossSetting>> GetWinLossSettings(string custIds, string superUUS, bool isTest);

    Task<IEnumerable<AccumulatedWinLoss>> GetAccumulatedWinLoss(
        string creditCustIds,
        string creditSMAIds,
        string depositCustIds,
        DateTime currentDate);

    Task UpdateAccumulatedWinLoss(
        IEnumerable<AccumulatedWinLossParam> accumulatedWinLossParams,
        long lastId,
        string serviceName);
}

public sealed class WinLossService : IWinLossService
{
    private readonly IDynamicRepository dynamicRepository;

    public WinLossService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public Task<IEnumerable<WinLossSetting>> GetWinLossSettings(string custIds, string superUUS, bool isTest)
        => dynamicRepository.FetchAsync<WinLossSetting>(new GetWinLossSettingCriteria(custIds, superUUS, isTest));

    public async Task<IEnumerable<AccumulatedWinLoss>> GetAccumulatedWinLoss(
        string creditCustIds,
        string creditSMAIds,
        string depositCustIds,
        DateTime currentDate)
    {
        var accumulatedWinLoss = new List<AccumulatedWinLoss>();

        if (!string.IsNullOrWhiteSpace(creditCustIds) || !string.IsNullOrWhiteSpace(creditSMAIds))
        {
            accumulatedWinLoss.AddRange(await dynamicRepository.FetchAsync<AccumulatedWinLoss>(
                new GetCreditAccumulatedWinLossCriteria(creditCustIds, creditSMAIds, currentDate)));
        }

        if (!string.IsNullOrWhiteSpace(depositCustIds))
        {
            accumulatedWinLoss.AddRange(await dynamicRepository.FetchAsync<AccumulatedWinLoss>(
                new GetDepositAccumulatedWinLossCriteria(depositCustIds, currentDate)));
        }

        return accumulatedWinLoss;
    }

    public Task UpdateAccumulatedWinLoss(
        IEnumerable<AccumulatedWinLossParam> accumulatedWinLossParams,
        long lastId,
        string serviceName)
        => dynamicRepository.ExecuteAsync(new UpdateAccumulateWinLossCommand(
            BuildCustWinlostXml(accumulatedWinLossParams),
            lastId,
            serviceName));

    private static string BuildCustWinlostXml(IEnumerable<AccumulatedWinLossParam> accumulatedWinLossParams)
    {
        var xmlBuilder = new StringBuilder();
        xmlBuilder.Append("<CustIds>");

        foreach (var item in accumulatedWinLossParams)
        {
            item.AppendCustXml(xmlBuilder);
        }

        xmlBuilder.Append("</CustIds>");
        return xmlBuilder.ToString();
    }
}