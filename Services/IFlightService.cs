using FlightDataAnalyzer.Models;

namespace FlightDataAnalyzer.Services
{
    public interface IFlightService
    {
        List<FlightInfo> GetFlightInfo();
    }
}
