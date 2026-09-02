<#
.SYNOPSIS
    Complete-parity completion-gate validation for the orchestrator-state checkpoint.

.DESCRIPTION
    Provides the destination-runtime PowerShell implementation of the completion
    validation the pushed-down validate-orchestrator-output hook performs. As of
    issue #475 this is a COMPLETE-PARITY port of the Python validator's call
    surface `orchestrator-state --require-complete --require-model-routing`,
    measured row by row against the parity inventory. It is no longer a
    presence-level subset and no longer a fallback for an importable Python
    validator: the portable path is the only path.

    `Test-OrchestratorStateCompletionReadiness` composes, in the Python
    reference's order:

      U1        the loader contract (missing file, invalid JSON, non-object root),
                fail-closed, from `Get-OrchestratorStateCheckpoint`
      U2-U6     the whole unconditional block, from
                `Get-OrchestratorStateUnconditionalError`
      C1,C2     completion step statuses and blocked_reason
      C3,C4     the route-gated pr_gate and ci_gate contracts
      C5        mandatory route phases
      C6        the routing contract, from `OrchestratorStateRoutingContract.psm1`
      C7        the preparation terminal contract
      M1        model-routing receipt required once delegated
      M2        a complexity assessment for every matched receipt phase
      M3        per-entry re-validation of the U6.C and U6.M families

    PD-2 SINGLE EMISSION (declared divergence). The Python reference emits the
    U6.C and U6.M per-entry errors TWICE for this flag pair: once in the
    unconditional block and again inside the model-routing gate's M3 re-run. This
    port emits each error string exactly once. The M3 reuse requirement is
    satisfied by INVOKING the same per-entry validator implementation
    (`Get-OrchestratorStateComplexityAssessmentError` and
    `Get-OrchestratorStateModelRoutingReceiptError` from
    `OrchestratorStateModelReceipts.psm1`) inside the gate, exactly as the Python
    gate reuses its validators, and then adding only strings the accumulated
    result does not already carry. Reuse is therefore real, and duplication is
    not. The divergence is deliberate: a hook that counted errors would behave
    differently against a duplicating validator.

    The `MODEL_ROUTING_BLOCKED` routing guarantee is preserved: a missing routing
    receipt still yields error text containing the literal token
    `model_routing_receipts`, so the completion hook maps the failure to its
    `MODEL_ROUTING_BLOCKED:` block reason.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Import the sibling shared module, the portable model-routing formulas, and the
# ported check families, resolved relative to this module's directory so every
# import travels with the pushed-down pack regardless of the working directory.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorState.psm1') -Force -ErrorAction Stop
$script:ModelRoutingModulePath = Join-Path -Path (Join-Path -Path $PSScriptRoot -ChildPath '..') -ChildPath (Join-Path -Path 'model-routing' -ChildPath 'ModelRouting.psm1')
Import-Module $script:ModelRoutingModulePath -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateModelReceipts.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateUnconditional.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCompletionChecks.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateRoutingContract.psm1') -Force -ErrorAction Stop

# The two optional keys the M3 leg re-validates, guarded on key presence so an
# absent key does not emit a spurious "must be a list when present" error.
$script:COMPLEXITY_ASSESSMENTS_KEY = 'complexity_assessments'

# The subagent types delegated via the Agent tool that can be named by a delegating
# next_step. Pinned to _DELEGATING_AGENTS in
# scripts/dev_tools/_orchestrator_state_model_routing_gate.py. The `orchestrator`
# type is deliberately excluded: it is the caller, never a routing-receipt target.
$script:DELEGATING_AGENTS = @(
    'atomic-planner',
    'atomic-executor',
    'feature-review',
    'task-researcher',
    'prd-feature',
    'pr-author'
)

# Checkpoint keys the gate reads to derive the delegated-agent and receipt-agent sets.
$script:DELEGATION_RECEIPTS_KEY = 'delegation_receipts'
$script:MODEL_ROUTING_RECEIPTS_KEY = 'model_routing_receipts'
$script:NEXT_STEP_KEY = 'next_step'


