@echo off
echo Starting test database...
docker compose -f docker-compose.test.yml up -d

echo Waiting 20 seconds for MySQL...
timeout /t 20 /nobreak > nul

echo Running tests...
cd backend\Tests
dotnet test

echo Cleaning up...
cd ..\..

docker compose -f docker-compose.test.yml down

echo Done!
