using System.Text;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.LastBets;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class LastBetsAlertJob : NotificationBaseJob
{
    private readonly ILastBetsService service;

    public LastBetsAlertJob(
        ILastBetsService service,
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
    {
        this.service = service;
    }

    public override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var linkedCusts= await customerService.GetLinkedCustomersByFunction(FunctionKey.LastBetsAlert, job.Environment);
            
        var lastTransIds = await service.GetLastTransIds(job.Environment);
        var newLastTransIds = new Dictionary<string, long>();

        var notifications = new List<LastBetsNotificationModel>();

        foreach(var linkedCust in linkedCusts) {
           var lastBets = await GetCustomerLastBets(linkedCust, lastTransIds, newLastTransIds);
           if(!lastBets.Any()) {
            continue;
           }

           var message = BuildMessage(lastBets, linkedCust.Language);
            var n = new LastBetsNotificationModel(
                linkedCust.UserId,
                message,
                linkedCust.BotId,
                linkedCust.IsRealUser,
                job.IsUAT);

            notifications.Add(n);
        }

        if(lastTransIds != null && notifications.Any()) {
            var result = await service.Import(notifications, job.Environment);

            if (result.IsFailed)
            {
                await loggerService.Error(job, result.Message);
            }
        }

        await service.UpdateLastTransIds(newLastTransIds, job.Environment);
        await UpdateJobStatusToCompleted(job, 0);
    }

    public async Task<IEnumerable<LastBetsModel>> GetCustomerLastBets(
        LinkedCustomer linkedCust,
        Dictionary<string, long> lastTransIds,
        Dictionary<string, long> newLastTransIds) {

        var filters = await service.GetFilterConfigurations(linkedCust.CustId, linkedCust.SubAccId);
        var result = new List<LastBetsModel>();

        foreach(var filter in filters) {
            var key = $"{linkedCust.CustId}_{linkedCust.SubAccId}_{filter.FilterId}";
            var lastTransId = lastTransIds != null && lastTransIds.ContainsKey(key) ? lastTransIds[key] : 0;
            var lastBets = await service.GetData(linkedCust.CustId, lastTransId, filter);

            if(lastTransIds.ContainsKey(key) && lastBets.TicketCount > 0) {
                result.Add(lastBets);
            }
            
            newLastTransIds.Add(key, lastBets.LastTransId);
        }

        return result;
    }

    private string BuildMessage(IEnumerable<LastBetsModel> lastBets, string language)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitleWithEmoji(language, nameof(Template.LastBets_Title), EmojiUnicode.Envelope));

        messageBuilder.AppendLine(TemplateHelper.Get(nameof(Template.LastBets_FiltersTitle), language));
        
        foreach(var lb in lastBets) {
            var left = lb.FilterName ?? string.Empty;
            var right =  string.Format(TemplateHelper.Get(nameof(Template.LastBets_FilterResult), language), lb.TicketCount) ?? string.Empty;
            messageBuilder.AppendLine(left + ": " + right);
        }

        messageBuilder.AppendLine(TemplateHelper.Get(nameof(Template.LastBets_ViewDetails), language));
       
        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }
}