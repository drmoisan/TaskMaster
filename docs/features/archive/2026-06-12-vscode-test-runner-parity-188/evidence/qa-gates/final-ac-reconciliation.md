# Phase 2 — Acceptance Criteria Reconciliation

Timestamp: 2026-06-12T18-42

AC Source: docs/features/active/2026-06-12-vscode-test-runner-parity-188/issue.md (## Acceptance Criteria)

| AC | Implementing task(s) | Evidence | Status |
|---|---|---|---|
| AC1 | P1-T1, P1-T3 | `Invoke-MSTest.ps1`: `Get-VsTestArgumentList` builds `/Settings:<repo-root>\TaskMaster.runsettings`; passed via `Invoke-VsTestExe`. Tests "includes /Settings:" + "preserves ... /InIsolation" pass (final-pester.md). | PASS |
| AC2 | P1-T4, P1-T5 | `Invoke-MSTestWithCoverage.ps1`: `Get-DotnetCoverageArgumentList` adds inner `/Settings:` after `-- $vstestPath`, distinct from outer `--settings coverage.config`. Tests "includes inner vstest /Settings:" + "preserves distinct outer --settings coverage.config" pass. | PASS |
| AC3 | P1-T2, P1-T5 | `Resolve-RunSettingsPath -RepoRoot` resolves `Join-Path $repoRoot 'TaskMaster.runsettings'` and throws `"Runsettings file not found: <path>"` when absent. Negative test "fails fast with a specific error naming the missing path" passes. | PASS |
| AC4 | P1-T1, P1-T4 | Wrapper seams `Invoke-VsTestExe -VsTestArgs` and `Invoke-DotnetCoverageExe -DotnetCoverageArgs` (parameter names are NOT `Args`). Argument construction is unit-testable without launching the executables. | PASS |
| AC5 | P1-T7 | `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`: 9/9 pass; asserts `/Settings:` for both scripts; mocks only the wrapper seams (mock signatures match production: `param([string]$VsTestPath,[string[]]$VsTestArgs)` and `param([string[]]$DotnetCoverageArgs)`); never mocks the real `vstest.console.exe`/`dotnet-coverage`; deterministic, `$PSScriptRoot`-relative, no PATH/CWD assumptions. | PASS |
| AC6 | P1-T6 | `git diff -- TaskMaster.runsettings` produced no output; file still contains `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>`. | PASS |
| AC7 | P0-T4..T6, P2-T1..T5 | PoshQC format idempotent (final-poshqc-format.md, EXIT_CODE 0); PSScriptAnalyzer no new debt — folder count returned to baseline 16, changed files contribute only 2 pre-existing `PSAvoidUsingWriteHost` warnings, test dir 0->0 (final-poshqc-analyze.md); Pester new tests 9/9 pass; no coverage regression on changed lines, 100% of policy-testable new lines covered (final-coverage-comparison.md). | PASS |

## Out-of-scope confirmation

- Tesseract/OCR external-file test-isolation defect (18 failures from real
  `eng.traineddata`): NOT addressed; no `ImageStripper`/`EmailTokenizer`/
  `MailItemHelper` file touched. Confirmed unchanged.
- `TaskMaster.runsettings`: content unchanged (AC6).
- `.vscode/tasks.json`: not modified (the scripts retain their original
  parameter surface; task wiring unchanged).
- Pre-existing `Install-RepoDotNetSdk.Tests.ps1` SDK-version failure is unrelated
  and out of scope; no in-scope file touches it.

All AC1–AC7 mapped to completed, evidence-backed tasks. Out-of-scope items
confirmed unchanged.
