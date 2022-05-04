using System;

namespace TelegramBot.Infrastructure.Action.Models
{
    public class CustBotAction
    {
        public CustBotAction(
            long logActionId,
            int userId,
            int actionId,
            string userResponse,
            string compiledCommand,
            DateTime createdDate,
            byte status)
        {
            LogActionId = logActionId;
            UserId = userId;
            ActionId = actionId;
            UserResponse = userResponse;
            CompiledCommand = compiledCommand;
            CreatedDate = createdDate;
            Status = status;
        }

        public long LogActionId { get; }

        public int UserId { get; }

        public int ActionId { get; }

        public string UserResponse { get; }

        public string CompiledCommand { get; }

        public DateTime CreatedDate { get; }

        public byte Status { get; }
    }
}