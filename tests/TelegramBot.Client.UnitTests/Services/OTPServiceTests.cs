using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Newtonsoft.Json;
using NSubstitute;
using TelegramBot.Client.Models;
using TelegramBot.Client.Services;
using Xunit;

namespace TelegramBot.Client.UnitTests.Services;

public class OTPServiceTests
{
    [Theory, AutoData]
    public async Task VerifyOTP_Always_ReturnCorrectResult(int custId, int subAccId, string otp)
    {
        //Arrange
        var response = JsonConvert.SerializeObject(new Result(0));
        var httpClient = StubHttpClient(response);
        var otpService = new OTPService(httpClient);

        // Act
        var actualResult = await otpService.VerifyOTP(custId, subAccId, otp);

        // Assert
        Assert.Equal(response, JsonConvert.SerializeObject(actualResult));
    }

    private static HttpClient StubHttpClient(string response)
    {
        var messageHandler = new MockHttpMessageHandler(response, HttpStatusCode.OK);
        return new HttpClient(messageHandler) { BaseAddress = new Uri("https://localhost") };
    }
}