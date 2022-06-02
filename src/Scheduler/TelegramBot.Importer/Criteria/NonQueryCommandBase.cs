using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria;

public class NonQueryCommandBase : NonQueryCommand
{
    protected NonQueryCommandBase(string xmlData, string environment)
    {
        XmlData = xmlData;
        Environment = environment;
    }

    public string XmlData { get; }

    public string Environment { get; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = default!;

    public override string GetSettingKey()
        => throw new NotImplementedException();

    public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
}