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
        private readonly ILogger<FlightDataController> _logger;


        public FlightDataController(IFlightService flightService, ILogger<FlightDataController> logger)
        {
            _flightService = flightService;
            _logger = logger;

        }

        [HttpGet]
        public async Task<ActionResult<List<FlightInfo>>> GetFlights()
        {
            try
            {
                var (flights,errors) = await _flightService.GetFlightInfo();

                var response = new ApiResponse<List<FlightInfo>>();

                if (flights.Count > 0)
                {
                     response = new ApiResponse<List<FlightInfo>>
                    {
                        Success = true,
                        Message = errors.Any() ? "Flight information retrieved with some warnings." : "Flight information retrieved successfully.",
                        Data = flights,
                        Errors = errors
                    };
                }
                else
                {
                     response = new ApiResponse<List<FlightInfo>>
                    {
                        Success = false,
                        Message = "Flight information not retrieved.",
                        Data = flights,
                        Errors = errors
                    };
                }
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown exception occured while retrieving flights");

                return StatusCode(500, new ApiResponse<List<FlightInfo>>
                {
                    Success = false,
                    Message = "Internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }

        }

        [HttpGet("GetInconsistentFlightChains")]
        public async Task<ActionResult<List<FlightInfo>>> GetInconsistentFlightChains()
        {

            try
            {
                var (inconsistentFlights, errors) = await _flightService.GetInconsistentFlightList();

                var response = new ApiResponse<List<FlightInfo>>();

                if (inconsistentFlights.Any())
                {
                    // Return success with flight data and errors
                    response = new ApiResponse<List<FlightInfo>>
                    {
                        Success = true,
                        Message = errors.Any() ? $"{inconsistentFlights.Count} inconsistancies found with some data warnings." : $"{inconsistentFlights.Count} inconsistancies found.",
                        Data = inconsistentFlights,
                        Errors = errors
                    };
                }
                else
                {
                    if(errors.Contains("Unexpected error occurred while processing the file."))
                    {
                        response = new ApiResponse<List<FlightInfo>>
                        {
                            Success = false,
                            Message = "Some issue with the Data Source",
                            Data = inconsistentFlights,
                            Errors = errors
                        };
                    }
                    else
                    {
                        response = new ApiResponse<List<FlightInfo>>
                        {
                            Success = true,
                            Message = "No inconsistant flight chains found.",
                            Data = inconsistentFlights,
                            Errors = errors
                        };
                    }
                    
                }
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown exception occured while Flight Inconsistancy Analysis");

                return StatusCode(500, new ApiResponse<List<FlightInfo>>
                {
                    Success = false,
                    Message = "Internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }

        }


    }

   
}
