<#
.SYNOPSIS
    SubagentStop hook for the atomic-planner subagent.

.DESCRIPTION
    Blocks termination of the atomic-planner subagent unless the agent's final
    output advertises a plan-path, includes an exact preflight signal, and the
    advertised plan file satisfies the repository's mechanical atomic-plan
    structure requirements.

    Enforced checks:
      - hook payload is valid JSON and contains non-empty output,
      - output advertises `plan-path: <path>` (or supported equivalent forms),
      - output contains `PREFLIGHT: ALL CLEAR` or
        `PREFLIGHT: REVISIONS REQUIRED`,
      - the advertised plan exists on disk,
      - the plan contains canonical `### Phase N — <Title>` headings,
      - Phase 0 exists and includes policy-read and baseline tasks,
      - each task uses `- [ ] [P#-T#]` (or checked equivalent),
      - task numbering is sequential within each phase,
      - every task includes at least one explicit path token,
      - the final phase contains QA-oriented work.

.NOTES
    Reads the hook payload from CLAUDE_HOOK_INPUT as JSON. Exits 0 to allow
    termination; exits 1 with an error message to block. Filesystem reads go
    through Get-PlanFileContent so tests can mock the boundary without writing
    temporary files.
#>

[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-PlanFileContent {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return @{ Exists = $false; Lines = @() }
    }

    $lines = Get-Content -LiteralPath $Path -ErrorAction Stop
    if ($null -eq $lines) {
        $lines = @()
    }
    elseif ($lines -isnot [array]) {
        $lines = @($lines)
    }

    return @{ Exists = $true; Lines = $lines }
}

function Get-PlanPathFromOutput {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $AgentOutput
    )

    $pattern = 'plan-path\s*[:=]\s*["'']?([^\s"''\)`]+)|\[plan-path\]\(([^)]+)\)'
    $match = [regex]::Match(
        $AgentOutput,
        $pattern,
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )
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

function Test-TaskContainsExplicitPath {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $TaskText
    )

    $pathPattern = '(?i)(?:[.\w*?-]+[\\/])+[.\w*?-]+'
    return [regex]::IsMatch($TaskText, $pathPattern)
}

function Test-HasPreflightSignal {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $AgentOutput
    )

    return [regex]::IsMatch(
        $AgentOutput,
        '(?m)^\s*PREFLIGHT:\s*(ALL CLEAR|REVISIONS REQUIRED)\s*$'
    )
}

