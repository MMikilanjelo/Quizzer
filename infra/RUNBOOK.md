# Quizzer Infra Runbook

This runbook explains how to deploy, update, verify, and clean the infrastructure and applications for the Quizzer project.

---

# 1. Overview

The repository is organized into three deployment layers:

## Bootstrap

Environment preparation:

- namespace
- registry credentials
- application secrets
- database credentials
- kafka credentials
- mongodb credentials

## Platform

Shared infrastructure:

- PostgreSQL
- Kafka
- MongoDB

## Applications

Business services:

- API Gateway
- Identity Service

---

# 2. Prerequisites

Before deploying, the Kubernetes cluster must already contain the required operators and controllers.

## Required components

### PostgreSQL

CloudNativePG operator must be installed.

### Kafka

Strimzi operator must be installed.

### MongoDB

Percona Server for MongoDB operator must be installed.

### Ingress

An ingress controller such as Traefik must be installed.

### Sealed Secrets

The Bitnami Sealed Secrets controller is installed by the platform umbrella chart.

### Tools

The following CLI tools must be available:

- `kubectl`
- `helm`
- `kubeseal`

---

# 3. Directory responsibilities

## `bootstrap/<env>/`

Contains namespace-level and environment-specific resources such as:

- namespace manifest
- registry pull secret
- runtime application secrets
- database auth secrets
- kafka auth secrets
- mongodb user secrets

## `charts/platform/`

Deploys shared infrastructure dependencies.

## `charts/applications/`

Deploys application services.

## `environments/<env>/`

Contains Helm values for a specific environment.

---

# 4. Deployment order

Deploy in this exact order:

1. Bootstrap namespace
2. Platform controller bootstrap
3. Sealed secrets
4. Full platform
5. Applications

This order is required because:

- bootstrap creates namespace
- the platform controller bootstrap installs the Sealed Secrets CRD and controller
- sealed secrets decrypt into normal Kubernetes Secrets
- platform creates shared infra
- applications consume secrets and infra endpoints

---

# 5. Deploy DEV environment

All commands below assume the current working directory is the repository root.

## Step 1: Apply namespace

```
bash
kubectl apply -f bootstrap/dev/namespace.yaml
```

This creates namespace `quizzer-dev`.

Plaintext Secret manifests must not be committed or applied from `bootstrap/dev/`.

---

## Step 2: Install Sealed Secrets controller

```
bash
helm dependency build charts/platform
helm upgrade --install platform charts/platform \
-n quizzer-dev \
-f environments/dev/platform.yaml \
--set postgres.enabled=false \
--set kafka.enabled=false \
--set mongodb.enabled=false
```

This installs the Sealed Secrets controller without starting platform services that require decrypted secrets.

---

## Step 3: Generate and apply sealed secrets

Create local plaintext Secret files under `bootstrap/dev/plain-secrets/`.
Use `bootstrap/dev/plain-secrets.example/` for the expected names and keys.

Then run:

```
powershell
.\scripts\seal-secrets.ps1 `
  -InputDir bootstrap\dev\plain-secrets `
  -OutputDir bootstrap\dev\sealed-secrets `
  -Namespace quizzer-dev
