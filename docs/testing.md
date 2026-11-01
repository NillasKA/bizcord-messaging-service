# Verification

## Automated tests

Prerequisite: .NET 10 SDK. `global.json` configures Microsoft Testing Platform. The following command excludes tests that require RabbitMQ:

```powershell
dotnet restore Bizcord.slnx --locked-mode
dotnet test --solution Bizcord.slnx -c Release --filter-not-trait "Category=BrokerIntegration"
```

These tests cover domain validation, HTTP creation and retrieval, CRUD, channel filtering, event field values, invalid input, and publication failures. Each API test uses an isolated temporary SQLite file and a fake message publisher.

Run the full suite with RabbitMQ:

```powershell
docker compose up -d --wait rabbitmq
dotnet test --solution Bizcord.slnx -c Release
```

`BrokerContractTests` uses the EasyNetQ adapter and a running RabbitMQ instance. It subscribes to `MessagePostedEvent` before posting, waits for delivery, checks all five contract fields and UTC time, and retrieves the saved record. A random channel ID identifies the test's message. The subscription is disposed after the test.

## Container and API check

```powershell
.\scripts\verify.ps1
```

The script builds and starts the Compose deployment, checks liveness, runs all tests, and performs the CRUD demonstration. Containers remain running afterwards. `docker compose down` stops them while preserving the named data volumes.

The Docker image can also be built independently:

```powershell
docker build -f apps/messaging-microservice/Dockerfile -t bizcord-messaging .
```

## Verification results

Full verification completed on 1 November 2026:

| Check | Result |
| --- | --- |
| Release build and automated tests | 19 passed; no failures or skipped tests |
| RabbitMQ integration | Published event received with all contract fields intact |
| Docker build | Linux image built with locked dependencies |
| Compose startup | API started after RabbitMQ's health check passed |
| CRUD demonstration | Create, read, filter, edit, delete, and `404` after deletion passed |
| Persistence | Stored message remained available after an API container restart |
| Container user | API ran as UID 1654 |
