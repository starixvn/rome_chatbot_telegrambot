using TelegramBot.Importer.Constants;

namespace TelegramBot.Importer.Models.PersonalMessages;

public record PersonalMessage
{
    public PersonalMessage()
    {
    }

#pragma warning disable S107
    public PersonalMessage(
        int msgTargetId,
        int messageId,
        string contentEN,
        string contentCS,
        string contentCH,
        string contentTH,
        string contentVI,
        int targetId,
        string betTransId,
        int msgContentType,
        string remark)
#pragma warning restore S107
    {
        MsgTargetId = msgTargetId;
        MessageId = messageId;
        ContentEN = contentEN;
        ContentCS = contentCS;
        ContentCH = contentCH;
        ContentTH = contentTH;
        ContentVI = contentVI;
        TargetId = targetId;
        BetTransId = betTransId;
        MsgContentType = msgContentType;
        Remark = remark;
    }

    public int MsgTargetId { get; }

    public int MessageId { get; }

    public string ContentEN { get; } = default!;

    public string ContentCS { get; } = default!;

    public string ContentCH { get; } = default!;

    public string ContentTH { get; } = default!;

    public string ContentVI { get; } = default!;

    public int TargetId { get; }

    public string BetTransId { get; } = default!;

    public int MsgContentType { get; }

    public string Remark { get; } = default!;

    public string GetContentByLanguage(string language)
        => language switch
        {
            LanguageCode.ChineseSimplified => GetContent(ContentCS, ContentEN),
            LanguageCode.ChineseTraditional => GetContent(ContentCH, ContentEN),
            LanguageCode.Thai => GetContent(ContentTH, ContentEN),
            LanguageCode.Vietnamese => GetContent(ContentVI, ContentEN),
            _ => ContentEN
        };

    private static string GetContent(string content, string defaultContent)
        => string.IsNullOrWhiteSpace(content) ? defaultContent : content;
}
