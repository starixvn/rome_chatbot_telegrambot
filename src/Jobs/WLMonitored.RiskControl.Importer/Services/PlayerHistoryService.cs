using System.Text;
using Fanex.Data.Repository;
using WLMonitored.RiskControl.Importer.Criteria;
using WLMonitored.RiskControl.Importer.Models;

namespace WLMonitored.RiskControl.Importer.Services;

public interface IPlayerHistoryService
{
    Task Insert(
        IEnumerable<ReachPlayerHistoryParam> reachPlayerHistoryParams,
        byte notificationType,
        DateTime? lastCheckTime,
        DateTime? lastScanDate,
        string serviceName);
}

public sealed class PlayerHistoryService : IPlayerHistoryService
{
    private readonly IDynamicRepository dynamicRepository;

    public PlayerHistoryService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public Task Insert(
        IEnumerable<ReachPlayerHistoryParam> reachPlayerHistoryParams,
        byte notificationType,
        DateTime? lastCheckTime,
        DateTime? lastScanDate,
        string serviceName)
        => dynamicRepository.ExecuteAsync(new ReachPlayerHistoryCommand(
            BuildCustReachedXml(reachPlayerHistoryParams),
            notificationType,
            lastCheckTime,
            lastScanDate,
            serviceName));

    private static string BuildCustReachedXml(IEnumerable<ReachPlayerHistoryParam> reachPlayerHistoryParams)
    {
        var xmlBuilder = new StringBuilder();
        xmlBuilder.Append("<CustIds>");

        foreach (var item in reachPlayerHistoryParams)
        {
            item.AppendCustXml(xmlBuilder);
        }

        xmlBuilder.Append("</CustIds>");
        return xmlBuilder.ToString();
    }
}