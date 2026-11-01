# Assignment coverage

The System Integration (E2026) compulsory assignment covers the work from weeks 36–40. This repository implements Bizcord's Messaging bounded context and the Week 40 provider feature, Track A.

| Assignment task | Implementation and evidence |
| --- | --- |
| 1: Analysis and design | `docs/design.md`: context table, service responsibilities, boundaries, data ownership, context map, domain model, and posting sequence |
| 2: Setup and boilerplate | `apps/messaging-microservice/src`, `tests`, `Dockerfile`, and `README.md`; root solution and shared packages |
| 2: Message client | `packages/MessageClient/src/IMessageClient.cs`, `EasyNetQMessageClient.cs`; publish/subscribe abstraction and DI extension using EasyNetQ |
| 3: Domain model | `Message` entity/aggregate and immutable `MessageContent` value object with validation |
| 3: Shared model | `packages/Shared.Contracts`: message DTO, request contracts, and `MessagePostedEvent` |
| 3: REST API | POST, GET single/list, PUT, DELETE; thin controller, DI, validation, and status codes |
| 4: Dockerfile | Multi-stage SDK/runtime build preserving monorepo references; non-root application |
| 4: Compose | RabbitMQ and API; health check, healthy dependency condition, host environment variable, ports, and persistent volumes |
| 5: First feature | `POST /api/messages` saves to SQLite and publishes `MessagePostedEvent` |
| 5: Domain tests | Identity/references/UTC time; blank/oversized content; empty references; content edits and value equality |
| 5: Provider contract test | Captures the published event and checks every required value against the HTTP request and persisted response |
| 5: Integration test | HTTP POST then GET with SQLite, plus a real RabbitMQ test that subscribes before posting and checks the delivered event |
| Submission | Repository link submitted through Moodle; source, tests, deployment configuration, and analysis/design included in the repository |

## Source pages

- [Project description](https://moodle.easv.dk/mod/page/view.php?id=378850)
- [Week 36 workshop](https://moodle.easv.dk/mod/workshop/view.php?id=378852)
- [Week 37 lab 01: setup](https://moodle.easv.dk/mod/page/view.php?id=380462)
- [Week 37 lab 02: boilerplate](https://moodle.easv.dk/mod/page/view.php?id=380463)
- [Week 37 lab 03: message client](https://moodle.easv.dk/mod/page/view.php?id=380464)
- [Week 38 domain model](https://moodle.easv.dk/mod/page/view.php?id=385269)
- [Week 38 shared model](https://moodle.easv.dk/mod/page/view.php?id=385271)
- [Week 38 REST API](https://moodle.easv.dk/mod/page/view.php?id=385270)
- [Week 39 Dockerfile](https://moodle.easv.dk/mod/page/view.php?id=387695)
- [Week 39 Compose](https://moodle.easv.dk/mod/page/view.php?id=387696)
- [Week 40 first feature and testing](https://moodle.easv.dk/mod/page/view.php?id=389093)
- [Compulsory assignment hand-in](https://moodle.easv.dk/mod/assign/view.php?id=390209)

## Repository structure

The service's `src`, `tests`, `Dockerfile`, and `README.md` are placed under `apps/messaging-microservice`, following the monorepo layout in lab 02. Shared contracts and the message client are under `packages`.

The implementation uses .NET 10, EasyNetQ 8, and SQLite. Build settings are shared through `Directory.Build.props`, and dependency versions are recorded in NuGet lock files.
