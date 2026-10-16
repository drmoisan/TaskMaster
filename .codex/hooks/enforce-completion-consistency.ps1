<#
.SYNOPSIS
    Pre-tool-use hook that blocks Write operations on the orchestrator checkpoint
    file when the checkpoint asserts completion without verifiable completion
    evidence.

.DESCRIPTION
    Invoked by the Codex PreToolUse hook on apply_patch operations. The hook
    reads one JSON object from stdin and activates only when the target path normalizes to
    artifacts/orchestration/orchestrator-state.json.

    A written checkpoint asserts completion when any of the following hold:

      next_step == "complete"
      completed_steps contains "S12_complete"
      step8_status, step9_status, or step10_status == "completed"

    When completion is asserted, the write is blocked unless ALL of the following
    completion evidence is present:

      - a non-empty issue-num (top-level, falling back to variables.issue-num);
      - a non-empty feature-folder (top-level, falling back to
        variables.feature-folder);
      - a ci_gate object with conclusion == "success" and a non-empty head_sha.

    When completion evidence is missing the hook emits a PreToolUse JSON
    response with hookSpecificOutput.permissionDecision='deny' and a reason that
    names the specific missing evidence so the caller can remediate. When
    completion is not asserted, the write is allowed via
    hookSpecificOutput.permissionDecision='allow' (backward compatibility).

    Edit tool calls supply only old_string/new_string (a partial patch). An Edit
    call is validated by reading the checkpoint at the Edit's targeted file_path
    through the injectable CheckpointReader seam and applying the old_string ->
    new_string replacement in memory; the resulting content is then evaluated
    like a Write. One occurrence of old_string is replaced unless replace_all is
    true. The hook fails closed: a missing, empty, or unreadable targeted
    checkpoint, an absent or empty old_string, zero or ambiguous occurrences of
    old_string, and a Write with empty content all deny, and the deny reason
    names the cause. Write content or patched content that is not valid JSON is
    also denied, because the canonical checkpoint is required to stay valid JSON.

.NOTES
    Compatible with PowerShell 7+. Read-only validation gate.
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }

# Reuse the adjacent hook's shared checkpoint policy logic. This dot-source is by
# design, not a copy-paste defect, so the hook is not renamed.
try { . (Join-Path $PSScriptRoot 'enforce-checkpoint-monotonic.ps1') } catch { Add-HookDependencyFailure -Name 'enforce-checkpoint-monotonic.ps1' -ErrorRecord $_ }

# Shared Codex PreToolUse transport. Dot-sourced explicitly rather than relying on
# the transitive load above, so this hook's transport does not depend on its
# neighbour's internals. Every transport error it raises names this hook, because
# the shared parser is called with -HookName 'enforce-completion-consistency'.
try { . (Join-Path $PSScriptRoot 'codex-pretooluse-file-mapping.ps1') } catch { Add-HookDependencyFailure -Name 'codex-pretooluse-file-mapping.ps1' -ErrorRecord $_ }

# Dot-source the shared validation helpers. Guarded so a missing file produces a
# clear error and so dot-sourcing this hook in tests loads the helpers too.
try { . (Join-Path $PSScriptRoot 'enforce-completion-helpers.ps1') } catch { Add-HookDependencyFailure -Name 'enforce-completion-helpers.ps1' -ErrorRecord $_ }

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

function Get-CheckpointFileContent {
    <#
    .SYNOPSIS
        Reads the on-disk checkpoint content for the read-then-validate Edit path.
    .DESCRIPTION
        Returns the full file text when the path resolves to a file on disk, or
        $null when the file does not exist. An existing empty file yields an
        empty string, never $null, so a missing checkpoint and an empty
        checkpoint stay distinct. Tests inject a CheckpointReader scriptblock
        instead of mocking this function so no temporary files are required.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    $text = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop
    if ($null -eq $text) { return '' }
    return [string]$text
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

function Get-CheckpointStringValue {
    <#
    .SYNOPSIS
        Returns a trimmed string for a payload property, or an empty string when
        the property is absent or its value is not a non-empty string.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        $Payload,

        [Parameter(Mandatory)]
        [string] $Name
    )

    if ($null -eq $Payload) {
        return ''
    }
    if (-not ($Payload.PSObject.Properties.Name -contains $Name)) {
        return ''
    }

    $value = $Payload.$Name
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Trim()
}

