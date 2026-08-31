<#
.SYNOPSIS
    Portable codex_model_routing_receipts per-entry checks (inventory family U6.X).

.DESCRIPTION
    Destination-runtime PowerShell port of
    `scripts/dev_tools/_orchestrator_state_codex_model_routing.py`, covering
    parity-inventory rows U6.X1 through U6.X11: the list and object shape, the ten
    required keys, the non-empty phase, the resolver-invalid-inputs surface, the
    ceiling monotonicity rule, the three ceiling-transition rules, and the
    resolved-key comparison.

    SINGLE-IMPLEMENTATION RULE. The expected deployment is obtained by calling
    `Resolve-CodexDeployment` from `.claude/lib/codex-routing/CodexDeployment.psm1`.
    The profile table, the C3 overlay rule, and the forced-persona rule are never
    re-implemented here.

    Row U6.X11 renders both sides of a mismatch with Python `repr()` semantics
    (`{expected!r}` / `{actual!r}`), so it uses the shared `ConvertTo-PythonReprText`
    renderer. Row U6.X5 interpolates the resolver's exception text with Python
    `str()` semantics, so the resolver's ArgumentException Message is used verbatim.

    Every function is pure: it reads no file, starts no process, and never mutates
    its input.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Import the shared checkpoint-value primitives and the single Codex deployment
# resolver, resolved relative to this module's directory so both imports travel
# with the pushed-down pack regardless of the working directory.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force -ErrorAction Stop
$script:CodexDeploymentModulePath = Join-Path -Path (Join-Path -Path $PSScriptRoot -ChildPath '..') -ChildPath (Join-Path -Path 'codex-routing' -ChildPath 'CodexDeployment.psm1')
Import-Module $script:CodexDeploymentModulePath -Force -ErrorAction Stop

# The checkpoint key this family validates.
$script:CODEX_MODEL_ROUTING_RECEIPTS_KEY = 'codex_model_routing_receipts'

# The ten keys every receipt must carry, and the nine of them the resolver
# reproduces. Pinned to _REQUIRED_KEYS / _RESOLVED_KEYS in the Python reference;
# `phase` is checkpoint-only bookkeeping and is not resolver output.
$script:REQUIRED_RECEIPT_KEYS = @(
    'logical_agent',
    'deployment_agent',
    'phase',
    'complexity_band',
    'execution_context',
    'orchestration_complexity_ceiling',
    'c3_overlay_applied',
    'c3_overlay_reason',
    'model',
    'model_reasoning_effort'
)
$script:RESOLVED_RECEIPT_KEYS = @($script:REQUIRED_RECEIPT_KEYS | Where-Object { $_ -ne 'phase' })

# The complexity-band ordering used by the ceiling monotonicity comparison.
$script:BAND_ORDER = @('C1', 'C2', 'C3', 'C4')


