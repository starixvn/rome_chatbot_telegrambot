using System.Security;
using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Extensions;

namespace TelegramBot.Importer.Services;

public interface IProblemAccountService
{
    string BuildMessage(string language, string usernames);
}

public class ProblemAccountService : IProblemAccountService
{
    private static readonly IDictionary<string, string> MessageTemplates = new Dictionary<string, string>
    {
        [LanguageCode.English] = "[Problem Account Alert - {0}]\nHaving new problem account(s): {1}\n{Please kindly be informed and have a check the details for your downline(s).}"
    };
    
    private readonly IDynamicRepository dynamicRepository;

    public ProblemAccountService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public string BuildMessage(string language, string usernames)
    {
        if (!MessageTemplates.TryGetValue(language, out var template))
        {
            template = MessageTemplates[LanguageCode.English];
        }

        return string.Format(template, DateTime.Today.ToSimpleDate(), usernames);
    }
}