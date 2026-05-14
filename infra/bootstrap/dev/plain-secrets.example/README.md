# Local Plaintext Secret Inputs

This folder documents the expected local plaintext Secret input location.

Create real plaintext manifests in `bootstrap/dev/plain-secrets/` only on your workstation. That directory is ignored by Git.

Each file must be a normal Kubernetes `Secret` with the final name and namespace. Example:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: api-gateway-app-secret
  namespace: quizzer-dev
type: Opaque
stringData:
  JWT__SECRET: replace-me
```

Then run `scripts/seal-secrets.ps1` to generate commit-safe `SealedSecret` manifests under `bootstrap/dev/sealed-secrets/`.