function Test-CompletionAsserted {
    <#
    .SYNOPSIS
        Returns $true when the checkpoint payload asserts completion via
        next_step, completed_steps, or step8/9/10 status fields.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        $Payload
    )

    if ($null -eq $Payload) {
        return $false
    }

    $nextStep = Get-CheckpointStringValue -Payload $Payload -Name 'next_step'
    if ($nextStep -eq 'complete') {
        return $true
    }

    if ($Payload.PSObject.Properties.Name -contains 'completed_steps' -and $Payload.completed_steps) {
        foreach ($step in $Payload.completed_steps) {
            if (([string]$step) -eq 'S12_complete') {
                return $true
            }
        }
    }

    foreach ($statusField in @('step8_status', 'step9_status', 'step10_status')) {
        $status = Get-CheckpointStringValue -Payload $Payload -Name $statusField
        if ($status -eq 'completed') {
            return $true
        }
    }

    return $false
}

function Get-MissingCompletionEvidence {
    <#
    .SYNOPSIS
        Returns the list of missing completion-evidence descriptions for a
        completion-asserting checkpoint. An empty list means all evidence is
        present.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        $Payload,

        [Parameter(Mandatory = $false)]
        [scriptblock] $FolderExistsCheck = { param($p) Test-Path -LiteralPath $p -PathType Container },

        [Parameter(Mandatory = $false)]
        [scriptblock] $RoutingMatrixReader
    )

    $missing = @()

    $issueNum = Get-CheckpointStringValue -Payload $Payload -Name 'issue-num'
    if (-not $issueNum -and $null -ne $Payload -and ($Payload.PSObject.Properties.Name -contains 'variables')) {
        $issueNum = Get-CheckpointStringValue -Payload $Payload.variables -Name 'issue-num'
    }
    if (-not (Test-IsValidIssueNum -Value $issueNum)) {
        # Name the offending value so sentinel/placeholder inputs are explicit.
        $missing += "issue-num value '$issueNum' is not a valid issue number (must be digits-only)"
    }

    $featureFolder = Get-CheckpointStringValue -Payload $Payload -Name 'feature-folder'
    if (-not $featureFolder -and $null -ne $Payload -and ($Payload.PSObject.Properties.Name -contains 'variables')) {
        $featureFolder = Get-CheckpointStringValue -Payload $Payload.variables -Name 'feature-folder'
    }
    if (-not (Test-IsValidFeatureFolder -Value $featureFolder -FolderExistsCheck $FolderExistsCheck)) {
        $missing += "feature-folder value '$featureFolder' is not a valid feature folder (must be under docs/features/active/ and exist)"
    }

    $ciGate = $null
    if ($null -ne $Payload -and ($Payload.PSObject.Properties.Name -contains 'ci_gate')) {
        $ciGate = $Payload.ci_gate
    }
    if ($null -eq $ciGate -or $ciGate -isnot [System.Management.Automation.PSCustomObject]) {
        $missing += 'ci_gate (object with conclusion == "success" and non-empty head_sha)'
    }
    else {
        $conclusion = Get-CheckpointStringValue -Payload $ciGate -Name 'conclusion'
        if ($conclusion -ne 'success') {
            $missing += 'ci_gate.conclusion == "success"'
        }
        $headSha = Get-CheckpointStringValue -Payload $ciGate -Name 'head_sha'
        if (-not $headSha) {
            $missing += 'ci_gate.head_sha'
        }
    }

    # PR-gate evidence is required only when the checkpoint's selected route
    # opts into it via requires_pr_gate in the routing matrix. This replaces the
    # former issue-number special-casing with route-driven enforcement.
    $prGateArgs = @{ Payload = $Payload }
    if ($PSBoundParameters.ContainsKey('RoutingMatrixReader') -and $null -ne $RoutingMatrixReader) {
        $prGateArgs['RoutingMatrixReader'] = $RoutingMatrixReader
    }
    if (Test-RouteRequiresPrGate @prGateArgs) {
        $prGate = $null
        if ($null -ne $Payload -and ($Payload.PSObject.Properties.Name -contains 'pr_gate')) {
            $prGate = $Payload.pr_gate
        }
        if ($null -eq $prGate -or $prGate -isnot [System.Management.Automation.PSCustomObject]) {
            $missing += 'pr_gate (object with pr_number, pr_url, head_branch, and head_sha)'
        }
        else {
            foreach ($field in @('pr_number', 'pr_url', 'head_branch', 'head_sha')) {
                if (-not (Get-CheckpointStringValue -Payload $prGate -Name $field)) {
                    $missing += "pr_gate.$field"
                }
            }
            $prHeadSha = Get-CheckpointStringValue -Payload $prGate -Name 'head_sha'
            $ciHeadSha = Get-CheckpointStringValue -Payload $ciGate -Name 'head_sha'
            if ($prHeadSha -and $ciHeadSha -and $prHeadSha -ne $ciHeadSha) {
                $missing += 'ci_gate.head_sha matching pr_gate.head_sha'
            }
        }
    }

    return [string[]]$missing
}

