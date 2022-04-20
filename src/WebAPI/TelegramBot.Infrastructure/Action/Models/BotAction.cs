namespace TelegramBot.Infrastructure.Action.Models
{
    public class BotAction
    {
        public BotAction(
            short actionId,
            string actionName,
            byte type,
            string command,
            short functionId,
            short parentId,
            byte status)
        {
            ActionId = actionId;
            ActionName = actionName;
            Type = type;
            Command = command;
            FunctionId = functionId;
            ParentId = parentId;
            Status = status;
        }

        public short ActionId { get; }

        public string ActionName { get; }

        public byte Type { get; }

        public string Command { get; }

        public short FunctionId { get; }

        public short ParentId { get; }

        public byte Status { get; }
    }
}