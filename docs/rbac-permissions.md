# RBAC — Permissions & Roles

## Roles

| Role | ID | Description |
|------|----|-------------|
| **Admin** | `814E6317-41E8-4EFE-AC7C-E2FEDA2AE335` | Administrateur plateforme — accès total |
| **OrganizationManager** | `3B7BC1B9-43F3-4D6D-9E1E-FAF6A95A3D2B` | Gestionnaire de flotte — gère une organisation, ses membres et véhicules |
| **CarWashOwner** | `A8D5D7F1-9B0D-4E8F-9A6C-0A5D0F2D6B41` | Propriétaire de stations — gère ses partenaires (car washes) |
| **User** | `B2C4E521-171B-4367-83AA-C668F803AF0D` | Utilisateur de base — accès à son profil, véhicules, abonnements |

> Tous les utilisateurs ont le rôle **User** en base. Les rôles Admin, OrganizationManager et CarWashOwner sont des rôles additionnels.

---

## Matrice des permissions

Légende : `x` = accès autorisé, vide = refusé

### Auth

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Auth.Logout` | `POST /v1/auth/logout` | Logout | x | x | x | x |
| *(aucune)* | `POST /v1/auth/login` | Login | — | — | — | — |
| *(aucune)* | `POST /v1/auth/register` | Register | — | — | — | — |
| *(aucune)* | `POST /v1/auth/forgot-password` | Forgot password | — | — | — | — |
| *(aucune)* | `POST /v1/auth/set-password` | Set password | — | — | — | — |
| *(aucune)* | `POST /v1/auth/reset-password` | Reset password | — | — | — | — |

> Les routes sans permission sont publiques (pas d'authentification requise).

### Me (Profil)

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Me.Get` | `GET /v1/me` | Mon profil | x | x | x | x |
| `Me.Update` | `PUT /v1/me` | Modifier mon profil | x | x | x | x |
| `Me.Delete` | `DELETE /v1/me` | Supprimer mon compte | x | x | x | x |

### Me — Adresse

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Me.Address.Get` | `GET /v1/me/address` | Mon adresse | x | x | x | x |
| `Me.Address.Create` | `POST /v1/me/address` | Créer adresse | x | x | x | x |
| `Me.Address.Update` | `PUT /v1/me/address` | Modifier adresse | x | x | x | x |
| `Me.Address.Delete` | `DELETE /v1/me/address` | Supprimer adresse | x | x | x | x |

### Me — Véhicules

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Me.Vehicles.List` | `GET /v1/me/vehicles` | Mes véhicules | x | x | x | x |
| `Me.Vehicles.Get` | `GET /v1/me/vehicles/{id}` | Détail véhicule | x | x | x | x |
| `Me.Vehicles.Create` | `POST /v1/me/vehicles` | Ajouter véhicule | x | x | x | x |

### Me — Abonnements

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Me.Subscriptions.GetAll` | `GET /v1/me/subscriptions` | Mes abonnements | x | x | x | x |
| `Me.Subscriptions.GetPlans` | `GET /v1/me/subscriptions/plans` | Plans disponibles | x | x | x | x |
| `Me.Subscriptions.Create` | `POST /v1/me/subscriptions` | Souscrire | x | x | x | x |
| `Me.Subscriptions.Upgrade` | `PUT /v1/me/subscriptions/{id}/upgrade` | Upgrade plan | x | x | x | x |
| `Me.Subscriptions.Cancel` | `DELETE /v1/me/subscriptions/{id}` | Résilier | x | x | x | x |
| `Me.Subscriptions.AttachVehicle` | `POST /v1/me/subscriptions/{id}/vehicles` | Lier véhicule | x | x | x | x |
| `Me.Subscriptions.DetachVehicle` | `DELETE /v1/me/subscriptions/{id}/vehicles/{vid}` | Délier véhicule | x | x | x | x |

### Me — Wash Sessions & Credits

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Me.WashCredits.Get` | `GET /v1/me/wash-credits` | Mes crédits | x | x | x | x |
| `Me.WashSessions.List` | `GET /v1/me/wash-sessions` | Historique sessions | x | x | x | x |
| `Me.WashSessions.GetActive` | `GET /v1/me/wash-sessions/active` | Session en cours | x | x | x | x |
| `Me.WashSessions.GetById` | `GET /v1/me/wash-sessions/{id}` | Détail session | x | x | x | x |
| `Me.WashSessions.Create` | `POST /v1/me/wash-sessions` | Démarrer session | x | x | x | x |
| `Me.WashSessions.Complete` | `PUT /v1/me/wash-sessions/{id}/complete` | Terminer session | x | x | x | x |
| `Me.WashSessions.Cancel` | `PUT /v1/me/wash-sessions/{id}/cancel` | Annuler session | x | x | x | x |
| `Me.FidelityCard.Get` | `GET /v1/stations/{id}/fidelity-card` | Carte fidélité | x | x | x | x |

