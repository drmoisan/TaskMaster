---
name: koverage-analyzer-finding-misattributed
description: The pre-existing PSUseSingularNouns finding in Invoke-MSTestWithCoverage.Helpers.ps1 is on Get-CoberturaLineConditionCoverageParts, not Merge-CoberturaClassesByFilename as issue.md/prior evidence claim
metadata:
  type: project
---

In `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, the single pre-existing PSScriptAnalyzer `PSUseSingularNouns` warning is on the function `Get-CoberturaLineConditionCoverageParts` (HEAD baseline line 123; shifts to 133 when comment lines are added).

**Why:** Issue #193's `issue.md` AC5 and the implementer's `evidence/qa-gates/final-toolchain.2026-06-13T01-56.md` both attribute this finding to `Merge-CoberturaClassesByFilename`. That attribution is wrong; `Invoke-ScriptAnalyzer -Path` reports `Get-CoberturaLineConditionCoverageParts`. The finding pre-exists on HEAD regardless, so the verdict is unaffected — but do not copy the wrong function name into audits.

**How to apply:** When auditing changes to this helper module, verify the analyzer-finding function name with a fresh `Invoke-ScriptAnalyzer` run rather than trusting the issue/evidence text. Both functions are outside the #193 changed function `Get-KoverageProjectAllowlist`. See [[powershell-coverage-mandatory-when-ps1-in-diff]].
