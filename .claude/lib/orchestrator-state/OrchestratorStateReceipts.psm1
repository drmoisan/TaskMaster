<#
.SYNOPSIS
    Portable delegation-receipt, remediation-cycle, and human-interaction checks.

.DESCRIPTION
    Destination-runtime PowerShell port of the unconditional optional-key checks
    the authoritative Python validator performs for the completion hook's call.
    The rows ported here are the parity-inventory families U5 (delegation_receipts
    shape), U6.R (remediation_loop cycle invariants), and U6.H (human_interaction
    shape), matching the error-string templates in
    `scripts/dev_tools/validate_orchestrator_state.py` and
    `scripts/dev_tools/_orchestrator_state_human_interaction.py`.

    U6.H reconciliation (spec Parity Contract): these library checks are ADDITIVE
    to the stricter hook-internal `Test-HumanInteractionShape` in
    `.claude/hooks/validate-orchestrator-output.ps1`, which blocks a `halt`
    response and verifies runbook-file existence. That hook check is retained
    unchanged and continues to run alongside these rows; strictness never
    decreases. A checkpoint that fails either layer is blocked.

    The shared JSON shape predicates, the absent-versus-null member accessor, the
    ordinal key sort, the Python zero-equivalence rule, and the Python
    `str()`/`repr()` interpolation renderers live in the sibling helper module
    `OrchestratorStateCheckpointValue.psm1`, created under the plan's
    pre-authorized split so no parity module exceeds the 500-line file cap. Every
    module in this family imports those primitives from that one implementation.

    Every function is pure: it reads no file, starts no process, and never mutates
    its input. Each check returns a string array, empty when the block is valid.
#>

Set-StrictMode -Version Latest

# Import the shared checkpoint-value primitives, resolved relative to this
# module's directory so the import travels with the pushed-down pack regardless
# of the consumer repository's working directory.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force

# The eight keys every list-form delegation receipt must carry. Pinned to
# REQUIRED_RECEIPT_KEYS in scripts/dev_tools/validate_orchestrator_state.py.
$script:REQUIRED_RECEIPT_KEYS = @(
    'step',
    'agent_name',
    'agent_id',
    'skill_source',
    'started_at',
    'completed_at',
    'result_signal',
    'artifact_paths'
)

# The only two namespaces the object form of delegation_receipts may carry, and
# the three sub-keys the promotion namespace may carry.
$script:AGENT_RECEIPT_NAMESPACE_KEY = 'agents'
$script:PROMOTION_RECEIPT_NAMESPACE_KEY = 'promotion'
$script:PROMOTION_RECEIPT_KEYS = @('potential_entry', 'issue', 'feature_folder')

# Remediation-cycle constants: the cycles array key, the execution statuses that
# may only be recorded once preflight cleared, and the cleared status literal.
$script:REMEDIATION_CYCLES_KEY = 'cycles'
$script:EXECUTION_STATUSES_REQUIRING_CLEAR_PREFLIGHT = @('in_progress', 'complete', 'failed')
$script:PREFLIGHT_CLEARED_STATUS = 'clear'

# Human-interaction constants: the requirements list key, the three permitted
# response values, and the response that additionally requires a runbook path.
$script:HUMAN_INTERACTION_REQUIREMENTS_KEY = 'requirements'
$script:HUMAN_INTERACTION_RESPONSE_ENUM = @('scope_change', 'exception', 'halt')
$script:HUMAN_INTERACTION_EXCEPTION_RESPONSE = 'exception'


