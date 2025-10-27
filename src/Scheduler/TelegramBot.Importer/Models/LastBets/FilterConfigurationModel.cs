using System.Xml.Serialization;
using TelegramBot.Importer.Constants.LastBets;
using TelegramBot.Importer.Extensions;

namespace TelegramBot.Importer.Models.LastBets;

public record FilterConfigurationModel
{
    private FilterConfigurationXMLModel filterXml;

    public FilterConfigurationModel()
    {
    }

    public FilterConfigurationModel(
        int filterId,
        string filterName,
        string filterValue,
        FilterConfigurationXMLModel filterXml,
        bool isDefault,
        bool isNotify,
        DateTime createdDate,
        string watchList,
        string inputList)
    {
        this.filterXml = filterXml;
        FilterId = filterId;
        FilterName = filterName;
        FilterValue = filterValue;
        IsDefault = isDefault;
        IsNotify = isNotify;
        CreatedDate = createdDate;
        WatchList = watchList;
        InputList = inputList;
    }

    public int FilterId { get; }

    public string FilterName { get; }

    public string FilterValue { get; }

    public bool IsDefault { get; }

    public bool IsNotify { get; }

    public DateTime CreatedDate { get; }

    public string WatchList { get; }

    public string InputList { get; }

    public FilterConfigurationXMLModel FilterXml
    {
        get
        {
            filterXml = filterXml ?? Deserialize(FilterValue);

            return filterXml;
        }
    }

    private static FilterConfigurationXMLModel Deserialize(string xml)
    {
        var serializer = new XmlSerializer(typeof(FilterConfigurationXMLModel));

        using (TextReader reader = new StringReader(xml))
        {
            return (FilterConfigurationXMLModel)serializer.Deserialize(reader);
        }
    }

    public string Members
    {
        get
        {
            switch (FilterXml.FilterByMembers)
            {
                case FilterByMembersConstants.AllMembers:
                    return FilterXml.Members;
                case FilterByMembersConstants.AllWatchlistMembers:
                    return FilterXml.MyWatchlistMembers;
                default:
                    return $"{FilterXml.Members},{FilterXml.MyWatchlistMembers}".Trim(',');
            }
        }
    }

    public string Sports
    {
        get
        {
            var sportXmlList = FilterXml.Sports;
            if (sportXmlList.Any(s => string.Equals("0", s.Sport, StringComparison.OrdinalIgnoreCase)
                    && string.Equals("0", s.League, StringComparison.OrdinalIgnoreCase)))
            {
                return "0";
            }

            var sportTypes = new HashSet<string>();
            foreach (var item in sportXmlList)
            {
                foreach (var sport in item.Sport.SplitComma())
                {
                    if (item.League.Equals("0", StringComparison.OrdinalIgnoreCase))
                    {
                        sportTypes.Add(sport);
                    }
                }
            }

            return string.Join(",", sportTypes);
        }
    }

    public string Leagues
    {
        get
        {
            var sportXmlList = FilterXml.Sports;
            if (sportXmlList.Any(s => string.Equals("0", s.Sport, StringComparison.OrdinalIgnoreCase)
                    && string.Equals("0", s.League, StringComparison.OrdinalIgnoreCase)))
            {
                return "0";
            }

            var leagues = new HashSet<string>();
            foreach (var item in sportXmlList)
            {
                foreach (var sport in item.Sport.SplitComma())
                {
                    if (item.League.Equals("0", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    foreach (var league in item.League.SplitComma())
                    {
                        leagues.Add(league);
                    }
                }
            }

            return string.Join(",", leagues);
        }
    }
}
