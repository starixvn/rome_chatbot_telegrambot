using System.Threading.Tasks;
using NSubstitute;
using TelegramBot.Infrastructure.Action.Builders.ActionName;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Resource;

namespace TelegramBot.Infrastructure.UnitTests.Action.Builders.ActionName;

public class UpdateLanguageActionNameTests
{
    private readonly IResourceService resourceService;
    private readonly UpdateLanguageActionName updateLanguageActionName;

    public UpdateLanguageActionNameTests()
    {
        resourceService = Substitute.For<IResourceService>();
        updateLanguageActionName = new UpdateLanguageActionName(resourceService);
    }
}