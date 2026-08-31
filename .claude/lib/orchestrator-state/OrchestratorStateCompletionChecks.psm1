<#
.SYNOPSIS
    Portable completion-gate checks C1, C2, C3, C4, C5, and C7.

.DESCRIPTION
    Destination-runtime PowerShell port of the `--require-complete` checks the
    authoritative Python validator performs, excluding the routing contract (C6),
    which lives in the sibling `OrchestratorStateRoutingContract.psm1`. The rows
    ported here are:

      C1.1  completion-blocking step statuses, full five-value blocking set
      C2.1  blocked_reason must be absent, null, or the literal `none`
      C3.1  pr_gate must be an object (route-gated)
      C3.2  pr_gate missing required fields (route-gated)
      C4.1  ci_gate must be an object (route-gated)
      C4.2  ci_gate missing required fields (route-gated)
      C4.3  ci_gate.conclusion must be success (route-gated)
      C4.4  ci_gate.head_sha must match pr_gate.head_sha (route-gated)
      C5.1  mandatory route phases, from a static map
      C7.1  preparation terminal next_step (value-gated, repr rendering)
      C7.2  preparation terminal step statuses (value-gated, repr rendering)

    Route gating for C3 and C4 is resolved through
    `OrchestratorStateRoutingMatrix.psm1`, implementing deviation PD-1: the gate
    decision reads pinned constants and never opens
    `config/orchestration-routing.json`. The gates are deliberately asymmetric,
    matching the Python reference: the PR gate applies only when a route sets
    `requires_pr_gate: true`, while the CI gate applies unless a route explicitly
    sets `requires_ci_gate: false`.

    C5's mandatory-phase map is static in the Python reference too, so it needs no
    matrix lookup. C7 renders both sides with Python `repr()` semantics.

    Every function is pure: it reads no file, starts no process, and never mutates
    its input.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Import the shared checkpoint-value primitives and the pinned routing matrix,
# resolved relative to this module's directory so both imports travel with the
# pushed-down pack regardless of the working directory.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateRoutingMatrix.psm1') -Force -ErrorAction Stop

# The six step-status keys, in report order.
$script:STEP_STATUS_KEYS = @(
    'step5_status',
    'step6_status',
    'step7_status',
    'step8_status',
    'step9_status',
    'step10_status'
)

# Step statuses that must never appear in a checkpoint written as DONE. The
# documented S9 success value `passed` is deliberately absent: it records CI green
# and must not block completion. Pinned to COMPLETION_BLOCKING_STEP_STATUS in
# scripts/dev_tools/_orchestrator_state_step_status.py.
$script:COMPLETION_BLOCKING_STEP_STATUS = @(
    'pending',
    'blocked',
    'failed_remediation_required',
    'blocked_ci_loop_limit',
    'blocked_remediation_loop_limit'
)

# The gate object key sets, in the order the error messages join them.
$script:PR_GATE_KEYS = @('pr_number', 'pr_url', 'head_branch', 'head_sha')
$script:CI_GATE_KEYS = @('conclusion', 'head_sha', 'verified_at')

# The mandatory canonical phases per route. Static in the Python reference, so no
# matrix lookup is involved; a route absent from this map imposes no requirement.
$script:MANDATORY_ROUTE_PHASES = @{
    small       = @('S3_promotion', 'S4_atomic_planning')
    preparation = @('S3_promotion', 'S4_atomic_planning')
}

# The preparation-route terminal contract: the exact required next_step and the
# six step keys that must read not-applicable.
$script:PREPARATION_ROUTE_ID = 'preparation'
$script:PREPARATION_EXPECTED_NEXT_STEP = 'S5_atomic_execution'
$script:PREPARATION_NOT_APPLICABLE = 'not-applicable'


function Get-MissingGateKey {
    <#
    .SYNOPSIS
        Return the gate keys absent or blank in a gate object.
    .DESCRIPTION
        Private helper mirroring _missing_pr_gate_keys / _missing_object_keys. A
        non-object value reports every key as missing; a present key holding null
        or a blank string is also missing, so a placeholder cannot satisfy a gate.
    .PARAMETER Value
        The candidate gate value from the checkpoint. May be $null.
    .PARAMETER GateKey
        The required key names, in message order.
    .OUTPUTS
        System.String[] - the missing key names, in the given order.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value,

        [Parameter(Mandatory = $true)]
        [string[]] $GateKey
    )

    if (-not (Test-CheckpointObjectValue -Value $Value)) { return [string[]]@($GateKey) }

    $missing = [System.Collections.Generic.List[string]]::new()
    foreach ($key in $GateKey) {
        $item = (Get-CheckpointObjectMember -Owner $Value -Name $key).Value
        if ($null -eq $item -or (($item -is [string]) -and [string]::IsNullOrWhiteSpace([string]$item))) {
            $missing.Add($key)
        }
    }
    return $missing.ToArray()
}

