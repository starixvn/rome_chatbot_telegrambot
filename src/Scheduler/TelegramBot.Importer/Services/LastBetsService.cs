using System.Drawing;
using Fanex.Data.Repository;
using Newtonsoft.Json;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.LastBets;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.LastBets;
using TelegramBot.Importer.Models.SystemSettings;

namespace TelegramBot.Importer.Services;

public interface ILastBetsService
{
    Task<IEnumerable<FilterConfigurationModel>> GetFilterConfigurations(int custId, int subAccId);
    Task<LastBetsModel> GetData(int custId, long lastTransId, FilterConfigurationModel filter);
    Task<Result> Import(IEnumerable<LastBetsNotificationModel> notifications, string environment);
    Task<Dictionary<string, long>> GetLastTransIds(string env);
    Task UpdateLastTransIds(Dictionary<string, long> lastTransIds, string env);
}

public class LastBetsService : NotificationServiceBase, ILastBetsService
{
    public LastBetsService(IDynamicRepository dynamicRepository) : base(dynamicRepository) { }
    
    public async Task<IEnumerable<FilterConfigurationModel>> GetFilterConfigurations(int custId, int subAccId)
    {
        var criteria = new GetFiltersCriteria(custId, subAccId);
        var data = await dynamicRepository.FetchAsync<FilterConfigurationModel>(criteria);
        return data.Where(d => d.IsNotify);
    }

    public async Task<LastBetsModel> GetData(int custId, long lastTransId, FilterConfigurationModel filter) {
        var criteria = new GetLastBetsCriteria(custId, lastTransId, filter);
        await dynamicRepository.ExecuteAsync(criteria);

        return new LastBetsModel { 
            FilterName = filter.FilterName,
            TicketCount = criteria.BetCount,
            LastTransId = criteria.TransId
        };
    }

    public async Task<Result> Import(IEnumerable<LastBetsNotificationModel> notifications, string environment) {
        var command = new ImportLastBetsCommand(
            BuildXmlData(notifications),
            environment,
            FunctionKey.LastBetsAlert);

        var result = await ExecuteNonQuery(command);

        return result;
    }

    public async Task<Dictionary<string, long>> GetLastTransIds(string env) {
        var criteria = new GetLastTransIdsCriteria(env);
        var data = await dynamicRepository.FetchAsync<SystemSettingEntity>(criteria);
        var setting = data.FirstOrDefault();

        return setting?.Value == null
            ? new Dictionary<string, long>()
            : JsonConvert.DeserializeObject<Dictionary<string, long>>(setting.Value);
    }

    public async Task UpdateLastTransIds(Dictionary<string, long> lastTransIds, string env) {
        var command = new UpdateLastTransIdsCriteria(lastTransIds, env);
        await dynamicRepository.ExecuteAsync(command);
    }
}