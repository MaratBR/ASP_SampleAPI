using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_SampleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("claims")]
        [Authorize(Roles = "Admin")]
        public object GetUserClains()
        {
            var claims = new Dictionary<string, object>();

            foreach (var claim in HttpContext.User.Claims)
            {
                claims[claim.Type] = claim.Value;
            }

            return claims;
        }
    }
}
