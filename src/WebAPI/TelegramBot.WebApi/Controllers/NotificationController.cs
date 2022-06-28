using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using TelegramBot.Infrastructure.Notification;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.WebApi.Params;

namespace TelegramBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService notificationService;

        public NotificationController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        [HttpPost("change-password-alert")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> NotifyChangePassword([FromBody] AlertParam param)
        {
            return await notificationService.NotifyChangePassword(param.CustId, param.SubAccId);
        }

        [HttpPost("change-security-code-alert")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> NotifyChangeSecurityCode([FromBody] AlertParam param)
        {
            return await notificationService.NotifyChangeSecurityCode(param.CustId, param.SubAccId);
        }


        [HttpPost("{userId}/unlink")]
        public async Task<Result> Unlink(int userId, [FromBody] UnlinkNotificationParam param)
        {
            return await notificationService.NotifyUnlink(userId, param.ChatId, param.BotId, param.Language);
        }
    }
}