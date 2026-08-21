<#
.SYNOPSIS
    Portable orchestrator-state checkpoint checks for pushed-down enforcement hooks.

.DESCRIPTION
    Provides a self-contained PowerShell implementation of the pushed-down-relevant
    orchestrator-state checkpoint validations so the `.claude` enforcement hooks work
    in consumer repositories that do not ship `scripts/dev_tools` (the authoritative
    Python validator). This module mirrors the portable pattern of
    `.claude/lib/model-routing/ModelRouting.psm1`.

    It implements PR-creation-readiness parity with
    `scripts/dev_tools/_orchestrator_state_pr_creation_readiness.py` and the base
    checkpoint-presence checks (required keys, step-status validity, blocked_reason
    validity) from `scripts/dev_tools/validate_orchestrator_state.py`. The base
    constants below are pinned to `REQUIRED_STATE_KEYS`, `STEP_STATUS_KEYS`,
    `VALID_STEP_STATUS`, `VALID_BLOCKED_REASONS`, and `STEP_SPECIFIC_EXTRA_STATUS`.

    Every public function FAILS CLOSED: a missing checkpoint file, invalid JSON, a
    missing required key, an invalid step status, or an unmet readiness condition all
    yield a non-zero ExitCode with a non-empty Output message. The Python validator
    is the parity reference this module is measured against; as of issue #475 it is
    not consulted at runtime, and this module is the only implementation the hooks
    run in every repository, drm-copilot included.

    This module also hosts the PR-creation preflight orchestration helper
    (Invoke-OrchestratorStatePreflight) consumed by
    .claude/hooks/enforce-pr-author-skill.ps1. Its default seam runs the portable
    in-process validation and names no interpreter on any code path.
#>

Set-StrictMode -Version Latest

# The canonical top-level checkpoint keys required by the primary validator.
# Pinned to REQUIRED_STATE_KEYS in scripts/dev_tools/validate_orchestrator_state.py.
$script:REQUIRED_STATE_KEYS = @(
    'objective',
    'change_budget_estimate',
    'path_selected',
    'promotion-type',
    'short-name',
    'relativeFile',
    'long-name',
    'issue-num',
    'feature-folder',
    'work-mode',
    'plan-path',
    'completed_steps',
    'next_step',
    'last_updated',
    'step5_status',
    'step6_status',
    'step7_status',
    'step8_status',
    'step9_status',
    'step10_status',
    'delegation_receipts',
    'blocked_reason'
)

# The lifecycle step-status keys whose value, when present, must be a member of
# VALID_STEP_STATUS. Pinned to STEP_STATUS_KEYS in the primary validator.
$script:STEP_STATUS_KEYS = @(
    'step5_status',
    'step6_status',
    'step7_status',
    'step8_status',
    'step9_status',
    'step10_status'
)

# The allowed step-status vocabulary. Pinned to VALID_STEP_STATUS in the primary
# validator.
$script:VALID_STEP_STATUS = @(
    'not-applicable',
    'pending',
    'delegated',
    'verified',
    'blocked',
    'not_started',
    'in_progress',
    'completed'
)

# Per-key additive step-status vocabulary layered on VALID_STEP_STATUS: each value
# below is valid only on its owning key and is still rejected on every other step
# key. Pinned to STEP_SPECIFIC_EXTRA_STATUS in
# scripts/dev_tools/_orchestrator_state_step_status.py.
$script:STEP_SPECIFIC_EXTRA_STATUS = @{
    step6_status = @('blocked_remediation_loop_limit')
    step9_status = @('passed', 'failed_remediation_required', 'blocked_ci_loop_limit')
}

# The allowed blocked_reason vocabulary. Pinned to VALID_BLOCKED_REASONS in the
# primary validator.
$script:VALID_BLOCKED_REASONS = @(
    'none',
    'spawn_agent_unavailable',
    'delegation_launch_failed',
    'delegate_no_receipt',
    'delegate_contract_incomplete',
    'validator_failed',
    'user_requested_stop'
)

