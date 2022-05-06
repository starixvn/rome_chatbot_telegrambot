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
            byte status,
            bool isEndAction,
            short backToActionId)
        {
            ActionId = actionId;
            ActionName = actionName;
            Type = type;
            Command = command;
            FunctionId = functionId;
            ParentId = parentId;
            IsEndAction = isEndAction;
            Status = status;
            BackToActionId = backToActionId;
        }

        public short ActionId { get; }

        public string ActionName { get; }

        public byte Type { get; }

        public string Command { get; }

        public short FunctionId { get; }

        public short ParentId { get; }

        public bool IsEndAction { get; }

        public byte Status { get; }

        public short BackToActionId { get; }
    }
}