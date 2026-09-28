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

# Reason codes for the standalone-merge authorization branch (issue #670), spelled
# byte-identically to the Claude helpers file. Each of the four assignments below is the only
# place its code literal appears in this file.
$script:CodexStandaloneAbsentReasonCode = 'STANDALONE_MERGE_AUTHORIZATION_ABSENT'
$script:CodexStandalonePrMismatchReasonCode = 'STANDALONE_MERGE_AUTHORIZATION_PR_MISMATCH'
$script:CodexStandaloneMalformedReasonCode = 'STANDALONE_MERGE_AUTHORIZATION_MALFORMED'
$script:CodexStandaloneNotPrSpecificReasonCode = 'STANDALONE_MERGE_AUTHORIZATION_NOT_PR_SPECIFIC'

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

    # Standalone branch (issue #670), evaluated last and only for an explicit PR number: a
    # PR-specific authorization record bound to the payload session. Its deny keeps the gate
    # token first.
    if ($null -ne $prNumber) {
        $sessionId = [string]$payload.session_id
        $verdict = Test-CodexStandaloneMergeAuthorization -Checkpoints @($child, $epic) -CommandPrNumber $prNumber -SessionId $sessionId
        if ($verdict.Allowed) {
            return $null
        }
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = 'EPIC_MERGE_GATE_BLOCKED: ' + $verdict.ReasonCode + ': ' + $verdict.Message
            }
        }
    }

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = 'EPIC_MERGE_GATE_BLOCKED: gh pr merge --merge requires a safe epic child checkpoint or a successful final epic CI gate with a matching PR number.'
        }
    }
}

function Test-CodexStandalonePositiveInteger {
    <#
    .SYNOPSIS
        Report whether a parsed JSON value is an integer greater than zero; a digit string fails.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()] $Value)

    return (($Value -is [int] -or $Value -is [long]) -and $Value -gt 0)
}

function Test-CodexStandaloneText {
    <#
    .SYNOPSIS
        Report whether a value is a string whose trimmed length reaches a minimum.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()] $Value, [int] $MinimumLength = 1)

    return ($Value -is [string] -and $Value.Trim().Length -ge $MinimumLength)
}

function Get-CodexStandaloneField {
    <#
    .SYNOPSIS
        Read a named field off a parsed record, or $null; the unary comma keeps a
        one-element array from unrolling into its scalar.
    #>
    [CmdletBinding()]
    [OutputType([object], [object[]])]
    param([Parameter(Mandatory)] $Record, [Parameter(Mandatory)][string] $Name)

    $property = $Record.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }
    return , $property.Value
}

function Get-CodexStandaloneRecordFailure {
    <#
    .SYNOPSIS
        Return the first failing field of a matched record, or $null when every check passes.
    .DESCRIPTION
        Uses the same fixed order as the Claude helpers file: pr_url, issue_num, branch_name,
        authorized_by, authorized_at, basis, run_slug, session_id. branch_name is checked for
        presence only. A blank payload session fails closed.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)] $Record, [AllowNull()][AllowEmptyString()][string] $SessionId)

    $prUrl = Get-CodexStandaloneField -Record $Record -Name 'pr_url'
    if (-not (Test-CodexStandaloneText -Value $prUrl) -or
        -not $prUrl.EndsWith('/pull/' + [string]$Record.pr_number, [System.StringComparison]::Ordinal)) {
        return 'pr_url'
    }
    if (-not (Test-CodexStandalonePositiveInteger -Value (Get-CodexStandaloneField -Record $Record -Name 'issue_num'))) {
        return 'issue_num'
    }
    foreach ($name in @('branch_name', 'authorized_by')) {
        if (-not (Test-CodexStandaloneText -Value (Get-CodexStandaloneField -Record $Record -Name $name))) {
            return $name
        }
    }
    $authorizedAt = Get-CodexStandaloneField -Record $Record -Name 'authorized_at'
    $parsedAt = [DateTime]::MinValue
    $timestampValid = $authorizedAt -is [DateTime] -or $authorizedAt -is [DateTimeOffset]
    if (-not $timestampValid -and (Test-CodexStandaloneText -Value $authorizedAt)) {
        $timestampValid = [DateTime]::TryParse(
            $authorizedAt,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [System.Globalization.DateTimeStyles]::AdjustToUniversal -bor [System.Globalization.DateTimeStyles]::AssumeUniversal,
            [ref] $parsedAt)
    }
    if (-not $timestampValid) {
        return 'authorized_at'
    }
    if (-not (Test-CodexStandaloneText -Value (Get-CodexStandaloneField -Record $Record -Name 'basis') -MinimumLength 20)) {
        return 'basis'
    }
    if ($null -ne $Record.PSObject.Properties['run_slug'] -and
        -not (Test-CodexStandaloneText -Value (Get-CodexStandaloneField -Record $Record -Name 'run_slug'))) {
        return 'run_slug'
    }
    $recordSession = Get-CodexStandaloneField -Record $Record -Name 'session_id'
    if (-not (Test-CodexStandaloneText -Value $recordSession) -or
        [string]::IsNullOrWhiteSpace($SessionId) -or
        -not [string]::Equals($recordSession, $SessionId, [System.StringComparison]::Ordinal)) {
        return 'session_id'
    }
    return $null
}

