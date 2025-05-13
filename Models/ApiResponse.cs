namespace FlightDataAnalyzer.Models
{

    /// <summary>
    /// Streamlined  API response of the controller returns/http responses for better maintainability and reliability. Encapsulates success status, data, message, and error details.
    /// </summary>
    /// <typeparam name="T">The type of the data payload returned in the response.</typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public T? Data { get; set; }
    }
}
