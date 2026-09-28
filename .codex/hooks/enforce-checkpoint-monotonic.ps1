
<#
.SYNOPSIS
    Pre-tool-use hook that blocks Write operations on the orchestrator checkpoint
    file when completed_steps appear out of declared canonical order.

.DESCRIPTION
    Invoked by the Codex PreToolUse hook on Write or Edit operations. The
    hook activates only when the target file_path is
    artifacts/orchestration/orchestrator-state.json.

    For Write tool calls, the content field is parsed as JSON and its
    completed_steps array is validated against the canonical orchestrator
    workflow step prefixes:

      S0_startup_checks
      S1_change_budget_estimation
      S2_research
      S3_promotion
      S4_atomic_planning
      S5_atomic_execution
      S6_pre_review_commit
      S7_feature_review
      S8_create_pr
      S9_remediation_loop
      S10_post_pr
      S12_complete

    Entries that do not match any canonical prefix are treated as informational
    and ignored. If any pair (i, j) with i < j has a higher canonical index at
    position i than at position j, the script denies via a PreToolUse JSON
    response with hookSpecificOutput.permissionDecision='deny' and a reason
    listing the offending pair. A non-empty rollback_history array suppresses
    this check because rollbacks legitimately reorder steps. All allow paths
    emit hookSpecificOutput.permissionDecision='allow'.

    Edit tool calls supply only old_string/new_string (a partial patch) and
    cannot be reliably validated without the full target file content, so they
    are allowed by this hook. The next Write call will catch a regression.

.NOTES
    Compatible with PowerShell 7+. Read-only validation gate.
#>
[CmdletBinding()]
param()

# Shared Codex PreToolUse transport: stdin payload parsing and tool_input-to-file
# mapping for every tool name the ^(apply_patch|Edit|Write)$ matcher admits.
. (Join-Path $PSScriptRoot 'codex-pretooluse-file-mapping.ps1')

# The only path this hook governs. On-disk reconstruction of an apply_patch
# Update is requested for this path alone, so a patch that merely happens to
# touch other files can never fail the hook.
$script:GovernedCheckpointPath = 'artifacts/orchestration/orchestrator-state.json'

$script:CanonicalStepPrefixes = @(
    'S0_startup_checks',
    'S1_change_budget_estimation',
    'S2_research',
    'S3_promotion',
    'S4_atomic_planning',
    'S5_atomic_execution',
    'S6_pre_review_commit',
    'S7_feature_review',
    'S8_create_pr',
    'S9_remediation_loop',
    'S10_post_pr',
    'S12_complete'
)

function ConvertFrom-CheckpointJson {
    <#
    .SYNOPSIS
        Wrapper around ConvertFrom-Json for the checkpoint content. Mockable.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $Json
    )

    return $Json | ConvertFrom-Json -ErrorAction Stop
}

function Get-CanonicalStepIndex {
    <#
    .SYNOPSIS
        Returns the canonical index for a completed_steps entry, or -1 when the
        entry does not match any canonical prefix.
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $StepEntry
    )

    for ($i = 0; $i -lt $script:CanonicalStepPrefixes.Count; $i++) {
        $prefix = $script:CanonicalStepPrefixes[$i]
        if ($StepEntry -eq $prefix -or $StepEntry.StartsWith("$prefix" + '_') -or $StepEntry.StartsWith("$prefix.") -or $StepEntry.StartsWith("$prefix-")) {
            return $i
        }
        # The S3_promotion family of variants (S3_promotion_potential, S3_promotion_issue,
        # S3_promotion_folder, etc.) is matched by the StartsWith branches above.
    }

    return -1
}

function Get-OutOfOrderPair {
    <#
    .SYNOPSIS
        Returns the first pair (entry-at-i, entry-at-j, indices) where i<j but the
        canonical index at i is greater than the canonical index at j. Non-canonical
        entries (index -1) are skipped. Returns $null when in order.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]] $CompletedSteps
    )

    $indexed = @()
    for ($i = 0; $i -lt $CompletedSteps.Count; $i++) {
        $idx = Get-CanonicalStepIndex -StepEntry $CompletedSteps[$i]
        if ($idx -ge 0) {
            $indexed += [pscustomobject]@{ Position = $i; CanonicalIndex = $idx; Entry = $CompletedSteps[$i] }
        }
    }

    for ($a = 0; $a -lt $indexed.Count; $a++) {
        for ($b = $a + 1; $b -lt $indexed.Count; $b++) {
            if ($indexed[$a].CanonicalIndex -gt $indexed[$b].CanonicalIndex) {
                return [pscustomobject]@{
                    EarlierEntry = $indexed[$a].Entry
                    EarlierPos   = $indexed[$a].Position
                    LaterEntry   = $indexed[$b].Entry
                    LaterPos     = $indexed[$b].Position
                }
            }
        }
    }

    return $null
}

function Test-StepHasPrefix {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $StepEntry,

        [Parameter(Mandatory)]
        [string] $Prefix
    )

    return $StepEntry -eq $Prefix -or $StepEntry.StartsWith("$Prefix" + '_') -or $StepEntry.StartsWith("$Prefix.") -or $StepEntry.StartsWith("$Prefix-")
}

