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

        public List<FlightInfo> GetInconsistentFlightList()
{
            var flights = GetFlightInfo(); 
            var inconsistentFlights = new List<FlightInfo>();

            //Identifying flight chains by flight Registration Number
            var flightchains = flights.GroupBy(f => f.FlightNumber);

            foreach (var chain in flightchains)
            {
                // Sort by DepartureDatetime
                var orderedFlights = chain
                    .OrderBy(f => DateTime.TryParse(f.DepartureDatetime, out var dt) ? dt : DateTime.MinValue)
                    .ToList();

                // Only for the flight chains appeared multiple times
                if (orderedFlights.Count > 1)
                {
                    for (int i = 0; i < orderedFlights.Count - 1; i++)
                    {
                        var currentFlight = orderedFlights[i];
                        var nextFlight = orderedFlights[i + 1];

                        // Check if the arrival airport of the current flight matches the departure airport of the next flight
                        if (currentFlight.ArrivalAirport != nextFlight.DepartureAirport)
                        {
                            inconsistentFlights.Add(currentFlight);
                            inconsistentFlights.Add(nextFlight); 
                        }
                    }

                    Console.WriteLine($"Found {inconsistentFlights.Count} inconsistent flight chains.");
                   
                }
            }
            return inconsistentFlights.ToList(); 
        }
    }
}

