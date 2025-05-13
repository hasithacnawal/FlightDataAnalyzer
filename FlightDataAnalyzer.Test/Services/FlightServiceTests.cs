
using FlightDataAnalyzer.Models;
using FlightDataAnalyzer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace FlightDataAnalyzer.Test.Services
{
    /// <summary>
    /// Contains unit tests for the FlightService logic using mocked file input.
    /// Ensures data validation and logics behaves correctly.
    /// </summary>
    public class FlightServiceTests
    {
        private readonly Mock<IConfiguration> mockConfig;
        private readonly Mock<ILogger<FlightService>> mockLogger;
        private readonly Mock<IFileReader> mockFileReader;

        private class TestFlightService : FlightService
        {
            private readonly List<FlightInfo> _testData;
            private readonly List<string> _testErrors;

            public TestFlightService(List<FlightInfo> testData, List<string> testErrors) : base(Mock.Of<IConfiguration>(), Mock.Of<ILogger<FlightService>>(), Mock.Of<IFileReader>())//Mocking the injections of Ilogger and IFileReader
            {
                _testData = testData;
                _testErrors = testErrors;

            }

            public override Task<(List<FlightInfo>, List<string>)> GetFlightInfo()
            {
                return Task.FromResult((_testData,_testErrors));
            }         
        }

        public FlightServiceTests()
        {
            mockConfig = new Mock<IConfiguration>();         
            mockLogger = new Mock<ILogger<FlightService>>();
            mockFileReader = new Mock<IFileReader>();
        }

     
        [Fact]
        public async Task GetFlightInfo_ShouldReturnError_WhenFileDoesNotExist()
        {
            // Arrange
            var fileReader = new Mock<IFileReader>();

            fileReader.Setup(fr => fr.FileExists(It.IsAny<string>())).Returns(false);

            var service = new FlightService(mockConfig.Object,mockLogger.Object, fileReader.Object);

            // Act
            var (flights, errors) = await service.GetFlightInfo();

            // Assert
            Assert.Empty(flights);
            Assert.Single(errors);
            Assert.Contains("CSV file couldn't be found", errors[0]);
        }

        [Fact]
        public async Task GetFlightInfo_ShouldReturnValidFlights_WhenDataIsCorrect()
        {
            // Arrange
            var csvLines = new[]
            {
                 "id,aircraft_registration_number,aircraft_type,flight_number,departure_airport,departure_datetime,arrival_airport,arrival_datetime",
                "1,ABC123,A320,AA100,HEL,2024-01-01 08:00,LHR,2024-01-01 10:00"
            };
            // Creating a temp file and write lines to it
            var tempFilePath = Path.GetTempFileName();
            await File.WriteAllLinesAsync(tempFilePath, csvLines);

            mockConfig.Setup(c => c["CsvSettings:Path"]).Returns(tempFilePath);

            var service = new FlightService(mockConfig.Object, mockLogger.Object, new FileReader());//Calling FileReader Implementation to react the new file

            // Act
            var (flights, errors) = await service.GetFlightInfo();

            // Assert
            Assert.Single(flights);
            Assert.Empty(errors);
        }

        /// <summary>
        /// Verifies that records with missing columns are skipped and errors are returned.
        /// </summary>
        [Fact]
        public async Task GetFlightInfo_ShouldSkipsRecord_TooFewColumns()
        {
            // Arrange
            var csvLines = new[]
            {
                "id,aircraft_registration_number,aircraft_type,flight_number,departure_airport,departure_datetime,arrival_airport,arrival_datetime",
                "3,DEF456,Airbus320" // too few columns
            };

            var tempFilePath = Path.GetTempFileName();
            await File.WriteAllLinesAsync(tempFilePath, csvLines);

            mockConfig.Setup(c => c["CsvSettings:Path"]).Returns(tempFilePath);

            var service = new FlightService(mockConfig.Object, mockLogger.Object, new FileReader());

            // Act
            var (flights, errors) = await service.GetFlightInfo();

            // Assert
            Assert.Empty(flights);
            Assert.Single(errors);
            Assert.Contains("Incorrect number of columns", errors[0]);
        }

        /// <summary>
        /// Ensures records with failed data annotation validation are skipped.
        /// </summary>
        [Fact]
        public async Task GetFlightInfo_ShouldSkipsRecord_ValidationFails()
        {
            // Arrange: blank required field `FlightNumber`
            var csvLines = new[]
            {
                "id,aircraft_registration_number,aircraft_type,flight_number,departure_airport,departure_datetime,arrival_airport,arrival_datetime",
                "4,DEF456,Airbus320,,LHR,2024-01-01T10:00:00,JFK,2024-01-01T14:00:00"
            };
            var tempFilePath = Path.GetTempFileName();
            await File.WriteAllLinesAsync(tempFilePath, csvLines);

            mockConfig.Setup(c => c["CsvSettings:Path"]).Returns(tempFilePath);

            var service = new FlightService(mockConfig.Object, mockLogger.Object, new FileReader());

            // Act
            var (flights, errors) = await service.GetFlightInfo();

            // Assert
            Assert.Empty(flights);
            Assert.Single(errors);
            Assert.Contains("FlightNumber", errors[0]);
        }

        

        [Fact]
        public async Task GetInconsistentFlightList_ShouldReturnEmpty_WhenFlightListIsEmpty()
        {
            var service = new TestFlightService([], []);

            var result = await service.GetInconsistentFlightList();

            Assert.Empty(result.Item1);
        }
        [Fact]
        public async void GetInconsistentFlightList_ShouldReturnEmptyList_WhenAllChainsAreConsistent()
        {
            // Arrange
            var testData = new List<FlightInfo>
        {
            new FlightInfo { FlightNumber = "AA100", DepartureAirport = "HEL", ArrivalAirport = "LHR", DepartureDatetime = "2024-01-01 08:00" },
            new FlightInfo { FlightNumber = "AA100", DepartureAirport = "LHR", ArrivalAirport = "JFK", DepartureDatetime = "2024-01-01 12:00" },
            new FlightInfo { FlightNumber = "BB200", DepartureAirport = "HEL", ArrivalAirport = "LHR", DepartureDatetime = "2024-01-01 08:00" },
            new FlightInfo { FlightNumber = "BB200", DepartureAirport = "LHR", ArrivalAirport = "JFK", DepartureDatetime = "2024-01-01 12:00" }
        };

            var service = new TestFlightService(testData, []);

            // Act
            var result = await service.GetInconsistentFlightList();

            // Assert
            Assert.Empty(result.Item1);
        }
        [Fact]
        public async Task GetInconsistentFlightList_ShouldReturnInconsistentFlights()
        {
            // Arrange
            var testData = new List<FlightInfo>
        {
            new FlightInfo { FlightNumber = "XY789", DepartureAirport = "CDG", ArrivalAirport = "FRA", DepartureDatetime = "2024-01-01 15:00" },
            new FlightInfo { FlightNumber = "XY789", DepartureAirport = "AMS", ArrivalAirport = "MAD", DepartureDatetime = "2024-01-01 16:30" }, // Inconsistent
        };

            var service = new TestFlightService(testData, []);

            // Act
            var result = await service.GetInconsistentFlightList();

            // Assert
            Assert.Equal(2, result.Item1.Count);
            Assert.All(result.Item1, f => Assert.Equal("XY789", f.FlightNumber));
        }

        [Fact]
        public async Task GetInconsistentFlightList_ShouldReturnEmptyList_SingleFlightPerNumber()
        {
            // Arrange
            var testData = new List<FlightInfo>
        {
            new FlightInfo { FlightNumber = "ZZ999", DepartureAirport = "HEL", ArrivalAirport = "LHR", DepartureDatetime = "2024-01-01 08:00" }
        };

            var service = new TestFlightService(testData, []);

            // Act
            var result = await service.GetInconsistentFlightList();

            // Assert
            Assert.Empty(result.Item1);
        }

        [Fact]
        public async Task GetInconsistentFlightList_ShouldReturnInconsistentFlights_MixedValidity()
        {
            // Arrange
            var testData = new List<FlightInfo>
        {
            new FlightInfo { FlightNumber = "AB123", DepartureAirport = "HEL", ArrivalAirport = "LHR", DepartureDatetime = "2024-01-01 08:00" },
            new FlightInfo { FlightNumber = "AB123", DepartureAirport = "LHR", ArrivalAirport = "JFK", DepartureDatetime = "2024-01-01 12:00" },

            new FlightInfo { FlightNumber = "CD456", DepartureAirport = "CDG", ArrivalAirport = "FRA", DepartureDatetime = "2024-01-01 09:00" },
            new FlightInfo { FlightNumber = "CD456", DepartureAirport = "AMS", ArrivalAirport = "MAD", DepartureDatetime = "2024-01-01 13:00" } // Inconsistent
        };

            var service = new TestFlightService(testData, []);

            // Act
            var result = await service.GetInconsistentFlightList();

            // Assert
            Assert.Equal(2, result.Item1.Count);
            Assert.All(result.Item1, f => Assert.Equal("CD456", f.FlightNumber));
        }

        [Fact]
        public async Task GetInconsistentFlightList_ShouldIgnoreInvalidEntries()
        {
            var testData = new List<FlightInfo>
                {
                    new FlightInfo { FlightNumber = "XY789", DepartureAirport = "CDG", ArrivalAirport = "FRA", DepartureDatetime = "InvalidDate" },
                    new FlightInfo { FlightNumber = "XY789", DepartureAirport = "FRA", ArrivalAirport = "MAD", DepartureDatetime = "2024-01-01 16:30" }
                };

            var service = new TestFlightService(testData, ["Id 1: Invalid DepartureDatetime"]);

            var result = await service.GetInconsistentFlightList();

            // The invalid one should be ignored, but second alone is not enough to form a chain
            Assert.Empty(result.Item1);
        }

        [Fact]
        public async Task GetInconsistentFlightList_ShouldHandleDuplicateEntries()
        {
            var testData = new List<FlightInfo>
                {
                    new FlightInfo { FlightNumber = "AA100", DepartureAirport = "HEL", ArrivalAirport = "LHR", DepartureDatetime = "2024-01-01 08:00" },
                    new FlightInfo { FlightNumber = "AA100", DepartureAirport = "HEL", ArrivalAirport = "LHR", DepartureDatetime = "2024-01-01 08:00" } 
                };      

            var service = new TestFlightService(testData, []);

            var result = await service.GetInconsistentFlightList();

            Assert.Empty(result.Item1); // Duplicates shouldn't make it inconsistent
        }

    }
}