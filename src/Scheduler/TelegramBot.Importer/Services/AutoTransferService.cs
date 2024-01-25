using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.AutoTransfer;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.AutoTransfer;

namespace TelegramBot.Importer.Services;

public interface IAutoTransferService
{
    Task<IEnumerable<AutoTransferResultModel>> GetAutoTransferResults(string siteDB);
    Task<Result> Import(IEnumerable<AutoTransferAlert> notifications, string environment);
}

public class AutoTransferService : NotificationServiceBase, IAutoTransferService
{
    public AutoTransferService(IDynamicRepository dynamicRepository) : base(dynamicRepository) { }

    public async Task<IEnumerable<AutoTransferResultModel>> GetAutoTransferResults(string siteDB)
    {
        var command = new GetAutoTransferResultCriteria(siteDB);
        var result = await dynamicRepository.FetchAsync<AutoTransferResultModel>(command);

        return result;
    }

    public async Task<Result> Import(IEnumerable<AutoTransferAlert> notifications, string environment)
    {
        var command = new ImportAutoTransferAlertCommand(
            BuildXmlData(notifications),
            environment,
            FunctionKey.AutoTransferAlert);

        var result = await ExecuteNonQuery(command);

        return result;
    }
}