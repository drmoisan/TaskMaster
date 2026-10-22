# Remediation QA Gate — Downstream Review-Gate Coverage Artifact

Timestamp: 2026-08-23T19-28

Command:
```powershell
New-Item -ItemType Directory -Force -Path "artifacts\csharp"
Copy-Item -LiteralPath "coverage\remediation.cobertura.xml" -Destination "artifacts\csharp\coverage.xml" -Force
Get-FileHash -Algorithm SHA256 -LiteralPath "coverage\remediation.cobertura.xml"
Get-FileHash -Algorithm SHA256 -LiteralPath "artifacts\csharp\coverage.xml"
([xml](Get-Content -Raw -LiteralPath "artifacts\csharp\coverage.xml")).coverage.'line-rate'
([xml](Get-Content -Raw -LiteralPath "artifacts\csharp\coverage.xml")).coverage.'branch-rate'
git check-ignore -q artifacts/csharp/coverage.xml
```
(run from the worktree root)

EXIT_CODE: 0

Output Summary:

| Measure | Value | Required |
| --- | --- | --- |
| `artifacts/csharp/coverage.xml` exists | yes | yes |
| SHA-256 of `coverage\remediation.cobertura.xml` | `94180AA0875F0C64AC2D8689F865EDF4A1ED7EB1B03292A9CD82FA82180050B1` | — |
| SHA-256 of `artifacts/csharp/coverage.xml` | `94180AA0875F0C64AC2D8689F865EDF4A1ED7EB1B03292A9CD82FA82180050B1` | must match |
| Byte-identical | **yes** | yes |
| Root `line-rate` | `0.855916` = **85.59%** | >= 85 |
| Root `branch-rate` | `0.790598` = **79.06%** | >= 75 |
| `git check-ignore -q artifacts/csharp/coverage.xml` | exit 0 (ignored) | — |

Both thresholds are met against the baseline of 85.55% line and 79.03% branch: the copied artifact
records 85.59% line (>= 85, and above the baseline) and 79.06% branch (>= 75, and above the
baseline).

## Location note

`artifacts/csharp/` is a **tool-output producer path** read by the downstream feature-review coverage
hook, not an evidence location. The evidence-location invariant is therefore unaffected: the numeric
record lives here, under
`docs/features/active/winformspumphost-suite-determinism-511/evidence/qa-gates/`. The copied XML
itself is excluded by the repository-root `.gitignore` — `git check-ignore` exits 0 on it — so it does
not disturb the P4-T9 clean-tree gate.
