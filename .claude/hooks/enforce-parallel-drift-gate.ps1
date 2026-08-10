<#
.SYNOPSIS
    Pre-tool-use hook that is the Layer 1 per-call deterrent for the parallel drift gate.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Agent" matcher. Activates only when
    CLAUDE_TOOL_INPUT.subagent_type == "feature-review" and the prompt carries the
    parallel-mode kickoff marker defined in the "Parallel-Mode Kickoff Parameter" section
    of .claude/skills/parallel-orchestrate/SKILL.md, matched byte-for-byte (ordinal).

    Decision procedure: resolve the target item's feature folder from the prompt by scanning
    for a docs/features/active/<token> path, the shape the parallel kickoff contract emits
    for exactly this purpose (adapted from enforce-epic-wave-barrier.ps1's
    Find-EpicWaveBarrierFeatureFolderFromPrompt: longest match wins, a .md-suffixed match
    uses its parent directory); read the parallel checkpoint and locate the items[] record
    whose feature_folder basename matches; derive which item keys have an unresolved latest
    drift event; deny with PARALLEL_DRIFT_GATE_BLOCKED when the resolved item is one of them
    and no synthetic Blocking finding file dated at or after that item's latest drift event has
    been written. Allowed: a non-feature-review target, a prompt without the marker, a resolved
    latest event, and an unresolved event whose finding file is dated at or after it, so the
    R1-R5 remediation review is never deadlocked. A missing or unreadable checkpoint, an
    unresolvable target item, and an unreadable drift log that yields no latest event timestamp
    all deny (fail-closed).

    PRESENCE GATING ONLY: checkpoint-state reads plus exactly one finding-file existence
    check through the finding-presence seam. No git command, no diff computation, and no
    path-glob matching; all path-matching semantics stay in the single Python
    implementation, scripts/dev_tools/parallel_drift_detection.py. The finding-presence check
    additionally requires the matched name's embedded yyyy-MM-ddTHH-mm timestamp to be
    ordinally at or after the current drift event's at, so a finding from an earlier
    remediation cycle does not open the gate; that narrowing is substring extraction plus
    CompareOrdinal over directory entry NAMES and reads no file content.

    Resolution semantics are owned by Python, not reimplemented here. That module's
    unresolved_drift_item_keys derives resolution from two disjuncts: (a) the recorded
    radius widened to cover every escaped path, evaluated with F1's is_path_subsumed glob
    semantics; or (b) the radius was re-recorded from a later observed diff (source ==
    'observed' and computed_at strictly greater than the event's at). This hook implements
    only the narrower check Layer 1 needs -- latest-event selection plus disjunct (b), which
    is ordinal string comparison and needs no glob matcher. Omitting disjunct (a) can only
    report unresolved where Python reports resolved, the fail-closed direction, and the
    finding-file allowance keeps that from deadlocking review. The cross-runtime seam test in
    tests/scripts/claude-hooks/enforce-parallel-drift-gate-helpers.Tests.ps1 runs both runtimes
    over one shared checkpoint-state table and fails when they diverge. Layer 2, the
    retrospective backstop, is the PARALLEL_DRIFT_GATE_VIOLATION invariant in
    scripts/dev_tools/_parallel_orchestrator_state_drift.py.

.NOTES
    PowerShell 7+, no module dependencies. Both read boundaries -- the checkpoint read and the
    finding-file existence check -- are injectable wrapper functions, so tests mock them
    without writing temporary files.

    The eight shape-and-derivation helpers this hook calls live in the dot-sourced sibling
    module .claude/hooks/enforce-parallel-drift-gate-helpers.ps1, together with the
    $script:ObservedRadiusSource and $script:CanonicalTimestampPattern constants they alone
    read. They were split out to restore
    file-size headroom (issue #446 remediation cycle 1, finding F8-N10); the split was a pure
    move with no behaviour change. This file keeps the two read seams, the prompt and item
    resolution, and the decision path.
#>
[CmdletBinding()]
param()

# Dot-source the shape-and-derivation helpers. Guarded so a missing file produces a clear error
# and so dot-sourcing this hook in tests loads the helpers too.
$script:ParallelDriftGateHelpersPath = Join-Path $PSScriptRoot 'enforce-parallel-drift-gate-helpers.ps1'
. $script:ParallelDriftGateHelpersPath

$script:ParallelCheckpointPath = 'artifacts/orchestration/parallel-orchestrator-state.json'
$script:ParallelModeMarker = 'Parallel mode: true'
$script:ReviewSubagentType = 'feature-review'
$script:ActiveFeatureRoot = 'docs/features/active'
$script:FindingFilePrefix = 'remediation-inputs.'
$script:FindingFileSuffix = '.md'

# Character length of the canonical yyyy-MM-ddTHH-mm timestamp a finding file name embeds
# immediately after the prefix. Read only by Test-ParallelDriftFindingPresent, which takes the
# substring at that fixed offset rather than matching a pattern against the path.
$script:FindingFileStampLength = 16

function Get-ParallelDriftGateCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the parallel checkpoint, or $null when the file is absent.
        Tests mock this function (checkpoint-read seam).
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    if (-not (Test-Path -LiteralPath $script:ParallelCheckpointPath -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $script:ParallelCheckpointPath -Raw)
}

function Test-ParallelDriftFindingPresent {
    <#
    .SYNOPSIS
        Report whether the item's synthetic Blocking finding file exists. Tests mock this
        function (finding-presence seam).
    .DESCRIPTION
        The parallel-orchestrator writes the finding as remediation-inputs.<timestamp>.md in
        the child's own active feature folder (flat form), reached through the item's recorded
        worktree_path, which is optional in the schema and may be null: absence reports $false
        so the caller fails closed. Names are compared with ordinal prefix and suffix tests, so
        no glob matcher is involved.

        The finding must correspond to the CURRENT unresolved drift event, not to any earlier
        remediation cycle: a matched name's embedded yyyy-MM-ddTHH-mm timestamp must be
        ordinally greater than or equal to EventAt before $true is reported (issue #446
        remediation cycle 1, finding F8-N3). Before that narrowing, a remediation-inputs file
        written by an unrelated earlier cycle opened the Layer 1 gate for drifted, unsurfaced
        work.

        The narrowing stays PRESENCE GATING ONLY. The timestamp is taken with Substring from
        the fixed offset after the remediation-inputs. prefix and compared with CompareOrdinal.
        There is no path-glob match, no git invocation, and no read of any file's CONTENT; only
        directory entry names are inspected. A name too short to carry the substring, or whose
        substring is not canonically formatted, reports $false, as does a non-canonical EventAt,
        so an unreadable timestamp on either side holds the gate closed.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()][AllowEmptyString()][string] $WorktreePath,
        [AllowNull()][AllowEmptyString()][string] $FeatureFolder,
        [Parameter(Mandatory)][AllowEmptyString()][string] $EventAt
    )

    if ([string]::IsNullOrWhiteSpace($WorktreePath) -or [string]::IsNullOrWhiteSpace($FeatureFolder)) {
        return $false
    }

    # Fail closed on an unusable reference timestamp rather than comparing against it: an
    # ordinal comparison with a differently shaped value is exactly the inversion F8-N4 closed.
    if (-not (Test-ParallelDriftGateCanonicalTimestamp -Value $EventAt)) {
        return $false
    }
    $folder = Join-Path -Path $WorktreePath -ChildPath $script:ActiveFeatureRoot -AdditionalChildPath $FeatureFolder
    if (-not (Test-Path -LiteralPath $folder -PathType Container)) {
        return $false
    }

    # Report on the first remediation-inputs.<timestamp>.md entry whose embedded timestamp is at
    # or after the current event. The timestamp varies per cycle, so the name is matched by
    # ordinal prefix and suffix and the timestamp is read at a fixed offset, not by a pattern.
    $stampOffset = $script:FindingFilePrefix.Length
    $stampLength = $script:FindingFileStampLength
    foreach ($entry in @(Get-ChildItem -LiteralPath $folder -File)) {
        $name = [string]$entry.Name
        if (-not ($name.StartsWith($script:FindingFilePrefix, [System.StringComparison]::Ordinal) -and
                $name.EndsWith($script:FindingFileSuffix, [System.StringComparison]::Ordinal))) {
            continue
        }
        if ($name.Length -lt ($stampOffset + $stampLength)) {
            continue
        }
        $stamp = $name.Substring($stampOffset, $stampLength)
        if (-not (Test-ParallelDriftGateCanonicalTimestamp -Value $stamp)) {
            continue
        }
        if ([string]::CompareOrdinal($stamp, $EventAt) -ge 0) {
            return $true
        }
    }
    return $false
}

