<#
.SYNOPSIS
    SubagentStop hook for the atomic-executor subagent.

.DESCRIPTION
    Blocks termination of the atomic-executor subagent unless the output and
    plan state satisfy the executor's mechanical contract.

    Enforced checks:
      - the SubagentStop envelope read from stdin carries a non-empty last_assistant_message,
      - output advertises `plan-path: <path>` (or supported equivalent forms),
      - the advertised plan exists on disk,
      - a run is classified as preflight from the delegation prompt in the subagent
        transcript (Test-IsPreflightRun), falling back to the plan file or one output signal,
      - preflight runs return exactly one preflight signal consistent with their
        PREFLIGHT-DEFECT lines, name an existing fix record for any PREFLIGHT-FIX line,
        and may not claim non-canonical blocked text,
      - non-preflight runs may not stop in a blocked state, except a canonical preflight
        block with a plan delta and no checked task (combined proportionate run); only the
        uppercase BLOCKED token counts as a blocked claim, and lowercase prose does not,
      - non-preflight plans contain checkboxes and all are checked,
      - non-preflight output includes `AC Status Summary`,
      - non-preflight output includes command/status reporting,
      - for each language referenced by explicit file paths in the plan, the
        output includes a PASS or FAIL status line for that language.

.NOTES
    Reads the SubagentStop envelope from stdin through Read-SubagentStopEnvelope
    (subagentstop-envelope.ps1); the assistant text comes from the helper's
    AssistantText. A non-repairable condition blocks once: when StopHookActive is
    set, the re-entered stop is allowed. Exits 0 to allow termination; a block is
    written to stderr with exit 2 through Exit-SubagentStopBlock. The dependency
    guard and the E1 helper are guarded loads: a failure blocks with exit 2,
    naming the file. Plan file contents are read through Get-PlanFileContent so
    tests can mock the filesystem boundary without writing temporary files.
    An unhandled error during validation blocks with exit 2 and a reason (issue #928).
#>

[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }

Set-StrictMode -Version Latest
try { . (Join-Path $PSScriptRoot 'subagentstop-envelope.ps1') } catch { Add-HookDependencyFailure -Name 'subagentstop-envelope.ps1' -ErrorRecord $_ }
$ErrorActionPreference = 'Stop'
$script:SubagentStopReportAndHaltLine = 'This condition is not repairable by the subagent. Report it to the operator and halt.'

function Get-PlanFileContent {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([Parameter(Mandatory = $true)] [string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return @{ Exists = $false; Lines = @() }
    }

    # @() yields an array for any line count; the filter drops a $null emitted at the read boundary.
    $lines = @(Get-Content -LiteralPath $Path -ErrorAction Stop | Where-Object { $null -ne $_ })
    return @{ Exists = $true; Lines = $lines }
}

function Get-PlanPathFromOutput {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory = $true)] [string] $AgentOutput)

    $pattern = 'plan-path\s*[:=]\s*["'']?([^\s"''\)`]+)|\[plan-path\]\(([^)]+)\)'
    $match = [regex]::Match($AgentOutput, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if (-not $match.Success) {
        return $null
    }

    $value = if ($match.Groups[1].Success) { $match.Groups[1].Value } else { $match.Groups[2].Value }
    $value = $value.Trim()
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $null
    }

    return $value
}

function Test-IsPreflightPlan {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory = $true)] [AllowEmptyCollection()] [AllowEmptyString()] [string[]] $Lines)

    return @($Lines | Where-Object { $_ -match '(?i)^\s*DIRECTIVE:\s*PREFLIGHT VALIDATION ONLY\s*$' }).Count -gt 0
}

function Get-JsonPropertyValue {
    # Returns a named property of a parsed JSON object, or $null; safe under StrictMode.
    [CmdletBinding()]
    [OutputType([object])]
    param([AllowNull()] [object] $Object, [Parameter(Mandatory = $true)] [string] $Name)

    if ($null -eq $Object -or -not ($Object -is [System.Management.Automation.PSCustomObject])) {
        return $null
    }
    $property = @($Object.PSObject.Properties | Where-Object { $_.Name -ceq $Name })
    if ($property.Count -eq 0) {
        return $null
    }
    return $property[0].Value
}

