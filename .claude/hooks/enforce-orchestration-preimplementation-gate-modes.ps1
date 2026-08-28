<#
.SYNOPSIS
    Mode dispatch and per-mode readiness predicates for the orchestration
    preimplementation gate (Claude surface).
.DESCRIPTION
    Normative contract: the issue #554 mode dispatch and readiness predicates.
    This file owns the fixed mode table, the canonical checkpoint-path map, the
    implementation-agent allow-list, mode resolution, the target-token finder, the
    prompt-declared-path cross-check, and the epic and parallel readiness
    predicates. It owns nothing else.

    The readiness source is resolved from the recognized mode marker through the
    fixed table below, and NEVER from a path parsed out of a prompt: a delegation
    that named its own readiness file would choose its own gate. A prompt-declared
    path is a cross-check operand only. The posture follows the shipped precedents
    enforce-epic-wave-barrier.ps1 and enforce-parallel-cohort-barrier.ps1.

    PURITY. This file is pure string and object logic, with no filesystem,
    process, network, or environment access: it opens no file, probes no path,
    issues no web request, launches no executable, and imports no module. Every
    readiness predicate accepts an ALREADY-PARSED checkpoint object, or $null; the
    per-mode read seams live in the main gate hook. It is a new sibling rather than
    an addition to the issue #539 helpers file, whose header declares a different
    normative contract and which lacks headroom under the 500-line cap; leaving
    that file byte-untouched is the proof the #539 exemption is unchanged.
#>
[CmdletBinding()]
param()

# --- Constant table 1: the fixed mode table --------------------------------------
# Markers are reused verbatim from shipped contracts and hooks; do not invent them.
# The trailing-period asymmetry is deliberate: preparation markers are matched WITH
# their periods as the shipped gate hook does and an existing test pins, epic and
# parallel WITHOUT, as the two barrier hooks do, which makes the three hooks on the
# same Agent matcher agree. MatchCase carries the same asymmetry. Both forms are
# containment tests over the prompt, so neither is sensitive to edge whitespace.
# Rows evaluate in order, so preparation is first and exempts; all markers on a row
# must be present for that row to match.
$script:OrchestrationDelegationModeTable = @(
    [pscustomobject]@{
        Mode      = 'preparation'
        Markers   = @('Preparation mode: true.', 'route_id: preparation.')
        MatchCase = $true
    }
    [pscustomobject]@{ Mode = 'epic'; Markers = @('Epic mode: true'); MatchCase = $false }
    [pscustomobject]@{ Mode = 'parallel'; Markers = @('Parallel mode: true'); MatchCase = $false }
)

# The mode a prompt carrying no recognized marker resolves to.
$script:OrchestrationDelegationDefaultMode = 'single-feature'

# --- Constant table 2: the canonical checkpoint-path map -------------------------
# Preparation is exempt and has no readiness source, expressed as an empty string
# so callers test it with one truthiness check. No value here is ever derived from
# prompt text.
$script:OrchestrationDelegationCheckpointPathMap = [ordered]@{
    'preparation'    = ''
    'epic'           = 'artifacts/orchestration/epic-orchestrator-state.json'
    'parallel'       = 'artifacts/orchestration/parallel-orchestrator-state.json'
    'single-feature' = 'artifacts/orchestration/orchestrator-state.json'
}

# --- Constant table 3: the implementation-agent allow-list -----------------------
# Exactly five members: the agent tokens carried over from the replaced seven-token
# regex, dropping only the two free-text tokens. Retaining atomic-executor and the
# four typed-engineer names is a hard invariant; pre-existing cases supply two of
# them and assert deny.
$script:OrchestrationImplementationAgentAllowList = @(
    'python-typed-engineer'
    'powershell-typed-engineer'
    'typescript-engineer'
    'csharp-typed-engineer'
    'atomic-executor'
)

function Get-OrchestrationModeProperty {
    <#
    .SYNOPSIS
        Reads a named property off an already-parsed object, or $null. The single
        field-access seam here; never throws.
    #>
    [CmdletBinding()]
    [OutputType([object])]
    param(
        [Parameter(Mandatory)][AllowNull()] $Value,
        [Parameter(Mandatory)][string] $Name
    )

    if ($null -eq $Value) { return $null }
    $properties = $null
    try {
        $properties = $Value.PSObject.Properties
    } catch {
        Write-Debug "Property probe failed for '$Name': $($_.Exception.Message)"
        return $null
    }
    if ($null -eq $properties -or -not ($properties.Name -contains $Name)) { return $null }
    return $properties[$Name].Value
}

