# Souscription d'abonnement — Guide API & Stripe

## Vue d'ensemble

Le flux de souscription suit ce chemin :

```
Client (frontend/mobile)
  → API REST (/v1/me/subscriptions)
    → MediatR Command Handler
      → Stripe API (creation de l'abonnement)
        → Stripe Webhooks
          → Rebus Saga (lifecycle management)
            → Allocation de credits de lavage
```

---

## Plans disponibles

| Type | Nom | Prix | Vehicules max | Acces | TVA requise | Upgrade vers |
|------|-----|------|:---:|---------|:---:|:---:|
| `comfort` | Confort | 21,99 EUR/28j | 1 | Heures creuses | Non | `comfort_flex` |
| `comfort_flex` | Confort Flex | 30,99 EUR/28j | 1 | Temps plein | Non | — |
| `family` | Famille | 28,99 EUR/28j | 2 | Heures creuses | Non | `family_flex` |
| `family_flex` | Famille Flex | 37,99 EUR/28j | 2 | Temps plein | Non | — |
| `pro` | Pro | 26,00 EUR HT/28j | 1 | Temps plein | Oui | — |

> Les prix sont TTC sauf le plan Pro (HT). Le cycle de facturation est de 28 jours.

### Recuperer les plans

```
GET /v1/me/subscriptions/plans
Authorization: Bearer <token>
```

Reponse :
```json
{
  "data": [
    {
      "type": "comfort",
      "displayName": "Confort",
      "basePlanType": "comfort",
      "isFlex": false,
      "maxVehicles": 1,
      "requiresBusiness": false,
      "accessType": "off_peak",
      "priceInCents": 2199,
      "isTaxInclusive": true,
      "billingIntervalDays": 28,
      "stripePriceId": "price_xxx",
      "flexUpgradeType": "comfort_flex",
      "baseDowngradeType": null
    }
  ]
}
```

---

## Etape 1 — Creer un abonnement

```
POST /v1/me/subscriptions
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "type": "comfort",
  "paymentMethodId": "pm_xxx",
  "quantity": 1,
  "metadata": {
    "source": "web"
  }
}
```

### Champs

