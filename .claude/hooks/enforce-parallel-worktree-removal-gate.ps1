<#
.SYNOPSIS
    Pre-tool-use hook that gates git worktree remove behind parallel checkpoint merge state.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Bash" matcher before any Bash
    command runs. Regex-matches git worktree remove against the envelope's tool_input.command,
    extracts the target worktree path argument, reads
    artifacts/orchestration/parallel-orchestrator-state.json, and finds the items[] record
    whose worktree_path matches. Allows removal only when that record's merge_status is
    merged or worktree_removed. Denies with reason PARALLEL_WORKTREE_REMOVAL_BLOCKED when
    the checkpoint is unreadable, no matching record exists, or merge_status is anything
    else - fail-closed, following the enforce-epic-worktree-removal-gate.ps1 precedent of
    treating an unreadable/no-match checkpoint as deny.

    Adapted from enforce-epic-worktree-removal-gate.ps1. The command interception regexes
    and the path normalization are unchanged; the checkpoint path, the read seam name, and
    the record collection differ, because the parallel surface records per-item state in
    items[] rather than features[]. A parallel run has no integration branch: each item
    opens its own pull request against main, so a removed worktree is unrecoverable work
    unless that item's own merge has been durably confirmed.

    Issue #690: each checkpoint is read beneath the live worktree whose run checkpoint
    records the removal target under worktree_path. An ambiguous target denies before any
    allow; when neither kind resolves, the deny names TARGET_WORKTREE_NOT_DERIVABLE.

.NOTES
    Compatible with PowerShell 7+. Depends on WorktreeRunResolution.psm1 (issue #690),
    imported inside a guard: a failed import is recorded and the decision denies naming
    the module. Filesystem reads go through injectable wrapper functions so tests can
    mock the boundary without writing temporary files.
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }


try { Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'HookPayload.psm1' -ErrorRecord $_ }
# Sanctioned-removal manifest reader (issue #635), consumed by the manifest branch in
# Invoke-ParallelWorktreeRemovalGateDecision. Same module the epic gate imports.
try { Import-Module (Join-Path $PSScriptRoot '../lib/cleanup-manifest/CleanupWorktreeManifest.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'CleanupWorktreeManifest.psm1' -ErrorRecord $_ }
# Shared command-line parser (issue #545), consumed by the scope filter in
# Invoke-ParallelWorktreeRemovalGateDecision and by Resolve-CommandLineInvocationTarget.
# Both call sites are byte-for-byte the calls the epic gate makes, so the duplicated
# concern now has one implementation.
try { . (Join-Path $PSScriptRoot 'hook-command-scanner.ps1') } catch { Add-HookDependencyFailure -Name 'hook-command-scanner.ps1' -ErrorRecord $_ }
try { . (Join-Path $PSScriptRoot 'hook-command-invocation.ps1') } catch { Add-HookDependencyFailure -Name 'hook-command-invocation.ps1' -ErrorRecord $_ }

try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeRunResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'WorktreeRunResolution.psm1' -ErrorRecord $_ }

$script:AllowedMergeStatuses = @('merged', 'worktree_removed')
# Sanctioned-removal manifest location. Recorded here so every hook-read document this
# gate consults is named; the module owns the read itself.
$script:CleanupWorktreeManifestPath = 'artifacts/orchestration/cleanup-worktrees-manifest.json'

function Get-ParallelWorktreeRemovalGateCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the parallel checkpoint. Tests mock this function
        (read seam).
    .PARAMETER Path
        The absolute checkpoint path composed beneath the resolved worktree root.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][ValidatePattern('^([A-Za-z]:[\\/]|/)')][string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $Path -Raw)
}

function Get-ParallelWorktreeRemovalGateEpicCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the epic checkpoint. Tests mock this function
        (read seam, issue #688).
    .DESCRIPTION
        A separate seam from the parallel checkpoint reader so a test can drive the
        two documents independently: the epic branch must be exercised with the
        parallel checkpoint absent, present-but-not-covering, and covering. An epic run
        records per-child state in features[] and never writes the parallel checkpoint,
        so without this read every merged epic child worktree reached the deny below.
    .PARAMETER Path
        The absolute checkpoint path composed beneath the resolved worktree root.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][ValidatePattern('^([A-Za-z]:[\\/]|/)')][string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $Path -Raw)
}

