<#
.SYNOPSIS
    Denies covered child mutations before all epic dependencies are durably merged.

.DESCRIPTION
    For launcher-bound execution children, this PreToolUse gate resolves the exact feature and
    epic checkpoint from the active session receipt before any local checkpoint exists. Native
    compatibility paths use the local epic context. Mutation remains blocked until every
    dependency is merged or worktree_removed.
#>
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'codex-epic-child-launch-attestation.ps1')

$script:AllowedDependencyStatuses = @('merged', 'worktree_removed')

function ConvertFrom-CodexWaveJson {
    [CmdletBinding()]
    param(
        [AllowNull()][AllowEmptyString()][string] $Raw,
        [Parameter(Mandatory)][string] $Name,
        [switch] $Optional
    )

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        if ($Optional) {
            return $null
        }
        throw "EPIC_WAVE_BARRIER_BLOCKED: $Name is empty."
    }
    try {
        return $Raw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "EPIC_WAVE_BARRIER_BLOCKED: $Name is malformed JSON: $_"
    }
}

function Get-CodexWaveFeatureKey {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)] $LocalCheckpoint)

    foreach ($name in @('issue-num', 'issue_num', 'feature-folder', 'feature_folder')) {
        if (@($LocalCheckpoint.PSObject.Properties.Name) -contains $name -and
            -not [string]::IsNullOrWhiteSpace([string]$LocalCheckpoint.$name)) {
            return (([string]$LocalCheckpoint.$name) -replace '\\', '/').TrimEnd('/').Split('/')[-1]
        }
    }
    if (@($LocalCheckpoint.PSObject.Properties.Name) -contains 'epic_context' -and
        $null -ne $LocalCheckpoint.epic_context) {
        foreach ($name in @('issue_num', 'feature_folder', 'target_feature_folder')) {
            if (@($LocalCheckpoint.epic_context.PSObject.Properties.Name) -contains $name -and
                -not [string]::IsNullOrWhiteSpace([string]$LocalCheckpoint.epic_context.$name)) {
                return (([string]$LocalCheckpoint.epic_context.$name) -replace '\\', '/').TrimEnd('/').Split('/')[-1]
            }
        }
    }
    return ''
}

function Find-CodexWaveFeature {
    [CmdletBinding()]
    param([AllowNull()] $Checkpoint, [Parameter(Mandatory)][string] $Key)

    if ($null -eq $Checkpoint -or
        @($Checkpoint.PSObject.Properties.Name) -notcontains 'features') {
        return $null
    }
    foreach ($feature in @($Checkpoint.features)) {
        if ($null -eq $feature) {
            continue
        }
        $properties = @($feature.PSObject.Properties.Name)
        if ($properties -contains 'issue_num' -and [string]$feature.issue_num -eq $Key) {
            return $feature
        }
        if ($properties -contains 'feature_folder') {
            $basename = (([string]$feature.feature_folder) -replace '\\', '/').TrimEnd('/').Split('/')[-1]
            if ($basename -eq $Key) {
                return $feature
            }
        }
    }
    return $null
}

function Find-CodexWaveLaunchFeature {
    [CmdletBinding()]
    param([AllowNull()] $Checkpoint, [Parameter(Mandatory)] $Receipt)

    if ($null -eq $Checkpoint -or
        @($Checkpoint.PSObject.Properties.Name) -notcontains 'features') {
        return $null
    }
    $expectedFolder = (([string]$Receipt.feature_folder) -replace '\\', '/').TrimEnd('/')
    foreach ($feature in @($Checkpoint.features)) {
        if ($null -eq $feature) {
            continue
        }
        $actualFolder = (([string]$feature.feature_folder) -replace '\\', '/').TrimEnd('/')
        $issueMatches = ($feature.issue_num | ConvertTo-Json -Compress) -ceq
        ($Receipt.issue_num | ConvertTo-Json -Compress)
        if ($issueMatches -and $actualFolder -ceq $expectedFolder) {
            return $feature
        }
    }
    return $null
}

