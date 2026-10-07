<#
.SYNOPSIS
    Portable remediation-loop review-outcome and accounting checks (R5-R11).

.DESCRIPTION
    Destination-runtime PowerShell port of the issue #484 invariants the
    authoritative Python validator applies to a checkpoint's remediation_loop:
    the review-outcome rules R8a, R8b, R9a-R9d, and R10, and the accounting
    rules R5, R6, R7a, R7b, and R11, matching the error-string templates in
    `scripts/dev_tools/_orchestrator_state_remediation_loop.py` and the rules
    document `.claude/rules/orchestrator-state.md`.

    The receipts module calls the exported entry point after its unchanged
    per-cycle checks, passing the cycle list or $null when cycles is absent or
    not a list, so the new checks also run for a loop with review outcomes and
    no cycles.

    Every comparison against the verdict and class vocabulary is case-sensitive
    (-ccontains, -cnotcontains, -ceq). An integer is a JSON number deserialized
    as [int] or [long]; a boolean is never an integer. Values interpolated into
    a message ({v}) render through ConvertTo-PythonDisplayText so the text
    matches Python's str().

    Every function is pure: it reads no file, starts no process, and never
    mutates its input. Each check returns a string array, empty when valid.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Import the shared checkpoint-value primitives, resolved relative to this
# module's directory so the import travels with the pushed-down pack.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force -ErrorAction Stop

# Loop and cycle keys read by the accounting rules.
$script:REVIEW_OUTCOMES_KEY = 'review_outcomes'
$script:COMPLETED_ATTEMPTS_KEY = 'completed_attempts'
$script:CANDIDATE_APPLIED_KEY = 'candidate_applied'
$script:OPENED_BY_REVIEW_KEY = 'opened_by_review'
$script:COMPLETED_EXECUTION_STATUS = 'complete'

# Review verdicts and remediability classes in spec table order, and the two
# derived class partitions. Pinned to the Python constants of the same names.
$script:REVIEW_VERDICTS = @('PASS', 'REMEDIATION_REQUIRED', 'HALT_NON_REMEDIABLE', 'AWAITING_CI')
$script:REMEDIABILITY_CLASSES = @('autonomous', 'external_dependency', 'policy_hold', 'awaiting_ci', 'human_decision_required')
$script:NON_REMEDIABLE_CLASSES = @('external_dependency', 'policy_hold', 'awaiting_ci', 'human_decision_required')
$script:HALT_CLASSES = @('external_dependency', 'policy_hold', 'human_decision_required')
$script:REMEDIABLE_CLASS = 'autonomous'
$script:REMEDIATION_REQUIRED_VERDICT = 'REMEDIATION_REQUIRED'


function Test-RemediationStrictInteger {
    <#
    .SYNOPSIS
        Report whether a deserialized JSON value is an integer and not a boolean.
    .DESCRIPTION
        Private predicate mirroring Python's isinstance(value, int) and not
        isinstance(value, bool). ConvertFrom-Json materializes a JSON integer as
        [int] or [long] and a JSON boolean as [bool].
    .PARAMETER Value
        The deserialized JSON value to classify. May be $null.
    .OUTPUTS
        System.Boolean - $true only for an [int] or [long] value.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    return (($Value -is [int]) -or ($Value -is [long])) -and -not ($Value -is [bool])
}

function Get-RemediationReviewVerdict {
    <#
    .SYNOPSIS
        Derive a review verdict from its findings' remediability classes.
    .DESCRIPTION
        Private helper mirroring derive_review_verdict. Evaluates, in order: no
        classes yields PASS; any autonomous class yields REMEDIATION_REQUIRED;
        any member of HALT_CLASSES yields HALT_NON_REMEDIABLE; otherwise
        AWAITING_CI. A member outside REMEDIABILITY_CLASSES (compared
        case-sensitively) raises a terminating error whose message begins
        'invalid remediability: '.
    .PARAMETER Remediability
        The remediability class of every blocking finding; may be empty and may
        repeat a class.
    .OUTPUTS
        System.String - one of the four review verdicts.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]] $Remediability
    )

    foreach ($value in $Remediability) {
        if ($script:REMEDIABILITY_CLASSES -cnotcontains $value) {
            throw "invalid remediability: $value"
        }
    }

    if ($Remediability.Count -eq 0) { return 'PASS' }
    if ($Remediability -ccontains $script:REMEDIABLE_CLASS) { return 'REMEDIATION_REQUIRED' }
    foreach ($value in $Remediability) {
        if ($script:HALT_CLASSES -ccontains $value) { return 'HALT_NON_REMEDIABLE' }
    }
    return 'AWAITING_CI'
}

