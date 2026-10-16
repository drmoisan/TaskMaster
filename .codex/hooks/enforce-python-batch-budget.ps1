
<#
.SYNOPSIS
    Pre-tool-use hook that routes Python changes of more than three production files to the orchestrated large path.

.DESCRIPTION
    This script is invoked by the Codex PreToolUse hook before any Write or Edit
    operation. When the target file is a Python file (.py), it decides whether the
    change may proceed in direct mode.

    Direct mode counts distinct production Python paths per Codex session. The running
    set is persisted under .codex/state/python-batch-budget.<session_id>.json, and
    repeated edits to the same file are counted once. The 4th distinct production path
    is denied with a PYTHON_LARGE_PATH_REQUIRED reason that instructs the caller to
    route the change through .codex/prompts/orchestrate-work.md.

    The orchestrated large path is detected from
    <root>/artifacts/orchestration/orchestrator-state.json. The selected route is the
    route_id value when that key is present, otherwise the path_selected value, and it
    is usable only as a non-blank string. When the selected route is large,
    remediation, or preparation and the checkpoint is not terminal (next_step is not
    complete and completed_steps does not contain S12_complete), no path is denied for
    count and no state is read or written. Every other checkpoint outcome, including
    an absent, unreadable, or malformed checkpoint, enforces direct mode.

    Test files are never counted. They are those matching:
      - tests/**/*.py
      - test_*.py

    All other .py files are production files. Non-Python paths pass through. The
    threshold of three production files is a routing constant and is not configurable
    at runtime. Legacy prodCap, testCap, and testFiles keys in a persisted state file
    are ignored when the state is loaded.

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

# Shared Codex PreToolUse transport: stdin payload parsing and tool_input-to-file
# mapping for every tool name the ^(apply_patch|Edit|Write)$ matcher admits.
try { . (Join-Path $PSScriptRoot 'codex-pretooluse-file-mapping.ps1') } catch { Add-HookDependencyFailure -Name 'codex-pretooluse-file-mapping.ps1' -ErrorRecord $_ }

# Shared route helpers; this file is byte-identical to the Claude runtime copy.
try { . (Join-Path $PSScriptRoot 'enforce-batch-budget-route.ps1') } catch { Add-HookDependencyFailure -Name 'enforce-batch-budget-route.ps1' -ErrorRecord $_ }

function Get-PythonBatchBudgetState {
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

function ConvertTo-PythonBatchBudgetState {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSReviewUnusedParameter', 'TestCap', Justification = 'Accepted and ignored for callers written against the removed test-file cap.')]
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        $InputObject,

        [Parameter(Mandatory)]
        [int] $ProdCap,

        [int] $TestCap = 0
    )

    # Only prodFiles is carried over. Persisted prodCap, testCap, and testFiles keys
    # come from the removed test-file cap and recorded overrides and are ignored.
    $state = Get-PythonBatchBudgetState -ProdCap $ProdCap
    if ($null -ne $InputObject.prodFiles) { $state.prodFiles = @($InputObject.prodFiles) }

    return $state
}

function Get-PythonBatchBudgetBlockDecision {
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

function Invoke-PythonBatchBudgetDecision {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSReviewUnusedParameter', 'TestCap', Justification = 'Accepted and ignored for callers written against the removed test-file cap.')]
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $FilePath,

        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $State,

        [AllowEmptyString()]
        [string] $StateFile = '',

        [int] $TestCap = 0,

        [switch] $LargePathRoute,

        [AllowEmptyString()]
        [string] $ObservedRoute = ''
    )
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'PYTHON_LARGE_PATH_REQUIRED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    $normalized = $FilePath -replace '\\', '/'
    if ($normalized -notmatch '\.py$') {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $false }
    }

    # The orchestrated large path has no production-file cap, so nothing is counted.
    if ($LargePathRoute) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $false }
    }

    # Test files never count toward the routing threshold.
    $isTestFile = ($normalized -match '(^|/)tests/.*\.py$') -or ($normalized -match '(^|/)test_[^/]+\.py$')
    if ($isTestFile) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $false }
    }

    $countedFiles = @($State.prodFiles)
    if ($countedFiles -contains $normalized) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $false }
    }

    $cap = [int]$State.prodCap
    if ($countedFiles.Count -ge $cap) {
        Write-Verbose "Denying production Python path '$normalized' in direct mode; counted paths are recorded in '$StateFile'."
        $counted = $countedFiles -join ', '
        $route = if ([string]::IsNullOrWhiteSpace($ObservedRoute)) { 'none' } else { $ObservedRoute }
        $reason = "PYTHON_LARGE_PATH_REQUIRED: this change touches more than $cap production Python files (already counted: $counted; requested: $normalized). A change of this size belongs on the orchestrated large path, which has no production-file cap. Route the change through .codex/prompts/orchestrate-work.md. Checkpoint route observed: $route."
        return Get-PythonBatchBudgetBlockDecision -Reason $reason -State $State
    }

    $State.prodFiles = $countedFiles + @($normalized)

    return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' }; state = $State; shouldWriteState = $true }
}