```

Apply generated sealed manifests:

```
bash
kubectl apply -f bootstrap/dev/sealed-secrets/
```

Expected decrypted Secret objects:

- `registry-credentials`
- `api-gateway-app-secret`
- `identity-service-app-secret`
- `activity-service-app-secret`
- `identity-service-postgres-auth`
- `activity-service-postgres-auth`
- `identity-service-mongo-auth`
- `dev-mongodb-secrets`
- `cloudflare-origin-cert`

---

## Step 4: Build platform chart dependencies

```
bash
helm dependency build charts/platform
```

---

## Step 5: Deploy platform

```
bash
helm upgrade --install platform charts/platform \
-n quizzer-dev \
-f environments/dev/platform.yaml
```

This deploys enabled platform services such as:

- PostgreSQL
- Kafka
- MongoDB

---

## Step 6: Verify platform

```
bash
kubectl get pods -n quizzer-dev
kubectl get svc -n quizzer-dev
```

Additional resource checks:

```
bash
kubectl get clusters.postgresql.cnpg.io -n quizzer-dev
kubectl get kafka -n quizzer-dev
kubectl get kafkanodepool -n quizzer-dev
kubectl get perconaservermongodb -n quizzer-dev
```

---

## Step 7: Build applications chart dependencies

```
bash
helm dependency build charts/applications
```

---

## Step 8: Deploy applications

```
bash
helm upgrade --install applications charts/applications \
-n quizzer-dev \
-f environments/dev/applications.yaml
```

This deploys:

- API Gateway
- Identity Service
- Identity Service KafkaUser if enabled

---

## Step 9: Verify applications

```
bash
kubectl get deploy -n quizzer-dev
kubectl get pods -n quizzer-dev
kubectl get svc -n quizzer-dev
kubectl get ingress -n quizzer-dev
kubectl get kafkauser -n quizzer-dev
```

---

# 6. Render manifests without deploying

Use these commands to preview generated manifests before applying them.

## Render platform

```
bash
helm template platform charts/platform \
-n quizzer-dev \
-f environments/dev/platform.yaml
```

## Render applications

```
bash
helm template applications charts/applications \
-n quizzer-dev \
-f environments/dev/applications.yaml
```

This is useful for:

- validating values
- checking names
- debugging template logic

---

# 7. Updating resources

## If bootstrap secrets or namespace files change

Regenerate and apply `SealedSecret` manifests:

```
powershell
.\scripts\seal-secrets.ps1 -InputDir bootstrap\dev\plain-secrets -OutputDir bootstrap\dev\sealed-secrets -Namespace quizzer-dev
kubectl apply -f bootstrap/dev/sealed-secrets/
```

## If platform chart or platform values change

```
bash
helm upgrade --install platform charts/platform \
-n quizzer-dev \
-f environments/dev/platform.yaml
```

## If application chart or application values change

```
bash
helm upgrade --install applications charts/applications \
-n quizzer-dev \
-f environments/dev/applications.yaml
```

---

# 8. Debugging checklist

If something fails, use the checks below.

## Check Helm releases

```
bash
helm list -n quizzer-dev
```

## Check all resources

```
bash
kubectl get all -n quizzer-dev
```

## Check secrets

```
bash
kubectl get secrets -n quizzer-dev
```

## Check events

```
bash
kubectl get events -n quizzer-dev --sort-by=.metadata.creationTimestamp
```

## Describe a pod

```
bash
kubectl describe pod <POD_NAME> -n quizzer-dev
```

## View pod logs

```
bash
kubectl logs <POD_NAME> -n quizzer-dev
```

If container has multiple containers:

```
bash
kubectl logs <POD_NAME> -n quizzer-dev -c <CONTAINER_NAME>
```

## View deployment logs indirectly

First get pod names:

```
bash
kubectl get pods -n quizzer-dev
```

Then inspect the pod logs.

---

# 9. Common failure patterns

## Problem: namespace does not exist

Cause:

- bootstrap was not applied

Fix:

```
bash
kubectl apply -f bootstrap/dev/namespace.yaml
kubectl apply -f bootstrap/dev/sealed-secrets/
```

---

## Problem: image pull fails

Cause:

- registry secret missing or invalid

Fix:

- verify `registry-credentials` exists in the target namespace
- verify the secret content is valid
- verify `imagePullSecrets` references the correct secret name

Check:

```
bash
kubectl get secret registry-credentials -n quizzer-dev
```

---

## Problem: app cannot connect to Postgres

Cause:

- postgres cluster not ready
- wrong DB host
- wrong DB secret name
- wrong DB password

Check:

```
bash
kubectl get clusters.postgresql.cnpg.io -n quizzer-dev
kubectl get secrets -n quizzer-dev
kubectl describe pod <IDENTITY_POD_NAME> -n quizzer-dev
```

---

## Problem: app cannot connect to Kafka

Cause:

- kafka cluster not ready
- wrong bootstrap server hostname
- wrong Kafka user or password secret

Check:

```
bash
kubectl get kafka -n quizzer-dev
kubectl get kafkauser -n quizzer-dev
kubectl get secrets -n quizzer-dev
```

---

## Problem: app cannot connect to MongoDB

Cause:

- mongodb cluster not ready
- wrong Mongo host
- wrong secret reference

Check:

```
bash
kubectl get perconaservermongodb -n quizzer-dev
kubectl get secrets -n quizzer-dev
kubectl describe pod <IDENTITY_POD_NAME> -n quizzer-dev
```

---

## Problem: ingress not reachable

Cause:

- ingress controller not installed
- wrong host
- DNS/local hosts not configured
- ingress not created

Check:

```
bash
kubectl get ingress -n quizzer-dev
kubectl describe ingress <INGRESS_NAME> -n quizzer-dev
```

---

# 10. Clean removal

## Remove applications

```
bash
helm uninstall applications -n quizzer-dev
```

## Remove platform

```
bash
helm uninstall platform -n quizzer-dev
```

## Remove namespace and everything inside it

```
bash
kubectl delete namespace quizzer-dev
```

Use with care.

---

# 11. Rebuild from scratch

To rebuild the full DEV environment from zero:

```
bash
kubectl delete namespace quizzer-dev
kubectl apply -f bootstrap/dev/namespace.yaml
kubectl apply -f bootstrap/dev/sealed-secrets/

