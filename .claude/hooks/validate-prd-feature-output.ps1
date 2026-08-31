<#
.SYNOPSIS
    Validates numeric acceptance criteria produced by the prd-feature worker.
#>

[CmdletBinding()]
param()

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
    if ($values['Exhaustive Search Scope'] -notmatch '(?i)\b(entire|all|complete)\b.*\b(repository|repo|source tree|tree)\b') { return @{ Ok = $false; Message = 'prd-feature hook: numeric derivation evidence does not declare an exhaustive repository search scope.' } }
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
    return $section.Success -and [regex]::IsMatch($section.Value, '(?m)^\s*[-*]\s*\[.\].*\b\d+\b')
}

function Invoke-PrdFeatureOutputValidation {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([string] $RawPayload)

    if ([string]::IsNullOrWhiteSpace($RawPayload)) { return @{ Ok = $false; Message = 'prd-feature hook: CLAUDE_HOOK_INPUT is empty.' } }
    try { $payload = $RawPayload | ConvertFrom-Json -ErrorAction Stop } catch { return @{ Ok = $false; Message = "prd-feature hook: failed to parse CLAUDE_HOOK_INPUT as JSON: $($_.Exception.Message)" } }
    $output = if ($null -ne $payload.PSObject.Properties['output']) { $payload.output } else { $null }
    if ([string]::IsNullOrWhiteSpace($output)) { return @{ Ok = $false; Message = 'prd-feature hook: agent output is empty.' } }
    $specPath = Get-ArtifactPathFromOutput -Output $output -Label 'spec-path'
    if ([string]::IsNullOrWhiteSpace($specPath) -or -not (Test-Path -LiteralPath $specPath -PathType Leaf)) { return @{ Ok = $false; Message = 'prd-feature hook: spec-path is required and must exist.' } }
    $specContent = Get-Content -LiteralPath $specPath -Raw -ErrorAction Stop
    if (-not (Test-SpecNumericCriterion -SpecContent $specContent)) { return @{ Ok = $true; Message = $null } }
    $researchPath = Get-ArtifactPathFromOutput -Output $output -Label 'research-path'
    if ([string]::IsNullOrWhiteSpace($researchPath) -or -not (Test-Path -LiteralPath $researchPath -PathType Leaf)) { return @{ Ok = $false; Message = 'prd-feature hook: numeric acceptance criterion requires an existing research-path.' } }
    return Test-NumericDerivationEvidence -Content (Get-Content -LiteralPath $researchPath -Raw -ErrorAction Stop)
}

if ($MyInvocation.InvocationName -eq '.') { return }
$result = Invoke-PrdFeatureOutputValidation -RawPayload $env:CLAUDE_HOOK_INPUT
if (-not $result.Ok) { Write-Error $result.Message; exit 1 }
exit 0