# The upstream steps that must not be pending/blocked before the first PR creation
# of a branch. Pinned to PR_CREATION_READY_STEP_KEYS in
# _orchestrator_state_pr_creation_readiness.py (deliberately narrower than the full
# step set: steps 9-10 can only populate after PR creation and CI have run).
$script:PR_CREATION_READY_STEP_KEYS = @(
    'step5_status',
    'step6_status',
    'step7_status',
    'step8_status'
)

# The checkpoint list fields that must be empty (or absent) before the first PR
# creation. Pinned to PR_CREATION_READY_EMPTY_LIST_KEYS in the Python reference.
$script:PR_CREATION_READY_EMPTY_LIST_KEYS = @(
    'local_execution_overrides',
    'delegation_bypasses'
)


function Get-OrchestratorStateCheckpoint {
    <#
    .SYNOPSIS
        Load and parse an orchestrator-state checkpoint, failing closed on error.
    .DESCRIPTION
        Private load helper. Reads the checkpoint at -CheckpointPath and parses it as
        JSON. It never throws to the caller: a missing file or unparseable/invalid
        JSON is reported through a structured result with Ok = $false and a non-empty
        Error string, so callers can translate the failure into a fail-closed
        non-zero ExitCode.
    .PARAMETER CheckpointPath
        The path to the orchestrator-state checkpoint JSON file.
    .OUTPUTS
        System.Collections.Hashtable with keys:
          - Ok    (bool): $true when the file exists and parsed to a JSON object.
          - State (object): the parsed PSCustomObject on success, otherwise $null.
          - Error (string): a non-empty failure message on failure, otherwise ''.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $CheckpointPath
    )

    # A missing checkpoint file is a fail-closed condition: there is no state to
    # validate, so readiness cannot be established.
    if (-not (Test-Path -LiteralPath $CheckpointPath -PathType Leaf)) {
        return @{
            Ok    = $false
            State = $null
            Error = "Checkpoint file '$CheckpointPath' does not exist."
        }
    }

    $raw = Get-Content -LiteralPath $CheckpointPath -Raw -ErrorAction Stop

    # An empty checkpoint carries no state; treat it as fail-closed rather than
    # letting ConvertFrom-Json return $null silently.
    if ([string]::IsNullOrWhiteSpace($raw)) {
        return @{
            Ok    = $false
            State = $null
            Error = "Checkpoint file '$CheckpointPath' is empty."
        }
    }

    # Parse the checkpoint text. Invalid JSON is a fail-closed condition, caught here
    # so the caller receives a message instead of a terminating error.
    try {
        $state = $raw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        return @{
            Ok    = $false
            State = $null
            Error = "Checkpoint file '$CheckpointPath' is not valid JSON: $($_.Exception.Message)"
        }
    }

    # A JSON scalar or array root is not a checkpoint object; require an object so
    # the presence checks can inspect named fields.
    if ($state -isnot [System.Management.Automation.PSCustomObject]) {
        return @{
            Ok    = $false
            State = $null
            Error = "Checkpoint file '$CheckpointPath' root must be a JSON object."
        }
    }

    return @{ Ok = $true; State = $state; Error = '' }
}

function Get-OrchestratorStateField {
    <#
    .SYNOPSIS
        Read a checkpoint field, distinguishing an absent key from a null value.
    .DESCRIPTION
        Private accessor that safely reads a named property from the parsed
        checkpoint object under Set-StrictMode, where accessing an undefined property
        would otherwise throw. Returns whether the key is present and its value,
        mirroring the semantics of Python's ``dict.get`` (absent and null both read
        as "no value" for the readiness checks).
    .PARAMETER State
        The parsed checkpoint PSCustomObject.
    .PARAMETER Name
        The property name to read.
    .OUTPUTS
        System.Collections.Hashtable with keys Present (bool) and Value (object).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State,

        [Parameter(Mandatory = $true)]
        [string] $Name
    )

    $names = @($State.PSObject.Properties | ForEach-Object { $_.Name })
    if ($names -contains $Name) {
        return @{ Present = $true; Value = $State.$Name }
    }
    return @{ Present = $false; Value = $null }
}

