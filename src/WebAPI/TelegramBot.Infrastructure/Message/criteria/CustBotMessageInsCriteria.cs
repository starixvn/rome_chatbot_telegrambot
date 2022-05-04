using Fanex.Data.Repository;

namespace TelegramBot.Infrastructure.Message.Criteria
{
    public class CustBotMessageInsCriteria : INonQueryCommand
    {
        public CustBotMessageInsCriteria(int sender, int receiver, long chatId, int botId, short functionId, string message, byte status)
        {
            Sender = sender;
            Receiver = receiver;
            ChatId = chatId;
            BotId = botId;
            FunctionId = functionId;
            Message = message;
            Status = status;
        }

        public int Sender { get; }

        public int Receiver { get; }

        public long ChatId { get; }

        public int BotId { get; }

        public short FunctionId { get; }

        public string Message { get; }

        public byte Status { get; }

        public string GetSettingKey() => "CustBotMessageIns";

        public bool IsValid() => !string.IsNullOrEmpty(Message);
    }
}