function Get-OrchestratorStateDelegatedAgent {
    <#
    .SYNOPSIS
        Derive the set of agents a checkpoint has delegated (or is about to).
    .DESCRIPTION
        Private helper mirroring ``_delegated_agents`` in the Python gate. Collects
        each well-formed ``delegation_receipts[]`` entry's non-empty ``agent_name``
        plus the agent implied by a ``next_step`` that names a recognized delegating
        agent. The list form of ``delegation_receipts`` is the authoritative "a
        delegation happened" record; the namespaced (promotion) object form carries
        no agent_name list and contributes no delegated agents.
    .PARAMETER State
        The parsed checkpoint PSCustomObject.
    .OUTPUTS
        System.String[] - the delegated-agent names (may be empty).
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $agents = [System.Collections.Generic.HashSet[string]]::new()

    # Collect each list-form delegation receipt's non-empty agent_name. A non-list
    # (namespaced) delegation_receipts value contributes nothing here.
    $receiptsField = Get-OrchestratorStateField -State $State -Name $script:DELEGATION_RECEIPTS_KEY
    if ($receiptsField.Present -and ($receiptsField.Value -is [System.Array])) {
        foreach ($receipt in $receiptsField.Value) {
            if ($receipt -is [System.Management.Automation.PSCustomObject]) {
                $nameField = Get-OrchestratorStateField -State $receipt -Name 'agent_name'
                if ($nameField.Present -and $null -ne $nameField.Value -and
                    -not [string]::IsNullOrWhiteSpace([string]$nameField.Value)) {
                    [void]$agents.Add([string]$nameField.Value)
                }
            }
        }
    }

    # A delegating next_step names the upcoming delegation that may not yet have a
    # receipt; include it only when it matches a recognized delegating agent so a
    # non-delegating label (for example "complete") never triggers the gate.
    $nextStepField = Get-OrchestratorStateField -State $State -Name $script:NEXT_STEP_KEY
    if ($nextStepField.Present -and $null -ne $nextStepField.Value -and
        ($script:DELEGATING_AGENTS -contains [string]$nextStepField.Value)) {
        [void]$agents.Add([string]$nextStepField.Value)
    }

    return [string[]]@($agents)
}

function Get-OrchestratorStateRoutingReceiptAgent {
    <#
    .SYNOPSIS
        Collect the set of agents that carry a model-routing receipt.
    .DESCRIPTION
        Private helper mirroring the receipt-agent harvest in the Python gate. Reads
        the checkpoint's ``model_routing_receipts[]`` array and returns the set of
        non-empty ``agent`` values present on well-formed receipt objects. A non-list
        value contributes no agents (the existence gate then reports every delegated
        agent as unreceipted, preserving fail-closed semantics).
    .PARAMETER State
        The parsed checkpoint PSCustomObject.
    .OUTPUTS
        System.String[] - the receipt-agent names (may be empty).
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $agents = [System.Collections.Generic.HashSet[string]]::new()

    $receiptsField = Get-OrchestratorStateField -State $State -Name $script:MODEL_ROUTING_RECEIPTS_KEY
    if ($receiptsField.Present -and ($receiptsField.Value -is [System.Array])) {
        # Record each well-formed receipt's non-empty agent so the existence gate can
        # test the delegated-agent set against it.
        foreach ($receipt in $receiptsField.Value) {
            if ($receipt -is [System.Management.Automation.PSCustomObject]) {
                $agentField = Get-OrchestratorStateField -State $receipt -Name 'agent'
                if ($agentField.Present -and $null -ne $agentField.Value -and
                    -not [string]::IsNullOrWhiteSpace([string]$agentField.Value)) {
                    [void]$agents.Add([string]$agentField.Value)
                }
            }
        }
    }

    return [string[]]@($agents)
}