function Get-OrchestratorStateCompletionStepStatusError {
    <#
    .SYNOPSIS
        Return the completion-blocking step-status errors (inventory row C1.1).
    .DESCRIPTION
        Mirrors collect_completion_blocking_step_errors. One error per step key
        whose recorded value is in the five-value blocking set, in step-key order.
        The check is applied across all six step keys because the failure values
        are per-key-valid only.
    .PARAMETER State
        The parsed checkpoint object.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # Report in step-key order so the operator reads failures in pipeline order.
    foreach ($key in $script:STEP_STATUS_KEYS) {
        $value = (Get-CheckpointObjectMember -Owner $State -Name $key).Value
        if (($value -is [string]) -and ($script:COMPLETION_BLOCKING_STEP_STATUS -ccontains [string]$value)) {
            $errors.Add("Checkpoint completion validation failed: $key is $value.")
        }
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateCompletionBlockedReasonError {
    <#
    .SYNOPSIS
        Return the completion blocked_reason error (inventory row C2.1).
    .DESCRIPTION
        Mirrors the `state.get("blocked_reason") not in {None, "none"}` guard.
        Absent, null, and the literal `none` all satisfy the gate; every other
        recorded reason blocks completion. The message quotes `none` with
        backticks, exactly as the Python reference does.
    .PARAMETER State
        The parsed checkpoint object.
    .OUTPUTS
        System.String[] - zero or one error string.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    $value = (Get-CheckpointObjectMember -Owner $State -Name 'blocked_reason').Value
    if ($null -ne $value -and -not (($value -is [string]) -and ([string]$value -ceq 'none'))) {
        $errors.Add('Checkpoint completion validation failed: blocked_reason is not `none`.')
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateCompletionPrGateError {
    <#
    .SYNOPSIS
        Return the completion PR-gate errors (inventory rows C3.1-C3.2).
    .DESCRIPTION
        Mirrors validate_completion_pr_gate. The gate applies only to routes whose
        pinned `requires_pr_gate` is true; every other route contributes no
        pr_gate errors. A non-object pr_gate reports the object-shape error alone,
        and an object with absent or blank required fields names them.
    .PARAMETER State
        The parsed checkpoint object.
    .PARAMETER RoutingMatrix
        Optional matrix override forwarded to the route lookup.
    .OUTPUTS
        System.String[] - zero or one error string.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [hashtable] $RoutingMatrix = $null
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    $routeId = Get-OrchestratorStateSelectedRouteId -State $State
    if (-not (Test-OrchestratorStateRouteRequiresPrGate -RouteId $routeId -RoutingMatrix $RoutingMatrix)) {
        return $errors.ToArray()
    }

    $prGate = (Get-CheckpointObjectMember -Owner $State -Name 'pr_gate').Value
    $missing = @(Get-MissingGateKey -Value $prGate -GateKey $script:PR_GATE_KEYS)

    # A non-object pr_gate reports only the shape error; the field list would be
    # every key and adds nothing.
    if (-not (Test-CheckpointObjectValue -Value $prGate)) {
        $errors.Add("Checkpoint completion validation failed: pr_gate must be an object with keys: $($script:PR_GATE_KEYS -join ', ').")
        return $errors.ToArray()
    }
    if ($missing.Count -gt 0) {
        $errors.Add("Checkpoint completion validation failed: pr_gate missing required fields: $($missing -join ', ').")
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateCompletionCiGateError {
    <#
    .SYNOPSIS
        Return the completion CI-gate errors (inventory rows C4.1-C4.4).
    .DESCRIPTION
        Mirrors _validate_completion_ci_gate plus its route gate. The gate applies
        unless the route's pinned `requires_ci_gate` is exactly false, so an
        absent flag keeps the gate on. A non-object ci_gate reports the shape
        error alone; otherwise the missing-field, success-conclusion, and
        head_sha-match rules each contribute independently.
    .PARAMETER State
        The parsed checkpoint object.
    .PARAMETER RoutingMatrix
        Optional matrix override forwarded to the route lookup.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [hashtable] $RoutingMatrix = $null
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    $routeId = Get-OrchestratorStateSelectedRouteId -State $State
    if (-not (Test-OrchestratorStateRouteRequiresCiGate -RouteId $routeId -RoutingMatrix $RoutingMatrix)) {
        return $errors.ToArray()
    }

    $ciGate = (Get-CheckpointObjectMember -Owner $State -Name 'ci_gate').Value
    $missing = @(Get-MissingGateKey -Value $ciGate -GateKey $script:CI_GATE_KEYS)

    if (-not (Test-CheckpointObjectValue -Value $ciGate)) {
        $errors.Add("Checkpoint completion validation failed: ci_gate must be an object with keys: $($script:CI_GATE_KEYS -join ', ').")
        return $errors.ToArray()
    }
    if ($missing.Count -gt 0) {
        $errors.Add("Checkpoint completion validation failed: ci_gate missing required fields: $($missing -join ', ').")
    }

    $conclusion = (Get-CheckpointObjectMember -Owner $ciGate -Name 'conclusion').Value
    if (-not (Test-PythonValueEqual -Actual $conclusion -Expected 'success')) {
        $errors.Add('Checkpoint completion validation failed: ci_gate.conclusion must be success.')
    }

    # The head_sha match runs only when a pr_gate object records a non-null
    # head_sha; without one there is nothing to match against.
    $prGate = (Get-CheckpointObjectMember -Owner $State -Name 'pr_gate').Value
    $prHeadSha = $null
    if (Test-CheckpointObjectValue -Value $prGate) {
        $prHeadSha = (Get-CheckpointObjectMember -Owner $prGate -Name 'head_sha').Value
    }
    if ($null -ne $prHeadSha) {
        $ciHeadSha = (Get-CheckpointObjectMember -Owner $ciGate -Name 'head_sha').Value
        if (-not (Test-PythonValueEqual -Actual $ciHeadSha -Expected $prHeadSha)) {
            $errors.Add('Checkpoint completion validation failed: ci_gate.head_sha must match pr_gate.head_sha.')
        }
    }

    return $errors.ToArray()
}

function Get-OrchestratorStatePhaseCompletenessError {
    <#
    .SYNOPSIS
        Return the mandatory-phase errors for the selected route (row C5.1).
    .DESCRIPTION
        Mirrors validate_phase_completeness. The mandatory-phase set is read from
        a static map, not from the routing matrix, so no matrix parameter exists.
        A route absent from the map, or an unusable route id, imposes no
        requirement. A `completed_steps` value that is not a list of non-blank
        strings is treated as recording no completed phases.
    .PARAMETER State
        The parsed checkpoint object.
    .OUTPUTS
        System.String[] - one error per missing mandatory phase.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    $routeId = Get-OrchestratorStateSelectedRouteId -State $State
    if ($null -eq $routeId -or -not $script:MANDATORY_ROUTE_PHASES.ContainsKey($routeId)) {
        return $errors.ToArray()
    }

    # A malformed completed_steps records no phases, so every mandatory phase is
    # reported missing rather than the malformed shape being reported separately.
    $completed = (Get-CheckpointObjectMember -Owner $State -Name 'completed_steps').Value
    $present = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    if (Test-CheckpointListValue -Value $completed) {
        $wellFormed = $true
        foreach ($item in @($completed)) {
            if (-not ($item -is [string]) -or [string]::IsNullOrWhiteSpace([string]$item)) {
                $wellFormed = $false
                break
            }
        }
        if ($wellFormed) {
            foreach ($item in @($completed)) { [void]$present.Add([string]$item) }
        }
    }

    foreach ($phase in $script:MANDATORY_ROUTE_PHASES[$routeId]) {
        if (-not $present.Contains($phase)) {
            $errors.Add("Checkpoint completion validation failed: route $routeId is missing mandatory phase $phase.")
        }
    }

    return $errors.ToArray()
}

function Get-OrchestratorStatePreparationTerminalError {
    <#
    .SYNOPSIS
        Return the preparation terminal-contract errors (rows C7.1-C7.2).
    .DESCRIPTION
        Mirrors validate_preparation_terminal_contract. The check is value-gated
        on the RAW route value being exactly the string `preparation`, so a
        checkpoint on any other route contributes nothing. Both messages render
        the offending value with Python repr() semantics.
    .PARAMETER State
        The parsed checkpoint object.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    $routeValue = Get-OrchestratorStateRawRouteValue -State $State
    if (-not (Test-PythonValueEqual -Actual $routeValue -Expected $script:PREPARATION_ROUTE_ID)) {
        return $errors.ToArray()
    }

    $nextStep = (Get-CheckpointObjectMember -Owner $State -Name 'next_step').Value
    if (-not (Test-PythonValueEqual -Actual $nextStep -Expected $script:PREPARATION_EXPECTED_NEXT_STEP)) {
        $expectedText = ConvertTo-PythonReprText -Value $script:PREPARATION_EXPECTED_NEXT_STEP
        $errors.Add("Preparation checkpoint next_step must be $expectedText, found $(ConvertTo-PythonReprText -Value $nextStep).")
    }

    # A preparation run performs no execution steps, so all six step keys must
    # read not-applicable; each deviation is reported with its own key.
    foreach ($key in $script:STEP_STATUS_KEYS) {
        $value = (Get-CheckpointObjectMember -Owner $State -Name $key).Value
        if (-not (Test-PythonValueEqual -Actual $value -Expected $script:PREPARATION_NOT_APPLICABLE)) {
            $errors.Add("Preparation checkpoint $key must be 'not-applicable', found $(ConvertTo-PythonReprText -Value $value).")
        }
    }

    return $errors.ToArray()
}

# Each check family is exported individually so the completion entry point can
# compose them in the Python reference's order; the gate-key helper stays private.
Export-ModuleMember -Function `
    Get-OrchestratorStateCompletionStepStatusError, `
    Get-OrchestratorStateCompletionBlockedReasonError, `
    Get-OrchestratorStateCompletionPrGateError, `
    Get-OrchestratorStateCompletionCiGateError, `
    Get-OrchestratorStatePhaseCompletenessError, `
    Get-OrchestratorStatePreparationTerminalError
