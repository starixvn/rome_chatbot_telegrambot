using System.Xml.Serialization;

namespace TelegramBot.Importer.Models.LastBets;

[Serializable]
public record SportConfigurationXMLModel
{
    [XmlAttribute(AttributeName = "s")]
    public string Sport { get; set; }

    [XmlAttribute(AttributeName = "l")]
    public string League { get; set; }
}