function Get-CodexCeilingTransitionError {
    <#
    .SYNOPSIS
        Return one receipt's ceiling-transition errors (rows U6.X7-U6.X10).
    .DESCRIPTION
        Private helper mirroring _validate_ceiling_transition. Transition evidence
        is required exactly when the orchestration ceiling rises: absent when it
        does not rise, and otherwise an object recording the exact from/to pair and
        a non-empty unique list of affected delegation ids.
    .PARAMETER Receipt
        The deserialized receipt object.
    .PARAMETER Prefix
        The error-message prefix for this receipt position.
    .PARAMETER PreviousCeiling
        The previous receipt's resolved ceiling, or $null for the first receipt.
    .PARAMETER CurrentCeiling
        This receipt's resolved ceiling.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $Receipt,

        [Parameter(Mandatory = $true)]
        [string] $Prefix,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $PreviousCeiling,

        [Parameter(Mandatory = $true)]
        [string] $CurrentCeiling
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $transition = (Get-CheckpointObjectMember -Owner $Receipt -Name 'ceiling_transition').Value

    # U6.X7: with no previous ceiling, or an unchanged ceiling, transition
    # evidence must be absent entirely.
    if ([string]::IsNullOrEmpty($PreviousCeiling) -or ($CurrentCeiling -ceq $PreviousCeiling)) {
        if ($null -ne $transition) {
            $errors.Add("$Prefix.ceiling_transition must be absent unless the ceiling rises.")
        }
        return $errors.ToArray()
    }

    # U6.X8: a risen ceiling requires an object recording the increase.
    if (-not (Test-CheckpointObjectValue -Value $transition)) {
        $errors.Add("$Prefix.ceiling_transition must record a ceiling increase.")
        return $errors.ToArray()
    }

    # U6.X9: the recorded from/to pair must be the actual transition.
    $from = (Get-CheckpointObjectMember -Owner $transition -Name 'from').Value
    $to = (Get-CheckpointObjectMember -Owner $transition -Name 'to').Value
    if (-not (Test-PythonValueEqual -Actual $from -Expected $PreviousCeiling) -or
        -not (Test-PythonValueEqual -Actual $to -Expected $CurrentCeiling)) {
        $errors.Add("$Prefix.ceiling_transition must record $PreviousCeiling to $CurrentCeiling.")
    }

    # U6.X10: the affected delegation ids must be a non-empty list of distinct,
    # non-blank strings. A non-list value is treated as empty, matching Python.
    $affected = (Get-CheckpointObjectMember -Owner $transition -Name 'affected_delegation_ids').Value
    $affectedItems = @()
    if (Test-CheckpointListValue -Value $affected) { $affectedItems = @($affected) }
    $malformed = $affectedItems.Count -eq 0
    if (-not $malformed) {
        $distinct = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
        foreach ($item in $affectedItems) {
            if (-not ($item -is [string]) -or [string]::IsNullOrWhiteSpace([string]$item)) {
                $malformed = $true
                break
            }
            [void]$distinct.Add([string]$item)
        }
        if (-not $malformed -and $distinct.Count -ne $affectedItems.Count) { $malformed = $true }
    }
    if ($malformed) {
        $errors.Add("$Prefix.ceiling_transition.affected_delegation_ids must be a non-empty unique string list.")
    }

    return $errors.ToArray()
}

