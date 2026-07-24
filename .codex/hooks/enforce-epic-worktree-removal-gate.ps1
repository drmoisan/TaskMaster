<#
.SYNOPSIS
    Denies git worktree removal until the matching epic feature is safely merged.
#>
[CmdletBinding()]
param()

$script:SafeWorktreeStatuses = @('merged', 'worktree_removed')

function ConvertFrom-CodexWorktreeJson {
    [CmdletBinding()]
    param([AllowNull()][AllowEmptyString()][string] $Raw, [Parameter(Mandatory)][string] $Name, [switch] $Optional)

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        if ($Optional) {
            return $null
        }
        throw "EPIC_WORKTREE_REMOVAL_BLOCKED: $Name is empty."
    }
    try {
        return $Raw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        if ($Optional) {
            return $null
        }
        throw "EPIC_WORKTREE_REMOVAL_BLOCKED: $Name is malformed JSON: $_"
    }
}

function Get-CodexWorktreeRemovalPath {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Command)

    $pattern = '(?i)\bgit\s+worktree\s+remove(?:\s+--force)?\s+(?:"(?<double>[^"]+)"|''(?<single>[^'']+)''|(?<bare>\S+))'
    if ($Command -notmatch $pattern) {
        return ''
    }
    foreach ($name in @('double', 'single', 'bare')) {
        if ($Matches[$name]) {
            return [string]$Matches[$name]
        }
    }
    return ''
}

function Get-NormalizedCodexWorktreePath {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $Path, [Parameter(Mandatory)][string] $WorkingDirectory)

    $resolved = if ([System.IO.Path]::IsPathRooted($Path)) {
        [System.IO.Path]::GetFullPath($Path)
    } else {
        [System.IO.Path]::GetFullPath((Join-Path $WorkingDirectory $Path))
    }
    return ($resolved -replace '\\', '/').TrimEnd('/')
}

function Find-CodexWorktreeFeature {
    [CmdletBinding()]
    param(
        [AllowNull()] $Checkpoint,
        [Parameter(Mandatory)][string] $TargetPath,
        [Parameter(Mandatory)][string] $WorkingDirectory
    )

    if ($null -eq $Checkpoint -or
        @($Checkpoint.PSObject.Properties.Name) -notcontains 'features') {
        return $null
    }
    $normalizedTarget = Get-NormalizedCodexWorktreePath -Path $TargetPath -WorkingDirectory $WorkingDirectory
    foreach ($feature in @($Checkpoint.features)) {
        if ($null -eq $feature -or
            @($feature.PSObject.Properties.Name) -notcontains 'worktree_path' -or
            [string]::IsNullOrWhiteSpace([string]$feature.worktree_path)) {
            continue
        }
        $normalizedFeature = Get-NormalizedCodexWorktreePath -Path ([string]$feature.worktree_path) -WorkingDirectory $WorkingDirectory
        if ($normalizedFeature -eq $normalizedTarget) {
            return $feature
        }
    }
    return $null
}

function Invoke-CodexWorktreeRemovalDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)][string] $PayloadRaw,
        [AllowNull()][AllowEmptyString()][string] $EpicCheckpointRaw
    )

    $payload = ConvertFrom-CodexWorktreeJson -Raw $PayloadRaw -Name 'PreToolUse input'
    if ([string]$payload.tool_name -ne 'Bash') {
        return $null
    }
    $command = [string]$payload.tool_input.command
    if ($command -notmatch '(?i)\bgit\s+worktree\s+remove\b') {
        return $null
    }
    $target = Get-CodexWorktreeRemovalPath -Command $command
    $checkpoint = ConvertFrom-CodexWorktreeJson -Raw $EpicCheckpointRaw -Name 'epic checkpoint' -Optional
    $workingDirectory = if ([string]::IsNullOrWhiteSpace([string]$payload.cwd)) {
        (Get-Location).Path
    } else {
        [string]$payload.cwd
    }
    $feature = if ($target) {
        Find-CodexWorktreeFeature -Checkpoint $checkpoint -TargetPath $target -WorkingDirectory $workingDirectory
    } else {
        $null
    }
    if ($null -ne $feature -and
        @($feature.PSObject.Properties.Name) -contains 'merge_status' -and
        $script:SafeWorktreeStatuses -contains [string]$feature.merge_status) {
        return $null
    }

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = "EPIC_WORKTREE_REMOVAL_BLOCKED: '$target' requires a matching epic feature with merge_status merged or worktree_removed."
        }
    }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payloadRaw = [Console]::In.ReadToEnd()
    $repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
    $checkpointPath = Join-Path $repositoryRoot 'artifacts/orchestration/epic-orchestrator-state.json'
    $checkpointRaw = if (Test-Path -LiteralPath $checkpointPath -PathType Leaf) {
        Get-Content -Raw -LiteralPath $checkpointPath
    } else {
        ''
    }
    $decision = Invoke-CodexWorktreeRemovalDecision -PayloadRaw $payloadRaw -EpicCheckpointRaw $checkpointRaw
    if ($null -ne $decision) {
        $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
