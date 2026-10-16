<#
.SYNOPSIS
    Pre-tool-use hook that routes PowerShell changes of more than three production files to the orchestrated large path.

.DESCRIPTION
    This script is invoked by the Claude Code PreToolUse hook before any Write or Edit
    operation. When the target file is a PowerShell source file (.ps1, .psm1, .psd1),
    it decides whether the change may proceed in direct mode.

    Direct mode counts distinct production PowerShell paths per Claude Code session.
    The running set is persisted under .claude/state/powershell-batch-budget.<session_id>.json,
    and repeated edits to the same file are counted once. The 4th distinct production
    path is denied with a POWERSHELL_LARGE_PATH_REQUIRED reason that instructs the
    caller to route the change through /orchestrate.

    The orchestrated large path is detected from
    <root>/artifacts/orchestration/orchestrator-state.json. The selected route is the
    route_id value when that key is present, otherwise the path_selected value, and it
    is usable only as a non-blank string. When the selected route is large,
    remediation, or preparation and the checkpoint is not terminal (next_step is not
    complete and completed_steps does not contain S12_complete), no path is denied for
    count and no state is written. Every other checkpoint outcome, including an absent,
    unreadable, or malformed checkpoint, enforces direct mode.

    Test files are never counted. They are those matching:
      - tests/**/*.ps1
      - *.Tests.ps1

    All other .ps1/.psm1/.psd1 files are production files. Non-PowerShell paths pass
    through. The threshold of three production files is a routing constant and is not
    configurable at runtime. Legacy prodCap, testCap, and testFiles keys in a persisted
    state file are ignored when the state is loaded.

    The session id is resolved from the first non-empty of: the CLAUDE_SESSION_ID
    environment variable; the contents of <root>/.claude/state/current-session-id;
    a worktree-derived identifier built from the root's leaf name and a short
    stable hash of its normalized path. The resolved value is sanitized before it
    is composed into a file name, so a hostile id cannot escape the state
    directory. Resolving the id never creates the state directory.

    Candidate paths are contained to the resolved root. A candidate that resolves
    outside it is discarded: the decision is 'allow', no slot is consumed, and no
    state is written. Persisted entries that fail the same containment test are
    dropped when state is rehydrated, so a state file carried between worktrees
    cannot spend this worktree's budget.

    When a production path is denied, the script emits a PreToolUse JSON response with
    hookSpecificOutput.permissionDecision = 'deny' and exits 0. Files already counted
    are always allowed through.

    Known limitation: a stale non-terminal large-path checkpoint left at the root
    exempts a later direct-mode session at that root. Orchestrator checkpoint hygiene
    (issue #673) moves foreign checkpoints aside before a new run.

.NOTES
    Compatible with PowerShell 7+.
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }


try { Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'HookPayload.psm1' -ErrorRecord $_ }
try { . (Join-Path $PSScriptRoot 'enforce-batch-budget-route.ps1') } catch { Add-HookDependencyFailure -Name 'enforce-batch-budget-route.ps1' -ErrorRecord $_ }

function Test-PowerShellBatchBudgetPathInRoot {
    <#
    .SYNOPSIS
        Reports whether a candidate path belongs to the batch-budget root.
    .DESCRIPTION
        Compares forward-slash-normalized forms of the candidate and the root,
        case-insensitively. A relative candidate carries no root of its own and is
        admitted, which is what keeps a relative path recorded by one worktree from
        being treated as foreign by another.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Path,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $Root
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $false
    }

    $normalizedPath = $Path -replace '\\', '/'
    if ($normalizedPath -notmatch '^(/|[A-Za-z]:/)') {
        return $true
    }

    $normalizedRoot = ($Root -replace '\\', '/').TrimEnd('/')
    if ([string]::IsNullOrWhiteSpace($normalizedRoot)) {
        return $true
    }

    return ([string]::Equals($normalizedPath, $normalizedRoot, [System.StringComparison]::OrdinalIgnoreCase) -or $normalizedPath.StartsWith($normalizedRoot + '/', [System.StringComparison]::OrdinalIgnoreCase))
}

