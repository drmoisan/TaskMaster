<#
.SYNOPSIS
    Portable codex_topology_receipts per-entry checks (inventory family U6.T).

.DESCRIPTION
    Destination-runtime PowerShell port of
    `scripts/dev_tools/_orchestrator_state_codex_topology.py`, covering
    parity-inventory rows U6.T1 through U6.T11: the list and object shape, the
    thirteen required keys, the non-empty phase, the resolver input-type checks
    (languages, the two file counts, the cross-cutting flag, the execution
    context, and the root-persona enum), the resolver-invalid-inputs surface, and
    the resolved-key comparison.

    SINGLE-IMPLEMENTATION RULE. The expected topology is obtained by calling
    `Resolve-CodexTopology`, and the permitted root personas are read from
    `Get-CodexForcedRootPersona`, both from
    `.claude/lib/codex-routing/CodexTopology.psm1`. The language-budget table and
    the escalation precedence are never re-implemented here.

    Row U6.T6 rejects a boolean where an integer is required, reproducing the
    Python guard that exists because bool is a subclass of int. Row U6.T11 renders
    both sides of a mismatch with Python `repr()` semantics, which for the
    `languages` key means a Python list literal. Row U6.T10 interpolates the
    resolver's exception text with Python `str()` semantics.

    Every function is pure: it reads no file, starts no process, and never mutates
    its input.
#>

Set-StrictMode -Version Latest

# Import the shared checkpoint-value primitives and the single Codex topology
# resolver, resolved relative to this module's directory so both imports travel
# with the pushed-down pack regardless of the working directory.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force
$script:CodexTopologyModulePath = Join-Path -Path (Join-Path -Path $PSScriptRoot -ChildPath '..') -ChildPath (Join-Path -Path 'codex-routing' -ChildPath 'CodexTopology.psm1')
Import-Module $script:CodexTopologyModulePath -Force

# The checkpoint key this family validates.
$script:CODEX_TOPOLOGY_RECEIPTS_KEY = 'codex_topology_receipts'

# The thirteen keys every receipt must carry, and the twelve of them the resolver
# reproduces. Pinned to _REQUIRED_KEYS / _RESOLVED_KEYS in the Python reference;
# `phase` is checkpoint-only bookkeeping and is not resolver output.
$script:REQUIRED_RECEIPT_KEYS = @(
    'phase',
    'execution_context',
    'languages',
    'production_file_count',
    'test_file_count',
    'cross_cutting',
    'root_persona',
    'route',
    'topology',
    'logical_agent',
    'routing_reason',
    'max_production_files',
    'max_test_files'
)
$script:RESOLVED_RECEIPT_KEYS = @($script:REQUIRED_RECEIPT_KEYS | Where-Object { $_ -ne 'phase' })

# The two file-count keys subject to the integer-not-boolean rule.
$script:FILE_COUNT_KEYS = @('production_file_count', 'test_file_count')

# Rendered form of the Python sorted FORCED_ROOT_PERSONAS tuple, used verbatim in
# the root-persona message. The membership test itself reads the live set from the
# resolver module so the enum has one source.
$script:FORCED_ROOT_PERSONAS_PYTHON_TUPLE = "('epic-orchestrator', 'epic-planner')"

# The integral CLR types a JSON integer can deserialize to. A CLR boolean is
# deliberately absent, matching the Python bool rejection.
$script:INTEGRAL_TYPES = @([int], [long], [short], [byte])


function Get-CodexTopologyInputError {
    <#
    .SYNOPSIS
        Return one receipt's resolver-input type errors (rows U6.T5-U6.T9).
    .DESCRIPTION
        Private helper mirroring _receipt_inputs. Every input the resolver
        consumes is type-checked here first, in the Python order, so the resolver
        is never called with a value it would reject by type. A non-empty result
        means the receipt is skipped before resolution, matching the Python
        `if inputs is None: continue` branch.
    .PARAMETER Receipt
        The deserialized receipt object.
    .PARAMETER Prefix
        The error-message prefix for this receipt position.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $Receipt,

        [Parameter(Mandatory = $true)]
        [string] $Prefix
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # U6.T5: languages must be a list in which every member is a non-blank string.
    $languages = (Get-CheckpointObjectMember -Owner $Receipt -Name 'languages').Value
    $languagesValid = Test-CheckpointListValue -Value $languages
    if ($languagesValid) {
        foreach ($language in @($languages)) {
            if (-not ($language -is [string]) -or [string]::IsNullOrWhiteSpace([string]$language)) {
                $languagesValid = $false
                break
            }
        }
    }
    if (-not $languagesValid) {
        $errors.Add("$Prefix.languages must be a list of non-empty strings.")
    }

    # U6.T6: both file counts must be integers, and a boolean is explicitly not an
    # integer here even though Python's bool subclasses int.
    foreach ($key in $script:FILE_COUNT_KEYS) {
        $value = (Get-CheckpointObjectMember -Owner $Receipt -Name $key).Value
        $isIntegral = $false
        if ($null -ne $value -and -not ($value -is [bool])) {
            foreach ($integralType in $script:INTEGRAL_TYPES) {
                if ($value.GetType() -eq $integralType) { $isIntegral = $true; break }
            }
        }
        if (-not $isIntegral) {
            $errors.Add("$Prefix.$key must be an integer.")
        }
    }

    # U6.T7 and U6.T8: the cross-cutting flag and the execution context.
    $crossCutting = (Get-CheckpointObjectMember -Owner $Receipt -Name 'cross_cutting').Value
    if (-not ($crossCutting -is [bool])) {
        $errors.Add("$Prefix.cross_cutting must be a boolean.")
    }
    $receiptExecutionContext = (Get-CheckpointObjectMember -Owner $Receipt -Name 'execution_context').Value
    if (-not ($receiptExecutionContext -is [string])) {
        $errors.Add("$Prefix.execution_context must be a string.")
    }

    # U6.T9: root_persona is optional, but a present value must be a forced root
    # persona. The permitted set is read from the resolver module, not restated.
    $rootPersona = (Get-CheckpointObjectMember -Owner $Receipt -Name 'root_persona').Value
    if ($null -ne $rootPersona) {
        $permitted = @(Get-CodexForcedRootPersona)
        if (-not ($rootPersona -is [string]) -or ($permitted -cnotcontains [string]$rootPersona)) {
            $errors.Add("$Prefix.root_persona must be null or one of $($script:FORCED_ROOT_PERSONAS_PYTHON_TUPLE).")
        }
    }

    return $errors.ToArray()
}

