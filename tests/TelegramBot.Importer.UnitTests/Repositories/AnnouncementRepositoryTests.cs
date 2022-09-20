using System.Data;
using System.Text;
using Alpha.Announcements.Models;
using Alpha.Announcements.Specs;
using Fanex.Data;
using Newtonsoft.Json;
using TelegramBot.Importer.Criteria.Announcement;
using TelegramBot.Importer.Repositories;
using TelegramBot.Importer.UnitTests.Helpers;

namespace TelegramBot.Importer.UnitTests.Repositories;

public class AnnouncementRepositoryTests
{
    private readonly IObjectDb objectDb;
    private readonly AnnouncementRepository announcementRepository;

    public AnnouncementRepositoryTests()
    {
        DbSettingProviderManager
            .StartTestingSession()
            .Run();
        objectDb = Substitute.For<IObjectDb>();
        ObjectDbFactory.Mock(Substitute.For<CreateObjectDbFunc>());
        ObjectDbFactory.CreateInstance(Arg.Any<string>()).Returns(objectDb);
        announcementRepository = new AnnouncementRepository();
    }

    [Fact]
    public async Task GetAnnouncements_NullDataReturnFromDb_ReturnsDefaultData()
    {
        // Arrange
        var criteria = new GetAnnouncementCriteria(DateTime.Now, DateTime.Now, string.Empty);
        objectDb.ExecuteReaderAsync(Arg.Any<GetAnnouncementCriteria>()).Returns(Task.FromResult<IDataReader>(null));
        var expected = new List<Announcement>();

        // Act
        var actual = await announcementRepository.GetAnnouncements(criteria);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetAnnouncements_HasData_ReturnsAnnouncements()
    {
        // Arrange
        var criteria = new GetAnnouncementCriteria(DateTime.Now, DateTime.Now, string.Empty);
        var dataSet = FileHelper.LoadDataSet("/Data/Announcements.xml");
        objectDb.ExecuteReaderAsync(Arg.Any<GetAnnouncementCriteria>()).Returns(Task.FromResult<IDataReader>(dataSet.CreateDataReader()));
        var encodedContents = Convert.FromBase64String("W3siTGFuZ3VhZ2VLZXkiOiJlbi1VUyIsIlRleHQiOiJhIiwiSXNEaXNwbGF5aW5nQnlVcmwiOmZhbHNlLCJVcmwiOiIifSx7Ikxhbmd1YWdlS2V5IjoidmktVk4iLCJUZXh0IjoiIiwiSXNEaXNwbGF5aW5nQnlVcmwiOmZhbHNlLCJVcmwiOiIifSx7Ikxhbmd1YWdlS2V5IjoiemgtQ04iLCJUZXh0IjoiIiwiSXNEaXNwbGF5aW5nQnlVcmwiOmZhbHNlLCJVcmwiOiIifSx7Ikxhbmd1YWdlS2V5IjoiemgtVFciLCJUZXh0IjoiIiwiSXNEaXNwbGF5aW5nQnlVcmwiOmZhbHNlLCJVcmwiOiIifSx7Ikxhbmd1YWdlS2V5IjoidGgtVEgiLCJUZXh0IjoiIiwiSXNEaXNwbGF5aW5nQnlVcmwiOmZhbHNlLCJVcmwiOiIifSx7Ikxhbmd1YWdlS2V5Ijoia28tS1IiLCJUZXh0IjoiIiwiSXNEaXNwbGF5aW5nQnlVcmwiOmZhbHNlLCJVcmwiOiIifSx7Ikxhbmd1YWdlS2V5IjoiamEtSlAiLCJUZXh0IjoiIiwiSXNEaXNwbGF5aW5nQnlVcmwiOmZhbHNlLCJVcmwiOiIifV0=");
        var plainText = Encoding.UTF8.GetString(encodedContents);
        var expected = new List<Announcement>(){
            new Announcement{
                Id=1,
                Code="72973",
                Contents=JsonConvert.DeserializeObject<IEnumerable<MessageContent>>(plainText),
                Environments=new List<string>{"Local"},
                IsActive=true,
                Specifications=Enumerable.Empty<ISpecification<IParam>>(),
                Systems=new List<string>{"IBCBet"},
                Title="Add 4 New Sport Type and 5 bettype",
                ReleasedDate=DateTime.Parse("2018-08-29 00:00:00.000"),
                ExpiredDate=DateTime.Parse("2018-08-29 00:00:00.000"),
                CreatedDate=DateTime.Parse("2018-08-29 00:00:00.000")
            }
        };

        // Act
        var actual = await announcementRepository.GetAnnouncements(criteria);

        // Assert
        Assert.Equal(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(actual));
    }
}

