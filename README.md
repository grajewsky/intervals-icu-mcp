# intervals-mcp

A [Model Context Protocol](https://modelcontextprotocol.io) server that connects your [intervals.icu](https://intervals.icu) training data to LLM assistants like Claude.

Ask questions such as *"How did my training load change over the last 8 weeks?"* or *"What workouts do I have planned this week?"* and let the model pull the answers straight from your intervals.icu account.

Built with C# / .NET 9, ASP.NET Core and PostgreSQL.

## Features

- **Activity sync & cache**: pulls activities from intervals.icu into PostgreSQL, so queries are fast and don't hit API limits
- **Weekly summaries**: distance, time and training load aggregated per week
- **Wellness data**: fitness (CTL), fatigue (ATL), form, HRV, resting HR, sleep
- **Planned workouts**: upcoming events from your intervals.icu calendar
- **Streamable HTTP transport**: works with Claude Code, Claude Desktop and any MCP-compatible client
- **One-command setup** with Docker Compose

## Available tools

| Tool | Description |
|---|---|
| `sync_activities` | Fetch activities from the last N days and store them locally |
| `get_activities` | List cached activities, optionally filtered by type (`Ride`, `Run`, ...) |
| `weekly_summary` | Weekly totals of distance, hours and training load |
| `get_wellness` | Daily wellness metrics for the last N days |
| `get_planned_workouts` | Planned workouts for the next N days |

> `get_activities` and `weekly_summary` read from the local cache. Run `sync_activities` first.

## Quick start

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) with Docker Compose
- An intervals.icu API key: **Settings → Developer Settings → API Key**

### Run

```bash
git clone <repository-url>
cd intervals-mcp

cp .env.example .env
# edit .env and set INTERVALS_API_KEY

docker compose up -d --build
```

Check that it's running:

```bash
curl http://localhost:8080/health
# {"status":"ok"}
```

### Connect a client

**Claude Code**

```bash
claude mcp add --transport http intervals http://localhost:8080/mcp
```

**OpenAI Codex CLI**

```bash
codex mcp add intervals --url http://localhost:8080/mcp
```

Or add it manually to `~/.codex/config.toml`:

```toml
[mcp_servers.intervals]
url = "http://localhost:8080/mcp"
```

Verify with `codex mcp list`, or run `/mcp` inside a Codex session.

<details>
<summary>Older Codex versions without HTTP support</summary>

Older releases support only stdio servers. Bridge the HTTP endpoint with [`mcp-remote`](https://www.npmjs.com/package/mcp-remote) (requires Node.js):

```toml
[mcp_servers.intervals]
command = "npx"
args = ["-y", "mcp-remote", "http://localhost:8080/mcp"]
```

</details>

**Other MCP clients**: point them to the Streamable HTTP endpoint at `http://localhost:8080/mcp`.

## Configuration

Settings are read from `appsettings.json` and can be overridden with environment variables (`__` separates nested keys).

| Variable | Default | Description |
|---|---|---|
| `INTERVALS_API_KEY` | (required) | Your intervals.icu API key |
| `INTERVALS_ATHLETE_ID` | `0` | Athlete ID; `0` means the owner of the API key |
| `POSTGRES_DB` | `intervals` | Database name |
| `POSTGRES_USER` | `intervals` | Database user |
| `POSTGRES_PASSWORD` | `intervals` | Database password |

## Development

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download).

```bash
# start only the database
docker compose up -d db

cd src/IntervalsMcp
dotnet user-secrets init
dotnet user-secrets set "Intervals:ApiKey" "<your-api-key>"
dotnet watch
```

### Project structure

```
src/IntervalsMcp/
├── Program.cs     # host setup: DI, configuration, EF Core, MCP server
├── Intervals/     # typed HttpClient for the intervals.icu API
├── Data/          # EF Core DbContext and entities
└── Tools/         # MCP tools exposed to the model
```

Adding a tool is just a method with attributes:

```csharp
[McpServerTool(Name = "my_tool")]
[Description("What the tool does. The model reads this description.")]
public async Task<string> MyTool([Description("Parameter description")] int days = 7) { ... }
```

### Resetting the database

The schema is created on startup with `EnsureCreated` (migrations are on the roadmap). After changing an entity, recreate the database:

```bash
docker compose down -v
```

### Learning resources

This project started as a way to learn C#. [docs/NAUKA-CSHARP.md](docs/NAUKA-CSHARP.md) (in Polish) walks through every C# and .NET concept used in the codebase.

## Security

The MCP endpoint has **no authentication**. Docker Compose binds it to `127.0.0.1` only. Don't expose it to a network without putting an auth layer in front of it. Your `.env` file contains your API key and is git-ignored.

## Roadmap

- [ ] EF Core migrations
- [ ] Background sync service
- [ ] Wellness history stored in the database
- [ ] Creating planned workouts
- [ ] Endpoint authentication
- [ ] Unit tests

## Contributing

Issues and pull requests are welcome. For larger changes, please open an issue first to discuss what you'd like to change.

## License

<!-- TODO: choose a license (e.g. MIT) and add a LICENSE file -->

## Acknowledgements

- [intervals.icu](https://intervals.icu) by David Tinker
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)

*This project is not affiliated with intervals.icu.*
