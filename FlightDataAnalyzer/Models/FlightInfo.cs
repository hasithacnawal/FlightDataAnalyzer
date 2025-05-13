using System.ComponentModel.DataAnnotations;

namespace FlightDataAnalyzer.Models
{

    /// <summary>
    /// Structured flight information.
    /// </summary>
    public class FlightInfo
    {
        
        public int Id { get; set; }
        [Required]
        public string AircraftRegistrationNumber { get; set; }
        [Required]
        public string AircraftType { get; set; }
        [Required]
        public string FlightNumber { get; set; }
        [Required]   
        public string DepartureAirport { get; set; }
        [Required]
        public string DepartureDatetime { get; set; }
        [Required]
        public string ArrivalAirport { get; set; }
        [Required]
        public string ArrivalDatetime { get; set; }
    }
}
