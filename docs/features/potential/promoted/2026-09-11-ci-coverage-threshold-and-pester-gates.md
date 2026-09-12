# ci-coverage-threshold-and-pester-gates (Issue #869)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ci-coverage-threshold-and-pester-gates/ (Issue #869)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #869
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/869
- Last Updated: 2026-09-12
## Summary

CI collects C# coverage but enforces no threshold (#561), and runs no Pester at all, so the production PowerShell under `scripts/vscode/` has zero CI coverage (#562). Both gates are wired in one delivery against the thresholds the maintainer settled on 2026-09-11 under #563: 80% line and 75% branch for C#, 80% line for PowerShell. PowerShell coverage measures 78.3% today, so this item also raises `scripts/vscode` coverage above 80 rather than lowering the floor.

## Environment

- OS/version: GitHub Actions `windows-latest`; local Windows 11 Pro 10.0.26200
- Python version: not applicable (GitHub Actions YAML, PowerShell 7, Pester)
- Command/flags used: `.github/workflows/ci.yml` and `_mstest-coverage.yml`; `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- Data source or fixture: `main` at 3cb974422

## Steps to Reproduce

1. Read `.github/workflows/_mstest-coverage.yml`: coverage is collected and uploaded; no step converts to Cobertura or asserts a floor. The `main` ruleset's five required contexts (`actionlint`, `format-check`, `build-analyzers`, `build-nullable`, `mstest-coverage`) assert no percentage.
2. Search `.github/workflows/*.yml` for `Pester` or `Invoke-Pester`: zero matches.
3. Run Pester with coverage over `scripts/vscode` (12 production scripts, 12 test files): `COVERAGE LinePercent=78.3` (item #752 baseline, 2026-09-03).
4. Read `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`: `BeforeAll` dot-sources the script's top-level body, so a run invokes `vswhere.exe` and executes `Sync-PackageReferences.ps1`, which writes `.csproj` files. Measured coverage of that file was non-deterministic (53/84 versus 71/84 on the same lines).

## Expected Behavior

- `_mstest-coverage.yml` runs the same route as the local tooling (`Invoke-MSTestWithCoverage.ps1`), so the Cobertura document exists in CI, then asserts 80% line and 75% branch through `Assert-CoberturaLineCoverageThreshold` and a new branch assertion in `Invoke-MSTestWithCoverage.Threshold.ps1`. A deliberately introduced regression fails the job.
- A reusable `_pester.yml`, following the `_<name>.yml` convention in `.github/workflows/README.md`, runs Pester over `tests/scripts/vscode/` with coverage and asserts 80% line. `ci.yml` calls it.
- `Invoke-VSBuild.Tests.ps1` gains an injectable seam so the test does not invoke `vswhere.exe` or `Sync-PackageReferences.ps1`, and its measured coverage is deterministic on a clean runner.
- `scripts/vscode` line coverage is at or above 80% by adding tests to the least-covered scripts.
- The two new check-run names are listed for addition to the `main` ruleset's required contexts. The ruleset edit itself is a repository-settings change performed by the maintainer, not by the delivery; the item records the exact context names to add.

## Actual Behavior

A coverage regression on `main` cannot fail CI. A regression in the PowerShell coverage arithmetic or closure filter merges green.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the quality gates the policy documents describe are not enforced on merge.

## Suspected Cause / Notes

- The `build-ci-coverage-gate-fidelity` epic corrected the gate arithmetic without wiring the gate into CI.
- Thresholds are fixed by the #563 decision; no number in this item is negotiable. If `scripts/vscode` cannot reach 80 within the item, the item halts and reports rather than lowering the floor or merging the job without its gate.
- `Invoke-MSTestWithCoverage.Helpers.ps1` is near the 500-line ceiling (469 lines at #815); new assertions belong in `Invoke-MSTestWithCoverage.Threshold.ps1` or a new dot-sourced part.
- Blast radius: `.github/workflows/**`, `scripts/vscode/**`, `tests/scripts/vscode/**`. Shares `scripts/vscode/Invoke-MSTestWithCoverage.ps1` with the evidence-projection item; the cohort computation will serialize the two.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: branch-threshold assertion (pass, fail, missing counters), `Invoke-VSBuild` seam, new tests on the least-covered `scripts/vscode` files. Pester, no temporary files.
- [x] Integration scenario to retest: a PR that removes one test must go red on the new gate; the run on this item's own PR must go green.
- [x] Manual verification notes: `actionlint` passes; both new jobs appear as check runs on the PR.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Closes #561 and #562 on merge.
