<#
.SYNOPSIS
    Routing-contract dispatch for the orchestrator SubagentStop hook validate-orchestrator-output.ps1 (issue #903).

.DESCRIPTION
    Dot-sourced by validate-orchestrator-output.ps1 through a guarded load whose catch records the
    failure with Add-HookDependencyFailure, so a missing or broken copy of this file blocks the
    hook instead of failing open. It defines Test-OrchestratorCheckpointStructure and
    Invoke-RoutingContractValidation, moved unchanged from the main hook file to keep that file
    under the 500-line cap.

    The default Invoker of Invoke-RoutingContractValidation resolves
    ../lib/orchestrator-state/OrchestratorStateCompletion.psm1 from $PSScriptRoot, which is the
    same directory for this dot-sourced file as for the main hook. The file starts no process
    and invokes no Python.
#>

function Test-OrchestratorCheckpointStructure {
    <#
    .SYNOPSIS
        Type-scoped structural check for the epic and parallel checkpoint types.
    .DESCRIPTION
        PD-3 implementation. The Python reference exposes no validation surface for
        `epic-orchestrator-state` or `parallel-orchestrator-state` under this hook's
        flag pair: argparse rejects the pair and exits 2 without running a single
        check, so parity is UNDEFINED in this region. Rather than inherit an
        undefined behavior, this hook defines it: the checkpoint must exist, parse
        as JSON, and have an object root. That is the largest assertion that holds
        for every checkpoint type without importing a schema this hook does not own.

        Deliberately NOT applied here:
          - the standard-checkpoint REQUIRED_STATE_KEYS presence block, whose key
            set belongs to the standard checkpoint and would produce false
            ROUTING_CONTRACT_BLOCKED verdicts against a well-formed epic or
            parallel checkpoint (defect D-1),
          - the model-routing gate, whose receipts live on the standard checkpoint.

        The check fails closed: a missing file, unreadable content, invalid JSON, or
        a non-object root all yield ExitCode 1 with the load error as Output.
    .PARAMETER CheckpointPath
        The path to the checkpoint JSON file.
    .OUTPUTS
        System.Collections.Hashtable with keys ExitCode (int, 0 or 1) and Output (string).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $CheckpointPath
    )

    # Get-OrchestratorStateCheckpoint already implements exactly the three
    # structural conditions (exists, parses, object root) and reports each as a
    # fail-closed Error string, so the structural leg reuses it rather than
    # duplicating the load contract.
    $loaded = Get-OrchestratorStateCheckpoint -CheckpointPath $CheckpointPath
    if (-not $loaded.Ok) {
        return @{ ExitCode = 1; Output = $loaded.Error }
    }

    return @{ ExitCode = 0; Output = '' }
}

function Invoke-RoutingContractValidation {
    <#
    .SYNOPSIS
        Runs the portable routing-contract validation against the on-disk
        checkpoint and reports whether it emitted errors.
    .DESCRIPTION
        Invokes the validation through an injectable scriptblock seam. As of issue
        #475 the default Invoker names no interpreter and starts no subprocess: the
        portable PowerShell path is the ONLY path, so the hook behaves identically
        in this repository and in every destination that received only the
        pushed-down `.claude` pack. The former capability-detection probe and the
        interpreter-subprocess leg it guarded are both gone.

        The default Invoker dispatches on ArtifactType:

          orchestrator-state
              the COMPLETE-PARITY completion validation
              (Test-OrchestratorStateCompletionReadiness), a row-by-row port of the
              Python call surface `--require-complete --require-model-routing`.

          epic-orchestrator-state, parallel-orchestrator-state
              the type-scoped structural check
              (Test-OrchestratorCheckpointStructure): exists, parses as JSON,
              object root. PD-3: this is DEFINED behavior in a region where Python
              parity is UNDEFINED (argparse exit 2, zero checks run). It is a
              design decision, not a deferral. The standard-checkpoint
              REQUIRED_STATE_KEYS block and the model-routing gate are deliberately
              not applied, which is the D-1 fix.

          anything else
              fail closed, naming the unsupported type. An unrecognized type must
              never read as a clean pass.

        ArtifactType defaults to 'orchestrator-state' so every existing caller of
        this hook keeps its current behavior.

        Returns a hashtable with keys:
          - HasErrors:  $true only when the validation reported a non-zero exit
                        code; $false when it exited 0. The exit code is the sole
                        discriminator.
          - ErrorText:  the validation's output text, carried through unchanged:
                        the error lines on a failure, empty on a clean pass.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $CheckpointPath,

        [Parameter(Mandatory = $false)]
        [string] $ArtifactType = 'orchestrator-state',

        [Parameter(Mandatory = $false)]
        [scriptblock] $Invoker = {
            param($Path, $Type)
            switch ($Type) {
                'orchestrator-state' {
                    # Import the portable completion module only when its function is not
                    # already available, so a repeated call (or a test that pre-imports and
                    # mocks the function) does not reload the module and reset the seam.
                    if (-not (Get-Command -Name Test-OrchestratorStateCompletionReadiness -ErrorAction SilentlyContinue)) {
                        Import-Module (Join-Path $PSScriptRoot '../lib/orchestrator-state/OrchestratorStateCompletion.psm1') -Force
                    }
                    $portable = Test-OrchestratorStateCompletionReadiness -CheckpointPath $Path
                    [pscustomobject]@{
                        ExitCode = $portable.ExitCode
                        Output   = $portable.Output
                    }
                }
                { $_ -in @('epic-orchestrator-state', 'parallel-orchestrator-state') } {
                    # PD-3: defined fail-closed structural behavior in an
                    # undefined-parity region. See Test-OrchestratorCheckpointStructure.
                    $structural = Test-OrchestratorCheckpointStructure -CheckpointPath $Path
                    [pscustomobject]@{
                        ExitCode = $structural.ExitCode
                        Output   = $structural.Output
                    }
                }
                default {
                    # Fail closed on an unwired type: an unrecognized artifact type is
                    # not a clean pass.
                    [pscustomobject]@{
                        ExitCode = 1
                        Output   = "orchestrator hook: unsupported artifact type '$Type'; no validation surface is wired for it. Supported types: orchestrator-state, epic-orchestrator-state, parallel-orchestrator-state."
                    }
                }
            }
        }
    )

    $result = & $Invoker $CheckpointPath $ArtifactType
    $exitCode = 0
    if ($null -ne $result -and ($result.PSObject.Properties.Name -contains 'ExitCode')) {
        $exitCode = [int]$result.ExitCode
    }
    $outputText = ''
    if ($null -ne $result -and ($result.PSObject.Properties.Name -contains 'Output')) {
        $outputText = ([string]$result.Output).Trim()
    }

    # The exit code is the complete failure discriminator: every dispatch leg
    # returns a non-zero ExitCode with its error text on failure and ExitCode 0
    # with empty Output on a clean pass, so output text must not influence this
    # decision.
    $hasErrors = ($exitCode -ne 0)
    return @{ HasErrors = $hasErrors; ErrorText = $outputText }
}
