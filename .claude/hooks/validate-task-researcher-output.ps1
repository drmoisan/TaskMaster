<#
.SYNOPSIS
    SubagentStop hook for the task-researcher subagent.

.DESCRIPTION
    Blocks termination of the task-researcher subagent unless the agent's
    final output advertises a `research-path` token, the path is rooted
    under one of the two tracked research roots
    (docs/features/<feature>/research/ for feature-associated research, or
    docs/research/ for one-off research), matches the documented filename
    convention, and the file exists on disk.

.NOTES
    Reads the hook payload from CLAUDE_HOOK_INPUT as JSON. Exits 0 to allow
    termination; exits 1 with an error message to block. Filesystem reads go
    through Test-ResearchFile so tests can mock the boundary without writing
    temporary files.
#>

[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Test-ResearchFile {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    return [bool](Test-Path -LiteralPath $Path -PathType Leaf)
}

function Get-ResearchPathFromOutput {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $AgentOutput
    )

    $pattern = 'research-path\s*[:=]\s*["'']?([^\s"''\)`]+)|\[research-path\]\(([^)]+)\)'
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

function Test-IsUnderResearchRoot {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $normalized = $Path -replace '\\', '/'

    # Accept either of the two tracked research roots:
    #   - feature-associated research: docs/features/<...>/research/<file>
    #     (a feature-folder path that contains a /research/ segment), or
    #   - one-off research: docs/research/<file>.
    # The /research/ segment requirement distinguishes feature research from
    # other files under docs/features/ (for example spec.md or plan files).
    $isFeatureResearch =
    $normalized.StartsWith('docs/features/', [System.StringComparison]::OrdinalIgnoreCase) -and
    $normalized.IndexOf('/research/', [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    $isOneOffResearch =
    $normalized.StartsWith('docs/research/', [System.StringComparison]::OrdinalIgnoreCase)

    return ($isFeatureResearch -or $isOneOffResearch)
}

function Test-IsValidResearchFileName {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $normalized = $Path -replace '\\', '/'
    $fileName = [System.IO.Path]::GetFileName($normalized)
    return [regex]::IsMatch(
        $fileName,
        '^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-[A-Za-z0-9][A-Za-z0-9-]*-research\.md$'
    )
}

function Test-AutomationFeasibilitySection {
    <#
    .SYNOPSIS
        Enforces the '## Automation Feasibility' section for applicable
        autonomous-execution research artifacts.
    .DESCRIPTION
        Returns a hashtable with keys:
          - Ok:      $true when the artifact is not applicable, or it is
                     applicable and contains the '## Automation Feasibility'
                     section.
          - Message: rejection message; $null on success.

        Detection is narrow (OD-45-7): the section is required only when the
        research filename or the agent output contains an autonomous-execution
        token (for example 'autonomous-execution' or 'human-interaction').
        Non-matching research artifacts pass unaffected.

        ReadFileContent is an injectable scriptblock so tests can supply the
        research file body without writing temporary files. It defaults to
        Get-Content -Raw.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $ResearchFilePath,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $AgentOutput,

        [Parameter(Mandatory = $false)]
        [scriptblock] $ReadFileContent = { param($Path) Get-Content -LiteralPath $Path -Raw -ErrorAction Stop }
    )

    $detectionPattern = 'autonomous-execution|human-interaction'
    $fileName = [System.IO.Path]::GetFileName(($ResearchFilePath -replace '\\', '/'))

    $isApplicable = ([regex]::IsMatch($fileName, $detectionPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) -or
    ([regex]::IsMatch($AgentOutput, $detectionPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase))

    if (-not $isApplicable) {
        return @{ Ok = $true; Message = $null }
    }

    $content = & $ReadFileContent $ResearchFilePath
    if ([string]::IsNullOrWhiteSpace($content)) {
        return @{ Ok = $false; Message = "task-researcher hook: autonomous-execution research artifact '$ResearchFilePath' is empty; it must include an '## Automation Feasibility' section." }
    }

    $hasSection = [regex]::IsMatch(
        $content,
        '(?m)^\s{0,3}#{2,}\s+Automation\s+Feasibility\s*$',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    if (-not $hasSection) {
        return @{ Ok = $false; Message = "task-researcher hook: autonomous-execution research artifact '$ResearchFilePath' is missing the required '## Automation Feasibility' section." }
    }

    return @{ Ok = $true; Message = $null }
}

function Test-NumericDerivationEvidence {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Content
    )

    $numericClaimPattern = '(?im)^\s*[-*]\s*Numeric\s+spec\.md\s+acceptance\s+criterion:\s*.*\b\d+\b'
    if (-not [regex]::IsMatch($Content, $numericClaimPattern)) {
        return @{ Ok = $true; Message = $null }
    }

    $section = [regex]::Match($Content, '(?ims)^##\s+Numeric\s+Derivation\s+Evidence\s*$.*?(?=^##\s|\z)')
    if (-not $section.Success) {
        return @{ Ok = $false; Message = 'task-researcher hook: numeric spec.md acceptance criterion is missing ## Numeric Derivation Evidence.' }
    }

    $requiredLabels = @(
        'Complete Family', 'Exhaustive Search Scope', 'Inclusion Rules', 'Exclusion Rules',
        'Primary Search Strategy or Query Expression', 'Primary Member Set', 'Primary Count',
        'Cross-check Search Strategy or Query Expression', 'Cross-check Member Set', 'Cross-check Count',
        'Member-set Comparison'
    )
    $values = @{}
    foreach ($label in $requiredLabels) {
        $match = [regex]::Match($section.Value, "(?im)^[\t ]*[-*]?[\t ]*$([regex]::Escape($label))[\t ]*:[\t ]*(?<value>\S(?:.*\S)?)[\t ]*$")
        if (-not $match.Success) { return @{ Ok = $false; Message = "task-researcher hook: numeric derivation evidence is missing $label." } }
        $values[$label] = $match.Groups['value'].Value.Trim()
    }
    if ($values['Exhaustive Search Scope'] -notmatch '(?i)\b(entire|all|complete)\b.*\b(repository|repo|source tree|tree)\b') {
        return @{ Ok = $false; Message = 'task-researcher hook: numeric derivation evidence does not declare an exhaustive repository search scope.' }
    }
    $primaryStrategy = $values['Primary Search Strategy or Query Expression']
    $crossCheckStrategy = $values['Cross-check Search Strategy or Query Expression']
    if ($primaryStrategy -match '(?i)\b(single|narrow|named[- ]?pattern)\b' -or $crossCheckStrategy -match '(?i)\b(single|narrow|named[- ]?pattern)\b') {
        return @{ Ok = $false; Message = 'task-researcher hook: numeric derivation evidence uses a narrow named-pattern search.' }
    }
    if ([regex]::Replace($primaryStrategy, '\s+', '').ToLowerInvariant() -eq [regex]::Replace($crossCheckStrategy, '\s+', '').ToLowerInvariant()) {
        return @{ Ok = $false; Message = 'task-researcher hook: numeric derivation cross-check repeats the primary search strategy or query expression.' }
    }
    $familyMembers = @($values['Complete Family'].Split(',') | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    foreach ($familyMember in $familyMembers) {
        if ($primaryStrategy -notmatch [regex]::Escape($familyMember) -or $crossCheckStrategy -notmatch [regex]::Escape($familyMember)) {
            return @{ Ok = $false; Message = "task-researcher hook: numeric derivation search does not cover complete family member '$familyMember'." }
        }
    }
    if ($values['Primary Count'] -notmatch '^\d+$' -or $values['Cross-check Count'] -notmatch '^\d+$') { return @{ Ok = $false; Message = 'task-researcher hook: numeric derivation counts must be numeric.' } }
    $primaryMembers = @($values['Primary Member Set'].Split(',') | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $crossCheckMembers = @($values['Cross-check Member Set'].Split(',') | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ([int]$values['Primary Count'] -ne $primaryMembers.Count -or [int]$values['Cross-check Count'] -ne $crossCheckMembers.Count) {
        return @{ Ok = $false; Message = 'task-researcher hook: numeric derivation count does not match its independently enumerated member set.' }
    }
    $normalizedPrimaryMembers = @($primaryMembers | ForEach-Object { $_.ToLowerInvariant() } | Sort-Object -Unique) -join '|'
    $normalizedCrossCheckMembers = @($crossCheckMembers | ForEach-Object { $_.ToLowerInvariant() } | Sort-Object -Unique) -join '|'
    if ($normalizedPrimaryMembers -ne $normalizedCrossCheckMembers) { return @{ Ok = $false; Message = 'task-researcher hook: numeric derivation primary and cross-check member sets disagree.' } }
    if ($values['Member-set Comparison'] -notmatch '(?i)\b(equal|match|identical)\b') { return @{ Ok = $false; Message = 'task-researcher hook: numeric derivation evidence is missing an explicit member-set comparison.' } }

    return @{ Ok = $true; Message = $null }
}

function Invoke-TaskResearcherOutputValidation {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [string] $RawPayload
    )

    if ([string]::IsNullOrWhiteSpace($RawPayload)) {
        return @{ Ok = $false; Message = 'task-researcher hook: CLAUDE_HOOK_INPUT is empty; cannot validate task-researcher output.' }
    }

    try {
        $payload = $RawPayload | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        return @{ Ok = $false; Message = "task-researcher hook: failed to parse CLAUDE_HOOK_INPUT as JSON: $($_.Exception.Message)" }
    }

    $agentOutput = $null
    if ($payload.PSObject.Properties.Name -contains 'output') {
        $agentOutput = $payload.output
    }
    if ([string]::IsNullOrWhiteSpace($agentOutput)) {
        return @{ Ok = $false; Message = 'task-researcher hook: agent output is empty; researcher must return research-path before termination.' }
    }

    $researchPath = Get-ResearchPathFromOutput -AgentOutput $agentOutput
    if ($null -eq $researchPath) {
        return @{ Ok = $false; Message = 'task-researcher hook: agent output does not advertise a research-path. Researcher must report `research-path: <path>` pointing to docs/features/<feature>/research/ (feature-associated) or docs/research/ (one-off).' }
    }

    if (-not (Test-IsUnderResearchRoot -Path $researchPath)) {
        return @{ Ok = $false; Message = "task-researcher hook: research-path '$researchPath' is not under a tracked research root. All research artifacts must be written to docs/features/<feature>/research/ (feature-associated) or docs/research/ (one-off)." }
    }

    if (-not (Test-IsValidResearchFileName -Path $researchPath)) {
        return @{ Ok = $false; Message = "task-researcher hook: research-path '$researchPath' does not match the required filename convention `<timestamp>-<short-name>-research.md` under docs/features/<feature>/research/ or docs/research/." }
    }

    if (-not (Test-ResearchFile -Path $researchPath)) {
        return @{ Ok = $false; Message = "task-researcher hook: researcher advertised research-path '$researchPath' but no file exists at that location." }
    }

    $feasibilityResult = Test-AutomationFeasibilitySection -ResearchFilePath $researchPath -AgentOutput $agentOutput
    if (-not $feasibilityResult.Ok) {
        return @{ Ok = $false; Message = $feasibilityResult.Message }
    }

    if (Test-Path -LiteralPath $researchPath -PathType Leaf) {
        $numericEvidenceResult = Test-NumericDerivationEvidence -Content (Get-Content -LiteralPath $researchPath -Raw -ErrorAction Stop)
        if (-not $numericEvidenceResult.Ok) {
            return $numericEvidenceResult
        }
    }

    return @{ Ok = $true; Message = $null }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

$result = Invoke-TaskResearcherOutputValidation -RawPayload $env:CLAUDE_HOOK_INPUT
if (-not $result.Ok) {
    Write-Error $result.Message
    exit 1
}

exit 0
