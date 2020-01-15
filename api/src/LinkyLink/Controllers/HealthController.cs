using Microsoft.AspNetCore.Mvc;
using System;

namespace LinkyLink.Controllers
{
    /// <summary>
    /// The class handles health checks for the backend API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Performs a simple check that returns a 200 OK response
        /// </summary>
        /// <returns></returns>
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
