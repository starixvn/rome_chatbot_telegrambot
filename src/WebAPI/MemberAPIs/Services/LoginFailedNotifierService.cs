using Fanex.Data.Repository;
using MemberAPIs.Criteria;
using MemberAPIs.Constants;
using MemberAPIs.Models;
using MemberAPIs.Params;

namespace MemberAPIs.Services
{
    public interface ILoginFailedNotifierService
    {
        Task<APIResultModel> AlertFailedLogin(AlertLoginFailedParam param);
    }

    public class LoginFailedNotifierService : ILoginFailedNotifierService
    {
        private readonly IDynamicRepository dynamicRepository;
        protected readonly ILoggerService loggerService;

        private const string MESSAGE_KEY_LOGIN_FAILED_EXCEED_COUNT = "PersonalMessage_DownlineLoginFailed_ExceedCount";
        private const string MESSAGE_KEY_LOGIN_FAILED_CHANGED_PASSWORD = "PersonalMessage_DownlineLoginFailed_ChangedPassword";

        public LoginFailedNotifierService(IDynamicRepository dynamicRepository, ILoggerService loggerService)
        {
            this.dynamicRepository = dynamicRepository;
            this.loggerService = loggerService;
        }

        public async Task<APIResultModel> AlertFailedLogin(AlertLoginFailedParam param)
        {
            var getCustInfoCriteria = new GetCustInfoCriteria(param.MemberId);
            await dynamicRepository.ExecuteAsync(getCustInfoCriteria);

            var validationResult = ValidateLoginStatus(param, getCustInfoCriteria);
            if (validationResult != null)
            {
                return validationResult;
            }

            if (getCustInfoCriteria.UplineId <= 0)
            {
                var errMsg = $"Invalid UplineId for user {param.MemberId}: {getCustInfoCriteria.UplineId}";
                await loggerService.Error(new Exception(errMsg));
                return APIResultModel.Failed(errMsg);
            }

            var (messageKey, messageParams) = GetMessageDetails(param, getCustInfoCriteria);
            var messages = LocalizedMessagesModel.GetLocalizedMessages(messageKey, messageParams);
            
            await InsertPersonalMessage(messages, getCustInfoCriteria);
            await InsertToastMessage(messages, getCustInfoCriteria, param.MemberId);

            return APIResultModel.Success();
        }

        private async Task InsertPersonalMessage(LocalizedMessagesModel messages, GetCustInfoCriteria getCustInfoCriteria)
        {
            var personalMessageCriteria = new InsertPersonalMessageCriteria(
                messages.En, messages.Ch, messages.Cs, messages.Th, messages.Vi, messages.Jp, messages.Ko);

            await dynamicRepository.ExecuteAsync(personalMessageCriteria);

            var assignPersonalMessageCriteria = new AssignPersonalMessageCriteria(
                personalMessageCriteria.MessageIdOutput ?? 0, 
                getCustInfoCriteria.UplineId, 
                getCustInfoCriteria.UplineName);
            await dynamicRepository.ExecuteAsync(assignPersonalMessageCriteria);
        }

        private async Task InsertToastMessage(LocalizedMessagesModel messages, GetCustInfoCriteria getCustInfoCriteria, int memberId)
        {
            var insertToastMessageCriteria = new InsertToastMessageCriteria(
                getCustInfoCriteria.UplineId, 
                memberId,
                messages.En, messages.Ch, messages.Cs, messages.Th, messages.Vi, messages.Jp, messages.Ko);
            await dynamicRepository.ExecuteAsync(insertToastMessageCriteria);
        }

        private (string messageKey, object[] messageParams) GetMessageDetails(AlertLoginFailedParam param, GetCustInfoCriteria getCustInfoCriteria)
        {
            return param.LoginFailedType == LoginFailedTypeConstants.ReachFailedCount
                ? (MESSAGE_KEY_LOGIN_FAILED_EXCEED_COUNT, new object[] { getCustInfoCriteria.CustName })
                : (MESSAGE_KEY_LOGIN_FAILED_CHANGED_PASSWORD, new object[] { getCustInfoCriteria.CustName, param.LastChangedPasswordDate });
        }

        private APIResultModel ValidateLoginStatus(AlertLoginFailedParam param, GetCustInfoCriteria getCustInfoCriteria)
        {
            if (getCustInfoCriteria.FailedCount == 0)
            {
                return APIResultModel.Failed(ApiMessagesConstants.LoginFailedCountZero);
            }

            if (getCustInfoCriteria.FailedCount >= 6)
            {
                return APIResultModel.Failed(ApiMessagesConstants.AccountIsLocked);
            }

            if (param.LoginFailedType == LoginFailedTypeConstants.ReachFailedCount)
            {
                return getCustInfoCriteria.FailedCount == 3
                    ? null
                    : APIResultModel.Failed(ApiMessagesConstants.FailedCountNotThree);
            }  
            
            if (param.LoginFailedType == LoginFailedTypeConstants.ChangedPassword)
            {
                if (string.IsNullOrWhiteSpace(param.LastChangedPasswordDate))
                {
                    return APIResultModel.Failed(ApiMessagesConstants.LastChangedPasswordDateEmpty);
                }

                if (getCustInfoCriteria.FailedCount == 3)
                {
                    return APIResultModel.Failed(ApiMessagesConstants.LoginFailedTypeMismatchForReach3Attempts);
                }

                return null;
            }

            return null;
        }
    }
}