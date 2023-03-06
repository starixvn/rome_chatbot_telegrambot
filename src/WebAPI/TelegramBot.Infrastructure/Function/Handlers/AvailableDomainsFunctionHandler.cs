using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Function.Models;
using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Function.Criteria;
using TelegramBot.Infrastructure.Bot.Models;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public class AvailableDomainsFunctionHandler : BaseFunctionHandler, IFunctionHandler
    {
        private readonly IDynamicRepository dynamicRepository;

        public AvailableDomainsFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IDynamicRepository dynamicRepository)
            : base(messageBuilder, messageFormatter)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public async Task<Result> Handle(CustBotAccount account, string[] parameters, BotEntity bot)
        {
            var siteLabel = bot.System;
            var criteria = new GetGetBackupDomainsCriteria(siteLabel);
            var data = await dynamicRepository.FetchAsync<BackupDomainModel>(criteria);

            var textContainer = new List<string>();

            foreach (var d in data)
            {
                textContainer.Add(d.Domain);
            }

            return new Result(0, string.Join(messageFormatter.NewLine(), textContainer));
        }

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => Enumerable.Empty<string>();
    }
}