function Get-OrchestratorStateBasePresenceError {
    <#
    .SYNOPSIS
        Return the base checkpoint-presence errors, mirroring the primary validator.
    .DESCRIPTION
        Private base check. Emits one error string per missing required key, one per
        step5_status..step10_status value outside VALID_STEP_STATUS and that key's
        STEP_SPECIFIC_EXTRA_STATUS set, and one when blocked_reason is present with a
        value outside VALID_BLOCKED_REASONS. This mirrors the base block of
        scripts/dev_tools/validate_orchestrator_state.py (required keys, step-status
        validity, blocked_reason validity) that runs before any mode-specific gate.
    .PARAMETER State
        The parsed checkpoint PSCustomObject.
    .OUTPUTS
        System.String[] - zero or more error strings; empty when the base shape is valid.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $names = @($State.PSObject.Properties | ForEach-Object { $_.Name })

    # Require every canonical top-level field; a missing key is reported individually
    # so the operator sees exactly which fields are absent.
    foreach ($key in $script:REQUIRED_STATE_KEYS) {
        if ($names -notcontains $key) {
            $errors.Add("Checkpoint missing required key: $key")
        }
    }

    # Every present step status must be a member of the shared vocabulary or of that
    # key's additive extra set; an absent step key contributes no error (mirrors the
    # primary validator's None guard).
    foreach ($key in $script:STEP_STATUS_KEYS) {
        $field = Get-OrchestratorStateField -State $State -Name $key
        $extra = @()
        if ($script:STEP_SPECIFIC_EXTRA_STATUS.ContainsKey($key)) { $extra = @($script:STEP_SPECIFIC_EXTRA_STATUS[$key]) }
        if ($field.Present -and $null -ne $field.Value -and
            ($script:VALID_STEP_STATUS -notcontains [string]$field.Value) -and
            ($extra -notcontains [string]$field.Value)) {
            $errors.Add("Checkpoint has invalid $key`: $($field.Value)")
        }
    }

    # A present, non-null blocked_reason must be a member of the allowed vocabulary.
    $blocked = Get-OrchestratorStateField -State $State -Name 'blocked_reason'
    if ($blocked.Present -and $null -ne $blocked.Value -and
        ($script:VALID_BLOCKED_REASONS -notcontains [string]$blocked.Value)) {
        $errors.Add("Checkpoint has invalid blocked_reason: $($blocked.Value)")
    }

    return $errors.ToArray()
}

