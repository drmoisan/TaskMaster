<#
.SYNOPSIS
    Checkpoint resolution and Layer 2 decision helpers for validate-orchestrator-output.ps1 (issues #787, #840).

.DESCRIPTION
    Dot-sourced by validate-orchestrator-output.ps1. Resolves the run checkpoint that a
    SubagentStop validation reads through the exported functions of
    WorktreeRunResolution.psm1 and Get-WorktreeItemLiveRoot (WorktreeItemResolution.psm1)
    only, so the hook evaluates the checkpoint of the run that stopped rather than whatever
    file sits at a session-relative path. The payload cwd is never used as a target selector.

    Per artifact type: orchestrator-state resolves the enclosing worktree of the session
    root; epic-orchestrator-state and parallel-orchestrator-state resolve from the
    integration_branch: or parallel_slug: literal in the agent output, and otherwise from the
    single distinct value recorded by the live worktrees' run checkpoints. A bound
    -CheckpointPath is a repository-relative leaf that must compose the canonical checkpoint
    path beneath the resolved root.

    Reads no file except through Get-WorktreeRunCheckpointText and the
    Test-OrchestratorOutputRunbookFile seam. Starts no process and invokes no Python.
#>

# The run kind each supported artifact type resolves as.
$script:OrchestratorOutputRunKind = @{
    'orchestrator-state'          = 'item'
    'epic-orchestrator-state'     = 'epic'
    'parallel-orchestrator-state' = 'parallel'
}

# Reason codes. The first two equal the WorktreeResolution.psm1 accessor values, which the
# sibling suite asserts, because this file calls no accessor of that module.
$script:OrchestratorOutputAmbiguousReasonCode = 'TARGET_WORKTREE_AMBIGUOUS'
$script:OrchestratorOutputNoTargetReasonCode = 'TARGET_WORKTREE_NOT_DERIVABLE'
$script:OrchestratorOutputMismatchReasonCode = 'CHECKPOINT_PATH_MISMATCH'

# The fixed instruction that follows Layer 2 violation lines, and the unevaluable token.
$script:OrchestratorOutputHaltInstruction = 'orchestrator hook: the lines above record a wave-barrier ordering violation in the epic checkpoint that this agent cannot clear. Report them to the operator and halt.'
$script:OrchestratorOutputUnevaluableToken = 'EPIC_WAVE_BARRIER_UNEVALUABLE:'

function ConvertTo-OrchestratorOutputResolution {
    <#
    .SYNOPSIS
        Build the resolution result object.
    .DESCRIPTION
        Returns an object with exactly Resolved, CheckpointPath, WorktreeRoot, Status,
        ReasonCode, and Detail; a value that is not supplied is $null.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [bool] $Resolved,
        [AllowNull()] [object] $CheckpointPath = $null,
        [AllowNull()] [object] $WorktreeRoot = $null,
        [Parameter(Mandatory = $true)] [string] $Status,
        [AllowNull()] [object] $ReasonCode = $null,
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $Detail
    )

    return [pscustomobject]@{
        Resolved       = $Resolved
        CheckpointPath = $CheckpointPath
        WorktreeRoot   = $WorktreeRoot
        Status         = $Status
        ReasonCode     = $ReasonCode
        Detail         = $Detail
    }
}

