# Contributing Guide

Thank you for your interest in contributing to MCP Datadog Server!

This project provides a Model Context Protocol (MCP) server that exposes tools for interacting with Datadog Logs, Monitors, Metrics, Incidents, and Dashboards. Contributions of all kinds are welcome, including bug fixes, documentation improvements, new tools, refactors, and tests.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Project Goals](#project-goals)
- [Ways to Contribute](#ways-to-contribute)
- [Development Setup](#development-setup)
- [Branching & Workflow](#branching--workflow)
- [Commit & PR Guidelines](#commit--pr-guidelines)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Adding a New MCP Tool](#adding-a-new-mcp-tool)
- [Architecture Overview](#architecture-overview)
- [Security](#security)
- [Getting Help](#getting-help)

---

## Code of Conduct

Be respectful, constructive, and collaborative.

Harassment, discrimination, or abusive behavior will not be tolerated. All contributors are expected to interact professionally and respectfully. See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) for details.

---

## Project Goals

- Provide a reliable MCP server for Datadog integration
- Offer useful, composable tools for Logs, Monitors, Metrics, Incidents, and Dashboards
- Keep the server safe-by-default (minimal permissions, no secret leakage)
- Maintain a clean and extensible architecture for future domains (APM, Synthetics, RUM, etc.)

---

## Ways to Contribute

You can contribute by:

- **Reporting bugs** with clear reproduction steps
- **Improving documentation** (README, examples, guides)
- **Adding new MCP tools** or extending existing ones
- **Improving logging**, error handling, and observability
- **Writing or improving tests**
- **Refactoring code** for clarity and maintainability

---

## Development Setup

### Prerequisites

| Requirement | Version | Purpose |
|-------------|---------|---------|
| .NET SDK | 10.0+ | Build and run |
| Docker | Latest | Container deployment (optional) |
| Datadog Account | - | API authentication |

**Required API Keys:**
- API Key (for authentication)
- Application Key (for read access)

**Required Scopes:**
- Logs: Read
- Monitors: Read
- Metrics: Read
- Incidents: Read
- Dashboards: Read

### Clone & Configure

```bash
# 1. Clone the repository
git clone https://github.com/viamus/mcp-datadog.git
cd mcp-datadog

# 2. Create environment file
cp .env.example .env

# 3. Edit .env with your credentials
```

> **Warning**: Never commit `.env` files or hardcode credentials!

### Run Locally

```bash
# Using .NET CLI
dotnet run --project src/Viamus.DataDog.Mcp.Server

# Using Docker
docker compose up -d
```

### Verify Setup

```bash
# .NET CLI (port 5100)
curl http://localhost:5100/health

# Docker (port 8081)
curl http://localhost:8081/health
```

---

## Branching & Workflow

The `main` branch must remain stable. Create feature branches using these patterns:

| Prefix | Purpose | Example |
|--------|---------|---------|
| `feat/` | New features | `feat/add-apm-tools` |
| `fix/` | Bug fixes | `fix/metric-query-error` |
| `docs/` | Documentation | `docs/improve-readme` |
| `chore/` | Maintenance | `chore/update-deps` |
| `test/` | Test additions | `test/incident-tools` |

### Workflow

1. Create a branch from `main`
2. Make your changes
3. Add or update tests
4. Run tests locally: `dotnet test`
5. Open a Pull Request targeting `main`

---

## Commit & PR Guidelines

### Commits

Use [Conventional Commits](https://www.conventionalcommits.org/) style:

```
feat: add query_apm_traces tool
fix: handle metric query timeout gracefully
docs: clarify API key permissions
test: add unit tests for dashboard tools
chore: bump dependencies
```

### Pull Requests

A good PR includes:

- **What** changed and **why**
- Link to related issue (if applicable)
- Notes about breaking changes (avoid if possible)
- Confirmation that no secrets were introduced
- Logs or screenshots when helpful

---

## Coding Standards

### General Principles

- Prefer clarity over cleverness
- Keep MCP tools focused (single responsibility)
- Avoid leaking secrets via logs or exceptions
- Validate inputs and return consistent outputs
- Errors should be actionable and safe

### .NET Guidelines

- Use `async/await` consistently for I/O operations
- Favor dependency injection
- Keep handlers/controllers thin
- Put business logic in services
- Keep models and DTOs explicit and simple
- Use `sealed record` for DTOs when possible (immutability)

---

## Testing

When behavior changes, tests should be added or updated.

### Running Tests

```bash
# All tests
dotnet test

# Specific test class
dotnet test --filter "FullyQualifiedName~LogToolsTests"
dotnet test --filter "FullyQualifiedName~MonitorToolsTests"
dotnet test --filter "FullyQualifiedName~MetricToolsTests"
dotnet test --filter "FullyQualifiedName~IncidentToolsTests"
dotnet test --filter "FullyQualifiedName~DashboardToolsTests"

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

```
tests/Viamus.DataDog.Mcp.Server.Tests/
├── Models/     # DTO serialization and equality tests
└── Tools/      # Tool behavior tests with mocked services
```

### Testing Layers

- **Unit tests**: Services and mapping logic
- **Contract tests**: MCP tool outputs
- **Integration tests**: HTTP endpoints (optional but encouraged)

---

## Adding a New MCP Tool

### Checklist

Before creating a new tool, ensure it has:

- [ ] Clear and descriptive name (snake_case)
- [ ] Single responsibility
- [ ] Stable inputs and outputs
- [ ] Parameter validation
- [ ] Safe and consistent error handling
- [ ] Unit tests
- [ ] Documentation in README.md

### Steps

1. **Add tool implementation** in `src/.../Tools/`

```csharp
[McpServerToolType]
public sealed class MyTools
{
    private readonly IDatadogClient _client;
    private readonly ILogger<MyTools> _logger;

    public MyTools(IDatadogClient client, ILogger<MyTools> logger)
    {
        _client = client;
        _logger = logger;
    }

    [McpServerTool(Name = "my_tool")]
    [Description("Description of what this tool does")]
    public async Task<string> MyToolAsync(
        [Description("Parameter description")] string param,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing my_tool with param: {Param}", param);

            var result = await _client.MyMethodAsync(param, cancellationToken);

            var response = new
            {
                // Map result to response object
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to execute my_tool");
            return JsonSerializer.Serialize(new { error = $"Failed to execute: {ex.Message}" });
        }
    }
}
```

2. **Add service method** in `src/.../Services/`
   - Add signature to `IDatadogClient.cs`
   - Implement in `DatadogClient.cs`

3. **Add DTOs if needed** in `src/.../Models/`
   - Include `[JsonPropertyName]` attributes for API mapping
   - Include XML documentation

4. **Add tests** in `tests/.../Tools/`

5. **Update README.md** with the new tool

> Tools are auto-registered via `.WithToolsFromAssembly()`

---

## Architecture Overview

### Project Structure

```
src/Viamus.DataDog.Mcp.Server/
├── Configuration/
│   └── DatadogSettings.cs           # Configuration binding
├── Models/
│   ├── LogSearchResponse.cs         # Log search results
│   ├── MonitorModels.cs             # Monitor DTOs
│   ├── MetricModels.cs              # Metric DTOs
│   ├── IncidentModels.cs            # Incident DTOs
│   └── DashboardModels.cs           # Dashboard DTOs
├── Services/
│   ├── IDatadogClient.cs            # Service interface
│   └── DatadogClient.cs             # HTTP client implementation
├── Tools/
│   ├── SearchLogsTool.cs            # Log search tool (1)
│   ├── MonitorsTool.cs              # Monitor tools (2)
│   ├── MetricsTool.cs               # Metric tools (3)
│   ├── IncidentsTool.cs             # Incident tools (2)
│   └── DashboardsTool.cs            # Dashboard tools (2)
└── Program.cs                       # Entry point & DI
```

### Key Patterns

| Pattern | Description |
|---------|-------------|
| Dependency Injection | Services registered via `AddHttpClient` |
| Interface-based design | Enables testing with mocks |
| JSON serialization | CamelCase responses with `System.Text.Json` |
| Error handling | JSON error responses, no exceptions to client |
| Logging | Structured logging with `ILogger<T>` |

### Datadog API Endpoints

| API Version | Used For |
|-------------|----------|
| `/api/v1/` | Monitors, Metrics, Dashboards |
| `/api/v2/` | Logs, Incidents |

### Datadog Sites

| Region | Base URL |
|--------|----------|
| US1 | `https://api.datadoghq.com` |
| US3 | `https://api.us3.datadoghq.com` |
| US5 | `https://api.us5.datadoghq.com` |
| EU | `https://api.datadoghq.eu` |
| AP1 | `https://api.ap1.datadoghq.com` |

---

## Security

- **Never commit secrets** (API keys, Application keys)
- **Avoid logging sensitive data**
- **Validate all external inputs**
- **Use environment variables** or user secrets for credentials

If you discover a security vulnerability:
- **Do NOT open a public issue**
- Contact the maintainers privately (see [SECURITY.md](SECURITY.md))

---

## Getting Help

If you need help:

1. Check the [README](README.md) first
2. Search existing [issues](https://github.com/viamus/mcp-datadog/issues)
3. Open a new issue with:
   - Expected behavior
   - Actual behavior
   - Reproduction steps
   - Logs (with secrets removed)
   - Environment details (OS, .NET version, Docker version)

---

**Thank you for contributing to MCP Datadog Server!**

Your help makes this project better for everyone.
