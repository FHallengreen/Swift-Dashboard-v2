#!/bin/bash
set -e

echo "Starting test database..."
docker compose -f docker-compose.test.yml up -d

echo "Waiting 10 seconds for MySQL..."
sleep 10

echo "Running tests..."
cd backend/Tests
dotnet test

echo "Cleaning up..."
cd ../..
docker compose -f docker-compose.test.yml down

echo "Done!"
