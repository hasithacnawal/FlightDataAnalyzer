namespace FlightDataAnalyzer.Services
{
    public interface IFileReader
    {
        Task<string[]> ReadAllLinesAsync(string path);

        bool FileExists(string path);
    }
}
