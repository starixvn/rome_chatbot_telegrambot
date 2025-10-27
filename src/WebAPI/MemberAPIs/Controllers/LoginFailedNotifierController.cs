using System.Net;
using System.Net.Mime;
using MemberAPIs.Configurations;
using MemberAPIs.Constants;
using MemberAPIs.Params;
using MemberAPIs.Requests;
using MemberAPIs.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemberAPIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginFailedNotifierController : ControllerBase
    {
        private readonly ILoginFailedNotifierService service;
        private readonly ILogger<LoginFailedNotifierController> logger;

        public LoginFailedNotifierController(
            ILoginFailedNotifierService service,
            ILogger<LoginFailedNotifierController> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        [Route("InsertNotification"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<object> InsertNotification(
            [FromHeader(Name = "APIKey")] string apiKey,
            [FromBody] AlertLoginFailedRequest request)
        {
            try
            {
                var validationResult = ValidateInsertNotificationParameters(apiKey, request);
                if (validationResult != null)
                {
                    return validationResult;
                }

                var param = new AlertLoginFailedParam
                {
                    LoginFailedType = request.LoginFailedType,
                    MemberId = request.MemberId,
                    LastChangedPasswordDate = request.LastChangedPasswordDate
                };

                return await service.AlertFailedLogin(param);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process login failure alert for member {MemberId}", request?.MemberId);
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return ApiMessagesConstants.InternalServerError;
            }
        }

        private object ValidateInsertNotificationParameters(string apiKey, AlertLoginFailedRequest request)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return ApiMessagesConstants.ApiKeyRequired;
            }

            if (apiKey != AppSetting.APIToken)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return ApiMessagesConstants.ApiKeyInvalid;
            }

            if (!LoginFailedTypeConstants.IsValidLoginFailedType(request.LoginFailedType))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return ApiMessagesConstants.InvalidLoginFailedType;
            }

            return null;
        }
    }
}