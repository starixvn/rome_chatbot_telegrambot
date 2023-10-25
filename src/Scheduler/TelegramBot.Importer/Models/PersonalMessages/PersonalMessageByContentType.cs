namespace TelegramBot.Importer.Models.PersonalMessages;

public record PersonalMessageByContentType(
    IEnumerable<PersonalMessage> NormalMessages,
    string CustIds,
    IEnumerable<PersonalMessage> DomainMessages,
    string Sites
);
