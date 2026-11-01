# Bizcord Messaging Service

The Messaging service for Bizcord, a Discord-like communication platform. It provides an API for creating, reading, editing, and deleting channel messages.

`POST /api/messages` stores a message in SQLite and publishes a `MessagePostedEvent` through RabbitMQ using EasyNetQ.

## Start

Prerequisites: Docker with Compose and Linux container support. The .NET 10 SDK is required for local development and tests outside Docker.

From the repository root:

```powershell
docker compose up --build -d --wait
```

- API: <http://127.0.0.1:8000/api/messages>
- Liveness check: <http://127.0.0.1:8000/health>
- RabbitMQ management: <http://127.0.0.1:15672> (local development login: `bizcord` / `bizcord-local-dev`).

SQLite and RabbitMQ data use named Docker volumes and survive an ordinary stop/restart. The included credentials are for this localhost-only development setup.

## Demonstrate the API

```powershell
.\scripts\demo.ps1
```

The script checks message creation, retrieval, channel filtering, editing, and deletion. It prints the generated IDs and removes its sample message. [HTTP request examples](apps/messaging-microservice/messages.http) are also available for manual testing.

| Method | Route | Result |
| --- | --- | --- |
| POST | `/api/messages` | Create and publish; `201` with a Location header and `MessageDto` |
| GET | `/api/messages` | List messages; optional `?channelId=<guid>` filter |
| GET | `/api/messages/{id}` | Read a message; `200` or `404` |
| PUT | `/api/messages/{id}` | Replace its content; `200`, `400`, or `404` |
| DELETE | `/api/messages/{id}` | Delete; `204` or `404` |

`ChannelId` and `AuthorId` must be nonempty GUIDs. Content must contain non-whitespace text and be at most 2,000 characters. Editing preserves the message's original ID, channel, author, and UTC posting time.

## Tests

Run the domain and API tests with a temporary SQLite database and a fake message publisher:

```powershell
dotnet test --solution Bizcord.slnx -c Release --filter-not-trait "Category=BrokerIntegration"
```

Run all tests, including RabbitMQ publication and subscription:

```powershell
docker compose up -d --wait rabbitmq
dotnet test --solution Bizcord.slnx -c Release
```

Build and start the containers, run all tests, and check the API:

```powershell
.\scripts\verify.ps1
```

Tests use xUnit and Microsoft Testing Platform, configured in `global.json`. The broker test subscribes before posting, checks the received event fields, and retrieves the saved message over HTTP.

## Local development

```powershell
docker compose up -d --wait rabbitmq
dotnet run --project apps/messaging-microservice/src/Bizcord.Messaging.Api --urls http://127.0.0.1:8000
```

Configuration is loaded from `appsettings.json` and environment variables:

| Variable | Local default | Compose value |
| --- | --- | --- |
| `RABBITMQ_HOST` | `127.0.0.1` | `rabbitmq` (Compose DNS name) |
| `RabbitMq__Username` | `bizcord` | `bizcord` |
| `RabbitMq__Password` | `bizcord-local-dev` | `bizcord-local-dev` |
| `ConnectionStrings__Messages` | `Data Source=data/messages.db` | `Data Source=/app/data/messages.db` |

To stop the project while keeping its data:

```powershell
docker compose down
```

## Repository and documentation

```text
apps/messaging-microservice/
  src/Bizcord.Messaging.Api/     Controllers, Application, Domain, Infrastructure
  tests/                       Unit tests and HTTP/broker integration tests
  Dockerfile
  README.md
packages/
  MessageClient/src/           Broker-independent interface and EasyNetQ adapter
  Shared.Contracts/            Public DTOs, requests, and MessagePostedEvent
docs/
  design.md                    Domains, boundaries, ownership, and interactions
  requirements.md              Each assignment task mapped to this implementation
  testing.md                   Verification procedure and recorded results
```

See [design](docs/design.md), [assignment coverage](docs/requirements.md), and [testing](docs/testing.md).

`Directory.Build.props` contains the build settings shared by all projects: the .NET target framework, nullable reference types, implicit usings, warnings as errors, and NuGet lock-file generation. Each `.csproj` inherits these settings.

## Scope and limitations

Channel management, user profiles, and event consumers are separate services described in the design. Messaging accepts channel and author IDs without validating them against those services. Authentication and authorization are not implemented.

Only message creation publishes an event. Edits and deletions update storage without publishing additional events.

Saving to SQLite and publishing to RabbitMQ are separate operations. If publication fails, the API returns `503` with `persisted: true` and the saved `messageId`. The message remains available through GET. There is no outbox, automatic publication retry, or request deduplication, so repeating POST can create duplicates.

The schema is created on startup; schema migrations and list pagination are not implemented. `/health` reports application liveness without checking broker connectivity.
