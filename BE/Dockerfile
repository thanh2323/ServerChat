# Use the official .NET 9 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the Solution file
COPY DocuMind.Supabase.sln .

# Copy the Project files
COPY src/DocuMind.API/DocuMind.API.csproj src/DocuMind.API/
COPY src/DocuMind.Application/DocuMind.Application.csproj src/DocuMind.Application/
COPY src/DocuMind.Infrastructure/DocuMind.Infrastructure.csproj src/DocuMind.Infrastructure/
COPY src/DocuMind.Core/DocuMind.Core.csproj src/DocuMind.Core/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish the application
WORKDIR /app/src/DocuMind.API
RUN dotnet publish -c Release -o /app/publish

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (adjust if needed, .NET 8+ often uses 8080 by default in containers)
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "DocuMind.API.dll"]
