<#
.SYNOPSIS
    Pre-tool-use hook that blocks atomic-planner delegations when the target
    feature folder does not yet contain the prd-feature outputs its persisted
    work mode requires.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the Agent (Task) tool. Reads
    the tool payload through the shared hook-payload reader (stdin first, with
    environment-variable fallback) and takes the tool arguments from the
    envelope's nested tool_input object. Activates only when subagent_type is
    'atomic-planner'.

    Feature folder resolution order:
      1. Resolve the worktree this call pertains to from the assembled prompt and
         description text, by portable identity only: the canonical issue-number
         line, and a branch signal. The resolution belongs to the
         worktree-resolution library and is not re-implemented here. No path
         signal selects a worktree, because a merged feature folder exists in
         every checkout branched from main and so places a call in many at once.
         When the call carries no identity, or an identity the library cannot
         narrow to one live worktree, the gate denies at once with that library's
         own reason code -- the no-target code or the ambiguity code -- before any
         document probe runs. A probe at that point would validate the call
         against whichever root happened to be current.
      2. Scan the prompt text for any path matching
         docs/features/active/<token>, accepting both forward-slash and
         backslash separators. Truncate every match to two segments past the
         docs/features/active/ prefix -- that is, to exactly four path segments:
         docs, features, active, and the feature-folder name. Truncation is
         depth-insensitive, so the feature folder itself, a spec.md path, a
         research/ artifact path, and an evidence/ artifact path all resolve to
         the same folder. A match that truncates to fewer than four segments is
         rejected. Candidates are deduplicated preserving first-occurrence
         order.
      3. Choose among the distinct candidates by asking which one the resolved
         worktree's own orchestrator checkpoint names. That checkpoint is the only
         disambiguator, and neither prompt position nor a foreign session's record
         takes that role: position would make selection an accident of wording,
         and another session's checkpoint records its own unrelated work.
      4. When more than one folder was cited and that checkpoint names none of
         them, the tie is unresolved and the gate denies with the ambiguity
         reason code rather than selecting any one of the candidates.
      5. When the prompt named no folder at all, the resolved worktree's
         checkpoint supplies one. It is the item's own checkpoint by
         construction, because identity selected the worktree it sits in, so the
         conditional guard that used to protect this step is unnecessary and has
         been removed.
      6. If no folder is resolved by any of the steps above, block with a reason
         instructing the caller to reference a feature folder explicitly.
      7. Once a folder is resolved, anchor every document probe to the resolved
         target root, and only when that root differs from the session root. When
         the two coincide the bare repo-relative spelling is kept, because
         prefixing unconditionally would break every call that legitimately
         resolves where it runs. A folder that is absent under the resolved target
         root therefore denies on the missing-document reason for that root. It
         never denies on the marker-is-broken reason, which is reachable only when
         the folder does exist under that root and its work-mode marker cannot be
         read.

    Known limitation: resolution stops at the feature-folder segment, so it does
    not descend into a version folder (v1/, v2/). No versioned folder exists
    under docs/features/active/ today, and issue.md sits at the feature root in
    every case, so the limitation is inert; it is recorded here rather than coded
    around.

    Once the folder is resolved, the hook reads the persisted work-mode marker
    (`- Work Mode: minor-audit|full-feature|full-bug|full`) from that folder's
    issue.md, per the mode contract in
    .claude/skills/feature-promotion-lifecycle/SKILL.md, and derives the
    required prerequisite set:
      - full-feature -> spec.md and user-story.md are both required.
      - full-bug     -> spec.md only is required.
      - minor-audit  -> neither is required; issue.md carries the acceptance
                        criteria for this mode.
      - marker absent, unreadable, or unrecognized -> deny on a distinct
        decision path that names the resolved folder and the issue.md path it
        probed and states adding or correcting the marker as the remedy. That
        path does not run the required-file probe and names neither prerequisite
        document, because when the mode is unknown no prerequisite set is
        knowable: a set containing user-story.md cannot be satisfied by full-bug
        or minor-audit work without violating the lifecycle contract, and the
        empty set would fail open. The delegation is still denied, so the gate
        remains fail-closed. The legacy `full` marker normalizes to
        full-feature's requirement set.

    If any required file is missing, the script emits a PreToolUse JSON
    response with hookSpecificOutput.permissionDecision='deny' and a reason
    naming the missing file(s) and instructing the orchestrator to invoke
    prd-feature first. Allowed delegations emit
    hookSpecificOutput.permissionDecision='allow'.

    Filesystem reads and orchestrator-state lookups go through wrapper functions
    so tests can inject fakes without touching disk.

