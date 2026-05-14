# Sealed Secrets Workflow

This repository uses Bitnami Sealed Secrets for GitOps-safe secret storage.

## Ownership

- `charts/platform` installs the Sealed Secrets controller.
- `bootstrap/<env>/sealed-secrets/` stores committed `SealedSecret` manifests.
- `bootstrap/<env>/plain-secrets/` is local-only and ignored by Git.
- `environments/<env>/*.yaml` reference final Kubernetes Secret names and keys only.

## First-time DEV setup

Install the namespace and controller first:

```powershell
kubectl apply -f bootstrap/dev/namespace.yaml

helm dependency build charts/platform
helm upgrade --install platform charts/platform `
  -n quizzer-dev `
  -f environments/dev/platform.yaml `
  --set postgres.enabled=false `
  --set kafka.enabled=false `
  --set mongodb.enabled=false
```

Create local plaintext Secret manifests in `bootstrap/dev/plain-secrets/`.
Use the redacted examples in `bootstrap/dev/plain-secrets.example/` as the contract for names, namespaces, types, and keys.

Generate commit-safe sealed manifests:

```powershell
.\scripts\seal-secrets.ps1 `
  -InputDir bootstrap\dev\plain-secrets `
  -OutputDir bootstrap\dev\sealed-secrets `
  -Namespace quizzer-dev
```

Apply the sealed secrets:

```powershell
kubectl apply -f bootstrap/dev/sealed-secrets/
```

Then deploy the full platform and applications:

```powershell
helm upgrade --install platform charts/platform `
  -n quizzer-dev `
  -f environments/dev/platform.yaml

helm dependency build charts/applications
helm upgrade --install applications charts/applications `
  -n quizzer-dev `
  -f environments/dev/applications.yaml
```

## Normal update flow

1. Update local plaintext files in `bootstrap/dev/plain-secrets/`.
2. Run `scripts/seal-secrets.ps1`.
3. Commit the generated files in `bootstrap/dev/sealed-secrets/`.
4. Apply or let GitOps sync the sealed manifests.

Never commit plaintext `Secret` manifests.