function Get-OrchestratorStatePrCreationReadinessError {
    <#
    .SYNOPSIS
        Return the PR-creation-readiness errors, parity with the Python reference.
    .DESCRIPTION
        Private readiness check mirroring validate_orchestrator_state_pr_creation_readiness in
        _orchestrator_state_pr_creation_readiness.py: steps 5-8 must not be pending, blocked, or
        blocked_remediation_loop_limit; blocked_reason must be `none` or absent; and the
        local_execution_overrides / delegation_bypasses lists must be empty when present. It does
        not enforce completion, CI, PR, or routing-contract gates.
    .PARAMETER State
        The parsed checkpoint PSCustomObject.
    .OUTPUTS
        System.String[] - zero or more error strings; empty when ready for PR creation.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # Reject an upstream step recorded as pending, blocked, or blocked_remediation_loop_limit; steps
    # 5-8 must have finished before the first PR of a branch is created.
    foreach ($key in $script:PR_CREATION_READY_STEP_KEYS) {
        $field = Get-OrchestratorStateField -State $State -Name $key
        if ($field.Present -and (@('pending', 'blocked', 'blocked_remediation_loop_limit') -contains $field.Value)) {
            $errors.Add("Checkpoint PR-creation readiness validation failed: $key is $($field.Value).")
        }
    }

    # blocked_reason must read as clear: absent, null, or the literal 'none'. Any
    # other recorded reason means the branch is not ready for PR creation.
    $blocked = Get-OrchestratorStateField -State $State -Name 'blocked_reason'
    if ($blocked.Present -and $null -ne $blocked.Value -and ([string]$blocked.Value -ne 'none')) {
        $errors.Add('Checkpoint PR-creation readiness validation failed: blocked_reason is not `none`.')
    }

    # Each override list must be empty when present: a present non-list value, or a
    # present non-empty list, means overrides/bypasses were recorded (mirrors the
    # Python `value is not None and (not isinstance(value, list) or value)` guard).
    foreach ($key in $script:PR_CREATION_READY_EMPTY_LIST_KEYS) {
        $field = Get-OrchestratorStateField -State $State -Name $key
        if ($field.Present -and $null -ne $field.Value) {
            $isList = $field.Value -is [System.Array]
            if (-not $isList -or @($field.Value).Count -gt 0) {
                $errors.Add("Checkpoint PR-creation readiness validation failed: $key must be an empty list when present.")
            }
        }
    }

    return $errors.ToArray()
}

function Test-OrchestratorStatePrCreationReadiness {
    <#
    .SYNOPSIS
        Validate a checkpoint is ready for the first `gh pr create` of a branch.
    .DESCRIPTION
        Public entry point consumed by the preflight seam that the pushed-down
        enforce-pr-author-skill hook runs. It is the only implementation: there is no
        alternative branch and no capability detection. Loads the checkpoint
        (fail-closed on missing file / invalid JSON), runs the base-presence check
        (required keys, step-status validity, blocked_reason validity), then runs the
        PR-creation-readiness parity check. Returns a hashtable compatible with the
        hook's existing invoker contract: ExitCode is 1 whenever any error is present,
        and Output carries the newline-joined error text (empty on success).
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

    # Fail closed when the checkpoint cannot be loaded: the load error is the whole
    # output and ExitCode is 1.
    $loaded = Get-OrchestratorStateCheckpoint -CheckpointPath $CheckpointPath
    if (-not $loaded.Ok) {
        return @{ ExitCode = 1; Output = $loaded.Error }
    }

    # Accumulate base-presence errors and readiness errors; any error yields a
    # non-zero ExitCode so the hook blocks PR creation.
    $errors = [System.Collections.Generic.List[string]]::new()
    $errors.AddRange([string[]]@(Get-OrchestratorStateBasePresenceError -State $loaded.State))
    $errors.AddRange([string[]]@(Get-OrchestratorStatePrCreationReadinessError -State $loaded.State))

    if ($errors.Count -gt 0) {
        return @{ ExitCode = 1; Output = ($errors -join [System.Environment]::NewLine) }
    }

    return @{ ExitCode = 0; Output = '' }
}

