using FlightDataAnalyzer.Models;

namespace FlightDataAnalyzer.Services
{
    public class FlightService : IFlightService
    {
        //file path on the server
        private readonly string _csvPath = "flightdata.csv";

        public List<FlightInfo> GetFlightInfo()
        {
            var flights = new List<FlightInfo>();

            if (!File.Exists(_csvPath))
                return flights;

            var lines = File.ReadAllLines(_csvPath).Skip(1); // get data without header

            foreach (var line in lines)
            {
                var values = line.Split(',');

                if (values.Length >= 8)
                {
                    flights.Add(new FlightInfo
                    {
                        Id = int.TryParse(values[0], out var id) ? id : 0,
                        AircraftRegistrationNumber = values[1],
                        AircraftType = values[2],
                        FlightNumber = values[3],
                        DepartureAirport = values[4],
                        DepartureDatetime = values[5],
                        ArrivalAirport = values[6],
                        ArrivalDatetime = values[7]
                    });
                }
            }

            return flights;
        }
    }
}

