# Subscription Lifecycle Saga

## Vue d'ensemble

La `SubscriptionLifecycleSaga` orchestre le cycle de vie complet d'un abonnement CarWashFlow via Rebus. Elle est correlee par `StripeSubscriptionId` et reagit aux evenements Stripe webhook ainsi qu'aux commandes internes (upgrade, cancel).

---

## Machine d'etats

```mermaid
stateDiagram-v2
    [*] --> Created : subscription.created

    Created --> PendingPayment : status = incomplete
    Created --> Active : status = active / trialing

    PendingPayment --> Active : invoice.payment_succeeded
    PendingPayment --> PendingPayment : invoice.payment_failed\n(tentative < 3)
    PendingPayment --> Expired : subscription.updated\n(incomplete_expired)
    PendingPayment --> Expired : ExpireIncompleteSubscriptionSagaCommand\n(timeout 30min)

    Active --> Active : invoice.payment_succeeded\n(renouvellement 28j)
    Active --> PastDue : invoice.payment_failed
    Active --> Upgrading : UpgradeSubscriptionSagaCommand
    Active --> Canceled : CancelSubscriptionSagaCommand

    PastDue --> Active : invoice.payment_succeeded
    PastDue --> PastDue : invoice.payment_failed\n(tentative < 3)
    PastDue --> Expired : invoice.payment_failed\n(tentative >= 3)
    PastDue --> Expired : subscription.deleted
    PastDue --> Canceled : CancelSubscriptionSagaCommand

    Upgrading --> Active : subscription.updated\n(plan change confirme)

    Canceled --> [*] : MarkAsComplete()
    Expired --> [*] : MarkAsComplete()
```

---

## Messages et correlation

Tous les messages portent un champ `EventJson`. La saga extrait le `StripeSubscriptionId` pour la correlation :

```mermaid
flowchart LR
    subgraph Webhook Stripe
        E1[subscription.created]
        E2[subscription.updated]
        E3[subscription.deleted]
        E4[invoice.payment_succeeded]
        E5[invoice.payment_failed]
    end

    subgraph Commandes internes
        C1[UpgradeSubscriptionSagaCommand]
        C2[CancelSubscriptionSagaCommand]
        C3[ExpireIncompleteSubscriptionSagaCommand]
    end

    subgraph Correlation
        direction TB
        K["StripeSubscriptionId"]
    end

    E1 -->|Subscription.Id| K
    E2 -->|Subscription.Id| K
    E3 -->|Subscription.Id| K
    E4 -->|"Invoice.SubscriptionId ?? Invoice.Subscription?.Id"| K
    E5 -->|"Invoice.SubscriptionId ?? Invoice.Subscription?.Id"| K
    C1 -->|propriete directe| K
    C2 -->|propriete directe| K
    C3 -->|propriete directe| K

    K --> SAGA["SubscriptionLifecycleSaga"]
```

---

## Detail des transitions

### `subscription.created` (IAmInitiatedBy)

```mermaid
flowchart TD
    A[Event recu] --> B{User trouve par\nStripeCustomerId ?}
    B -->|Non| Z[Log warning, return]
    B -->|Oui| C{Subscription locale\nexiste deja ?}
    C -->|Oui| D[Sync saga state\ndepuis Stripe status]
    C -->|Non| E[Creer Subscription locale]
    E --> F[Setter IsFlex / BasePlanType\ndepuis PlanRegistry]
    F --> G[Creer SubscriptionItems]
    G --> H{Stripe status = active ?}
    H -->|Oui| I["State = Active"]
    H -->|Non| J["State = PendingPayment"]
```

### `subscription.updated`

```mermaid
flowchart TD
    A[Event recu] --> B{Subscription locale\ntrouvee ?}
    B -->|Non| Z[Log warning, return]
    B -->|Oui| C{status =\nincomplete_expired ?}
    C -->|Oui| D[Supprimer subscription]
    D --> E["State = Expired\nMarkAsComplete()"]
    C -->|Non| F[Sync prix, quantite,\nstatus, items]
    F --> G[Detecter changement\nde plan via PriceMapping]
    G --> H{Plan change ?}
    H -->|Oui| I[Mettre a jour Type,\nIsFlex, BasePlanType]
    H -->|Non| J[Conserver plan actuel]
    I --> K[Resoudre nouvel etat]
    J --> K
    K --> L{cancel_at_period_end ?}
    L -->|Oui| M["State = Canceled"]
    L -->|Non| N{status}
    N -->|active / trialing| O["State = Active"]
    N -->|past_due| P["State = PastDue"]
    N -->|canceled| Q["State = Expired"]
```

### `invoice.payment_succeeded`

