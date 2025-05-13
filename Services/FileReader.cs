namespace FlightDataAnalyzer.Services
{
    public class FileReader: IFileReader
    {
        
        public Task<string[]> ReadAllLinesAsync(string path)
        {
            return File.ReadAllLinesAsync(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
    
    }
}
