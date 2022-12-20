using Fanex.Caching;
using TelegramBot.Common.Extensions;
using TelegramBot.Infrastructure.Setting.Constants;
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.OTP;

public interface IOTPService
{
    Task<string> GenerateOTP(int custId, int subAccId);

    Task<Result> VerifyOTP(int custId, int subAccId, string otp, int maxFailedTimes);
}

public class OTPService : IOTPService
{
    private readonly ICacheService cacheService;
    private readonly WebAPIConfiguration configuration;

    public OTPService(ICacheServiceFactory cacheServiceFactory, WebAPIConfiguration configuration)
    {
        cacheService = cacheServiceFactory.Get(CacheProvider.Redis);
        this.configuration = configuration;
    }

    public async Task<string> GenerateOTP(int custId, int subAccId)
    {
        const int maxValue = 999999;
        var otp = ThreadSafeRandom.Next(0, maxValue).ToString("D6");

        await cacheService.SetAsync(
            $"otp_value_{custId}_{subAccId}",
            otp,
            new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(configuration.OtpExpirationInMinutes)));

        return otp;
    }

    public async Task<Result> VerifyOTP(int custId, int subAccId, string otp, int maxFailedTimes)
    {
        var countCacheKey = $"otp_count_{custId}_{subAccId}";
        var failedCount = await cacheService.GetAsync<int>(countCacheKey);
        var valueCacheKey = $"otp_value_{custId}_{subAccId}";
        var expectedOtp = await cacheService.GetAsync<string>(valueCacheKey);
        var isValidOTP = !string.IsNullOrWhiteSpace(expectedOtp) && otp == expectedOtp;
        if (!isValidOTP)
        {
            failedCount++;
            await cacheService.SetAsync(countCacheKey, failedCount);
            if (failedCount >= maxFailedTimes)
            {
                return new Result(ErrorCodes.OTP_FailedManyTimes, "OTP failed many times");
            }
        }

        if (string.IsNullOrEmpty(expectedOtp))
        {
            return new Result(ErrorCodes.OTP_Expired, "OTP is expired");
        }

        if (otp != expectedOtp)
        {
            return new Result(ErrorCodes.OTP_Invalid, "OTP is invalid");
        }

        await cacheService.RemoveAsync(countCacheKey);
        return Result.CreateSuccessfulResult();
    }
}