function Find-ParallelDriftGateFeatureFolderFromPrompt {
    <#
    .SYNOPSIS
        Scan a delegation prompt for docs/features/active/<...> path tokens and return the
        longest unique match's basename, or $null when none is found.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Prompt)

    if (-not $Prompt) {
        return $null
    }
    $matchList = [regex]::Matches($Prompt, 'docs[\\/]+features[\\/]+active[\\/]+[^\s"''`]+')
    if ($matchList.Count -eq 0) {
        return $null
    }

    # Collect distinct normalized tokens so the longest, most specific one can be selected.
    $unique = @{}
    foreach ($found in $matchList) {
        $unique[($found.Value -replace '\\', '/').TrimEnd('/')] = $true
    }

    $best = @(@($unique.Keys) | Sort-Object -Property Length -Descending)[0]
    if ($best -match '\.md$') {
        $best = $best -replace '/[^/]+\.md$', ''
    }
    return ($best -split '/')[-1]
}

function Find-ParallelDriftGateItemRecord {
    <#
    .SYNOPSIS
        Locate the items[] record whose feature_folder basename equals the target basename
        resolved from the prompt, or $null when none does.
    #>
    [CmdletBinding()]
    param(
        [AllowNull()] $Checkpoint,
        [AllowNull()][string] $FeatureFolder
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($FeatureFolder)) {
        return $null
    }
    if (@($Checkpoint.PSObject.Properties.Name) -notcontains 'items') {
        return $null
    }

    # feature_folder may be recorded as a bare basename or as a full repo-relative path, so
    # compare on the trailing segment of the normalized value.
    foreach ($item in @($Checkpoint.items)) {
        if ($null -eq $item -or -not (Test-ParallelDriftGateText -Value $item.feature_folder)) {
            continue
        }
        $normalized = (([string]$item.feature_folder) -replace '\\', '/').TrimEnd('/')
        if ((($normalized -split '/')[-1]) -ceq $FeatureFolder) {
            return $item
        }
    }
    return $null
}

