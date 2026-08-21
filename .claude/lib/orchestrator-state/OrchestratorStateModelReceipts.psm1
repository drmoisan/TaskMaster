<#
.SYNOPSIS
    Portable complexity-assessment and model-routing-receipt per-entry checks.

.DESCRIPTION
    Destination-runtime PowerShell port of the key-gated per-entry validators the
    authoritative Python validator runs in its unconditional block. The rows
    ported here are the parity-inventory families U6.C (complexity_assessments,
    rows U6.C1-U6.C7) and U6.M (model_routing_receipts, rows U6.M1-U6.M6),
    matching the error-string templates in
    `scripts/dev_tools/_orchestrator_state_complexity.py` and
    `scripts/dev_tools/_orchestrator_state_model_routing.py`.

    SINGLE-IMPLEMENTATION RULE. Row U6.C5 recomputes the floor by calling
    `Get-ComplexityFloor` and row U6.M4 resolves the expected model by calling
    `Resolve-DelegationModel`, both from `.claude/lib/model-routing/ModelRouting.psm1`.
    Neither formula is re-implemented here, mirroring the Python gate's own reuse
    constraint. A future change to either formula must be made in that one module.

    Every function is pure: it reads no file, starts no process, and never mutates
    its input. Each check returns a string array, empty when the block is valid.
#>

Set-StrictMode -Version Latest

# Import the shared checkpoint-value primitives and the two reference formulas,
# resolved relative to this module's directory so the imports travel with the
# pushed-down pack regardless of the consumer repository's working directory.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force
$script:ModelRoutingModulePath = Join-Path -Path (Join-Path -Path $PSScriptRoot -ChildPath '..') -ChildPath (Join-Path -Path 'model-routing' -ChildPath 'ModelRouting.psm1')
Import-Module $script:ModelRoutingModulePath -Force

# The complexity-band vocabulary, ordered lowest to highest. Pinned to BAND_ORDER
# in scripts/dev_tools/compute_complexity_floor.py and to the identical ordering
# inside ModelRouting.psm1. This is the enum and its ordering only; the floor and
# model formulas themselves are never duplicated here.
$script:BAND_ORDER = @('C1', 'C2', 'C3', 'C4')

# The model tier removed from consideration under the disabled policy, the tier a
# disabled-mode fable cell clamps down to, and the policy literal itself.
$script:FABLE_MODEL = 'fable'
$script:DISABLED_CLAMP_MODEL = 'opus'
$script:DISABLED_POLICY = 'disabled'


function Get-CheckpointStringList {
    <#
    .SYNOPSIS
        Return a value as a string list only when it has that exact shape.
    .DESCRIPTION
        Private helper mirroring _string_list in the Python complexity module. A
        non-list value, or a list containing a non-string element, has no string
        list form; an empty list does, and yields an empty result.
    .PARAMETER Value
        The deserialized JSON value to inspect. May be $null.
    .OUTPUTS
        System.Collections.Hashtable with keys Ok (bool) and Value (string[]).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    if (-not (Test-CheckpointListValue -Value $Value)) {
        return @{ Ok = $false; Value = [string[]]@() }
    }

    # A single non-string element disqualifies the whole list, matching Python's
    # all(isinstance(item, str)) guard.
    foreach ($item in $Value) {
        if (-not ($item -is [string])) {
            return @{ Ok = $false; Value = [string[]]@() }
        }
    }

    return @{ Ok = $true; Value = [string[]]@($Value) }
}

