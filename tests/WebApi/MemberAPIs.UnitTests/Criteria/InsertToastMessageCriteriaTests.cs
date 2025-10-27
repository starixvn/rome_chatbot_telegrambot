using FluentAssertions;
using MemberAPIs.Criteria;
using MemberAPIs.Constants;
using MemberAPIs.Models;
using Newtonsoft.Json;
using Xunit;

namespace MemberAPIs.UnitTests.Criteria;

public class InsertToastMessageCriteriaTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        int targetId = 123;
        int creator = 456;
        string contentEN = "Failed login message in English";
        string contentCH = "Failed login message in Chinese";
        string contentCS = "Failed login message in Czech";
        string contentTH = "Failed login message in Thai";
        string contentVN = "Failed login message in Vietnamese";
        string contentJP = "Failed login message in Japanese";
        string contentKO = "Failed login message in Korean";

        // Act
        var criteria = new InsertToastMessageCriteria(
            targetId,
            creator,
            contentEN,
            contentCH,
            contentCS,
            contentTH,
            contentVN,
            contentJP,
            contentKO);

        // Assert
        criteria.TypeId.Should().Be(ToastMessageTypeConstants.Warning);
        criteria.SiteFunctionId.Should().Be(ToastMessageFunctionConstants.FailedLogin);
        criteria.TargetId.Should().Be(targetId);
        criteria.Creator.Should().Be(creator);
        criteria.ContentIT.Should().Be(string.Empty);
        criteria.ContentES.Should().Be(string.Empty);
        criteria.IsValid().Should().BeTrue();
        criteria.GetSettingKey().Should().Be("Age_ToastMessage_Ins");
    }

    [Fact]
    public void GetMessage_SerializesContentCorrectly()
    {
        // Arrange
        int targetId = 123;
        int creator = 456;
        string contentEN = "Failed login message in English";
        string contentCH = "Failed login message in Chinese";
        string contentCS = "Failed login message in Czech";
        string contentTH = "Failed login message in Thai";
        string contentVN = "Failed login message in Vietnamese";
        string contentJP = "Failed login message in Japanese";
        string contentKO = "Failed login message in Korean";

        // Act
        var criteria = new InsertToastMessageCriteria(
            targetId,
            creator,
            contentEN,
            contentCH,
            contentCS,
            contentTH,
            contentVN,
            contentJP,
            contentKO);

        // Assert
        // Verify that the content is properly serialized
        var deserializedEN = JsonConvert.DeserializeObject<ToastContentModel>(criteria.ContentEN);
        deserializedEN.Should().NotBeNull();
        deserializedEN.Body.Should().Be(contentEN);
        deserializedEN.Title.Should().NotBeNullOrEmpty();

        var deserializedCH = JsonConvert.DeserializeObject<ToastContentModel>(criteria.ContentCH);
        deserializedCH.Should().NotBeNull();
        deserializedCH.Body.Should().Be(contentCH);
        deserializedCH.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IsValid_AlwaysReturnsTrue()
    {
        // Arrange
        var criteria = new InsertToastMessageCriteria(
            123, 456, "EN", "CH", "CS", "TH", "VN", "JP", "KO");

        // Act
        bool result = criteria.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetSettingKey_ReturnsCorrectValue()
    {
        // Arrange
        var criteria = new InsertToastMessageCriteria(
            123, 456, "EN", "CH", "CS", "TH", "VN", "JP", "KO");

        // Act
        string settingKey = criteria.GetSettingKey();

        // Assert
        settingKey.Should().Be("Age_ToastMessage_Ins");
    }

    [Fact]
    public void ToastId_CanBeSetAndRetrieved()
    {
        // Arrange
        var criteria = new InsertToastMessageCriteria(
            123, 456, "EN", "CH", "CS", "TH", "VN", "JP", "KO");
        int toastId = 789;

        // Act
        criteria.ToastId = toastId;

        // Assert
        criteria.ToastId.Should().Be(toastId);
    }

    [Fact]
    public void ErrCodeAndErrMsg_CanBeSetAndRetrieved()
    {
        // Arrange
        var criteria = new InsertToastMessageCriteria(
            123, 456, "EN", "CH", "CS", "TH", "VN", "JP", "KO");
        int errCode = 500;
        string errMsg = "Test error message";

        // Act
        criteria.ErrCode = errCode;
        criteria.ErrMsg = errMsg;

        // Assert
        criteria.ErrCode.Should().Be(errCode);
        criteria.ErrMsg.Should().Be(errMsg);
    }
}