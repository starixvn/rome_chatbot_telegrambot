
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Announcement;

namespace TelegramBot.Importer.UnitTests.Models.Announcement;

public class SpecificationParamTests
{
    [Theory, AutoData]
    public void Constructor_Always_AssignCorrectValues(Customer customer, bool isSubAcc, IEnumerable<byte> offeredProductIds)
    {
        // Act
        var param = new SpecificationParam(customer, isSubAcc, offeredProductIds);

        // Assert
        Assert.True(param.ARecommendId == customer.ARecommendId
            && param.CurrencyId == customer.CurrencyId
            && param.EnableYesterdayTransfer == customer.IsAllDayBalTransfer
            && param.IsIMBCCustomer == customer.IsIMBC
            && param.IsSubAcc == isSubAcc
            && param.LevelId == customer.RoleId
            && param.MRecommendId == customer.MRecommendId
            && param.SRecommendId == customer.SRecommendId
            && param.UserId == customer.CustId
            && param.UserName == customer.UserName
            && param.OfferedProductIds == offeredProductIds);
    }
}