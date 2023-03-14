using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using PortalAPIs.Models;
using PortalAPIs.Params;
using Newtonsoft.Json;
using PortalAPIs.Services;

namespace PortalAPIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbnormalLoginController : ControllerBase
    {
        private readonly IAbnormalLoginService service;

        public AbnormalLoginController(IAbnormalLoginService service)
        {
            this.service = service;
        }

        [Route("AlertAbnormalLogin"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Boolean), StatusCodes.Status200OK)]
        public async Task<Boolean> AlertAbnormalLogin([FromBody] string trans)
        {
            var model = JsonConvert.DeserializeObject<IEnumerable<AbnormalLoginParam>>(trans).Select(t => new AbnormalLoginModel(t.ID, t.LoginName, t.TransTime, t.UserAgent, t.IP, t.OS, t.Device, t.Browser));

            return await service.AlertAbnormalLogin(model);
        }
    }
}