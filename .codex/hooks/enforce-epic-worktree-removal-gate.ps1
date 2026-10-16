<#
.SYNOPSIS
    Denies git worktree removal until the matching epic feature is safely merged.
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }

# Shared command-line parser (issue #545). The scope filter and the operand extractor below
# both run against the segment that structurally invokes `git worktree remove`, which is what
# keeps the two runtimes on one implementation of the same concern.
try { . (Join-Path $PSScriptRoot 'hook-command-scanner.ps1') } catch { Add-HookDependencyFailure -Name 'hook-command-scanner.ps1' -ErrorRecord $_ }
try { . (Join-Path $PSScriptRoot 'hook-command-invocation.ps1') } catch { Add-HookDependencyFailure -Name 'hook-command-invocation.ps1' -ErrorRecord $_ }

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
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'EPIC_WORKTREE_REMOVAL_BLOCKED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    $payload = ConvertFrom-CodexWorktreeJson -Raw $PayloadRaw -Name 'PreToolUse input'
    if ([string]$payload.tool_name -ne 'Bash') {
        return $null
    }
    $command = [string]$payload.tool_input.command
    # Scope filter. Structural, so a relocating spelling such as
    # 'git -C <dir> worktree remove <path>' is in scope and quoted prose that merely mentions
    # the phrase is not (issue #545).
    if (-not (Test-CommandLineInvocation -CommandText $command -CommandWord 'git' -SubcommandPath @('worktree', 'remove'))) {
        return $null
    }
    # Issue #824: the removal targets are derived structurally from every invocation. A
    # target that cannot be derived denies before the checkpoint is read, and every derived
    # target must be authorized on its own.
    $resolution = Resolve-CommandLineInvocationTarget -CommandText $command -CommandWord 'git' -SubcommandPath @('worktree', 'remove')
    if ($resolution.Status -eq 'NoMatch') {
        return $null
    }
    if ($resolution.Status -ne 'Targets') {
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = 'EPIC_WORKTREE_REMOVAL_BLOCKED: TARGET_WORKTREE_NOT_DERIVABLE: the git worktree remove target cannot be derived from the command text. No checkpoint can authorize this removal.'
            }
        }
    }
    $checkpoint = ConvertFrom-CodexWorktreeJson -Raw $EpicCheckpointRaw -Name 'epic checkpoint' -Optional
    $workingDirectory = if ([string]::IsNullOrWhiteSpace([string]$payload.cwd)) {
        (Get-Location).Path
    } else {
        [string]$payload.cwd
    }
    foreach ($target in @($resolution.Targets)) {
        $denial = Get-CodexWorktreeRemovalTargetDenial -Target $target -Checkpoint $checkpoint -WorkingDirectory $workingDirectory
        if ($null -ne $denial) {
            return $denial
        }
    }
    return $null
}

function Get-CodexWorktreeRemovalTargetDenial {
    <#
    .SYNOPSIS
        Evaluate one derived removal target and return its deny decision, or $null when authorized.
    .PARAMETER Target
        One removal target derived by Resolve-CommandLineInvocationTarget.
    .PARAMETER Checkpoint
        The parsed epic checkpoint, or $null when absent or unreadable.
    .PARAMETER WorkingDirectory
        The directory a relative target resolves against.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary or $null
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)][string] $Target,
        [AllowNull()] $Checkpoint,
        [Parameter(Mandatory)][string] $WorkingDirectory
    )

    $feature = Find-CodexWorktreeFeature -Checkpoint $Checkpoint -TargetPath $Target -WorkingDirectory $WorkingDirectory
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
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('EPIC_WORKTREE_REMOVAL_BLOCKED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'EPIC_WORKTREE_REMOVAL_BLOCKED:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

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
