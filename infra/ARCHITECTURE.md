# Quizzer Infra Architecture

## Goal

This repository manages infrastructure and application deployment in a way that is:

- readable
- predictable
- maintainable
- easy to extend
- safe for secrets handling

The main design principle is:

> one area = one responsibility

---

## Repository structure

text infra/ bootstrap/ charts/ platform/ applications/ common/ environments/ docs/```

---

## Responsibilities

### `bootstrap/`
Bootstrap contains namespace-level and environment-level setup.

Examples:
- namespaces
- registry pull secrets
- application runtime secrets
- database auth secrets
- kafka auth secrets

Bootstrap is applied before Helm charts.

Why:
- these resources are environment-specific
- they are not business logic
- they often contain sensitive data
- they may be managed outside Helm later

Rule:
- namespace manifests belong here
- committed secret resources must be Bitnami `SealedSecret` manifests
- plaintext Kubernetes `Secret` manifests are local-only inputs for `kubeseal` and must not be committed

---

### `charts/platform/`
Platform contains shared infrastructure services.

Examples:
- PostgreSQL
- Kafka
- MongoDB

Why:
- these components are shared dependencies
- they have different lifecycle than applications
- applications depend on them, but they should remain separated

Rule:
- if a service is shared infrastructure, it belongs here

---

### `charts/applications/`
Applications contains deployable business services.

Examples:
- api-gateway
- identity-service
- future microservices

Why:
- application rollout is independent from platform rollout
- each service should have the same deployment shape
- this makes changes predictable

Rule:
- application charts should not create platform clusters
- application charts only consume platform dependencies

---

### `charts/common/`
Common is reserved for shared Helm helpers or library templates.

Examples:
- common labels
- naming helpers
- common deployment blocks
- common ingress rendering

Why:
- prevents copy-paste across charts
- keeps conventions consistent

Rule:
- reusable templating logic belongs here
- environment-specific values do not belong here

---

### `environments/`
Environment folders hold Helm values for each environment.

Examples:
- dev
- prod

Why:
- chart defaults should stay generic
- environment-specific overrides should be explicit and easy to find

Rule:
- environment values reference secret names
- environment values do not contain raw secret values

---

## Deployment flow

### Step 1: Bootstrap
Apply the namespace.

Example:

bash kubectl apply -f bootstrap/dev/namespace.yaml``` 

### Step 2: Platform
Deploy shared dependencies, including the Sealed Secrets controller.

Example:

bash helm dependency build charts/platform helm upgrade --install platform charts/platform -n quizzer-dev -f environments/dev/platform.yaml```

### Step 3: Applications
Deploy business services.

Example:

bash helm dependency build charts/applications helm upgrade --install applications charts/applications -n quizzer-dev -f environments/dev/applications.yaml``` 

---

## Secret flow

We separate secrets into 3 categories:

### 1. App secrets
Used directly by application code.

Examples:
- JWT secret
- encryption secret
- internal API secret

These are committed as Bitnami `SealedSecret` manifests in `bootstrap/<env>/sealed-secrets/`.
The Sealed Secrets controller decrypts them into normal Kubernetes `Secret` objects in the target namespace.

Applications reference them by name in `environments/<env>/applications.yaml`.

---

### 2. Dependency credentials
Used to connect to platform services.

Examples:
- database password
- kafka password
- mongodb password

These are also committed as `SealedSecret` manifests in `bootstrap/<env>/sealed-secrets/`.

Applications do not store secret values in Helm values.
Applications only reference:
- secret name
- secret key

---

### 3. Registry credentials
Used to pull images from a container registry.

Example:
- GHCR docker config secret

These belong to `bootstrap/<env>/sealed-secrets/` as a sealed `kubernetes.io/dockerconfigjson` Secret.

They are namespace-level operational secrets, not application logic.

---

## Why we write it this way

### Reason 1: clear ownership
Each area of the repo has a single responsibility.

### Reason 2: easier changes
You can change:
- platform
- application config
- secrets
independently.

### Reason 3: safer secrets
Raw secret values are not mixed into Helm chart defaults.

### Reason 4: easier scaling
When more services are added, the structure remains understandable.

### Reason 5: easier onboarding
A new person can understand:
- where secrets go
- where infra goes
- where apps go
- where env overrides go

---

## Rules for adding a new application service

If adding a new service:

1. create a new chart under `charts/applications/charts/`
2. follow the same values contract as existing services
3. place runtime secrets in `bootstrap/<env>/`
4. reference secret names in `environments/<env>/applications.yaml`
5. never hardcode platform assumptions into the chart if values can be passed explicitly

---

## Rules for adding a new platform dependency

If adding a new shared database or broker:

1. create a new chart under `charts/platform/charts/`
2. keep it responsible only for the infrastructure resource
3. expose configurable values via `values.yaml`
4. add it to `charts/platform/Chart.yaml`
5. wire environment-specific values in `environments/<env>/platform.yaml`

---

## Practical examples

### PostgreSQL
PostgreSQL is in `charts/platform/charts/postgres/`
because it is a shared infrastructure dependency.

Applications consume PostgreSQL through:
- host
- port
- database
- user
- password secret reference

Applications do not create the PostgreSQL cluster.

---

### MongoDB
MongoDB should follow the same pattern as PostgreSQL.

That means:
- cluster chart in `charts/platform/charts/mongodb/`
- environment values in `environments/<env>/platform.yaml`
- applications reference mongodb connection settings and secret names via `environments/<env>/applications.yaml`

---

## Summary

This architecture is intentionally simple:

- `bootstrap` = prepare environment
- `platform` = deploy shared infra
- `applications` = deploy business services
- `environments` = configure per environment
- `common` = share Helm logic

If the structure stays consistent, the project remains easy to manage as it grows.