function Get-ParallelDriftGateAllowDecision {
    <#
    .SYNOPSIS
        Build the PreToolUse allow decision payload.
    #>
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

function Get-ParallelDriftGateBlockDecision {
    <#
    .SYNOPSIS
        Build the PreToolUse deny decision payload carrying the PARALLEL_DRIFT_GATE_BLOCKED
        reason surfaced to the caller.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([Parameter(Mandatory)][string] $Reason)

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

function Invoke-ParallelDriftGateDecision {
    <#
    .SYNOPSIS
        Parse the raw CLAUDE_TOOL_INPUT JSON payload and return an allow-or-block decision.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([string] $ToolInputRaw)

    if (-not $ToolInputRaw) {
        return Get-ParallelDriftGateAllowDecision
    }
    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "enforce-parallel-drift-gate hook received malformed JSON in CLAUDE_TOOL_INPUT: $_"
    }

    # Two cheap disqualifiers first: this gate governs only feature-review delegations, and
    # only under the parallel-mode marker, matched byte-for-byte with an ordinal Contains
    # rather than a wildcard or a culture-sensitive comparison.
    if (([string]$toolInput.subagent_type) -cne $script:ReviewSubagentType) {
        return Get-ParallelDriftGateAllowDecision
    }
    $prompt = [string]$toolInput.prompt
    if (-not $prompt -or -not $prompt.Contains($script:ParallelModeMarker, [System.StringComparison]::Ordinal)) {
        return Get-ParallelDriftGateAllowDecision
    }

    $featureFolder = Find-ParallelDriftGateFeatureFolderFromPrompt -Prompt $prompt
    if (-not $featureFolder) {
        return Get-ParallelDriftGateBlockDecision -Reason 'PARALLEL_DRIFT_GATE_BLOCKED: a parallel-mode feature-review delegation must reference the target item feature folder in the prompt so its drift state can be verified.'
    }

    $checkpointRaw = Get-ParallelDriftGateCheckpointContent
    $checkpoint = $null
    if (-not [string]::IsNullOrWhiteSpace($checkpointRaw)) {
        try {
            $checkpoint = $checkpointRaw | ConvertFrom-Json -ErrorAction Stop
        } catch {
            $checkpoint = $null
        }
    }
    if ($null -eq $checkpoint) {
        return Get-ParallelDriftGateBlockDecision -Reason "PARALLEL_DRIFT_GATE_BLOCKED: the parallel checkpoint '$script:ParallelCheckpointPath' is missing or unreadable, so the drift state of '$featureFolder' cannot be verified."
    }

    $item = Find-ParallelDriftGateItemRecord -Checkpoint $checkpoint -FeatureFolder $featureFolder
    if ($null -eq $item -or -not (Test-ParallelDriftGateItemKey -Value $item.issue_num)) {
        return Get-ParallelDriftGateBlockDecision -Reason "PARALLEL_DRIFT_GATE_BLOCKED: no parallel checkpoint items[] record with a readable issue_num resolves to '$featureFolder', so its drift state cannot be verified."
    }

    # A resolved or never-drifted item is allowed outright. An unresolved item is allowed only
    # once its synthetic Blocking finding exists, so the remediation review that resolves the
    # drift is never deadlocked by this gate.
    $itemKey = [long]$item.issue_num
    $driftState = Get-ParallelDriftGateUnresolvedState -Checkpoint $checkpoint
    if (-not $driftState.Malformed -and ($driftState.UnresolvedItemKeys -notcontains $itemKey)) {
        return Get-ParallelDriftGateAllowDecision
    }

    # The finding must correspond to the CURRENT event, so the allowance needs that event's at.
    # An unreadable log carries no trustworthy timestamp, so no allowance is possible and the
    # gate denies (fail closed) rather than falling back to bare presence.
    $eventAt = ''
    if ($driftState.LatestAt.ContainsKey($itemKey)) {
        $eventAt = [string]$driftState.LatestAt[$itemKey]
    }
    if ($eventAt -and (Test-ParallelDriftFindingPresent -WorktreePath ([string]$item.worktree_path) -FeatureFolder $featureFolder -EventAt $eventAt)) {
        return Get-ParallelDriftGateAllowDecision
    }

    return Get-ParallelDriftGateBlockDecision -Reason "PARALLEL_DRIFT_GATE_BLOCKED: item $itemKey ('$featureFolder') has an unresolved radius drift event and no $script:FindingFilePrefix<timestamp>$script:FindingFileSuffix finding dated at or after that event recorded in its feature folder. The synthetic Blocking finding for the current event must be written before review proceeds, or the drift event log was unreadable."
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $decision = Invoke-ParallelDriftGateDecision -ToolInputRaw $env:CLAUDE_TOOL_INPUT
} catch {
    Write-Error $_
    exit 1
}

$decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

exit 0
