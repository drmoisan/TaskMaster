<#
.SYNOPSIS
    SubagentStop hook for the orchestrator subagent.

.DESCRIPTION
    Blocks termination of the orchestrator subagent unless the orchestration
    checkpoint file at artifacts/orchestration/orchestrator-state.json has been
    updated with current `completed_steps` and `next_step` fields.

    Per the powershell-orchestration-state-machine and atomic-plan-contract
    skills, the orchestrator must persist progress to the checkpoint file
    after every completed step. This hook confirms that:

      - the SubagentStop envelope read from stdin through Read-SubagentStopEnvelope is valid,
      - the orchestrator's final output is non-empty,
      - the checkpoint file exists on disk,
      - the checkpoint is valid JSON,
      - the checkpoint contains the required progress fields:
            objective, completed_steps, next_step, last_updated,
      - the `objective` field is non-empty.

    The checkpoint is resolved through WorktreeRunResolution.psm1 by the dot-sourced
    sibling validate-orchestrator-output-resolution.ps1 (issue #787), so every read uses
    an absolute path beneath the worktree of the run that stopped. An unresolved,
    ambiguous, or mismatched target blocks with ORCHESTRATOR_CHECKPOINT_UNRESOLVED:
    before any checkpoint is read. For epic-orchestrator-state, the Layer 2 wave-barrier
    ordering check (OrchestratorStateEpicWaveBarrier.psm1, issue #840) runs after the
    routing dispatch passes.

.NOTES
    Reads the SubagentStop envelope from stdin through Read-SubagentStopEnvelope
    (subagentstop-envelope.ps1); the assistant text comes from the helper's
    AssistantText. A non-repairable condition blocks once: when StopHookActive is
    set, the re-entered stop is allowed. Exits 0 to allow termination; a block is
    written to stderr with exit 2 through Exit-SubagentStopBlock. Filesystem reads
    go through Get-CheckpointFileContent so tests can mock the boundary without
    writing temporary files. The routing sibling, the resolution sibling, the E1
    helper, and the modules are guarded loads: a failure is recorded and blocks
    every validation, naming the file, before any read. A failed Layer 2 import
    blocks the epic leg only.
    An unhandled error during validation blocks with exit 2 and a reason (issue #928).
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string] $CheckpointPath = 'artifacts/orchestration/orchestrator-state.json',

    [Parameter(Mandatory = $false)]
    [string] $ArtifactType = 'orchestrator-state'
)
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }

Set-StrictMode -Version Latest

try { . (Join-Path $PSScriptRoot 'validate-orchestrator-output-resolution.ps1') } catch { Add-HookDependencyFailure -Name 'validate-orchestrator-output-resolution.ps1' -ErrorRecord $_ }
try { . (Join-Path $PSScriptRoot 'validate-orchestrator-output-routing.ps1') } catch { Add-HookDependencyFailure -Name 'validate-orchestrator-output-routing.ps1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeItemResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'WorktreeItemResolution.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeRunResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'WorktreeRunResolution.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/orchestrator-state/OrchestratorStateEpicWaveBarrier.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'OrchestratorStateEpicWaveBarrier.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/orchestrator-state/OrchestratorStateCompletion.psm1') -Force -Global -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'OrchestratorStateCompletion.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/orchestrator-state/OrchestratorStateUnconditional.psm1') -Force -Global -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'OrchestratorStateUnconditional.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/orchestrator-state/OrchestratorState.psm1') -Force -Global -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'OrchestratorState.psm1' -ErrorRecord $_ }
try { . (Join-Path $PSScriptRoot 'subagentstop-envelope.ps1') } catch { Add-HookDependencyFailure -Name 'subagentstop-envelope.ps1' -ErrorRecord $_ }
$ErrorActionPreference = 'Stop'
$script:SubagentStopReportAndHaltLine = 'This condition is not repairable by the subagent. Report it to the operator and halt.'

function Get-CheckpointFileContent {
    <#
    .SYNOPSIS
        Thin wrapper around the filesystem-read boundary for the checkpoint.
    .DESCRIPTION
        Returns a hashtable with two keys:
          - Exists:  $true when the path resolves to a file on disk.
          - Content: full file text as a single string ($null when missing).
        Tests mock this function to inject checkpoint content without temp files.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return @{ Exists = $false; Content = $null }
    }

    $content = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop
    return @{ Exists = $true; Content = $content }
}