.NOTES
    Compatible with PowerShell 7+. Read-only validation gate.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
# Issue #669 owns worktree location and path normalisation; issue #673 owns the
# portable-identity resolution this gate now selects a worktree with. Both imports are
# unguarded on purpose: a resolution module that cannot be loaded is itself the
# target-not-resolvable state, and the gate must fail closed on it rather than degrade to
# a permissive path. Both are required rather than one: the absolute-path join the probe
# step below calls is exported only by the first, because a module whose exports are pinned
# by an explicit list does not re-export a sibling's functions to its own importer.
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeTargetResolution.psm1') -Force
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeItemResolution.psm1') -Force
. (Join-Path $PSScriptRoot 'enforce-prd-feature-before-planner-helpers.ps1')
function Get-PrdFeatureFileExistence {
    <#
    .SYNOPSIS
        Wrapper around Test-Path for sibling-file existence checks. Tests mock this.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    return [bool](Test-Path -LiteralPath $Path -PathType Leaf)
}

function Get-PrdFeatureIssueContent {
    <#
    .SYNOPSIS
        Wrapper around Get-Content for a feature folder's issue.md. Tests mock
        this directly to inject work-mode marker content without touching disk.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $FeatureFolder
    )

    $issuePath = "$FeatureFolder/issue.md"
    if (-not (Test-Path -LiteralPath $issuePath -PathType Leaf)) {
        return $null
    }

    try {
        return Get-Content -LiteralPath $issuePath -Raw -ErrorAction Stop
    }
    catch {
        return $null
    }
}

function Get-PrdFeatureCheckpointFolder {
    <#
    .SYNOPSIS
        Returns the feature-folder field from the orchestrator checkpoint, or
        $null when the file or field is absent.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $CheckpointPath
    )

    if (-not (Test-Path -LiteralPath $CheckpointPath -PathType Leaf)) {
        return $null
    }

    try {
        $raw = Get-Content -LiteralPath $CheckpointPath -Raw -ErrorAction Stop
        $obj = $raw | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        return $null
    }

    if ($obj.PSObject.Properties.Name -contains 'feature-folder' -and $obj.'feature-folder') {
        return [string]$obj.'feature-folder'
    }
    return $null
}

function Get-PrdFeatureTargetDecision {
    <#
    .SYNOPSIS
        Builds a target-resolution deny, carrying the caller's reason code.
    .DESCRIPTION
        The code is supplied by the caller rather than chosen here, because two are
        now reachable: the no-target code when the call carries no identity or an
        identity that places in no live worktree, and the ambiguity code when
        several live worktrees match or two identities disagree. Both are read from
        the resolution library's accessors, so the gate and the library can never
        disagree about either spelling.

        Every reason built here is distinct from the missing-document and marker
        reasons, which name a resolved folder rather than a resolution failure.

        The remedy sentence names the two identity lines rather than an absolute
        path. Citing a path is no longer an action that selects anything, so the
        previous remedy would have instructed the caller to do something inert.
    .PARAMETER ReasonCode
        The resolution reason code to carry, from one of the library's accessors.
    .PARAMETER Detail
        The library's own detail text for the unresolved state.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $ReasonCode,

        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Detail
    )

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = "PRD_FEATURE_BLOCKED: $ReasonCode - $Detail. " +
            "Put the line 'Canonical issue number for this feature is <N>.' and a " +
            "'branch: <item branch>' label in the delegation prompt so the gate can identify the item."
        }
    }
}

function Resolve-PrdFeatureWorktreeTarget {
    <#
    .SYNOPSIS
        Resolve the delegation's target worktree. Tests mock this function (seam).
    .DESCRIPTION
        A seam rather than a direct call because the resolver reads real git state: the
        worktree list, each registration's branch, and the filesystem. A test that drove
        it directly would depend on whichever worktrees exist on the machine running it.
    .PARAMETER Text
        The assembled prompt and description text to read identity from.
    .OUTPUTS
        The target result object from Resolve-WorktreeItemTarget.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Text
    )

    return (Resolve-WorktreeItemTarget -Text $Text -SessionRoot (Get-Location).Path)
}