function Get-OrchestrationModeString {
    <#
    .SYNOPSIS
        Reads a named property as a trimmed string, or an empty string. The name is
        distinct from the gate hooks' Get-StringProperty so it cannot shadow it.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][AllowNull()] $Value,
        [Parameter(Mandatory)][string] $Name
    )

    $raw = Get-OrchestrationModeProperty -Value $Value -Name $Name
    if ($null -eq $raw) { return '' }
    return ([string]$raw).Trim()
}

function Get-OrchestrationModeCollection {
    <#
    .SYNOPSIS
        Reads a named property as an array, or an empty array.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [Parameter(Mandatory)][AllowNull()] $Value,
        [Parameter(Mandatory)][string] $Name
    )

    $raw = Get-OrchestrationModeProperty -Value $Value -Name $Name
    if ($null -eq $raw) { return @() }
    return @($raw)
}

function Get-OrchestrationModeFolderBasename {
    <#
    .SYNOPSIS
        Normalizes a feature_folder value to its bare basename. A record's value
        may be a full path with a lifecycle prefix or a bare basename, so both
        sides of every comparison are normalized, following the cohort-barrier hook.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([AllowNull()][AllowEmptyString()][string] $Path)

    if (-not $Path) { return '' }
    $normalized = ($Path -replace '\\', '/').TrimEnd('/')
    if (-not $normalized) { return '' }
    return ($normalized -split '/')[-1]
}

function Resolve-OrchestrationDelegationMode {
    <#
    .SYNOPSIS
        Resolves a delegation prompt to one of the four mode names. Reads nothing
        but the supplied string; evaluates preparation, then epic, then parallel,
        then the default. Null and empty prompts resolve to the default. No
        checkpoint path is ever read out of a prompt.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([AllowNull()][AllowEmptyString()][string] $Prompt)

    if (-not $Prompt) { return $script:OrchestrationDelegationDefaultMode }

    foreach ($row in $script:OrchestrationDelegationModeTable) {
        $allPresent = $true
        foreach ($marker in $row.Markers) {
            $present = if ($row.MatchCase) {
                $Prompt.Contains($marker)
            } else {
                $Prompt -like ('*' + $marker + '*')
            }
            if (-not $present) {
                $allPresent = $false
                break
            }
        }
        if ($allPresent) { return $row.Mode }
    }
    return $script:OrchestrationDelegationDefaultMode
}

function Get-OrchestrationDelegationCheckpointPath {
    <#
    .SYNOPSIS
        Returns the canonical readiness source for a mode name, from the fixed
        table and nowhere else. Preparation returns an empty string, as does an
        unrecognized name, so no caller can manufacture a source.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([AllowNull()][AllowEmptyString()][string] $Mode)

    if (-not $Mode -or -not $script:OrchestrationDelegationCheckpointPathMap.Contains($Mode)) {
        return ''
    }
    return [string]$script:OrchestrationDelegationCheckpointPathMap[$Mode]
}

function Test-OrchestrationImplementationAgent {
    <#
    .SYNOPSIS
        Tests a subagent_type value against the implementation-agent allow-list.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()][AllowEmptyString()][string] $SubagentType)

    if (-not $SubagentType) { return $false }
    return ($script:OrchestrationImplementationAgentAllowList -contains $SubagentType)
}

function Find-OrchestrationDelegationTargetFolder {
    <#
    .SYNOPSIS
        Resolves the target feature-folder basename out of a delegation prompt,
        reusing the wave-barrier technique in shape: scan for slash-separated
        docs/features/active/ tokens, longest unique match wins, a Markdown match
        resolves to its parent, and the basename is returned. $null when no token
        resolves, which the caller treats as a deny.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([AllowNull()][AllowEmptyString()][string] $Prompt)

    if (-not $Prompt) { return $null }

    $pattern = 'docs[\\/]+features[\\/]+active[\\/]+[^\s"''`]+'
    $matchList = [regex]::Matches($Prompt, $pattern)
    if ($matchList.Count -eq 0) { return $null }

    $unique = [ordered]@{}
    foreach ($item in $matchList) { $unique[$item.Value] = $true }
    $candidates = @(@($unique.Keys) | Sort-Object -Property Length -Descending)
    $best = [string]$candidates[0]

    # Sentence punctuation trails a bare path token in every shipped kickoff
    # contract. A trailing period is stripped only when it does not form the
    # Markdown extension the next branch depends on.
    $best = $best.TrimEnd(',', ';', ':')
    while ($best.EndsWith('.') -and -not $best.EndsWith('.md')) {
        $best = $best.Substring(0, $best.Length - 1)
    }
    if ($best -match '\.md$') { $best = $best -replace '[\\/][^\\/]+\.md$', '' }

    $basename = Get-OrchestrationModeFolderBasename -Path $best
    if (-not $basename) { return $null }
    return $basename
}

