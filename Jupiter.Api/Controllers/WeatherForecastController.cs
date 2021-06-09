using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jupiter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ApplicationDbContext _context;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var entities = _context.Entities;

            return Ok(entities);
        }
    }
}
