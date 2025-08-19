using System.Text;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Models.AbnormalLogin;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;
using TelegramBot.Importer.Extensions;
using UAParser.FormFactor;
using UAParser.FormFactor.Models;

namespace TelegramBot.Importer.Jobs;

public sealed class AbnormalLoginAlertJob : NotificationBaseJob
{
    private readonly IAbnormalLoginService abnormalLoginService;
    private readonly IUAParser uaParser;

    public AbnormalLoginAlertJob(
        IAbnormalLoginService abnormalLoginService,
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
    {
        this.abnormalLoginService = abnormalLoginService;
        uaParser = Parser.GetDefault();
    }

    public override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var (maxLogId, abnormalLogins) = await abnormalLoginService.GetAbnormalLogins(systemSetting.MaxLogIdLong);

        if (!abnormalLogins.Any())
        {
            return;
        }

        var custIds = abnormalLogins.Select(a => a.CustId).Distinct().JoinWithComma();
        var linkedInfo = await customerService.GetLinkedCustomers(FunctionKey.AbnormalLoginAlert, custIds, job.Environment);
        var linkedAbnormalLogins = abnormalLogins.Where(a => linkedInfo.Any(l => l.CustId == a.CustId && l.SubAccId == a.SubAccId)).ToList();
        if (!linkedAbnormalLogins.Any())
        {
            return;
        }

        var ips = linkedAbnormalLogins.Select(a => a.IP).Distinct();
        var ipInfo = await abnormalLoginService.GetIPInfo(ips);
        var defaultIPInfo = new IPInfoModel(string.Empty, "-", "-");
        var notifications = new List<AbnormalLoginNotification>();

        foreach (var login in linkedAbnormalLogins)
        {
            var linkedCust = linkedInfo.FirstOrDefault(l => l.CustId == login.CustId && l.SubAccId == login.SubAccId);

            if (linkedCust == null)
            {
                continue;
            }

            var ip = ipInfo.ContainsKey(login.IP) ? ipInfo[login.IP] : defaultIPInfo;
            login.City = string.IsNullOrWhiteSpace(ip.City) ? defaultIPInfo.City : ip.City;
            login.Country = string.IsNullOrWhiteSpace(ip.Country) ? defaultIPInfo.Country : ip.Country;

            var message = BuildMessage(login, linkedCust.Language);
            var n = new AbnormalLoginNotification(
                linkedCust.UserId,
                message,
                linkedCust.BotId,
                linkedCust.IsRealUser,
                job.IsUAT,
                login.LoginID);

            notifications.Add(n);
        }

        var result = await abnormalLoginService.Import(notifications, job.Environment, maxLogId);

        if (result.IsFailed)
        {
            await loggerService.Error(job, result.Message);
        }

        await UpdateJobStatusToCompleted(job, maxLogId);
    }

    private string BuildMessage(AbnormalLoginModel login, string language)
    {
        var c = uaParser.Parse(login.UserAgent);
        var device = string.IsNullOrWhiteSpace(login.Device) ? Enum.GetName(typeof(DeviceFormFactor), GetDeviceType(c)) : login.Device;

        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitleWithEmoji(language, nameof(Template.AbnormalLogin_Title), EmojiUnicode.Warning));

        var loginTime = DateTime.Parse(login.LoginTime);
        messageBuilder.AppendLine(string.Format(TemplateHelper.Get(nameof(Template.AbnormalLogin_Header), language), loginTime.ToLongTimeString()));

        messageBuilder.Append("<pre>");
        messageBuilder.AppendLine(string.Format(TemplateHelper.Get(nameof(Template.AbnormalLogin_Near), language), login.City, login.Country, login.IP));
        messageBuilder.AppendLine(string.Format(TemplateHelper.Get(nameof(Template.AbnormalLogin_Device), language), string.IsNullOrWhiteSpace(device) ? string.Empty : device + ", ", c.OS));

        var browser = string.IsNullOrWhiteSpace(login.Browser) ? c.UA.Family : login.Browser;
        messageBuilder.AppendLine(string.Format(TemplateHelper.Get(nameof(Template.AbnormalLogin_Browser), language), browser));
        messageBuilder.Append("</pre>");

        messageBuilder.Append(TemplateHelper.Get(nameof(Template.AbnormalLogin_Footer1), language));
        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private static DeviceFormFactor GetDeviceType(ClientInfo clientInfo)
    {
        if (clientInfo == null || clientInfo.Device == null)
        {
            return DeviceFormFactor.Unknown;
        }

        var osName = clientInfo.OS == null ? string.Empty : clientInfo.OS.Family;
        var browserName = clientInfo.UA == null ? string.Empty : clientInfo.UA.Family;

        if (clientInfo.Device.FormFactor == DeviceFormFactor.Unknown
            && osName == "Mac OS X" && browserName == "Safari")
        {
            return DeviceFormFactor.Desktop;
        }

        return clientInfo.Device.FormFactor;
    }
}