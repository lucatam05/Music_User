# Music.User

UserService — handles registration, authentication and per-user aggregate data in the [Music Microservices](https://github.com/lucatam05/Music_Compose) project. It's the entry point most clients talk to first.

> Looking to run the full stack? Start from [Music_Compose](https://github.com/lucatam05/Music_Compose).

## Responsibilities

- User registration and login, issuing JWTs
- On registration, triggers the creation of a library for the new user (via LibraryService)
- Aggregates a user's song data by calling LibraryService and CatalogueService
- Keeps an aggregate song count per user in sync by consuming library-change events from Kafka

## Project layout

```
Music.User.WebApi        → HTTP API, DI composition root, resilience/health/logging wiring, Kafka consumers
Music.User.Business       → use cases, password hashing, JWT issuing
Music.User.Repository     → EF Core + Postgres
Music.User.ClientHttp     → thin orchestrator composing calls to LibraryService and CatalogueService's typed clients
```

## Communication

- **Synchronous** — `UserService → LibraryService` and `UserService → CatalogueService`, over resilient HTTP clients.
- **Asynchronous** — consumes `song-added-to-library` / `song-removed-from-library` events published by LibraryService (via its [transactional outbox](https://github.com/lucatam05/Music_Library#transactional-outbox)) to keep a per-user song counter up to date without a synchronous round trip on every library change.

## Security

- Passwords are hashed with PBKDF2 + SHA256 + a per-user salt (never stored or logged in plaintext)
- Authentication is JWT-based; the signing key and issuer/audience are shared with LibraryService via configuration

## Resilience

Calls to LibraryService and CatalogueService go through a Polly pipeline tuned for internal, Docker-network traffic (`UserResilienceExtensions`): retry with a 200ms initial backoff, circuit breaker over a 20s window that breaks for 10s, 3s per-attempt / 10s total timeout.

## Observability

- **Structured logging** via Serilog, enriched with `ServiceName` and `CorrelationId`
- **Correlation ID**: read from the incoming `X-Correlation-Id` header (or generated if absent), propagated to outbound HTTP calls to LibraryService/CatalogueService, and re-attached to the Serilog log context when consuming a Kafka event — so a chain like *"user adds a song" → LibraryService writes + queues the event → UserService updates the counter* is traceable end-to-end via a single ID across all three services' logs.
- **Health check** — `GET /health`:
  - `database`: Postgres connectivity
  - `kafka`: broker reachability (via cluster metadata, with its own short timeout so the check fails fast rather than hanging)

## API

Base route: `/User`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/User/Register?nome=&cognome=&dataNascita=&username=&email=&password=` | Anonymous | Register a new user (also creates their library) |
| POST | `/User/Login?email=&password=` | Anonymous | Returns a JWT on success |
| GET | `/User/GetCanzoniUtente` | JWT | List the current user's songs (calls LibraryService + CatalogueService) |
| GET | `/User/GetCanzoniPopolari` | JWT | Popular songs across the platform |

Full request/response schemas are on Swagger at `/swagger` once the service is running.

## Configuration

| Setting | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | Postgres connection string |
| `Services:Library` / `Services:Catalogue` | Base URLs of the other services |
| `Kafka:ConsumerClient:BootstrapServers` / `GroupId` | Kafka consumer configuration |
| `Jwt:*` | Token issuing/validation parameters (shared secret with LibraryService) |

In the full stack, all of this is wired via `Music_Compose`'s `docker-compose.yml` and `.env`.

## Local development

```bash
dotnet restore
dotnet ef database update --project Music.User.Repository --startup-project Music.User.WebApi
dotnet run --project Music.User.WebApi
```

Requires a running Postgres and Kafka instance, and LibraryService/CatalogueService reachable (see [Music_Compose](https://github.com/lucatam05/Music_Compose) for the easiest way to get a full local environment up).
