using FluentAssertions;
using MemberAPIs.Models;

namespace MemberAPIs.UnitTests.Models;

public class LocalizedMessagesModelTests
{
    [Fact]
    public void GetLocalizedMessages_WithValidKeyAndNoParams_ShouldReturnLocalizedMessages()
    {
        // Arrange
        const string messageKey = "ToastTitle_DownlineLoginFailed"; // This key has no parameters

        // Act
        var result = LocalizedMessagesModel.GetLocalizedMessages(messageKey, null);

        // Assert
        result.Should().NotBeNull();
        result.En.Should().Be("Downline Login Issue");
        result.Ch.Should().NotBeNullOrEmpty();
        result.Cs.Should().NotBeNullOrEmpty();
        result.Th.Should().NotBeNullOrEmpty();
        result.Vi.Should().NotBeNullOrEmpty();
        result.Jp.Should().NotBeNullOrEmpty();
        result.Ko.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetLocalizedMessages_WithValidKeyAndParams_ShouldReturnFormattedMessages()
    {
        // Arrange
        const string messageKey = "PersonalMessage_DownlineLoginFailed_ExceedCount";
        var messageParams = new object[] { "TestUser" };

        // Act
        var result = LocalizedMessagesModel.GetLocalizedMessages(messageKey, messageParams);

        // Assert
        result.Should().NotBeNull();
        result.En.Should().Be("Your downline, <b>TestUser</b>, has reached <b>3 failed login attempts</b>. This could be a login issue or a security concern. Please check and take action if needed.");
        result.Ch.Should().NotBeNullOrEmpty();
        result.Cs.Should().NotBeNullOrEmpty();
        result.Th.Should().NotBeNullOrEmpty();
        result.Vi.Should().NotBeNullOrEmpty();
        result.Jp.Should().NotBeNullOrEmpty();
        result.Ko.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetLocalizedMessages_WithEmptyKey_ShouldThrowArgumentNullException()
    {
        // Act
        var action = () => LocalizedMessagesModel.GetLocalizedMessages(string.Empty, null);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Message key cannot be null or empty. (Parameter 'messageKey')");
    }

    [Fact]
    public void GetLocalizedMessages_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Act
        var action = () => LocalizedMessagesModel.GetLocalizedMessages(null, null);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Message key cannot be null or empty. (Parameter 'messageKey')");
    }

    [Fact]
    public void GetLocalizedMessages_WithNonExistentKey_ShouldReturnEmptyStrings()
    {
        // Arrange
        const string messageKey = "NonExistentKey";

        // Act
        var result = LocalizedMessagesModel.GetLocalizedMessages(messageKey, null);

        // Assert
        result.Should().NotBeNull();
        result.En.Should().BeEmpty();
        result.Ch.Should().BeEmpty();
        result.Cs.Should().BeEmpty();
        result.Th.Should().BeEmpty();
        result.Vi.Should().BeEmpty();
        result.Jp.Should().BeEmpty();
        result.Ko.Should().BeEmpty();
    }

    [Fact]
    public void Properties_ShouldBeInitializedAsEmptyStrings()
    {
        // Act
        var messages = new LocalizedMessagesModel();

        // Assert
        messages.En.Should().BeEmpty();
        messages.Ch.Should().BeEmpty();
        messages.Cs.Should().BeEmpty();
        messages.Th.Should().BeEmpty();
        messages.Vi.Should().BeEmpty();
        messages.Jp.Should().BeEmpty();
        messages.Ko.Should().BeEmpty();
    }
} 