<#
.SYNOPSIS
    Enforces the planning-only boundary for route_id preparation.

.DESCRIPTION
    PreToolUse hook for Bash, apply_patch, and drm-copilot MCP tools. When the local canonical
    orchestrator checkpoint selects preparation, only planning artifacts, lifecycle MCP calls,
    read-only inspection, and the preparation commit sequence are permitted.
#>
[CmdletBinding()]
param()

$script:AllowedPreparationMcpTools = @(
    'mcp__drm-copilot__new_potential_entry',
    'mcp__drm-copilot__new_potential_bug_entry',
    'mcp__drm-copilot__potential_to_issue',
    'mcp__drm-copilot__new_active_feature_folder',
    'mcp__drm-copilot__resolve_atomic_plan_prompt',
    'mcp__drm-copilot__resolve_execute_hard_lock_prompt',
    'mcp__drm-copilot__validate_orchestration_artifacts'
)

function ConvertFrom-EpicPlanningJson {
    [CmdletBinding()]
    param(
        [AllowNull()][AllowEmptyString()][string] $Raw,
        [Parameter(Mandatory)][string] $Name,
        [switch] $Optional
    )

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        if ($Optional) {
            return $null
        }
        throw "EPIC_PLANNING_ONLY_BLOCKED: $Name is empty."
    }
    try {
        return $Raw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "EPIC_PLANNING_ONLY_BLOCKED: $Name is malformed JSON: $_"
    }
}

function Get-EpicPlanningDenyDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([Parameter(Mandatory)][string] $Reason)

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = "EPIC_PLANNING_ONLY_BLOCKED: $Reason"
        }
    }
}

function Get-EpicPlanningPatchPath {
    [CmdletBinding()]
    [OutputType([string[]])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Patch)

    $paths = [System.Collections.Generic.List[string]]::new()
    foreach ($match in [regex]::Matches(
            $Patch,
            '(?m)^\*\*\* (?:(?:Add|Update|Delete) File:|Move to:)\s*(?<path>[^\r\n]+)\r?$'
        )) {
        $paths.Add((([string]$match.Groups['path'].Value) -replace '\\', '/').Trim())
    }
    return [string[]]$paths
}

function Test-EpicPlanningArtifactPath {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][string] $Path)

    $normalized = ($Path -replace '\\', '/').Trim()
    if ([System.IO.Path]::IsPathRooted($Path) -or
        $normalized -match '(^|/)\.\.(?:/|$)' -or
        $normalized -match '^[A-Za-z]:' -or
        $normalized.StartsWith('//')) {
        return $false
    }
    $normalized = $normalized -replace '^\./', ''
    if ($normalized -match '(^|/)docs/features/(?:active|potential)/') {
        return $normalized -match '\.(?:md|json|txt)$'
    }
    if ($normalized -match '(^|/)artifacts/(?:orchestration|research)/') {
        if ($normalized -match '(?i)(?:epic-root-invocation|codex-routing-attestation|codex-worktree-(?:launch|wave))') {
            return $false
        }
        return $normalized -match '\.(?:md|json|txt)$'
    }
    return $false
}