```mermaid
flowchart TD
    A[Event recu] --> B[Reset FailedPaymentAttempts = 0]
    B --> C["State = Active"]
    C --> D[Log succes]
```

### `invoice.payment_failed`

```mermaid
flowchart TD
    A[Event recu] --> B[FailedPaymentAttempts++]
    B --> C{Tentatives >= 3 ?}
    C -->|Non| D["State = PastDue"]
    C -->|Oui| E[Mettre subscription\nen past_due + endsAt]
    E --> F["State = Expired\nMarkAsComplete()"]
```

### `UpgradeSubscriptionSagaCommand`

```mermaid
flowchart TD
    A[Commande recue] --> B[Sauvegarder PreviousPlanType]
    B --> C[PlanType = NewPlanType]
    C --> D["State = Upgrading"]
    D --> E[Attendre subscription.updated\npour confirmer]
```

### `CancelSubscriptionSagaCommand`

L'annulation est toujours immediate. Le `CancelSubscriptionCommandHandler` desactive d'abord la subscription en base (status = Canceled, EndsAt = now), puis envoie la commande saga pour annuler cote Stripe.

```mermaid
flowchart TD
    A[Commande recue] --> B[Annuler sur Stripe]
    B --> C[Mettre subscription locale\nen Canceled + EndsAt = now]
    C --> D["State = Canceled\nMarkAsComplete()"]
```

### `ExpireIncompleteSubscriptionSagaCommand`

Commande differee (30 min) programmee lors de la creation d'une subscription incomplete. Si la saga est toujours en `PendingPayment` a reception, elle annule la subscription sur Stripe, supprime la subscription locale et termine la saga.

```mermaid
flowchart TD
    A[Commande recue apres 30min] --> B{State = PendingPayment ?}
    B -->|Non| C[Ignorer, le paiement a reussi]
    B -->|Oui| D[Annuler sur Stripe]
    D --> E[Supprimer subscription locale]
    E --> F["State = Expired\nMarkAsComplete()"]
```

---

## Saga Data

| Champ | Type | Description |
|-------|------|-------------|
| `StripeSubscriptionId` | `string` | Cle de correlation |
| `UserId` | `Guid` | Proprietaire de l'abonnement |
| `LocalSubscriptionId` | `Guid` | ID de la Subscription locale |
| `StripeCustomerId` | `string` | ID client Stripe |
| `PlanType` | `string` | Type de plan actuel (ex: `comfort_flex`) |
| `PreviousPlanType` | `string?` | Plan precedent lors d'un upgrade |
| `CurrentState` | `string` | Etat courant de la machine d'etats |
| `FailedPaymentAttempts` | `int` | Compteur d'echecs de paiement consecutifs |
| `LastPaymentFailedAt` | `DateTimeOffset?` | Date du dernier echec |
| `CreatedAt` | `DateTimeOffset` | Date de creation de la saga |
| `UpdatedAt` | `DateTimeOffset` | Derniere mise a jour |

---

## Coexistence avec les handlers existants

Pendant la phase de transition, les anciens handlers Rebus (`CustomerSubscriptionCreatedHandler`, `CustomerSubscriptionUpdatedHandler`, etc.) et la saga recoivent les memes evenements :

```mermaid
flowchart LR
    W[Webhook Stripe] --> R[RabbitMQ]
    R --> H1[Handler existant]
    R --> S[Saga]

    H1 -->|"skip si deja traite"| DB[(PostgreSQL)]
    S -->|"upsert + state machine"| DB
```

- La saga gere les **nouvelles** souscriptions (`IAmInitiatedBy<SubscriptionCreated>`)
- Les handlers existants verifient si la subscription existe deja avant d'agir
- **Phase 3** : suppression des 5 handlers remplaces par la saga

---

## Plans d'abonnement

| Type | Nom | Prix / 28j | Vehicules | Acces | Flex de |
|------|-----|-----------|-----------|-------|---------|
| `comfort` | Confort | 21,99 EUR TVAC | 1 | Heures creuses | - |
| `comfort_flex` | Confort Flex | 30,99 EUR TVAC | 1 | 24/7 | `comfort` |
| `family` | Famille | 28,99 EUR TVAC | 2 | Heures creuses | - |
| `family_flex` | Famille Flex | 37,99 EUR TVAC | 2 | 24/7 | `family` |
| `pro` | Pro | 26,00 EUR HTVA | 1 | 24/7 | - |

---

## Fichiers source

```
Infrastructure/Stripe/Sagas/
  SubscriptionLifecycleSaga.cs        # Saga principale
  SubscriptionLifecycleSagaData.cs    # Donnees + etats
  SagaCommands.cs                      # Commandes internes
```
