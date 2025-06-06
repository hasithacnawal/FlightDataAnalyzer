﻿using FlightDataAnalyzer.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FlightDataAnalyzer.Services
{
    public class FlightService : IFlightService
    {
        private readonly string _csvPath;
        private readonly ILogger<FlightService> _logger;
        private readonly IFileReader _fileReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlightService"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration to read the CSV path.</param>
        /// <param name="logger">Logger for capturing information and errors.</param>
        /// <param name="fileReader">File reader abstraction for easier testing and file operations.</param>
        public FlightService(IConfiguration configuration, ILogger<FlightService> logger, IFileReader fileReader)
        {
            _csvPath = configuration["CsvSettings:Path"]; //Configuraion path added for avoiding hardcoding
            _logger = logger;
            _fileReader = fileReader;

        }
      

        /// <summary>
        /// Loads flight data from a CSV file, validates, and parses it.
        /// </summary>
        /// <returns>A tuple containing a list of valid flights and any parsing errors.
        /// </returns>
        public virtual async Task<(List<FlightInfo>,List<string> Errors)> GetFlightInfo()
        {
            var flights = new List<FlightInfo>();
            var errors = new List<string>();

            if (!_fileReader.FileExists(_csvPath))
            {
                string error = $"CSV file couldn't be found at path: {_csvPath}";
                _logger.LogError(error);

                errors.Add(error);
                return (flights, errors);
            }

            try
            {
                var lines = await _fileReader.ReadAllLinesAsync(_csvPath);

                foreach (var line in lines.Skip(1)) // get data without header
                {
                    var values = line.Split(',');

                    //Checks for required column data
                    if (values.Length < 8)
                    {
                        string error = $"Id {values[0].Trim()}: Incorrect number of columns. Expected 8, got {values.Length}.";
                        _logger.LogWarning(error);
                        errors.Add(error);
                        continue;
                    }

                    //Checks for invalid departure time data
                    if (!DateTime.TryParse(values[5], out var departureTime))
                    {
                        string error = $"Id {values[0].Trim()}: Invalid DepartureDatetime '{values[5].Trim()}'.";
                        _logger.LogWarning(error);
                        errors.Add(error);
                        continue;
                    }

                    //Checks for invalid arrival time data
                    if (!DateTime.TryParse(values[7], out var arrivalTime))
                    {
                        string error = $"Id {values[0].Trim()}: Invalid ArrivalDatetime '{values[7].Trim()}'.";
                        _logger.LogWarning(error);
                        errors.Add(error);
                        continue;
                    }

                    var flight = new FlightInfo
                    {
                        Id = int.TryParse(values[0], out var id) ? id : 0,
                        AircraftRegistrationNumber = values[1],
                        AircraftType = values[2],
                        FlightNumber = values[3],
                        DepartureAirport = values[4],
                        DepartureDatetime = values[5],
                        ArrivalAirport = values[6],
                        ArrivalDatetime = values[7]
                    };

                    var validationContext = new ValidationContext(flight);
                    var validationResults = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(flight, validationContext, validationResults, true))
                    {
                        foreach (var vr in validationResults)
                        {
                            string error = $"Id {values[0].Trim()}: {vr.ErrorMessage}";
                            _logger.LogWarning(error);
                            errors.Add(error);
                        }
                        continue;
                    }

                    flights.Add(flight);

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while parsing the CSV.");
                errors.Add("Unexpected error occurred while processing the file.");
                flights.Clear();
            }
           
            return (flights,errors);
        }

        /// <summary>
        /// Analyzes the flight data and returns flights with inconsistent chains.
        /// Inconsistencies are defined as flights in the same chain (same flight number) where
        /// the arrival airport of one flight does not match the departure airport of the next.
        /// </summary>
        /// <returns>
        /// A tuple containing:
        /// - A list of <see cref="FlightInfo"/> objects that are inconsistent.
        /// - A list of error messages encountered during analysis.
        /// </returns>
        public async Task<(List<FlightInfo>, List<string> Errors)> GetInconsistentFlightList()
{
            var (flights,errors) = await GetFlightInfo();
            var inconsistentFlights = new List<FlightInfo>();


            if (flights == null || !flights.Any())
            {
                _logger.LogWarning($"Flight Inconsistency Analysis: No flight data available.");
                return (inconsistentFlights, errors);
            }

            try
            {
                // Identifying flight chains by flight Number
                var flightchains = flights.GroupBy(f => f.FlightNumber);

                foreach (var chain in flightchains)
                {
                    // Sort by DepartureDatetime and remove duplicates
                    var orderedFlights = chain
                            .GroupBy(f => new { f.DepartureAirport, f.ArrivalAirport, f.DepartureDatetime })
                            .Select(g => g.First()) // removing duplicate entries
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


                    }
                }
                _logger.LogInformation($"Found {inconsistentFlights.Count} inconsistent flight chains.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Flight Inconsistency Analysis: Unexpected error during inconsistency analysis.");
                errors.Add(ex.Message);
            }

            return (inconsistentFlights.ToList(), errors);

        }

    }
}