function Test-EpicPlanningBashAllowed {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string] $Command,
        [AllowNull()][string[]] $StagedPaths,
        [AllowNull()][AllowEmptyString()][string] $CurrentBranch
    )

    if (-not $Command -or $Command -match '[;\r\n|&<>`]' -or $Command -match '\$\(' -or
        $Command -match '(?i)(?:^|\s)--(?:output|exec|ext-diff|textconv|upload-pack|receive-pack|pre)(?:=|\s|$)') {
        return $false
    }
    $allowedPatterns = @(
        '^\s*git\s+(?:status|diff|log|show|rev-parse|fetch|ls-files)\b',
        '^\s*git\s+worktree\s+list\b',
        '^\s*gh\s+(?:issue|pr)\s+view\b',
        '^\s*(?:rg|Get-Content|Get-ChildItem|Test-Path|Select-String)\b',
        '^\s*(?:poetry\s+run\s+)?(?:python|py)\s+-m\s+scripts\.dev_tools\.(?:validate_[A-Za-z0-9_.-]+|epic_wave_computation)\b'
    )
    foreach ($pattern in $allowedPatterns) {
        if ($Command -match $pattern) {
            return $true
        }
    }

    if ($Command -match '^\s*git\s+add(?:\s+--)?\s+(?<paths>.+?)\s*$') {
        $tokens = [regex]::Matches($Matches.paths, '"[^"]+"|''[^'']+''|\S+')
        if ($tokens.Count -eq 0) {
            return $false
        }
        foreach ($token in $tokens) {
            $path = ([string]$token.Value).Trim('"', "'")
            if ($path.StartsWith('-') -or -not (Test-EpicPlanningArtifactPath -Path $path)) {
                return $false
            }
        }
        return $true
    }
    $quotedMessage = '(?:"[^"\r\n]+"|''[^''\r\n]+'')'
    $commitPattern = "^\s*git\s+commit\s+(?:-m\s+$quotedMessage|--message(?:=|\s+)$quotedMessage)\s*`$"
    if ($Command -match $commitPattern) {
        if ($null -eq $StagedPaths -or $StagedPaths.Count -eq 0) {
            return $false
        }
        foreach ($path in $StagedPaths) {
            if (-not (Test-EpicPlanningArtifactPath -Path $path)) {
                return $false
            }
        }
        return $true
    }
    if (-not [string]::IsNullOrWhiteSpace($CurrentBranch)) {
        $escapedBranch = [regex]::Escape($CurrentBranch)
        if ($Command -match "^\s*git\s+push(?:\s+(?:-u|--set-upstream))?\s+origin\s+$escapedBranch\s*`$") {
            return $true
        }
    }
    return $false
}

function Invoke-EpicPlanningOnlyDecision {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $PayloadRaw,
        [AllowNull()][AllowEmptyString()][string] $CheckpointRaw,
        [AllowNull()][AllowEmptyString()][string] $EpicExecutionContext,
        [AllowNull()][string[]] $StagedPaths,
        [AllowNull()][AllowEmptyString()][string] $CurrentBranch
    )

    $payload = ConvertFrom-EpicPlanningJson -Raw $PayloadRaw -Name 'PreToolUse input'
    $attestedPreparation = $EpicExecutionContext -eq 'epic_preparation_child'
    if ([string]::IsNullOrWhiteSpace($CheckpointRaw) -and -not $attestedPreparation) {
        return $null
    }
    $checkpoint = if ([string]::IsNullOrWhiteSpace($CheckpointRaw)) {
        [pscustomobject]@{ route_id = 'preparation' }
    } else {
        ConvertFrom-EpicPlanningJson -Raw $CheckpointRaw -Name 'orchestrator checkpoint'
    }
    if ([string]$checkpoint.route_id -ne 'preparation' -and $attestedPreparation) {
        return Get-EpicPlanningDenyDecision -Reason 'an attested preparation child must keep route_id set to preparation.'
    }
    if ([string]$checkpoint.route_id -ne 'preparation') {
        return $null
    }

    $toolName = [string]$payload.tool_name
    if ($toolName -eq 'apply_patch') {
        $patch = [string]$payload.tool_input.command
        if ($patch -match '(?mi)^\*\*\* Delete File:\s*.*artifacts[\\/]orchestration[\\/]orchestrator-state\.json\s*$') {
            return Get-EpicPlanningDenyDecision -Reason 'the canonical preparation checkpoint may not be deleted.'
        }
        if ($patch -match '(?mi)^\*\*\* Update File:\s*.*artifacts[\\/]orchestration[\\/]orchestrator-state\.json\s*$' -and
            $patch -match '(?mi)^\*\*\* Move to:') {
            return Get-EpicPlanningDenyDecision -Reason 'the canonical preparation checkpoint may not be moved.'
        }
        if ($patch -match '(?m)^-\s*"route_id"\s*:\s*"preparation"') {
            return Get-EpicPlanningDenyDecision -Reason 'the canonical preparation route may not be removed or changed.'
        }
        if ($patch -match '(?mi)^\*\*\* Update File:\s*.*artifacts[\\/]orchestration[\\/]orchestrator-state\.json\s*$' -and
            $patch -match '(?m)^\+[^\r\n]*"route_id"\s*:') {
            return Get-EpicPlanningDenyDecision -Reason 'the canonical preparation checkpoint may not add or duplicate route_id.'
        }
        if ($patch -match '(?m)^\+\s*"step(?:5|6|7|8|9|10)_status"\s*:\s*"(?!not-applicable")') {
            return Get-EpicPlanningDenyDecision -Reason 'execution-through-CI statuses must remain not-applicable during preparation.'
        }
        $paths = @(Get-EpicPlanningPatchPath -Patch $patch)
        if ($paths.Count -eq 0) {
            return Get-EpicPlanningDenyDecision -Reason 'preparation apply_patch input must identify planning artifact paths.'
        }
        foreach ($path in $paths) {
            if (-not (Test-EpicPlanningArtifactPath -Path $path)) {
                return Get-EpicPlanningDenyDecision -Reason "preparation may not edit '$path'; only feature planning documents and orchestration/research artifacts are writable."
            }
        }
        return $null
    }

    if ($toolName -eq 'Bash') {
        $command = [string]$payload.tool_input.command
        if (Test-EpicPlanningBashAllowed -Command $command -StagedPaths $StagedPaths -CurrentBranch $CurrentBranch) {
            return $null
        }
        return Get-EpicPlanningDenyDecision -Reason 'the Bash command is outside the preparation read, branch, commit, push, or validator allowlist.'
    }

    if ($toolName -like 'mcp__*' -and $toolName -notlike 'mcp__drm-copilot__*') {
        return Get-EpicPlanningDenyDecision -Reason "MCP tool '$toolName' is not part of the repository-scoped preparation surface."
    }

    if ($toolName -like 'mcp__drm-copilot__*') {
        if ($attestedPreparation) {
            $workspaceRoot = [string]$payload.tool_input.workspace_root
            if ([string]::IsNullOrWhiteSpace($workspaceRoot) -or
                [string]::IsNullOrWhiteSpace([string]$payload.cwd) -or
                [System.IO.Path]::GetFullPath($workspaceRoot) -ne [System.IO.Path]::GetFullPath([string]$payload.cwd)) {
                return Get-EpicPlanningDenyDecision -Reason 'preparation MCP calls require workspace_root equal to the attested session cwd.'
            }
        }
        if ($script:AllowedPreparationMcpTools -contains $toolName) {
            return $null
        }
        return Get-EpicPlanningDenyDecision -Reason "MCP tool '$toolName' is outside the preparation lifecycle and validator allowlist."
    }

    return Get-EpicPlanningDenyDecision -Reason "tool '$toolName' is not classified for preparation mode."
}