function Resolve-EditedCheckpointContent {
    <#
    .SYNOPSIS
        Returns an object with Content and Failure for an Edit-tool call: the
        patched checkpoint content on success, or a failure cause when the patch
        cannot be applied against the targeted checkpoint.
    .DESCRIPTION
        Implements the read-then-validate Edit path. The checkpoint at the
        caller-supplied CheckpointPath (the Edit's targeted file_path, passed to
        the reader unchanged) is read through the injectable CheckpointReader seam
        and the old_string -> new_string replacement is applied in memory (no
        on-disk mutation). One occurrence of old_string is replaced unless
        replace_all is true. Failure is one of no-old_string (absent or empty
        old_string), checkpoint-unreadable (the reader threw), checkpoint-missing
        (the reader returned $null), checkpoint-empty (the reader returned an
        empty string), old_string-not-found, or old_string-ambiguous. Content is
        $null on failure and Failure is $null on success. The caller denies on
        any Failure.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        $ToolInput,

        [Parameter(Mandatory)]
        [scriptblock] $CheckpointReader,

        [Parameter(Mandatory)]
        [string] $CheckpointPath
    )

    $oldString = $null
    if ($null -ne $ToolInput -and ($ToolInput.PSObject.Properties.Name -contains 'old_string')) {
        $oldString = [string]$ToolInput.old_string
    }
    if ([string]::IsNullOrEmpty($oldString)) {
        return [pscustomobject]@{ Content = $null; Failure = 'no-old_string' }
    }

    $newString = ''
    if ($ToolInput.PSObject.Properties.Name -contains 'new_string') {
        $newString = [string]$ToolInput.new_string
    }

    try {
        $onDisk = & $CheckpointReader $CheckpointPath
    }
    catch {
        # A reader exception must not escape the gate; report it as a failure cause.
        return [pscustomobject]@{ Content = $null; Failure = 'checkpoint-unreadable' }
    }
    if ($null -eq $onDisk) {
        return [pscustomobject]@{ Content = $null; Failure = 'checkpoint-missing' }
    }

    $onDiskText = [string]$onDisk
    if ($onDiskText.Length -eq 0) {
        return [pscustomobject]@{ Content = $null; Failure = 'checkpoint-empty' }
    }

    # Apply the patch in memory; one occurrence unless replace_all is true.
    $replaceAll = Get-EditReplaceAllFlag -ToolInput $ToolInput
    return Invoke-SingleOccurrenceEdit -Text $onDiskText -OldString $oldString -NewString $newString -ReplaceAll $replaceAll
}