function Get-CodexTopologyResolvedKeyError {
    <#
    .SYNOPSIS
        Return the resolved-key mismatch errors for one receipt (row U6.T11).
    .DESCRIPTION
        Private helper comparing each of the twelve resolver-reproduced keys
        against the resolver output. Both sides render with Python repr()
        semantics, which for the languages key produces a Python list literal.
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

function Get-OrchestratorStateCodexTopologyReceiptError {
    <#
    .SYNOPSIS
        Return the codex_topology_receipts errors (inventory rows U6.T1-U6.T11).
    .DESCRIPTION
        Public entry mirroring validate_codex_topology_receipts. Each receipt is
        validated independently, in order, and reported with its own index.

        Control flow reproduces the Python reference exactly: missing keys stop
        that receipt before any type check; a malformed phase is reported but does
        not stop the receipt; any resolver-input type error stops the receipt
        before resolution; and a resolver failure stops the receipt before the
        resolved-key comparison.
    .PARAMETER Value
        The raw deserialized value of the codex_topology_receipts key.
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

    # U6.T1: the caller invokes this only when the key is present, so a non-list
    # value is itself the error and nothing further can be inspected.
    if (-not (Test-CheckpointListValue -Value $Value)) {
        $errors.Add("Checkpoint $($script:CODEX_TOPOLOGY_RECEIPTS_KEY) must be a list when present.")
        return $errors.ToArray()
    }

    $index = 0
    foreach ($item in @($Value)) {
        $prefix = "Checkpoint $($script:CODEX_TOPOLOGY_RECEIPTS_KEY)[$index]"
        $index++

        # U6.T2: a non-object entry has no keys to inspect.
        if (-not (Test-CheckpointObjectValue -Value $item)) {
            $errors.Add("$prefix must be an object.")
            continue
        }

        # U6.T3: a receipt missing any required key stops here, because the
        # resolver cannot be called without complete inputs.
        $names = @(Get-CheckpointObjectMemberName -Owner $item)
        $missing = @($script:REQUIRED_RECEIPT_KEYS | Where-Object { $names -notcontains $_ })
        if ($missing.Count -gt 0) {
            $errors.Add("$prefix missing required keys: $($missing -join ', ').")
            continue
        }

        # U6.T4: the phase is checkpoint bookkeeping; a malformed phase is
        # reported but does not stop the resolver comparison.
        $phase = (Get-CheckpointObjectMember -Owner $item -Name 'phase').Value
        if (-not ($phase -is [string]) -or [string]::IsNullOrWhiteSpace([string]$phase)) {
            $errors.Add("$prefix.phase must be a non-empty string.")
        }

        # U6.T5-U6.T9: any resolver-input type error stops this receipt, so the
        # resolver is never handed a value it would reject by type.
        $inputErrors = @(Get-CodexTopologyInputError -Receipt $item -Prefix $prefix)
        if ($inputErrors.Count -gt 0) {
            $errors.AddRange([string[]]$inputErrors)
            continue
        }

        # U6.T10: resolve through the single Codex topology resolver. Only the
        # ValueError-equivalent surface is caught, matching the Python except.
        $expected = $null
        try {
            $expected = Resolve-CodexTopology `
                -Language (Get-CheckpointObjectMember -Owner $item -Name 'languages').Value `
                -ProductionFileCount (Get-CheckpointObjectMember -Owner $item -Name 'production_file_count').Value `
                -TestFileCount (Get-CheckpointObjectMember -Owner $item -Name 'test_file_count').Value `
                -ExecutionContext ([string](Get-CheckpointObjectMember -Owner $item -Name 'execution_context').Value) `
                -CrossCutting (Get-CheckpointObjectMember -Owner $item -Name 'cross_cutting').Value `
                -RootPersona (Get-CheckpointObjectMember -Owner $item -Name 'root_persona').Value
        } catch [System.ArgumentException] {
            $errors.Add("$prefix has invalid routing inputs: $($_.Exception.Message)")
            continue
        }

        # U6.T11: every resolver-reproduced key must match the resolver output.
        $errors.AddRange([string[]]@(Get-CodexTopologyResolvedKeyError -Receipt $item -Prefix $prefix -Expected $expected))
    }

    return $errors.ToArray()
}

# Only the family entry point is exported; the input-type and resolved-key helpers
# stay private so no consumer can skip the ordered per-receipt walk.
Export-ModuleMember -Function Get-OrchestratorStateCodexTopologyReceiptError