function Get-PrdFeatureCallTarget {
    <#
    .SYNOPSIS
        Resolves the worktree this tool call pertains to, or $null when the call
        supplies no text to resolve from.
    .DESCRIPTION
        The resolution itself belongs to the worktree-resolution library and is not
        re-implemented here: this function only assembles the text it reads, from the
        envelope root and the nested tool_input, and supplies the session root from
        the envelope's own cwd field when the runtime sets one.

        The assembled text is handed to the derivation unconditionally. Which
        citations are eligible to be placed is the derivation's decision and not
        this hook's: it answers NoTarget for a call it cannot place at all, and
        Ambiguous for a placed signal that resolves to no worktree or to several.
        A repo-relative citation therefore has a real placement channel, because
        the derivation keeps only the worktrees under which that repo-relative
        path exists. The outcome the caller sees is fail-closed:
        a call that places in no worktree, or in several, denies before any probe
        runs, rather than being validated against whichever root happened to be
        current.
    #>
    [CmdletBinding()]
    [OutputType([object])]
    param(
        [AllowNull()]
        [object] $ToolInput
    )

    $prompt = Get-ClaudeHookToolInputString -ToolInput $ToolInput -Name 'prompt'
    $description = Get-ClaudeHookToolInputString -ToolInput $ToolInput -Name 'description'
    $text = (@($prompt, $description) | Where-Object { $_ }) -join ' '

    # An empty call resolves nothing and, importantly, performs no enumeration and no
    # file read. The caller treats the null as the no-target state.
    if (-not $text) {
        return $null
    }

    return (Resolve-PrdFeatureWorktreeTarget -Text $text)
}

