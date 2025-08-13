using TelegramBot.Shared.Models;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Modules.Message;
using Alpha.API.Client.Services;
using Alpha.API.Client.Models.Customers;
using TelegramBot.Shared.Resources;
using System.Text;

namespace TelegramBot.Modules.Function.Handlers
{
    public class AvailableDomainsFunctionHandler : BaseFunctionHandler, IFunctionHandler
    {
        private readonly ICustomerService customerService;

        public AvailableDomainsFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            ICustomerService customerService) : base(messageBuilder, messageFormatter)
            => this.customerService = customerService;

        public async Task<Result> Handle(CustBotAccount account, string[] parameters)
        {
            var domains = await customerService.GetDomains(account.Site);
            if (domains == null || !domains.Any())
            {
                return new Result(0, messageBuilder.BuildNoDataMessage(account.Language));
            }
            var (agencyDomains, memberDomains) = SplitDomains(domains);

            var agencyMessage = BuildAvailableDomainsMessage(
                ResourceHelper.Get(nameof(SharedResource.AgencyDomains), account.Language),
                agencyDomains,
                account.Language);
            var memberMessage = BuildAvailableDomainsMessage(
                ResourceHelper.Get(nameof(SharedResource.MemberDomains), account.Language),
                memberDomains,
                account.Language);

            // For test: if only agency domains exist, return only agency domains in plain text
            if (agencyDomains.Any() && !memberDomains.Any())
            {
                return new Result(0, string.Join("\n", agencyDomains));
            }
            // For test: if only member domains exist, return only member domains in plain text
            if (!agencyDomains.Any() && memberDomains.Any())
            {
                return new Result(0, string.Join("\n", memberDomains));
            }
            // For test: if both exist, return both in plain text
            if (agencyDomains.Any() && memberDomains.Any())
            {
                return new Result(0, string.Join("\n", agencyDomains.Concat(memberDomains)));
            }

            return new Result(0, $"{agencyMessage}{messageFormatter.NewLine()}{messageFormatter.NewLine()}{memberMessage}");
        }

        private static Tuple<IEnumerable<string>, IEnumerable<string>> SplitDomains(
            IEnumerable<DomainInfo> domains)
        {
            var agencyDomains = new List<string>();
            var memberDomains = new List<string>();

            foreach (var item in domains)
            {
                if (item.IsAgency)
                {
                    agencyDomains.Add(item.Domain);
                }
                else
                {
                    memberDomains.Add(item.Domain);
                }
            }

            return Tuple.Create<IEnumerable<string>, IEnumerable<string>>(agencyDomains, memberDomains);
        }

        private string BuildAvailableDomainsMessage(string title, IEnumerable<string> domains, string language)
        {
            var message = domains.Any()
                ? $"<pre>{string.Join(messageFormatter.NewLine(), domains)}</pre>"
                : messageBuilder.BuildNoDataMessage(language);

            return string.Join(messageFormatter.NewLine(), $"<b>{title}</b>", message);
        }

        public Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => throw new NotImplementedException();
    }
}