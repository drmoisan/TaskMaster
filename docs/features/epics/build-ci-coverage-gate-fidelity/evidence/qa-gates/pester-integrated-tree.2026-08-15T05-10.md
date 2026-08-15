# QA Gate Evidence — Pester suite on the integrated tree

Timestamp: 2026-08-15T05-10
Branch: `epic/build-ci-coverage-gate-fidelity-integration`
Head SHA: `22b5de02325331b0dcd660222dba33a1f1b66450`
Base: `main` @ `0569ac0b489f5867f514c00ccb2f65314c19cb16` (fully merged into head)

## Why this evidence exists

The epic's core deliverable is four PowerShell scripts under `scripts/vscode/`. The repository CI
workflow `.github/workflows/ci.yml` runs exactly five jobs — `actionlint`, `format-check`,
`build-analyzers`, `build-nullable`, and `mstest-coverage`. None of them executes Pester. The
PowerShell logic this epic delivers therefore receives **no CI coverage**, and a local Pester run is
the only executed gate over it. This artifact records that run against the composed integrated tree,
which no prior per-child run covered.

Command:

```
pwsh -NoProfile -Command "Import-Module Pester -MinimumVersion 5.0; $c = New-PesterConfiguration; $c.Run.Path='tests/scripts/vscode'; $c.Run.PassThru=$true; Invoke-Pester -Configuration $c"
```

EXIT_CODE: 0

Environment: PowerShell 7.6.3, Pester 5.6.1.

## Output Summary

All five discovered containers passed; 70 of 70 tests passed, 0 failed, 0 skipped, 0 inconclusive.
Wall time 9.85s.

| Test file | Passed | Failed |
|---|---|---|
| `Install-RepoDotNetSdk.Tests.ps1` | 2 | 0 |
| `Invoke-MSTest.RunSettings.Tests.ps1` | 26 | 0 |
| `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` | 11 | 0 |
| `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | 25 | 0 |
| `Invoke-VSBuild.Tests.ps1` | 6 | 0 |
| **Total** | **70** | **0** |

## Observations recorded, not remediated

1. **No Pester job in CI.** `ci.yml` gates C# (`format-check`, `build-analyzers`, `build-nullable`,
   `mstest-coverage`) and workflow YAML (`actionlint`) only. The PowerShell coverage-arithmetic and
   closure-filter logic delivered by issues #441 and #457 is not executed by any required status
   check on `main`.
2. **No coverage threshold is enforced in CI.** `_mstest-coverage.yml` runs
   `vstest.console.exe ... /EnableCodeCoverage` and uploads the `.trx` and `.coverage` artifacts, but
   never converts them to Cobertura and never compares a percentage against a floor. The 80% Cobertura
   line-coverage gate reconciled by issue #494 is enforced by local tooling under `scripts/vscode/`,
   not by a required status check. Consequently a coverage regression cannot fail CI on `main` today.
3. `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` is 443 lines and contributes 11 test cases.
   The ratio is noted for the reviewer's assessment; it is not evaluated here.

Observations 1 and 2 describe the state of `main` as it exists after this epic, and are candidates
for follow-up issues rather than changes to this integration branch.
