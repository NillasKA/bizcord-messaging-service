# Assignment coverage

This implementation follows the actual Moodle task pages read on 31 October 2026. The project is **Bizcord**, not the Orders example used in the earlier generated study notes. The selected domain is Messaging, and the Week 40 feature is **Track A**.

| Assignment task | Implementation and evidence |
| --- | --- |
| 1: Analysis and design | `docs/design.md`: context table, service responsibilities, boundaries, data ownership, context map, domain model, and posting sequence |
| 2: Setup and boilerplate | `apps/messaging-microservice/src`, `tests`, `Dockerfile`, and `README.md`; root solution; local Git setup branch `chore/initialize-project` |
| 2: Message client | `packages/MessageClient/src/IMessageClient.cs`, `EasyNetQMessageClient.cs`; publish/subscribe abstraction and DI extension using EasyNetQ |
| 3: Domain model | `Message` entity/aggregate and immutable `MessageContent` value object with validation |
| 3: Shared model | `packages/Shared.Contracts`: message DTO, request contracts, and the prescribed event |
| 3: REST API | POST, GET single/list, PUT, DELETE; thin controller, DI, validation, and status codes |
| 4: Dockerfile | Multi-stage SDK/runtime build preserving monorepo references; non-root application |
| 4: Compose | RabbitMQ and API; health check, healthy dependency condition, host environment variable, ports, and persistent volumes |
| 5: First feature | `POST /api/messages` saves to SQLite and publishes the exact `MessagePostedEvent` |
| 5: Domain tests | Identity/references/UTC time; blank/oversized content; empty references; content edits and value equality |
| 5: Provider contract test | Captures the published event and checks every required value against the HTTP request and persisted response |
| 5: Integration test | HTTP POST then GET with SQLite, plus a real RabbitMQ test that subscribes before posting and checks the delivered event |
| Submission | Repository containing source, tests, deploy configuration, and this `docs` directory; owner submits only its repository link |

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

Lab 01 describes the service's `src/tests/Dockerfile/README` layout; lab 02 places that layout under `apps/[domain]-microservice`. This repository follows the latter monorepo layout. No instructor starter repository is prescribed by those setup instructions.

The workshop currently displays its evaluation phase rather than its original workshop instructions. The analysis covers the explicit documentation requirements in the compulsory assignment and the project description; no extra workshop-specific rules are invented here.

The labs show older .NET 7 examples. This implementation uses the installed .NET 10 SDK and EasyNetQ 8, retaining the required behaviour and interfaces. The SQLite native bundle is pinned explicitly because the older transitive native package was reported vulnerable during restore.
