set DOCKER_BUILDKIT=0 && docker build -t mcp-datadog-mcp-datadog:latest -f src/Viamus.DataDog.Mcp.Server/Dockerfile .
docker compose up -d --no-build
