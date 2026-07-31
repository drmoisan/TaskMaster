<#
.SYNOPSIS
    Denies epic-child mutations outside the launch-authorized worktree and profile.

.DESCRIPTION
    A launcher-created environment attestation activates this guard. The guard fails closed when
    the hook repository root, payload working directory, live branch, immutable launch receipt,
    specification hash, deployment profile, model, reasoning effort, or delegation differs from
    the launch authorization. Every drm-copilot MCP call must name the authorized workspace_root.
#>
[CmdletBinding()]
param()

$contractPath = Join-Path (Split-Path $PSScriptRoot -Parent) 'scripts/epic-child-launch-contract.ps1'
if (Test-Path -LiteralPath $contractPath -PathType Leaf) {
    . $contractPath
}

function ConvertFrom-CodexChildGuardJson {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Raw, [Parameter(Mandatory)][string] $Name)

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        throw "EPIC_WORKTREE_BINDING_BLOCKED: $Name is empty."
    }
    try {
        return $Raw | ConvertFrom-Json -Depth 32 -ErrorAction Stop
    } catch {
        throw "EPIC_WORKTREE_BINDING_BLOCKED: $Name is malformed JSON: $_"
    }
}

function Get-CodexChildGuardCanonicalPath {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $Path, [Parameter(Mandatory)][string] $BasePath)

    $candidate = if ([System.IO.Path]::IsPathFullyQualified($Path)) {
        $Path
    } else {
        Join-Path $BasePath $Path
    }
    return [System.IO.Path]::GetFullPath($candidate).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar
    )
}

function Get-CodexChildGuardDenyDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([Parameter(Mandatory)][string] $Reason)

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = "EPIC_WORKTREE_BINDING_BLOCKED: $Reason"
        }
    }
}

function Test-CodexChildGuardMutation {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)] $Payload)

    $toolName = [string]$Payload.tool_name
    if ($toolName -in @('apply_patch', 'Edit', 'Write')) {
        return $true
    }
    if ($toolName -like 'mcp__*') {
        return $true
    }
    if ($toolName -in @('Bash', 'shell_command')) {
        return $true
    }
    return $false
}

function Test-CodexChildGuardProtectedPathMutation {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)] $Payload,
        [Parameter(Mandatory)][string] $ReceiptPath,
        [Parameter(Mandatory)][string] $SpecPath,
        [Parameter(Mandatory)][string] $ProfilePath,
        [Parameter(Mandatory)][string] $WorktreePath
    )

    if (-not (Test-CodexChildGuardMutation -Payload $Payload)) {
        return $false
    }
    $inputText = (($Payload.tool_input | ConvertTo-Json -Depth 16 -Compress) -replace '\\+', '/').ToLowerInvariant()
    if ($inputText -match 'codex_epic_child|(?:^|[/"''])(?:\.codex|\.agents)(?:[/"'']|$)|agents\.md|orchestration-routing\.json') {
        return $true
    }
    foreach ($path in @(
            $ReceiptPath, (Split-Path $ReceiptPath -Parent), $SpecPath, $ProfilePath,
            (Join-Path $WorktreePath '.codex/config.toml'),
            (Join-Path $WorktreePath '.codex/hooks'),
            (Join-Path $WorktreePath '.codex/scripts'),
            (Join-Path $WorktreePath '.agents'),
            (Join-Path $WorktreePath 'AGENTS.md'),
            (Join-Path $WorktreePath 'config/orchestration-routing.json')
        )) {
        $normalized = $path.Replace('\', '/').ToLowerInvariant()
        if ($inputText.Contains($normalized) -or $inputText.Contains((Split-Path $normalized -Leaf))) {
            return $true
        }
    }
    return $false
}

function Test-CodexChildGuardNestedCodexInvocation {
    [OutputType([bool])]
    param([Parameter(Mandatory)] $Payload)
    if ([string]$Payload.tool_name -notin @('Bash', 'shell_command')) {
        return $false
    }
    $command = [string]$Payload.tool_input.command
    return $command -match '(?i)(?:^|[\s;&|`"''])codex(?:\.exe|\.cmd|\.ps1)?(?:[\s;&|`"'']|$)|@openai[/\\]codex|codex\.js'
}