function Get-PlannerInternalReviewValidation {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([Parameter(Mandatory = $true)][string] $AgentOutput)

    $lines = @($AgentOutput -split "`r?`n")
    $labelPattern = '^\s*(?:PLANNER-INTERNAL-REVIEW|CITATION-TO-TREE|AC-TRACEABILITY|SCOPE-BOUNDARY|CITATION|AC-INVENTORY|AC-MAPPING|UNRESOLVED-GAPS|PREFLIGHT)\s*:'
    $headerPattern = '^\s*PLANNER-INTERNAL-REVIEW\s*:\s*(?<Value>.*?)\s*$'
    $preflightPattern = '^\s*PREFLIGHT\s*:\s*(?:ALL CLEAR|REVISIONS REQUIRED)\s*$'
    $labelIndexes = [System.Collections.Generic.List[int]]::new()
    $headerIndexes = [System.Collections.Generic.List[int]]::new()
    $preflightIndexes = [System.Collections.Generic.List[int]]::new()

    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -match $labelPattern) {
            $labelIndexes.Add($index)
        }
        if ($lines[$index] -match $headerPattern) {
            $headerIndexes.Add($index)
        }
        if ($lines[$index] -match $preflightPattern) {
            $preflightIndexes.Add($index)
        }
    }

    if ($headerIndexes.Count -ne 1) {
        return @{ Ok = $false; Message = 'planner internal review must contain exactly one `PLANNER-INTERNAL-REVIEW:` declaration.' }
    }

    $headerIndex = $headerIndexes[0]
    $headerMatch = [regex]::Match($lines[$headerIndex], $headerPattern)
    if ($headerMatch.Groups['Value'].Value.Trim() -ne 'PASS') {
        return @{ Ok = $false; Message = 'planner internal review header must be exactly `PLANNER-INTERNAL-REVIEW: PASS`.' }
    }

    $preflightAfterHeader = @($preflightIndexes | Where-Object { $_ -gt $headerIndex })
    if ($preflightAfterHeader.Count -ne 1) {
        return @{ Ok = $false; Message = 'planner internal review must terminate at exactly one required `PREFLIGHT:` signal after its header.' }
    }

    $preflightIndex = $preflightAfterHeader[0]
    foreach ($labelIndex in $labelIndexes) {
        if ($labelIndex -lt $headerIndex -or $labelIndex -gt $preflightIndex) {
            return @{ Ok = $false; Message = 'planner internal review declarations must not occur outside the bounded record.' }
        }
    }

    $recordLines = @($lines[$headerIndex..$preflightIndex])
    $requiredDeclarations = @('CITATION-TO-TREE', 'AC-TRACEABILITY', 'SCOPE-BOUNDARY')
    foreach ($declaration in $requiredDeclarations) {
        $declarationMatches = @($recordLines | Where-Object { $_ -match "^\s*$declaration\s*:" })
        if ($declarationMatches.Count -ne 1) {
            return @{ Ok = $false; Message = "planner internal review must contain exactly one `$declaration: PASS` declaration." }
        }
        if ($declarationMatches[0] -notmatch "^\s*$declaration\s*:\s*PASS\s*$") {
            return @{ Ok = $false; Message = "planner internal review declaration `$declaration must be exactly PASS." }
        }
    }

    $citationLines = @($recordLines | Where-Object { $_ -match '^\s*CITATION\s*:' })
    if ($citationLines.Count -eq 0) {
        return @{ Ok = $false; Message = 'planner internal review must contain at least one `CITATION:` record.' }
    }
    foreach ($citationLine in $citationLines) {
        $citation = [regex]::Match($citationLine, '^\s*CITATION\s*:\s*(?<Path>[^|\s]+)\s*\|\s*(?<Locator>.+?\S)\s*$')
        if (-not $citation.Success -or $citation.Groups['Path'].Value -notmatch '^(?![A-Za-z]:)(?!/)(?!\\)(?:\.?[^/\\|\s]+)(?:/[^/\\|\s]+)+$') {
            return @{ Ok = $false; Message = 'planner internal review citations require a repository-relative path and nonblank locator.' }
        }
    }

    $inventoryLines = @($recordLines | Where-Object { $_ -match '^\s*AC-INVENTORY\s*:' })
    if ($inventoryLines.Count -ne 1) {
        return @{ Ok = $false; Message = 'planner internal review must contain exactly one nonblank `AC-INVENTORY:` declaration.' }
    }
    $inventoryValue = ($inventoryLines[0] -replace '^\s*AC-INVENTORY\s*:\s*', '').Trim()
    if ([string]::IsNullOrWhiteSpace($inventoryValue)) {
        return @{ Ok = $false; Message = 'planner internal review `AC-INVENTORY:` must contain nonblank unique IDs.' }
    }
    $inventoryIds = @($inventoryValue -split ',' | ForEach-Object { $_.Trim() })
    if ((@($inventoryIds | Where-Object { [string]::IsNullOrWhiteSpace($_) }).Count -gt 0) -or
        (@($inventoryIds | Select-Object -Unique).Count -ne $inventoryIds.Count)) {
        return @{ Ok = $false; Message = 'planner internal review `AC-INVENTORY:` must contain nonblank unique IDs.' }
    }

    $mappingLines = @($recordLines | Where-Object { $_ -match '^\s*AC-MAPPING\s*:' })
    if ($mappingLines.Count -eq 0) {
        return @{ Ok = $false; Message = 'planner internal review must contain one `AC-MAPPING:` record for every inventory ID.' }
    }
    $mappingIds = [System.Collections.Generic.List[string]]::new()
    foreach ($mappingLine in $mappingLines) {
        $mapping = [regex]::Match($mappingLine, '^\s*AC-MAPPING\s*:\s*(?<Id>[^|]*?)\s*\|\s*IMPLEMENTATION\s*:\s*(?<Implementation>[^|]*?)\s*\|\s*TESTS\s*:\s*(?<Tests>[^|]*?)\s*\|\s*EVIDENCE\s*:\s*(?<Evidence>.*?)\s*$')
        if (-not $mapping.Success -or
            [string]::IsNullOrWhiteSpace($mapping.Groups['Id'].Value) -or
            [string]::IsNullOrWhiteSpace($mapping.Groups['Implementation'].Value) -or
            [string]::IsNullOrWhiteSpace($mapping.Groups['Tests'].Value) -or
            [string]::IsNullOrWhiteSpace($mapping.Groups['Evidence'].Value)) {
            return @{ Ok = $false; Message = 'planner internal review `AC-MAPPING:` requires nonblank ID, IMPLEMENTATION, TESTS, and EVIDENCE fields.' }
        }
        $mappingIds.Add($mapping.Groups['Id'].Value.Trim())
    }
    if (@($mappingIds | Select-Object -Unique).Count -ne $mappingIds.Count) {
        return @{ Ok = $false; Message = 'planner internal review `AC-MAPPING:` identifiers must be unique.' }
    }
    if ($inventoryIds.Count -ne $mappingIds.Count -or
        (@($inventoryIds | Where-Object { $_ -notin $mappingIds }).Count -gt 0) -or
        (@($mappingIds | Where-Object { $_ -notin $inventoryIds }).Count -gt 0)) {
        return @{ Ok = $false; Message = 'planner internal review AC inventory and mapping identifiers must match exactly.' }
    }

    $gapLines = @($recordLines | Where-Object { $_ -match '^\s*UNRESOLVED-GAPS\s*:' })
    if ($gapLines.Count -ne 1 -or $gapLines[0] -notmatch '^\s*UNRESOLVED-GAPS\s*:\s*NONE\s*$') {
        return @{ Ok = $false; Message = 'planner internal review must contain exactly one `UNRESOLVED-GAPS: NONE` declaration.' }
    }

    return @{ Ok = $true; Message = $null }
}

