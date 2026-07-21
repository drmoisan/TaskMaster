# Phase 2 — Coverage Floor Verification via Gate Hook Functions (P2-T6)

Timestamp: 2026-07-20T23-16

Command:
```
pwsh -NoProfile -Command '
. ./.claude/hooks/validate-feature-review-coverage.ps1
Get-JacocoRepoCoverage   -Path "artifacts/csharp/coverage.xml"   # line
Get-JacocoBranchCoverage -Path "artifacts/csharp/coverage.xml"   # branch
'
```
(The hook returns early on dot-source — `if ($MyInvocation.InvocationName -eq '.') { return }` — so only
the coverage functions are exercised, not the SubagentStop body.)

EXIT_CODE: 0

Output Summary:
- Get-JacocoRepoCoverage (line): 86.54%  ->  >= 85% floor: PASS
- Get-JacocoBranchCoverage (branch): 80.85%  ->  >= 75% floor: PASS
- GATE: PASS.

The regenerated canonical HEAD artifact artifacts/csharp/coverage.xml parses under the same
Get-JacocoRepoCoverage / Get-JacocoBranchCoverage functions the feature-review coverage hook uses, and
clears both first-party floors. This resolves the R2 procedural FAIL (absent HEAD JaCoCo artifact) and
the AC-5 coverage sub-clause.