function Test-OrchestratorOutputCheckpointLeafShape {
    <#
    .SYNOPSIS
        Check that a -CheckpointPath value is a non-empty repository-relative leaf.
    .DESCRIPTION
        Pure. Normalizes the value (trim, forward slashes, no repeated separator, no leading
        ./) and returns @{ Ok; Leaf; Detail }. A blank, rooted, or parent-escaping value is
        not Ok.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $CheckpointPath
    )

    $leaf = ($CheckpointPath.Trim() -replace '\\', '/') -replace '/{2,}', '/'
    while ($leaf.StartsWith('./')) { $leaf = $leaf.Substring(2) }

    # Decision order: blank first, then rooted, then a .. segment; each names the value.
    if ([string]::IsNullOrWhiteSpace($leaf)) {
        return @{ Ok = $false; Leaf = $null; Detail = 'the -CheckpointPath value is empty' }
    }
    if ($leaf -match '^([A-Za-z]:|/)') {
        return @{ Ok = $false; Leaf = $null; Detail = ("the -CheckpointPath value '{0}' is rooted; it must be a repository-relative path" -f $CheckpointPath) }
    }
    if (@($leaf -split '/') -contains '..') {
        return @{ Ok = $false; Leaf = $null; Detail = ("the -CheckpointPath value '{0}' escapes the worktree root" -f $CheckpointPath) }
    }
    return @{ Ok = $true; Leaf = $leaf; Detail = $null }
}

function Test-OrchestratorOutputCheckpointLeaf {
    <#
    .SYNOPSIS
        Cross-check a -CheckpointPath leaf against the canonical checkpoint of a run kind.
    .DESCRIPTION
        Composes the leaf beneath the resolved root and requires an ordinal match with
        Get-WorktreeRunCheckpointPath for the kind. Returns @{ Ok; CanonicalPath; Detail }.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $CheckpointPath,
        [Parameter(Mandatory = $true)] [ValidateSet('epic', 'parallel', 'item')] [string] $Kind,
        [Parameter(Mandatory = $true)] [string] $WorktreeRoot
    )

    $shape = Test-OrchestratorOutputCheckpointLeafShape -CheckpointPath $CheckpointPath
    if (-not $shape.Ok) { return @{ Ok = $false; CanonicalPath = $null; Detail = $shape.Detail } }
    $canonical = Get-WorktreeRunCheckpointPath -Kind $Kind -WorktreeRoot $WorktreeRoot
    $composed = (($WorktreeRoot.Trim() -replace '\\', '/').TrimEnd('/')) + '/' + $shape.Leaf
    if ($composed -ceq $canonical) { return @{ Ok = $true; CanonicalPath = $canonical; Detail = $null } }
    return @{
        Ok            = $false
        CanonicalPath = $canonical
        Detail        = ("the -CheckpointPath value '{0}' composes '{1}' beneath the resolved root, which is not the canonical {2} checkpoint '{3}'" -f
            $CheckpointPath, $composed, $Kind, $canonical)
    }
}

function Find-OrchestratorOutputRunSignalValue {
    <#
    .SYNOPSIS
        Collect the distinct run identity values recorded by the live worktrees' checkpoints.
    .DESCRIPTION
        For kind epic, the integration_branch of every checkpoint with route_id epic; for
        kind parallel, the parallel_slug of every checkpoint with route_id parallel. Blank,
        unparseable, non-object, and off-route checkpoints are skipped. Values are distinct
        under ordinal comparison and kept in discovery order.
    #>
    [CmdletBinding()]
    [OutputType([string[]], [object[]])]
    param(
        [Parameter(Mandatory = $true)] [ValidateSet('epic', 'parallel')] [string] $Kind,
        [Parameter(Mandatory = $true)] [string] $SessionRoot
    )

    $field = if ($Kind -eq 'epic') { 'integration_branch' } else { 'parallel_slug' }
    $route = if ($Kind -eq 'epic') { 'epic' } else { 'parallel' }
    $values = [System.Collections.Generic.List[string]]::new()
    # The seam returns its array as one object, so it is assigned before it is iterated.
    $liveRoots = Get-WorktreeItemLiveRoot -SessionRoot $SessionRoot
    foreach ($root in $liveRoots) {
        if ([string]::IsNullOrWhiteSpace($root)) { continue }
        $text = Get-WorktreeRunCheckpointText -Path (Get-WorktreeRunCheckpointPath -Kind $Kind -WorktreeRoot $root)
        if ([string]::IsNullOrWhiteSpace($text)) { continue }
        try { $parsed = ConvertFrom-Json -InputObject $text -NoEnumerate -ErrorAction Stop } catch { continue }
        if ($parsed -isnot [System.Management.Automation.PSCustomObject]) { continue }
        $names = @($parsed.PSObject.Properties.Name)
        if (-not ($names -contains 'route_id') -or ([string]$parsed.route_id -cne $route)) { continue }
        if (-not ($names -contains $field)) { continue }
        $value = $parsed.$field
        if ($value -isnot [string] -or [string]::IsNullOrWhiteSpace($value)) { continue }
        if (-not $values.Contains($value)) { $values.Add($value) }
    }
    return , $values.ToArray()
}