function Get-PlanStructureValidationReport {
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $Lines
    )

    $phasePattern = '^### Phase (?<Phase>\d+)\s+—\s+(?<Title>.+)$'
    $taskPattern = '^- \[(?<State>[ xX])\] \[P(?<Phase>\d+)-T(?<Task>\d+)\] (?<Text>.+)$'
    $errors = [System.Collections.Generic.List[string]]::new()
    $tasksByPhase = @{}
    $phaseTitles = @{}
    $currentPhase = $null
    $expectedPhase = 0
    $foundTask = $false

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $lineNumber = $i + 1
        $line = $Lines[$i]

        if ($line -match '^### Phase ') {
            $phaseMatch = [regex]::Match($line, $phasePattern)
            if (-not $phaseMatch.Success) {
                $errors.Add("Line ${lineNumber}: phase heading must match `### Phase N — <Title>`.")
                $currentPhase = $null
                continue
            }

            $phaseNumber = [int]$phaseMatch.Groups['Phase'].Value
            if ($phaseNumber -ne $expectedPhase) {
                $errors.Add("Line ${lineNumber}: expected Phase $expectedPhase but found Phase $phaseNumber.")
                $expectedPhase = $phaseNumber
            }

            $currentPhase = $phaseNumber
            $expectedPhase = $phaseNumber + 1
            $tasksByPhase[$phaseNumber] = [System.Collections.Generic.List[string]]::new()
            $phaseTitles[$phaseNumber] = $phaseMatch.Groups['Title'].Value.Trim()
            continue
        }

        if ($line -notmatch '^- \[') {
            continue
        }

        $foundTask = $true
        $taskMatch = [regex]::Match($line, $taskPattern)
        if (-not $taskMatch.Success) {
            $errors.Add("Line ${lineNumber}: task line must match `- [ ] [P#-T#] <description>`.")
            continue
        }

        if ($null -eq $currentPhase) {
            $errors.Add("Line ${lineNumber}: task appears before the first canonical phase heading.")
            continue
        }

        $taskPhase = [int]$taskMatch.Groups['Phase'].Value
        $taskNumber = [int]$taskMatch.Groups['Task'].Value
        $taskText = $taskMatch.Groups['Text'].Value.Trim()

        if ($taskPhase -ne $currentPhase) {
            $errors.Add("Line ${lineNumber}: task phase P$taskPhase does not match current phase $currentPhase.")
        }

        if (-not $tasksByPhase.ContainsKey($taskPhase)) {
            $tasksByPhase[$taskPhase] = [System.Collections.Generic.List[string]]::new()
        }
        $expectedTaskNumber = $tasksByPhase[$taskPhase].Count + 1
        if ($taskNumber -ne $expectedTaskNumber) {
            $errors.Add("Line ${lineNumber}: expected task number T$expectedTaskNumber for phase $taskPhase, found T$taskNumber.")
        }

        if (-not (Test-TaskContainsExplicitPath -TaskText $taskText)) {
            $errors.Add("Line ${lineNumber}: task [P$taskPhase-T$taskNumber] must include at least one explicit path.")
        }

        $tasksByPhase[$taskPhase].Add($taskText)
    }

    if ($phaseTitles.Count -eq 0) {
        $errors.Add('Plan does not contain any canonical phase headings.')
        return [string[]]@($errors)
    }

    if (-not $foundTask) {
        $errors.Add('Plan does not contain any canonical task lines.')
    }

    if (-not $tasksByPhase.ContainsKey(0) -or $tasksByPhase[0].Count -eq 0) {
        $errors.Add('Plan must include Phase 0 with at least one baseline task.')
    }
    else {
        $phaseZeroText = ($tasksByPhase[0] -join "`n")
        if ($phaseZeroText -notmatch '(?i)\b(policy|instruction|read)\b') {
            $errors.Add('Phase 0 must include a policy-read task.')
        }
        if ($phaseZeroText -notmatch '(?i)\bbaseline\b') {
            $errors.Add('Phase 0 must include a baseline task.')
        }
    }

    $maxPhaseMeasure = ($phaseTitles.Keys | Measure-Object -Maximum).Maximum
    $maxPhase = if ($null -eq $maxPhaseMeasure) { $null } else { [int]$maxPhaseMeasure }
    $finalPhaseText = ''
    if ($null -ne $maxPhase -and $tasksByPhase.ContainsKey($maxPhase)) {
        $finalPhaseText = (($phaseTitles[$maxPhase]), ($tasksByPhase[$maxPhase] -join "`n")) -join "`n"
    }
    if ([string]::IsNullOrWhiteSpace($finalPhaseText) -or $finalPhaseText -notmatch '(?i)(qa|quality|toolchain|format|lint|type|test|coverage)') {
        $errors.Add('The final phase must contain explicit QA or toolchain validation work.')
    }

    return [string[]]@($errors)
}

