<#
.SYNOPSIS
    Blocks implementation writes before Issue #232 orchestration readiness exists.
#>
[CmdletBinding()]
param()

$script:CheckpointPath = 'artifacts/orchestration/orchestrator-state.json'
$script:Issue232FeatureFolder = 'docs/features/active/2026-06-24-harden-orchestrate-skill-232'

function ConvertFrom-CheckpointJson {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Json)

    return $Json | ConvertFrom-Json -ErrorAction Stop
}

function Get-StringProperty {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][AllowNull()] $Value,
        [Parameter(Mandatory)][string] $Name
    )

    if ($null -eq $Value -or -not ($Value.PSObject.Properties.Name -contains $Name)) {
        return ''
    }
    return ([string]$Value.$Name).Trim()
}

function Test-DocumentationOrEvidencePath {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][string] $NormalizedPath)

    return $NormalizedPath.StartsWith($script:Issue232FeatureFolder + '/')
}

function Test-ImplementationPath {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][string] $NormalizedPath)

    if (Test-DocumentationOrEvidencePath -NormalizedPath $NormalizedPath) {
        return $false
    }
    if ($NormalizedPath -eq $script:CheckpointPath) {
        return $false
    }
    return $NormalizedPath -match '\.(py|ps1|psm1|ts|tsx|js|jsx|cs|json|yml|yaml)$'
}

function Test-Issue232OrchestrationReady {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowNull()] $Payload)

    if ($null -eq $Payload) {
        return $false
    }
    $issueNum = Get-StringProperty -Value $Payload -Name 'issue-num'
    $featureFolder = Get-StringProperty -Value $Payload -Name 'feature-folder'
    $routeId = Get-StringProperty -Value $Payload -Name 'route_id'
    if (-not $routeId) {
        $routeId = Get-StringProperty -Value $Payload -Name 'path_selected'
    }
    $lifecycleReady = $false
    if ($Payload.PSObject.Properties.Name -contains 'lifecycle_ready') {
        $lifecycleReady = [bool]$Payload.lifecycle_ready
    }

    return (
        $issueNum -eq '232' -and
        $featureFolder -eq $script:Issue232FeatureFolder -and
        $routeId -and
        $lifecycleReady
    )
}

function Get-CheckpointContent {
    [CmdletBinding()]
    [OutputType([string])]
    param()

    if (-not (Test-Path -LiteralPath $script:CheckpointPath)) {
        return ''
    }
    return Get-Content -Raw -LiteralPath $script:CheckpointPath
}

function Invoke-OrchestrationPreimplementationGateDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw,
        [string] $CheckpointRaw
    )

    if (-not $ToolInputRaw) {
        return [ordered]@{ decision = 'allow' }
    }
    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "enforce-orchestration-preimplementation-gate hook received malformed JSON in CLAUDE_TOOL_INPUT: $_"
    }

    $filePath = $toolInput.file_path
    if (-not $filePath) {
        return [ordered]@{ decision = 'allow' }
    }
    $normalized = ([string]$filePath) -replace '\\', '/'
    if (-not (Test-ImplementationPath -NormalizedPath $normalized)) {
        return [ordered]@{ decision = 'allow' }
    }

    if (-not $CheckpointRaw) {
        $CheckpointRaw = Get-CheckpointContent
    }
    try {
        $checkpoint = ConvertFrom-CheckpointJson -Json $CheckpointRaw
    } catch {
        $checkpoint = $null
    }

    if (Test-Issue232OrchestrationReady -Payload $checkpoint) {
        return [ordered]@{ decision = 'allow' }
    }
    return [ordered]@{
        decision = 'block'
        reason   = 'PREIMPLEMENTATION_GATE_BLOCKED: Issue #232 implementation writes require artifacts/orchestration/orchestrator-state.json to contain route metadata, lifecycle readiness, and checkpoint state before implementation begins.'
    }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $decision = Invoke-OrchestrationPreimplementationGateDecision -ToolInputRaw $env:CLAUDE_TOOL_INPUT
} catch {
    Write-Error $_
    exit 1
}

$decision | ConvertTo-Json -Compress | Write-Output
exit 0
