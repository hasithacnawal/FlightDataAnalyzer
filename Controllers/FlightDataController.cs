using FlightDataAnalyzer.Models;
using FlightDataAnalyzer.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightDataAnalyzer.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class FlightDataController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public FlightDataController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet]
        public ActionResult<List<FlightInfo>> GetFlights()
        {
            var flights = _flightService.GetFlightInfo();
            return Ok(flights);
        }

        private readonly ILogger<FlightDataController> _logger;

        [HttpGet("GetInconsistentFlightChains")]
        public ActionResult<List<FlightInfo>> GetInconsistentFlightChains()
        {
            var inconsistentFlights = _flightService.GetInconsistentFlightList();

            if (inconsistentFlights.Any())
            {
                return Ok(inconsistentFlights);
            }

            return NoContent(); // No inconsistencies found
        }


    }

   
}
