# Validate inherited external-launch authority for routed epic child subagents.

$contractPath = Join-Path (Split-Path $PSScriptRoot -Parent) 'scripts/epic-child-launch-contract.ps1'
if (Test-Path -LiteralPath $contractPath -PathType Leaf) {
    . $contractPath
}

$script:CodexEpicChildContexts = @('epic_preparation_child', 'epic_execution_child')

function Get-CodexEpicChildLaunchEnvironment {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param()

    return [pscustomobject]@{
        launch_id         = [string]$env:CODEX_EPIC_CHILD_LAUNCH_ID
        receipt_path      = [string]$env:CODEX_EPIC_CHILD_LAUNCH_RECEIPT
        spec_path         = [string]$env:CODEX_EPIC_CHILD_LAUNCH_SPEC
        worktree_path     = [string]$env:CODEX_EPIC_CHILD_EXPECTED_WORKTREE
        delegation_id     = [string]$env:CODEX_EPIC_CHILD_DELEGATION_ID
        execution_context = [string]$env:CODEX_EPIC_CHILD_EXECUTION_CONTEXT
        deployment_agent  = [string]$env:CODEX_EPIC_CHILD_AGENT
        model             = [string]$env:CODEX_EPIC_CHILD_MODEL
        reasoning_effort  = [string]$env:CODEX_EPIC_CHILD_REASONING_EFFORT
        profile_sha256    = [string]$env:CODEX_EPIC_CHILD_PROFILE_SHA256
    }
}

function Get-CodexEpicChildCanonicalPath {
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

function Test-CodexEpicChildRoutingLaunchAuthority {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()] $RoutingReceipt,
        [Parameter(Mandatory)] $Payload,
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [AllowNull()] $LaunchEnvironment,
        [AllowNull()][AllowEmptyString()][string] $LaunchReceiptRaw,
        [datetimeoffset] $Now = [datetimeoffset]::UtcNow
    )

    $context = if ($null -eq $RoutingReceipt) { '' } else {
        [string]$RoutingReceipt.execution_context
    }
    if ($script:CodexEpicChildContexts -cnotcontains $context) {
        return $true
    }
    $environment = if ($null -eq $LaunchEnvironment) {
        Get-CodexEpicChildLaunchEnvironment
    } else {
        $LaunchEnvironment
    }
    try {
        $receiptPath = Get-CodexEpicChildCanonicalPath -Path ([string]$environment.receipt_path) -BasePath $RepositoryRoot
        $raw = if ($PSBoundParameters.ContainsKey('LaunchReceiptRaw')) {
            $LaunchReceiptRaw
        } else {
            Get-Content -Raw -LiteralPath $receiptPath -ErrorAction Stop
        }
        $receipt = $raw | ConvertFrom-Json -Depth 32 -ErrorAction Stop
        if ((Get-CodexChildActiveReceiptErrorList -Receipt $receipt -ExpectedSessionId ([string]$Payload.session_id) -Now $Now).Count -gt 0) {
            return $false
        }

        $comparison = if ($IsWindows) {
            [System.StringComparison]::OrdinalIgnoreCase
        } else {
            [System.StringComparison]::Ordinal
        }
        $worktree = Get-CodexEpicChildCanonicalPath -Path ([string]$receipt.worktree_path) -BasePath $RepositoryRoot
        $trustedRoot = Get-CodexEpicChildCanonicalPath -Path ([string]$receipt.trusted_repository_root) -BasePath $RepositoryRoot
        $launchRoot = Join-Path $trustedRoot 'artifacts/orchestration/epic-child-launches'
        $specPath = Get-CodexEpicChildCanonicalPath -Path ([string]$receipt.spec_path) -BasePath $trustedRoot
        $checkpointName = if ($context -ceq 'epic_preparation_child') {
            'epic-planner-state.json'
        } else {
            'epic-orchestrator-state.json'
        }
        $expectedCheckpoint = Join-Path $trustedRoot "artifacts/orchestration/$checkpointName"
        $pathPairs = @(
            @($receiptPath, (Get-CodexEpicChildCanonicalPath -Path ([string]$receipt.receipt_path) -BasePath $trustedRoot)),
            @((Get-CodexEpicChildCanonicalPath -Path ([string]$environment.spec_path) -BasePath $trustedRoot), $specPath),
            @((Get-CodexEpicChildCanonicalPath -Path ([string]$environment.worktree_path) -BasePath $RepositoryRoot), $worktree)
        )
        foreach ($pair in $pathPairs) {
            if (-not ([string]$pair[0]).Equals([string]$pair[1], $comparison)) {
                return $false
            }
        }
        $identityPairs = @(
            @([string]$environment.launch_id, [string]$receipt.launch_id),
            @([string]$environment.delegation_id, [string]$receipt.delegation_id),
            @([string]$environment.execution_context, [string]$receipt.execution_context),
            @([string]$environment.deployment_agent, [string]$receipt.deployment_agent),
            @([string]$environment.model, [string]$receipt.model),
            @([string]$environment.reasoning_effort, [string]$receipt.model_reasoning_effort),
            @([string]$environment.profile_sha256, [string]$receipt.profile_sha256)
        )
        foreach ($pair in $identityPairs) {
            if (-not ([string]$pair[0]).Equals([string]$pair[1], [System.StringComparison]::Ordinal)) {
                return $false
            }
        }
        $repository = Get-CodexEpicChildCanonicalPath -Path $RepositoryRoot -BasePath $RepositoryRoot
        $checkpoint = Get-CodexEpicChildCanonicalPath -Path ([string]$receipt.checkpoint_path) -BasePath $trustedRoot
        return $repository.Equals($worktree, $comparison) -and
        $context -ceq [string]$receipt.execution_context -and
        $checkpoint.Equals($expectedCheckpoint, $comparison) -and
        $receiptPath.StartsWith($launchRoot + [System.IO.Path]::DirectorySeparatorChar, $comparison) -and
        $specPath.StartsWith($launchRoot + [System.IO.Path]::DirectorySeparatorChar, $comparison)
    } catch {
        return $false
    }
}
