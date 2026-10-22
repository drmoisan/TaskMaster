<#
.SYNOPSIS
    Pre-tool-use hook for Claude Code that blocks forbidden patterns in Python unit tests.

.DESCRIPTION
    This script is invoked by the Claude Code PreToolUse hook before any Write or Edit
    operation on a file path matching tests/**/*.py. It acquires the hook payload
    through the shared reader and reads the envelope's nested tool_input ('file_path'
    plus a content field: 'content' for Write, 'new_string' for Edit), then rejects the
    operation when the proposed content introduces forbidden runtime dependencies.

    Forbidden patterns in unit tests include:
      - temporary filesystem usage (tempfile, NamedTemporaryFile, TemporaryDirectory,
        mkstemp, mkdtemp, Path.touch)
      - network access (requests, httpx, urllib.request, socket, http.client)
      - subprocess execution (subprocess, os.system, os.popen)
      - time-based flakiness (time.sleep)
      - real database drivers (psycopg2, pymysql, sqlite3.connect on real files)

    If the content contains any forbidden pattern, the script writes a JSON response
    to stdout with 'decision': 'block' and exits with code 0 to let Claude Code surface
    the reason.

.NOTES
    Compatible with PowerShell 7+.
    This script must not modify any state; it is a read-only validation gate.
    It only inspects tool inputs targeting tests/ paths; all other paths pass through.
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }


try { Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'HookPayload.psm1' -ErrorRecord $_ }
function Get-PythonTestPurityBlockDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Reason
    )

    [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

function Test-PythonTestFilePath {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $FilePath
    )

    $normalized = $FilePath -replace '\\', '/'
    return (($normalized -match '(^|/)tests/.*\.py$') -or ($normalized -match '(^|/)test_[^/]+\.py$'))
}

function Invoke-PythonTestPurityDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw
    )
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'Python unit test purity hook:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-PythonTestPurityBlockDecision -Reason (
            'Python unit test purity hook received an unreadable PreToolUse envelope: ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $filePath = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'file_path'
    if (-not $filePath) {
        return $null
    }

    if (-not (Test-PythonTestFilePath -FilePath $filePath)) {
        return $null
    }

    $content = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'content'
    if (-not $content) {
        $content = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'new_string'
    }

    if (-not $content) {
        return $null
    }

    $forbiddenPatterns = @(
        @{ Pattern = 'import\s+tempfile'; Reason = 'tempfile usage forbidden in unit tests' },
        @{ Pattern = 'from\s+tempfile\s+import'; Reason = 'tempfile usage forbidden in unit tests' },
        @{ Pattern = 'NamedTemporaryFile'; Reason = 'temporary files forbidden in unit tests' },
        @{ Pattern = 'TemporaryDirectory'; Reason = 'temporary directories forbidden in unit tests' },
        @{ Pattern = '\bmkstemp\s*\('; Reason = 'temporary files forbidden in unit tests' },
        @{ Pattern = '\bmkdtemp\s*\('; Reason = 'temporary directories forbidden in unit tests' },
        @{ Pattern = '\.touch\s*\('; Reason = 'Path.touch forbidden in unit tests' },
        @{ Pattern = 'import\s+requests\b'; Reason = 'network access forbidden in unit tests' },
        @{ Pattern = 'from\s+requests\b'; Reason = 'network access forbidden in unit tests' },
        @{ Pattern = 'import\s+httpx\b'; Reason = 'network access forbidden in unit tests' },
        @{ Pattern = 'from\s+httpx\b'; Reason = 'network access forbidden in unit tests' },
        @{ Pattern = 'urllib\.request'; Reason = 'network access forbidden in unit tests' },
        @{ Pattern = 'import\s+socket\b'; Reason = 'raw socket access forbidden in unit tests' },
        @{ Pattern = 'from\s+http\.client'; Reason = 'network access forbidden in unit tests' },
        @{ Pattern = 'import\s+subprocess\b'; Reason = 'subprocess execution forbidden in unit tests' },
        @{ Pattern = 'from\s+subprocess\s+import'; Reason = 'subprocess execution forbidden in unit tests' },
        @{ Pattern = 'os\.system\s*\('; Reason = 'os.system forbidden in unit tests' },
        @{ Pattern = 'os\.popen\s*\('; Reason = 'os.popen forbidden in unit tests' },
        @{ Pattern = 'time\.sleep\s*\('; Reason = 'time.sleep forbidden in unit tests; avoid timing hacks' },
        @{ Pattern = 'import\s+psycopg2\b'; Reason = 'real database drivers forbidden in unit tests' },
        @{ Pattern = 'import\s+pymysql\b'; Reason = 'real database drivers forbidden in unit tests' },
        @{ Pattern = 'sqlite3\.connect\s*\('; Reason = 'sqlite3.connect on real files forbidden in unit tests' }
    )

    $violations = @()
    foreach ($entry in $forbiddenPatterns) {
        if ($content -match $entry.Pattern) {
            $violations += $entry.Reason
        }
    }

    if ($violations.Count -eq 0) {
        return $null
    }

    $uniqueViolations = $violations | Select-Object -Unique
    $reason = "Python unit test purity violations in '$filePath': " + ($uniqueViolations -join '; ') + ". Replace with pure code paths, dependency-injection seams, or in-memory fakes per .claude/rules/python.md."

    return Get-PythonTestPurityBlockDecision -Reason $reason
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('Python unit test purity hook: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'Python unit test purity hook:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

$decision = Invoke-PythonTestPurityDecision -ToolInputRaw (Read-ClaudeHookRawPayload)
if ($null -ne $decision -and $decision.hookSpecificOutput.permissionDecision -eq 'deny') {
    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
}

exit 0
