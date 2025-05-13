using FlightDataAnalyzer.Models;

namespace FlightDataAnalyzer.Services
{
    public interface IFlightService
    {
         Task<(List<FlightInfo>, List<string> Errors)> GetFlightInfo();

         Task<(List<FlightInfo>, List<string> Errors)> GetInconsistentFlightList();
    }
}