function Get-TranscriptPathFromEnvelope {
    # Returns agent_transcript_path from the SubagentStop envelope with a leading ~ expanded, or $null.
    [CmdletBinding()]
    [OutputType([string])]
    param([AllowNull()] [object] $Envelope)

    $path = Get-JsonPropertyValue -Object $Envelope -Name 'agent_transcript_path'
    if (-not ($path -is [string]) -or [string]::IsNullOrWhiteSpace($path)) {
        return $null
    }
    if ($path.StartsWith('~')) {
        $path = $HOME + $path.Substring(1)
    }
    return $path
}

function Get-TranscriptFirstUserText {
    # Returns the text of the first `user` entry of transcript JSONL lines, or $null when none parses.
    [CmdletBinding()]
    [OutputType([string])]
    param([AllowEmptyString()] [string[]] $Lines = @())

    foreach ($line in $Lines) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }
        try {
            $entry = ConvertFrom-Json -InputObject $line -ErrorAction Stop
        }
        catch {
            continue
        }
        if ((Get-JsonPropertyValue -Object $entry -Name 'type') -ne 'user') {
            continue
        }
        $container = Get-JsonPropertyValue -Object $entry -Name 'message'
        if ($null -eq $container) {
            $container = $entry
        }
        $content = Get-JsonPropertyValue -Object $container -Name 'content'
        if ($content -is [string]) {
            return $content
        }
        $textBlocks = @(@($content) | Where-Object { (Get-JsonPropertyValue -Object $_ -Name 'type') -eq 'text' })
        $texts = @($textBlocks | ForEach-Object { [string](Get-JsonPropertyValue -Object $_ -Name 'text') })
        if ($texts.Count -gt 0) {
            return ($texts -join "`n")
        }
    }
    return $null
}

function Read-TranscriptHead {
    # File seam: returns the first 50 lines of the transcript at Path, or $null when no file exists.
    [CmdletBinding()]
    [OutputType([string[]])]
    param([Parameter(Mandatory = $true)] [string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    return [string[]]@(Get-Content -LiteralPath $Path -TotalCount 50 -ErrorAction Stop)
}

function Test-IsPreflightRun {
    <#
    .SYNOPSIS
        Classifies an executor run as preflight or execution (issue #964).
    .DESCRIPTION
        A readable transcript is authoritative: its first `user` entry (the delegation prompt) decides, even
        against a stale plan-file directive. Otherwise the plan-file directive, then exactly one output
        `PREFLIGHT:` signal, decide (self-declared). ReadTranscriptHead returns JSONL lines or $null.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $AgentOutput,
        [Parameter(Mandatory = $true)] [AllowEmptyCollection()] [AllowEmptyString()] [string[]] $PlanLines,
        [AllowNull()] [AllowEmptyString()] [string] $TranscriptPath,
        [scriptblock] $ReadTranscriptHead = ${function:Read-TranscriptHead}
    )

    if (-not [string]::IsNullOrWhiteSpace($TranscriptPath)) {
        $head = $null
        try {
            $head = & $ReadTranscriptHead $TranscriptPath
        }
        catch {
            $head = $null
        }
        if ($null -ne $head) {
            $prompt = Get-TranscriptFirstUserText -Lines ([string[]]@($head))
            if ($null -ne $prompt) {
                return [regex]::IsMatch($prompt, '(?im)^\s*DIRECTIVE:\s*PREFLIGHT VALIDATION ONLY\s*$')
            }
        }
    }
    if (Test-IsPreflightPlan -Lines $PlanLines) {
        return $true
    }
    return [regex]::Matches($AgentOutput, '(?m)^\s*PREFLIGHT:\s*(ALL CLEAR|REVISIONS REQUIRED)\s*$').Count -eq 1
}

function Test-PreflightFixRecord {
    # File seam for the preflight-fix-record path.
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory = $true)] [string] $Path)
    return [bool](Test-Path -LiteralPath $Path -PathType Leaf)
}