function ConvertTo-CompletionDenyDecision {
    <#
    .SYNOPSIS
        Builds the PreToolUse deny decision for the given reason.
    #>
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

function Invoke-CompletionConsistencyDecision {
    <#
    .SYNOPSIS
        Parses mapped Codex tool_input JSON and returns an allow-or-deny decision based on
        completion-evidence consistency.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw,

        [Parameter(Mandatory = $false)]
        [scriptblock] $FolderExistsCheck = { param($p) Test-Path -LiteralPath $p -PathType Container },

        [Parameter(Mandatory = $false)]
        [scriptblock] $CheckpointReader = { param($Path) Get-CheckpointFileContent -Path $Path },

        [Parameter(Mandatory = $false)]
        [scriptblock] $RoutingMatrixReader
    )
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'COMPLETION_CONSISTENCY_BLOCKED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    if (-not $ToolInputRaw) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw "enforce-completion-consistency received malformed mapped tool_input JSON: $_"
    }

    $filePath = $toolInput.file_path
    if (-not $filePath) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $normalized = $filePath -replace '\\', '/'
    if (-not (Test-IsCheckpointPath -NormalizedPath $normalized)) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    # Write tool: a present content property is validated directly, and empty
    # content is denied. Edit tool: no content property is supplied, so read the
    # targeted checkpoint through the injectable seam and apply the old_string ->
    # new_string patch in memory (read-then-validate). Any failure cause denies.
    $cause = $null
    if ($toolInput.PSObject.Properties.Name -contains 'content') {
        $content = $toolInput.content
        if (-not $content) {
            $cause = 'write-content-empty'
        }
    }
    else {
        $edit = Resolve-EditedCheckpointContent -ToolInput $toolInput -CheckpointReader $CheckpointReader -CheckpointPath $filePath
        $content = $edit.Content
        $cause = $edit.Failure
    }
    if ($cause) {
        return ConvertTo-CompletionDenyDecision -Reason "COMPLETION_CONSISTENCY_BLOCKED: the canonical checkpoint cannot be deleted, emptied, or replaced through an unresolved patch ($cause)."
    }

    try {
        $payload = ConvertFrom-CheckpointJson -Json $content
    }
    catch {
        return [ordered]@{ hookSpecificOutput = [ordered]@{
                hookEventName = 'PreToolUse'; permissionDecision = 'deny'
                permissionDecisionReason = 'COMPLETION_CONSISTENCY_BLOCKED: the canonical checkpoint must remain valid JSON.'
            } }
    }

    if (-not (Test-CompletionAsserted -Payload $payload)) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $missingArgs = @{ Payload = $payload; FolderExistsCheck = $FolderExistsCheck }
    if ($PSBoundParameters.ContainsKey('RoutingMatrixReader') -and $null -ne $RoutingMatrixReader) {
        $missingArgs['RoutingMatrixReader'] = $RoutingMatrixReader
    }
    $missing = Get-MissingCompletionEvidence @missingArgs
    if ($missing.Count -eq 0) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $reason = "COMPLETION_CONSISTENCY_BLOCKED: the checkpoint asserts completion but is missing required completion evidence: $($missing -join ', '). A completion-asserting checkpoint must include a non-empty issue-num, a non-empty feature-folder, and a ci_gate object with conclusion == 'success' and a non-empty head_sha; routes whose requires_pr_gate is true must also include pr_gate evidence with a matching head_sha. Supply the missing evidence or remove the completion assertion."
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
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('COMPLETION_CONSISTENCY_BLOCKED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'COMPLETION_CONSISTENCY_BLOCKED:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

try {
    # -HookName makes every transport error name this hook rather than the
    # neighbour it dot-sources. Update reconstruction is scoped to the governed
    # checkpoint path for the same reason as in enforce-checkpoint-monotonic.
    $payload = ConvertFrom-CodexPreToolUsePayload -PayloadRaw ([Console]::In.ReadToEnd()) -HookName 'enforce-completion-consistency'
    $mappedInputs = @(
        ConvertTo-CodexFileEditInput -Payload $payload -ResolveUpdateContent -GovernedPath $script:GovernedCheckpointPath
    )
    foreach ($toolInput in $mappedInputs) {
        $toolInputRaw = $toolInput | ConvertTo-Json -Compress -Depth 20
        $decision = Invoke-CompletionConsistencyDecision -ToolInputRaw $toolInputRaw
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
