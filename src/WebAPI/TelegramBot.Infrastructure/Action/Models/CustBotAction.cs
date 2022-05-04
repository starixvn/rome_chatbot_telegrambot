using System;

namespace TelegramBot.Infrastructure.Action.Models
{
    public class CustBotAction
    {

        public CustBotAction(
            long id,
            int userId,
            int actionId,
            string userResponse,
            string compiledCommand,
            DateTime createdDate)
        {
            Id = id;
            UserId = userId;
            ActionId = actionId;
            UserResponse = userResponse;
            CompiledCommand = compiledCommand;
            CreatedDate = createdDate;
        }

        public long Id { get; }

        public int UserId { get; }

        public int ActionId { get; }

        public string UserResponse { get; }

        public string CompiledCommand { get; }

        public DateTime CreatedDate { get; }
    }
}