function Test-HumanInteractionShape {
    <#
    .SYNOPSIS
        Validates the optional human_interaction sub-object against the
        invariants documented in .claude/rules/orchestrator-state.md and
        enforced by scripts/dev_tools/validate_orchestrator_state.py,
        enforcing the autonomous-execution mandate at the completion gate.
    .DESCRIPTION
        Returns a hashtable with keys:
          - Ok:      $true if the field is absent or every requirement is resolved.
          - Message: rejection message naming the first unresolved requirement; $null on success.
        A null human_interaction (absent key) passes the gate. When present,
        the requirements array is inspected and DONE is blocked when, in order:
          - requirements is missing or non-array.
          - any requirement has a missing/blank response.
          - any requirement has a response outside the enum
            (scope_change | exception | halt).
          - any requirement has response == 'halt'.
          - any requirement has response == 'exception' with a missing/empty
            runbook_path, or a runbook_path whose file does not exist on disk
            (existence is checked through the injected FileExistsCheck seam).
        FileExistsCheck is an injectable scriptblock so tests can exercise the
        existence branch without writing temporary files. It defaults to
        Test-Path -PathType Leaf.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        $HumanInteraction,

        [Parameter(Mandatory = $false)]
        [scriptblock] $FileExistsCheck = { param($Path) Test-Path -LiteralPath $Path -PathType Leaf }
    )

    if ($null -eq $HumanInteraction) {
        return @{ Ok = $true; Message = $null }
    }

    $hiProps = @($HumanInteraction.PSObject.Properties.Name)
    if ($hiProps -notcontains 'requirements') {
        return @{ Ok = $false; Repairable = $true; Message = "orchestrator hook: 'human_interaction' is present but 'requirements' array is missing." }
    }

    $requirements = @($HumanInteraction.requirements)
    $allowedResponses = @('scope_change', 'exception', 'halt')

    for ($i = 0; $i -lt $requirements.Count; $i++) {
        $req = $requirements[$i]
        $reqProps = @($req.PSObject.Properties.Name)

        $response = $null
        if ($reqProps -contains 'response') { $response = [string]$req.response }

        if ([string]::IsNullOrWhiteSpace($response)) {
            return @{ Ok = $false; Repairable = $true; Message = "orchestrator hook: 'human_interaction.requirements[$i]' has no resolved 'response'. Every unautomatable requirement must resolve to one of: scope_change, exception, halt." }
        }

        if ($allowedResponses -notcontains $response) {
            return @{ Ok = $false; Repairable = $true; Message = "orchestrator hook: 'human_interaction.requirements[$i]' has 'response' value '$response' outside the allowed set (scope_change, exception, halt)." }
        }

        if ($response -eq 'halt') {
            return @{ Ok = $false; Repairable = $false; Message = "orchestrator hook: 'human_interaction.requirements[$i]' has 'response' == 'halt'; DONE is blocked while a halt is present." }
        }

        if ($response -eq 'exception') {
            $runbookPath = $null
            if ($reqProps -contains 'runbook_path') { $runbookPath = [string]$req.runbook_path }

            if ([string]::IsNullOrWhiteSpace($runbookPath)) {
                return @{ Ok = $false; Repairable = $true; Message = "orchestrator hook: 'human_interaction.requirements[$i]' has 'response' == 'exception' but no non-empty 'runbook_path'. A permitted exception requires a runbook." }
            }

            if (-not (& $FileExistsCheck $runbookPath)) {
                return @{ Ok = $false; Repairable = $true; Message = "orchestrator hook: 'human_interaction.requirements[$i]' references runbook_path '$runbookPath' but no file exists at that location." }
            }
        }
    }

    return @{ Ok = $true; Message = $null }
}

