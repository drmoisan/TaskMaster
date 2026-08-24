<#
.SYNOPSIS
    Pre-tool-use hook for Claude Code that blocks forbidden patterns in PowerShell unit tests.

.DESCRIPTION
    This script is invoked by the Claude Code PreToolUse hook before any Write or Edit
    operation on a file path matching a Pester test file (*.Tests.ps1 or tests/**/*.ps1).
    It acquires the hook payload through the shared reader and reads the envelope's
    nested tool_input ('file_path' plus a content field: 'content' for Write,
    'new_string' for Edit), then rejects the operation when the proposed content
    introduces forbidden runtime dependencies or violates the mocking rules.

    Forbidden patterns in Pester unit tests include:
      - direct external-executable mocking (Mock git, Mock gh, Mock actionlint, etc.)
        instead of mocking the wrapper function (Invoke-GitExe, Invoke-GhExe, etc.)
      - temporary filesystem usage (New-TemporaryFile, [System.IO.Path]::GetTempFileName,
        [System.IO.Path]::GetTempPath, $env:TEMP usage, $env:TMP usage)
      - network access (Invoke-WebRequest, Invoke-RestMethod, System.Net.Http.*,
        System.Net.WebRequest, System.Net.Sockets)
      - subprocess execution (Start-Process with raw executables)
      - time-based flakiness (Start-Sleep)

    If the content contains any forbidden pattern, the script writes a PreToolUse JSON
    response to stdout with hookSpecificOutput.permissionDecision = 'deny' and exits with
    code 0 to let Claude Code surface the reason.

.NOTES
    Compatible with PowerShell 7+.
    This script must not modify any state; it is a read-only validation gate.
    It only inspects tool inputs targeting Pester test paths; all other paths pass through.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
function Get-PowerShellTestPurityBlockDecision {
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

function Test-PowerShellTestFilePath {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $FilePath
    )

    $normalized = $FilePath -replace '\\', '/'
    return (($normalized -match '(^|/)tests/.*\.ps1$') -or ($normalized -match '\.Tests\.ps1$'))
}

function Invoke-PowerShellTestPurityDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw
    )

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-PowerShellTestPurityBlockDecision -Reason (
            'PowerShell unit test purity hook received an unreadable PreToolUse envelope: ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $filePath = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'file_path'
    if (-not $filePath) {
        return $null
    }

    if (-not (Test-PowerShellTestFilePath -FilePath $filePath)) {
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
        @{ Pattern = '(?m)^\s*Mock\s+git\b'; Reason = 'direct Mock git forbidden in Pester tests; mock the Invoke-GitExe wrapper instead' },
        @{ Pattern = '(?m)^\s*Mock\s+gh\b'; Reason = 'direct Mock gh forbidden in Pester tests; mock the Invoke-GhExe wrapper instead' },
        @{ Pattern = '(?m)^\s*Mock\s+actionlint\b'; Reason = 'direct Mock actionlint forbidden in Pester tests; mock the Invoke-ActionlintExe wrapper instead' },
        @{ Pattern = "(?m)^\s*Mock\s+['""]git['""]"; Reason = 'direct Mock ''git'' forbidden in Pester tests; mock the Invoke-GitExe wrapper instead' },
        @{ Pattern = "(?m)^\s*Mock\s+['""]gh['""]"; Reason = 'direct Mock ''gh'' forbidden in Pester tests; mock the Invoke-GhExe wrapper instead' },
        @{ Pattern = '\bNew-TemporaryFile\b'; Reason = 'New-TemporaryFile forbidden in Pester unit tests' },
        @{ Pattern = '\[System\.IO\.Path\]::GetTempFileName'; Reason = 'temporary files forbidden in Pester unit tests' },
        @{ Pattern = '\[System\.IO\.Path\]::GetTempPath'; Reason = 'temp path usage forbidden in Pester unit tests' },
        @{ Pattern = '\$env:TEMP\b'; Reason = '$env:TEMP usage forbidden in Pester unit tests' },
        @{ Pattern = '\$env:TMP\b'; Reason = '$env:TMP usage forbidden in Pester unit tests' },
        @{ Pattern = '\bInvoke-WebRequest\b'; Reason = 'network access (Invoke-WebRequest) forbidden in Pester unit tests' },
        @{ Pattern = '\bInvoke-RestMethod\b'; Reason = 'network access (Invoke-RestMethod) forbidden in Pester unit tests' },
        @{ Pattern = '\[System\.Net\.Http\.'; Reason = 'System.Net.Http usage forbidden in Pester unit tests' },
        @{ Pattern = '\[System\.Net\.WebRequest\]'; Reason = 'System.Net.WebRequest usage forbidden in Pester unit tests' },
        @{ Pattern = '\[System\.Net\.Sockets\.'; Reason = 'raw socket access forbidden in Pester unit tests' },
        @{ Pattern = '\bStart-Process\b'; Reason = 'Start-Process forbidden in Pester unit tests; mock the wrapper seam instead' },
        @{ Pattern = '\bStart-Sleep\b'; Reason = 'Start-Sleep forbidden in Pester unit tests; avoid timing hacks' }
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
    $reason = "PowerShell unit test purity violations in '$filePath': " + ($uniqueViolations -join '; ') + ". Replace with wrapper-seam mocks, in-memory fakes, or pure code paths per .claude/rules/powershell.md."

    return Get-PowerShellTestPurityBlockDecision -Reason $reason
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

$decision = Invoke-PowerShellTestPurityDecision -ToolInputRaw (Read-ClaudeHookRawPayload)
if ($null -ne $decision -and $decision.hookSpecificOutput.permissionDecision -eq 'deny') {
    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
}

exit 0
