using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkyLink.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public ActionResult Ping()
        {
            var status = new
            {
                Status = "OK",
                Timestamp = DateTime.UtcNow
            };

            return Ok(status);
        }
    }
}