function Test-CodexChildGuardAttestation {
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory)] $Payload,
        [Parameter(Mandatory)] $Receipt,
        [Parameter(Mandatory)] $Attestation,
        [Parameter(Mandatory)][string] $HookRepositoryRoot,
        [Parameter(Mandatory)][string] $LiveBranch,
        [Parameter(Mandatory)][string] $ActualSpecSha256,
        [Parameter(Mandatory)][string] $ActualProfileSha256
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $expectedWorktree = Get-CodexChildGuardCanonicalPath -Path ([string]$Receipt.worktree_path) -BasePath $HookRepositoryRoot
    $actualRoot = Get-CodexChildGuardCanonicalPath -Path $HookRepositoryRoot -BasePath $HookRepositoryRoot
    $payloadCwd = if ([string]::IsNullOrWhiteSpace([string]$Payload.cwd)) {
        ''
    } else {
        Get-CodexChildGuardCanonicalPath -Path ([string]$Payload.cwd) -BasePath $actualRoot
    }
    if ($actualRoot -ne $expectedWorktree -or $payloadCwd -ne $expectedWorktree) {
        $errors.Add('payload.cwd, hook repository root, and expected worktree must match exactly.')
    }
    if ([string]$LiveBranch -cne [string]$Receipt.branch_name) {
        $errors.Add('the live Git branch does not match the launch receipt.')
    }
    $pairs = @(
        @('launch_id', 'launch_id'),
        @('delegation_id', 'delegation_id'),
        @('execution_context', 'execution_context'),
        @('agent', 'deployment_agent'),
        @('model', 'model'),
        @('reasoning_effort', 'model_reasoning_effort'),
        @('profile_sha256', 'profile_sha256'),
        @('expected_worktree', 'worktree_path'),
        @('receipt_path', 'receipt_path'),
        @('spec_path', 'spec_path')
    )
    foreach ($pair in $pairs) {
        if ([string]$Attestation.($pair[0]) -cne [string]$Receipt.($pair[1])) {
            $errors.Add("environment attestation $($pair[0]) does not match the launch receipt.")
        }
    }
    if (Get-Command Get-CodexChildActiveReceiptErrorList -ErrorAction SilentlyContinue) {
        foreach ($receiptError in @(Get-CodexChildActiveReceiptErrorList -Receipt $Receipt `
                    -ExpectedSessionId ([string]$Payload.session_id))) {
            $errors.Add($receiptError)
        }
    } else {
        $errors.Add('launch receipt validator is unavailable.')
    }
    if ([string]$ActualSpecSha256 -ne [string]$Receipt.spec_sha256) {
        $errors.Add('the immutable launch specification hash has changed.')
    }
    $expectedProfile = Join-Path $expectedWorktree ".codex/agents/$($Receipt.deployment_agent).toml"
    if ((Get-CodexChildGuardCanonicalPath -Path ([string]$Receipt.profile_path) -BasePath $expectedWorktree) -ne
        (Get-CodexChildGuardCanonicalPath -Path $expectedProfile -BasePath $expectedWorktree) -or
        [string]$ActualProfileSha256 -ne [string]$Receipt.profile_sha256) {
        $errors.Add('the checked-in deployment profile path or hash has changed.')
    }
    if (-not [string]::IsNullOrWhiteSpace([string]$Payload.model) -and
        [string]$Payload.model -cne [string]$Receipt.model) {
        $errors.Add('the hook payload model does not match the launch receipt.')
    }
    if (-not [string]::IsNullOrWhiteSpace([string]$Payload.agent_type) -and
        [string]$Payload.agent_type -cne [string]$Receipt.deployment_agent) {
        $errors.Add('the hook payload agent type does not match the launch receipt.')
    }
    $payloadReasoning = if (@($Payload.PSObject.Properties.Name) -contains 'model_reasoning_effort') {
        [string]$Payload.model_reasoning_effort
    } elseif (@($Payload.PSObject.Properties.Name) -contains 'reasoning_effort') {
        [string]$Payload.reasoning_effort
    } else {
        ''
    }
    if (-not [string]::IsNullOrWhiteSpace($payloadReasoning) -and
        $payloadReasoning -cne [string]$Receipt.model_reasoning_effort) {
        $errors.Add('the hook payload reasoning effort does not match the launch receipt.')
    }
    return $errors.ToArray()
}

function Invoke-CodexEpicChildGuardDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)][string] $PayloadRaw,
        [AllowNull()][AllowEmptyString()][string] $ReceiptRaw,
        [Parameter(Mandatory)] $Attestation,
        [Parameter(Mandatory)][string] $HookRepositoryRoot,
        [Parameter(Mandatory)][AllowEmptyString()][string] $LiveBranch,
        [Parameter(Mandatory)][AllowEmptyString()][string] $ActualSpecSha256,
        [Parameter(Mandatory)][AllowEmptyString()][string] $ActualProfileSha256
    )

    $payload = ConvertFrom-CodexChildGuardJson -Raw $PayloadRaw -Name 'PreToolUse input'
    if ([string]::IsNullOrWhiteSpace([string]$Attestation.launch_id)) {
        return $null
    }
    $isMcp = [string]$payload.tool_name -like 'mcp__*'
    $isMutation = Test-CodexChildGuardMutation -Payload $payload
    if (-not $isMcp -and -not $isMutation) {
        return $null
    }
    if ([string]::IsNullOrWhiteSpace($ReceiptRaw)) {
        return Get-CodexChildGuardDenyDecision -Reason 'the launcher authorization receipt is missing.'
    }
    $receipt = ConvertFrom-CodexChildGuardJson -Raw $ReceiptRaw -Name 'launch receipt'
    if ($isMcp -and [string]$payload.tool_name -notlike 'mcp__drm-copilot__*') {
        return Get-CodexChildGuardDenyDecision -Reason 'only repository-scoped drm-copilot MCP tools are authorized for an epic child.'
    }
    if (Test-CodexChildGuardNestedCodexInvocation -Payload $payload) {
        return Get-CodexChildGuardDenyDecision -Reason 'nested Codex execution is prohibited inside an epic child.'
    }
    if ($isMcp) {
        $workspaceRoot = [string]$payload.tool_input.workspace_root
        $expected = Get-CodexChildGuardCanonicalPath -Path ([string]$receipt.worktree_path) -BasePath $HookRepositoryRoot
        if ([string]::IsNullOrWhiteSpace($workspaceRoot) -or
            (Get-CodexChildGuardCanonicalPath -Path $workspaceRoot -BasePath $HookRepositoryRoot) -ne $expected) {
            return Get-CodexChildGuardDenyDecision -Reason 'drm-copilot MCP calls require workspace_root equal to the authorized worktree.'
        }
    }
    if (Test-CodexChildGuardProtectedPathMutation -Payload $payload `
            -ReceiptPath ([string]$receipt.receipt_path) -SpecPath ([string]$receipt.spec_path) `
            -ProfilePath ([string]$receipt.profile_path) -WorktreePath ([string]$receipt.worktree_path)) {
        return Get-CodexChildGuardDenyDecision -Reason 'the launch authorization or committed Codex customizations cannot be mutated by the child.'
    }
    $errors = Test-CodexChildGuardAttestation -Payload $payload -Receipt $receipt -Attestation $Attestation `
        -HookRepositoryRoot $HookRepositoryRoot -LiveBranch $LiveBranch -ActualSpecSha256 $ActualSpecSha256 `
        -ActualProfileSha256 $ActualProfileSha256
    if ($errors.Count -gt 0) {
        return Get-CodexChildGuardDenyDecision -Reason ($errors -join ' ')
    }
    return $null
}

function Invoke-CodexChildGuardGit {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string[]] $GitArgs)

    return & git @GitArgs 2>&1
}

function Get-CodexChildGuardLiveBranch {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $RepositoryRoot)

    $liveBranch = [string](Invoke-CodexChildGuardGit -GitArgs @('-C', $RepositoryRoot, 'branch', '--show-current'))
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($liveBranch)) {
        return ''
    }
    return $liveBranch.Trim()
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payloadRaw = [Console]::In.ReadToEnd()
    $repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
    $attestation = [pscustomobject]@{
        launch_id         = [string]$env:CODEX_EPIC_CHILD_LAUNCH_ID
        receipt_path      = [string]$env:CODEX_EPIC_CHILD_LAUNCH_RECEIPT
        spec_path         = [string]$env:CODEX_EPIC_CHILD_LAUNCH_SPEC
        expected_worktree = [string]$env:CODEX_EPIC_CHILD_EXPECTED_WORKTREE
        delegation_id     = [string]$env:CODEX_EPIC_CHILD_DELEGATION_ID
        execution_context = [string]$env:CODEX_EPIC_CHILD_EXECUTION_CONTEXT
        agent             = [string]$env:CODEX_EPIC_CHILD_AGENT
        model             = [string]$env:CODEX_EPIC_CHILD_MODEL
        reasoning_effort  = [string]$env:CODEX_EPIC_CHILD_REASONING_EFFORT
        profile_sha256    = [string]$env:CODEX_EPIC_CHILD_PROFILE_SHA256
    }
    $receiptRaw = if (-not [string]::IsNullOrWhiteSpace($attestation.receipt_path) -and
        (Test-Path -LiteralPath $attestation.receipt_path -PathType Leaf)) {
        Get-Content -Raw -LiteralPath $attestation.receipt_path
    } else {
        ''
    }
    $actualSpecSha256 = if (-not [string]::IsNullOrWhiteSpace($attestation.spec_path) -and
        (Test-Path -LiteralPath $attestation.spec_path -PathType Leaf)) {
        (Get-FileHash -LiteralPath $attestation.spec_path -Algorithm SHA256).Hash.ToLowerInvariant()
    } else {
        ''
    }
    $receipt = if ([string]::IsNullOrWhiteSpace($receiptRaw)) { $null } else {
        ConvertFrom-CodexChildGuardJson -Raw $receiptRaw -Name 'launch receipt'
    }
    $actualProfileSha256 = if ($null -ne $receipt -and
        -not [string]::IsNullOrWhiteSpace([string]$receipt.profile_path) -and
        (Test-Path -LiteralPath ([string]$receipt.profile_path) -PathType Leaf)) {
        (Get-FileHash -LiteralPath ([string]$receipt.profile_path) -Algorithm SHA256).Hash.ToLowerInvariant()
    } else {
        ''
    }
    $liveBranch = Get-CodexChildGuardLiveBranch -RepositoryRoot $repositoryRoot
    $decision = Invoke-CodexEpicChildGuardDecision -PayloadRaw $payloadRaw -ReceiptRaw $receiptRaw `
        -Attestation $attestation -HookRepositoryRoot $repositoryRoot -LiveBranch $liveBranch `
        -ActualSpecSha256 $actualSpecSha256 -ActualProfileSha256 $actualProfileSha256
    if ($null -ne $decision) {
        $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
