$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Push-Location $root
try {
    $demoDotnet = $env:FAITHTECH_DEMO_DOTNET
    if (-not $demoDotnet) { $demoDotnet = Join-Path $env:LOCALAPPDATA 'FaithTech/dotnet/dotnet.exe' }
    if (-not (Test-Path -LiteralPath $demoDotnet)) { throw 'Set FAITHTECH_DEMO_DOTNET to the SDK executable pinned by global.json.' }
    foreach ($project in @('frontend', 'e2e', 'design-system')) {
        & npm --prefix $project ci
        if ($LASTEXITCODE -ne 0) { throw "Dependency installation failed: $project" }
    }
    Push-Location e2e
    try { & npx playwright install chromium --only-shell; if ($LASTEXITCODE -ne 0) { throw 'Headless browser installation failed.' } }
    finally { Pop-Location }
    & npm --prefix frontend run build
    if ($LASTEXITCODE -ne 0) { throw 'Frontend build failed.' }
    & npm --prefix design-system run build
    if ($LASTEXITCODE -ne 0) { throw 'Gallery build failed.' }
    & $demoDotnet restore backend/FaithTechTorontoAiBuildEvent.slnx --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Backend restore failed.' }
    & $demoDotnet build backend/FaithTechTorontoAiBuildEvent.slnx --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Backend build failed.' }
    & node --test e2e/demo/media.test.mjs e2e/demo/host.test.mjs e2e/demo/delivery.test.mjs
    if ($LASTEXITCODE -ne 0) { throw 'Recording helper checks failed.' }
} finally { Pop-Location }