function Get-OrchestratorStateModelRoutingGateError {
    <#
    .SYNOPSIS
        Return the required-once-delegated existence-gate errors.
    .DESCRIPTION
        Private gate mirroring ``validate_model_routing_gate`` at the presence level:
        it fires only when the checkpoint has delegated (or is about to delegate to)
        at least one agent, then reports one error per delegated agent that lacks a
        matching ``model_routing_receipts[]`` entry. A delegation-free checkpoint
        contributes zero errors, preserving backward compatibility. Each error names
        the literal token ``model_routing_receipts`` so the completion hook routes a
        failure to ``MODEL_ROUTING_BLOCKED:``.
    .PARAMETER State
        The parsed checkpoint PSCustomObject.
    .OUTPUTS
        System.String[] - zero or more error strings; empty when the gate is satisfied
        or does not fire.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # Backward-compat gate: a delegation-free checkpoint imposes no routing-receipt
    # requirement, so return early with no errors.
    $delegated = @(Get-OrchestratorStateDelegatedAgent -State $State)
    if ($delegated.Count -eq 0) {
        return $errors.ToArray()
    }

    $receiptAgents = @(Get-OrchestratorStateRoutingReceiptAgent -State $State)

    # M1: the routing-receipt agent set must be a superset of the delegated-agent
    # set. Report each delegated agent with no receipt, sorted for deterministic
    # error ordering.
    $missing = $delegated | Where-Object { $receiptAgents -notcontains $_ } | Sort-Object
    foreach ($agent in $missing) {
        $errors.Add("Checkpoint model_routing_receipts is missing a receipt for delegated agent: $agent.")
    }

    # M2: every phase named by a receipt that MATCHED a delegated agent must carry
    # a complexity assessment. Only matched receipts impose the requirement, so an
    # unrelated receipt cannot force an assessment.
    $matchedPhases = @(Get-OrchestratorStateMatchedReceiptPhase -State $State -DelegatedAgent $delegated)
    $assessedPhases = @(Get-OrchestratorStateAssessedPhase -State $State)
    $unassessed = $matchedPhases | Where-Object { $assessedPhases -cnotcontains $_ } | Sort-Object
    foreach ($phase in $unassessed) {
        $errors.Add("Checkpoint complexity_assessments is missing an entry for phase $phase referenced by a model_routing_receipts entry.")
    }

    # M3: re-validate the U6.M and U6.C families by INVOKING the same per-entry
    # validator implementation the unconditional block uses, exactly as the Python
    # gate reuses its validators. Both calls are key-gated so an absent key does
    # not emit a spurious "must be a list when present" error. Emission is left to
    # the caller, which applies the PD-2 single-emission rule.
    $routingField = Get-CheckpointObjectMember -Owner $State -Name $script:MODEL_ROUTING_RECEIPTS_KEY
    if ($routingField.Present) {
        $errors.AddRange([string[]]@(Get-OrchestratorStateModelRoutingReceiptError -Value $routingField.Value))
    }
    $complexityField = Get-CheckpointObjectMember -Owner $State -Name $script:COMPLEXITY_ASSESSMENTS_KEY
    if ($complexityField.Present) {
        $errors.AddRange([string[]]@(Get-OrchestratorStateComplexityAssessmentError -Value $complexityField.Value))
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateMatchedReceiptPhase {
    <#
    .SYNOPSIS
        Collect the phases named by routing receipts that matched a delegated agent.
    .DESCRIPTION
        Private helper mirroring the matched-phase half of
        ``_routing_receipt_agents_and_matched_phases``. Only a receipt whose
        ``agent`` is in the delegated set contributes its ``phase``, so an
        unrelated receipt cannot force a complexity assessment.
    .PARAMETER State
        The parsed checkpoint object.
    .PARAMETER DelegatedAgent
        The delegated-agent names.
    .OUTPUTS
        System.String[] - the matched phases, rendered with Python str() semantics
        so a non-string phase still yields a stable, comparable key.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]] $DelegatedAgent
    )

    $phases = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $receipts = (Get-CheckpointObjectMember -Owner $State -Name $script:MODEL_ROUTING_RECEIPTS_KEY).Value
    if (-not (Test-CheckpointListValue -Value $receipts)) { return [string[]]@($phases) }

    foreach ($receipt in @($receipts)) {
        if (-not (Test-CheckpointObjectValue -Value $receipt)) { continue }
        $agent = (Get-CheckpointObjectMember -Owner $receipt -Name 'agent').Value
        if (($agent -is [string]) -and ($DelegatedAgent -ccontains [string]$agent)) {
            $phase = (Get-CheckpointObjectMember -Owner $receipt -Name 'phase').Value
            [void]$phases.Add((ConvertTo-PythonDisplayText -Value $phase))
        }
    }
    return [string[]]@($phases)
}

function Get-OrchestratorStateAssessedPhase {
    <#
    .SYNOPSIS
        Collect the phases that carry a complexity-assessment entry.
    .DESCRIPTION
        Private helper mirroring ``_assessed_phases``. Every well-formed
        assessment object contributes its ``phase`` value, including a null one,
        so a receipt phase paired with an assessment is not reported missing.
    .PARAMETER State
        The parsed checkpoint object.
    .OUTPUTS
        System.String[] - the assessed phases, rendered with Python str()
        semantics to match the matched-phase keys.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $phases = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $assessments = (Get-CheckpointObjectMember -Owner $State -Name $script:COMPLEXITY_ASSESSMENTS_KEY).Value
    if (-not (Test-CheckpointListValue -Value $assessments)) { return [string[]]@($phases) }

    foreach ($assessment in @($assessments)) {
        if (-not (Test-CheckpointObjectValue -Value $assessment)) { continue }
        $phase = (Get-CheckpointObjectMember -Owner $assessment -Name 'phase').Value
        [void]$phases.Add((ConvertTo-PythonDisplayText -Value $phase))
    }
    return [string[]]@($phases)
}