function Resolve-ParallelWorktreeGateRunTarget {
    <#
    .SYNOPSIS
        Resolve the worktree whose run checkpoint records a worktree path (issue #690 seam).
    .PARAMETER Kind
        epic or parallel.
    .PARAMETER WorktreePath
        The removal target the command names.
    .OUTPUTS
        System.Management.Automation.PSCustomObject (the worktree-resolution target result).
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][ValidateSet('epic', 'parallel')][string] $Kind,
        [AllowNull()][AllowEmptyString()][string] $WorktreePath
    )

    return Resolve-WorktreeRunTargetByRecord -Kind $Kind -RecordField worktree_path -Value $WorktreePath -SessionRoot (Get-Location).Path
}

function Read-ParallelWorktreeGateRunCheckpoint {
    <#
    .SYNOPSIS
        Resolve one run kind and read its checkpoint beneath the resolved root.
    .PARAMETER Kind
        epic or parallel.
    .PARAMETER WorktreePath
        The removal target the command names.
    .OUTPUTS
        System.Management.Automation.PSCustomObject with Target and Checkpoint ($null when
        unresolved, absent, or unparseable).
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][ValidateSet('epic', 'parallel')][string] $Kind,
        [AllowNull()][AllowEmptyString()][string] $WorktreePath
    )

    $target = Resolve-ParallelWorktreeGateRunTarget -Kind $Kind -WorktreePath $WorktreePath
    $checkpoint = $null
    if ($target.Status -eq 'SessionRoot' -or $target.Status -eq 'OtherWorktree') {
        $path = Get-WorktreeRunCheckpointPath -Kind $Kind -WorktreeRoot $target.WorktreeRoot
        $raw = if ($Kind -eq 'parallel') { Get-ParallelWorktreeRemovalGateCheckpointContent -Path $path } else { Get-ParallelWorktreeRemovalGateEpicCheckpointContent -Path $path }
        if (-not [string]::IsNullOrWhiteSpace($raw)) {
            try {
                $checkpoint = $raw | ConvertFrom-Json -ErrorAction Stop
            } catch {
                $checkpoint = $null
            }
        }
    }
    return [pscustomobject]@{
        Target     = $target
        Checkpoint = $checkpoint
    }
}

function Find-ParallelWorktreeItemRecord {
    <#
    .SYNOPSIS
        Locate the record whose worktree_path matches the target path.
    .PARAMETER Checkpoint
        Parsed checkpoint, or $null when absent/unreadable.
    .PARAMETER WorktreePath
        The target worktree path extracted from the command text.
    .PARAMETER RecordArrayName
        Name of the record array to scan. Defaults to 'items', the parallel
        checkpoint's array, so every existing caller is unchanged; the epic branch
        added for issue #688 passes 'features'. Parameterized rather than duplicated
        so both topologies share one path-normalization implementation.
    .OUTPUTS
        System.Object or $null
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $WorktreePath,

        [ValidateNotNullOrEmpty()]
        [string] $RecordArrayName = 'items'
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($WorktreePath)) {
        return $null
    }
    $checkpointProps = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains $RecordArrayName) {
        return $null
    }

    $normalizedTarget = ($WorktreePath -replace '\\', '/').TrimEnd('/')

    # Scan every recorded item for a worktree_path that matches the removal target;
    # path separators are normalized so Windows- and POSIX-style paths compare equal.
    foreach ($item in @($Checkpoint.$RecordArrayName)) {
        $itemProps = @($item.PSObject.Properties.Name)
        if ($itemProps -notcontains 'worktree_path') {
            continue
        }
        $normalizedItemPath = (([string]$item.worktree_path) -replace '\\', '/').TrimEnd('/')
        if ($normalizedItemPath -eq $normalizedTarget) {
            return $item
        }
    }
    return $null
}

function Test-ParallelWorktreeRemovalAllowed {
    <#
    .SYNOPSIS
        Decision logic: allow only when the matching item record's merge_status is
        merged or worktree_removed.
    .PARAMETER ItemRecord
        The matched items[] record, or $null when no match was found.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $ItemRecord
    )

    if ($null -eq $ItemRecord) {
        return $false
    }
    $props = @($ItemRecord.PSObject.Properties.Name)
    if ($props -notcontains 'merge_status') {
        return $false
    }
    return $script:AllowedMergeStatuses -contains ([string]$ItemRecord.merge_status)
}

function Get-ParallelWorktreeGateAllowDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param()

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName      = 'PreToolUse'
            permissionDecision = 'allow'
        }
    }
}

function Get-ParallelWorktreeGateBlockDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Reason
    )

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

function Invoke-ParallelWorktreeRemovalGateDecision {
    <#
    .SYNOPSIS
        Parses the PreToolUse envelope and returns an allow-or-block decision.
    .PARAMETER ToolInputRaw
        The raw JSON tool payload supplied by Claude Code.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw
    )
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'PARALLEL_WORKTREE_REMOVAL_BLOCKED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-ParallelWorktreeGateBlockDecision -Reason (
            'PARALLEL_WORKTREE_REMOVAL_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $commandText = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'command'
    if (-not $commandText) {
        return Get-ParallelWorktreeGateAllowDecision
    }

    # Scope filter. The test is structural, so a relocating spelling such as
    # 'git -C <dir> worktree remove <path>' is now in scope and quoted prose that merely
    # mentions the phrase is not (issue #545). This is the same call the epic gate makes.
    if (-not (Test-CommandLineInvocation -CommandText $commandText -CommandWord 'git' -SubcommandPath @('worktree', 'remove'))) {
        return Get-ParallelWorktreeGateAllowDecision
    }

    # Issue #824: the removal targets are derived structurally from every invocation.
    # A target that cannot be derived denies before any checkpoint is read, and every
    # derived target must be authorized on its own.
    $resolution = Resolve-CommandLineInvocationTarget -CommandText $commandText -CommandWord 'git' -SubcommandPath @('worktree', 'remove')
    if ($resolution.Status -eq 'NoMatch') {
        return Get-ParallelWorktreeGateAllowDecision
    }
    if ($resolution.Status -ne 'Targets') {
        return Get-ParallelWorktreeGateBlockDecision -Reason 'PARALLEL_WORKTREE_REMOVAL_BLOCKED: TARGET_WORKTREE_NOT_DERIVABLE: the git worktree remove target cannot be derived from the command text. No checkpoint can authorize this removal.'
    }
    foreach ($target in @($resolution.Targets)) {
        $denial = Get-ParallelWorktreeRemovalTargetDenial -WorktreePath $target
        if ($null -ne $denial) {
            return $denial
        }
    }
    return Get-ParallelWorktreeGateAllowDecision
}

