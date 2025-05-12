namespace FlightDataAnalyzer.Models
{
    public class FlightInfo
    {
        public int Id { get; set; }
        public string AircraftRegistrationNumber { get; set; }
        public string AircraftType { get; set; }
        public string FlightNumber { get; set; }
        public string DepartureAirport { get; set; }
        public string DepartureDatetime { get; set; }
        public string ArrivalAirport { get; set; }
        public string ArrivalDatetime { get; set; }
    }
}
