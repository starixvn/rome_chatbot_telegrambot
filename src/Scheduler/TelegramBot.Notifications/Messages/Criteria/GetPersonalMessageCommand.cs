using Fanex.Data.Repository;

namespace Alpha.Notifications.Messages.Criteria;

public class GetPersonalMessageCommand : NonQueryCommand
{
    public GetPersonalMessageCommand(int msgTargetId, int batchSize, bool isTest)
    {
        MsgTargetId = msgTargetId;
        BatchSize = batchSize;
        IsTest = isTest;
    }

    public int MsgTargetId { get; }

    public int BatchSize { get; }

    public bool IsTest { get; }

    public int MaxMsgTargetId { get; set; }

    public override string GetSettingKey() => "Age_Chatbot_PersonalMessage_Sel";

    public override bool IsValid() => BatchSize > 0;
}