function Get-DelegationReceiptListError {
    <#
    .SYNOPSIS
        Return the list-form delegation-receipt errors (rows U5.1-U5.3).
    .DESCRIPTION
        Private helper mirroring _validate_list_delegation_receipts. Validates each
        receipt independently so callers receive a complete error list.
    .PARAMETER Receipts
        The deserialized JSON array of receipt objects.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Receipts
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $index = 0

    # Walk every receipt position so each malformed entry is reported with its own
    # index; a non-object entry short-circuits that entry's key checks.
    foreach ($receipt in @($Receipts)) {
        if (-not (Test-CheckpointObjectValue -Value $receipt)) {
            $errors.Add("Checkpoint delegation receipt #$index must be an object.")
            $index++
            continue
        }

        # Key PRESENCE is the Python test (`key not in receipt`), so a present key
        # holding null satisfies the requirement.
        $names = @(Get-CheckpointObjectMemberName -Owner $receipt)
        foreach ($key in $script:REQUIRED_RECEIPT_KEYS) {
            if ($names -notcontains $key) {
                $errors.Add("Checkpoint delegation receipt #$index missing key: $key")
            }
        }

        # artifact_paths may be absent or null, but a present non-null value must
        # be a list.
        $artifactPaths = (Get-CheckpointObjectMember -Owner $receipt -Name 'artifact_paths').Value
        if ($null -ne $artifactPaths -and -not (Test-CheckpointListValue -Value $artifactPaths)) {
            $errors.Add("Checkpoint delegation receipt #$index artifact_paths must be a list.")
        }

        $index++
    }

    return $errors.ToArray()
}