function Test-CodexStandaloneMergeAuthorization {
    <#
    .SYNOPSIS
        Codex counterpart of the Claude standalone-merge authorization predicate.
    .DESCRIPTION
        Evaluates the parsed child and epic checkpoints with the same activation condition
        as the Claude side: a PR-specific record naming CommandPrNumber, with well-formed
        fields and a session_id equal to the payload session. Returns an object whose
        Allowed member is $true on success, or $false with ReasonCode and Message.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()][AllowEmptyCollection()][object[]] $Checkpoints,
        [Parameter(Mandatory)][int] $CommandPrNumber,
        [AllowNull()][AllowEmptyString()][string] $SessionId
    )

    $key = 'standalone_merge_authorizations'
    $present = $false
    $specific = $true
    $match = $null
    foreach ($checkpoint in @($Checkpoints)) {
        if ($checkpoint -isnot [System.Management.Automation.PSCustomObject] -or
            $null -eq $checkpoint.PSObject.Properties[$key]) {
            continue
        }
        $present = $true
        $block = $checkpoint.PSObject.Properties[$key].Value
        if ($block -isnot [System.Array] -or $block.Count -eq 0) {
            $specific = $false
            continue
        }
        foreach ($entry in $block) {
            $number = $null
            if ($entry -is [System.Management.Automation.PSCustomObject]) {
                $number = Get-CodexStandaloneField -Record $entry -Name 'pr_number'
            }
            if (-not (Test-CodexStandalonePositiveInteger -Value $number)) {
                $specific = $false
            } elseif ($null -eq $match -and $number -eq $CommandPrNumber) {
                $match = $entry
            }
        }
    }

    if (-not $present) {
        $code = $script:CodexStandaloneAbsentReasonCode
        $message = 'no orchestrator checkpoint carries an authorization record, but an authorized standalone merge path exists.'
    } elseif (-not $specific) {
        $code = $script:CodexStandaloneNotPrSpecificReasonCode
        $message = 'an orchestrator checkpoint carries the authorization key but names no specific pull request; a blanket flag is not an authorization record.'
    } elseif ($null -eq $match) {
        $code = $script:CodexStandalonePrMismatchReasonCode
        $message = "authorization records exist, but none names pull request $CommandPrNumber."
    } else {
        $field = Get-CodexStandaloneRecordFailure -Record $match -SessionId $SessionId
        if ($null -eq $field) {
            return [pscustomobject]@{ Allowed = $true; ReasonCode = $null; Message = $null }
        }
        $code = $script:CodexStandaloneMalformedReasonCode
        $message = "the matched authorization record has an invalid '$field'."
    }

    return [pscustomobject]@{
        Allowed    = $false
        ReasonCode = $code
        Message    = "$message To authorize a standalone merge, write a $key record naming this pull request and the current session_id into an orchestrator checkpoint, as described in .claude/rules/orchestrator-state.md."
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
