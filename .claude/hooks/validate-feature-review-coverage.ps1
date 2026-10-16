<#
.SYNOPSIS
    SubagentStop hook that validates feature-review artifacts and coverage verdicts.

.DESCRIPTION
    Runs when the feature-review subagent terminates. Validates that the final
    output advertises the required review artifact paths, that those artifacts
    exist in the canonical active-feature location, and that the policy audit
    carries explicit PASS or FAIL coverage verdicts for each language with
    changed files in the branch diff.

    Required advertised output tokens:
      - policy-audit-path
      - code-review-path
      - feature-audit-path

    Optional advertised output token:
      - remediation-inputs-path

    Additional coverage rules:
      - Enumerate languages that have changed files in artifacts/pr_context.summary.txt:
          .ts / .tsx            -> TypeScript
          .py                   -> Python
          .ps1 / .psm1          -> PowerShell
          .cs                   -> CSharp
      - For each changed language, the policy audit must contain a
        coverage-scoped PASS or FAIL verdict.
      - Scope-narrowing phrases on coverage rows are treated as failures.
      - Coverage floors follow the threshold precedence of the pushed quality-tiers
        rule: when the repository's root CLAUDE.md states a line or branch coverage
        threshold, that threshold governs; otherwise the 85 percent line and 75 percent
        branch defaults govern.
      - Each metric falls back independently, so a root CLAUDE.md that states only
        a line threshold leaves the 75 percent branch default in force. The figures are
        read by Get-FeatureReviewCoverageThreshold (feature-review-coverage-thresholds.ps1).
      - The root CLAUDE.md is read by Get-FeatureReviewClaudeMdText: an absent root
        CLAUDE.md uses the defaults, and an unreadable root CLAUDE.md fails closed (the
        gate blocks, because the governing floors cannot be resolved).
      - When repo-wide line coverage for an available artifact is below the governing
        line threshold, the policy audit must carry a FAIL verdict for that language;
        branch coverage below the governing branch threshold blocks termination.

.NOTES
    Reads the SubagentStop envelope from stdin through Read-SubagentStopEnvelope
    (subagentstop-envelope.ps1). Exits 0 to allow termination. Every block path,
    including a payload anomaly and a validator error, blocks through
    Exit-SubagentStopBlock, which writes the reason to stderr and exits 2. A hook
    dependency failure blocks through the inline guard tail (stderr reason, exit 2).
    A condition the subagent can repair (a missing token, path, file, or coverage
    verdict) may block again; a condition it cannot repair blocks once and does not
    block again when stop_hook_active is true.
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }
try { . (Join-Path $PSScriptRoot 'feature-review-coverage-thresholds.ps1') } catch { Add-HookDependencyFailure -Name 'feature-review-coverage-thresholds.ps1' -ErrorRecord $_ }
try { . (Join-Path $PSScriptRoot 'validate-feature-review-coverage-metrics.ps1') } catch { Add-HookDependencyFailure -Name 'validate-feature-review-coverage-metrics.ps1' -ErrorRecord $_ }
try { . (Join-Path $PSScriptRoot 'validate-feature-review-coverage-rules.ps1') } catch { Add-HookDependencyFailure -Name 'validate-feature-review-coverage-rules.ps1' -ErrorRecord $_ }
try { . (Join-Path $PSScriptRoot 'subagentstop-envelope.ps1') } catch { Add-HookDependencyFailure -Name 'subagentstop-envelope.ps1' -ErrorRecord $_ }

$ErrorActionPreference = 'Stop'

