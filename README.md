# MCP Datadog Server

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![MCP](https://img.shields.io/badge/MCP-Compatible-blue)](https://modelcontextprotocol.io/)

A [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) server for Datadog integration, enabling AI assistants to interact with Datadog Logs, Monitors, Metrics, Incidents, and Dashboards.

---

## Quick Start

Get up and running in 3 steps:

### 1. Clone and configure

```bash
git clone https://github.com/viamus/mcp-datadog.git
cd mcp-datadog
```

Copy and edit the environment file with your Datadog credentials:

```bash
cp .env.example .env
```

Edit `.env` with your Datadog API keys:

```env
DATADOG_API_KEY=your-api-key-here
DATADOG_APPLICATION_KEY=your-application-key-here
DATADOG_SITE=datadoghq.com
```

> **Need API Keys?** See [Creating Datadog API Keys](#creating-datadog-api-keys) below.

### 2. Run the server

**Option A - Docker (recommended):**
```bash
docker compose up -d
# Server runs at http://localhost:8081
```

**Option B - .NET CLI:**
```bash
dotnet run --project src/Viamus.DataDog.Mcp.Server
# Server runs at http://localhost:5100
```

### 3. Verify it's working

```bash
# Docker
curl http://localhost:8081/health

# .NET CLI
curl http://localhost:5100/health
```

You should see: `Healthy`

---

## About

This project implements an MCP server that exposes tools for querying and managing Logs, Monitors, Metrics, Incidents, and Dashboards in Datadog. It can be used with any compatible MCP client, such as Claude Desktop, Claude Code, or other assistants that support the protocol.

---

## Available Tools

### Log Tools

| Tool | Description |
|------|-------------|
| `search_logs` | Search logs using Datadog query syntax with time range and pagination |

### Monitor Tools

| Tool | Description |
|------|-------------|
| `list_monitors` | Lists all monitors with optional filters (name, tags, state) |
| `get_monitor` | Gets details of a specific monitor by ID including thresholds and state |

### Metric Tools

| Tool | Description |
|------|-------------|
| `query_metrics` | Query time-series metric data using Datadog query syntax |
| `list_metrics` | Lists available metric names, optionally filtered by host |
| `get_metric_metadata` | Gets metadata for a specific metric (type, description, unit) |

### Incident Tools

| Tool | Description |
|------|-------------|
| `list_incidents` | Lists incidents with pagination |
| `get_incident` | Gets detailed information about a specific incident |

### Dashboard Tools

| Tool | Description |
|------|-------------|
| `list_dashboards` | Lists all dashboards with basic information |
| `get_dashboard` | Gets detailed dashboard configuration including widgets |

---

## Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ | Required for local development |
| [Docker](https://www.docker.com/) | Latest | Recommended for running |
| Datadog Account | - | With API and Application Keys |

### Creating Datadog API Keys

1. Go to your Datadog organization settings: `https://app.datadoghq.com/organization-settings/api-keys`
2. Click **+ New Key** to create an API Key
3. Go to Application Keys: `https://app.datadoghq.com/organization-settings/application-keys`
4. Click **+ New Key** to create an Application Key

**Required Scopes:**

| Scope | Permission | Required for |
|-------|------------|--------------|
| Logs | Read | Log search operations |
| Monitors | Read | Monitor listing and details |
| Metrics | Read | Metric queries and metadata |
| Incidents | Read | Incident listing and details |
| Dashboards | Read | Dashboard listing and details |

5. Copy both keys immediately (you won't see them again!)

**Datadog Sites:**

| Region | Site URL |
|--------|----------|
| US1 (default) | `datadoghq.com` |
| US3 | `us3.datadoghq.com` |
| US5 | `us5.datadoghq.com` |
| EU | `datadoghq.eu` |
| AP1 | `ap1.datadoghq.com` |

---

## Running Options

### Option 1: Docker Compose (Recommended)

Best for: Production use, quick setup without .NET installed

```bash
docker compose up -d
```

Server URL: `http://localhost:8081`

**Useful commands:**
```bash
docker compose logs -f          # View logs
docker compose down             # Stop the server
docker compose up -d --build    # Rebuild and start
```

### Option 2: .NET CLI

Best for: Development, debugging

```bash
# Set environment variables
export Datadog__ApiKey="your-api-key"
export Datadog__ApplicationKey="your-application-key"

# Run the server
dotnet run --project src/Viamus.DataDog.Mcp.Server
```

Server URL: `http://localhost:5100`

### Option 3: Self-Contained Executable

Best for: Deployment without .NET runtime

```bash
# Windows
dotnet publish src/Viamus.DataDog.Mcp.Server -c Release -r win-x64 -o ./publish/win-x64

# Linux
dotnet publish src/Viamus.DataDog.Mcp.Server -c Release -r linux-x64 -o ./publish/linux-x64

# macOS (Intel)
dotnet publish src/Viamus.DataDog.Mcp.Server -c Release -r osx-x64 -o ./publish/osx-x64

# macOS (Apple Silicon)
dotnet publish src/Viamus.DataDog.Mcp.Server -c Release -r osx-arm64 -o ./publish/osx-arm64
```

Then run the executable directly:
```bash
# Windows
./publish/win-x64/Viamus.DataDog.Mcp.Server.exe

# Linux/macOS
./publish/linux-x64/Viamus.DataDog.Mcp.Server
```

---

## Client Configuration

### Claude Desktop

**Option A - Using CLI (recommended):**
```bash
claude mcp add datadog --transport http http://localhost:8081
```

**Option B - Manual configuration:**

Edit `claude_desktop_config.json`:
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "datadog": {
      "url": "http://localhost:8081"
    }
  }
}
```

### Claude Code

Run from your project directory:
```bash
claude mcp add datadog --transport http http://localhost:8081
```

Or add manually to `.claude/settings.json`:
```json
{
  "mcpServers": {
    "datadog": {
      "type": "http",
      "url": "http://localhost:8081"
    }
  }
}
```

> **Note**: Use port `5100` if running with .NET CLI, or `8081` if running with Docker.

---

## Usage Examples

After configuring the MCP client, you can ask questions like:

### Logs

- "Search for error logs in the last hour"
- "Find logs with status:error from service:api"
- "Show me the latest logs containing 'timeout'"
- "Search for logs from host:web-server-01 in the last 15 minutes"

### Monitors

- "List all monitors that are currently alerting"
- "Show me monitors tagged with env:production"
- "Get details of monitor #12345"
- "What monitors are in a warning state?"
- "Find monitors with 'CPU' in the name"

### Metrics

- "Query the average CPU usage across all hosts in the last hour"
- "Show me memory usage for host:web-01"
- "List all available metrics"
- "Get metadata for the system.cpu.user metric"
- "Query request latency by service for the last 24 hours"

### Incidents

- "List all active incidents"
- "Show me incidents from the last week"
- "Get details of incident INC-123"
- "What's the current incident affecting production?"

### Dashboards

- "List all available dashboards"
- "Show me the details of the 'Production Overview' dashboard"
- "What widgets are on dashboard abc-123?"

---

## Configuration Options

### Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `Datadog__ApiKey` | Yes | - | Your Datadog API Key |
| `Datadog__ApplicationKey` | Yes | - | Your Datadog Application Key |
| `Datadog__Site` | No | `datadoghq.com` | Datadog site/region |

### appsettings.json

```json
{
  "Datadog": {
    "ApiKey": "your-api-key",
    "ApplicationKey": "your-application-key",
    "Site": "datadoghq.com"
  }
}
```

### User Secrets (Development)

```bash
cd src/Viamus.DataDog.Mcp.Server
dotnet user-secrets set "Datadog:ApiKey" "your-api-key"
dotnet user-secrets set "Datadog:ApplicationKey" "your-application-key"
```

---

## Troubleshooting

### Common Issues

<details>
<summary><strong>Health check returns error or connection refused</strong></summary>

1. Verify the server is running:
   ```bash
   # Docker
   docker compose ps

   # Check if port is in use
   netstat -an | grep 8081  # or 5100
   ```

2. Check logs for errors:
   ```bash
   # Docker
   docker compose logs

   # .NET CLI - errors appear in terminal
   ```
</details>

<details>
<summary><strong>Authentication failed / 403 Forbidden</strong></summary>

1. Verify your API Key and Application Key are correct
2. Check keys haven't been revoked in Datadog
3. Ensure keys have required scopes/permissions
4. Verify the Datadog site is correct for your region
</details>

<details>
<summary><strong>No data returned from queries</strong></summary>

1. Verify the time range includes data
2. Check your query syntax matches Datadog's query language
3. Ensure your API key has access to the requested data
</details>

<details>
<summary><strong>Docker: Container exits immediately</strong></summary>

1. Check if `.env` file exists and has the required credentials
2. View logs: `docker compose logs`
3. Ensure port 8081 is not in use by another application
</details>

<details>
<summary><strong>.NET CLI: dotnet run fails</strong></summary>

1. Verify .NET 10 SDK is installed: `dotnet --version`
2. Restore packages: `dotnet restore`
3. Check environment variables are set correctly
</details>

---

## Project Structure

```
mcp-datadog/
├── src/
│   └── Viamus.DataDog.Mcp.Server/
│       ├── Configuration/      # App configuration classes
│       ├── Models/             # DTOs and data models
│       ├── Services/           # Datadog API client
│       ├── Tools/              # MCP tool implementations
│       ├── Program.cs          # Entry point
│       ├── appsettings.json    # App settings
│       └── Dockerfile          # Container definition
├── tests/
│   └── Viamus.DataDog.Mcp.Server.Tests/
├── .github/                    # GitHub templates
├── .env.example                # Environment template
├── docker-compose.yml          # Docker orchestration
└── LICENSE                     # MIT License
```

---

## API Reference

### Log DTOs

#### LogSearchResponse
Search results containing log entries with metadata and pagination cursor.

#### LogEntry
Individual log entry with Id, Type, and Attributes (timestamp, status, service, host, message, tags).

### Monitor DTOs

#### DatadogMonitor
Complete monitor details including Id, Name, Type, Query, Message, OverallState, Tags, Priority, Options, and State.

#### MonitorOptions
Monitor configuration including thresholds (Critical, Warning, Ok), notification settings, and timeouts.

### Metric DTOs

#### MetricQueryResponse
Query results containing time-series data with status, query, time range, and series data.

#### MetricSeries
Individual metric series with metric name, display name, unit, scope, tags, aggregation, and data points.

#### MetricMetadata
Metric metadata including type, description, unit, and statsd interval.

### Incident DTOs

#### Incident
Incident details including Id, Title, Severity, State, CustomerImpacted, timestamps (Created, Detected, Resolved), and time metrics (TimeToDetect, TimeToResolve).

### Dashboard DTOs

#### DashboardSummary
Dashboard overview with Id, Title, Description, LayoutType, URL, and author information.

#### Dashboard
Complete dashboard with all configuration including Widgets, TemplateVariables, and NotifyList.

#### DashboardWidget
Widget configuration including Definition (type, title, requests/queries) and Layout (position and size).

---

## Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Building

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release
```

---

## License

This project is licensed under the [MIT License](LICENSE).

---

## Links

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Datadog API Documentation](https://docs.datadoghq.com/api/)
- [Report an Issue](https://github.com/viamus/mcp-datadog/issues)
