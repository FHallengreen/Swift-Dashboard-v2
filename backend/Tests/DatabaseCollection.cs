using Xunit;

namespace SwiftDashboard.Tests;

// This collection forces all integration tests to run sequentially
// to avoid database conflicts
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