### Stations

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| *(aucune)* | `GET /v1/stations` | Lister stations | — | — | — | — |
| *(aucune)* | `GET /v1/stations/{id}` | Détail station | — | — | — | — |
| `Stations.GetQrCode` | `GET /v1/stations/{id}/qr-code` | QR code station | x | x | x | x |
| `Stations.Create` | `POST /v1/stations` | Créer station | | | | x |
| `Stations.CreateService` | `POST /v1/stations/{id}/services` | Ajouter service | | | | x |
| `Stations.CreateAmenity` | `POST /v1/stations/{id}/amenities` | Ajouter équipement | | | | x |

> `GET /v1/stations` et `GET /v1/stations/{id}` sont publics (pas de `.RequireAuthorization`).

### Users (Admin)

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Users.GetAll` | `GET /v1/users` | Lister utilisateurs | | | | x |
| `Users.GetById` | `GET /v1/users/{id}` | Détail utilisateur | | | | x |
| `Users.Create` | `POST /v1/users` | Créer utilisateur | | | | x |
| `Users.Update` | `PUT /v1/users/{id}` | Modifier utilisateur | | | | x |
| `Users.Delete` | `DELETE /v1/users/{id}` | Supprimer utilisateur | | | | x |

### Partners (CarWashOwner)

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Partners.GetAll` | `GET /v1/partners` | Lister partenaires | | | x | x |
| `Partners.GetById` | `GET /v1/partners/{id}` | Détail partenaire | | | x | x |
| `Partners.Create` | `POST /v1/partners` | Créer partenaire | | | x | x |
| `Partners.Update` | `PUT /v1/partners/{id}` | Modifier partenaire | | | x | x |
| `Partners.Delete` | `DELETE /v1/partners/{id}` | Supprimer partenaire | | | x | x |

### Organizations (OrganizationManager)

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Organizations.GetAll` | `GET /v1/organizations` | Lister organisations | | x | | x |
| `Organizations.GetById` | `GET /v1/organizations/{id}` | Détail organisation | | x | | x |
| `Organizations.Create` | `POST /v1/organizations` | Créer organisation | | x | | x |
| `Organizations.Update` | `PUT /v1/organizations/{id}` | Modifier organisation | | x | | x |
| `Organizations.Delete` | `DELETE /v1/organizations/{id}` | Supprimer organisation | | x | | x |

### Organizations — Membres

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Organizations.Members.GetAll` | `GET /v1/organizations/{id}/members` | Lister membres | | x | | x |
| `Organizations.Members.Invite` | `POST /v1/organizations/{id}/members` | Inviter membre | | x | | x |
| `Organizations.Members.Remove` | `DELETE /v1/organizations/{id}/members/{mid}` | Retirer membre | | x | | x |

### Organizations — Véhicules

| Permission | Route | Méthode | User | OrgManager | CarWashOwner | Admin |
|---|---|---|:---:|:---:|:---:|:---:|
| `Organizations.Vehicles.GetAll` | `GET /v1/organizations/{id}/vehicles` | Lister véhicules org | | x | | x |
| `Organizations.Vehicles.Add` | `POST /v1/organizations/{id}/vehicles` | Ajouter véhicule | | x | | x |
| `Organizations.Vehicles.Assign` | `POST /v1/organizations/{id}/vehicles/assign` | Assigner véhicule | | x | | x |

---

## Résumé par rôle

### User (20 permissions)
Accès à tout ce qui concerne **son propre compte** : profil, adresse, véhicules, abonnements, sessions de lavage, crédits, carte fidélité, consultation des stations, logout.

### OrganizationManager (31 permissions)
Tout ce que User a **+** gestion complète des **organisations** : CRUD organisations, gestion des membres (inviter, retirer), gestion des véhicules de flotte (ajouter, assigner).

### CarWashOwner (25 permissions)
Tout ce que User a **+** gestion complète des **partenaires** (car washes) : CRUD partenaires.

### Admin (toutes les permissions)
Accès à **l'intégralité** des permissions définies dans le système, y compris la gestion des utilisateurs, stations, organisations, partenaires, et toutes les fonctionnalités utilisateur.

---

## Fichiers source

- Définitions des permissions : `Domain/Authorizations/Constants/Permissions.cs`
- Attribution par rôle : `Domain/Authorizations/Constants/RolePermissions.cs`
- Seeding des rôles/permissions : `Persistence/Seeding/PermissionSeeder.cs`
- Évaluation dynamique : `Persistence/Authorizations/Services/PermissionService.cs`
