using System;
using System.IO;
using FluentAssertions;
using MemberAPIs.Configurations;
using Xunit;

namespace MemberAPIs.UnitTests.Configurations;

public class AppSettingTests : IDisposable
{
    private readonly string _testConfigPath;
    private const string TestConfigContent = @"{
        ""APIToken"": ""test-token"",
        ""IntValue"": 42,
        ""BoolValue"": true,
        ""StringValue"": ""test-string""
    }";

    public AppSettingTests()
    {
        _testConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        File.WriteAllText(_testConfigPath, TestConfigContent);
    }

    public void Dispose()
    {
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }

    [Fact]
    public void GetValue_WithExistingStringKey_ReturnsCorrectValue()
    {
        // Act
        var result = AppSetting.GetValue<string>("StringValue");

        // Assert
        result.Should().Be("test-string");
    }

    [Fact]
    public void GetValue_WithExistingIntKey_ReturnsCorrectValue()
    {
        // Act
        var result = AppSetting.GetValue<int>("IntValue");

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void GetValue_WithExistingBoolKey_ReturnsCorrectValue()
    {
        // Act
        var result = AppSetting.GetValue<bool>("BoolValue");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void APIToken_ReturnsCorrectValue()
    {
        // Act
        var result = AppSetting.APIToken;

        // Assert
        result.Should().Be("test-token");
    }

    [Fact]
    public void GetValue_WithNonExistentKey_ReturnsDefaultValue()
    {
        // Act
        var result = AppSetting.GetValue<string>("NonExistentKey");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetValue_WithTypeMismatch_ThrowsInvalidCastException()
    {
        // Act
        var act = () => AppSetting.GetValue<int>("StringValue");

        // Assert
        act.Should().Throw<InvalidCastException>()
            .WithMessage("Key: StringValue*");
    }
}