function Test-CodexWaveDependenciesReady {
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()] $EpicCheckpoint, [AllowNull()] $Feature)

    if ($null -eq $EpicCheckpoint -or $null -eq $Feature) {
        return $false
    }
    if (@($Feature.PSObject.Properties.Name) -notcontains 'depends_on' -or
        @($Feature.depends_on).Count -eq 0) {
        return $true
    }
    foreach ($dependency in @($Feature.depends_on)) {
        $record = Find-CodexWaveFeature -Checkpoint $EpicCheckpoint -Key ([string]$dependency)
        if ($null -eq $record -or
            @($record.PSObject.Properties.Name) -notcontains 'merge_status' -or
            $script:AllowedDependencyStatuses -notcontains [string]$record.merge_status) {
            return $false
        }
    }
    return $true
}

function Test-CodexWaveMutation {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)] $Payload)

    $toolName = [string]$Payload.tool_name
    if ($toolName -in @('apply_patch', 'Edit', 'Write')) {
        return $true
    }
    if ($toolName -like 'mcp__*' -and $toolName -notlike 'mcp__drm-copilot__*') {
        return $true
    }
    if ($toolName -like 'mcp__drm-copilot__*') {
        return $toolName -notmatch '__(?:collect_|validate_|resolve_)'
    }
    if ($toolName -in @('Bash', 'shell_command')) {
        return $true
    }
    return $false
}

function Get-CodexWaveDenyDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([Parameter(Mandatory)][string] $FeatureKey, [Parameter(Mandatory)][string] $Reason)

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = "EPIC_WAVE_BARRIER_BLOCKED: '$FeatureKey' $Reason"
        }
    }
}

function Invoke-CodexEpicWaveDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)][string] $PayloadRaw,
        [AllowNull()][AllowEmptyString()][string] $LocalCheckpointRaw,
        [AllowNull()][AllowEmptyString()][string] $EpicCheckpointRaw,
        [AllowNull()][AllowEmptyString()][string] $LauncherReceiptRaw = '',
        [AllowNull()] $LauncherEnvironment,
        [AllowEmptyString()][string] $RepositoryRoot = '',
        [datetimeoffset] $Now = [datetimeoffset]::UtcNow
    )

    $payload = ConvertFrom-CodexWaveJson -Raw $PayloadRaw -Name 'PreToolUse input'
    if (-not (Test-CodexWaveMutation -Payload $payload)) {
        return $null
    }
    $launchId = if ($null -eq $LauncherEnvironment) { '' } else {
        [string]$LauncherEnvironment.launch_id
    }
    if (-not [string]::IsNullOrWhiteSpace($launchId)) {
        $context = [string]$LauncherEnvironment.execution_context
        if ($context -eq 'epic_preparation_child') {
            return $null
        }
        if ($context -ne 'epic_execution_child') {
            return Get-CodexWaveDenyDecision -FeatureKey $launchId -Reason 'has an invalid launcher execution context.'
        }
        $routingReceipt = [pscustomobject]@{ execution_context = $context }
        $authorityValid = Test-CodexEpicChildRoutingLaunchAuthority -RoutingReceipt $routingReceipt `
            -Payload $payload -RepositoryRoot $RepositoryRoot -LaunchEnvironment $LauncherEnvironment `
            -LaunchReceiptRaw $LauncherReceiptRaw -Now $Now
        if (-not $authorityValid) {
            return Get-CodexWaveDenyDecision -FeatureKey $launchId -Reason 'has no valid session-bound launcher receipt.'
        }
        $launchReceipt = ConvertFrom-CodexWaveJson -Raw $LauncherReceiptRaw -Name 'launcher receipt'
        $epicCheckpoint = ConvertFrom-CodexWaveJson -Raw $EpicCheckpointRaw -Name 'receipt-bound epic checkpoint'
        $feature = Find-CodexWaveLaunchFeature -Checkpoint $epicCheckpoint -Receipt $launchReceipt
        $featureKey = (([string]$launchReceipt.feature_folder) -replace '\\', '/').TrimEnd('/').Split('/')[-1]
        if (Test-CodexWaveDependenciesReady -EpicCheckpoint $epicCheckpoint -Feature $feature) {
            return $null
        }
        return Get-CodexWaveDenyDecision -FeatureKey $featureKey `
            -Reason 'cannot mutate until every depends_on edge is merged or worktree_removed in the receipt-bound epic checkpoint.'
    }
    if ([string]::IsNullOrWhiteSpace($LocalCheckpointRaw)) {
        return $null
    }
    $localCheckpoint = ConvertFrom-CodexWaveJson -Raw $LocalCheckpointRaw -Name 'child checkpoint'
    if (@($localCheckpoint.PSObject.Properties.Name) -notcontains 'epic_mode' -or
        $localCheckpoint.epic_mode -isnot [bool] -or
        -not [bool]$localCheckpoint.epic_mode) {
        return $null
    }

    $epicCheckpoint = ConvertFrom-CodexWaveJson -Raw $EpicCheckpointRaw -Name 'epic checkpoint'
    $featureKey = Get-CodexWaveFeatureKey -LocalCheckpoint $localCheckpoint
    $feature = if ($featureKey) {
        Find-CodexWaveFeature -Checkpoint $epicCheckpoint -Key $featureKey
    } else {
        $null
    }
    if (Test-CodexWaveDependenciesReady -EpicCheckpoint $epicCheckpoint -Feature $feature) {
        return $null
    }

    return Get-CodexWaveDenyDecision -FeatureKey $featureKey `
        -Reason 'cannot mutate until every depends_on edge is merged or worktree_removed in the epic checkpoint.'
}