function Get-RemediationReviewOutcomeError {
    <#
    .SYNOPSIS
        Return one review outcome's errors (R9a-R9d, then R10).
    .DESCRIPTION
        Private helper mirroring _validate_review_outcome. Checks the verdict
        (string check before membership), the findings list, and each finding's
        remediability (string check before membership); R10 runs only when none
        of those checks produced an error for the outcome.
    .PARAMETER Index
        The outcome's zero-based position, used for error context.
    .PARAMETER Outcome
        The deserialized outcome object.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [int] $Index,

        [Parameter(Mandatory = $true)]
        [psobject] $Outcome
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $prefix = "Checkpoint remediation review outcome #$Index"

    $verdict = (Get-CheckpointObjectMember -Owner $Outcome -Name 'verdict').Value
    if (-not ($verdict -is [string]) -or ($script:REVIEW_VERDICTS -cnotcontains [string]$verdict)) {
        $rendered = ConvertTo-PythonDisplayText -Value $verdict
        $errors.Add("$prefix verdict must be one of PASS, REMEDIATION_REQUIRED, HALT_NON_REMEDIABLE, AWAITING_CI; got: $rendered")
    }

    $classes = [System.Collections.Generic.List[string]]::new()
    $findings = (Get-CheckpointObjectMember -Owner $Outcome -Name 'findings').Value
    if (-not (Test-CheckpointListValue -Value $findings)) {
        $errors.Add("$prefix findings must be a list.")
    } else {
        $findingIndex = 0
        foreach ($finding in @($findings)) {
            if (-not (Test-CheckpointObjectValue -Value $finding)) {
                $errors.Add("$prefix finding #$findingIndex must be an object.")
            } else {
                $remediability = (Get-CheckpointObjectMember -Owner $finding -Name 'remediability').Value
                if (-not ($remediability -is [string]) -or ($script:REMEDIABILITY_CLASSES -cnotcontains [string]$remediability)) {
                    $rendered = ConvertTo-PythonDisplayText -Value $remediability
                    $errors.Add("$prefix finding #$findingIndex remediability must be one of autonomous, external_dependency, policy_hold, awaiting_ci, human_decision_required; got: $rendered")
                } else {
                    $classes.Add([string]$remediability)
                }
            }
            $findingIndex++
        }
    }

    # R10 compares only a well-formed outcome; any shape error above suppresses it.
    if ($errors.Count -eq 0) {
        $expected = Get-RemediationReviewVerdict -Remediability $classes.ToArray()
        if (-not ([string]$verdict -ceq $expected)) {
            $errors.Add("$prefix verdict $verdict does not match its findings (expected $expected).")
        }
    }

    return $errors.ToArray()
}

function Get-RemediationReviewOutcomeListError {
    <#
    .SYNOPSIS
        Return the review-outcome errors (R8a, R8b, then each outcome).
    .DESCRIPTION
        Private helper mirroring _validate_review_outcomes. An absent
        review_outcomes key yields no errors; a present non-list value yields R8a
        alone; otherwise each non-object outcome yields R8b and is skipped.
    .PARAMETER RemediationLoop
        The deserialized remediation_loop object.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $RemediationLoop
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $member = Get-CheckpointObjectMember -Owner $RemediationLoop -Name $script:REVIEW_OUTCOMES_KEY
    if (-not $member.Present) { return $errors.ToArray() }
    if (-not (Test-CheckpointListValue -Value $member.Value)) {
        $errors.Add('Checkpoint remediation_loop review_outcomes must be a list.')
        return $errors.ToArray()
    }

    $index = 0
    foreach ($outcome in @($member.Value)) {
        if (-not (Test-CheckpointObjectValue -Value $outcome)) {
            $errors.Add("Checkpoint remediation review outcome #$index must be an object.")
        } else {
            $errors.AddRange([string[]]@(Get-RemediationReviewOutcomeError -Index $index -Outcome $outcome))
        }
        $index++
    }

    return $errors.ToArray()
}

