# vscode-test-runner-parity (Issue #188)

- Date captured: 2026-06-12
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/vscode-test-runner-parity/ (Issue #188)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #188
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/188
- Last Updated: 2026-06-12
- Work Mode: minor-audit

## Summary

The VS Code MSTest tasks do not apply the solution's `TaskMaster.runsettings`, so they run tests with different MSTest parallelization than Visual Studio. The two environments therefore produce divergent test results for the same code.

## Environment

- OS/version: Windows, Visual Studio 2022 / VS Code with bundled vstest.console.exe
- Python version: n/a (PowerShell task runners)
- Command/flags used: VS Code tasks `test: MSTest (vstest.console)` and `test: MSTest with Coverage (Koverage)`, which call `scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- Data source or fixture: `TaskMaster.runsettings`, `coverage.config`, `UtilitiesCS.Test/Properties/AssemblyInfo.cs`

## Steps to Reproduce

1. Run the VS Code task `test: MSTest with Coverage (Koverage)` and observe that only `UtilitiesCS.Test` reports class-level parallelization.
2. Run the same assemblies in Visual Studio, which auto-detects `TaskMaster.runsettings` and reports class-level parallelization for all assemblies (Workers resolved to the logical processor count).
3. Compare results; the parallelization configuration differs between the two environments.

## Expected Behavior

The VS Code test tasks apply the same `TaskMaster.runsettings` that Visual Studio auto-detects, so both environments run with identical MSTest parallelization configuration.

## Actual Behavior

`scripts/vscode/Invoke-MSTest.ps1` invokes `vstest.console.exe` with the test assemblies but no `/Settings:` argument. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` passes `--settings coverage.config` to `dotnet-coverage` (an instrumentation-exclude file), not a vstest `/Settings:` runsettings. As a result no `<MSTest><Parallelize>` configuration from `TaskMaster.runsettings` is applied under VS Code. Only `UtilitiesCS.Test` parallelizes, because its parallelization is compiled in via `[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]`.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:
  - Visual Studio: `Test Parallelization enabled for ...\UtilitiesCS.Test.dll (Workers: 24, Scope: ClassLevel)` while other assemblies show no such line.
  - VS Code coverage task: `vstest.console.exe <assemblies> /InIsolation` with no `/Settings:`.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Test outcomes diverge between local VS Code and Visual Studio (and CI), undermining reproducibility of the test gate.

## Suspected Cause / Notes

- `scripts/vscode/Invoke-MSTest.ps1` line ~52: `& $vstestPath $testAssemblies /InIsolation` — no `/Settings:`.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line ~73: dotnet-coverage `--settings $coverageConfig` is coverage.config, and the inner vstest receives no runsettings.
- `TaskMaster.runsettings` already declares `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>`, matching what Visual Studio uses; no content change to the runsettings is required.
- Separate, deferred defect: Tesseract OCR tests in `ImageStripper_Tests` (and OCR paths reached from `EmailTokenizer`/`MailItemHelper` tests) load a real `eng.traineddata` file from `%LOCALAPPDATA%\TaskMaster\tessdata`, an external-file dependency that fails when the file is absent. That is tracked separately and is out of scope here.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: pass `/Settings:<repoRoot>/TaskMaster.runsettings` to `vstest.console.exe` in both task runner scripts; add a PowerShell wrapper seam for the vstest invocation so a Pester test can assert the `/Settings:` argument is present and points at the repo-root runsettings.
- [x] Integration scenario to retest: run both VS Code test tasks and confirm all assemblies report class-level parallelization consistent with Visual Studio.
- [x] Manual verification notes: configuration parity only; the deferred OCR test-isolation defect will be addressed in a follow-up change.

## Acceptance Criteria

> Revision note (2026-06-12): The `/Settings:` target in AC1–AC3 was changed from the repo-root `TaskMaster.runsettings` to a dedicated off-root CLI runsettings file (parallelization-only) as part of issue #189 Option A. This was required because adding the Visual Studio Code Coverage exclusion block to `TaskMaster.runsettings` force-activates coverage at the CLI; the VS Code tasks must therefore consume a runsettings without that collector. AC1–AC3 below are re-opened and are re-satisfied through the combined #189 implementation; the parallelization-parity intent is unchanged.

- [ ] AC1: `scripts/vscode/Invoke-MSTest.ps1` passes `/Settings:<CLI-runsettings>` (the off-root parallelization-only file, per #189 AC1) to `vstest.console.exe` when running the test assemblies.
- [ ] AC2: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` passes `/Settings:<CLI-runsettings>` to the inner `vstest.console.exe` invocation. The existing `dotnet-coverage --settings coverage.config` (instrumentation excludes) remains unchanged and distinct from the vstest runsettings.
- [ ] AC3: The runsettings path is resolved deterministically and each script fails fast with a clear, specific error if the CLI runsettings file is absent.
- [x] AC4: A wrapper-function seam (per the repository PowerShell wrapper-seam pattern, e.g. `Invoke-VsTestExe -VsTestArgs <string[]>`; parameter name is not `Args`) is introduced so the vstest argument list is unit-testable without launching the external executable.
- [x] AC5: Pester tests assert that the constructed argument list for both scripts includes `/Settings:` pointing at the repo-root `TaskMaster.runsettings`. Tests mock the wrapper seam (never the real `vstest.console.exe`/`dotnet-coverage`), are deterministic, and produce identical results in the terminal and the VS Code Test Explorer.
- [x] AC6: `TaskMaster.runsettings` content is preserved; if edited at all, it must retain `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>`, which mirrors the configuration Visual Studio auto-detects.
- [x] AC7: PowerShell toolchain passes in order — PoshQC format -> PSScriptAnalyzer -> Pester — with no new analyzer debt and no coverage regression on changed lines.

### Out of scope (explicitly deferred)

- The Tesseract OCR external-file test-isolation defect (the 18 failures caused by loading a real `eng.traineddata` from `%LOCALAPPDATA%\TaskMaster\tessdata`) is NOT addressed by this change and is tracked separately. This change aligns runner *configuration* only; it does not by itself drive the suite to zero failures. After this change, the VS Code tasks will apply class-level parallelization to all assemblies and will therefore surface the same OCR failures Visual Studio shows — that convergence is the intended parity outcome.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch