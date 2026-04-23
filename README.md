# CarWashFlow Platform

Backend API de la plateforme CarWashFlow — gestion d'abonnements de lavage auto, stations, flottes et paiements Stripe.

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
- Compte [Stripe](https://stripe.com) (cle test)

## Demarrage rapide

### 1. Lancer l'infrastructure

```bash
docker compose up -d
```

Cela demarre :
- **PostgreSQL** sur le port `5432`
- **RabbitMQ** sur le port `5672` (management UI : `http://localhost:15672`)
- **Mailpit** sur le port `1025` (UI : `http://localhost:8025`)
- **API** sur le port `8080`

### 2. Lancer en local (sans Docker pour l'API)

```bash
# Infrastructure uniquement
docker compose up -d carwashflow-db carwashflow-rabbitmq mailpit

# Lancer l'API
dotnet run --project CarWashFlow.Platform.Api
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
| `Stripe` | SecretKey, ApiVersion, WebhookSecret |
| `RabbitMQ` | Host, Username, Password, QueueName |
| `MailServer` | Host, Port, UseSsl |
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
    "Email": "admin@carwashflow.com",
    "Password": "Admin123!"
  }
}
```

## Commandes

```bash
# Build
dotnet build CarWashFlow.Platform.sln

# Tests
dotnet test CarWashFlow.Platform.sln

# Tests avec filtre
dotnet test Tests/CarWashFlow.Platform.Application.Tests --filter "CreatePartner"

# Migrations EF Core
dotnet ef migrations add NomMigration \
  -p Src/CarWashFlow.Platform.Persistence \
  -s CarWashFlow.Platform.Api
```

## Roles et permissions

4 roles avec permissions granulaires :

| Role | Description | Permissions |
|------|-------------|:-----------:|
| **User** | Utilisateur de base | 20 |
| **OrganizationManager** | Gestionnaire de flotte | 31 |
| **CarWashOwner** | Proprietaire de stations | 25 |
| **Admin** | Administrateur plateforme | Toutes |

Detail complet : [`docs/rbac-permissions.md`](docs/rbac-permissions.md)

## Abonnements et Stripe

Plans disponibles (cycle 28 jours) :

| Plan | Prix | Vehicules | Acces |
|------|------|:---------:|-------|
| Confort | 21,99 EUR | 1 | Heures creuses |
| Confort Flex | 30,99 EUR | 1 | Temps plein |
| Famille | 28,99 EUR | 2 | Heures creuses |
| Famille Flex | 37,99 EUR | 2 | Temps plein |
| Pro | 26,00 EUR HT | 1 | Temps plein |

Le cycle de vie des abonnements est gere par une [saga Rebus](docs/subscription-lifecycle-saga.md) :
1. [Creation via API](docs/subscription-api-guide.md#etape-1--creer-un-abonnement) -> Stripe Subscription (incomplete)
2. Confirmation paiement cote client (SDK Stripe)
3. Webhooks Stripe -> [saga](docs/subscription-lifecycle-saga.md) -> activation + allocation credits

| Guide | Contenu |
|-------|---------|
| [`docs/subscription-api-guide.md`](docs/subscription-api-guide.md) | Endpoints, requetes/reponses, validations, flux complet |
| [`docs/subscription-lifecycle-saga.md`](docs/subscription-lifecycle-saga.md) | Etats de la saga, webhooks Stripe, gestion des echecs |

## API endpoints

### Publics (pas d'authentification)

| Methode | Route | Description |
|---------|-------|-------------|
| POST | `/v1/auth/register` | Inscription |
| POST | `/v1/auth/login` | Connexion |
| POST | `/v1/auth/forgot-password` | Mot de passe oublie |
| POST | `/v1/auth/reset-password` | Reinitialisation mot de passe |
| GET | `/v1/stations` | Liste des stations |
| GET | `/v1/stations/{id}` | Detail station |

### Authentifies (Bearer token)

| Methode | Route | Description |
|---------|-------|-------------|
| GET | `/v1/me` | Mon profil |
| PUT | `/v1/me` | Modifier profil |
| GET | `/v1/me/vehicles` | Mes vehicules |
| POST | `/v1/me/vehicles` | Ajouter vehicule |
| GET | `/v1/me/subscriptions` | Mes abonnements |
| POST | `/v1/me/subscriptions` | Souscrire |
| POST | `/v1/me/subscriptions/{id}/vehicles` | Lier vehicule |
| PUT | `/v1/me/subscriptions/{id}/upgrade` | Upgrade plan |
| DELETE | `/v1/me/subscriptions/{id}` | Resilier |
| GET | `/v1/me/wash-credits` | Mes credits |
| POST | `/v1/me/wash-sessions` | Demarrer session |
| GET | `/v1/me/wash-sessions` | Historique sessions |

### Admin / Roles specifiques

| Methode | Route | Role requis |
|---------|-------|-------------|
| CRUD | `/v1/users/*` | Admin |
| CRUD | `/v1/partners/*` | CarWashOwner, Admin |
| CRUD | `/v1/organizations/*` | OrganizationManager, Admin |
| POST | `/v1/stations` | Admin |

> Matrice complete des permissions par route : [`docs/rbac-permissions.md`](docs/rbac-permissions.md)
> Guide detaille des endpoints abonnement : [`docs/subscription-api-guide.md`](docs/subscription-api-guide.md)

## Structure des fichiers source

```
CarWashFlow.Platform.Api/
  Program.cs                     Composition root
  Dockerfile                     Image Docker multi-stage
  appsettings.json               Configuration

Src/CarWashFlow.Platform.Domain/
  Auth/Entities/                 ApplicationUser (Identity)
  Users/Entities/                User (profil domaine)
  Subscriptions/                 Subscription, SubscriptionPlanRegistry
  WashCredits/                   WashCredit, WashSession
  Stations/                      Station, StationService, StationAmenity
  Partners/                      Partner
  Organizations/                 Organization, OrganizationMember
  Authorizations/                Permissions, RolePermissions

Src/CarWashFlow.Platform.Application/
  Auth/Commands/                 Login, Register, Logout, ForgotPassword
  Subscriptions/Commands/        Create, Upgrade, Cancel, AttachVehicle
  WashCredits/Commands/          CreateWashSession, CompleteWashSession
  [Feature]/Queries/             Queries de lecture

Src/CarWashFlow.Platform.Infrastructure/
  Stripe/Services/               StripeSubscriptionService
  Stripe/Dispatchers/            StripeEventDispatcher (webhooks)
  Stripe/Sagas/                  SubscriptionLifecycleSaga
  Stripe/EventHandlers/          Handlers webhooks Stripe
  WashCredits/EventHandlers/     Allocation et refill credits

Src/CarWashFlow.Platform.Persistence/
  Seeding/                       DatabaseSeeder, UserSeeder, PermissionSeeder
  Migrations/                    Migrations EF Core
  [Feature]/Repositories/        Implementations repositories
  [Feature]/EntityConfigurations/ Configurations EF Core

Src/CarWashFlow.Platform.Presentation/
  Auth/Endpoints/                AuthEndpoint
  Me/Endpoints/                  MeEndpoint, MeSubscriptionsEndpoint
  Stations/Endpoints/            StationsEndpoint
  Users/Endpoints/               UsersEndpoint
  Partners/Endpoints/            PartnersEndpoint
  Organizations/Endpoints/       OrganizationsEndpoint
  Stripe/Endpoints/              StripeWebhookEndpoint
```

## Documentation

| Document | Description |
|----------|-------------|
| [`docs/rbac-permissions.md`](docs/rbac-permissions.md) | Matrice complete des permissions par role |
| [`docs/subscription-api-guide.md`](docs/subscription-api-guide.md) | Guide API souscription + integration Stripe |
| [`docs/subscription-lifecycle-saga.md`](docs/subscription-lifecycle-saga.md) | Documentation de la saga Rebus |

## Docker

```bash
# Stack complete (API + PostgreSQL + RabbitMQ + Mailpit)
docker compose up -d

# Stack etendue (avec config Stripe detaillee)
docker compose -f stack.compose.yaml up -d

# Build uniquement l'API
docker compose build carwashflow-api
```

| Service | Port | URL |
|---------|------|-----|
| API | 8080 | `http://localhost:8080` |
| PostgreSQL | 5432 | — |
| RabbitMQ | 5672 / 15672 | `http://localhost:15672` |
| Mailpit | 1025 / 8025 | `http://localhost:8025` |