function Resolve-OrchestratorOutputCheckpointPath {
    <#
    .SYNOPSIS
        Resolve the absolute run checkpoint path for a SubagentStop validation.
    .DESCRIPTION
        Returns the ConvertTo-OrchestratorOutputResolution object. Resolved is $true only
        when exactly one target resolves and the -CheckpointPath leaf composes its canonical
        checkpoint path; otherwise Status, ReasonCode, and Detail name the failure. A
        rejected -CheckpointPath is detected before any resolver call.
    .PARAMETER ArtifactType
        orchestrator-state, epic-orchestrator-state, or parallel-orchestrator-state.
    .PARAMETER CheckpointPath
        The bound repository-relative checkpoint leaf.
    .PARAMETER AgentOutput
        The agent's final output text, scanned for the run identity literals.
    .PARAMETER SessionRoot
        The calling process's path, used only to find the repository.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $ArtifactType,
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $CheckpointPath,
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $AgentOutput,
        [Parameter(Mandatory = $true)] [string] $SessionRoot
    )

    if (-not $script:OrchestratorOutputRunKind.ContainsKey($ArtifactType)) {
        return (ConvertTo-OrchestratorOutputResolution -Resolved $false -Status 'NoTarget' -ReasonCode $script:OrchestratorOutputNoTargetReasonCode -Detail (
                "unsupported artifact type '{0}'; supported types: orchestrator-state, epic-orchestrator-state, parallel-orchestrator-state" -f $ArtifactType))
    }
    $kind = $script:OrchestratorOutputRunKind[$ArtifactType]

    # A malformed leaf is rejected before any resolver call, so nothing is read for it.
    $shape = Test-OrchestratorOutputCheckpointLeafShape -CheckpointPath $CheckpointPath
    if (-not $shape.Ok) {
        return (ConvertTo-OrchestratorOutputResolution -Resolved $false -Status 'Rejected' -ReasonCode $script:OrchestratorOutputMismatchReasonCode -Detail $shape.Detail)
    }

    # Routing table: item resolves the session worktree; epic and parallel take the agent
    # output's literal first and fall back to discovery over the live run checkpoints.
    if ($kind -eq 'item') {
        $target = Resolve-WorktreeOperandTarget -Path '' -SessionRoot $SessionRoot
    }
    else {
        $signal = Find-WorktreeRunIdentitySignal -Text $AgentOutput
        $field = if ($kind -eq 'epic') { 'integration_branch' } else { 'parallel_slug' }
        $identity = if ($kind -eq 'epic') { $signal.IntegrationBranch } else { $signal.ParallelSlug }
        if ([string]::IsNullOrWhiteSpace($identity)) {
            $discovered = [System.Collections.Generic.List[string]]::new()
            foreach ($value in (Find-OrchestratorOutputRunSignalValue -Kind $kind -SessionRoot $SessionRoot)) { $discovered.Add([string]$value) }
            if ($discovered.Count -gt 1) {
                $detail = ("the {0} checkpoints of live worktrees record {1} distinct {2} values ({3}), and the agent output names no {2}: value" -f
                    $kind, $discovered.Count, $field, ($discovered -join ', '))
                return (ConvertTo-OrchestratorOutputResolution -Resolved $false -Status 'Ambiguous' -ReasonCode $script:OrchestratorOutputAmbiguousReasonCode -Detail $detail)
            }
            $identity = if ($discovered.Count -eq 1) { $discovered[0] } else { '' }
        }
        $target = if ($kind -eq 'epic') {
            Resolve-WorktreeEpicTarget -IntegrationBranch $identity -EpicSlug $signal.EpicSlug -SessionRoot $SessionRoot
        }
        else {
            Resolve-WorktreeParallelTarget -ParallelSlug $identity -SessionRoot $SessionRoot
        }
    }

    if ($target.Status -in @('NoTarget', 'Ambiguous')) {
        return (ConvertTo-OrchestratorOutputResolution -Resolved $false -Status $target.Status -ReasonCode $target.ReasonCode -Detail $target.Detail)
    }
    $leaf = Test-OrchestratorOutputCheckpointLeaf -CheckpointPath $CheckpointPath -Kind $kind -WorktreeRoot $target.WorktreeRoot
    if (-not $leaf.Ok) {
        return (ConvertTo-OrchestratorOutputResolution -Resolved $false -Status 'Rejected' -ReasonCode $script:OrchestratorOutputMismatchReasonCode -Detail $leaf.Detail)
    }
    return (ConvertTo-OrchestratorOutputResolution -Resolved $true -CheckpointPath $leaf.CanonicalPath -WorktreeRoot $target.WorktreeRoot -Status $target.Status -Detail $target.Detail)
}