function Invoke-EpicPlanningGit {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string[]] $GitArgs)

    return & git @GitArgs 2>$null
}

function Get-EpicPlanningCurrentBranch {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $RepositoryRoot)

    $currentBranch = [string](Invoke-EpicPlanningGit -GitArgs @('-C', $RepositoryRoot, 'branch', '--show-current'))
    if ($LASTEXITCODE -ne 0) {
        throw 'EPIC_PLANNING_ONLY_BLOCKED: current branch could not be resolved before push.'
    }
    if ([string]::IsNullOrWhiteSpace($currentBranch)) {
        return ''
    }
    return $currentBranch.Trim()
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payloadRaw = [Console]::In.ReadToEnd()
    $repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
    $checkpointPath = Join-Path $repositoryRoot 'artifacts/orchestration/orchestrator-state.json'
    $checkpointRaw = if (Test-Path -LiteralPath $checkpointPath -PathType Leaf) {
        Get-Content -Raw -LiteralPath $checkpointPath
    } else {
        ''
    }
    $payload = ConvertFrom-EpicPlanningJson -Raw $payloadRaw -Name 'PreToolUse input'
    $stagedPaths = $null
    $currentBranch = ''
    if ([string]$payload.tool_name -eq 'Bash' -and
        [string]$payload.tool_input.command -match '^\s*git\s+commit\b') {
        $stagedPaths = @(& git -C $repositoryRoot diff --cached --name-only --diff-filter=ACMR 2>$null)
        if ($LASTEXITCODE -ne 0) {
            throw 'EPIC_PLANNING_ONLY_BLOCKED: staged paths could not be resolved before commit.'
        }
    }
    if ([string]$payload.tool_name -eq 'Bash' -and
        [string]$payload.tool_input.command -match '^\s*git\s+push\b') {
        $currentBranch = Get-EpicPlanningCurrentBranch -RepositoryRoot $repositoryRoot
    }
    $decision = Invoke-EpicPlanningOnlyDecision `
        -PayloadRaw $payloadRaw `
        -CheckpointRaw $checkpointRaw `
        -EpicExecutionContext ([string]$env:CODEX_EPIC_CHILD_EXECUTION_CONTEXT) `
        -StagedPaths $stagedPaths `
        -CurrentBranch $currentBranch
    if ($null -ne $decision) {
        $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