function Find-OrchestrationDelegationIssueNumber {
    <#
    .SYNOPSIS
        Resolves an issue number out of a delegation prompt, as a string. The
        alternative target resolution of decision D3, issue_num being the primary
        key on both checkpoints. The keyed form is preferred over the bare hash
        form; $null when neither resolves. Accepted widening: a bare hash form such
        as a pull-request reference can supply a number that is not the target's,
        which widens the SEARCH only - an unmatched number yields no record and
        denies, so deny-by-default is preserved.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([AllowNull()][AllowEmptyString()][string] $Prompt)

    if (-not $Prompt) { return $null }

    $keyed = [regex]::Match($Prompt, 'issue[_-]?num(?:ber)?\s*[:=]\s*#?(\d+)', 'IgnoreCase')
    if ($keyed.Success) { return $keyed.Groups[1].Value }
    $hashForm = [regex]::Match($Prompt, '(?:^|\s)#(\d+)\b')
    if ($hashForm.Success) { return $hashForm.Groups[1].Value }
    return $null
}

function Test-OrchestrationDelegationDeclaredCheckpointPath {
    <#
    .SYNOPSIS
        Cross-checks a prompt-declared checkpoint path against the mode's canonical
        path. True only when the prompt declares none for the mode, or declares one
        equal to the canonical value. The declared value is a cross-check operand
        ONLY and never selects a source; a disagreement is a deny. A mode with no
        declared-path key returns true.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()][AllowEmptyString()][string] $Prompt,
        [AllowNull()][AllowEmptyString()][string] $Mode
    )

    if ($Mode -ne 'epic' -and $Mode -ne 'parallel') { return $true }
    if (-not $Prompt) { return $true }

    $canonical = Get-OrchestrationDelegationCheckpointPath -Mode $Mode
    $key = $Mode + '_checkpoint_path'
    $declaredMatch = [regex]::Match(
        $Prompt, [regex]::Escape($key) + '\s*[:=]\s*([^\s"''`]+)', 'IgnoreCase')
    if (-not $declaredMatch.Success) { return $true }

    $declared = $declaredMatch.Groups[1].Value.TrimEnd(',', ';', ':')
    while ($declared.EndsWith('.') -and -not $declared.EndsWith('.json')) {
        $declared = $declared.Substring(0, $declared.Length - 1)
    }
    $declared = $declared -replace '\\', '/'
    return ($declared -eq $canonical)
}

function Test-OrchestrationModeTerminalMergeStatus {
    <#
    .SYNOPSIS
        Tests whether a target record's merge_status is terminal-merged (decision
        D8). The two terminal members are merged and worktree_removed, the same two
        the barrier hooks treat as terminal-safe. Every other member, including the
        failure members, is pre-merge here: re-delegation after a blocked or
        conflicted state is legitimate remediation and must not be gated off. An
        ABSENT merge_status is treated as not_started, per parallel invariant 7.
        This predicate CONSUMES the existing member sets and extends neither.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowNull()] $Record)

    if ($null -eq $Record) { return $false }
    $status = Get-OrchestrationModeString -Value $Record -Name 'merge_status'
    if (-not $status) { return $false }
    return (@('merged', 'worktree_removed') -contains $status)
}

function Find-OrchestrationModeRecord {
    <#
    .SYNOPSIS
        Finds the target record in a checkpoint's feature or item collection,
        matching the normalized feature_folder basename first and issue_num second.
        $null when neither resolves, which is a failed conjunct.
    #>
    [CmdletBinding()]
    [OutputType([object])]
    param(
        [Parameter(Mandatory)][AllowNull()] $Records,
        [AllowNull()][AllowEmptyString()][string] $TargetFolder,
        [AllowNull()][AllowEmptyString()][string] $IssueNumber
    )

    if ($null -eq $Records) { return $null }
    foreach ($record in @($Records)) {
        if ($null -eq $record) { continue }
        if ($TargetFolder) {
            $folder = Get-OrchestrationModeString -Value $record -Name 'feature_folder'
            $basename = Get-OrchestrationModeFolderBasename -Path $folder
            if ($basename -and $basename -eq $TargetFolder) { return $record }
        }
        if ($IssueNumber) {
            $issue = Get-OrchestrationModeString -Value $record -Name 'issue_num'
            if ($issue -and $issue -eq $IssueNumber) { return $record }
        }
    }
    return $null
}

