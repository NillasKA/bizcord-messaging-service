# Messaging microservice

Owns message identity, content, author/channel references, and posting timestamps. Its SQLite database belongs only to this service.

The deployable application is `src/Bizcord.Messaging.Api`. Its layers are folders in one small application project. Broker integration and public contracts are separate projects under `packages`.

Run Docker Compose from the **repository root**, because the Dockerfile builds with the full monorepo as its context:

```powershell
docker compose up --build -d --wait
```

See the [root README](../../README.md) for API examples, tests, configuration, and limitations, and [design](../../docs/design.md) for service boundaries.
