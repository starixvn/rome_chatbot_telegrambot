using PortalAPIs.Criteria;
using PortalAPIs.Models;

namespace PortalAPIs.UnitTests.Criteria;

public class PrepareAbnormalLoginInsertCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnsCorrectKey()
    {
        // Arrange
        var criteria = new PrepareAbnormalLoginInsertCriteria(new List<AbnormalLoginModel>());

        // Act
        var result = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Acc_CustAbnormalLogin_PrepareIns", result);
    }

    [Fact]
    public void IsValid_WhenTransIsEmpty_ReturnsFalse()
    {
        // Arrange
        var criteria = new PrepareAbnormalLoginInsertCriteria(new List<AbnormalLoginModel>());

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenTransHasItems_ReturnsTrue()
    {
        // Arrange
        var trans = new List<AbnormalLoginModel>
        {
            new AbnormalLoginModel(
                ID: 1,
                LoginName: "testuser",
                TransTime: new DateTime(2024, 3, 20),
                UserAgent: "Mozilla/5.0",
                IP: "127.0.0.1",
                OS: "Windows",
                DeviceTypeName: "Desktop",
                Browser: "Chrome"
            )
        };
        var criteria = new PrepareAbnormalLoginInsertCriteria(trans);

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OrgBatch_WhenTransHasItems_ReturnsCorrectXml()
    {
        // Arrange
        var trans = new List<AbnormalLoginModel>
        {
            new AbnormalLoginModel(
                ID: 1,
                LoginName: "testuser",
                TransTime: new DateTime(2024, 3, 20),
                UserAgent: "Mozilla/5.0",
                IP: "127.0.0.1",
                OS: "Windows",
                DeviceTypeName: "Desktop",
                Browser: "Chrome"
            )
        };
        var criteria = new PrepareAbnormalLoginInsertCriteria(trans);

        // Act
        var result = criteria.OrgBatch;

        // Assert
        var expectedXml = "<ROOT><r TransID=\"1\" TransTime=\"3/20/2024 12:00:00 AM\" LoginName=\"testuser\" UserAgent=\"Mozilla/5.0\" IP=\"127.0.0.1\" Browser=\"Chrome\" Device=\"Desktop\" OS=\"Windows\" /></ROOT>";
        Assert.Equal(expectedXml, result);
    }

    [Fact]
    public void OrgBatch_WhenTransHasMultipleItems_ReturnsCorrectXml()
    {
        // Arrange
        var trans = new List<AbnormalLoginModel>
        {
            new AbnormalLoginModel(
                ID: 1,
                LoginName: "testuser1",
                TransTime: new DateTime(2024, 3, 20),
                UserAgent: "Mozilla/5.0",
                IP: "127.0.0.1",
                OS: "Windows",
                DeviceTypeName: "Desktop",
                Browser: "Chrome"
            ),
            new AbnormalLoginModel(
                ID: 2,
                LoginName: "testuser2",
                TransTime: new DateTime(2024, 3, 21),
                UserAgent: "Mozilla/5.0",
                IP: "192.168.1.1",
                OS: "Android",
                DeviceTypeName: "Mobile",
                Browser: "Firefox"
            )
        };
        var criteria = new PrepareAbnormalLoginInsertCriteria(trans);

        // Act
        var result = criteria.OrgBatch;

        // Assert
        var expectedXml = "<ROOT>" +
                         "<r TransID=\"1\" TransTime=\"3/20/2024 12:00:00 AM\" LoginName=\"testuser1\" UserAgent=\"Mozilla/5.0\" IP=\"127.0.0.1\" Browser=\"Chrome\" Device=\"Desktop\" OS=\"Windows\" />" +
                         "<r TransID=\"2\" TransTime=\"3/21/2024 12:00:00 AM\" LoginName=\"testuser2\" UserAgent=\"Mozilla/5.0\" IP=\"192.168.1.1\" Browser=\"Firefox\" Device=\"Mobile\" OS=\"Android\" />" +
                         "</ROOT>";
        Assert.Equal(expectedXml, result);
    }
}