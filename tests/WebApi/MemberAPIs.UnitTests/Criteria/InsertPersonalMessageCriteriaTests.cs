using FluentAssertions;
using MemberAPIs.Criteria;
using Xunit;

namespace MemberAPIs.UnitTests.Criteria;

public class InsertPersonalMessageCriteriaTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        string contentEN = "Hello in English";
        string contentCH = "Hello in Chinese";
        string contentCS = "Hello in Czech";
        string contentTH = "Hello in Thai";
        string contentVN = "Hello in Vietnamese";
        string contentJP = "Hello in Japanese";
        string contentKO = "Hello in Korean";

        // Act
        var criteria = new InsertPersonalMessageCriteria(
            contentEN,
            contentCH,
            contentCS,
            contentTH,
            contentVN,
            contentJP,
            contentKO);

        // Assert
        criteria.ContentEN.Should().Be(contentEN);
        criteria.ContentCH.Should().Be(contentCH);
        criteria.ContentCS.Should().Be(contentCS);
        criteria.ContentTH.Should().Be(contentTH);
        criteria.ContentVN.Should().Be(contentVN);
        criteria.ContentJP.Should().Be(contentJP);
        criteria.ContentKO.Should().Be(contentKO);
        criteria.ContentIT.Should().Be(string.Empty);
        criteria.ContentES.Should().Be(string.Empty);
        criteria.MessageType.Should().Be(1);
        criteria.CreatorName.Should().Be(string.Empty);
        criteria.IsBatch.Should().BeTrue();
    }

    [Fact]
    public void IsValid_AlwaysReturnsTrue()
    {
        // Arrange
        var criteria = new InsertPersonalMessageCriteria(
            "EN", "CH", "CS", "TH", "VN", "JP", "KO");

        // Act
        bool result = criteria.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetSettingKey_ReturnsCorrectValue()
    {
        // Arrange
        var criteria = new InsertPersonalMessageCriteria(
            "EN", "CH", "CS", "TH", "VN", "JP", "KO");

        // Act
        string settingKey = criteria.GetSettingKey();

        // Assert
        settingKey.Should().Be("Age_Msg_SetMessage");
    }

    [Fact]
    public void MessageIdOutput_CanBeSetAndRetrieved()
    {
        // Arrange
        var criteria = new InsertPersonalMessageCriteria(
            "EN", "CH", "CS", "TH", "VN", "JP", "KO");
        int messageId = 12345;

        // Act
        criteria.MessageIdOutput = messageId;

        // Assert
        criteria.MessageIdOutput.Should().Be(messageId);
    }
}