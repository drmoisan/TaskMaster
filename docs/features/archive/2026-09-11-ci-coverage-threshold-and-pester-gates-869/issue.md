# ci-coverage-threshold-and-pester-gates (Issue #869)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/ (Issue #869)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #869
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/869
- Last Updated: 2026-09-12
- Work Mode: full-bug

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
- [x] Move to active fix folder / branch

Closes #561 and #562 on merge.

## Delivered Outcome

- The C# gate now runs `scripts/vscode/Invoke-MSTestWithCoverage.ps1` inside `_mstest-coverage.yml`, so the Cobertura document exists in CI, and asserts 80% line through the existing `Assert-CoberturaLineCoverageThreshold` and 75% branch through the new `Assert-CoberturaBranchCoverageThreshold`. The upload step now publishes `coverage/coverage.cobertura.xml` with `if-no-files-found: error`.
- A new reusable callee `.github/workflows/_pester.yml` runs Pester over `tests/scripts/vscode` with JaCoCo coverage scoped to `scripts/vscode` and asserts the `LINE` figure at 80%. `ci.yml` calls it under the job key `pester`.
- `Invoke-VSBuild.ps1` and `Invoke-Restore.ps1` now carry invocation guards with their bodies extracted into `Invoke-VSBuildMain` and `Invoke-RestoreMain` behind named wrapper seams, so a test run no longer invokes `vswhere.exe` or executes `Sync-PackageReferences.ps1` against the real repository root.
- `scripts/vscode` LINE coverage moved from a measured 78.90% to 83.93%, above the 80 floor.
- The coverage of `Sync-PackageReferences.ps1` is now identical on two consecutive clean runs, which is the determinism regression the item reports as non-deterministic at 53/84 versus 71/84.

### Superseded: the new required check-run context count

This item's Expected Behavior section anticipates **two** new check-run contexts. That figure is **superseded**: the delivery adds exactly **one**.

The reason is that the C# threshold assertion was placed inside the existing `_mstest-coverage.yml` callee rather than in a callee of its own. A check-run context name takes the form `<caller job id> / <callee job name>`, so an assertion added inside an existing callee changes no name: the existing context `mstest-coverage / Run MSTest suite with coverage` continues to report under its current name and needs no ruleset edit. A separate callee would have produced a second context, but the pipeline has zero `needs:` edges and the callees share no artifacts, so it would have duplicated the full restore, build and instrumented test run for no additional signal. That route was rejected as settled decision D1 of the specification.

The single new context is predicted as `pester / Run Pester suite with coverage`. It is **predicted, not confirmed**: the workflow README forbids hand-writing these strings and requires capturing them from a live run against the pull request head SHA. No run exists yet, so the capture is recorded as `Status: PENDING LIVE RUN` in `evidence/other/check-run-contexts.2026-09-12T10-25.md` together with the exact command to run once the run completes. The maintainer's ruleset edit should use the captured value verbatim, and should add that one context only.
