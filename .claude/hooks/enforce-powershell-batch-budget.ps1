<#
.SYNOPSIS
    Pre-tool-use hook that enforces the PowerShell per-batch change budget.

.DESCRIPTION
    This script is invoked by the Claude Code PreToolUse hook before any Write or Edit
    operation. When the target file is a PowerShell source file (.ps1, .psm1, .psd1),
    it classifies the file as either production or test and checks the running count
    against the per-batch cap:
      - 3 production PowerShell files per batch
      - 3 test PowerShell files per batch

    A "batch" is scoped to the current Claude Code session. The running count is
    persisted under .claude/state/powershell-batch-budget.<session_id>.json. Only
    distinct file paths are counted; repeated edits to the same file consume one slot.

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

    Test files are those matching:
      - tests/**/*.ps1
      - *.Tests.ps1

    All other .ps1/.psm1/.psd1 files are treated as production files. Non-PowerShell
    paths pass through.

    The cap may be overridden per session by setting the environment variable
    CLAUDE_POWERSHELL_BUDGET_PROD or CLAUDE_POWERSHELL_BUDGET_TEST to a positive integer
    before the session starts, or by writing {"prodCap": N, "testCap": M} into the
    state file.

    When the cap would be exceeded by a new file, the script emits a PreToolUse JSON
    response with hookSpecificOutput.permissionDecision = 'deny' and exits 0. The session
    must explicitly reset the counter by deleting the state file before starting a new
    batch. Files already counted are always allowed through.

.NOTES
    Compatible with PowerShell 7+.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force

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
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [int] $ProdCap,

        [Parameter(Mandatory)]
        [int] $TestCap
    )

    [ordered]@{
        prodCap   = $ProdCap
        testCap   = $TestCap
        prodFiles = @()
        testFiles = @()
    }
}

function ConvertTo-PowerShellBatchBudgetState {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        $InputObject,

        [Parameter(Mandatory)]
        [int] $ProdCap,

        [Parameter(Mandatory)]
        [int] $TestCap,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $Root = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent)
    )

    $state = Get-PowerShellBatchBudgetState -ProdCap $ProdCap -TestCap $TestCap
    if ($null -ne $InputObject.prodCap) { $state.prodCap = [int]$InputObject.prodCap }
    if ($null -ne $InputObject.testCap) { $state.testCap = [int]$InputObject.testCap }

    # Persisted entries that resolve outside this root belong to another worktree
    # and are dropped, so a state file carried across worktrees cannot spend this
    # worktree's budget.
    if ($null -ne $InputObject.prodFiles) {
        $state.prodFiles = @(@($InputObject.prodFiles) | Where-Object { Test-PowerShellBatchBudgetPathInRoot -Path $_ -Root $Root })
    }
    if ($null -ne $InputObject.testFiles) {
        $state.testFiles = @(@($InputObject.testFiles) | Where-Object { Test-PowerShellBatchBudgetPathInRoot -Path $_ -Root $Root })
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

        [Parameter(Mandatory)]
        [string] $StateFile,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $Root = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent)
    )

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

    $isTestFile = ($normalized -match '(^|/)tests/.*\.ps1$') -or ($normalized -match '\.Tests\.ps1$')
    $targetList = if ($isTestFile) { @($State.testFiles) } else { @($State.prodFiles) }
    $cap = if ($isTestFile) { [int]$State.testCap } else { [int]$State.prodCap }
    $kind = if ($isTestFile) { 'test' } else { 'production' }

    if ($targetList -contains $normalized) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $false }
    }

    if ($targetList.Count -ge $cap) {
        $currentFiles = ($targetList -join ', ')
        $kindUpper = $kind.ToUpperInvariant()
        $reason = "PowerShell per-batch budget exceeded: $kind file cap is $cap and is already full ($currentFiles). Requested new file: $normalized. Split the work into a new batch, raise the cap via CLAUDE_POWERSHELL_BUDGET_$kindUpper environment variable with approved scope, or reset the batch by deleting $StateFile."
        return Get-PowerShellBatchBudgetBlockDecision -Reason $reason -State $State
    }

    if ($isTestFile) {
        $State.testFiles = @($State.testFiles) + @($normalized)
    } else {
        $State.prodFiles = @($State.prodFiles) + @($normalized)
    }

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
        [int] $TestCap = 3,
        [scriptblock] $ReadSessionIdFile = {
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

    if (-not (& $TestPathExists $stateDir)) {
        & $EnsureDirectory $stateDir
    }

    $stateFile = Join-Path -Path $stateDir -ChildPath ("powershell-batch-budget.$resolvedSessionId.json")
    $state = Get-PowerShellBatchBudgetState -ProdCap $ProdCap -TestCap $TestCap

    if (& $TestPathExists $stateFile) {
        try {
            $loaded = & $ReadState $stateFile | ConvertFrom-Json -ErrorAction Stop
            $state = ConvertTo-PowerShellBatchBudgetState -InputObject $loaded -ProdCap $ProdCap -TestCap $TestCap -Root $Root
        } catch {
            Write-Verbose "Ignoring unreadable PowerShell batch-budget state file '$stateFile': $($_.Exception.Message)"
        }
    }

    $decision = Invoke-PowerShellBatchBudgetDecision -FilePath $filePath -State $state -StateFile $stateFile -Root $Root
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

    $prodCap = 3
    $testCap = 3
    if ($env:CLAUDE_POWERSHELL_BUDGET_PROD -match '^\d+$') {
        $prodCap = [int]$env:CLAUDE_POWERSHELL_BUDGET_PROD
    }
    if ($env:CLAUDE_POWERSHELL_BUDGET_TEST -match '^\d+$') {
        $testCap = [int]$env:CLAUDE_POWERSHELL_BUDGET_TEST
    }

    $decision = Invoke-PowerShellBatchBudgetHook -ToolInputRaw $ToolInputRaw -SessionId $sessionId -ProdCap $prodCap -TestCap $testCap
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

# The entry point returns its [int] exit code as the last pipeline element and the
# decision JSON before it. `exit (<call>)` would capture BOTH into the exit
# expression and emit nothing, so the decision is written explicitly here first.
$entryPointResult = @(Invoke-PowerShellBatchBudgetEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])
