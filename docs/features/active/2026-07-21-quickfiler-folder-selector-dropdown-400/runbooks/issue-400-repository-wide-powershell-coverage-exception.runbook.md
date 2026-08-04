# Issue #400 Repository-Wide PowerShell Coverage Exception

## Cue

Use this runbook only when issue #400 has completed the mandatory PoshQC format, analyzer, and Pester gates; the focused coverage collection for `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is at least 90%; and the repository-wide PowerShell aggregate remains below the 80% policy floor because of pre-existing coverage debt. The user authorized this bounded exception on 2026-08-04.

## Prerequisites

- The issue #400 remediation plan remains the plan of record.
- Mandatory MCP PoshQC format, analyzer, and Pester results are recorded as passing.
- The focused deterministic wrapper coverage evidence records at least 90% coverage for `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
- The repository-wide coverage diagnostic records the aggregate result and confirms that no coverage configuration, filter, exclusion, or threshold was changed.
- Changed-line coverage has not regressed from its recorded baseline.

## Step-by-step Instructions

1. Record the exception only in the issue #400 orchestration checkpoint as a `human_interaction` requirement whose response is `exception` and whose `runbook_path` is this file.
2. Limit the exception to the pre-existing repository-wide PowerShell aggregate coverage below 80% for issue #400. Do not apply it to another issue, branch, script, or test suite.
3. Keep the mandatory PoshQC format, analyzer, and Pester gates in force. Do not treat this exception as authority to skip, weaken, filter, exclude, or reconfigure any gate.
4. Keep the focused coverage requirement for `scripts/vscode/Invoke-MSTestWithCoverage.ps1` at or above 90%, and require no coverage regression on changed lines.
5. Preserve coverage configuration, filters, exclusions, and thresholds byte-for-byte. If any change is needed, stop this exception path and obtain separate authorization for a materially expanded repository-wide remediation scope.
6. Mirror the authorization and its bounded scope in the issue #400 update evidence before completing the feature-review, PR, CI, and checkpoint gates.

## Verification

- The checkpoint records `response: "exception"`, this non-empty `runbook_path`, and a resolution timestamp.
- PoshQC format, analyzer, and Pester evidence is present and passing.
- Focused wrapper coverage is at least 90%, with changed-line coverage non-regressed.
- The repository-wide coverage diagnostic remains recorded as pre-existing debt below 80%; it is not represented as a passing aggregate gate.
- The diff confirms that coverage configuration, filters, exclusions, and thresholds were not modified.

## Source and Citation

- Issue #400: https://github.com/drmoisan/TaskMaster/issues/400 (captured 2026-08-04).
- Pull request #416: https://github.com/drmoisan/TaskMaster/pull/416 (captured 2026-08-04).
- Repository PowerShell coverage policy: `AGENTS.md` and `.agents/skills/powershell/SKILL.md` (reviewed 2026-08-04).
