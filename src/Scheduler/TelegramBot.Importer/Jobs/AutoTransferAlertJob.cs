using System.Text;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Models.AutoTransfer;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;
using TelegramBot.Importer.Extensions;

namespace TelegramBot.Importer.Jobs;

public sealed class AutoTransferAlertJob : NotificationBaseJob
{
    private readonly IAutoTransferService service;

    public AutoTransferAlertJob(
        IAutoTransferService service,
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
        var siteDBs = job.SiteDBs.Split("," , StringSplitOptions.RemoveEmptyEntries);

        foreach(var siteDB in siteDBs) {
            var autoTransferResult = await service.GetAutoTransferResults(siteDB);
            if (!autoTransferResult.Any())
            {
                continue;
            }
            
            var custIds = autoTransferResult.Select(a => a.CustId).Distinct().JoinWithComma();
            var linkedInfo = await customerService.GetLinkedCustomers(FunctionKey.AutoTransferAlert, custIds, job.Environment);
            var notifications = new List<AutoTransferAlert>();

            foreach (var autoTransfer in autoTransferResult)
            {
                var linkedCusts = linkedInfo.Where(l => l.CustId == autoTransfer.CustId);
                foreach (var cust in linkedCusts)
                {
                    var message = BuildMessage(autoTransfer, cust.Language);
                    var n = new AutoTransferAlert(
                        cust.UserId,
                        message,
                        cust.BotId,
                        cust.IsRealUser,
                        job.IsUAT);

                    notifications.Add(n);
                }
            }

            if(notifications.Any()) {
                var result = await service.Import(notifications, job.Environment);

                if (result.IsFailed)
                {
                    await loggerService.Error(job, result.Message);
                }
            }
        }

        await UpdateJobStatusToCompleted(job, 0);
    }

    private string BuildMessage(AutoTransferResultModel autoTransfer, string language)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitleWithEmoji(language, nameof(Template.AutoTransfer_Title), EmojiUnicode.Envelope));

        messageBuilder.AppendLine(TemplateHelper.Get(nameof(Template.AutoTransfer_Header), language));

        messageBuilder.Append("<pre>");
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.AutoTransfer_SuccessCount), autoTransfer.SuccessCount.ToString(), language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.AutoTransfer_FailedCount), autoTransfer.FailedCount.ToString(), language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.AutoTransfer_CancelledCount), autoTransfer.CancelledCount.ToString(), language));
        messageBuilder.Append("</pre>");

        if(!string.IsNullOrWhiteSpace(autoTransfer.FailedCusts)) {
            messageBuilder.AppendLine();
            messageBuilder.AppendLine(TemplateHelper.Get(nameof(Template.AutoTransfer_FailedCusts), language));
            messageBuilder.Append("<pre>");
            messageBuilder.AppendLine(autoTransfer.FailedCusts);
            messageBuilder.Append("</pre>");
        }

        if(!string.IsNullOrWhiteSpace(autoTransfer.CancelledCusts)) {
            messageBuilder.AppendLine();
            messageBuilder.AppendLine(TemplateHelper.Get(nameof(Template.AutoTransfer_CancelledCusts), language));
            messageBuilder.Append("<pre>");
            messageBuilder.AppendLine(autoTransfer.CancelledCusts);
            messageBuilder.Append("</pre>");
        }

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }
}