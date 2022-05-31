using System.Threading.Tasks;
using Alpha.API.Client.Models.Customers;
using Alpha.API.Client.Services;
using NSubstitute;
using TelegramBot.Infrastructure.Action.Validators;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Models;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Action.Validators;

public class DirectDownlinesValidatorTest
{
    private readonly IMessageBuilder messageBuilder;
    private readonly ICustomerService customerService;
    private readonly DirectDownlinesValidator  directDownlinesValidator ;
    
    public DirectDownlinesValidatorTest()
    {
        messageBuilder= Substitute.For<IMessageBuilder>();
        customerService = Substitute.For<ICustomerService>();
        directDownlinesValidator = new DirectDownlinesValidator(messageBuilder, customerService);
    }

    [Fact] 
    public async Task Validate_ValidDownlineAccount_ReturnSuccessResult()
    {
        var account = new CustBotAccount() { CustId = 1, CustomerLevel = 3 };
        customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(new DownlineInfo(22, 2, true));
        var expectResult = Result.CreateSuccessfulResult();
        
        var result = directDownlinesValidator.Validate(account, "directDownline");
        
        Assert.Equal(expectResult.Code,result.Result.Code);
        Assert.Equal(expectResult.Message,result.Result.Message);
    } 
    
    [Fact] 
    public async Task Validate_EmptyUserResponse_ReturnErrorResult()
    {
        var account = new CustBotAccount() { CustId = 1, CustomerLevel = 3 };
        customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(new DownlineInfo(22, 2, true));
        var expectResult = Result.CreateErrorResult();
        
        var result = directDownlinesValidator.Validate(account, "");
        
        Assert.Equal(expectResult.Code,result.Result.Code);
        Assert.Equal(expectResult.Message,result.Result.Message);
    } 
    
    [Fact] 
    public async Task Validate_BadDownlineInfo_ReturnErrorResult()
    {
        var account = new CustBotAccount() { CustId = 1, CustomerLevel = 3 };
        customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(new DownlineInfo(-1, 2, true));
        var expectResult = Result.CreateErrorResult();
        
        var result = directDownlinesValidator.Validate(account, "directDownline");
        
        Assert.Equal(expectResult.Code,result.Result.Code);
        Assert.Equal(expectResult.Message,result.Result.Message);
    }   
    
    [Fact] 
    public async Task Validate_NullDownlineInfo_ReturnErrorResult()
    {
        var account = new CustBotAccount() { CustId = 1, CustomerLevel = 3 };
        var expectResult = Result.CreateErrorResult();
        
        var result = directDownlinesValidator.Validate(account, "directDownline");
        
        Assert.Equal(expectResult.Code,result.Result.Code);
        Assert.Equal(expectResult.Message,result.Result.Message);
    }
}