function Invoke-PlannerOutputValidation {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [string] $RawPayload
    )

    if ([string]::IsNullOrWhiteSpace($RawPayload)) {
        return @{ Ok = $false; Message = 'atomic-planner hook: CLAUDE_HOOK_INPUT is empty; cannot validate planner output.' }
    }

    try {
        $payload = $RawPayload | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        return @{ Ok = $false; Message = "atomic-planner hook: failed to parse CLAUDE_HOOK_INPUT as JSON: $($_.Exception.Message)" }
    }

    $agentOutput = $null
    if ($null -ne $payload.PSObject.Properties['output']) {
        $agentOutput = $payload.output
    }
    if ([string]::IsNullOrWhiteSpace($agentOutput)) {
        return @{ Ok = $false; Message = 'atomic-planner hook: agent output is empty; planner must return plan-path and final preflight signal.' }
    }

    if (-not (Test-HasPreflightSignal -AgentOutput $agentOutput)) {
        return @{ Ok = $false; Message = 'atomic-planner hook: agent output must include `PREFLIGHT: ALL CLEAR` or `PREFLIGHT: REVISIONS REQUIRED`.' }
    }

    $planPath = Get-PlanPathFromOutput -AgentOutput $agentOutput
    if ($null -eq $planPath) {
        return @{ Ok = $false; Message = 'atomic-planner hook: agent output does not advertise a plan-path. Planner must report `plan-path: <path>` per atomic-plan-contract.' }
    }

    $file = Get-PlanFileContent -Path $planPath
    if (-not $file.Exists) {
        return @{ Ok = $false; Message = "atomic-planner hook: planner advertised plan-path '$planPath' but no file exists at that location." }
    }

    $errors = @(Get-PlanStructureValidationReport -Lines $file.Lines)
    if ($errors.Count -gt 0) {
        $message = "atomic-planner hook: plan '$planPath' violates the atomic plan contract:`n  - " + ($errors -join "`n  - ")
        return @{ Ok = $false; Message = $message }
    }

    $review = Get-PlannerInternalReviewValidation -AgentOutput $agentOutput
    if (-not $review.Ok) {
        return @{ Ok = $false; Message = "atomic-planner hook: $($review.Message)" }
    }

    return @{ Ok = $true; Message = $null }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

$result = Invoke-PlannerOutputValidation -RawPayload $env:CLAUDE_HOOK_INPUT
if (-not $result.Ok) {
    Write-Error $result.Message
    exit 1
}

exit 0
