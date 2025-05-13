using FlightDataAnalyzer.Models;

namespace FlightDataAnalyzer.Services
{
    public interface IFlightService
    {
         Task<List<FlightInfo>> GetFlightInfo();

         Task< List<FlightInfo>> GetInconsistentFlightList();
    }
}
