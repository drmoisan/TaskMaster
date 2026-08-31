<#
.SYNOPSIS
    Single entry point for the orchestrator-state unconditional check block.

.DESCRIPTION
    Composes the whole U family of the issue #475 parity inventory into one
    portable call, mirroring the unconditional block of
    `validate_orchestrator_state_text` in
    `scripts/dev_tools/validate_orchestrator_state.py`:

      U2-U4  required keys, step-status validity, blocked_reason validity, from
             the existing `Get-OrchestratorStateBasePresenceError` in
             `OrchestratorState.psm1`
      U5     delegation_receipts shape, from `OrchestratorStateReceipts.psm1`
      U6.R   remediation_loop cycles, same module
      U6.H   human_interaction shape, same module
      U6.C   complexity_assessments per-entry, from `OrchestratorStateModelReceipts.psm1`
      U6.M   model_routing_receipts per-entry, same module
      U6.X   codex_model_routing_receipts, from `OrchestratorStateCodexModelReceipts.psm1`
      U6.T   codex_topology_receipts, from `OrchestratorStateCodexTopologyReceipts.psm1`

    U1 (parse failure and non-object root) is the LOADER's contract, produced by
    `Get-OrchestratorStateCheckpoint` in `OrchestratorState.psm1`. Every caller
    runs the loader first and fails closed on its error before reaching this
    function, so the loader and this function together are the complete U family.
    The loader's path-prefixed message text is the one documented parity
    divergence from the Python strings, recorded in the feature spec.

    KEY-GATED SEMANTICS ARE PRESERVED. Each optional-key family runs only when
    its key is PRESENT on the checkpoint, exactly as the Python
    `optional_key_validators` loop does. An absent key contributes zero errors and
    never produces a "must be a list when present" message. The distinction
    matters: a present key holding null is validated, an absent key is not.

    Families run in the Python reference's order so accumulated error output is
    ordered identically.

    The function is pure: it reads no file, starts no process, and never mutates
    its input.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Import the four leaf check modules eagerly. None of them imports this module,
# so this import graph has no cycle. OrchestratorState.psm1 is deliberately NOT
# imported here; it is loaded lazily inside the function, because the preflight
# path in that module imports this one and an eager import in both directions
# would couple their load order.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateReceipts.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateModelReceipts.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCodexModelReceipts.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCodexTopologyReceipts.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force -ErrorAction Stop

# The checkpoint key whose value carries the delegation receipts. It is read with
# the Python `is not None` guard rather than a presence guard, matching the
# reference.
$script:DELEGATION_RECEIPTS_KEY = 'delegation_receipts'

# The optional-key families, in the Python reference's evaluation order. The
# dispatch below routes each present key to its validator by name; the list is
# declared here so the order is stated once and is visible at a glance.
$script:OPTIONAL_KEYS = @(
    'remediation_loop',
    'human_interaction',
    'complexity_assessments',
    'model_routing_receipts',
    'codex_model_routing_receipts',
    'codex_topology_receipts'
)


function Import-OrchestratorStateBaseModule {
    <#
    .SYNOPSIS
        Ensure the shared base-presence command is available, importing it lazily.
    .DESCRIPTION
        Private helper following the guarded lazy-import pattern used by
        .claude/hooks/validate-orchestrator-output.ps1. The base module is
        imported only when its command is not already resolvable, so this module
        can be loaded from inside that module's own call path without an
        eager two-way import.
    .OUTPUTS
        None.
    #>
    [CmdletBinding()]
    [OutputType([void])]
    param()

    if (Get-Command -Name 'Get-OrchestratorStateBasePresenceError' -ErrorAction SilentlyContinue) { return }
    Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorState.psm1')
}

function Get-OrchestratorStateUnconditionalError {
    <#
    .SYNOPSIS
        Return every unconditional-block error for a parsed checkpoint.
    .DESCRIPTION
        The single U-family entry point. Runs the base-presence checks (U2-U4),
        the delegation-receipt shape checks (U5), and each optional-key family
        (U6.R, U6.H, U6.C, U6.M, U6.X, U6.T) in the Python reference's order.
        Every optional family is key-gated: it runs only when its key is present
        on the checkpoint, so an absent key contributes zero errors.

        U1 is not produced here. Parse failure and a non-object root are the
        loader's contract; callers run Get-OrchestratorStateCheckpoint first and
        fail closed on its error before reaching this function.
    .PARAMETER State
        The parsed checkpoint object, as returned by the loader.
    .OUTPUTS
        System.String[] - zero or more error strings, in Python reference order.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    Import-OrchestratorStateBaseModule

    $errors = [System.Collections.Generic.List[string]]::new()

    # U2-U4: required keys, step-status validity, blocked_reason validity.
    $errors.AddRange([string[]]@(Get-OrchestratorStateBasePresenceError -State $State))

    # U5: the delegation-receipt shape, guarded on a non-null value rather than
    # key presence, matching the Python `receipts is not None` test.
    $receipts = (Get-CheckpointObjectMember -Owner $State -Name $script:DELEGATION_RECEIPTS_KEY).Value
    $errors.AddRange([string[]]@(Get-OrchestratorStateDelegationReceiptError -Value $receipts))

    # U6: each optional-key family, in reference order, key-gated so an absent key
    # never produces a "must be a list when present" message. The dispatch names
    # every validator explicitly rather than invoking a command stored in a
    # variable, so the enforcement-hook AST guard sees only constant command
    # names and no dynamic invocation.
    foreach ($key in $script:OPTIONAL_KEYS) {
        $field = Get-CheckpointObjectMember -Owner $State -Name $key
        if (-not $field.Present) { continue }
        switch ($key) {
            'remediation_loop' {
                $errors.AddRange([string[]]@(Get-OrchestratorStateRemediationLoopError -Value $field.Value))
            }
            'human_interaction' {
                $errors.AddRange([string[]]@(Get-OrchestratorStateHumanInteractionError -Value $field.Value))
            }
            'complexity_assessments' {
                $errors.AddRange([string[]]@(Get-OrchestratorStateComplexityAssessmentError -Value $field.Value))
            }
            'model_routing_receipts' {
                $errors.AddRange([string[]]@(Get-OrchestratorStateModelRoutingReceiptError -Value $field.Value))
            }
            'codex_model_routing_receipts' {
                $errors.AddRange([string[]]@(Get-OrchestratorStateCodexModelRoutingReceiptError -Value $field.Value))
            }
            'codex_topology_receipts' {
                $errors.AddRange([string[]]@(Get-OrchestratorStateCodexTopologyReceiptError -Value $field.Value))
            }
        }
    }

    return $errors.ToArray()
}

# Only the aggregate entry point is exported; the lazy-import helper is private.
Export-ModuleMember -Function Get-OrchestratorStateUnconditionalError
