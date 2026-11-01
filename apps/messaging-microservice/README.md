# Messaging microservice

The Messaging service owns message identity, content, author and channel references, and posting timestamps. It stores this data in its own SQLite database.

The application is `src/Bizcord.Messaging.Api`, organized into presentation, application, domain, and infrastructure folders. Broker integration and public contracts are separate projects under `packages`.

Run Docker Compose from the repository root. The Dockerfile uses the monorepo root as its build context:

```powershell
docker compose up --build -d --wait
```

See the [root README](../../README.md) for API examples, tests, configuration, and limitations, and [design](../../docs/design.md) for service boundaries.