function Invoke-OrchestratorOutputValidation {
    <#
    .SYNOPSIS
        Reads the SubagentStop envelope, validates the assistant text, and applies the re-entry policy.
    .DESCRIPTION
        Returns a hashtable with keys:
          - Ok:      $true to allow termination, $false to block.
          - Message: error message when blocking; $null on success.
        SessionRoot is the calling process's path, used only to find the repository
        when the run checkpoint is resolved.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [AllowNull()] [AllowEmptyString()] [string] $InputText,
        [string] $CheckpointPath = 'artifacts/orchestration/orchestrator-state.json',
        [string] $ArtifactType = 'orchestrator-state',
        [string] $SessionRoot = (Get-Location).Path,

        [Parameter(Mandatory = $false)]
        [scriptblock] $RoutingInvoker
    )
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'ORCHESTRATOR_CHECKPOINT_UNRESOLVED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    $envelope = if ($PSBoundParameters.ContainsKey('InputText')) { Read-SubagentStopEnvelope -InputText $InputText } else { Read-SubagentStopEnvelope }
    if (-not $envelope.IsValid) { $decision = @{ Ok = $false; Repairable = $false; Message = ('{0} payload anomaly - {1}' -f 'orchestrator hook:', (Get-SubagentStopEnvelopeAnomalyReason -Anomaly $envelope.Anomaly)) } }
    else {
        $agentOutput = $envelope.AssistantText
        $blockArguments = @{ AgentOutput = ([string]$agentOutput); CheckpointPath = $CheckpointPath; ArtifactType = $ArtifactType; SessionRoot = $SessionRoot }
        if ($PSBoundParameters.ContainsKey('RoutingInvoker')) { $blockArguments['RoutingInvoker'] = $RoutingInvoker }
        $decision = Get-OrchestratorOutputBlockDecision @blockArguments
    }
    if ($decision.Ok) { return @{ Ok = $true; Message = $null } }
    if (-not $decision.Repairable) {
        if ($envelope.StopHookActive) { return @{ Ok = $true; Message = $null } }
        if (-not $decision.Message.Contains('to the operator and halt.')) { $decision.Message = $decision.Message + "`n" + $script:SubagentStopReportAndHaltLine }
    }
    return @{ Ok = $false; Message = $decision.Message }
}