function Get-CodexModelRoutingResolvedKeyError {
    <#
    .SYNOPSIS
        Return the resolved-key mismatch errors for one receipt (row U6.X11).
    .DESCRIPTION
        Private helper comparing each of the nine resolver-reproduced keys against
        the resolver output. Both sides render with Python repr() semantics because
        the inventory template uses {expected!r} and {actual!r}.
    .PARAMETER Receipt
        The deserialized receipt object.
    .PARAMETER Prefix
        The error-message prefix for this receipt position.
    .PARAMETER Expected
        The resolver output hashtable.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $Receipt,

        [Parameter(Mandatory = $true)]
        [string] $Prefix,

        [Parameter(Mandatory = $true)]
        [hashtable] $Expected
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # Compare every resolver-reproduced key so a receipt reports all of its
    # mismatches at once rather than only the first.
    foreach ($key in $script:RESOLVED_RECEIPT_KEYS) {
        $actual = (Get-CheckpointObjectMember -Owner $Receipt -Name $key).Value
        if (-not (Test-PythonValueEqual -Actual $actual -Expected $Expected[$key])) {
            $expectedText = ConvertTo-PythonReprText -Value $Expected[$key]
            $actualText = ConvertTo-PythonReprText -Value $actual
            $errors.Add("$Prefix.$key must be $expectedText, found $actualText.")
        }
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateCodexModelRoutingReceiptError {
    <#
    .SYNOPSIS
        Return the codex_model_routing_receipts errors (rows U6.X1-U6.X11).
    .DESCRIPTION
        Public entry mirroring validate_codex_model_routing_receipts. Walks the
        receipt array in order, carrying the previous resolved ceiling forward so
        the monotonicity and transition rules can be applied, and reports every
        malformed receipt with its own index.

        Control flow reproduces the Python reference exactly: missing keys stop
        that receipt; a resolver failure stops that receipt and leaves the carried
        ceiling unchanged; a monotonicity violation suppresses the transition check
        for that receipt but still advances the carried ceiling.
    .PARAMETER Value
        The raw deserialized value of the codex_model_routing_receipts key.
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

    # U6.X1: the caller invokes this only when the key is present, so a non-list
    # value is itself the error and nothing further can be inspected.
    if (-not (Test-CheckpointListValue -Value $Value)) {
        $errors.Add("Checkpoint $($script:CODEX_MODEL_ROUTING_RECEIPTS_KEY) must be a list when present.")
        return $errors.ToArray()
    }

    $previousCeiling = $null
    $index = 0
    foreach ($item in @($Value)) {
        $prefix = "Checkpoint $($script:CODEX_MODEL_ROUTING_RECEIPTS_KEY)[$index]"
        $index++

        # U6.X2: a non-object entry has no keys to inspect.
        if (-not (Test-CheckpointObjectValue -Value $item)) {
            $errors.Add("$prefix must be an object.")
            continue
        }

        # U6.X3: a receipt missing any required key stops here, because the
        # resolver cannot be called without complete inputs.
        $names = @(Get-CheckpointObjectMemberName -Owner $item)
        $missing = @($script:REQUIRED_RECEIPT_KEYS | Where-Object { $names -notcontains $_ })
        if ($missing.Count -gt 0) {
            $errors.Add("$prefix missing required keys: $($missing -join ', ').")
            continue
        }

        # U6.X4: the phase is checkpoint bookkeeping; a malformed phase is
        # reported but does not stop the resolver comparison.
        $phase = (Get-CheckpointObjectMember -Owner $item -Name 'phase').Value
        if (-not ($phase -is [string]) -or [string]::IsNullOrWhiteSpace([string]$phase)) {
            $errors.Add("$prefix.phase must be a non-empty string.")
        }

        # U6.X5: resolve through the single Codex deployment resolver. Only the
        # ValueError-equivalent surface is caught, matching the Python except
        # clause; every input is coerced with Python str() semantics first.
        $expected = $null
        try {
            $expected = Resolve-CodexDeployment `
                -LogicalAgent (ConvertTo-PythonDisplayText -Value (Get-CheckpointObjectMember -Owner $item -Name 'logical_agent').Value) `
                -ComplexityBand (ConvertTo-PythonDisplayText -Value (Get-CheckpointObjectMember -Owner $item -Name 'complexity_band').Value) `
                -ExecutionContext (ConvertTo-PythonDisplayText -Value (Get-CheckpointObjectMember -Owner $item -Name 'execution_context').Value) `
                -OrchestrationComplexityCeiling (ConvertTo-PythonDisplayText -Value (Get-CheckpointObjectMember -Owner $item -Name 'orchestration_complexity_ceiling').Value)
        } catch [System.ArgumentException] {
            $errors.Add("$prefix has invalid routing inputs: $($_.Exception.Message)")
            continue
        }

        # U6.X6 and the transition rules. A ceiling that drops is a monotonicity
        # violation and suppresses the transition check for this receipt; the
        # carried ceiling advances either way.
        $currentCeiling = [string]$expected['orchestration_complexity_ceiling']
        if ($null -ne $previousCeiling -and
            ($script:BAND_ORDER.IndexOf($currentCeiling) -lt $script:BAND_ORDER.IndexOf([string]$previousCeiling))) {
            $errors.Add("$prefix.orchestration_complexity_ceiling must be monotonic; found $currentCeiling after $previousCeiling.")
        } else {
            $errors.AddRange([string[]]@(
                    Get-CodexCeilingTransitionError -Receipt $item -Prefix $prefix `
                        -PreviousCeiling $previousCeiling -CurrentCeiling $currentCeiling
                ))
        }
        $previousCeiling = $currentCeiling

        # U6.X11: every resolver-reproduced key must match the resolver output.
        $errors.AddRange([string[]]@(Get-CodexModelRoutingResolvedKeyError -Receipt $item -Prefix $prefix -Expected $expected))
    }

    return $errors.ToArray()
}

# Only the family entry point is exported; the transition and resolved-key helpers
# stay private so the ordered, ceiling-carrying walk cannot be bypassed.
Export-ModuleMember -Function Get-OrchestratorStateCodexModelRoutingReceiptError