helm dependency build charts/platform
helm upgrade --install platform charts/platform -n quizzer-dev -f environments/dev/platform.yaml

helm dependency build charts/applications
helm upgrade --install applications charts/applications -n quizzer-dev -f environments/dev/applications.yaml
```

---

# 12. Production flow

Production uses the same deployment order:

1. `bootstrap/prod/`
2. `charts/platform` with `environments/prod/platform.yaml`
3. `charts/applications` with `environments/prod/applications.yaml`

Example:

```
bash
kubectl apply -f bootstrap/prod/

helm dependency build charts/platform
helm upgrade --install platform charts/platform -n quizzer-prod -f environments/prod/platform.yaml

helm dependency build charts/applications
helm upgrade --install applications charts/applications -n quizzer-prod -f environments/prod/applications.yaml
```

---

# 13. Rules to remember

## Rule 1

Bootstrap first, always.

## Rule 2

Platform before applications.

## Rule 3

Do not store real secrets in chart defaults.

## Rule 4

Environment values reference secret names, not raw secret values.

## Rule 5

If something is shared infrastructure, it belongs in platform.

## Rule 6

If something is application runtime config, it belongs in applications values.

---

# 14. Quick command summary

## Full DEV deploy

```
bash
kubectl apply -f bootstrap/dev/namespace.yaml
kubectl apply -f bootstrap/dev/sealed-secrets/

helm dependency build charts/platform
helm upgrade --install platform charts/platform -n quizzer-dev -f environments/dev/platform.yaml

helm dependency build charts/applications
helm upgrade --install applications charts/applications -n quizzer-dev -f environments/dev/applications.yaml
```

## Verify

```
bash
helm list -n quizzer-dev
kubectl get all -n quizzer-dev
kubectl get ingress -n quizzer-dev
kubectl get secrets -n quizzer-dev
```

## Cleanup

```
bash
helm uninstall applications -n quizzer-dev
helm uninstall platform -n quizzer-dev
kubectl delete namespace quizzer-dev
```

CloudNativePG
``` bash
helm repo add cloudnative-pg https://cloudnative-pg.github.io/charts
helm repo update
kubectl create namespace operators
helm upgrade --install cnpg cloudnative-pg/cloudnative-pg \
--namespace operators \
--create-namespace
```

Strimzi
``` bash
helm repo add strimzi https://strimzi.io/charts/
helm repo update
helm upgrade --install strimzi strimzi/strimzi-kafka-operator \
--namespace operators \
--create-namespace \
-f strimzi-operator-values.yaml

```

With:
``` yaml
watchAnyNamespace: true
createClusterRoles: true
```

Percona MongoDB operator
``` bash
git clone -b v1.22.0 https://github.com/percona/percona-server-mongodb-operator
cd percona-server-mongodb-operator
kubectl apply --server-side -f deploy/cw-bundle.yaml -n operators

```