function Get-ParallelWorktreeRemovalTargetDenial {
    <#
    .SYNOPSIS
        Evaluate one derived removal target and return its deny decision, or $null when authorized.
    .DESCRIPTION
        Holds the per-path authorization cascade (parallel checkpoint, epic checkpoint,
        sanctioned-removal manifest) for a single worktree path (issue #824).
    .PARAMETER WorktreePath
        One removal target derived by Resolve-CommandLineInvocationTarget.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary or $null
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $WorktreePath
    )

    # Each run checkpoint is read beneath the worktree that records this path (issue #690),
    # parallel kind first; an ambiguous target denies before any allow or manifest check.
    $parallelRead = Read-ParallelWorktreeGateRunCheckpoint -Kind parallel -WorktreePath $worktreePath
    $epicRead = Read-ParallelWorktreeGateRunCheckpoint -Kind epic -WorktreePath $worktreePath
    foreach ($read in @($parallelRead, $epicRead)) {
        if ($read.Target.Status -eq 'Ambiguous') {
            return Get-ParallelWorktreeGateBlockDecision -Reason "PARALLEL_WORKTREE_REMOVAL_BLOCKED: $($read.Target.ReasonCode): $($read.Target.Detail)"
        }
    }
    $checkpoint = $parallelRead.Checkpoint

    $itemRecord = Find-ParallelWorktreeItemRecord -Checkpoint $checkpoint -WorktreePath $worktreePath
    if (Test-ParallelWorktreeRemovalAllowed -ItemRecord $itemRecord) {
        return $null
    }

    # Epic-authorization branch (issue #688). It runs after the parallel allow above and
    # before the manifest branch below, so neither of those decisions changes. An epic run
    # records per-child state in the epic checkpoint's features[] array and never writes the
    # parallel checkpoint, so before this branch every merged epic child worktree fell
    # through to the deny even though enforce-epic-worktree-removal-gate.ps1 authorized the
    # same call from that same record. The merge_status predicate is the one already used
    # for parallel items, so the two topologies cannot drift apart on what counts as safe.
    #
    # Fail-closed is preserved: an absent, unreadable, or malformed epic checkpoint parses to
    # $null, which yields no record, which is not allowed.
    $epicCheckpoint = $epicRead.Checkpoint

    $featureRecord = Find-ParallelWorktreeItemRecord -Checkpoint $epicCheckpoint -WorktreePath $worktreePath -RecordArrayName 'features'
    if (Test-ParallelWorktreeRemovalAllowed -ItemRecord $featureRecord) {
        return $null
    }

    # Sanctioned-removal manifest branch (issue #635). It runs last, so a
    # checkpoint-authorized removal still allows at the same decision point it does
    # today and no transcript attribution changes.
    #
    # The coverage test is a PRESENCE test, deliberately not an authorization test. A
    # target this checkpoint records at all is excluded from this branch regardless of
    # that record's merge_status, so a removal the checkpoint does not authorize still
    # reaches the unchanged deny below. This gate defines only the parallel checkpoint
    # seam and has no epic-checkpoint seam, so its exclusion covers the items array
    # only; the epic gate covers the features array with its own exclusion.
    if (-not (Test-CleanupManifestCheckpointCoversPath -Checkpoint $checkpoint -RecordArrayName 'items' -WorktreePath $worktreePath) -and
        (Test-CleanupWorktreeManifestAuthorizesRemoval -WorktreePath $worktreePath)) {
        return $null
    }

    # When neither kind resolved, the deny names the resolution reason first (issue #690).
    $prefix = ''
    if ($parallelRead.Target.Status -eq 'NoTarget' -and $epicRead.Target.Status -eq 'NoTarget') {
        $prefix = "$($parallelRead.Target.ReasonCode): $($parallelRead.Target.Detail). "
    }
    return Get-ParallelWorktreeGateBlockDecision -Reason ('PARALLEL_WORKTREE_REMOVAL_BLOCKED: ' + $prefix + "git worktree remove for '$worktreePath' requires a matching parallel checkpoint items[] record with merge_status in {merged, worktree_removed}. The checkpoint was unreadable, no matching record was found, or merge_status was not yet safe for removal.")
}

function Invoke-ParallelWorktreeRemovalGateEntryPoint {
    <#
    .SYNOPSIS
        Runs the hook decision and returns the process exit code.
    .DESCRIPTION
        Acquires the payload through the shared reader unless the caller supplies
        one, emits the compact decision JSON, and returns 0. It never returns 1:
        exit 1 is non-blocking for PreToolUse, so every anomaly is already a deny
        decision by the time control reaches here. The function does not call exit;
        the thin tail converts the returned code into a process exit.
    .PARAMETER ToolInputRaw
        Optional pre-acquired payload text. When omitted the ReadPayload seam runs.
    .PARAMETER ReadPayload
        Seam for payload acquisition, so tests can drive the empty-on-all-transports
        case without touching a console.
    .OUTPUTS
        System.Int32
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw,

        [scriptblock] $ReadPayload = { Read-ClaudeHookRawPayload }
    )

    if (-not $PSBoundParameters.ContainsKey('ToolInputRaw')) {
        $ToolInputRaw = [string](& $ReadPayload)
    }

    $decision = Invoke-ParallelWorktreeRemovalGateDecision -ToolInputRaw $ToolInputRaw
    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

    return 0
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('PARALLEL_WORKTREE_REMOVAL_BLOCKED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'PARALLEL_WORKTREE_REMOVAL_BLOCKED:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

# The entry point returns its [int] exit code as the last pipeline element and the
# decision JSON before it. `exit (<call>)` would capture BOTH into the exit
# expression and emit nothing, so the decision is written explicitly here first.
$entryPointResult = @(Invoke-ParallelWorktreeRemovalGateEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])