function Get-OrchestratorOutputBlockDecision {
    <#
    .SYNOPSIS
        Applies the orchestrator completion checks to the assistant text and returns the block decision.
    .DESCRIPTION
        Returns @{ Ok = $true; Message = $null } to allow, or a hashtable with Ok = $false, a
        Repairable boolean, and Message. Repairable is $true when the resumed subagent can clear
        the condition (message content, its checkpoint, or its runbook) and $false when it cannot
        (a registration argument, an internal evaluation failure, or a state it may not clear).
        The caller applies the re-entry policy to that flag.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $AgentOutput,
        [string] $CheckpointPath = 'artifacts/orchestration/orchestrator-state.json',
        [string] $ArtifactType = 'orchestrator-state',
        [string] $SessionRoot = (Get-Location).Path,

        [Parameter(Mandatory = $false)]
        [scriptblock] $RoutingInvoker
    )

    if ([string]::IsNullOrWhiteSpace($agentOutput)) {
        return @{ Ok = $false; Repairable = $true; Message = 'orchestrator hook: agent output is empty (last_assistant_message carries no text); orchestrator must report final completion summary before termination.' }
    }

    # Resolve the run checkpoint before any read (issue #787). A failed import, or an
    # unresolved, ambiguous, or mismatched target, blocks here, so nothing is read.
    try {
        $resolution = Resolve-OrchestratorOutputCheckpointPath -ArtifactType $ArtifactType -CheckpointPath $CheckpointPath -AgentOutput ([string]$agentOutput) -SessionRoot $SessionRoot
    }
    catch {
        return @{ Ok = $false; Repairable = $false; Message = ('ORCHESTRATOR_CHECKPOINT_UNRESOLVED: {0}: NoTarget (RESOLVER_FAILED): {1}' -f $ArtifactType, $_.Exception.Message) }
    }
    if (-not $resolution.Resolved) {
        $resolutionRepairable = ($resolution.Status -in @('NoTarget', 'Ambiguous')) -and ($ArtifactType -in @('epic-orchestrator-state', 'parallel-orchestrator-state'))
        return @{ Ok = $false; Repairable = $resolutionRepairable; Message = ('ORCHESTRATOR_CHECKPOINT_UNRESOLVED: {0}: {1} ({2}): {3}' -f
                $ArtifactType, $resolution.Status, $resolution.ReasonCode, $resolution.Detail) }
    }

    $file = Get-CheckpointFileContent -Path $resolution.CheckpointPath
    if (-not $file.Exists) {
        return @{ Ok = $false; Repairable = $true; Message = "orchestrator hook: checkpoint file '$($resolution.CheckpointPath)' does not exist. Orchestrator must persist progress per powershell-orchestration-state-machine before termination." }
    }

    if ([string]::IsNullOrWhiteSpace($file.Content)) {
        return @{ Ok = $false; Repairable = $true; Message = "orchestrator hook: checkpoint file '$($resolution.CheckpointPath)' is empty; cannot validate orchestrator progress." }
    }

    try {
        $checkpoint = $file.Content | ConvertFrom-Json -ErrorAction Stop
    } catch {
        return @{ Ok = $false; Repairable = $true; Message = "orchestrator hook: checkpoint file '$($resolution.CheckpointPath)' is not valid JSON: $($_.Exception.Message)" }
    }

    $requiredFields = @('objective', 'completed_steps', 'next_step', 'last_updated')
    $missingFields = New-Object System.Collections.Generic.List[string]
    $checkpointProps = @($checkpoint.PSObject.Properties.Name)
    foreach ($field in $requiredFields) {
        if ($checkpointProps -notcontains $field) {
            $missingFields.Add($field)
        }
    }

    if ($missingFields.Count -gt 0) {
        $missingList = ($missingFields -join ', ')
        return @{ Ok = $false; Repairable = $true; Message = "orchestrator hook: checkpoint file '$($resolution.CheckpointPath)' is missing required field(s): $missingList. Orchestrator must persist objective, completed_steps, next_step, and last_updated." }
    }

    if ([string]::IsNullOrWhiteSpace([string]$checkpoint.objective)) {
        return @{ Ok = $false; Repairable = $true; Message = "orchestrator hook: checkpoint file '$($resolution.CheckpointPath)' has an empty 'objective' field; orchestrator must record the active objective." }
    }

    $humanInteraction = $null
    if ($checkpointProps -contains 'human_interaction') {
        $humanInteraction = $checkpoint.human_interaction
    }
    # A relative runbook_path is checked beneath the resolved root, not the process location.
    $hiResult = Test-HumanInteractionShape -HumanInteraction $humanInteraction -FileExistsCheck {
        param($Path) Test-OrchestratorOutputRunbookFile -Path (Resolve-OrchestratorOutputRunbookPath -WorktreeRoot $resolution.WorktreeRoot -RunbookPath $Path)
    }
    if (-not $hiResult.Ok) {
        return @{ Ok = $false; Repairable = $hiResult.Repairable; Message = $hiResult.Message }
    }

    # Delegate to the portable routing-contract validation, dispatched on
    # ArtifactType. The optional RoutingInvoker seam lets tests inject a mock; the
    # default seam runs the in-process PowerShell validation and starts no
    # subprocess.
    $routingArgs = @{ CheckpointPath = $resolution.CheckpointPath; ArtifactType = $ArtifactType }
    if ($PSBoundParameters.ContainsKey('RoutingInvoker') -and $null -ne $RoutingInvoker) {
        $routingArgs['Invoker'] = $RoutingInvoker
    }
    $routingResult = Invoke-RoutingContractValidation @routingArgs
    if ($routingResult.HasErrors) {
        # The in-process validation (issue #475) covers both the completion check
        # and the model-routing check in one call. Surface a model-routing gate
        # failure under its own block reason (its errors name model_routing_receipts
        # or complexity_assessments); otherwise fall back to the routing-contract
        # block reason for a generic completion/routing failure.
        if ($routingResult.ErrorText -match 'model_routing_receipts|complexity_assessments') {
            return @{ Ok = $false; Repairable = $true; Message = "MODEL_ROUTING_BLOCKED: $($routingResult.ErrorText)" }
        }
        return @{ Ok = $false; Repairable = $true; Message = "ROUTING_CONTRACT_BLOCKED: $($routingResult.ErrorText)" }
    }

    # Layer 2 (issue #840): the epic checkpoint only, after the routing dispatch passes,
    # on the checkpoint text already read.
    if ($ArtifactType -eq 'epic-orchestrator-state') {
        $barrier = Get-OrchestratorOutputWaveBarrierDecision -CheckpointText $file.Content
        if (-not $barrier.Ok) { return @{ Ok = $false; Repairable = $barrier.Repairable; Message = $barrier.Message } }
    }

    return @{ Ok = $true; Message = $null }
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('ORCHESTRATOR_CHECKPOINT_UNRESOLVED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'ORCHESTRATOR_CHECKPOINT_UNRESOLVED:'
if ($null -ne $dependencyDecision) { [Console]::Error.WriteLine($dependencyDecision.Reason); exit $dependencyDecision.ExitCode }

try { $result = Invoke-OrchestratorOutputValidation -CheckpointPath $CheckpointPath -ArtifactType $ArtifactType }
catch { Exit-SubagentStopBlock -Message ('ORCHESTRATOR_CHECKPOINT_UNRESOLVED: validator error: ' + ([string]$_.Exception.Message -split "`r?`n")[0] + '; the gate fails closed.' + "`n" + $script:SubagentStopReportAndHaltLine) }
if (-not $result.Ok) { Exit-SubagentStopBlock -Message $result.Message }
exit 0