function Invoke-FeatureReviewCoverageValidation {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [AllowNull()] [AllowEmptyString()] [string] $InputText
    )
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'validate-feature-review-coverage:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    $envelope = if ($PSBoundParameters.ContainsKey('InputText')) { Read-SubagentStopEnvelope -InputText $InputText } else { Read-SubagentStopEnvelope }
    if (-not $envelope.IsValid) {
        return @{ Ok = $false; Message = ('validate-feature-review-coverage: payload anomaly - ' + (Get-SubagentStopEnvelopeAnomalyReason -Anomaly $envelope.Anomaly)) }
    }

    # A single-pass do-while block carries the validation so that each finding exits with break; a
    # script-block invocation would be a non-constant command position, which the enforcement-hook
    # invocation scan rejects as unverifiable.
    $finding = $null
    try {
        do {
            $agentOutput = [string]$envelope.AssistantText
            if ([string]::IsNullOrWhiteSpace($agentOutput)) {
                $finding = @{ Message = 'validate-feature-review-coverage: agent output is empty; feature-review must report required artifact paths before termination.'; HasRepairable = $true; HasNonRepairable = $false }
                break
            }

            $requiredArtifacts = @(
                @{ Token = 'policy-audit-path'; Stem = 'policy-audit'; Description = 'policy audit artifact' },
                @{ Token = 'code-review-path'; Stem = 'code-review'; Description = 'code review artifact' },
                @{ Token = 'feature-audit-path'; Stem = 'feature-audit'; Description = 'feature audit artifact' }
            )

            $artifactMeta = @{}
            $errors = [System.Collections.Generic.List[string]]::new()

            foreach ($artifact in $requiredArtifacts) {
                $path = Get-ReviewArtifactPathFromOutput -AgentOutput $agentOutput -Token $artifact.Token
                if ($null -eq $path) {
                    $errors.Add("missing $($artifact.Token): <path> for the required $($artifact.Description)")
                    continue
                }

                $metadata = Get-ReviewArtifactInfo -Path $path -Stem $artifact.Stem
                if ($null -eq $metadata) {
                    $errors.Add("$($artifact.Token) '$path' is outside the required docs/features/active/.../$($artifact.Stem).<timestamp>.md location")
                    continue
                }

                $file = Get-ArtifactFileContent -Path $path
                if (-not $file.Exists) {
                    $errors.Add("$($artifact.Token) '$path' was advertised for the $($artifact.Description) but no file exists at that location")
                    continue
                }

                $metadata['Text'] = $file.Text
                $artifactMeta[$artifact.Stem] = $metadata
            }

            if ($artifactMeta.ContainsKey('policy-audit')) {
                $reference = $artifactMeta['policy-audit']
                foreach ($stem in @('code-review', 'feature-audit')) {
                    if (-not $artifactMeta.ContainsKey($stem)) {
                        continue
                    }
                    $candidate = $artifactMeta[$stem]
                    if ($candidate.Folder -ne $reference.Folder -or $candidate.Timestamp -ne $reference.Timestamp) {
                        $errors.Add("$stem artifact must share the same feature folder and timestamp as the policy audit artifact.")
                    }
                }

                $remediationPath = Get-ReviewArtifactPathFromOutput -AgentOutput $agentOutput -Token 'remediation-inputs-path'
                if ($null -ne $remediationPath) {
                    $remediation = Get-ReviewArtifactInfo -Path $remediationPath -Stem 'remediation-inputs'
                    if ($null -eq $remediation) {
                        $errors.Add("remediation-inputs-path '$remediationPath' is outside the required docs/features/active/.../remediation-inputs.<timestamp>.md location")
                    }
                    elseif ($remediation.Folder -ne $reference.Folder -or $remediation.Timestamp -ne $reference.Timestamp) {
                        $errors.Add('remediation-inputs artifact must share the same feature folder and timestamp as the policy audit artifact.')
                    }
                    elseif (-not (Get-ArtifactFileContent -Path $remediationPath).Exists) {
                        $errors.Add("remediation-inputs-path '$remediationPath' was advertised but no file exists at that location")
                    }
                }
            }

            if ($errors.Count -gt 0) {
                $message = "validate-feature-review-coverage: required review artifact validation failed:`n  - " + ($errors -join "`n  - ")
                $finding = @{ Message = $message; HasRepairable = $true; HasNonRepairable = $false }
                break
            }

            $prSummary = Get-ArtifactFileContent -Path 'artifacts/pr_context.summary.txt'
            $changedLanguages = if ($prSummary.Exists) { Get-ChangedLanguageSet -Lines $prSummary.Lines } else { [ordered]@{} }
            if ($changedLanguages.Count -eq 0) {
                break
            }

            $policyAuditText = $artifactMeta['policy-audit'].Text
            $claudeMd = Get-FeatureReviewClaudeMdText
            if ($null -ne $claudeMd.ReadError) {
                $finding = @{ Message = "validate-feature-review-coverage: root CLAUDE.md exists but could not be read ($($claudeMd.ReadError)); coverage floors cannot be resolved, so the gate fails closed."; HasRepairable = $false; HasNonRepairable = $true }
                break
            }
            $thresholds = Get-FeatureReviewCoverageThreshold -ClaudeMdText $claudeMd.Text
            $coverageFailures = [System.Collections.Generic.List[hashtable]]::new()
            foreach ($lang in $changedLanguages.Keys) {
                $repoPct = Get-LanguageRepoCoverage -Language $lang
                $branchPct = Get-LanguageBranchCoverage -Language $lang
                $result = Test-LanguageCoverageRow -AuditText $policyAuditText -Language $lang -RepoWidePct $repoPct -BranchPct $branchPct -LineFloor $thresholds.Line -BranchFloor $thresholds.Branch -LineSource $thresholds.LineSource -BranchSource $thresholds.BranchSource
                if (-not $result.Ok) {
                    $coverageFailures.Add($result)
                }
            }

            if ($coverageFailures.Count -gt 0) {
                $message = "validate-feature-review-coverage: coverage validation failed against branch diff:`n  - " + (@($coverageFailures | ForEach-Object { $_.Reason }) -join "`n  - ")
                $hasRepairable = @($coverageFailures | Where-Object { $_.Repairable -eq $true }).Count -gt 0
                $hasNonRepairable = @($coverageFailures | Where-Object { $_.Repairable -eq $false }).Count -gt 0
                $finding = @{ Message = $message; HasRepairable = $hasRepairable; HasNonRepairable = $hasNonRepairable }
            }
        } while ($false)
    }
    catch {
        $finding = @{ Message = ('validate-feature-review-coverage: validator error: ' + ([string]$_.Exception.Message -split "`r?`n")[0] + '; the gate fails closed.'); HasRepairable = $false; HasNonRepairable = $true }
    }

    if ($null -eq $finding) { return @{ Ok = $true; Message = $null } }
    if ($envelope.StopHookActive -and -not $finding.HasRepairable) { return @{ Ok = $true; Message = $null } }
    $message = if ($finding.HasNonRepairable) { "$($finding.Message) The subagent cannot repair this condition; report it in the final response and stop." } else { $finding.Message }
    return @{ Ok = $false; Message = $message }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('validate-feature-review-coverage: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'validate-feature-review-coverage:'
if ($null -ne $dependencyDecision) { [Console]::Error.WriteLine($dependencyDecision.Reason); exit $dependencyDecision.ExitCode }

try { $result = Invoke-FeatureReviewCoverageValidation }
catch { Exit-SubagentStopBlock -Message ('validate-feature-review-coverage: validator error: ' + ([string]$_.Exception.Message -split "`r?`n")[0] + '; the gate fails closed. The subagent cannot repair this condition; report it in the final response and stop.') }
if (-not $result.Ok) { Exit-SubagentStopBlock -Message $result.Message }
exit 0