function Get-CodexPrimaryWorktreeRoot {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $RepositoryRoot)

    $output = & git -C $RepositoryRoot worktree list --porcelain 2>$null
    $first = @($output | Where-Object { $_ -like 'worktree *' } | Select-Object -First 1)
    if ($first.Count -eq 0) {
        return $RepositoryRoot
    }
    return ([string]$first[0]).Substring('worktree '.Length)
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payloadRaw = [Console]::In.ReadToEnd()
    $repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
    $localPath = Join-Path $repositoryRoot 'artifacts/orchestration/orchestrator-state.json'
    $localRaw = if (Test-Path -LiteralPath $localPath -PathType Leaf) {
        Get-Content -Raw -LiteralPath $localPath
    } else {
        ''
    }
    $launcherEnvironment = Get-CodexEpicChildLaunchEnvironment
    $launcherReceiptRaw = if (-not [string]::IsNullOrWhiteSpace($launcherEnvironment.receipt_path) -and
        (Test-Path -LiteralPath $launcherEnvironment.receipt_path -PathType Leaf)) {
        Get-Content -Raw -LiteralPath $launcherEnvironment.receipt_path
    } else {
        ''
    }
    $launcherReceipt = ConvertFrom-CodexWaveJson -Raw $launcherReceiptRaw -Name 'launcher receipt' -Optional
    $primaryRoot = Get-CodexPrimaryWorktreeRoot -RepositoryRoot $repositoryRoot
    $epicPath = if ($null -ne $launcherReceipt -and
        [string]$launcherEnvironment.execution_context -eq 'epic_execution_child') {
        [string]$launcherReceipt.checkpoint_path
    } else {
        Join-Path $primaryRoot 'artifacts/orchestration/epic-orchestrator-state.json'
    }
    $epicRaw = if (Test-Path -LiteralPath $epicPath -PathType Leaf) {
        Get-Content -Raw -LiteralPath $epicPath
    } else {
        ''
    }
    $decision = Invoke-CodexEpicWaveDecision -PayloadRaw $payloadRaw -LocalCheckpointRaw $localRaw `
        -EpicCheckpointRaw $epicRaw -LauncherReceiptRaw $launcherReceiptRaw `
        -LauncherEnvironment $launcherEnvironment -RepositoryRoot $repositoryRoot
    if ($null -ne $decision) {
        $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