function Get-PreflightRunBlockDecision {
    <#
    .SYNOPSIS
        Applies the single-pass preflight output rules (issue #964); every block is repairable.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([Parameter(Mandatory = $true)] [string] $AgentOutput)

    if (-not (Test-HasCanonicalBlockedText -AgentOutput $AgentOutput)) {
        return @{ Ok = $false; Repairable = $true; Message = 'atomic-executor hook: blocked preflight output must include the exact text `BLOCKED at preflight (before [P0-T1])` plus a concrete plan delta.' }
    }
    if ($AgentOutput -cmatch 'BLOCKED') {
        return @{ Ok = $true; Message = $null }
    }

    $signals = @([regex]::Matches($AgentOutput, '(?m)^\s*PREFLIGHT:\s*(?<Signal>ALL CLEAR|REVISIONS REQUIRED)\s*$'))
    if ($signals.Count -ne 1) {
        return @{ Ok = $false; Repairable = $true; Message = "atomic-executor hook: preflight output must carry exactly one ``PREFLIGHT: ALL CLEAR`` or ``PREFLIGHT: REVISIONS REQUIRED`` signal line; found $($signals.Count)." }
    }
    $defectCount = [regex]::Matches($AgentOutput, '(?m)^\s*PREFLIGHT-DEFECT:\s*\S').Count
    if ($signals[0].Groups['Signal'].Value -eq 'ALL CLEAR' -and $defectCount -gt 0) {
        return @{ Ok = $false; Repairable = $true; Message = 'atomic-executor hook: `PREFLIGHT: ALL CLEAR` must not carry a `PREFLIGHT-DEFECT:` line; fix trivial defects in place or return `PREFLIGHT: REVISIONS REQUIRED`.' }
    }
    if ($signals[0].Groups['Signal'].Value -eq 'REVISIONS REQUIRED' -and $defectCount -eq 0) {
        return @{ Ok = $false; Repairable = $true; Message = 'atomic-executor hook: `PREFLIGHT: REVISIONS REQUIRED` must list every substantive defect as a `PREFLIGHT-DEFECT:` line.' }
    }
    if ([regex]::IsMatch($AgentOutput, '(?m)^\s*PREFLIGHT-FIX:\s*\S')) {
        $record = [regex]::Match($AgentOutput, '(?im)^\s*preflight-fix-record:\s*`?(?<Path>[^\s`]+)`?\s*$')
        $recordPath = if ($record.Success) { ($record.Groups['Path'].Value -replace '\\', '/') -replace '^\./', '' } else { '' }
        if (-not $recordPath.StartsWith('docs/features/', [System.StringComparison]::OrdinalIgnoreCase) -or -not (Test-PreflightFixRecord -Path $recordPath)) {
            return @{ Ok = $false; Repairable = $true; Message = 'atomic-executor hook: a `PREFLIGHT-FIX:` line requires a `preflight-fix-record: <path>` line naming an existing record under docs/features/.' }
        }
    }
    return @{ Ok = $true; Message = $null }
}

function Get-TouchedLanguagesFromPlan {
    [CmdletBinding()]
    [OutputType([string[]])]
    param([Parameter(Mandatory = $true)] [AllowEmptyString()] [string[]] $Lines)

    $languages = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $pathPattern = '(?i)(?:[.\w*?-]+[\\/])+[.\w*?-]+'

    foreach ($line in $Lines) {
        foreach ($match in [regex]::Matches($line, $pathPattern)) {
            $extension = [System.IO.Path]::GetExtension($match.Value).ToLowerInvariant()
            switch -Regex ($extension) {
                '^\.(ts|tsx)$' { $null = $languages.Add('TypeScript') }
                '^\.py$' { $null = $languages.Add('Python') }
                '^\.(ps1|psm1|psd1)$' { $null = $languages.Add('PowerShell') }
                '^\.cs$' { $null = $languages.Add('CSharp') }
            }
        }
    }

    return [string[]]@($languages)
}

function Test-OutputHasLanguageStatus {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory = $true)] [string] $AgentOutput, [Parameter(Mandatory = $true)] [string] $Language)

    $labelMap = @{
        'TypeScript' = @('TypeScript', 'typescript')
        'Python'     = @('Python', 'python')
        'PowerShell' = @('PowerShell', 'powershell')
        'CSharp'     = @('C#', 'CSharp', 'csharp', '\.NET', 'dotnet')
    }

    $labels = $labelMap[$Language]
    $labelPattern = '(?i)(' + (($labels | ForEach-Object { $_ }) -join '|') + ')'
    return [regex]::IsMatch($AgentOutput, "(?im)^.*$labelPattern.*\b(PASS|FAIL)\b.*$")
}

function Test-HasPreflightSignal {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory = $true)] [string] $AgentOutput)

    return [regex]::IsMatch($AgentOutput, '(?m)^\s*PREFLIGHT:\s*(ALL CLEAR|REVISIONS REQUIRED)\s*$')
}

