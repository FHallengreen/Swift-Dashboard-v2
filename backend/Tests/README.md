# SwiftDashboard Tests

This directory contains unit and integration tests for the Swift Dashboard backend.

## Test Structure

- **Services/** - Unit tests for service layer (InvoiceService, InfoService, HolidayService)
- **Controllers/** - Integration tests for API controllers

## Running Tests

### Prerequisites

For integration tests, you need a MySQL test database running. The tests use the connection string from `appsettings.Test.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=swift_dashboard_test;User=root;Password=SwiftDK!;"
  }
}
```

### Start Test Database

If you have Docker running, you can start a test database:

```bash
docker run --name mysql-test -e MYSQL_ROOT_PASSWORD=SwiftDK! -e MYSQL_DATABASE=swift_dashboard_test -p 3306:3306 -d mysql:8.0
```

### Run All Tests

```bash
dotnet test
```

### Run Only Unit Tests

```bash
dotnet test --filter FullyQualifiedName~Services
```

### Run Only Integration Tests

```bash
dotnet test --filter FullyQualifiedName~Controllers
```

## Test Database Cleanup

Integration tests automatically:
- Create the test database schema before tests
- Clean up test data after each test
- Delete the test database when tests complete

## Notes

- Unit tests use mocked dependencies (no database required)
- Integration tests use a real MySQL database for end-to-end testing
- Each test class cleans up its own data to prevent test interference