function Add-OrchestratorStateErrorOnce {
    <#
    .SYNOPSIS
        Append error strings the accumulated list does not already carry.
    .DESCRIPTION
        Private helper implementing the PD-2 single-emission rule. The M3 leg
        deliberately re-invokes the U6.C and U6.M per-entry validators, which the
        unconditional block already ran, so its output overlaps. Appending only
        the strings not already present keeps the reuse real while emitting each
        error exactly once, the declared divergence from the Python reference's
        duplicate emission.
    .PARAMETER Accumulated
        The accumulated error list, appended in place.
    .PARAMETER Candidate
        The candidate error strings.
    .OUTPUTS
        None. The Accumulated list is appended in place.
    #>
    [CmdletBinding()]
    [OutputType([void])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[string]] $Accumulated,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]] $Candidate
    )

    foreach ($item in $Candidate) {
        if (-not $Accumulated.Contains($item)) { $Accumulated.Add($item) }
    }
}

function Test-OrchestratorStateCompletionReadiness {
    <#
    .SYNOPSIS
        Validate a checkpoint against the complete-parity completion contract.
    .DESCRIPTION
        Public entry point used by the pushed-down validate-orchestrator-output
        hook. Loads the checkpoint (fail-closed on missing file, invalid JSON, or
        non-object root), then runs the whole unconditional block, the C family,
        and the M family in the Python reference's order. Returns a hashtable
        compatible with the hook's invoker contract: ExitCode is 1 whenever any
        error is present, and Output carries the newline-joined error text (empty
        on success).

        PD-2 single emission applies to the M3 leg only: the per-entry U6.C and
        U6.M validators are invoked again there, as the Python gate does, but a
        string already emitted by the unconditional block is not repeated. Every
        other check contributes its errors directly.

        A missing routing receipt yields error text containing
        ``model_routing_receipts`` so the hook surfaces it under
        ``MODEL_ROUTING_BLOCKED:``.
    .PARAMETER CheckpointPath
        The path to the orchestrator-state checkpoint JSON file.
    .OUTPUTS
        System.Collections.Hashtable with keys ExitCode (int, 0 or 1) and Output (string).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $CheckpointPath
    )

    # U1: fail closed when the checkpoint cannot be loaded. The load error is the
    # whole output and ExitCode is 1.
    $loaded = Get-OrchestratorStateCheckpoint -CheckpointPath $CheckpointPath
    if (-not $loaded.Ok) {
        return @{ ExitCode = 1; Output = $loaded.Error }
    }
    $state = $loaded.State

    $errors = [System.Collections.Generic.List[string]]::new()

    # U2 through U6: the whole unconditional block, in reference order.
    $errors.AddRange([string[]]@(Get-OrchestratorStateUnconditionalError -State $state))

    # C family, in the reference's require_complete order: step statuses,
    # blocked_reason, pr_gate, ci_gate, phase completeness, routing contract, and
    # the preparation terminal contract.
    $errors.AddRange([string[]]@(Get-OrchestratorStateCompletionStepStatusError -State $state))
    $errors.AddRange([string[]]@(Get-OrchestratorStateCompletionBlockedReasonError -State $state))
    $errors.AddRange([string[]]@(Get-OrchestratorStateCompletionPrGateError -State $state))
    $errors.AddRange([string[]]@(Get-OrchestratorStateCompletionCiGateError -State $state))
    $errors.AddRange([string[]]@(Get-OrchestratorStatePhaseCompletenessError -State $state))
    $errors.AddRange([string[]]@(Get-OrchestratorStateRoutingContractError -State $state))
    $errors.AddRange([string[]]@(Get-OrchestratorStatePreparationTerminalError -State $state))

    # M family. The gate's M3 leg re-invokes the U6.C and U6.M validators, so its
    # output is merged under the PD-2 single-emission rule rather than appended
    # wholesale.
    Add-OrchestratorStateErrorOnce -Accumulated $errors `
        -Candidate ([string[]]@(Get-OrchestratorStateModelRoutingGateError -State $state))

    if ($errors.Count -gt 0) {
        return @{ ExitCode = 1; Output = ($errors -join [System.Environment]::NewLine) }
    }

    return @{ ExitCode = 0; Output = '' }
}

Export-ModuleMember -Function Test-OrchestratorStateCompletionReadiness
