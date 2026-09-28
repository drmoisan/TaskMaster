
<#
.SYNOPSIS
    Pre-tool-use hook that enforces the PowerShell per-batch change budget.

.DESCRIPTION
    This script is invoked by the Codex PreToolUse hook before any Write or Edit
    operation. When the target file is a PowerShell source file (.ps1, .psm1, .psd1),
    it classifies the file as either production or test and checks the running count
    against the per-batch cap:
      - 3 production PowerShell files per batch
      - 3 test PowerShell files per batch

    A "batch" is scoped to the current Codex session. The running count is
    persisted under .codex/state/powershell-batch-budget.<session_id>.json. Only
    distinct file paths are counted; repeated edits to the same file consume one slot.

    Test files are those matching:
      - tests/**/*.ps1
      - *.Tests.ps1

    All other .ps1/.psm1/.psd1 files are treated as production files. Non-PowerShell
    paths pass through.

    An approved per-session override is recorded by writing
    {"prodCap": N, "testCap": M} into the Codex state file.

    When the cap would be exceeded by a new file, the script emits a PreToolUse JSON
    response with hookSpecificOutput.permissionDecision = 'deny' and exits 0. The session
    must explicitly reset the counter by deleting the state file before starting a new
    batch. Files already counted are always allowed through.

.NOTES
    Compatible with PowerShell 7+.
#>
[CmdletBinding()]
param()

# Shared Codex PreToolUse transport: stdin payload parsing and tool_input-to-file
# mapping for every tool name the ^(apply_patch|Edit|Write)$ matcher admits.
. (Join-Path $PSScriptRoot 'codex-pretooluse-file-mapping.ps1')

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
        [int] $TestCap
    )

    $state = Get-PowerShellBatchBudgetState -ProdCap $ProdCap -TestCap $TestCap
    if ($null -ne $InputObject.prodCap) { $state.prodCap = [int]$InputObject.prodCap }
    if ($null -ne $InputObject.testCap) { $state.testCap = [int]$InputObject.testCap }
    if ($null -ne $InputObject.prodFiles) { $state.prodFiles = @($InputObject.prodFiles) }
    if ($null -ne $InputObject.testFiles) { $state.testFiles = @($InputObject.testFiles) }

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
        [string] $StateFile
    )

    $normalized = $FilePath -replace '\\', '/'
    if ($normalized -notmatch '\.(ps1|psm1|psd1)$') {
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
        $reason = "PowerShell per-batch budget exceeded: $kind file cap is $cap and is already full ($currentFiles). Requested new file: $normalized. Split the work into a new batch, record an approved cap in $StateFile, or reset the batch by deleting that state file."
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
        [string] $SessionId = 'default',
        [string] $Root = (Get-Location).Path,
        [int] $ProdCap = 3,
        [int] $TestCap = 3,
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
        return Get-PowerShellBatchBudgetBlockDecision -Reason 'PowerShell batch-budget hook received malformed JSON in Codex tool_input.'
    }

    $filePath = $toolInput.file_path
    if (-not $filePath) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $normalized = $filePath -replace '\\', '/'
    if ($normalized -notmatch '\.(ps1|psm1|psd1)$') {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $stateDir = Join-Path -Path $Root -ChildPath '.codex/state'
    if (-not (& $TestPathExists $stateDir)) {
        & $EnsureDirectory $stateDir
    }

    $stateFile = Join-Path -Path $stateDir -ChildPath ("powershell-batch-budget.$SessionId.json")
    $state = Get-PowerShellBatchBudgetState -ProdCap $ProdCap -TestCap $TestCap

    if (& $TestPathExists $stateFile) {
        try {
            $loaded = & $ReadState $stateFile | ConvertFrom-Json -ErrorAction Stop
            $state = ConvertTo-PowerShellBatchBudgetState -InputObject $loaded -ProdCap $ProdCap -TestCap $TestCap
        } catch {
            Write-Verbose "Ignoring unreadable PowerShell batch-budget state file '$stateFile': $($_.Exception.Message)"
        }
    }

    $decision = Invoke-PowerShellBatchBudgetDecision -FilePath $filePath -State $state -StateFile $stateFile
    if ($decision.shouldWriteState) {
        try {
            & $WriteState $stateFile $decision.state
        } catch {
            Write-Verbose "Unable to write PowerShell batch-budget state file '$stateFile': $($_.Exception.Message)"
        }
    }

    return $decision
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    # Transport and mapping come from the shared module. session_id is still
    # required because the batch counter is keyed by it.
    $payload = ConvertFrom-CodexPreToolUsePayload -PayloadRaw ([Console]::In.ReadToEnd()) -HookName 'enforce-powershell-batch-budget' -RequireSessionId
    $sessionId = ([string]$payload.session_id) -replace '[^A-Za-z0-9._-]', '_'
    $repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
    $prodCap = 3
    $testCap = 3

    # Both sides of a rename consume budget, matching the pre-fix path scan that
    # collected Add/Update/Delete targets and Move destinations alike. Ordering
    # and de-duplication are preserved. A well-formed payload that maps to no
    # file yields no paths, so no state is written and the hook allows silently.
    $budgetPaths = @(
        @(ConvertTo-CodexFileEditInput -Payload $payload) |
            ForEach-Object { $_.source_path; $_.file_path } |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
                    Select-Object -Unique
    )

    foreach ($path in $budgetPaths) {
        $toolInputRaw = @{ file_path = $path } | ConvertTo-Json -Compress
        $decision = Invoke-PowerShellBatchBudgetHook -ToolInputRaw $toolInputRaw -SessionId $sessionId -Root $repositoryRoot -ProdCap $prodCap -TestCap $testCap
        if ($decision.hookSpecificOutput.permissionDecision -eq 'deny') {
            $decision.Remove('state')
            $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
            exit 0
        }
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