function ConvertTo-PowerShellBatchBudgetSafeSegment {
    <#
    .SYNOPSIS
        Reduces a session id to characters that are safe in a file name.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Value
    )

    return ($Value -replace '[^A-Za-z0-9._-]', '_')
}

function Get-PowerShellBatchBudgetSessionId {
    <#
    .SYNOPSIS
        Resolves the session id used to compose the batch-budget state-file name.
    .DESCRIPTION
        Returns the first non-empty of: the explicit SessionId argument; the
        CLAUDE_SESSION_ID environment variable; the contents of the session-id state
        file; a worktree-derived identifier. The result is sanitized so it cannot
        escape the state directory. The session-id state file is read through the
        ReadSessionIdFile seam and is never created, so resolution performs no write.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $SessionId,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $Root,

        [Parameter(Mandatory)]
        [string] $SessionIdFilePath,

        [scriptblock] $ReadSessionIdFile
    )

    $candidates = @(
        $SessionId
        $env:CLAUDE_SESSION_ID
    )

    foreach ($candidate in $candidates) {
        if (-not [string]::IsNullOrWhiteSpace($candidate)) {
            return (ConvertTo-PowerShellBatchBudgetSafeSegment -Value $candidate.Trim())
        }
    }

    $fromFile = ''
    try {
        $fromFile = [string](& $ReadSessionIdFile $SessionIdFilePath)
    } catch {
        Write-Verbose "Ignoring unreadable session-id file '$SessionIdFilePath': $($_.Exception.Message)"
        $fromFile = ''
    }

    if (-not [string]::IsNullOrWhiteSpace($fromFile)) {
        return (ConvertTo-PowerShellBatchBudgetSafeSegment -Value $fromFile.Trim())
    }

    $normalizedRoot = ($Root -replace '\\', '/').TrimEnd('/')
    $leaf = ConvertTo-PowerShellBatchBudgetSafeSegment -Value (Split-Path -Path $normalizedRoot -Leaf)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($normalizedRoot))
    } finally {
        $sha.Dispose()
    }
    $shortHash = -join (@($hashBytes[0..3]) | ForEach-Object { $_.ToString('x2') })

    return "worktree-$leaf-$shortHash"
}

function Get-PowerShellBatchBudgetState {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSReviewUnusedParameter', 'TestCap', Justification = 'Accepted and ignored for callers written against the removed test-file cap.')]
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [int] $ProdCap,

        [int] $TestCap = 0
    )

    [ordered]@{
        prodCap   = $ProdCap
        prodFiles = @()
    }
}

function ConvertTo-PowerShellBatchBudgetState {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSReviewUnusedParameter', 'TestCap', Justification = 'Accepted and ignored for callers written against the removed test-file cap.')]
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        $InputObject,

        [Parameter(Mandatory)]
        [int] $ProdCap,

        [int] $TestCap = 0,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $Root = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent)
    )

    # Only prodFiles is carried over. Persisted prodCap, testCap, and testFiles keys
    # come from the removed test-file cap and runtime overrides and are ignored.
    $state = Get-PowerShellBatchBudgetState -ProdCap $ProdCap

    # Persisted entries that resolve outside this root belong to another worktree
    # and are dropped, so a state file carried across worktrees cannot spend this
    # worktree's budget.
    if ($null -ne $InputObject.prodFiles) {
        $state.prodFiles = @(@($InputObject.prodFiles) | Where-Object { Test-PowerShellBatchBudgetPathInRoot -Path $_ -Root $Root })
    }

    return $state
}

function Get-PowerShellBatchBudgetBlockDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Reason,

        [System.Collections.IDictionary] $State
    )

    $decision = [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
    if ($State) {
        $decision.state = $State
    }

    return $decision
}

function Invoke-PowerShellBatchBudgetDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $FilePath,

        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $State,

        [AllowEmptyString()]
        [string] $StateFile = '',

        [AllowNull()]
        [AllowEmptyString()]
        [string] $Root = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent),

        [switch] $LargePathRoute,

        [AllowEmptyString()]
        [string] $ObservedRoute = ''
    )
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'POWERSHELL_LARGE_PATH_REQUIRED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    $normalized = $FilePath -replace '\\', '/'
    if ($normalized -notmatch '\.(ps1|psm1|psd1)$') {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $false }
    }

    # An out-of-root candidate is discarded rather than denied: it consumes no
    # slot and writes no state, so this hook stays deny-only for real overruns.
    if (-not (Test-PowerShellBatchBudgetPathInRoot -Path $normalized -Root $Root)) {
        Write-Verbose "Discarding batch-budget candidate '$normalized': it resolves outside the batch-budget root '$Root'."
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $false }
    }

    # The orchestrated large path has no production-file cap, so nothing is counted.
    if ($LargePathRoute) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $false }
    }

    # Test files never count toward the routing threshold.
    $isTestFile = ($normalized -match '(^|/)tests/.*\.ps1$') -or ($normalized -match '\.Tests\.ps1$')
    if ($isTestFile) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $false }
    }

    $countedFiles = @($State.prodFiles)
    if ($countedFiles -contains $normalized) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $false }
    }

    $cap = [int]$State.prodCap
    if ($countedFiles.Count -ge $cap) {
        Write-Verbose "Denying production PowerShell path '$normalized' in direct mode; counted paths are recorded in '$StateFile'."
        $counted = $countedFiles -join ', '
        $route = if ([string]::IsNullOrWhiteSpace($ObservedRoute)) { 'none' } else { $ObservedRoute }
        $reason = "POWERSHELL_LARGE_PATH_REQUIRED: this change touches more than $cap production PowerShell files (already counted: $counted; requested: $normalized). A change of this size belongs on the orchestrated large path, which has no production-file cap. Route the change through /orchestrate. Checkpoint route observed: $route."
        return Get-PowerShellBatchBudgetBlockDecision -Reason $reason -State $State
    }

    $State.prodFiles = $countedFiles + @($normalized)

    return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $true }
}

