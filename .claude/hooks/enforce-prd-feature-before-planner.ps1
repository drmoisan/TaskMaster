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
      1. Scan the prompt text for any path matching
         docs/features/active/<token>, accepting both forward-slash and
         backslash separators. The longest match wins; when it points at a
         file (ends with .md), use its parent directory.
      2. If no candidate was found in the prompt, read the feature-folder field
         from artifacts/orchestration/orchestrator-state.json.
      3. If neither yields a folder, block with a reason instructing the caller
         to reference a feature folder explicitly.

    Once the folder is resolved, the hook reads the persisted work-mode marker
    (`- Work Mode: minor-audit|full-feature|full-bug|full`) from that folder's
    issue.md, per the mode contract in
    .claude/skills/feature-promotion-lifecycle/SKILL.md, and derives the
    required prerequisite set:
      - full-feature -> spec.md and user-story.md are both required.
      - full-bug     -> spec.md only is required.
      - minor-audit  -> neither is required; issue.md carries the acceptance
                        criteria for this mode.
      - marker absent, unreadable, or unrecognized -> fail closed to the
        strictest set (spec.md and user-story.md), and the block reason states
        that the work mode could not be determined so the operator can tell
        this case apart from a genuine missing prerequisite. The legacy `full`
        marker normalizes to full-feature's requirement set.

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

function Resolve-PrdFeatureWorkMode {
    <#
    .SYNOPSIS
        Parses the persisted `- Work Mode: ...` marker out of issue.md content
        and returns the canonical mode, or $null when the marker is absent,
        unreadable, or unrecognized.
    .DESCRIPTION
        Recognizes minor-audit, full-feature, full-bug, and the legacy full
        marker (normalized to full-feature), mirroring the regex convention
        used by scripts/dev_tools/prompt_mode_contract.py so both runtimes
        agree on what counts as a valid marker line.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [string] $IssueContent
    )

    if ([string]::IsNullOrWhiteSpace($IssueContent)) {
        return $null
    }

    $match = [regex]::Match($IssueContent, '(?im)^-\s*Work Mode:\s*(minor-audit|full-feature|full-bug|full)\s*$')
    if (-not $match.Success) {
        return $null
    }

    $rawMode = $match.Groups[1].Value
    if ($rawMode -eq 'full') {
        return 'full-feature'
    }
    return $rawMode
}

function Get-PrdFeatureRequiredFile {
    <#
    .SYNOPSIS
        Maps a resolved work mode to the set of prd-feature output files the
        target folder must contain before an atomic-planner delegation is
        allowed.
    .DESCRIPTION
        full-feature requires spec.md and user-story.md; full-bug requires
        spec.md only; minor-audit requires neither. A $null or unrecognized
        mode fails closed to the strictest set (spec.md and user-story.md) so
        an undeterminable mode never becomes permissive.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [AllowNull()]
        [string] $WorkMode
    )

    # Route on the canonical mode; anything outside the three known values
    # (including $null) falls through to the fail-closed default case.
    switch ($WorkMode) {
        'full-feature' { return [string[]]@('spec.md', 'user-story.md') }
        'full-bug' { return [string[]]@('spec.md') }
        'minor-audit' { return [string[]]@() }
        default { return [string[]]@('spec.md', 'user-story.md') }
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
        [string] $CheckpointPath = 'artifacts/orchestration/orchestrator-state.json'
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

function Find-PrdFeatureFolderFromPrompt {
    <#
    .SYNOPSIS
        Scans a prompt string for docs/features/active/<...> path tokens and
        returns the longest unique match resolved to a folder path. Returns
        $null when no match is found.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Prompt
    )

    if (-not $Prompt) {
        return $null
    }

    # Allow forward or backslash separators inside the matched path token.
    $pattern = 'docs[\\/]+features[\\/]+active[\\/]+[^\s"''`]+'
    $matchList = [regex]::Matches($Prompt, $pattern)
    if ($matchList.Count -eq 0) {
        return $null
    }

    $unique = @{}
    foreach ($m in $matchList) {
        $normalized = ($m.Value -replace '\\', '/').TrimEnd('/')
        $unique[$normalized] = $true
    }

    $candidates = @(@($unique.Keys) | Sort-Object -Property Length -Descending)
    $best = $candidates[0]

    # If the longest match ends in .md, treat it as a file and use its parent.
    if ($best -match '\.md$') {
        $parent = $best -replace '/[^/]+\.md$', ''
        return $parent
    }

    return $best
}

function Get-PrdFeatureMissingFile {
    <#
    .SYNOPSIS
        Returns the subset of $RequiredFile that is missing from the target
        folder.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory)]
        [string] $FeatureFolder,

        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]] $RequiredFile
    )

    [System.Collections.Generic.List[string]] $missing = [System.Collections.Generic.List[string]]::new()
    foreach ($name in $RequiredFile) {
        $candidate = "$FeatureFolder/$name"
        if (-not (Get-PrdFeatureFileExistence -Path $candidate)) {
            $missing.Add($name)
        }
    }
    return [string[]] $missing.ToArray()
}

function Invoke-PrdFeatureBeforePlannerDecision {
    <#
    .SYNOPSIS
        Parses the envelope's nested tool_input and returns an allow-or-block decision.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw
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
    $folder = Find-PrdFeatureFolderFromPrompt -Prompt $prompt
    if (-not $folder) {
        $folder = Get-PrdFeatureCheckpointFolder
    }

    if (-not $folder) {
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = "PRD_FEATURE_BLOCKED: atomic-planner delegation must reference a feature folder (either in the prompt or via orchestrator-state.json) so spec.md and user-story.md prerequisites can be verified."
            }
        }
    }

    $folderNormalized = ($folder -replace '\\', '/').TrimEnd('/')

    # Derive the prerequisite set from the persisted work-mode marker rather
    # than a fixed spec.md/user-story.md pair. A marker that cannot be read or
    # recognized must fail closed to the strictest set, not fail open.
    $issueContent = Get-PrdFeatureIssueContent -FeatureFolder $folderNormalized
    $workMode = Resolve-PrdFeatureWorkMode -IssueContent $issueContent
    $modeDetermined = [bool]$workMode
    # Force array wrapping: PowerShell unravels a zero-element array return down
    # the pipeline to $null, which would otherwise fail the Mandatory
    # -RequiredFile parameter on Get-PrdFeatureMissingFile for minor-audit mode.
    $required = @(Get-PrdFeatureRequiredFile -WorkMode $workMode)

    $missing = Get-PrdFeatureMissingFile -FeatureFolder $folderNormalized -RequiredFile $required
    if ($missing.Count -eq 0) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $list = ($missing -join ', ')
    if ($modeDetermined) {
        $reason = "PRD_FEATURE_BLOCKED: cannot delegate to atomic-planner before prd-feature outputs are present in '$folderNormalized'. Missing: $list (work mode: $workMode). Invoke the prd-feature subagent first."
    }
    else {
        $reason = "PRD_FEATURE_BLOCKED: cannot delegate to atomic-planner before prd-feature outputs are present in '$folderNormalized'. Missing: $list. Work mode could not be determined from '$folderNormalized/issue.md' (marker absent, unreadable, or unrecognized); failing closed to the strictest prerequisite set (spec.md, user-story.md). Invoke the prd-feature subagent first."
    }

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
