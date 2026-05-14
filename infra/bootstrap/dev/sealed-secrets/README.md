# DEV Sealed Secrets

Commit generated `SealedSecret` manifests in this directory.

Do not commit plaintext `Secret` manifests. Keep local plaintext inputs in:

```text
bootstrap/dev/plain-secrets/
```

Generate sealed manifests after the controller is installed:

```powershell
.\scripts\seal-secrets.ps1 `
  -InputDir bootstrap\dev\plain-secrets `
  -OutputDir bootstrap\dev\sealed-secrets `
  -Namespace quizzer-dev
```

The generated `SealedSecret` objects must keep the same Secret names used by:

- `environments/dev/applications.yaml`
- `environments/dev/platform.yaml`
