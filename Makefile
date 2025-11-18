.PHONY: test test-db deploy test-and-deploy

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

# Deploy the application
deploy:
	docker compose up -d

# Run tests and deploy if successful
test-and-deploy: test deploy
