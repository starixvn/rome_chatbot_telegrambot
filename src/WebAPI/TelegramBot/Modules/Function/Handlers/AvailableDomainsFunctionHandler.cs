using Fanex.Data.Repository;
using TelegramBot.Shared.Models;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Function.Criteria;
using TelegramBot.Modules.Function.Models;

namespace TelegramBot.Modules.Function.Handlers
{
    public abstract class AvailableDomainsFunctionHandler : BaseFunctionHandler, IFunctionHandler
    {
        private readonly IDynamicRepository dynamicRepository;

        protected AvailableDomainsFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IDynamicRepository dynamicRepository)
            : base(messageBuilder, messageFormatter)
            => this.dynamicRepository = dynamicRepository;

        public virtual async Task<Result> Handle(CustBotAccount account, string[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return Result.CreateErrorResult();
            }

            var isAgency = parameters[0] == "1";
            var data =
                (await dynamicRepository.FetchAsync<BackupDomainModel>(new GetGetBackupDomainsCriteria(account.Site)))
                .Where(d => d.IsAgency == isAgency)
                .Select(d => d.Domain)
                .ToList();
            var message = data.Count > 0
                ? string.Join(messageFormatter.NewLine(), data)
                : messageBuilder.BuildNoDataMessage(account.Language);

            return new Result(0, message);
        }

        public Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => Task.FromResult(Enumerable.Empty<string>());
    }
}