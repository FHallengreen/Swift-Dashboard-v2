# Swift Display Dashboard

A real-time business dashboard application designed for Raspberry Pi and TV displays. Built with React, C#, and MySQL.

## Testing

### Quick Start

**Run all tests:**
```bash
# Linux/macOS
./run-tests.sh

# Windows
run-tests.bat

# Or manually
docker compose -f docker-compose.test.yml up -d
sleep 20
cd backend/Tests && dotnet test
docker compose -f docker-compose.test.yml down
```

**During development** (keep DB running):
```bash
# Start test database (port 3307)
docker compose -f docker-compose.test.yml up -d

# Run tests as many times as needed
cd backend/Tests
dotnet test

# Stop when done
docker compose -f docker-compose.test.yml down
```

**In VS Code:** Press `Ctrl+Shift+P` → "Tasks: Run Task" → "Run All Tests"

### Test Structure

- **Unit Tests**: 19 tests for service logic (no DB needed)
- **Integration Tests**: 11 tests for API endpoints (requires DB)

Tests run automatically in GitHub Actions on push/PR.
