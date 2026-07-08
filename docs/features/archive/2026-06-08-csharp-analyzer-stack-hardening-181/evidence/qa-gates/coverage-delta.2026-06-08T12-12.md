# P6-T6 — Coverage Threshold and No-Regression Verification (Issue #181)

Timestamp: 2026-06-08T13-39

## Coverage comparison
| Measure | Value | Source |
|---|---|---|
| Baseline repo-wide line coverage | 58.89% (101554 / 172456) | P0-T6 (`evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`) |
| Post-change repo-wide line coverage | 58.99% (101734 / 172456) | P6-T5 (`evidence/qa-gates/final-test-coverage.2026-06-08T12-12.md`) |
| Delta | +0.10 percentage points | — |
| Changed-code coverage | N/A (no production `.cs` lines changed) | see below |

## Changed-code coverage
This feature changes only build configuration (`packages.config`, `.csproj`), `.editorconfig`, `BannedSymbols.txt`, and `.claude/rules/csharp.md`. No production or test `.cs` source lines were added or modified. Therefore:
- No new compile-required production code was introduced, so the >= 90% new-code obligation is not triggered.
- No changed `.cs` lines exist, so there is no changed-line coverage to regress.

## Threshold assessment
- Repo-wide raw line coverage is 58.99%, above the 58.89% baseline (no regression). The raw figure is collected over ALL instrumented modules, including vendored assemblies and large COM/interop/auto-generated code that the CI coverage gate scopes out. The authoritative repo-wide 80% and new-code 90% policy gates are evaluated by the PR GitHub Actions CI run, which applies the repository's coverage configuration; this local raw figure is the no-regression reference, not the policy-gate value.
- The known Moq binding-redirect caveat (System.Threading.Tasks.Extensions) did not block coverage collection; the collector produced a `.coverage` attachment successfully.

## Verdict
No coverage regression (post-change 58.99% >= baseline 58.89%). No changed production `.cs` lines, so no new-code threshold is triggered. Canonical Cobertura coverage at `artifacts/csharp/coverage.xml`.
