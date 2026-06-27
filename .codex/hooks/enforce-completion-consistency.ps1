<#
.SYNOPSIS
    Pre-tool-use hook that blocks Write operations on the orchestrator checkpoint
    file when the checkpoint asserts completion without verifiable completion
    evidence.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on Write or Edit operations. The
    hook activates only when the target file_path normalizes to
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

    The block reason names the specific missing evidence so the caller can
    remediate. When completion is not asserted, the write is allowed
    (backward compatibility).

    Edit tool calls supply only old_string/new_string (a partial patch) and
    cannot be reliably validated without the full target file content, so they
    are allowed by this hook, matching enforce-checkpoint-monotonic.ps1. For
    Write calls whose content is not valid JSON, the hook allows the operation
    and defers to downstream tools to surface the error.

.NOTES
    Compatible with PowerShell 7+. Read-only validation gate.
#>
[CmdletBinding()]
param()

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
        $Payload
    )

    $missing = @()

    $issueNum = Get-CheckpointStringValue -Payload $Payload -Name 'issue-num'
    if (-not $issueNum -and $null -ne $Payload -and ($Payload.PSObject.Properties.Name -contains 'variables')) {
        $issueNum = Get-CheckpointStringValue -Payload $Payload.variables -Name 'issue-num'
    }
    if (-not $issueNum) {
        $missing += 'issue-num'
    }

    $featureFolder = Get-CheckpointStringValue -Payload $Payload -Name 'feature-folder'
    if (-not $featureFolder -and $null -ne $Payload -and ($Payload.PSObject.Properties.Name -contains 'variables')) {
        $featureFolder = Get-CheckpointStringValue -Payload $Payload.variables -Name 'feature-folder'
    }
    if (-not $featureFolder) {
        $missing += 'feature-folder'
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

    if ($issueNum -eq '232') {
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

function Invoke-CompletionConsistencyDecision {
    <#
    .SYNOPSIS
        Parses CLAUDE_TOOL_INPUT and returns an allow-or-block decision based on
        completion-evidence consistency.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw
    )

    if (-not $ToolInputRaw) {
        return [ordered]@{ decision = 'allow' }
    }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw "enforce-completion-consistency hook received malformed JSON in CLAUDE_TOOL_INPUT: $_"
    }

    $filePath = $toolInput.file_path
    if (-not $filePath) {
        return [ordered]@{ decision = 'allow' }
    }

    $normalized = $filePath -replace '\\', '/'
    if (-not (Test-IsCheckpointPath -NormalizedPath $normalized)) {
        return [ordered]@{ decision = 'allow' }
    }

    # Write tool: validate the content payload. Edit tool: partial new_string is
    # not reliable without the full target file content, so allow.
    $content = $toolInput.content
    if (-not $content) {
        return [ordered]@{ decision = 'allow' }
    }

    try {
        $payload = ConvertFrom-CheckpointJson -Json $content
    }
    catch {
        # The content itself is not valid JSON. Let downstream tools surface the
        # error rather than blocking with a misleading reason here.
        return [ordered]@{ decision = 'allow' }
    }

    if (-not (Test-CompletionAsserted -Payload $payload)) {
        return [ordered]@{ decision = 'allow' }
    }

    $missing = Get-MissingCompletionEvidence -Payload $payload
    if ($missing.Count -eq 0) {
        return [ordered]@{ decision = 'allow' }
    }

    $issueContext = ''
    if ((Get-CheckpointStringValue -Payload $payload -Name 'issue-num') -eq '232') {
        $issueContext = ' Issue #232 requires pr_gate evidence and current-head ci_gate evidence.'
    }

    return [ordered]@{
        decision = 'block'
        reason   = "COMPLETION_CONSISTENCY_BLOCKED: the checkpoint asserts completion but is missing required completion evidence: $($missing -join ', ').$issueContext A completion-asserting checkpoint must include a non-empty issue-num, a non-empty feature-folder, and a ci_gate object with conclusion == 'success' and a non-empty head_sha. Supply the missing evidence or remove the completion assertion."
    }
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $decision = Invoke-CompletionConsistencyDecision -ToolInputRaw $env:CLAUDE_TOOL_INPUT
}
catch {
    Write-Error $_
    exit 1
}

$decision | ConvertTo-Json -Compress | Write-Output

exit 0
