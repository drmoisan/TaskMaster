<#
.SYNOPSIS
    Validates numeric acceptance criteria produced by the prd-feature worker.

.NOTES
    Reads the SubagentStop envelope JSON from stdin through the shared envelope helper
    (subagentstop-envelope.ps1) and validates its last_assistant_message field, the prd-feature
    worker's last assistant message. Exits 0 to allow. A block writes the reason to stderr and
    exits 2 through the shared envelope helper; an invalid envelope blocks with a payload anomaly
    reason. A hook dependency that fails to load blocks with exit 2 and the
    validate-prd-feature-output: prefix.
    An unhandled error during validation blocks with exit 2 and a reason (issue #928).
#>

[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }
try { . (Join-Path $PSScriptRoot 'subagentstop-envelope.ps1') } catch { Add-HookDependencyFailure -Name 'subagentstop-envelope.ps1' -ErrorRecord $_ }

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-ArtifactPathFromOutput {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory = $true)][string] $Output, [Parameter(Mandatory = $true)][string] $Label)

    $match = [regex]::Match($Output, "(?im)^\s*$([regex]::Escape($Label))\s*:\s*(?<path>\S+)")
    if ($match.Success) { return $match.Groups['path'].Value }
    return $null
}

function Test-NumericDerivationEvidence {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string] $Content)

    $section = [regex]::Match($Content, '(?ims)^##\s+Numeric\s+Derivation\s+Evidence\s*$.*?(?=^##\s|\z)')
    if (-not $section.Success) { return @{ Ok = $false; Message = 'prd-feature hook: numeric criterion requires ## Numeric Derivation Evidence.' } }
    $requiredLabels = @(
        'Complete Family', 'Exhaustive Search Scope', 'Inclusion Rules', 'Exclusion Rules',
        'Primary Search Strategy or Query Expression', 'Primary Member Set', 'Primary Count',
        'Cross-check Search Strategy or Query Expression', 'Cross-check Member Set', 'Cross-check Count',
        'Member-set Comparison'
    )
    $values = @{}
    foreach ($label in $requiredLabels) {
        $match = [regex]::Match($section.Value, "(?im)^[\t ]*[-*]?[\t ]*$([regex]::Escape($label))[\t ]*:[\t ]*(?<value>\S(?:.*\S)?)[\t ]*$")
        if (-not $match.Success) { return @{ Ok = $false; Message = "prd-feature hook: numeric derivation evidence is missing $label." } }
        $values[$label] = $match.Groups['value'].Value.Trim()
    }
    if ($values['Exhaustive Search Scope'] -notmatch '(?i)\b(entire|whole|all|complete)\b.*\b(repository|repo|worktree|source tree|tree)\b') { return @{ Ok = $false; Message = 'prd-feature hook: numeric derivation evidence does not declare an exhaustive repository search scope.' } }
    $primaryStrategy = $values['Primary Search Strategy or Query Expression']
    $crossCheckStrategy = $values['Cross-check Search Strategy or Query Expression']
    if ($primaryStrategy -match '(?i)\b(single|narrow|named[- ]?pattern)\b' -or $crossCheckStrategy -match '(?i)\b(single|narrow|named[- ]?pattern)\b') { return @{ Ok = $false; Message = 'prd-feature hook: numeric derivation evidence uses a narrow named-pattern search.' } }
    if ([regex]::Replace($primaryStrategy, '\s+', '').ToLowerInvariant() -eq [regex]::Replace($crossCheckStrategy, '\s+', '').ToLowerInvariant()) { return @{ Ok = $false; Message = 'prd-feature hook: numeric derivation cross-check repeats the primary search strategy or query expression.' } }
    $familyMembers = @($values['Complete Family'].Split(',') | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    foreach ($familyMember in $familyMembers) {
        if ($primaryStrategy -notmatch [regex]::Escape($familyMember) -or $crossCheckStrategy -notmatch [regex]::Escape($familyMember)) { return @{ Ok = $false; Message = "prd-feature hook: numeric derivation search does not cover complete family member '$familyMember'." } }
    }
    if ($values['Primary Count'] -notmatch '^\d+$' -or $values['Cross-check Count'] -notmatch '^\d+$') { return @{ Ok = $false; Message = 'prd-feature hook: numeric derivation counts must be numeric.' } }
    $primaryMembers = @($values['Primary Member Set'].Split(',') | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $crossCheckMembers = @($values['Cross-check Member Set'].Split(',') | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ([int]$values['Primary Count'] -ne $primaryMembers.Count -or [int]$values['Cross-check Count'] -ne $crossCheckMembers.Count) { return @{ Ok = $false; Message = 'prd-feature hook: numeric derivation count does not match its independently enumerated member set.' } }
    $normalizedPrimaryMembers = @($primaryMembers | ForEach-Object { $_.ToLowerInvariant() } | Sort-Object -Unique) -join '|'
    $normalizedCrossCheckMembers = @($crossCheckMembers | ForEach-Object { $_.ToLowerInvariant() } | Sort-Object -Unique) -join '|'
    if ($normalizedPrimaryMembers -ne $normalizedCrossCheckMembers) { return @{ Ok = $false; Message = 'prd-feature hook: numeric derivation primary and cross-check member sets disagree.' } }
    if ($values['Member-set Comparison'] -notmatch '(?i)\b(equal|match|identical)\b') { return @{ Ok = $false; Message = 'prd-feature hook: numeric derivation evidence is missing an explicit member-set comparison.' } }
    return @{ Ok = $true; Message = $null }
}

function Test-SpecNumericCriterion {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string] $SpecContent)

    $section = [regex]::Match($SpecContent, '(?ims)^##\s+Acceptance\s+Criteria\s*$.*?(?=^##\s|\z)')
    if (-not $section.Success) { return $false }
    # Rule R2 (issue #905): a criterion is numeric only when it states a determiner-led cardinal count of a
    # plural noun and cites no policy file.
    $derivedCountPattern = '\b(?:all|exactly|the|these|those)\s+(?:[1-9]\d*|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|twenty)\s+(?:(?!(?:seconds|minutes|hours|days|weeks|milliseconds|lines|bytes|characters|percent|is|as|us|has|was|does|this|its|thus|plus|less|unless|across|always|yes)\b)[\w`./-]+\s+){0,3}?(?!(?:seconds|minutes|hours|days|weeks|milliseconds|lines|bytes|characters|percent|is|as|us|has|was|does|this|its|thus|plus|less|unless|across|always|yes)\b)[a-z][\w-]*s\b'
    $policyMarkers = @('.claude/rules/', '.github/instructions/', '.github/copilot-instructions.md')
    foreach ($line in ($section.Value -split "`r?`n")) {
        if ($line -notmatch '^\s*[-*]\s*\[.\]') { continue }
        if (-not [regex]::IsMatch($line, $derivedCountPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) { continue }
        $normalized = $line -replace '\\', '/'
        if (@($policyMarkers | Where-Object { $normalized.Contains($_) }).Count -gt 0) { continue }
        return $true
    }
    return $false
}

function Invoke-PrdFeatureOutputValidation {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([AllowNull()] [AllowEmptyString()] [string] $InputText)
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'validate-prd-feature-output:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }
    $envelope = if ($PSBoundParameters.ContainsKey('InputText')) { Read-SubagentStopEnvelope -InputText $InputText } else { Read-SubagentStopEnvelope }
    if (-not $envelope.IsValid) { return @{ Ok = $false; Message = "prd-feature hook: payload anomaly - $(Get-SubagentStopEnvelopeAnomalyReason -Anomaly $envelope.Anomaly)" } }
    $assistantMessage = [string]$envelope.AssistantText
    if ([string]::IsNullOrWhiteSpace($assistantMessage)) { return @{ Ok = $false; Message = 'prd-feature hook: last assistant message is empty.' } }
    $specPath = Get-ArtifactPathFromOutput -Output $assistantMessage -Label 'spec-path'
    if ([string]::IsNullOrWhiteSpace($specPath) -or -not (Test-Path -LiteralPath $specPath -PathType Leaf)) { return @{ Ok = $false; Message = 'prd-feature hook: spec-path is required and must exist.' } }
    try { $specContent = Get-Content -LiteralPath $specPath -Raw -ErrorAction Stop } catch { return @{ Ok = $false; Message = "prd-feature hook: failed to read spec-path '$specPath': $($_.Exception.Message)" } }
    if (-not (Test-SpecNumericCriterion -SpecContent $specContent)) { return @{ Ok = $true; Message = $null } }
    $researchPath = Get-ArtifactPathFromOutput -Output $assistantMessage -Label 'research-path'
    if ([string]::IsNullOrWhiteSpace($researchPath) -or -not (Test-Path -LiteralPath $researchPath -PathType Leaf)) { return @{ Ok = $false; Message = 'prd-feature hook: numeric acceptance criterion requires an existing research-path.' } }
    try { $researchContent = Get-Content -LiteralPath $researchPath -Raw -ErrorAction Stop } catch { return @{ Ok = $false; Message = "prd-feature hook: failed to read research-path '$researchPath': $($_.Exception.Message)" } }
    return Test-NumericDerivationEvidence -Content $researchContent
}

if ($MyInvocation.InvocationName -eq '.') { return }
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('validate-prd-feature-output: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'validate-prd-feature-output:'
if ($null -ne $dependencyDecision) { [Console]::Error.WriteLine($dependencyDecision.Reason); exit $dependencyDecision.ExitCode }
try { $result = Invoke-PrdFeatureOutputValidation }
catch { Exit-SubagentStopBlock -Message ('validate-prd-feature-output: validator error: ' + ([string]$_.Exception.Message -split "`r?`n")[0] + '; the gate fails closed.' + "`n" + 'This condition is not repairable by the subagent. Report it to the operator and halt.') }
if (-not $result.Ok) { Exit-SubagentStopBlock -Message $result.Message }
exit 0