function Get-EpicOrchestrationReadinessFailure {
    <#
    .SYNOPSIS
        Names the first failed epic readiness conjunct, or returns an empty string.
        Accepts an already-parsed checkpoint object or $null and enforces, in
        order: route_id exactly epic; non-empty epic_feature_folder; non-empty
        epic_manifest_path under docs/features/epics/; non-empty
        integration_branch; present and non-empty features; the resolved target
        present as a record in features; and that record's merge_status neither
        merged nor worktree_removed. The epic_manifest_path conjunct deliberately
        tightens relative to validate_epic_orchestrator_state.py, whose
        required-key set omits it, but is not stricter than the producing skill's
        contract, which mandates it; a false deny names the failed conjunct.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][AllowNull()] $Checkpoint,
        [AllowNull()][AllowEmptyString()][string] $TargetFolder,
        [AllowNull()][AllowEmptyString()][string] $IssueNumber
    )

    if ($null -eq $Checkpoint) { return 'checkpoint-absent' }
    if ((Get-OrchestrationModeString -Value $Checkpoint -Name 'route_id') -ne 'epic') {
        return 'route_id'
    }
    if (-not (Get-OrchestrationModeString -Value $Checkpoint -Name 'epic_feature_folder')) {
        return 'epic_feature_folder'
    }
    $manifest = (Get-OrchestrationModeString -Value $Checkpoint -Name 'epic_manifest_path') -replace '\\', '/'
    if (-not $manifest -or $manifest -notmatch '(^|/)docs/features/epics/') {
        return 'epic_manifest_path'
    }
    if (-not (Get-OrchestrationModeString -Value $Checkpoint -Name 'integration_branch')) {
        return 'integration_branch'
    }
    $features = Get-OrchestrationModeCollection -Value $Checkpoint -Name 'features'
    if ($features.Count -eq 0) { return 'features' }
    $record = Find-OrchestrationModeRecord -Records $features -TargetFolder $TargetFolder -IssueNumber $IssueNumber
    if ($null -eq $record) { return 'target-record' }
    if (Test-OrchestrationModeTerminalMergeStatus -Record $record) { return 'merge_status' }
    return ''
}

function Test-EpicOrchestrationReady {
    <#
    .SYNOPSIS
        Boolean wrapper over Get-EpicOrchestrationReadinessFailure.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowNull()] $Checkpoint,
        [AllowNull()][AllowEmptyString()][string] $TargetFolder,
        [AllowNull()][AllowEmptyString()][string] $IssueNumber
    )

    $failure = Get-EpicOrchestrationReadinessFailure -Checkpoint $Checkpoint `
        -TargetFolder $TargetFolder -IssueNumber $IssueNumber
    return (-not $failure)
}

function Get-ParallelOrchestrationReadinessFailure {
    <#
    .SYNOPSIS
        Names the first failed parallel readiness conjunct, or an empty string.
        Accepts an already-parsed checkpoint object or $null and enforces, in
        order: route_id exactly parallel; non-empty parallel_slug; non-empty
        parallel_manifest_path; present and non-empty items; the resolved target
        present as a record in items; and that record's merge_status neither merged
        nor worktree_removed. It consumes the parallel item-state and merge-status
        member sets and adds no member to either.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][AllowNull()] $Checkpoint,
        [AllowNull()][AllowEmptyString()][string] $TargetFolder,
        [AllowNull()][AllowEmptyString()][string] $IssueNumber
    )

    if ($null -eq $Checkpoint) { return 'checkpoint-absent' }
    if ((Get-OrchestrationModeString -Value $Checkpoint -Name 'route_id') -ne 'parallel') {
        return 'route_id'
    }
    if (-not (Get-OrchestrationModeString -Value $Checkpoint -Name 'parallel_slug')) {
        return 'parallel_slug'
    }
    if (-not (Get-OrchestrationModeString -Value $Checkpoint -Name 'parallel_manifest_path')) {
        return 'parallel_manifest_path'
    }
    $items = Get-OrchestrationModeCollection -Value $Checkpoint -Name 'items'
    if ($items.Count -eq 0) { return 'items' }
    $record = Find-OrchestrationModeRecord -Records $items -TargetFolder $TargetFolder -IssueNumber $IssueNumber
    if ($null -eq $record) { return 'target-record' }
    if (Test-OrchestrationModeTerminalMergeStatus -Record $record) { return 'merge_status' }
    return ''
}

function Test-ParallelOrchestrationReady {
    <#
    .SYNOPSIS
        Boolean wrapper over Get-ParallelOrchestrationReadinessFailure.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowNull()] $Checkpoint,
        [AllowNull()][AllowEmptyString()][string] $TargetFolder,
        [AllowNull()][AllowEmptyString()][string] $IssueNumber
    )

    $failure = Get-ParallelOrchestrationReadinessFailure -Checkpoint $Checkpoint `
        -TargetFolder $TargetFolder -IssueNumber $IssueNumber
    return (-not $failure)
}
