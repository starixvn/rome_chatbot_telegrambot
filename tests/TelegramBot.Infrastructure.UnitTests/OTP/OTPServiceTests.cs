using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Fanex.Caching;
using NSubstitute;
using TelegramBot.Infrastructure.OTP;
using TelegramBot.Infrastructure.Setting.Constants;
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.OTP;

public class OTPServiceTests
{
    private readonly ICacheService cacheService;
    private readonly WebAPIConfiguration configuration;
    private readonly OTPService otpService;

    public OTPServiceTests()
    {
        cacheService = Substitute.For<ICacheService>();
        configuration = Substitute.For<WebAPIConfiguration>();
        configuration.OtpExpirationInMinutes = 3;
        var cacheServiceFactory = Substitute.For<ICacheServiceFactory>();
        cacheServiceFactory.Get(Arg.Any<string>()).Returns(cacheService);
        otpService = new OTPService(cacheServiceFactory, configuration);
    }

    [Theory, AutoData]
    public async Task GenerateOTP_Always_CallsCacheServiceSetAsync(int custId, int subAccId)
    {
        // Act
        await otpService.GenerateOTP(custId, subAccId);

        // Assert
        await cacheService.Received().SetAsync($"otp_value_{custId}_{subAccId}", Arg.Any<string>(), Arg.Any<CacheItemOptions>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task VerifyOTP_OTPIsExpired_ReturnOTPExpiredResult(string expectedOtp)
    {
        // Arrange
        var custId = 1;
        var subAccId = 0;
        cacheService.GetAsync<string>($"otp_value_{custId}_{subAccId}").Returns(Task.FromResult(expectedOtp));
        var expectedResult = new Result(ErrorCodes.OTP_Expired, "OTP is expired");

        // Act
        var actualResult = await otpService.VerifyOTP(custId, subAccId, "123456", 3);

        // Assert
        Assert.True(actualResult.Code == expectedResult.Code && actualResult.Message == expectedResult.Message);
    }
}