param(
    [string]$InputDir = "bootstrap/dev/plain-secrets",
    [string]$OutputDir = "bootstrap/dev/sealed-secrets",
    [string]$Namespace = "quizzer-dev",
    [string]$ControllerName = "sealed-secrets-controller",
    [string]$ControllerNamespace = "quizzer-dev"
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command kubeseal -ErrorAction SilentlyContinue)) {
    throw "kubeseal was not found on PATH. Install kubeseal before sealing secrets."
}

if (-not (Test-Path -Path $InputDir -PathType Container)) {
    throw "Input directory '$InputDir' does not exist."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$secretFiles = Get-ChildItem -Path $InputDir -File |
    Where-Object { $_.Extension -in ".yaml", ".yml" }
if (-not $secretFiles) {
    throw "No YAML Secret files found in '$InputDir'."
}

foreach ($file in $secretFiles) {
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    $outputFile = Join-Path $OutputDir "$baseName.sealed.yaml"

    kubeseal `
        --format yaml `
        --scope strict `
        --namespace $Namespace `
        --controller-name $ControllerName `
        --controller-namespace $ControllerNamespace `
        --filename $file.FullName |
        Set-Content -Path $outputFile -Encoding utf8

    Write-Host "Wrote $outputFile"
}
