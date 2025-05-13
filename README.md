# FlightDataAnalyzer API (.NET Core)

This project is a RESTful API built with **ASP.NET Core**, designed to **load, validate, and analyze flight data** from a CSV file. It emphasizes structured responses, robust error handling, and extensible service-based architecture, suitable for aviation data processing applications.

---

## Features

- ✅ **CSV-based Flight Data Parsing**
  - Reads from a configurable `.csv` file
  - Flexible file access via an injectable file reader

- 🧪 **Flight Data Validation**
  - Column count and date format verification
  - Field-level model validation with error collection
 
- 🛫 **Automated Flight Chain Inconsistancy Checker**
  - Analyses and returns inconsistent flight chains.

- 📊 **API Responses**
  - Standardized structure for all responses:
    ```json
    {
      "success": true/false,
      "message": "Human-readable message",
      "data": [<List of Data>],
      "errors": [ <List of Errors> ]
    }
    ```

