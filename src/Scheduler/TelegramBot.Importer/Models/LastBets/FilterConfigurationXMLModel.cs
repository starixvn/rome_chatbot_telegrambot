using System.Xml.Serialization;

namespace TelegramBot.Importer.Models.LastBets;

[XmlRoot(ElementName = "Filter")]
public record FilterConfigurationXMLModel
{
    [XmlAttribute(AttributeName = "b")]
    public string BetType { get; set; }

    [XmlAttribute(AttributeName = "st")]
    public int Stake { get; set; }

    [XmlAttribute(AttributeName = "members")]
    public string Members { get; set; }

    [XmlAttribute(AttributeName = "wl")]
    public string MyWatchlistMembers { get; set; }

    [XmlAttribute(AttributeName = "iWL")]
    public byte FilterByMembers { get; set; }

    [XmlAttribute(AttributeName = "iL")]
    public byte LiveStatus { get; set; }

    [XmlElement(ElementName = "Sport")]
    public List<SportConfigurationXMLModel> Sports { get; set; }

    [XmlIgnore]
    public IEnumerable<string> BetTypeNames { get; set; }

    [XmlIgnore]
    public IEnumerable<SportModel> FlatternSports { get; set; }

    [XmlIgnore]
    public bool IsAllSports => !FlatternSports.Any();
}