function Invoke-PowerShellBatchBudgetHook {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw,
        [string] $SessionId = '',
        [string] $Root = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent),
        [int] $ProdCap = 3,
        [scriptblock] $ReadSessionIdFile = {
            param([string] $Path)
            if (Test-Path -LiteralPath $Path -PathType Leaf) {
                return (Get-Content -LiteralPath $Path -Raw)
            }
            return ''
        },
        [scriptblock] $ReadCheckpoint = {
            param([string] $Path)
            if (Test-Path -LiteralPath $Path -PathType Leaf) {
                return (Get-Content -LiteralPath $Path -Raw)
            }
            return ''
        },
        [scriptblock] $TestPathExists = { param([string] $Path) Test-Path -Path $Path },
        [scriptblock] $EnsureDirectory = { param([string] $Path) New-Item -ItemType Directory -Path $Path -Force | Out-Null },
        [scriptblock] $ReadState = { param([string] $Path) Get-Content -Path $Path -Raw },
        [scriptblock] $WriteState = {
            param([string] $Path, [System.Collections.IDictionary] $State)
            $State | ConvertTo-Json -Depth 5 | Set-Content -Path $Path -Encoding UTF8
        }
    )

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-PowerShellBatchBudgetBlockDecision -Reason (
            'PowerShell batch-budget hook received an unreadable PreToolUse envelope: ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $filePath = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'file_path'
    if (-not $filePath) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $normalized = $filePath -replace '\\', '/'
    if ($normalized -notmatch '\.(ps1|psm1|psd1)$') {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $stateDir = Join-Path -Path $Root -ChildPath '.claude/state'

    # Resolved before the directory is ensured, because reading the session-id
    # file must never be what creates the state directory.
    $resolvedSessionId = Get-PowerShellBatchBudgetSessionId `
        -SessionId $SessionId `
        -Root $Root `
        -SessionIdFilePath (Join-Path -Path $stateDir -ChildPath 'current-session-id') `
        -ReadSessionIdFile $ReadSessionIdFile

    $stateFile = Join-Path -Path $stateDir -ChildPath ("powershell-batch-budget.$resolvedSessionId.json")

    # The checkpoint is read before any state operation, so the large path neither
    # creates the state directory nor reads or writes the state file.
    $checkpointPath = Join-Path -Path $Root -ChildPath 'artifacts/orchestration/orchestrator-state.json'
    $checkpointText = ''
    try {
        $checkpointText = [string](& $ReadCheckpoint $checkpointPath)
    } catch {
        Write-Verbose "Treating unreadable orchestrator checkpoint '$checkpointPath' as direct mode: $($_.Exception.Message)"
        $checkpointText = ''
    }

    $isLargePath = Test-BatchBudgetLargePathRoute -CheckpointText $checkpointText
    $observedRoute = Get-BatchBudgetSelectedRoute -CheckpointText $checkpointText
    if ($isLargePath) {
        return Invoke-PowerShellBatchBudgetDecision -FilePath $filePath -State (Get-PowerShellBatchBudgetState -ProdCap $ProdCap) -StateFile $stateFile -Root $Root -LargePathRoute
    }

    if (-not (& $TestPathExists $stateDir)) {
        & $EnsureDirectory $stateDir
    }

    $state = Get-PowerShellBatchBudgetState -ProdCap $ProdCap

    if (& $TestPathExists $stateFile) {
        try {
            $loaded = & $ReadState $stateFile | ConvertFrom-Json -ErrorAction Stop
            $state = ConvertTo-PowerShellBatchBudgetState -InputObject $loaded -ProdCap $ProdCap -Root $Root
        } catch {
            Write-Verbose "Ignoring unreadable PowerShell batch-budget state file '$stateFile': $($_.Exception.Message)"
        }
    }

    $decision = Invoke-PowerShellBatchBudgetDecision -FilePath $filePath -State $state -StateFile $stateFile -Root $Root -ObservedRoute $observedRoute
    if ($decision.shouldWriteState) {
        try {
            & $WriteState $stateFile $decision.state
        } catch {
            Write-Verbose "Unable to write PowerShell batch-budget state file '$stateFile': $($_.Exception.Message)"
        }
    }

    return $decision
}

function Invoke-PowerShellBatchBudgetEntryPoint {
    <#
    .SYNOPSIS
        Runs the PowerShell batch-budget decision and returns the process exit code.
    .DESCRIPTION
        Wraps the dispatch logic that the hook entry point performs so it can be
        exercised by unit tests. It acquires the payload through the shared reader
        unless the caller supplies one, writes the compact JSON decision to the output
        stream only when the decision is a deny (this hook is deny-only: an allow
        decision emits nothing), and returns 0. This function does not call exit; the
        thin entry-point wiring converts the returned code into a process exit.
    .PARAMETER ToolInputRaw
        Optional pre-acquired payload text. When omitted the ReadPayload seam runs.
    .PARAMETER ReadPayload
        Seam for payload acquisition, so tests can drive the empty-on-all-transports
        case without touching a console.
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw,

        [scriptblock] $ReadPayload = { Read-ClaudeHookRawPayload }
    )

    if (-not $PSBoundParameters.ContainsKey('ToolInputRaw')) {
        $ToolInputRaw = [string](& $ReadPayload)
    }

    # No literal fallback here: an empty value routes through the hook's session
    # resolution, which falls back to the session-id state file and then to a
    # worktree-derived identifier.
    $sessionId = [string]$env:CLAUDE_SESSION_ID

    $decision = Invoke-PowerShellBatchBudgetHook -ToolInputRaw $ToolInputRaw -SessionId $sessionId
    if ($decision.hookSpecificOutput.permissionDecision -eq 'deny') {
        $decision.Remove('state')
        $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }

    return 0
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('POWERSHELL_LARGE_PATH_REQUIRED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'POWERSHELL_LARGE_PATH_REQUIRED:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

# The entry point returns its [int] exit code as the last pipeline element and the
# decision JSON before it. `exit (<call>)` would capture BOTH into the exit
# expression and emit nothing, so the decision is written explicitly here first.
$entryPointResult = @(Invoke-PowerShellBatchBudgetEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])
