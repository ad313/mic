using Mic.TestStardand.AopSpace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mic.Aop.TestAspnetcore3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly SampleService _sampleService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, SampleService sampleService)
        {
            _logger = logger;
            _sampleService = sampleService;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {

            var result = _sampleService.GetDateTimeSync();

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