function Invoke-PythonBatchBudgetHook {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw,
        [string] $SessionId = 'default',
        [string] $Root = (Get-Location).Path,
        [int] $ProdCap = 3,
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

    if (-not $ToolInputRaw) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        return Get-PythonBatchBudgetBlockDecision -Reason 'Python batch-budget hook received malformed JSON in Codex tool_input.'
    }

    $filePath = $toolInput.file_path
    if (-not $filePath) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $normalized = $filePath -replace '\\', '/'
    if ($normalized -notmatch '\.py$') {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $stateDir = Join-Path -Path $Root -ChildPath '.codex/state'
    $stateFile = Join-Path -Path $stateDir -ChildPath ("python-batch-budget.$SessionId.json")

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
        return Invoke-PythonBatchBudgetDecision -FilePath $filePath -State (Get-PythonBatchBudgetState -ProdCap $ProdCap) -StateFile $stateFile -LargePathRoute
    }

    if (-not (& $TestPathExists $stateDir)) {
        & $EnsureDirectory $stateDir
    }

    $state = Get-PythonBatchBudgetState -ProdCap $ProdCap

    if (& $TestPathExists $stateFile) {
        try {
            $loaded = & $ReadState $stateFile | ConvertFrom-Json -ErrorAction Stop
            $state = ConvertTo-PythonBatchBudgetState -InputObject $loaded -ProdCap $ProdCap
        } catch {
            Write-Verbose "Ignoring unreadable Python batch-budget state file '$stateFile': $($_.Exception.Message)"
        }
    }

    $decision = Invoke-PythonBatchBudgetDecision -FilePath $filePath -State $state -StateFile $stateFile -ObservedRoute $observedRoute
    if ($decision.shouldWriteState) {
        try {
            & $WriteState $stateFile $decision.state
        } catch {
            Write-Verbose "Unable to write Python batch-budget state file '$stateFile': $($_.Exception.Message)"
        }
    }

    return $decision
}

function Invoke-PythonBatchBudgetCodexEntryPoint {
    <#
    .SYNOPSIS
        Runs the Codex Python batch-budget decision and returns the process exit code.
    .DESCRIPTION
        Parses the Codex PreToolUse payload through the shared transport, collects both
        sides of every mapped file edit, and evaluates each path with the Python hook.
        The first deny is written to the output stream as compact JSON without its state
        property, followed by exit code 0. When nothing is denied the function returns
        0 and writes nothing else. Any failure writes the error text to standard error
        and returns 2. The function does not call exit.
    .PARAMETER PayloadRaw
        Raw PreToolUse payload text.
    .PARAMETER RepositoryRoot
        Root used to locate the Codex state directory and the orchestrator checkpoint.
    .PARAMETER HookSeams
        Optional seam overrides splatted into Invoke-PythonBatchBudgetHook.
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $PayloadRaw,

        [Parameter(Mandatory)]
        [string] $RepositoryRoot,

        [hashtable] $HookSeams = @{}
    )

    try {
        # Transport and mapping come from the shared module. session_id is still
        # required because the direct-mode counter is keyed by it.
        $payload = ConvertFrom-CodexPreToolUsePayload -PayloadRaw $PayloadRaw -HookName 'enforce-python-batch-budget' -RequireSessionId
        $sessionId = ([string]$payload.session_id) -replace '[^A-Za-z0-9._-]', '_'

        # Both sides of a rename are evaluated, matching the path scan that collected
        # Add/Update/Delete targets and Move destinations alike. Ordering and
        # de-duplication are preserved. A well-formed payload that maps to no file
        # yields no paths, so no state is written and the hook allows silently.
        $budgetPaths = @(
            @(ConvertTo-CodexFileEditInput -Payload $payload) |
                ForEach-Object { $_.source_path; $_.file_path } |
                    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
                        Select-Object -Unique
        )

        foreach ($path in $budgetPaths) {
            $toolInputRaw = @{ file_path = $path } | ConvertTo-Json -Compress
            $decision = Invoke-PythonBatchBudgetHook -ToolInputRaw $toolInputRaw -SessionId $sessionId -Root $RepositoryRoot @HookSeams
            if ($decision.hookSpecificOutput.permissionDecision -eq 'deny') {
                $decision.Remove('state')
                $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
                return 0
            }
        }

        return 0
    } catch {
        [Console]::Error.WriteLine([string]$_)
        return 2
    }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('PYTHON_LARGE_PATH_REQUIRED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'PYTHON_LARGE_PATH_REQUIRED:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

# The entry point returns its [int] exit code as the last pipeline element and any
# deny JSON before it, so the JSON is written explicitly before the process exits.
$entryPointResult = @(Invoke-PythonBatchBudgetCodexEntryPoint -PayloadRaw ([Console]::In.ReadToEnd()) -RepositoryRoot (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent))
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])
