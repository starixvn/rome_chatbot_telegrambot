using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.Announment;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Announcement;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Repositories;
using TelegramBot.Importer.Resources;

namespace TelegramBot.Importer.Services;

public interface IAnnouncementService
{
    Task<IEnumerable<Announcement>> GetAnnouncements(DateTime fromDate, DateTime toDate, string environment);

    Task<IEnumerable<SystemNotification>> FilterAnnouncementsByCustomer(LinkedCustomer customer, IEnumerable<Announcement> announcements, string language, bool isUAT);

    Task<Result> Import(IEnumerable<SystemNotification> notifications, string environment, DateTime lastQueryTime);
}

public class AnnouncementService : NotificationServiceBase, IAnnouncementService
{
    private const string TabChar = "\t";
    private static readonly string[] NewLineHtmlTags = { "<br>", "<br />" };
    private static readonly string[] ParagraphHtmlTags = { "</p>", "</ p>", "< /p>" };
    private static readonly IDictionary<int, string> SystemMapper = new Dictionary<int, string>
    {
        [1] = "B-IBCBet",
        [7] = "B-789Y",
        [11] = "B-IBCIndo"
    };
    private readonly IAnnouncementRepository announcementRepository;
    private readonly IProductService productService;
    private readonly ICustomerService customerService;

    public AnnouncementService(
        IAnnouncementRepository announcementRepository,
        IProductService productService,
        ICustomerService customerService,
        IDynamicRepository dynamicRepository)
        : base(dynamicRepository)
    {
        this.announcementRepository = announcementRepository;
        this.productService = productService;
        this.customerService = customerService;
    }

    public async Task<IEnumerable<Announcement>> GetAnnouncements(DateTime fromDate, DateTime toDate, string environment)
        => await announcementRepository.GetAnnouncements(new GetAnnouncementCriteria(fromDate, toDate, environment));

    public async Task<IEnumerable<SystemNotification>> FilterAnnouncementsByCustomer(LinkedCustomer customer, IEnumerable<Announcement> announcements, string language, bool isUAT)
    {
        var custInfo = await customerService.GetCustomerInfo(customer.CustId);
        var system = SystemMapper[custInfo.SiteCode];
        var offeredProductIds = await productService.GetOfferedProductIdsByCustomer(customer.CustId);
        var specificationParam = new SpecificationParam(custInfo, customer.SubAccId > 0, offeredProductIds);
        var result = new List<SystemNotification>();

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
            .AppendLine(TemplateHelper.BuildTitle(language, "SystemAnnouncement", DateTime.Now.ToString("yyyyMMdd")))
            .Append(messageContent.StripHTML().Replace(TabChar, string.Empty));

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private static string GetMessageContentByLanguage(IEnumerable<MessageContent> messages, string languageKey)
    {
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

    public Task<Result> Import(IEnumerable<SystemNotification> notifications, string environment, DateTime lastQueryTime)
        => ExecuteNonQuery(new ImportAnnouncementCommand(BuildXmlData(notifications), environment, lastQueryTime));
}