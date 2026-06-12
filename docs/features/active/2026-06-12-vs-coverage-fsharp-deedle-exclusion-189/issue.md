# vs-coverage-fsharp-deedle-exclusion (Issue #189)

- Date captured: 2026-06-12
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/vs-coverage-fsharp-deedle-exclusion/ (Issue #189)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #189
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/189
- Last Updated: 2026-06-12
- Work Mode: minor-audit

## Summary

When Visual Studio runs "Analyze Code Coverage," 17 Deedle/FSharp tests fail with `System.Security.VerificationException: Operation could destabilize the runtime`. The Microsoft Code Coverage collector instruments `FSharp.Core` (and `Deedle`), producing unverifiable IL. The VS Code Koverage task does not hit this because it excludes those modules via `coverage.config`; Visual Studio's auto-detected runsettings (`TaskMaster.runsettings`) has no equivalent exclusion.

## Environment

- OS/version: Windows, Visual Studio 2022 (Microsoft Code Coverage data collector, `datacollector://microsoft/CodeCoverage/2.0`)
- Python version: n/a (.NET / C# / F# interop)
- Command/flags used: Visual Studio "Analyze Code Coverage for All Tests" (auto-detects `TaskMaster.runsettings`)
- Data source or fixture: `TaskMaster.runsettings`, `coverage.config`, Deedle/FSharp.Core (UtilitiesCS reference)

## Steps to Reproduce

1. In Visual Studio, run "Analyze Code Coverage for All Tests" against `UtilitiesCS.Test`.
2. Observe 17 Deedle tests fail (`DfDeedle_COM_Tests`, `DfDeedle_Tests`, `DeedleTests.DeedleDoodles`).
3. Run the same tests without coverage (or via the VS Code Koverage task, which applies `coverage.config` exclusions) and observe they pass.

## Expected Behavior

Visual Studio's coverage run excludes the same third-party/F# modules that `coverage.config` excludes, so Deedle/FSharp.Core are not instrumented and the 17 tests pass, matching the VS Code Koverage task.

## Actual Behavior

The MS Code Coverage collector instruments `FSharp.Core`; the rewritten IL fails CLR verification at runtime, throwing `System.Security.VerificationException: Operation could destabilize the runtime` inside `SeqModule.ToArray` / `ArrayModule.OfSeq`, reached via `DfDeedle.FromArray2D` / `FromDefaultFolder` -> `Frame.FromRows`/`FromColumns`.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:
  - `System.Security.VerificationException: Operation could destabilize the runtime.`
  - `SeqModule.ToArray` -> `ArrayModule.OfSeq` -> `Series.ctor` -> `Frame.FromRows/FromColumns` -> `DfDeedle.FromArray2D/FromDefaultFolder`.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Visual Studio coverage runs report 17 false failures; results diverge from the VS Code task and from non-coverage runs.

## Suspected Cause / Notes

- Confirmed: failure is FSharp.Core instrumentation under coverage, not a defect in F#/Deedle (tests pass uninstrumented).
- `coverage.config` already excludes `.*FSharp.*`, `.*Deedle.*`, `.*Castle\.Core.*`, `.*FluentAssertions.*`, `.*Moq.*`, `.*Microsoft\.Testing.*`, `.*MSTest.*` from dotnet-coverage instrumentation; Visual Studio never reads `coverage.config`.
- No `.runsettings` in the repo contains a `<DataCollectors>` Code Coverage `ModulePaths` exclusion block.

## Proposed Fix / Validation Ideas

- [x] Add a `<DataCollectionRunSettings><DataCollectors><DataCollector friendlyName="Code Coverage"><Configuration><CodeCoverage><ModulePaths><Exclude>` block to `TaskMaster.runsettings`, mirroring the `coverage.config` exclusion list, so Visual Studio's coverage run skips FSharp.Core/Deedle and other third-party test libraries.
- [x] Keep coverage opt-in: the collector block must not force coverage on normal "Run Tests" runs, and must not double-instrument under the VS Code Koverage task (which uses dotnet-coverage with coverage.config and does not pass `--collect` to the inner vstest).
- [x] Verification: reproduce VS behavior from the CLI with `vstest.console.exe <UtilitiesCS.Test.dll> /collect:"Code Coverage" /Settings:TaskMaster.runsettings` — without the exclusion the Deedle tests fail with VerificationException; with the exclusion they pass. Confirm a normal run (no `/collect`) is unaffected and produces no coverage attachment.
- [x] Final acceptance: user confirms in Visual Studio "Analyze Code Coverage" that the 17 Deedle tests pass.

## Acceptance Criteria (Option A — split CLI / IDE runsettings)

This change consolidates the Option A design and therefore also revises the `/Settings:` target of the #188 task-runner scripts (both changes are still uncommitted and ship together).

Background facts established empirically (see `evidence/other/scope-change-finding.2026-06-12T19-45.md`):
- Standalone `vstest.console` (dynamic coverage) does NOT reproduce the Visual Studio static-coverage (`CodeCoverage/2.0`) `VerificationException`, so the exclusion's effect cannot be CLI-verified; VS confirmation is authoritative (AC8).
- At the CLI, the mere presence of a `<DataCollector friendlyName="Code Coverage">` block force-activates coverage, so it must NOT live in the runsettings the VS Code CLI tasks pass to vstest.

- [ ] AC1: A new CLI runsettings file is created **off the repository root** (e.g., `scripts/vscode/TaskMaster.cli.runsettings`) containing exactly the `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>` block and **no** `<DataCollectors>` block; valid `<RunSettings>` XML. It is placed off-root so it does not interfere with Visual Studio's auto-detection of the root `TaskMaster.runsettings`.
- [ ] AC2: `TaskMaster.runsettings` (repo root, VS auto-detected) gains a Microsoft Code Coverage `<DataCollectionRunSettings><DataCollectors><DataCollector friendlyName="Code Coverage"><Configuration><CodeCoverage><ModulePaths><Exclude>` block mirroring the full `coverage.config` exclusion list (`.*Deedle.*`, `.*FSharp.*`, `.*Castle\.Core.*`, `.*FluentAssertions.*`, `.*Moq.*`, `.*Microsoft\.Testing.*`, `.*MSTest.*`), while preserving its existing `<MSTest><Parallelize>` block; valid `<RunSettings>` XML. No `enabled="true"`.
- [ ] AC3: `scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` pass `/Settings:` pointing at the **CLI runsettings** (AC1 file), not at `TaskMaster.runsettings`. The deterministic resolution and fail-fast missing-file guard target the CLI runsettings path. (Revises #188 AC1/AC2 target.)
- [ ] AC4: The Pester tests (`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`) assert that both scripts pass `/Settings:` pointing at the CLI runsettings file, plus the missing-file throw, mocking only the wrapper seams (never the real executables). Deterministic; identical in terminal and Test Explorer.
- [ ] AC5: CLI no-regression demonstrated and captured: with the scripts repointed to the CLI runsettings, a plain `vstest.console` run of the Deedle tests passes and produces **no** code-coverage attachment (the CLI never sees the coverage collector), and the Koverage inner vstest still omits `/collect` (no double collection with `dotnet-coverage`).
- [ ] AC6: CLI parallelization parity preserved: the CLI runsettings retains `Workers=0`/`ClassLevel`, so VS Code CLI runs parallelize identically to the #188 intent. Captured by inspection/diff.
- [ ] AC7: PowerShell toolchain passes in order — PoshQC format -> PSScriptAnalyzer -> Pester — with no net-new analyzer debt and no coverage regression on changed lines.
- [ ] AC8 (user action, pending): User confirms in Visual Studio that (a) "Run Tests" runs the listed Deedle tests green with no coverage collected, and (b) "Analyze Code Coverage" runs them green with no `VerificationException` because the root `TaskMaster.runsettings` exclusions apply. This is the authoritative acceptance for the exclusion's effect, since the CLI cannot reproduce the VS static-coverage failure.

### Out of scope (explicitly deferred)

- The timing-test determinism defect `TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldReturnResult` (the single failure common to both environments, unrelated to instrumentation) is NOT addressed here and is tracked separately.
- OCR/Tesseract tests (not among the failing set) are not addressed.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch