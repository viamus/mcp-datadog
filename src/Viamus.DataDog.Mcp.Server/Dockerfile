# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["src/Viamus.DataDog.Mcp.Server/Viamus.DataDog.Mcp.Server.csproj", "src/Viamus.DataDog.Mcp.Server/"]
RUN dotnet restore "src/Viamus.DataDog.Mcp.Server/Viamus.DataDog.Mcp.Server.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/Viamus.DataDog.Mcp.Server"
RUN dotnet build "Viamus.DataDog.Mcp.Server.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Viamus.DataDog.Mcp.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "Viamus.DataDog.Mcp.Server.dll"]
