# TaskFlow Platform

Backend API de la plateforme TaskFlow — gestion d'abonnements de lavage auto, stations, flottes et paiements Stripe.

## Stack technique

| Composant | Technologie |
|-----------|-------------|
| Runtime | .NET 10.0 |
| API | ASP.NET Core Minimal APIs + Carter |
| Base de donnees | PostgreSQL 16 (EF Core + Npgsql) |
| Authentification | JWT Bearer + ASP.NET Core Identity |
| Autorisation | [RBAC dynamique par permissions](docs/rbac-permissions.md) |
| Paiements | [Stripe](docs/subscription-api-guide.md) (subscriptions, webhooks) |
| Messaging | RabbitMQ via Rebus ([sagas](docs/subscription-lifecycle-saga.md), outbox) |
| CQRS | MediatR |
| Tests | xUnit + Moq (370+ tests) |
| Documentation API | Scalar (OpenAPI) |
| Email | SMTP (Mailpit en dev) |

## Architecture

Clean Architecture en 6 projets source :

```
Src/
  Domain          Entites, interfaces repositories, services domaine (zero dependance)
  Application     Commandes/Queries MediatR, handlers, DTOs, exceptions
  Infrastructure  Stripe, Rebus/RabbitMQ, JWT, email (voir docs/subscription-lifecycle-saga.md)
  Persistence     EF Core DbContext, repositories, migrations, seeders
  Presentation    Endpoints HTTP Carter, filtres, requetes
Api/              Composition root ASP.NET Core, Program.cs, DI, Dockerfile
```

**Regle de dependance** : Domain <- Application <- Infrastructure/Persistence <- Presentation <- Api

```
Tests/
  Domain.Tests          Tests entites et logique domaine
  Application.Tests     Tests handlers MediatR (commandes/queries)
  Infrastructure.Tests  Tests Stripe, webhooks, sagas, event handlers
  Persistence.Tests     Tests repositories (SQLite in-memory)
  Architecture.Tests    Tests de conventions et dependances
```

## Prerequis

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) et Docker Compose

## Demarrage rapide

### 1. Lancer l'infrastructure

```bash
docker compose up -d
```

Cela demarre :
- **PostgreSQL** sur le port `5432`
- **API** sur le port `8080`

### 2. Lancer en local (sans Docker pour l'API)

```bash
# Infrastructure uniquement
docker compose up -d taskflow-db

# Lancer l'API
dotnet run --project TaskFlow.Platform.Api
```

L'API demarre sur `http://localhost:5000` (ou le port configure).
La base de donnees est migree et seedee automatiquement au demarrage.

### 3. Documentation API

Une fois l'API lancee, la documentation Scalar est disponible sur :
```
http://localhost:8080/scalar/v1
```

## Configuration

La configuration se fait via `appsettings.json` ou variables d'environnement.

### Sections principales

| Section | Description |
|---------|-------------|
| `ConnectionStrings:DefaultConnection` | Connexion PostgreSQL |
| `Jwt` | Issuer, Audience, Secret, ExpiresInDays |
| `SeedDatabase` | `true` pour seeder au demarrage |
| `AdminSeed` | Compte admin configurable (Enabled, Email, Password) |

### Seeding

Quand `SeedDatabase: true`, le demarrage cree automatiquement :

- [4 roles : Admin, OrganizationManager, CarWashOwner, User](docs/rbac-permissions.md)
- [48 permissions reparties par role](docs/rbac-permissions.md)
- Utilisateurs de test (admin, pro, partner, individual)
- Organisations, partenaires, stations, vehicules de demonstration

Le compte admin est configurable via la section `AdminSeed` :
```json
{
  "AdminSeed": {
    "Enabled": true,
    "Email": "admin@taskflow.com",
    "Password": "Admin123!"
  }
}
```

## Commandes

```bash
# Build
dotnet build TaskFlow.Platform.sln

# Tests
dotnet test TaskFlow.Platform.sln

# Tests avec filtre
dotnet test Tests/TaskFlow.Platform.Application.Tests --filter "CreatePartner"

# Migrations EF Core
dotnet ef migrations add NomMigration \
  -p Src/TaskFlow.Platform.Persistence \
  -s TaskFlow.Platform.Api
```

## API endpoints

### Publics (pas d'authentification)

| Methode | Route | Description |
|---------|-------|-------------|
| POST | `/v1/auth/register` | Inscription |
| POST | `/v1/auth/login` | Connexion |
| POST | `/v1/auth/forgot-password` | Mot de passe oublie |
| POST | `/v1/auth/reset-password` | Reinitialisation mot de passe |

### Authentifies (Bearer token)

| Methode | Route | Description |
|---------|-------|-------------|
| GET | `/v1/me` | Mon profil |
| PUT | `/v1/me` | Modifier profil |

### Admin / Roles specifiques

| Methode | Route | Role requis |
|---------|-------|-------------|
| CRUD | `/v1/users/*` | Admin |

> Matrice complete des permissions par route : [`docs/rbac-permissions.md`](docs/rbac-permissions.md)
> Guide detaille des endpoints abonnement : [`docs/subscription-api-guide.md`](docs/subscription-api-guide.md)

## Structure des fichiers source

```
TaskFlow.Platform.Api/
  Program.cs                     Composition root
  Dockerfile                     Image Docker multi-stage
  appsettings.json               Configuration

Src/TaskFlow.Platform.Domain/
  Auth/Entities/                 ApplicationUser (Identity)
  Users/Entities/                User (profil domaine)
  Authorizations/                Permissions, RolePermissions

Src/TaskFlow.Platform.Application/
  Auth/Commands/                 Login, Register, Logout, ForgotPassword
  [Feature]/Queries/             Queries de lecture

Src/TaskFlow.Platform.Infrastructure/
  Authentication/Services/               AuthenticationService

Src/TaskFlow.Platform.Persistence/
  Seeding/                       DatabaseSeeder, UserSeeder, PermissionSeeder
  Migrations/                    Migrations EF Core
  [Feature]/Repositories/        Implementations repositories
  [Feature]/EntityConfigurations/ Configurations EF Core

Src/TaskFlow.Platform.Presentation/
  Auth/Endpoints/                AuthEndpoint
  Me/Endpoints/                  MeEndpoint, MeSubscriptionsEndpoint
  Users/Endpoints/               UsersEndpoint
```

## Docker

```bash
# Stack complete (API + PostgreSQL + RabbitMQ + Mailpit)
docker compose up -d

# Stack etendue (avec config Stripe detaillee)
docker compose -f stack.compose.yaml up -d

# Build uniquement l'API
docker compose build taskflow-api
```

| Service | Port | URL |
|---------|------|-----|
| API | 8080 | `http://localhost:8080` |
| PostgreSQL | 5432 | — |
