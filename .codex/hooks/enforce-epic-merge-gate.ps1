<#
.SYNOPSIS
    Gates epic child and integration PR merges behind checkpointed green state.
#>
[CmdletBinding()]
param()

# Shared command-line parser (issue #545). The scope filter and the PR-number resolver below
# both run against the segment that structurally invokes `gh pr merge`, which is what keeps
# the two runtimes on one implementation of the same concern.
. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')

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
    <#
    .SYNOPSIS
        Resolve the explicit pull-request number of a gh pr merge invocation, or $null.
    .DESCRIPTION
        The number is taken from the segment that structurally invokes `gh pr merge` and
        from nowhere else: the positional spelling through Get-CommandLineOperand, the
        flag-led and equals-joined spellings through Get-CommandLineFlagValue. A leading
        `cd <path>` segment contributes no operand and no flag value, so no digit run
        outside the merge segment can be mistaken for a pull-request number.

        This copy has never carried the Claude copy's unanchored whole-text digit scan and
        deliberately does not acquire one. No regular expression matching a bare digit run
        is introduced here; the only pattern below anchors an all-digit token end to end.
    .OUTPUTS
        System.Nullable[int]
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param([Parameter(Mandatory)][string] $Command)

    foreach ($operand in @(Get-CommandLineOperand -CommandText $Command -CommandWord 'gh' -SubcommandPath @('pr', 'merge'))) {
        if ($operand -match '^\d+$') {
            return [int]$operand
        }
    }

    $flagValue = Get-CommandLineFlagValue -CommandText $Command -CommandWord 'gh' -SubcommandPath @('pr', 'merge') -FlagName '--merge'
    if ($null -ne $flagValue -and $flagValue -match '^\d+$') {
        return [int]$flagValue
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
    # Both legs are read structurally from the segment that invokes the command, so a
    # quoted mention of the phrase no longer brings a command into scope. The flag leg needs
    # a raw-scan fallback: a wrapper's quoted argument collapses into ONE token, so a flag
    # inside it is never read as a token, and the token-only read took
    # bash -c "gh pr merge --merge 688" out of scope.
    $isMergeInvocation = Test-CommandLineInvocation -CommandText $command -CommandWord 'gh' -SubcommandPath @('pr', 'merge')
    $hasMergeFlag = Test-CommandLineFlag -CommandText $command -CommandWord 'gh' -SubcommandPath @('pr', 'merge') -FlagName '--merge'
    if (-not $hasMergeFlag) {
        foreach ($segment in @(Read-CommandLineSegment -CommandText $command)) {
            if ((Test-CommandLineSegmentRawScan -Segment $segment) -and
                $segment.ScanText.IndexOf('--merge', [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
                $hasMergeFlag = $true
                break
            }
        }
    }
    if (-not $isMergeInvocation -or -not $hasMergeFlag) {
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