| Champ | Type | Requis | Description |
|-------|------|:---:|-------------|
| `type` | string | oui | Type de plan (`comfort`, `comfort_flex`, `family`, `family_flex`, `pro`) |
| `paymentMethodId` | string | non | ID du moyen de paiement Stripe (si le client n'a pas de methode par defaut) |
| `quantity` | int | non | Quantite (defaut : 1) |
| `metadata` | object | non | Metadata supplementaires passees a Stripe |

### Validations

| Condition | Erreur |
|-----------|--------|
| Type vide ou inconnu | `400 Bad Request` — "Type de plan invalide" |
| Plan `pro` sans `vatNumber` ou `legalName` sur le profil | `422 Validation Failed` — "Un numero de TVA et un nom d'entreprise sont requis" |
| Utilisateur non authentifie | `401 Unauthorized` |
| Aucun moyen de paiement | `422 Validation Failed` — "Aucun moyen de paiement par defaut" |

### Reponse (201 Created)

```json
{
  "data": {
    "subscription": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "stripeId": "sub_xxx",
      "type": "comfort",
      "status": "Active",
      "stripePrice": "price_xxx",
      "quantity": 1,
      "trialEndsAt": null,
      "endsAt": null,
      "createdAt": "2026-02-25T10:00:00Z",
      "updatedAt": "2026-02-25T10:00:00Z",
      "items": [
        {
          "stripeId": "si_xxx",
          "stripePrice": "price_xxx",
          "stripeProduct": "prod_xxx",
          "quantity": 1
        }
      ],
      "vehicles": [],
      "washCredits": []
    },
    "clientSecret": "pi_xxx_secret_xxx",
    "ephemeralKey": "ek_xxx",
    "stripeCustomerId": "cus_xxx"
  }
}
```

### Ce qui se passe cote backend

1. Le handler verifie l'utilisateur et valide le type de plan
2. `IStripePriceMapping` traduit le type de plan en `stripePriceId`
3. `StripeSubscriptionService.CreateUserSubscriptionAsync()` :
   - Cree un `Customer` Stripe si l'utilisateur n'en a pas
   - Attache le `paymentMethodId` s'il est fourni et le met par defaut
   - Cree une `Subscription` Stripe avec `payment_behavior: "default_incomplete"`
   - Genere une `EphemeralKey` pour le SDK mobile Stripe
   - Extrait le `clientSecret` du `PaymentIntent` de la premiere facture
4. Cree/met a jour l'entite `Subscription` locale en base
5. Retourne le `clientSecret` et `ephemeralKey` au client

### Ce qui se passe cote client

Le `clientSecret` et `ephemeralKey` sont utilises pour confirmer le paiement via le SDK Stripe :

**Web (Stripe.js)** :
```javascript
const { error } = await stripe.confirmCardPayment(clientSecret, {
  payment_method: paymentMethodId
});
```

**Mobile (Expo/React Native)** :
```javascript
const { error } = await confirmPayment(clientSecret, {
  paymentMethodType: 'Card'
});
```

### Ce qui se passe apres le paiement (webhooks)

Une fois le paiement confirme, Stripe envoie des webhooks qui declenchent la saga :

```
Stripe → POST /webhook/stripe
  → StripeEventDispatcher
    → Rebus bus.SendLocal()
      → SubscriptionLifecycleSaga
```

**Evenements Stripe traites par la saga :**

| Evenement Stripe | Action saga |
|-----------------|-------------|
| `customer.subscription.created` | Cree/synchronise l'abonnement local, etat → `Active` ou `PendingPayment` |
| `invoice.payment_succeeded` | Reset compteur echecs, etat → `Active`, alloue credits de lavage |
| `customer.subscription.updated` | Synchronise prix, statut, detecte changement de plan |
| `invoice.payment_failed` | Incremente echecs (max 3 avant expiration) |
| `customer.subscription.deleted` | Finalise l'annulation, etat → `Expired` |

---

## Etape 2 — Lier un vehicule

Apres souscription, l'utilisateur doit attacher un vehicule pour utiliser son abonnement.

```
POST /v1/me/subscriptions/{subscriptionId}/vehicles
Authorization: Bearer <token>
```

```json
{
  "vehicleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Validations

| Condition | Erreur |
|-----------|--------|
| Vehicule deja attache a un autre abonnement | `400 Bad Request` |
| Nombre max de vehicules atteint pour ce plan | `400 Bad Request` |
| Vehicule ou abonnement introuvable | `404 Not Found` |
| L'abonnement ne vous appartient pas | `403 Forbidden` |

### Detacher un vehicule

```
DELETE /v1/me/subscriptions/{subscriptionId}/vehicles/{vehicleId}
Authorization: Bearer <token>
```

---

## Etape 3 — Upgrade de plan

Un abonnement peut etre upgrade vers sa version Flex (temps plein) :
- `comfort` → `comfort_flex`
- `family` → `family_flex`

```
POST /v1/me/subscriptions/{subscriptionId}/upgrade
Authorization: Bearer <token>
```

```json
{
  "targetPlanType": "comfort_flex"
}
```

### Validations

| Condition | Erreur |
|-----------|--------|
| Abonnement pas en statut `Active` | `400 Bad Request` |
| Changement de plan invalide | `400 Bad Request` — "Changement de plan non autorise" |

### Ce qui se passe

1. Le handler verifie que le changement est autorise via `SubscriptionPlanRegistry.IsValidPlanChange()`
2. `StripeSubscriptionService.UpdateSubscriptionPlanAsync()` met a jour le prix Stripe avec prorata
3. L'entite locale est mise a jour (type, isFlex, basePlanType, items)
4. Stripe envoie `customer.subscription.updated` → la saga detecte le changement de plan et synchronise

---

## Etape 4 — Annuler un abonnement

```
POST /v1/me/subscriptions/{subscriptionId}/cancel
Authorization: Bearer <token>
```

```json
{
  "immediate": true
}
```

### Validations

| Condition | Erreur |
|-----------|--------|
| Abonnement deja annule | `400 Bad Request` |
| Statut non annulable (seuls `Active`, `PastDue`, `Trialing`) | `400 Bad Request` |

### Ce qui se passe

1. Le handler marque l'abonnement local comme `Canceled` avec `endsAt = now`
2. Envoie un `CancelSubscriptionSagaCommand` via Rebus
3. La saga appelle `StripeSubscriptionService.CancelSubscriptionAsync()` pour annuler sur Stripe
4. Stripe envoie `customer.subscription.deleted` → la saga finalise (etat `Expired`, saga terminee)

---

## Consulter ses abonnements

```
GET /v1/me/subscriptions
Authorization: Bearer <token>
```

Reponse :
```json
{
  "data": [
    {
      "id": "...",
      "stripeId": "sub_xxx",
      "type": "comfort",
      "status": "Active",
      "stripePrice": "price_xxx",
      "quantity": 1,
      "trialEndsAt": null,
      "endsAt": null,
      "items": [...],
      "vehicles": [
        {
          "id": "...",
          "licensePlate": "1-ABC-123",
          "brand": "Volkswagen",
          "model": "Golf"
        }
      ],
      "washCredits": [
        {
          "id": "...",
          "remaining": 4,
          "total": 4,
          "expiresAt": "2026-03-25T00:00:00Z"
        }
      ]
    }
  ]
}
```

> Les abonnements `Canceled` et `IncompleteExpired` sont exclus de la liste.

---

## Cycle de vie complet (diagramme)

```
┌─────────────┐
│ POST create  │  Client appelle l'API
└──────┬──────┘
       │
       ▼
┌─────────────────────────┐
│ Stripe: create Customer │  (si nouveau)
│ Stripe: create Sub      │  payment_behavior: default_incomplete
│ → clientSecret          │
│ → ephemeralKey          │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ Client: confirmPayment  │  SDK Stripe (web ou mobile)
│ avec clientSecret       │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────────────────┐
│ Stripe Webhooks                     │
│                                     │
│ customer.subscription.created       │──→ Saga: cree/synchro local
│ invoice.payment_succeeded           │──→ Saga: Active + credits alloues
│ customer.subscription.updated       │──→ Saga: synchro statut/plan
│                                     │
└─────────────────────────────────────┘
       │
       ▼
┌─────────────────────────┐
│ Abonnement actif        │
│ Credits de lavage OK    │
│ Vehicule(s) attachable  │
└─────────────────────────┘
```

---

## Gestion des echecs de paiement

La saga gere automatiquement les echecs :

| Tentative | Action |
|:---------:|--------|
| 1 | Etat → `PastDue`, notification |
| 2 | Etat reste `PastDue` |
| 3+ | Abonnement expire, saga terminee |

---

## Statuts possibles

| Statut | Description |
|--------|-------------|
| `Active` | Abonnement actif, paiement OK |
| `Trialing` | Periode d'essai (traite comme actif) |
| `PastDue` | Paiement echoue, en attente de regularisation |
| `Canceled` | Annule par l'utilisateur |
| `Incomplete` | Paiement initial en attente de confirmation |
| `IncompleteExpired` | Paiement initial jamais confirme (expire) |
| `Unpaid` | Factures impayees |
| `Paused` | Mis en pause |

---

## Configuration Stripe

`appsettings.json` :
```json
{
  "Stripe": {
    "SecretKey": "sk_test_xxx",
    "ApiVersion": "2025-08-15",
    "WebhookSecret": "whsec_xxx"
  }
}
```

Le webhook doit etre configure dans le dashboard Stripe pour pointer vers :
```
https://<domain>/webhook/stripe
```

Evenements a ecouter :
- `customer.subscription.created`
- `customer.subscription.updated`
- `customer.subscription.deleted`
- `invoice.payment_succeeded`
- `invoice.payment_failed`
- `invoice.payment_action_required`
- `invoice.finalized`
- `invoice.paid`
- `checkout.session.completed`
- `checkout.session.expired`
- `customer.updated`
- `customer.deleted`
- `payment_intent.succeeded`
- `payment_intent.canceled`
- `payment_intent.payment_failed`
- `payment_method.attached`
- `payment_method.detached`
- `payment_method.automatically_updated`

---

## Fichiers source

| Couche | Fichier | Role |
|--------|---------|------|
| Presentation | `Me/Endpoints/MeSubscriptionsEndpoint.cs` | Routes HTTP |
| Presentation | `Stripe/Endpoints/StripeWebhookEndpoint.cs` | Reception webhooks |
| Application | `Subscriptions/Commands/CreateAuthenticatedUserSubscription/` | Creation abonnement |
| Application | `Subscriptions/Commands/UpgradeSubscription/` | Upgrade de plan |
| Application | `Subscriptions/Commands/CancelSubscription/` | Annulation |
| Application | `Subscriptions/Commands/AttachVehicleToSubscription/` | Liaison vehicule |
| Application | `Subscriptions/Commands/DetachVehicleFromSubscription/` | Detachement vehicule |
| Application | `Subscriptions/Queries/GetSubscriptionPlans/` | Liste des plans |
| Application | `Subscriptions/Queries/GetAuthenticatedUserSubscriptions/` | Mes abonnements |
| Domain | `Subscriptions/Configuration/SubscriptionPlanRegistry.cs` | Registre des plans |
| Domain | `Subscriptions/Configuration/IStripePriceMapping.cs` | Mapping type ↔ Stripe price |
| Domain | `Stripe/Services/IStripeSubscriptionService.cs` | Interface Stripe |
| Infrastructure | `Stripe/Services/StripeSubscriptionService.cs` | Implementation Stripe API |
| Infrastructure | `Stripe/Dispatchers/StripeEventDispatcher.cs` | Dispatch webhooks → Rebus |
| Infrastructure | `Stripe/Sagas/SubscriptionLifecycleSaga.cs` | Saga lifecycle |
| Infrastructure | `Stripe/Events/StripeWebhookMessages.cs` | Messages Rebus |
