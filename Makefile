.PHONY: test test-db

# Run all tests (starts DB, runs tests, cleans up)
test:
	docker compose -f docker-compose.test.yml up -d
	sleep 20
	cd backend/Tests && dotnet test
	docker compose -f docker-compose.test.yml down

# Just start the test database on port 3307
test-db:
	docker compose -f docker-compose.test.yml up -d
	sleep 20
	@echo "Test database ready on port 3307"