function Test-HasCanonicalBlockedText {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory = $true)] [string] $AgentOutput)

    # Case-sensitive: lowercase prose such as `blocked_reason` is not a BLOCKED claim (issue #964).
    if ($AgentOutput -cnotmatch 'BLOCKED') {
        return $true
    }

    if ($AgentOutput -cnotmatch 'BLOCKED at preflight \(before \[P0-T1\]\)') {
        return $false
    }

    # The canonical phrase itself carries [P0-T1], so it is removed before looking for a plan delta.
    $remainder = $AgentOutput -creplace 'BLOCKED at preflight \(before \[P0-T1\]\)', ''
    if ($remainder -notmatch '(?i)plan delta' -and $remainder -notmatch '\[P\d+-T\d+\]') {
        return $false
    }

    return $true
}

function Invoke-ExecutorOutputValidation {
    <#
    .SYNOPSIS
        Reads the SubagentStop envelope, validates the assistant text, and applies the re-entry policy.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([AllowNull()] [AllowEmptyString()] [string] $InputText)

    $envelope = if ($PSBoundParameters.ContainsKey('InputText')) { Read-SubagentStopEnvelope -InputText $InputText } else { Read-SubagentStopEnvelope }
    if (-not $envelope.IsValid) { $decision = @{ Ok = $false; Repairable = $false; Message = ('{0} payload anomaly - {1}' -f 'atomic-executor hook:', (Get-SubagentStopEnvelopeAnomalyReason -Anomaly $envelope.Anomaly)) } }
    else {
        $agentOutput = $envelope.AssistantText
        $decision = Get-ExecutorOutputBlockDecision -AgentOutput ([string]$agentOutput) -TranscriptPath (Get-TranscriptPathFromEnvelope -Envelope $envelope.Envelope)
    }
    if ($decision.Ok) { return @{ Ok = $true; Message = $null } }
    if (-not $decision.Repairable) {
        if ($envelope.StopHookActive) { return @{ Ok = $true; Message = $null } }
        if (-not $decision.Message.Contains('to the operator and halt.')) { $decision.Message = $decision.Message + "`n" + $script:SubagentStopReportAndHaltLine }
    }
    return @{ Ok = $false; Message = $decision.Message }
}

