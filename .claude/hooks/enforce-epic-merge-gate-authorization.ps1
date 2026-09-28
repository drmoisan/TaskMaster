<#
.SYNOPSIS
    Dot-sourced standalone-merge authorization helpers for enforce-epic-merge-gate.ps1.

.DESCRIPTION
    Holds the fourth allow condition of the epic merge gate, the standalone-merge
    authorization record, together with the two pure decision-envelope factories the gate
    returns on every path.

    This file is split out of the parent hook, .claude/hooks/enforce-epic-merge-gate.ps1,
    because that file had 13 lines of headroom under the 500-line limit, which a fourth allow
    condition, its predicates, and the rewritten header could not fit (issue #670). The two
    envelope factories were moved here unchanged: they read no script-scoped state and no
    test calls them by name, so the move is a pure relocation with no behaviour change.

    Every function in this file is pure. It reads no file, starts no process, and reads no
    clock; the parent hook supplies the already-parsed checkpoint objects and the live
    envelope session_id. A test can therefore dot-source this file on its own.

    Honest disclosure. The standalone-merge authorization record is a policy-level,
    auditable declaration and is
    not a cryptographic or security control.
    It names a specific pull request, a specific session, an authorizer, a time, and a
    stated basis, so that a standalone merge leaves a reviewable trail and cannot be reached
    by accident or by a record written for a different pull request. authorized_by is a
    declaration only: the hook checks that it is present and non-empty and does not verify
    the identity it names, because the runtime exposes no attested agent identity at Bash
    PreToolUse time. The record is not tamper-proof: any actor able to write
    artifacts/orchestration/*.json inside the authorizing session can write a record,
    because all agents share one filesystem and defaultPermissionMode is bypassPermissions.
    The session_id cross-check binds a record to the session that emitted it, so a stale
    record from a previous run or a record copied between runs authorizes nothing; it does
    not stop same-session forgery. The mechanism converts an untraceable bypass into a
    deliberate, attributable, auditable act. This is a documented accepted trade, not an
    unexamined gap.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies.
#>
[CmdletBinding()]
param()

# Reason codes for the standalone-merge authorization branch. Each of the four assignments
# below is the only place its code literal appears in this file; every other use reads the
# variable, and the Codex hook's spelling is compared against these assignments by a test.
$script:StandaloneAbsentReasonCode = 'STANDALONE_MERGE_AUTHORIZATION_ABSENT'
$script:StandalonePrMismatchReasonCode = 'STANDALONE_MERGE_AUTHORIZATION_PR_MISMATCH'
$script:StandaloneMalformedReasonCode = 'STANDALONE_MERGE_AUTHORIZATION_MALFORMED'
$script:StandaloneNotPrSpecificReasonCode = 'STANDALONE_MERGE_AUTHORIZATION_NOT_PR_SPECIFIC'

# The checkpoint key that carries the authorization records, and the pointer every
# standalone deny message ends with.
$script:StandaloneAuthorizationKey = 'standalone_merge_authorizations'
$script:StandaloneAuthorizationGuidance = 'To authorize a standalone merge, write a standalone_merge_authorizations record naming this pull request and the current session_id into an orchestrator checkpoint, as described in the Standalone-Merge-Authorization section of .claude/rules/orchestrator-state.md.'

function Get-EpicMergeGateAllowDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param()

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName      = 'PreToolUse'
            permissionDecision = 'allow'
        }
    }
}

function Get-EpicMergeGateBlockDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Reason
    )

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

function Test-StandalonePositiveJsonInteger {
    <#
    .SYNOPSIS
        Report whether a parsed JSON value is an integer strictly greater than zero.
    .DESCRIPTION
        ConvertFrom-Json yields a 32- or 64-bit integer for a JSON integer, a double for a
        JSON number with a fraction, and a string for a quoted value. Only the two integer
        types qualify, so a string that spells digits is rejected on its type.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Value
    )

    if (-not ($Value -is [int] -or $Value -is [long])) {
        return $false
    }
    return ($Value -gt 0)
}

function Test-StandaloneJsonObject {
    <#
    .SYNOPSIS
        Report whether a parsed JSON value is an object rather than a scalar or an array.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Value
    )

    return ($Value -is [System.Management.Automation.PSCustomObject])
}

function Get-StandaloneRecordField {
    <#
    .SYNOPSIS
        Read a named field off a parsed record, returning $null when the field is absent.
    .OUTPUTS
        System.Object, System.Object[], or $null
    #>
    [CmdletBinding()]
    [OutputType([object], [object[]])]
    param(
        [Parameter(Mandatory)]
        $Record,

        [Parameter(Mandatory)]
        [string] $Name
    )

    $property = $Record.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }
    # The unary comma keeps a one-element array from unrolling into its scalar, so a
    # pr_number spelled as [691] is still seen as an array and rejected.
    return , $property.Value
}

function Get-StandaloneMergeAuthorizationRecord {
    <#
    .SYNOPSIS
        Select the authorization record for one pull request from a parsed checkpoint.
    .DESCRIPTION
        Returns the first entry of the checkpoint's authorization array that is an object
        whose pr_number is a positive JSON integer equal to PrNumber, or nothing when no
        such entry exists. Reads no file, starts no process, and reads no clock.
    .PARAMETER Checkpoint
        Parsed checkpoint object, or $null when the checkpoint is absent or unreadable.
    .PARAMETER PrNumber
        The pull request number extracted from the command under evaluation.
    .OUTPUTS
        System.Object or nothing
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        $Checkpoint,

        [Parameter(Mandatory)]
        [int] $PrNumber
    )

    if (-not (Test-StandaloneJsonObject -Value $Checkpoint)) {
        return
    }
    $property = $Checkpoint.PSObject.Properties[$script:StandaloneAuthorizationKey]
    if ($null -eq $property -or $property.Value -isnot [System.Array]) {
        return
    }
    foreach ($entry in $property.Value) {
        if (-not (Test-StandaloneJsonObject -Value $entry)) {
            continue
        }
        $entryNumber = Get-StandaloneRecordField -Record $entry -Name 'pr_number'
        if ((Test-StandalonePositiveJsonInteger -Value $entryNumber) -and $entryNumber -eq $PrNumber) {
            return $entry
        }
    }
}

function Test-StandaloneNonBlankString {
    <#
    .SYNOPSIS
        Report whether a value is a string whose trimmed length reaches a minimum.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Value,

        [int] $MinimumLength = 1
    )

    if ($Value -isnot [string]) {
        return $false
    }
    return ($Value.Trim().Length -ge $MinimumLength)
}

function Test-StandaloneAuthorizedAt {
    <#
    .SYNOPSIS
        Report whether an authorized_at value is a parseable date-time.
    .DESCRIPTION
        ConvertFrom-Json already converts an ISO 8601 string into a date-time value, so
        such a value is accepted as parsed. Any other string must parse with the invariant
        culture, assuming and adjusting to UTC. No clock is read.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Value
    )

    if ($Value -is [DateTime] -or $Value -is [DateTimeOffset]) {
        return $true
    }
    if (-not (Test-StandaloneNonBlankString -Value $Value)) {
        return $false
    }
    $parsed = [DateTime]::MinValue
    return [DateTime]::TryParse(
        $Value,
        [System.Globalization.CultureInfo]::InvariantCulture,
        [System.Globalization.DateTimeStyles]::AdjustToUniversal -bor [System.Globalization.DateTimeStyles]::AssumeUniversal,
        [ref] $parsed)
}

function Get-StandaloneRecordFailure {
    <#
    .SYNOPSIS
        Build the malformed-record result naming the failing field.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)]
        [string] $Field,

        [Parameter(Mandatory)]
        [string] $Rule
    )

    return [pscustomobject]@{
        ReasonCode = $script:StandaloneMalformedReasonCode
        Message    = "the matched authorization record has an invalid '$Field': $Rule"
    }
}

function Test-StandaloneMergeAuthorizationRecord {
    <#
    .SYNOPSIS
        Validate the fields of a selected authorization record.
    .DESCRIPTION
        Runs the field checks in the fixed order pr_url, issue_num, branch_name,
        authorized_by, authorized_at, basis, run_slug, session_id. Returns nothing when
        every check passes; otherwise returns the first failure as an object carrying
        ReasonCode and a Message that names the failing field. branch_name is checked for
        presence only and is never compared against the live branch or working directory.
    .PARAMETER Record
        The parsed record selected by pr_number.
    .PARAMETER EnvelopeSessionId
        The session_id read from the live PreToolUse envelope. A blank value fails closed.
    .OUTPUTS
        System.Management.Automation.PSCustomObject or nothing
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        $Record,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $EnvelopeSessionId
    )

    $prUrl = Get-StandaloneRecordField -Record $Record -Name 'pr_url'
    $pullSuffix = '/pull/' + [string](Get-StandaloneRecordField -Record $Record -Name 'pr_number')
    if (-not (Test-StandaloneNonBlankString -Value $prUrl) -or
        -not $prUrl.EndsWith($pullSuffix, [System.StringComparison]::Ordinal)) {
        return Get-StandaloneRecordFailure -Field 'pr_url' -Rule "it must end with $pullSuffix."
    }
    if (-not (Test-StandalonePositiveJsonInteger -Value (Get-StandaloneRecordField -Record $Record -Name 'issue_num'))) {
        return Get-StandaloneRecordFailure -Field 'issue_num' -Rule 'it must be a JSON integer greater than zero.'
    }
    foreach ($name in @('branch_name', 'authorized_by')) {
        if (-not (Test-StandaloneNonBlankString -Value (Get-StandaloneRecordField -Record $Record -Name $name))) {
            return Get-StandaloneRecordFailure -Field $name -Rule 'it must be a non-empty string.'
        }
    }
    if (-not (Test-StandaloneAuthorizedAt -Value (Get-StandaloneRecordField -Record $Record -Name 'authorized_at'))) {
        return Get-StandaloneRecordFailure -Field 'authorized_at' -Rule 'it must be a parseable date-time string.'
    }
    if (-not (Test-StandaloneNonBlankString -Value (Get-StandaloneRecordField -Record $Record -Name 'basis') -MinimumLength 20)) {
        return Get-StandaloneRecordFailure -Field 'basis' -Rule 'it must be at least 20 characters after trimming.'
    }
    if ($null -ne $Record.PSObject.Properties['run_slug'] -and
        -not (Test-StandaloneNonBlankString -Value (Get-StandaloneRecordField -Record $Record -Name 'run_slug'))) {
        return Get-StandaloneRecordFailure -Field 'run_slug' -Rule 'when present it must be a non-empty string.'
    }
    $recordSessionId = Get-StandaloneRecordField -Record $Record -Name 'session_id'
    if (-not (Test-StandaloneNonBlankString -Value $recordSessionId) -or
        [string]::IsNullOrWhiteSpace($EnvelopeSessionId) -or
        -not [string]::Equals($recordSessionId, $EnvelopeSessionId, [System.StringComparison]::Ordinal)) {
        return Get-StandaloneRecordFailure -Field 'session_id' -Rule 'it must be non-empty and equal the session_id of the live hook envelope.'
    }
}

function Get-StandaloneAuthorizationBlockState {
    <#
    .SYNOPSIS
        Classify one parsed checkpoint as Absent, NotPrSpecific, or PrSpecific.
    .DESCRIPTION
        Absent means the checkpoint is missing or carries no authorization key. The block
        is PrSpecific only when it is a non-empty array whose every entry is an object with
        a positive integer pr_number; any other shape, including a blanket boolean, is
        NotPrSpecific, so a blanket flag is rejected visibly rather than read as absent.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        $Checkpoint
    )

    if (-not (Test-StandaloneJsonObject -Value $Checkpoint)) {
        return 'Absent'
    }
    $property = $Checkpoint.PSObject.Properties[$script:StandaloneAuthorizationKey]
    if ($null -eq $property) {
        return 'Absent'
    }
    $block = $property.Value
    if ($block -isnot [System.Array] -or $block.Count -eq 0) {
        return 'NotPrSpecific'
    }
    foreach ($entry in $block) {
        if (-not (Test-StandaloneJsonObject -Value $entry)) {
            return 'NotPrSpecific'
        }
        if (-not (Test-StandalonePositiveJsonInteger -Value (Get-StandaloneRecordField -Record $entry -Name 'pr_number'))) {
            return 'NotPrSpecific'
        }
    }
    return 'PrSpecific'
}

function Test-StandaloneCheckpointAllowsMerge {
    <#
    .SYNOPSIS
        Decision logic for the standalone-merge authorization path (branch 4).
    .DESCRIPTION
        Evaluates the three parsed orchestrator checkpoints for a record that authorizes
        CommandPrNumber in the live session. Returns an object whose Allowed member is
        $true on success; otherwise Allowed is $false and ReasonCode and Message describe
        the denial:
          - absent: no checkpoint carries the authorization key;
          - not PR-specific: a checkpoint carries the key but names no specific pull request;
          - PR mismatch: records exist, but none names CommandPrNumber;
          - malformed: the matched record fails a field check, and the message names the field.
    .PARAMETER Checkpoints
        The three parsed checkpoint objects; any element may be $null.
    .PARAMETER CommandPrNumber
        The explicit pull request number extracted from the command.
    .PARAMETER EnvelopeSessionId
        The session_id read from the live PreToolUse envelope.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()]
        [AllowEmptyCollection()]
        [object[]] $Checkpoints,

        [Parameter(Mandatory)]
        [int] $CommandPrNumber,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $EnvelopeSessionId
    )

    $states = @(foreach ($checkpoint in @($Checkpoints)) { Get-StandaloneAuthorizationBlockState -Checkpoint $checkpoint })
    $reasonCode = $null
    $message = $null
    if ($states -contains 'NotPrSpecific') {
        $reasonCode = $script:StandaloneNotPrSpecificReasonCode
        $message = 'an orchestrator checkpoint carries the authorization key but names no specific pull request; a blanket flag is not an authorization record.'
    } elseif ($states -notcontains 'PrSpecific') {
        $reasonCode = $script:StandaloneAbsentReasonCode
        $message = 'no orchestrator checkpoint carries an authorization record, but an authorized standalone merge path exists.'
    } else {
        foreach ($checkpoint in @($Checkpoints)) {
            $record = Get-StandaloneMergeAuthorizationRecord -Checkpoint $checkpoint -PrNumber $CommandPrNumber
            if ($null -eq $record) {
                continue
            }
            $failure = Test-StandaloneMergeAuthorizationRecord -Record $record -EnvelopeSessionId $EnvelopeSessionId
            if ($null -eq $failure) {
                return [pscustomobject]@{ Allowed = $true; ReasonCode = $null; Message = $null }
            }
            $reasonCode = $failure.ReasonCode
            $message = $failure.Message
            break
        }
        if ($null -eq $reasonCode) {
            $reasonCode = $script:StandalonePrMismatchReasonCode
            $message = "authorization records exist, but none names pull request $CommandPrNumber."
        }
    }

    return [pscustomobject]@{
        Allowed    = $false
        ReasonCode = $reasonCode
        Message    = "$message $script:StandaloneAuthorizationGuidance"
    }
}