function Get-DelegationReceiptNamespaceError {
    <#
    .SYNOPSIS
        Return the object-form delegation-receipt errors (rows U5.4-U5.7).
    .DESCRIPTION
        Private helper mirroring _validate_namespaced_delegation_receipts. Reports
        unsupported top-level namespaces in ordinal order, applies the list-form
        checks to the agents namespace, and rejects an unsupported promotion
        sub-key or a non-object promotion namespace.
    .PARAMETER Receipts
        The deserialized JSON object form of delegation_receipts.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $Receipts
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $names = @(Get-CheckpointObjectMemberName -Owner $Receipts)

    # Reject every namespace outside {agents, promotion}, ordered ordinally so the
    # error sequence matches Python's sorted() output.
    $unsupported = @($names | Where-Object {
            $_ -ne $script:AGENT_RECEIPT_NAMESPACE_KEY -and $_ -ne $script:PROMOTION_RECEIPT_NAMESPACE_KEY
        })
    foreach ($key in (Get-CheckpointOrdinalSortedName -Name ([string[]]$unsupported))) {
        $errors.Add("Checkpoint delegation_receipts object contains unsupported key: $key")
    }

    # A present agents namespace must be a list; when it is, the list-form rows
    # apply to its entries unchanged.
    if ($names -contains $script:AGENT_RECEIPT_NAMESPACE_KEY) {
        $agentReceipts = (Get-CheckpointObjectMember -Owner $Receipts -Name $script:AGENT_RECEIPT_NAMESPACE_KEY).Value
        if (-not (Test-CheckpointListValue -Value $agentReceipts)) {
            $errors.Add('Checkpoint delegation_receipts.agents must be a list.')
        } else {
            $errors.AddRange([string[]]@(Get-DelegationReceiptListError -Receipts $agentReceipts))
        }
    }

    # An absent or null promotion namespace ends the check; a present non-object
    # value is malformed and stops further promotion inspection.
    $promotion = (Get-CheckpointObjectMember -Owner $Receipts -Name $script:PROMOTION_RECEIPT_NAMESPACE_KEY).Value
    if ($null -eq $promotion) {
        return $errors.ToArray()
    }
    if (-not (Test-CheckpointObjectValue -Value $promotion)) {
        $errors.Add('Checkpoint delegation_receipts.promotion must be an object namespace.')
        return $errors.ToArray()
    }

    $promotionNames = @(Get-CheckpointObjectMemberName -Owner $promotion)
    $unsupportedPromotion = @($promotionNames | Where-Object { $script:PROMOTION_RECEIPT_KEYS -notcontains $_ })
    foreach ($key in (Get-CheckpointOrdinalSortedName -Name ([string[]]$unsupportedPromotion))) {
        $errors.Add("Checkpoint delegation_receipts.promotion contains unsupported key: $key")
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateDelegationReceiptError {
    <#
    .SYNOPSIS
        Return the delegation_receipts errors (inventory rows U5.1-U5.8).
    .DESCRIPTION
        Public dispatch mirroring the delegation-receipt branch of
        validate_orchestrator_state_text: a null or absent value contributes no
        errors, a list routes to the legacy list-form checks, an object routes to
        the namespaced checks, and any other value is itself the error.
    .PARAMETER Value
        The raw deserialized value of the checkpoint's delegation_receipts key.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # A null value is the Python `receipts is not None` guard: nothing to check.
    if ($null -eq $Value) { return $errors.ToArray() }

    # Route on the deserialized shape: the list branch is the legacy form, the
    # object branch is the additive namespace form, anything else is invalid.
    if (Test-CheckpointListValue -Value $Value) {
        $errors.AddRange([string[]]@(Get-DelegationReceiptListError -Receipts $Value))
    } elseif (Test-CheckpointObjectValue -Value $Value) {
        $errors.AddRange([string[]]@(Get-DelegationReceiptNamespaceError -Receipts $Value))
    } else {
        $errors.Add('Checkpoint delegation_receipts must be a list or object namespace.')
    }

    return $errors.ToArray()
}

function Get-RemediationCycleError {
    <#
    .SYNOPSIS
        Return one remediation cycle's errors (rows U6.R2-U6.R4).
    .DESCRIPTION
        Private helper mirroring _validate_remediation_cycle: a non-empty
        plan_path, execution only after a cleared preflight, and a satisfied exit
        gate only with zero blocking findings.
    .PARAMETER Index
        The cycle's zero-based position, used for error context.
    .PARAMETER Cycle
        The deserialized cycle object.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [int] $Index,

        [Parameter(Mandatory = $true)]
        [psobject] $Cycle
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # Invariant 1: plan_path must be a non-empty, non-whitespace string.
    $planPath = (Get-CheckpointObjectMember -Owner $Cycle -Name 'plan_path').Value
    if (-not ($planPath -is [string]) -or [string]::IsNullOrWhiteSpace([string]$planPath)) {
        $errors.Add("Checkpoint remediation cycle #$Index plan_path must be a non-empty string.")
    }

    # Invariant 2: an execution status in the blocked set requires the nested
    # preflight to report exactly the cleared status; a missing or non-object
    # preflight cannot satisfy it.
    $executionStatus = (Get-CheckpointObjectMember -Owner $Cycle -Name 'execution_status').Value
    if ($executionStatus -is [string] -and
        ($script:EXECUTION_STATUSES_REQUIRING_CLEAR_PREFLIGHT -contains [string]$executionStatus)) {
        $preflight = (Get-CheckpointObjectMember -Owner $Cycle -Name 'preflight').Value
        $preflightStatus = (Get-CheckpointObjectMember -Owner $preflight -Name 'final_status').Value
        if (-not ($preflightStatus -is [string]) -or ([string]$preflightStatus -ne $script:PREFLIGHT_CLEARED_STATUS)) {
            $errors.Add("Checkpoint remediation cycle #$Index execution_status is $executionStatus but preflight.final_status is not 'clear'.")
        }
    }

    # Invariant 3: a satisfied exit gate requires zero blocking findings. The exit
    # flag must be the boolean True (Python `is True`), not merely truthy.
    $exitConditionMet = (Get-CheckpointObjectMember -Owner $Cycle -Name 'exit_condition_met').Value
    if (($exitConditionMet -is [bool]) -and $exitConditionMet) {
        $blockingCount = (Get-CheckpointObjectMember -Owner $Cycle -Name 'blocking_count').Value
        if (-not (Test-PythonZeroEquivalent -Value $blockingCount)) {
            $errors.Add("Checkpoint remediation cycle #$Index exit_condition_met is true but blocking_count is not 0.")
        }
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateRemediationLoopError {
    <#
    .SYNOPSIS
        Return the remediation_loop errors (inventory rows U6.R1-U6.R4).
    .DESCRIPTION
        Public entry mirroring _validate_remediation_loop. A non-object
        remediation_loop, or a cycles value that is not a list, carries no cycles
        to validate and deliberately yields ZERO errors, matching the Python
        tolerance rather than fabricating a structural error.
    .PARAMETER Value
        The raw deserialized value of the checkpoint's remediation_loop key.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    if (-not (Test-CheckpointObjectValue -Value $Value)) { return $errors.ToArray() }
    $cycles = (Get-CheckpointObjectMember -Owner $Value -Name $script:REMEDIATION_CYCLES_KEY).Value
    if (-not (Test-CheckpointListValue -Value $cycles)) { return $errors.ToArray() }

    # Validate each cycle independently so a malformed entry does not mask the
    # errors of the cycles that follow it.
    $index = 0
    foreach ($cycle in @($cycles)) {
        if (-not (Test-CheckpointObjectValue -Value $cycle)) {
            $errors.Add("Checkpoint remediation cycle #$index must be an object.")
            $index++
            continue
        }
        $errors.AddRange([string[]]@(Get-RemediationCycleError -Index $index -Cycle $cycle))
        $index++
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateHumanInteractionError {
    <#
    .SYNOPSIS
        Return the human_interaction errors (inventory rows U6.H1-U6.H5).
    .DESCRIPTION
        Public entry mirroring _validate_human_interaction: the block must be an
        object carrying a requirements list, each requirement must be an object
        whose response is within the permitted enum, and an exception response
        must carry a non-empty runbook_path. These rows are additive to the
        stricter hook-internal Test-HumanInteractionShape described in the module
        header; they never relax it.
    .PARAMETER Value
        The raw deserialized value of the checkpoint's human_interaction key.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # The key was present, so a non-object value is a malformed block rather than
    # an absent one, and there is nothing further to inspect.
    if (-not (Test-CheckpointObjectValue -Value $Value)) {
        $errors.Add('Checkpoint human_interaction must be an object when present.')
        return $errors.ToArray()
    }

    $requirements = (Get-CheckpointObjectMember -Owner $Value -Name $script:HUMAN_INTERACTION_REQUIREMENTS_KEY).Value
    if (-not (Test-CheckpointListValue -Value $requirements)) {
        $errors.Add('Checkpoint human_interaction.requirements must be a list.')
        return $errors.ToArray()
    }

    # Validate each requirement independently; an out-of-enum response stops that
    # requirement so the runbook rule is not applied to an unknown response.
    $index = 0
    foreach ($requirement in @($requirements)) {
        if (-not (Test-CheckpointObjectValue -Value $requirement)) {
            $errors.Add("Checkpoint human_interaction.requirements #$index must be an object.")
            $index++
            continue
        }

        $response = (Get-CheckpointObjectMember -Owner $requirement -Name 'response').Value
        if (-not ($response -is [string]) -or ($script:HUMAN_INTERACTION_RESPONSE_ENUM -notcontains [string]$response)) {
            $rendered = ConvertTo-PythonDisplayText -Value $response
            $errors.Add("Checkpoint human_interaction.requirements #$index response must be one of scope_change, exception, halt; got: $rendered")
            $index++
            continue
        }

        if ([string]$response -eq $script:HUMAN_INTERACTION_EXCEPTION_RESPONSE) {
            $runbookPath = (Get-CheckpointObjectMember -Owner $requirement -Name 'runbook_path').Value
            if (-not ($runbookPath -is [string]) -or [string]::IsNullOrWhiteSpace([string]$runbookPath)) {
                $errors.Add("Checkpoint human_interaction.requirements #$index response is exception but runbook_path is missing or empty.")
            }
        }

        $index++
    }

    return $errors.ToArray()
}

# The three family entry points are exported for the U-family aggregator; the
# shared primitives stay exported from the sibling OrchestratorStateCheckpointValue
# module so there is exactly one implementation of each.
Export-ModuleMember -Function `
    Get-OrchestratorStateDelegationReceiptError, `
    Get-OrchestratorStateRemediationLoopError, `
    Get-OrchestratorStateHumanInteractionError
