# Use the multi-arch SDK image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the .csproj file first and restore dependencies
COPY SwiftDashboard.csproj .
RUN dotnet restore SwiftDashboard.csproj

# Copy the rest of the application code
COPY . .

# Publish the application without specifying runtime (will use native platform)
RUN dotnet publish SwiftDashboard.csproj -c Release -o out --no-self-contained

# Use the multi-arch ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "SwiftDashboard.dll"]