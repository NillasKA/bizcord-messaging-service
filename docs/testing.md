# Verification

## Automated tests

Prerequisites: .NET 10 SDK. `global.json` selects Microsoft Testing Platform; use its filter syntax, not VSTest's `--filter`.

```powershell
dotnet restore Bizcord.slnx --locked-mode
dotnet test --solution Bizcord.slnx -c Release --filter-not-trait "Category=BrokerIntegration"
```

These tests cover domain validation, HTTP creation/retrieval, CRUD, channel filtering, provider event field values, invalid JSON/input, and explicit behaviour if publication fails. They use isolated temporary SQLite files and replace only the broker client. They do not pretend to prove delivery through RabbitMQ.

For the real-broker test:

```powershell
docker compose up -d --wait rabbitmq
dotnet test --solution Bizcord.slnx -c Release
```

`BrokerContractTests` uses the actual EasyNetQ adapter. It subscribes to `MessagePostedEvent` before POSTing, waits for delivery, asserts all five contract fields and UTC time, and retrieves the saved record. It uses a stable verification subscription and a random channel ID to distinguish its own message from other deliveries. The subscriber is asynchronously disposed after the test.

## Container and API check

```powershell
.\scripts\verify.ps1
```

The script starts the complete Compose deployment, checks liveness, runs every test, and performs the API demonstration. It keeps the containers/data available for inspection. Stop them with `docker compose down` when finished.

The Docker image can also be built independently:

```powershell
docker build -f apps/messaging-microservice/Dockerfile -t bizcord-messaging .
```

## Recorded validation

- Release compilation and 18 tests that do not require RabbitMQ: passed, 31 October 2026.
- Dependency vulnerability check (including transitive packages): no reported vulnerable packages, 31 October 2026.
- Real-broker and container verification: pending Docker Desktop engine startup recovery on the development machine.

The final entry will be updated only after those checks actually run.
