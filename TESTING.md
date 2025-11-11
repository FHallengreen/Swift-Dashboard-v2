# Testing Quick Reference

## Quick Commands

### All-in-One (Recommended for CI/local verification)
```bash
# Linux/macOS
./run-tests.sh

# Windows
run-tests.bat

# Make (Linux/macOS)
make test
```

### Development Workflow (Keep DB running)
```bash
# 1. Start test database once
make test-up
# OR: docker compose -f docker-compose.test.yml up -d

# 2. Run tests as many times as needed
cd backend/Tests
dotnet test

# 3. Stop database when done
make test-down
# OR: docker compose -f docker-compose.test.yml down -v
```

### Watch Mode (Auto-rerun on changes)
```bash
# Start DB first (see above)
make test-watch
# OR: cd backend/Tests && dotnet watch test
```

### Selective Testing
```bash
# Unit tests only (no DB needed)
make test-unit

# Integration tests only (DB needed)
make test-integration

# Specific test
cd backend/Tests
dotnet test --filter "FullyQualifiedName~InvoiceService"
```

## VS Code

Use `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac) and search for "Tasks: Run Task":

- **Run All Tests** - Complete test run with DB setup/cleanup
- **Start Test Database** - Start DB on port 3307
- **Stop Test Database** - Stop and remove DB
- **Run Tests (DB Running)** - Run tests assuming DB is already running
- **Run Tests in Watch Mode** - Auto-rerun on file changes
- **Run Unit Tests Only** - Services tests only
- **Run Integration Tests Only** - Controllers tests only

## Troubleshooting

### Port 3307 already in use
```bash
# Check what's using it
docker ps -a | grep 3307

# Stop test database
docker stop mysql-swift-test
docker rm mysql-swift-test
```

### Tests failing with "Table already exists"
```bash
# Clean database and restart
docker compose -f docker-compose.test.yml down -v
docker compose -f docker-compose.test.yml up -d
sleep 15
```

### MySQL not ready
Wait 15-20 seconds after starting the container, or check health:
```bash
docker exec mysql-swift-test mysqladmin ping -h localhost -u root -pSwiftDK!
```

## Test Database Details

- **Image**: MySQL 8.0
- **Port**: 3307 (to avoid conflict with dev DB on 3306)
- **Database**: `swift_dashboard_test`
- **User**: `root`
- **Password**: `SwiftDK!`
- **Connection**: `Server=localhost;Port=3307;Database=swift_dashboard_test;User=root;Password=SwiftDK!`

## CI/CD

### GitHub Actions
- **test.yml**: Runs on push/PR to `main` or `develop`
- **deploy.yml**: Runs tests before deploying to Raspberry Pi

View test results:
1. Go to Actions tab in GitHub
2. Click on the workflow run
3. Download test results artifact (if available)
