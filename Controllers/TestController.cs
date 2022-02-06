using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace test_space.Controllers
{
    [ApiController]
    [Route("Test")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("OnlyJwt")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult OnlyJwt()
        {
            var message = $"Hello from {nameof(OnlyJwt)}";
            return new ObjectResult(message);
        }

        [HttpGet("OnlyApiKey")]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.DefaultScheme)]
        public IActionResult OnlyApiKey()
        {
            var message = $"Hello from {nameof(OnlyApiKey)}";
            return new ObjectResult(message);
        }

        [HttpGet("OnlyManagers")]
        [Authorize(Policy = Policies.OnlyManagers)]
        public IActionResult OnlyManagers()
        {
            var message = $"Hello from {nameof(OnlyManagers)}";
            return new ObjectResult(message);
        }

        [HttpGet("OnlyEmployees")]
        [Authorize(Policy = Policies.OnlyEmployees)]
        public IActionResult OnlyEmployees()
        {
            var message = $"Hello from {nameof(OnlyEmployees)}";
            return new ObjectResult(message);
        }

        [HttpGet("OnlyThirdParty")]
        [Authorize(Policy = Policies.OnlyThirdParties)]
        public IActionResult OnlyThirdParty()
        {
            var message = $"Hello from {nameof(OnlyThirdParty)}";
            return new ObjectResult(message);
        }
    }
}
