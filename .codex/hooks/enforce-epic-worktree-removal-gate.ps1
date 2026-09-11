<#
.SYNOPSIS
    Denies git worktree removal until the matching epic feature is safely merged.
#>
[CmdletBinding()]
param()

# Shared command-line parser (issue #545). The scope filter and the operand extractor below
# both run against the segment that structurally invokes `git worktree remove`, which is what
# keeps the two runtimes on one implementation of the same concern.
. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')

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
    <#
    .SYNOPSIS
        Extract the target worktree path from a git worktree remove command.
    .DESCRIPTION
        The operand comes from the segment that structurally invokes git worktree remove, so
        a chained 'cd <path> &&' segment contributes nothing and a quoted mention of the
        phrase resolves to no operand. The tokenizer strips balanced double and single
        quotes, which is what the previous pattern's `double` and `single` alternatives did.

        The previous pattern accepted '--force' only immediately after 'remove'. That
        spelling is preserved and the trailing spelling now works too, because '--force' is a
        zero-argument flag that never contributes an operand wherever it is written. Its
        presence is read structurally through Test-CommandLineFlag rather than by a raw-text
        search. The empty-string-on-miss contract is unchanged: callers test `if ($target)`.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Command)

    $hasForce = Test-CommandLineFlag -CommandText $Command -CommandWord 'git' -SubcommandPath @('worktree', 'remove') -FlagName '--force'
    $operands = @(Get-CommandLineOperand -CommandText $Command -CommandWord 'git' -SubcommandPath @('worktree', 'remove'))

    if ($operands.Count -gt 0) {
        return [string]$operands[0]
    }
    if ($hasForce) {
        return '--force'
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
    # Scope filter. Structural, so a relocating spelling such as
    # 'git -C <dir> worktree remove <path>' is in scope and quoted prose that merely mentions
    # the phrase is not (issue #545).
    if (-not (Test-CommandLineInvocation -CommandText $command -CommandWord 'git' -SubcommandPath @('worktree', 'remove'))) {
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
