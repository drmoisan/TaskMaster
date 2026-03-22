Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$actionlintPath = Join-Path $repoRoot 'actionlint-bin\actionlint.exe'

if (-not (Test-Path $actionlintPath)) {
    throw "actionlint executable not found at '$actionlintPath'."
}

& $actionlintPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