function Get-MissingPrerequisiteForAdvancedStep {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]] $CompletedSteps
    )

    $hasPromotion = $false
    $hasPlanning = $false
    foreach ($step in $CompletedSteps) {
        if (Test-StepHasPrefix -StepEntry $step -Prefix 'S3_promotion') {
            $hasPromotion = $true
        }
        if (Test-StepHasPrefix -StepEntry $step -Prefix 'S4_atomic_planning') {
            $hasPlanning = $true
        }
    }

    foreach ($step in $CompletedSteps) {
        $index = Get-CanonicalStepIndex -StepEntry $step
        if ($index -ge 5 -and (-not $hasPromotion -or -not $hasPlanning)) {
            return [pscustomobject]@{
                Step             = $step
                MissingPromotion = -not $hasPromotion
                MissingPlanning  = -not $hasPlanning
            }
        }
    }

    return $null
}

function Test-IsCheckpointPath {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $NormalizedPath
    )

    return $NormalizedPath -match '(^|/)artifacts/orchestration/orchestrator-state\.json$'
}

function Invoke-CheckpointMonotonicDecision {
    <#
    .SYNOPSIS
        Parses Codex tool_input and returns an allow-or-block decision.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw
    )

    if (-not $ToolInputRaw) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw "enforce-checkpoint-monotonic hook received malformed JSON in Codex tool_input: $_"
    }

    $filePath = $toolInput.file_path
    if (-not $filePath) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $normalized = $filePath -replace '\\', '/'
    if (-not (Test-IsCheckpointPath -NormalizedPath $normalized)) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    # The adapter supplies complete post-patch content for every recognized
    # operation. Empty content therefore represents deletion and must fail
    # closed for the canonical checkpoint.
    $content = $toolInput.content
    if (-not $content) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{
                hookEventName = 'PreToolUse'; permissionDecision = 'deny'
                permissionDecisionReason = 'CHECKPOINT_ORDER_BLOCKED: the canonical checkpoint cannot be deleted or replaced with empty content.'
            } }
    }

    try {
        $payload = ConvertFrom-CheckpointJson -Json $content
    }
    catch {
        return [ordered]@{ hookSpecificOutput = [ordered]@{
                hookEventName = 'PreToolUse'; permissionDecision = 'deny'
                permissionDecisionReason = 'CHECKPOINT_ORDER_BLOCKED: the canonical checkpoint must remain valid JSON.'
            } }
    }

    if (-not $payload.PSObject.Properties.Name -contains 'completed_steps') {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $steps = @()
    if ($payload.completed_steps) {
        foreach ($s in $payload.completed_steps) {
            $steps += [string]$s
        }
    }

    $rollbackHistory = $null
    if ($payload.PSObject.Properties.Name -contains 'rollback_history') {
        $rollbackHistory = $payload.rollback_history
    }
    if ($rollbackHistory -and @($rollbackHistory).Count -gt 0) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $pair = if ($steps.Count -ge 2) { Get-OutOfOrderPair -CompletedSteps $steps } else { $null }
    if ($null -ne $pair) {
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = "CHECKPOINT_ORDER_BLOCKED: completed_steps lists '$($pair.EarlierEntry)' at position $($pair.EarlierPos) before '$($pair.LaterEntry)' at position $($pair.LaterPos), but the canonical orchestrator workflow requires the later step to follow the earlier one. Reorder completed_steps or, if a rollback occurred, record it in rollback_history."
            }
        }
    }

    $missingPrerequisite = Get-MissingPrerequisiteForAdvancedStep -CompletedSteps $steps
    if ($null -ne $missingPrerequisite) {
        $missing = @()
        if ($missingPrerequisite.MissingPromotion) {
            $missing += 'S3_promotion'
        }
        if ($missingPrerequisite.MissingPlanning) {
            $missing += 'S4_atomic_planning'
        }
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = "CHECKPOINT_ORDER_BLOCKED: completed_steps lists '$($missingPrerequisite.Step)' before required prerequisite step(s): $($missing -join ', '). Record promotion and planning completion before implementation, review, PR, CI, or DONE steps."
            }
        }
    }

    return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    # Transport and mapping come from the shared module. Update reconstruction is
    # requested for the governed checkpoint path only: an Update touching any
    # other file yields no record and therefore allows, instead of failing the
    # whole invocation because an unrelated source could not be read. A governed
    # reconstruction failure yields empty content, which routes into the existing
    # fail-closed deny below rather than exit 2.
    $payload = ConvertFrom-CodexPreToolUsePayload -PayloadRaw ([Console]::In.ReadToEnd()) -HookName 'enforce-checkpoint-monotonic'
    $mappedInputs = @(
        ConvertTo-CodexFileEditInput -Payload $payload -ResolveUpdateContent -GovernedPath $script:GovernedCheckpointPath
    )
    foreach ($toolInput in $mappedInputs) {
        $toolInputRaw = $toolInput | ConvertTo-Json -Compress -Depth 20
        $decision = Invoke-CheckpointMonotonicDecision -ToolInputRaw $toolInputRaw
        if ($decision.hookSpecificOutput.permissionDecision -eq 'deny') {
            $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
            exit 0
        }
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
