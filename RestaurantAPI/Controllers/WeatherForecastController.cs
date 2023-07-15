using Microsoft.AspNetCore.Mvc;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherForecastService _service;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var result = _service.Get();

            return result;
        }

        [HttpGet("currentDay/{max}")]
        public IEnumerable<WeatherForecast> Get2([FromQuery]int take, [FromRoute]int max)
        {
            var result = _service.Get();

            return result;
        }

        [HttpPost]
        public ActionResult<string> Hello([FromBody]string name)
        {
            return Ok($"Hello {name}");
        }
    }
}