function Invoke-OrchestratorStatePreflight {
    <#
    .SYNOPSIS
        Runs the orchestrator-state validator against the checkpoint and reports pass/fail.
    .DESCRIPTION
        Shared by the pushed-down enforce-pr-author-skill hook. Mirrors
        Invoke-RoutingContractValidation (.claude/hooks/validate-orchestrator-output.ps1):
        an injectable scriptblock seam whose default runs the portable in-process validation
        and starts no subprocess. As of issue #475 there is no capability detection and no
        alternative branch: the default seam runs the ported unconditional-block (U family)
        checks followed by Test-OrchestratorStatePrCreationReadiness, which together match the
        Python plain-call-plus---require-pr-creation-ready surface. A missing checkpoint or a
        readiness failure both surface via the non-zero exit code and error text; no separate
        file-existence check is made, validating pre-PR-creation readiness (steps 5-8,
        blocked_reason) not full completion.
    .PARAMETER CheckpointPath
        The path to the orchestrator-state checkpoint JSON file. Callers pass their own
        checkpoint-path variable explicitly; the default below is only used when a caller omits
        the parameter.
    .OUTPUTS
        System.Collections.Hashtable with keys HasErrors (bool) and ErrorText (string).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $false)]
        [string] $CheckpointPath = 'artifacts/orchestration/orchestrator-state.json',

        [Parameter(Mandatory = $false)]
        [scriptblock] $Invoker = {
            param($Path)
            # The portable in-process path is the only path. Import the U-family
            # aggregator lazily and only when its function is not already available,
            # so a repeated call (or a test that pre-imports and mocks it) does not
            # reload the module, and so this module does not import a sibling that
            # imports it back at load time.
            if (-not (Get-Command -Name Get-OrchestratorStateUnconditionalError -ErrorAction SilentlyContinue)) {
                Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateUnconditional.psm1') -Force
            }

            # Fail closed when the checkpoint cannot be loaded, before either check runs.
            $loaded = Get-OrchestratorStateCheckpoint -CheckpointPath $Path
            if (-not $loaded.Ok) {
                [pscustomobject]@{ ExitCode = 1; Output = $loaded.Error }
            } else {
                # The unconditional block, then the PR-creation-readiness gate: together
                # the equivalent of the Python plain call plus --require-pr-creation-ready.
                $errors = [System.Collections.Generic.List[string]]::new()
                $errors.AddRange([string[]]@(Get-OrchestratorStateUnconditionalError -State $loaded.State))

                # The readiness entry point re-runs the base-presence checks the
                # unconditional block already ran, so its lines are merged rather than
                # appended wholesale: each error string is emitted exactly once, the same
                # single-emission rule the completion module applies to its M3 leg.
                $readiness = Test-OrchestratorStatePrCreationReadiness -CheckpointPath $Path
                foreach ($line in ([string]$readiness.Output -split "`r?`n")) {
                    if (-not [string]::IsNullOrWhiteSpace($line) -and -not $errors.Contains($line)) {
                        $errors.Add($line)
                    }
                }

                if ($errors.Count -gt 0) {
                    [pscustomobject]@{ ExitCode = 1; Output = ($errors -join [System.Environment]::NewLine) }
                } else {
                    [pscustomobject]@{ ExitCode = 0; Output = '' }
                }
            }
        }
    )

    $result = & $Invoker $CheckpointPath
    # Under this module's Set-StrictMode -Version Latest, member-enumerating .Name directly over
    # a zero-property PSCustomObject throws (a PowerShell strict-mode gotcha not present in the
    # hook's un-strict scope this code was moved from), so the property collection is counted
    # before .Name is ever accessed.
    $resultPropertyNames = @()
    if ($null -ne $result -and @($result.PSObject.Properties).Count -gt 0) {
        $resultPropertyNames = @($result.PSObject.Properties | ForEach-Object { $_.Name })
    }
    $exitCode = 0
    if ($resultPropertyNames -contains 'ExitCode') { $exitCode = [int]$result.ExitCode }
    $outputText = ''
    if ($resultPropertyNames -contains 'Output') { $outputText = ([string]$result.Output).Trim() }

    return @{ HasErrors = ($exitCode -ne 0); ErrorText = $outputText }
}

# Export the public readiness entry point plus the reusable load, field-accessor,
# and base-presence primitives so the sibling OrchestratorStateCompletion module can
# consume them via Import-Module without duplicating the shared parsing and
# base-check logic. Invoke-OrchestratorStatePreflight is exported so the pushed-down
# enforce-pr-author-skill.ps1 hook can consume the PR-creation preflight
# orchestration without duplicating it.
Export-ModuleMember -Function `
    Test-OrchestratorStatePrCreationReadiness, `
    Get-OrchestratorStateCheckpoint, `
    Get-OrchestratorStateField, `
    Get-OrchestratorStateBasePresenceError, `
    Invoke-OrchestratorStatePreflight
