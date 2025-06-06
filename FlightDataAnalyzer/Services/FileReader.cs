﻿namespace FlightDataAnalyzer.Services
{
    public class FileReader: IFileReader
    {
        
        public async Task<string[]> ReadAllLinesAsync(string path)
        {
            return await File.ReadAllLinesAsync(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
    
    }
}
