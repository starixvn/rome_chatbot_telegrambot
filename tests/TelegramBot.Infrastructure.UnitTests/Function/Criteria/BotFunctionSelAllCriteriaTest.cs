namespace TelegramBot.Infrastructure.UnitTests.Function.Criteria
{
    using Xunit;
    using TelegramBot.Infrastructure.Function.Criteria;

    public class BotFunctionSelAllCriteriaTest
    {
        private readonly BotFunctionSelAllCriteria botFunctionSelAllCriteria;

        public BotFunctionSelAllCriteriaTest()
        {
            botFunctionSelAllCriteria = new BotFunctionSelAllCriteria(false);
        }

        [Fact]
        public void GetSettingKey_Always_ReturnName()
        {
            //Act
            var result = botFunctionSelAllCriteria.GetSettingKey();

            //Assert
            Assert.Equal("BotFunctionSelAll", result);
        }

        [Fact]
        public void IsValid_Always_ReturTrue()
        {
            //Assert
            Assert.True(botFunctionSelAllCriteria.IsValid());
        }
    }
}