function Test-RemediationOpenedByReview {
    <#
    .SYNOPSIS
        Report whether an index names a REMEDIATION_REQUIRED outcome object.
    .DESCRIPTION
        Private predicate for R11: the value must be an integer (not a boolean)
        from 0 to one less than the outcome count of a list review_outcomes, and
        the indexed entry must be an object whose verdict is exactly
        REMEDIATION_REQUIRED. A negative index is out of range.
    .PARAMETER OpenedByReview
        The raw opened_by_review value. May be $null.
    .PARAMETER Outcomes
        The raw review_outcomes value. May be $null.
    .OUTPUTS
        System.Boolean - $true only when the reference is valid.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $OpenedByReview,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Outcomes
    )

    if (-not (Test-RemediationStrictInteger -Value $OpenedByReview)) { return $false }
    if (-not (Test-CheckpointListValue -Value $Outcomes)) { return $false }
    $outcomeList = @($Outcomes)
    if ($OpenedByReview -lt 0 -or $OpenedByReview -ge $outcomeList.Count) { return $false }
    $target = $outcomeList[[int]$OpenedByReview]
    if (-not (Test-CheckpointObjectValue -Value $target)) { return $false }
    $verdict = (Get-CheckpointObjectMember -Owner $target -Name 'verdict').Value
    return ($verdict -is [string]) -and ([string]$verdict -ceq $script:REMEDIATION_REQUIRED_VERDICT)
}

function Get-OrchestratorStateRemediationAccountingError {
    <#
    .SYNOPSIS
        Return the remediation_loop review-outcome and accounting errors (R5-R11).
    .DESCRIPTION
        Public entry mirroring the issue #484 tail of _validate_remediation_loop.
        Returns, in order: the review-outcome errors (R8a; otherwise per outcome
        R8b, R9a, R9b, R9c or R9d per finding, then R10); then, for each object
        cycle in order, R5, R6, and R11; then R7a, otherwise R7b. With none of the
        new keys present it returns nothing.
    .PARAMETER RemediationLoop
        The deserialized remediation_loop object.
    .PARAMETER Cycle
        The loop's cycles list, or $null when cycles is absent or not a list.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $RemediationLoop,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyCollection()]
        [object] $Cycle
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $errors.AddRange([string[]]@(Get-RemediationReviewOutcomeListError -RemediationLoop $RemediationLoop))

    $outcomes = (Get-CheckpointObjectMember -Owner $RemediationLoop -Name $script:REVIEW_OUTCOMES_KEY).Value
    $appliedCount = 0
    $cycleItems = @()
    if ($null -ne $Cycle) { $cycleItems = @($Cycle) }
    $index = 0
    foreach ($item in $cycleItems) {
        if (Test-CheckpointObjectValue -Value $item) {
            $prefix = "Checkpoint remediation cycle #$index"
            $applied = Get-CheckpointObjectMember -Owner $item -Name $script:CANDIDATE_APPLIED_KEY
            if ($applied.Present) {
                if (-not ($applied.Value -is [bool])) {
                    $errors.Add("$prefix candidate_applied must be a boolean.")
                } elseif ($applied.Value) {
                    $appliedCount++
                    $executionStatus = (Get-CheckpointObjectMember -Owner $item -Name 'execution_status').Value
                    if (-not ($executionStatus -is [string]) -or -not ([string]$executionStatus -ceq $script:COMPLETED_EXECUTION_STATUS)) {
                        $errors.Add("$prefix candidate_applied is true but execution_status is not 'complete'.")
                    }
                }
            }
            $opened = Get-CheckpointObjectMember -Owner $item -Name $script:OPENED_BY_REVIEW_KEY
            if ($opened.Present -and -not (Test-RemediationOpenedByReview -OpenedByReview $opened.Value -Outcomes $outcomes)) {
                $errors.Add("$prefix opened_by_review must reference a review outcome whose verdict is REMEDIATION_REQUIRED.")
            }
        }
        $index++
    }

    $attempts = Get-CheckpointObjectMember -Owner $RemediationLoop -Name $script:COMPLETED_ATTEMPTS_KEY
    if ($attempts.Present) {
        if (-not (Test-RemediationStrictInteger -Value $attempts.Value) -or $attempts.Value -lt 0) {
            $errors.Add('Checkpoint remediation_loop completed_attempts must be a non-negative integer.')
        } elseif ($attempts.Value -ne $appliedCount) {
            $errors.Add("Checkpoint remediation_loop completed_attempts is $($attempts.Value) but $appliedCount cycles have candidate_applied true.")
        }
    }

    return $errors.ToArray()
}

# Only the entry point is exported; the verdict helper and the per-outcome and
# per-reference checks stay private to this module.
Export-ModuleMember -Function Get-OrchestratorStateRemediationAccountingError