function Get-ComplexityAssessmentEntryError {
    <#
    .SYNOPSIS
        Return one complexity assessment's errors (rows U6.C3-U6.C7).
    .DESCRIPTION
        Private helper mirroring _validate_one_assessment: band enum membership,
        a recomputable signals list, floor equality against Get-ComplexityFloor,
        the band-at-or-above-floor lower bound, and a non-empty rationale. The
        floor is never recomputed inline; the shared formula is called.
    .PARAMETER Index
        The assessment's zero-based position, used for error context.
    .PARAMETER Assessment
        The deserialized assessment object.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [int] $Index,

        [Parameter(Mandatory = $true)]
        [psobject] $Assessment
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $band = (Get-CheckpointObjectMember -Owner $Assessment -Name 'band').Value
    $floor = (Get-CheckpointObjectMember -Owner $Assessment -Name 'floor').Value
    $signalsPresent = (Get-CheckpointObjectMember -Owner $Assessment -Name 'signals_present').Value
    $rationale = (Get-CheckpointObjectMember -Owner $Assessment -Name 'rationale').Value

    $bandValid = ($band -is [string]) -and ($script:BAND_ORDER -ccontains [string]$band)
    $floorValid = ($floor -is [string]) -and ($script:BAND_ORDER -ccontains [string]$floor)

    # U6.C3: band must be within the permitted enum.
    if (-not $bandValid) {
        $errors.Add("Checkpoint complexity_assessments #$Index band must be one of C1, C2, C3, C4; got: $(ConvertTo-PythonDisplayText -Value $band).")
    }

    # U6.C4 / U6.C5: the floor can only be recomputed from a list of strings, so a
    # malformed signals list reports itself and suppresses the equality check.
    $signalList = Get-CheckpointStringList -Value $signalsPresent
    if (-not $signalList.Ok) {
        $errors.Add("Checkpoint complexity_assessments #$Index signals_present must be a list of strings.")
    } else {
        $expectedFloor = Get-ComplexityFloor -SignalsPresent $signalList.Value
        if (-not ($floor -is [string]) -or ([string]$floor -cne [string]$expectedFloor)) {
            $errors.Add("Checkpoint complexity_assessments #$Index floor $(ConvertTo-PythonDisplayText -Value $floor) does not equal compute_complexity_floor(signals_present) $expectedFloor.")
        }
    }

    # U6.C6: the band-at-or-above-floor lower bound. Both values must be valid
    # bands to compare, so a prior enum error suppresses a spurious ordering error.
    if ($bandValid -and $floorValid -and
        ($script:BAND_ORDER.IndexOf([string]$band) -lt $script:BAND_ORDER.IndexOf([string]$floor))) {
        $errors.Add("Checkpoint complexity_assessments #$Index band $band is below its floor $floor.")
    }

    # U6.C7: rationale must be a non-empty string.
    if (-not ($rationale -is [string]) -or [string]::IsNullOrWhiteSpace([string]$rationale)) {
        $errors.Add("Checkpoint complexity_assessments #$Index rationale must be a non-empty string.")
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateComplexityAssessmentError {
    <#
    .SYNOPSIS
        Return the complexity_assessments errors (inventory rows U6.C1-U6.C7).
    .DESCRIPTION
        Public entry mirroring _validate_complexity_assessments. The caller invokes
        this only when the key is present, so a non-list value is itself the error.
        Each entry is validated independently so callers receive a complete list.
    .PARAMETER Value
        The raw deserialized value of the checkpoint's complexity_assessments key.
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

    if (-not (Test-CheckpointListValue -Value $Value)) {
        $errors.Add('Checkpoint complexity_assessments must be a list when present.')
        return $errors.ToArray()
    }

    # Walk every entry position so each malformed assessment is reported with its
    # own index rather than stopping at the first one.
    $index = 0
    foreach ($assessment in @($Value)) {
        if (-not (Test-CheckpointObjectValue -Value $assessment)) {
            $errors.Add("Checkpoint complexity_assessments #$index must be an object.")
            $index++
            continue
        }
        $errors.AddRange([string[]]@(Get-ComplexityAssessmentEntryError -Index $index -Assessment $assessment))
        $index++
    }

    return $errors.ToArray()
}

function Get-ModelRoutingDisabledClampError {
    <#
    .SYNOPSIS
        Return one receipt's disabled-mode clamp errors (rows U6.M5-U6.M6).
    .DESCRIPTION
        Private helper mirroring _validate_disabled_clamp. Under the disabled
        policy fable is removed from the consideration set, so no receipt may
        resolve to fable, and a fable table cell must record the clamp to opus
        with clamped_from fable.
    .PARAMETER Index
        The receipt's zero-based position, used for error context.
    .PARAMETER TableModel
        The receipt's pre-clamp table_model value.
    .PARAMETER ClampedFrom
        The receipt's clamped_from value.
    .PARAMETER Model
        The receipt's post-clamp model value.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [int] $Index,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $TableModel,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $ClampedFrom,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Model
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # U6.M5: no receipt model may be fable under the disabled policy.
    if (($Model -is [string]) -and ([string]$Model -ceq $script:FABLE_MODEL)) {
        $errors.Add("Checkpoint model_routing_receipts #$Index model must not be fable under fable_policy disabled.")
    }

    # U6.M6: a fable table cell must record the clamp provenance; both halves of
    # the provenance (clamped_from fable and model opus) are required together.
    if (($TableModel -is [string]) -and ([string]$TableModel -ceq $script:FABLE_MODEL)) {
        $clampRecorded = ($ClampedFrom -is [string]) -and ([string]$ClampedFrom -ceq $script:FABLE_MODEL) -and
        ($Model -is [string]) -and ([string]$Model -ceq $script:DISABLED_CLAMP_MODEL)
        if (-not $clampRecorded) {
            $errors.Add("Checkpoint model_routing_receipts #$Index table_model fable under fable_policy disabled must record clamped_from fable and model opus.")
        }
    }

    return $errors.ToArray()
}

function Get-ModelRoutingReceiptEntryError {
    <#
    .SYNOPSIS
        Return one model-routing receipt's errors (rows U6.M3-U6.M6).
    .DESCRIPTION
        Private helper mirroring _validate_one_receipt: the band must be a valid
        enum member before the resolver can run, the recorded model must equal
        Resolve-DelegationModel's result, and the disabled-mode clamp invariants
        apply when the session policy removed fable. The model formula is never
        re-implemented here; the shared formula is called.
    .PARAMETER Index
        The receipt's zero-based position, used for error context.
    .PARAMETER Receipt
        The deserialized receipt object.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [int] $Index,

        [Parameter(Mandatory = $true)]
        [psobject] $Receipt
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $agent = (Get-CheckpointObjectMember -Owner $Receipt -Name 'agent').Value
    $band = (Get-CheckpointObjectMember -Owner $Receipt -Name 'complexity_band').Value
    $fablePolicy = (Get-CheckpointObjectMember -Owner $Receipt -Name 'fable_policy').Value
    $tableModel = (Get-CheckpointObjectMember -Owner $Receipt -Name 'table_model').Value
    $clampedFrom = (Get-CheckpointObjectMember -Owner $Receipt -Name 'clamped_from').Value
    $model = (Get-CheckpointObjectMember -Owner $Receipt -Name 'model').Value

    # U6.M3: an invalid band cannot be resolved, so report it and stop this
    # receipt rather than calling the resolver with an out-of-table key.
    if (-not ($band -is [string]) -or -not ($script:BAND_ORDER -ccontains [string]$band)) {
        $errors.Add("Checkpoint model_routing_receipts #$Index complexity_band must be one of C1, C2, C3, C4; got: $(ConvertTo-PythonDisplayText -Value $band).")
        return $errors.ToArray()
    }

    # U6.M4: resolve the expected model from the canonical shared formula. Agent
    # and policy are rendered through the Python str() renderer because the Python
    # reference coerces both with str() before the lookup.
    $expected = Resolve-DelegationModel `
        -Agent (ConvertTo-PythonDisplayText -Value $agent) `
        -Band ([string]$band) `
        -FablePolicy (ConvertTo-PythonDisplayText -Value $fablePolicy)
    $expectedModel = [string]$expected['model']
    if (-not ($model -is [string]) -or ([string]$model -cne $expectedModel)) {
        $errors.Add("Checkpoint model_routing_receipts #$Index model $(ConvertTo-PythonDisplayText -Value $model) does not equal resolve_delegation_model(agent, complexity_band, fable_policy) $expectedModel.")
    }

    # The clamp invariants apply only when fable is removed from the consideration
    # set for this session.
    if (($fablePolicy -is [string]) -and ([string]$fablePolicy -ceq $script:DISABLED_POLICY)) {
        $errors.AddRange([string[]]@(
                Get-ModelRoutingDisabledClampError -Index $Index -TableModel $tableModel -ClampedFrom $clampedFrom -Model $model
            ))
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateModelRoutingReceiptError {
    <#
    .SYNOPSIS
        Return the model_routing_receipts errors (inventory rows U6.M1-U6.M6).
    .DESCRIPTION
        Public entry mirroring _validate_model_routing_receipts. The caller invokes
        this only when the key is present, so a non-list value is itself the error.
        Each receipt is validated independently so callers receive a complete list.
    .PARAMETER Value
        The raw deserialized value of the checkpoint's model_routing_receipts key.
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

    if (-not (Test-CheckpointListValue -Value $Value)) {
        $errors.Add('Checkpoint model_routing_receipts must be a list when present.')
        return $errors.ToArray()
    }

    # Walk every receipt position so each malformed entry is reported with its own
    # index rather than stopping at the first one.
    $index = 0
    foreach ($receipt in @($Value)) {
        if (-not (Test-CheckpointObjectValue -Value $receipt)) {
            $errors.Add("Checkpoint model_routing_receipts #$index must be an object.")
            $index++
            continue
        }
        $errors.AddRange([string[]]@(Get-ModelRoutingReceiptEntryError -Index $index -Receipt $receipt))
        $index++
    }

    return $errors.ToArray()
}

# Both family entry points are exported for the U-family aggregator and for the
# completion gate's M3 reuse leg, which invokes this same implementation rather
# than re-implementing the per-entry rows.
Export-ModuleMember -Function `
    Get-OrchestratorStateComplexityAssessmentError, `
    Get-OrchestratorStateModelRoutingReceiptError
