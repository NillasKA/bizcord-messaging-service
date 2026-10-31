# Bizcord Messaging Service

System Integration (E2026), compulsory assignment #1: weeks 36–40. This repository implements **one** bounded context of Bizcord, a Discord-like backend: creating, reading, editing, and deleting channel messages.

The first feature follows **Week 40 Track A**: `POST /api/messages` saves a message to SQLite and publishes the prescribed `MessagePostedEvent` through RabbitMQ using EasyNetQ.

## Start

Prerequisites: Docker Desktop running with Linux containers. The .NET 10 SDK is also needed to run tests outside Docker.

From the repository root:

```powershell
docker compose up --build -d --wait
```

- API: <http://localhost:8000/api/messages>
- Liveness check: <http://localhost:8000/health>
- RabbitMQ management: <http://localhost:15672> (local development login: `bizcord` / `bizcord-local-dev`).

SQLite and RabbitMQ data use named Docker volumes and survive an ordinary stop/restart. The included credentials are for this localhost-only development setup.

## Demonstrate the API

```powershell
.\scripts\demo.ps1
```

This creates a fresh sample message, retrieves and filters it, edits it, and deletes that sample. The script prints the generated IDs and verifies HTTP behaviour. The event delivery is verified by the broker test below. Alternatively, open [the HTTP request examples](apps/messaging-microservice/messages.http) in an editor with HTTP client support.

| Method | Route | Result |
| --- | --- | --- |
| POST | `/api/messages` | Create and publish; `201` with a Location header and `MessageDto` |
| GET | `/api/messages` | List messages; optional `?channelId=<guid>` filter |
| GET | `/api/messages/{id}` | Read a message; `200` or `404` |
| PUT | `/api/messages/{id}` | Replace its content; `200`, `400`, or `404` |
| DELETE | `/api/messages/{id}` | Delete; `204` or `404` |

`ChannelId` and `AuthorId` must be nonempty GUIDs. Content must contain non-whitespace text and be at most 2,000 characters. Editing preserves the message's original ID, channel, author, and UTC posting time.

## Tests

Fast tests with a real temporary SQLite database and a fake message publisher; no broker needed:

```powershell
dotnet test --solution Bizcord.slnx -c Release --filter-not-trait "Category=BrokerIntegration"
```

All tests, including real RabbitMQ publication and subscription (start RabbitMQ first):

```powershell
docker compose up -d --wait rabbitmq
dotnet test --solution Bizcord.slnx -c Release
```

One command for build, all tests, container startup, and the API demonstration:

```powershell
.\scripts\verify.ps1
```

The projects use xUnit and the .NET 10 Microsoft Testing Platform runner configured in `global.json`. The broker test subscribes before posting, receives a real message through RabbitMQ, checks all five event fields, and retrieves the saved message over HTTP.

## Local development

```powershell
docker compose up -d --wait rabbitmq
dotnet run --project apps/messaging-microservice/src/Bizcord.Messaging.Api --urls http://localhost:8000
```

Configuration uses normal .NET configuration/environment variables:

| Variable | Local default | Compose value |
| --- | --- | --- |
| `RABBITMQ_HOST` | `localhost` | `rabbitmq` (Compose DNS name) |
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

## Deliberate scope and limitations

This is a small course backend. Channel management, user profiles, and downstream consumers are external bounded contexts described in the design. This service accepts their IDs; authentication and checking those external records are future integrations.

Only creation publishes `MessagePostedEvent`. Editing and deletion have CRUD behaviour; additional event contracts for those actions are future work.

Saving to SQLite and publishing to RabbitMQ are separate operations. If publication fails, the API returns `503` with `persisted: true` and the saved `messageId`. The caller can retrieve that message. Automatic retries, deduplication, and an outbox are future work; blindly repeating POST could duplicate a message. `/health` is a liveness check, not a broker-readiness guarantee.

The schema is created on startup for this first version. Future schema changes need a migration strategy. Lists are unpaginated and intended for this small demonstration.

## Hand-in

Review the code, documentation, and tests before publishing. The assignment requires **only a repository link** through the [Moodle hand-in](https://moodle.easv.dk/mod/assign/view.php?id=390209). Publishing to GitHub and submitting that link are manual steps for the repository owner.