function Invoke-PrdFeatureBeforePlannerDecision {
    <#
    .SYNOPSIS
        Parses the envelope's nested tool_input and returns an allow-or-block decision.
    .PARAMETER ToolInputRaw
        The raw PreToolUse payload.
    .PARAMETER ResolvedTarget
        Test-only injection seam for the derived call target, following the
        -CheckpointRaw precedent in enforce-orchestration-preimplementation-gate.ps1.
        Binding is decided with $PSBoundParameters.ContainsKey rather than a
        truthiness test, so an explicitly supplied empty value suppresses the
        derivation seam instead of falling through to it.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw,

        [AllowNull()]
        [object] $ResolvedTarget
    )

    $envelope = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $envelope.IsValid) {
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = 'PRD_FEATURE_BLOCKED: payload anomaly - ' +
                (Get-ClaudeHookPayloadAnomalyReason -Anomaly $envelope.Anomaly) +
                '. The gate fails closed on an envelope it cannot read.'
            }
        }
    }

    $subagent = Get-ClaudeHookToolInputString -ToolInput $envelope.Value -Name 'subagent_type'
    if (-not $subagent -or $subagent -ne 'atomic-planner') {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $prompt = Get-ClaudeHookToolInputString -ToolInput $envelope.Value -Name 'prompt'

    $target = if ($PSBoundParameters.ContainsKey('ResolvedTarget')) {
        $ResolvedTarget
    }
    else {
        Get-PrdFeatureCallTarget -ToolInput $envelope.Value
    }

    # Unresolved identity denies before any document probe runs; a probe here would
    # validate the call against whichever root happened to be current. A null target has
    # two origins that mean the same thing -- the injection seam bound to null, and a call
    # carrying no text at all -- so both take the no-target code.
    if ($null -eq $target -or $target.Status -eq 'NoTarget' -or $target.Status -eq 'Ambiguous') {
        $code = if ($null -eq $target) { Get-WorktreeResolutionNoTargetReasonCode } else { $target.ReasonCode }
        $detail = if ($null -eq $target) { 'the call target was not resolved' } else { $target.Detail }
        return (Get-PrdFeatureTargetDecision -ReasonCode $code -Detail $detail)
    }

    $candidates = @(Find-PrdFeatureFolderCandidate -Prompt $prompt)

    # The checkpoint is read only when it is needed. Exactly one cited candidate is used
    # directly, so the common case performs no checkpoint read at all; zero or several
    # candidates need the resolved worktree's own checkpoint, which is read once here.
    $checkpointFolder = $null
    if ($candidates.Count -ne 1) {
        $checkpointFolder = Get-PrdFeatureCheckpointFolder -CheckpointPath (Get-WorktreeItemCheckpointPath -WorktreeRoot $target.WorktreeRoot)
    }

    $folder = Find-PrdFeatureFolderFromPrompt -Prompt $prompt -CheckpointFolder $checkpointFolder

    # More than one folder was cited and the resolved worktree's checkpoint names none of
    # them. The tie is unresolved, which is a resolution failure rather than a document one.
    if (-not $folder -and $candidates.Count -gt 1) {
        return (Get-PrdFeatureTargetDecision -ReasonCode (Get-WorktreeResolutionAmbiguityReasonCode) -Detail ("the call cites $($candidates.Count) feature folders ($($candidates -join ', ')) and the resolved worktree's checkpoint chooses between none of them"))
    }

    # The prompt named no folder, so the resolved worktree's checkpoint supplies one. It is
    # the item's own checkpoint by construction, because identity selected the worktree it
    # sits in, so no conditional guard is needed before trusting it.
    if (-not $folder) {
        $folder = $checkpointFolder
    }

    if (-not $folder) {
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = "PRD_FEATURE_BLOCKED: atomic-planner delegation must reference a feature folder, either in the prompt or in the orchestrator checkpoint of the worktree the call resolves to, so spec.md and user-story.md prerequisites can be verified."
            }
        }
    }

    $folderNormalized = ($folder -replace '\\', '/').TrimEnd('/')

    # Anchor every probe to the resolved target root, and only when that root differs
    # from the session root. When the two coincide the bare repo-relative spelling is
    # kept: prefixing unconditionally would break every call that legitimately resolves
    # where it runs.
    $probeFolder = $folderNormalized
    if ($null -ne $target -and $target.Status -eq 'OtherWorktree' -and $target.WorktreeRoot) {
        $probeFolder = Join-WorktreeResolutionPath -WorktreeRoot $target.WorktreeRoot -RepoRelativePath $folderNormalized
    }

    # Derive the prerequisite set from the persisted work-mode marker rather
    # than a fixed spec.md/user-story.md pair. A marker that cannot be read or
    # recognized must fail closed, not fail open: it denies on its own branch
    # below, naming no prerequisite set and probing for no required file.
    $issueContent = Get-PrdFeatureIssueContent -FeatureFolder $probeFolder

    # A folder that is absent from the resolved target root is a resolution failure,
    # not a broken marker. Reporting it as a broken marker would describe a folder the
    # gate probed at a root the call never named. The conjunct on the guard above is
    # load-bearing: without it, an unreadable issue.md would take this branch on every
    # session-root call instead of falling through to the work-mode branch.
    if ($null -eq $issueContent -and $probeFolder -ne $folderNormalized) {
        return (Get-PrdFeatureTargetDecision -ReasonCode (Get-WorktreeResolutionAmbiguityReasonCode) -Detail ("the resolved feature folder '$probeFolder' does not exist under the resolved target worktree, so the call's target cannot be confirmed"))
    }

    $workMode = Resolve-PrdFeatureWorkMode -IssueContent $issueContent

    # An indeterminate mode is its own decision path, and it deliberately does NOT
    # run the required-file probe. When the mode is unknown no prerequisite set is
    # knowable, so any set the gate named would be wrong for at least one mode:
    # a set containing user-story.md is unsatisfiable for full-bug and minor-audit
    # without violating the lifecycle contract, and the empty set fails open. The
    # only remedy true in all three modes is repairing the marker, so that is what
    # the reason states. This still DENIES, so the gate remains fail-closed.
    if (-not $workMode) {
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = "PRD_FEATURE_BLOCKED: resolved feature folder '$probeFolder', " +
                "but its work mode could not be determined from '$probeFolder/issue.md' " +
                '(the ''- Work Mode:'' marker is absent, unreadable, or unrecognized). ' +
                'Confirm that is the intended feature folder, then add or correct the ' +
                '''- Work Mode:'' marker in that file so the prerequisite set can be derived.'
            }
        }
    }

    # Force array wrapping: PowerShell unravels a zero-element array return down
    # the pipeline to $null, which would otherwise fail the Mandatory
    # -RequiredFile parameter on Get-PrdFeatureMissingFile for minor-audit mode.
    $required = @(Get-PrdFeatureRequiredFile -WorkMode $workMode)

    $missing = Get-PrdFeatureMissingFile -FeatureFolder $probeFolder -RequiredFile $required
    if ($missing.Count -eq 0) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    # Lead with the resolved folder, not with the remedy: a reader who sees a
    # folder they did not intend diagnoses a path problem immediately instead of
    # re-running a step that has already completed correctly.
    $list = ($missing -join ', ')
    $reason = "PRD_FEATURE_BLOCKED: resolved feature folder '$probeFolder' is missing: " +
    "$list (work mode: $workMode). Confirm that is the intended feature folder, then " +
    'invoke the prd-feature subagent to produce the missing output(s).'

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $reason
        }
    }
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

$decision = Invoke-PrdFeatureBeforePlannerDecision -ToolInputRaw (Read-ClaudeHookRawPayload)

$decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

exit 0