function Resolve-OrchestratorOutputRunbookPath {
    <#
    .SYNOPSIS
        Place a runbook_path beneath the resolved worktree root.
    .DESCRIPTION
        Pure. A rooted value is returned with forward slashes; a relative value has any
        leading ./ removed and is joined beneath the root.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [string] $WorktreeRoot,
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $RunbookPath
    )

    $path = ([string]$RunbookPath).Trim() -replace '\\', '/'
    if ($path -match '^([A-Za-z]:/|/)') { return $path }
    while ($path.StartsWith('./')) { $path = $path.Substring(2) }
    return ((($WorktreeRoot.Trim() -replace '\\', '/').TrimEnd('/')) + '/' + $path.TrimStart('/'))
}

function Test-OrchestratorOutputRunbookFile {
    <#
    .SYNOPSIS
        The runbook existence seam: $true when the path is an existing file.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)] [string] $Path
    )

    return (Test-Path -LiteralPath $Path -PathType Leaf)
}

function Get-OrchestratorOutputWaveBarrierDecision {
    <#
    .SYNOPSIS
        Run the Layer 2 wave-barrier ordering check on epic checkpoint text.
    .DESCRIPTION
        Returns @{ Ok; Message } on success and @{ Ok; Repairable; Message } on a block, with
        Repairable set to $false on every block (the agent cannot clear it, so it blocks once under
        the re-entry policy of issue #903). Violation lines are surfaced unwrapped and followed by
        the fixed report-and-halt instruction. A failed port import, an
        EPIC_WAVE_BARRIER_UNEVALUABLE: error, or any other port failure blocks with a
        message that starts with EPIC_WAVE_BARRIER_UNEVALUABLE:.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $CheckpointText
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    try {
        foreach ($line in (Get-OrchestratorStateEpicWaveBarrierError -CheckpointText $CheckpointText)) { $lines.Add([string]$line) }
    }
    catch {
        $message = $_.Exception.Message
        if ($message.StartsWith($script:OrchestratorOutputUnevaluableToken, [System.StringComparison]::Ordinal)) {
            return @{ Ok = $false; Repairable = $false; Message = $message }
        }
        return @{ Ok = $false; Repairable = $false; Message = ('{0} the wave-barrier ordering check failed: {1}' -f $script:OrchestratorOutputUnevaluableToken, $message) }
    }

    if ($lines.Count -eq 0) { return @{ Ok = $true; Message = $null } }
    $lines.Add($script:OrchestratorOutputHaltInstruction)
    return @{ Ok = $false; Repairable = $false; Message = ($lines -join "`n") }
}