function Get-ExecutorOutputBlockDecision {
    <#
    .SYNOPSIS
        Applies the executor completion checks to the assistant text and returns the block decision.
    .DESCRIPTION
        Returns @{ Ok = $true; Message = $null } to allow, or a hashtable with Ok = $false, a
        Repairable boolean, and Message. Repairable is $false for a non-preflight BLOCKED claim
        and for a plan with no task checkboxes; every other block is repairable by the subagent.
        TranscriptPath and ReadTranscriptHead feed Test-IsPreflightRun. An execution run that
        stops with the canonical preflight BLOCKED text, a plan delta, and no checked task is a
        preflight block inside a combined proportionate run and is allowed.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $AgentOutput,
        [AllowNull()] [AllowEmptyString()] [string] $TranscriptPath,
        [scriptblock] $ReadTranscriptHead
    )

    if ([string]::IsNullOrWhiteSpace($agentOutput)) {
        return @{ Ok = $false; Repairable = $true; Message = 'atomic-executor hook: agent output is empty (last_assistant_message carries no text); executor must return plan-path and final completion summary.' }
    }

    $planPath = Get-PlanPathFromOutput -AgentOutput $agentOutput
    if ($null -eq $planPath) {
        return @{ Ok = $false; Repairable = $true; Message = 'atomic-executor hook: agent output does not advertise a plan-path. Executor must report `plan-path: <path>` per atomic-plan-contract.' }
    }

    $file = Get-PlanFileContent -Path $planPath
    if (-not $file.Exists) {
        return @{ Ok = $false; Repairable = $true; Message = "atomic-executor hook: executor advertised plan-path '$planPath' but no file exists at that location." }
    }

    $runArguments = @{ AgentOutput = $agentOutput; PlanLines = [string[]]@($file.Lines); TranscriptPath = $TranscriptPath }
    if ($null -ne $ReadTranscriptHead) { $runArguments['ReadTranscriptHead'] = $ReadTranscriptHead }
    if (Test-IsPreflightRun @runArguments) {
        if ($agentOutput -cnotmatch 'BLOCKED' -and -not (Test-HasPreflightSignal -AgentOutput $agentOutput)) {
            return @{ Ok = $false; Repairable = $true; Message = 'atomic-executor hook: preflight output must include `PREFLIGHT: ALL CLEAR` or `PREFLIGHT: REVISIONS REQUIRED`.' }
        }
        return Get-PreflightRunBlockDecision -AgentOutput $agentOutput
    }

    if ($agentOutput -cmatch 'BLOCKED') {
        $hasCheckedTask = @($file.Lines | Where-Object { $_ -match '^\s*-\s\[[xX]\]\s\[P\d+-T\d+\]' }).Count -gt 0
        if ($agentOutput -cmatch 'BLOCKED at preflight \(before \[P0-T1\]\)' -and (Test-HasCanonicalBlockedText -AgentOutput $agentOutput) -and -not $hasCheckedTask) {
            return @{ Ok = $true; Message = $null }
        }
        return @{ Ok = $false; Repairable = $false; Message = 'atomic-executor hook: blocking is permitted only during preflight validation (before [P0-T1]).' }
    }

    $checkboxLineCount = @($file.Lines | Where-Object { $_ -match '^\s*-\s\[(?:\s|[xX])\]\s' }).Count
    $uncheckedLineNumbers = [System.Collections.Generic.List[int]]::new()
    for ($i = 0; $i -lt $file.Lines.Count; $i++) {
        if ($file.Lines[$i] -match '^\s*-\s\[\s\]\s') {
            $uncheckedLineNumbers.Add($i + 1)
        }
    }

    if ($checkboxLineCount -eq 0) {
        return @{ Ok = $false; Repairable = $false; Message = "atomic-executor hook: plan file '$planPath' contains no task checkboxes; executor cannot complete against an empty plan." }
    }

    if ($uncheckedLineNumbers.Count -gt 0) {
        $reported = $uncheckedLineNumbers
        $suffix = ''
        if ($uncheckedLineNumbers.Count -gt 5) {
            $reported = $uncheckedLineNumbers.GetRange(0, 5)
            $suffix = ' (showing first 5)'
        }
        $message = "atomic-executor hook: plan file '$planPath' has $($uncheckedLineNumbers.Count) unchecked task(s) at line(s) $($reported -join ', ')${suffix}. All `- [ ]` items must be ticked to `- [x]` before termination."
        return @{ Ok = $false; Repairable = $true; Message = $message }
    }

    if ($agentOutput -notmatch '(?i)\bAC Status Summary\b') {
        return @{ Ok = $false; Repairable = $true; Message = 'atomic-executor hook: completion output must include an `AC Status Summary` section.' }
    }

    $hasCommandEvidence = $agentOutput -match '(?i)(Commands Run|Command[s]?:|poetry[ ]run |npx |pwsh |git |mcp__drm-copilot__)'
    $hasStatusEvidence = $agentOutput -match '(?i)\b(PASS|FAIL)\b'
    if (-not $hasCommandEvidence -or -not $hasStatusEvidence) {
        return @{ Ok = $false; Repairable = $true; Message = 'atomic-executor hook: completion output must report commands run and pass/fail status results.' }
    }

    $missingLanguageStatuses = @(Get-TouchedLanguagesFromPlan -Lines $file.Lines | Where-Object { -not (Test-OutputHasLanguageStatus -AgentOutput $agentOutput -Language $_) })
    if ($missingLanguageStatuses.Count -gt 0) {
        return @{ Ok = $false; Repairable = $true; Message = "atomic-executor hook: completion output is missing explicit PASS/FAIL toolchain status lines for: $($missingLanguageStatuses -join ', ')." }
    }

    return @{ Ok = $true; Message = $null }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('validate-executor-output: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'validate-executor-output:'
if ($null -ne $dependencyDecision) { [Console]::Error.WriteLine($dependencyDecision.Reason); exit $dependencyDecision.ExitCode }
try { $result = Invoke-ExecutorOutputValidation }
catch { Exit-SubagentStopBlock -Message ('validate-executor-output: validator error: ' + ([string]$_.Exception.Message -split "`r?`n")[0] + '; the gate fails closed.' + "`n" + $script:SubagentStopReportAndHaltLine) }
if (-not $result.Ok) { Exit-SubagentStopBlock -Message $result.Message }
exit 0
