using System.Text;
using Alpha.Announcements.Models;
using Alpha.Announcements.Specs;
using Microsoft.Extensions.Configuration;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Announcement;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class SystemAnnouncementJob : NotificationBaseJob
{
    private const string TabChar = "\t";
    private static readonly string[] NewLineHtmlTags = { "<br>", "<br />" };
    private static readonly string[] ParagraphHtmlTags = { "</p>", "</ p>", "< /p>" };
    private static readonly IDictionary<string, string> SystemMapper = new Dictionary<string, string>
    {
        ["IBC"] = "Bot-IBC",
        ["789y"] = "Bot-789Y",
        ["MBC"] = "Bot-MBC",
        ["TH7a5"] = "Bot-TH7a5"
    };
    private readonly IAnnouncementService announcementService;
    private readonly IProductService productService;
    private readonly string environment;

    public SystemAnnouncementJob(
        IConfiguration configuration,
        IAnnouncementService announcementService,
        IProductService productService,
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
    {
        this.announcementService = announcementService;
        this.productService = productService;
        environment = configuration["Environment"];
    }

    public override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var notifications = new List<SystemNotification>();
        var now = DateTime.Now;
        var announcements = await announcementService.GetAnnouncements(systemSetting.LastQueryTime.AddSeconds(1), now, environment);
        if (announcements.Any())
        {
            var linkedCustomers = await customerService.GetLinkedCustomersByFunction(job.Name, job.Environment);
            if (linkedCustomers.Any())
            {
                var customerStatus = await GetCustomerStatus(linkedCustomers);
                var customers = from linkedCustomer in linkedCustomers
                                join status in customerStatus
                                on new { linkedCustomer.CustId, linkedCustomer.SubAccId } equals new { status.CustId, status.SubAccId }
                                where status.IsValid(job.SubAccPermission)
                                select linkedCustomer;

                foreach (var customer in customers)
                {
                    notifications.AddRange(await FilterAnnouncementsByCustomer(
                        customer,
                        announcements,
                        customer.Language,
                        job.IsUAT));
                }
            }
        }

        var result = await announcementService.Import(notifications, job.Environment, now);

        if (result.IsFailed)
        {
            await loggerService.Error(job, result.Message);
        }
    }

    private async Task<IEnumerable<SystemNotification>> FilterAnnouncementsByCustomer(LinkedCustomer customer, IEnumerable<Announcement> announcements, string language, bool isUAT)
    {
        var result = new List<SystemNotification>();
        var custInfo = await customerService.GetCustomerInfo(customer.CustId);
        if (!string.IsNullOrEmpty(custInfo.SiteLabel))
        {
            var system = SystemMapper[custInfo.SiteLabel];
            var offeredProductIds = await productService.GetOfferedProductIdsByCustomer(customer.CustId);
            var specificationParam = new SpecificationParam(custInfo, customer.SubAccId > 0, offeredProductIds);

            foreach (var announcement in announcements.Where(a => a.Systems.Contains(system, StringComparer.OrdinalIgnoreCase)))
            {
                if (announcement.Specifications == null
                    || !announcement.Specifications.Any()
                    || IsSatisfied(announcement.Specifications, specificationParam))
                {
                    result.Add(new SystemNotification(
                        announcement.Id,
                        customer.UserId,
                        customer.BotId,
                        BuildMessage(language, announcement.Contents),
                        customer.IsRealUser,
                        isUAT));
                }
            }
        }

        return result;
    }

    private static string BuildMessage(string language, IEnumerable<MessageContent> contents)
    {
        var messageBuilder = new StringBuilder();
        var messageContent = GetMessageContentByLanguage(contents, language);
        foreach (var tag in ParagraphHtmlTags)
        {
            messageContent = messageContent.Replace(tag, tag + Environment.NewLine);
        }

        foreach (var tag in NewLineHtmlTags)
        {
            messageContent = messageContent.Replace(tag, Environment.NewLine);
        }

        messageBuilder
            .AppendLine(TemplateHelper.BuildTitleWithEmoji(language, "SystemAnnouncement", EmojiUnicode.Envelope))
            .Append(messageContent.StripHTML().Replace(TabChar, string.Empty));

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private static string GetMessageContentByLanguage(IEnumerable<MessageContent> messages, string language)
    {
        var languageKey = LanguageCode.LanguageMapper.ContainsKey(language) ? LanguageCode.LanguageMapper[language] : language;
        var message = messages.FirstOrDefault(i => i.LanguageKey.Equals(languageKey) && !string.IsNullOrEmpty(i.Text));
        message ??= messages.FirstOrDefault(i => i.LanguageKey.Equals(LanguageCode.English) && !string.IsNullOrEmpty(i.Text));

        return message == null ? string.Empty : message.Text;
    }

    private static bool IsSatisfied(IEnumerable<ISpecification<IParam>> specifications, IParam param)
    {
        if (specifications == null || !specifications.Any())
        {
            return true;
        }

        var accumulateSpecification = specifications.First();
        var totalSpec = specifications.Count();
        for (int i = 1; i < totalSpec; i++)
        {
            var nextSpec = specifications.ElementAt(i);
            accumulateSpecification = nextSpec.ConjunctionOperator == ConjunctionOperator.Or
                   ? accumulateSpecification.Or(nextSpec)
                   : accumulateSpecification.And(nextSpec);
        }

        return accumulateSpecification.IsSatisfiedBy(param);
    }
}