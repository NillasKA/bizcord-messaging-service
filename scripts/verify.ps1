$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $repositoryRoot
try {
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    $env:TESTINGPLATFORM_TELEMETRY_OPTOUT = '1'
    docker info --format '{{.ServerVersion}}'
    if ($LASTEXITCODE -ne 0) { throw 'Start Docker Desktop with Linux containers before verification.' }
    docker compose config --quiet
    if ($LASTEXITCODE -ne 0) { throw 'Compose configuration is invalid.' }
    docker compose up --build -d --wait --wait-timeout 180
    if ($LASTEXITCODE -ne 0) { throw 'Container deployment failed.' }
    $ready = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        try {
            $health = Invoke-WebRequest -Uri 'http://localhost:8000/health' -TimeoutSec 2
            if ($health.StatusCode -eq 200) { $ready = $true; break }
        } catch { Start-Sleep -Seconds 1 }
    }
    if (-not $ready) { throw 'The API did not become live.' }
    dotnet restore Bizcord.slnx --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Locked dependency restore failed.' }
    dotnet test --solution Bizcord.slnx -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
    & (Join-Path $PSScriptRoot 'demo.ps1')
    Write-Output 'PASS: deployment, all automated tests, and HTTP demonstration.'
} finally {
    Pop-Location
}
