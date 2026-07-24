<#
.SYNOPSIS
    Gates epic child and integration PR merges behind checkpointed green state.
#>
[CmdletBinding()]
param()

function ConvertFrom-CodexMergeJson {
    [CmdletBinding()]
    param([AllowNull()][AllowEmptyString()][string] $Raw, [Parameter(Mandatory)][string] $Name, [switch] $Optional)

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        if ($Optional) {
            return $null
        }
        throw "EPIC_MERGE_GATE_BLOCKED: $Name is empty."
    }
    try {
        return $Raw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        if ($Optional) {
            return $null
        }
        throw "EPIC_MERGE_GATE_BLOCKED: $Name is malformed JSON: $_"
    }
}

function Get-CodexMergeCommandPrNumber {
    [CmdletBinding()]
    [OutputType([int])]
    param([Parameter(Mandatory)][string] $Command)

    if ($Command -match '(?i)\bgh\s+pr\s+merge\s+(\d+)\b') {
        return [int]$Matches[1]
    }
    return $null
}

function Test-CodexChildMergeReady {
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()] $Checkpoint)

    if ($null -eq $Checkpoint) {
        return $false
    }
    $properties = @($Checkpoint.PSObject.Properties.Name)
    return (
        $properties -contains 'epic_mode' -and
        $Checkpoint.epic_mode -is [bool] -and
        [bool]$Checkpoint.epic_mode -and
        $properties -contains 'step9_status' -and
        @('passed', 'verified') -contains [string]$Checkpoint.step9_status
    )
}

function Test-CodexEpicMergeReady {
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()] $Checkpoint, [AllowNull()][Nullable[int]] $CommandPrNumber)

    if ($null -eq $Checkpoint -or
        @($Checkpoint.PSObject.Properties.Name) -notcontains 'epic_merge_pr' -or
        $null -eq $Checkpoint.epic_merge_pr) {
        return $false
    }
    $mergePr = $Checkpoint.epic_merge_pr
    if (@($mergePr.PSObject.Properties.Name) -notcontains 'ci_gate' -or
        $null -eq $mergePr.ci_gate -or
        [string]$mergePr.ci_gate.conclusion -ne 'success') {
        return $false
    }
    if ($null -ne $CommandPrNumber) {
        $parsed = 0
        if (@($mergePr.PSObject.Properties.Name) -notcontains 'pr_number' -or
            -not [int]::TryParse([string]$mergePr.pr_number, [ref]$parsed) -or
            $parsed -ne $CommandPrNumber) {
            return $false
        }
    }
    return $true
}

function Invoke-CodexEpicMergeDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)][string] $PayloadRaw,
        [AllowNull()][AllowEmptyString()][string] $ChildCheckpointRaw,
        [AllowNull()][AllowEmptyString()][string] $EpicCheckpointRaw
    )

    $payload = ConvertFrom-CodexMergeJson -Raw $PayloadRaw -Name 'PreToolUse input'
    if ([string]$payload.tool_name -ne 'Bash') {
        return $null
    }
    $command = [string]$payload.tool_input.command
    if ($command -notmatch '(?i)\bgh\s+pr\s+merge\b' -or $command -notmatch '(?i)--merge\b') {
        return $null
    }

    $child = ConvertFrom-CodexMergeJson -Raw $ChildCheckpointRaw -Name 'child checkpoint' -Optional
    if (Test-CodexChildMergeReady -Checkpoint $child) {
        return $null
    }
    $epic = ConvertFrom-CodexMergeJson -Raw $EpicCheckpointRaw -Name 'epic checkpoint' -Optional
    $prNumber = Get-CodexMergeCommandPrNumber -Command $command
    if (Test-CodexEpicMergeReady -Checkpoint $epic -CommandPrNumber $prNumber) {
        return $null
    }

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = 'EPIC_MERGE_GATE_BLOCKED: gh pr merge --merge requires a safe epic child checkpoint or a successful final epic CI gate with a matching PR number.'
        }
    }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payloadRaw = [Console]::In.ReadToEnd()
    $repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
    $childPath = Join-Path $repositoryRoot 'artifacts/orchestration/orchestrator-state.json'
    $epicPath = Join-Path $repositoryRoot 'artifacts/orchestration/epic-orchestrator-state.json'
    $childRaw = if (Test-Path -LiteralPath $childPath -PathType Leaf) { Get-Content -Raw -LiteralPath $childPath } else { '' }
    $epicRaw = if (Test-Path -LiteralPath $epicPath -PathType Leaf) { Get-Content -Raw -LiteralPath $epicPath } else { '' }
    $decision = Invoke-CodexEpicMergeDecision -PayloadRaw $payloadRaw -ChildCheckpointRaw $childRaw -EpicCheckpointRaw $epicRaw
    if ($null -ne $decision) {
        $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
