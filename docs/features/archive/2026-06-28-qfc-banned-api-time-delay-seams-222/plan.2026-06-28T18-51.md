# qfc-banned-api-time-delay-seams — Refactor Plan

- **Issue:** #222
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-28T18-51
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug

## Required References (read, do not restate)

- Acceptance criteria source (full-bug → `spec.md` only): `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222/spec.md` (`## Acceptance Criteria`)
- Issue context: `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222/issue.md`
- Research (design + exact call-site table + constructor evidence): `artifacts/research/2026-06-28-qfc-time-delay-seam-research.md`
- Banned API policy: `BannedSymbols.txt` (repo root); `.claude/rules/csharp.md` lines 55-63 and 65-79
- Policy reading order: `CLAUDE.md` → `.claude/rules/general-code-change.md` → `.claude/rules/general-unit-test.md` → `.claude/rules/csharp.md`

## Objective

Remediate eight pre-existing active banned-API usages in the Quick Filer controllers
(`DateTime.Now` x5, `Task.Delay` x3) by routing them through an injectable
`System.TimeProvider` seam so the affected code becomes deterministic and unit-testable.
Behavior must be preserved exactly: delay durations 5/200/20 ms unchanged; timestamp
format strings (`mm:ss.fff`, `MM/dd/yyyy`, `hh:mm`) and semantics unchanged. Public
`IQfcDatamodel` and `IQfcHomeController` surfaces must not change.

## Strategy

Single seam type: `System.TimeProvider` (backported via `Microsoft.Bcl.TimeProvider`,
declared by UtilitiesCS but not yet by QuickFiler). The package binaries are obtained via
NuGet restore into `packages\` (a restore artifact, not committed to the worktree), not
assumed pre-present on disk. Injection follows the issue
#218 internal-property-with-default convention:
`internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;` on
`QfcDatamodel` and `QfcHomeController`, plus an optional `TimeProvider timeProvider = null`
parameter on the static `LaunchAsync` factory. Current time via
`TimeProvider.GetLocalNow().LocalDateTime`; async delay via
`TimeProvider.Delay(TimeSpan.FromMilliseconds(n), token)`. Tests use `Mock<TimeProvider>`
for timestamp sites and `FakeTimeProvider` (Microsoft.Extensions.TimeProvider.Testing)
for delay sites.

Fail-closed evidence rule: this plan includes explicit baseline artifact tasks, final-QA
artifact tasks, and a coverage-comparison task. If any required baseline artifact, QA
artifact, or coverage-comparison artifact is missing or contains placeholder values, the
verdict must be BLOCKED or INCOMPLETE, never PASS.

Evidence accounting rule: every evidence-producing task records its canonical artifact
path. Do not mark evidence-backed work complete without the artifact on disk.

## Verified Pre-conditions (confirmed at plan authoring; re-confirm in Phase 0)

- Eight active sites confirmed at the issue/research line numbers on current HEAD.
- `QfcHomeController.cs` = 454 lines (limited headroom; the seam property is placed in the
  smaller `QfcHomeController.Metrics.cs` partial, not in `QfcHomeController.cs`).
- `QuickFiler/QuickFiler.csproj` and `QuickFiler/packages.config` do NOT reference
  `Microsoft.Bcl.TimeProvider`. Legacy `ProjectReference` to UtilitiesCS does not flow the
  assembly `<Reference>` transitively, so QuickFiler needs its own explicit reference.
- `Microsoft.Bcl.TimeProvider` 10.0.7 is declared by UtilitiesCS and obtained via NuGet
  restore; it is not assumed present on disk. `packages\` does not exist in this worktree
  until `nuget restore` (or `msbuild /t:Restore`) runs — legacy `packages.config` projects
  do NOT auto-restore on `msbuild /t:Build`.
- `Microsoft.Extensions.TimeProvider.Testing` 9.0.0 is not yet declared by any project and
  must be added (to `QuickFiler.Test`) then restored before its types resolve.
- `[assembly: InternalsVisibleTo("QuickFiler.Test")]` is present; internal seams are
  test-visible. `QfcDatamodel` carries `[ExcludeFromCodeCoverage]` (its delay-site tests
  are for correctness, not coverage); `QfcHomeController` is NOT exempt.

## Evidence Locations (canonical — non-overridable)

- Baseline: `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222/evidence/baseline/`
- QA gates: `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222/evidence/qa-gates/`
- Regression testing: `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222/evidence/regression-testing/`

---

## Work Breakdown

### Phase 0 — Policy Read and Baseline Capture

- [x] [P0-T1] Run `nuget restore TaskMaster.sln` (or `msbuild TaskMaster.sln /t:Restore`) to populate `packages\` for the legacy `packages.config` projects, and capture to `evidence/baseline/restore.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: EXIT_CODE 0 AND `packages\Microsoft.Bcl.TimeProvider.10.0.7\lib\net462\Microsoft.Bcl.TimeProvider.dll` exists. If restore cannot obtain the package from the configured feed, record `DEPENDENCY-BLOCKED` with the missing package id/version and halt.
- [x] [P0-T2] Read policy files in required order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `BannedSymbols.txt`) and write `evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read. Acceptance: artifact exists with all three fields populated.
- [x] [P0-T3] Re-confirm the eight banned-API call sites and their current line numbers in `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, and `QuickFiler/Controllers/QfcHomeController.Metrics.cs`; write `evidence/baseline/site-reconfirmation.md` with each site's file, line, and exact source text. Acceptance: all eight sites located; any line drift recorded. One binary outcome: artifact lists eight confirmed sites.
- [x] [P0-T4] Confirm dependency-resolution state: grep `QuickFiler/QuickFiler.csproj`, `QuickFiler/packages.config`, `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler.Test/packages.config` for `Microsoft.Bcl.TimeProvider` and `Microsoft.Extensions.TimeProvider.Testing`, and record the on-disk present/absent state of `packages\Microsoft.Bcl.TimeProvider.10.0.7\lib\net462\Microsoft.Bcl.TimeProvider.dll` and `packages\Microsoft.Extensions.TimeProvider.Testing.9.0.0\lib\net462\Microsoft.Extensions.TimeProvider.Testing.dll`. Write `evidence/baseline/dependency-state.md` with `Timestamp:`, each grep result, and each DLL present/absent result. Acceptance: artifact records present/absent for all four config files and both DLLs. The `Microsoft.Extensions.TimeProvider.Testing` package is not yet declared by any project, so its absence pre-restore is expected and MUST be recorded as `RESTORE-REQUIRED` (it is wired and restored in Phase 1) — NOT `DEPENDENCY-BLOCKED`. Reserve `DEPENDENCY-BLOCKED` for confirmed feed-unavailability after the Phase 1 restore (P1-T5).
- [x] [P0-T5] Capture baseline file line counts for every file to be touched (`QfcHomeController.cs`, `QfcHomeController.Metrics.cs`, `QfcDatamodel.cs`, `QfcDatamodel.FrameBuilding.cs`, `QfcDatamodel.QueueProcessing.cs`, and the two test files) into `evidence/baseline/file-line-counts.md` with `Timestamp:` and one line-count per file. Acceptance: artifact lists a numeric line count for each listed file.
- [x] [P0-T6] Run `csharpier .` and capture result to `evidence/baseline/baseline-format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact present with all four fields.
- [x] [P0-T7] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and capture to `evidence/baseline/baseline-analyzer.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (include any RS0030 occurrences for the eight sites). Acceptance: artifact present with all four fields.
- [x] [P0-T8] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and capture to `evidence/baseline/baseline-nullable.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact present with all four fields.
- [x] [P0-T9] Run `vstest.console.exe` against the QuickFiler.Test assembly with `/EnableCodeCoverage` and capture to `evidence/baseline/baseline-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including numeric baseline values: repo-wide line coverage percent and QuickFiler-assembly (or QfcHomeController) coverage percent. Acceptance: artifact present with numeric coverage headline (not a placeholder).

### Phase 1 — Dependency Wiring

- [x] [P1-T1] Add `<package id="Microsoft.Bcl.TimeProvider" version="10.0.7" targetFramework="net481" />` to `QuickFiler/packages.config`. Acceptance: package entry present; file remains valid XML.
- [x] [P1-T2] Add to `QuickFiler/QuickFiler.csproj` the reference `<Reference Include="Microsoft.Bcl.TimeProvider, Version=10.0.0.7, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51, processorArchitecture=MSIL"><HintPath>..\packages\Microsoft.Bcl.TimeProvider.10.0.7\lib\net462\Microsoft.Bcl.TimeProvider.dll</HintPath></Reference>` (matching the existing UtilitiesCS.csproj reference form). Acceptance: reference present with the exact HintPath; project XML valid.
- [x] [P1-T3] Add `<package id="Microsoft.Bcl.TimeProvider" version="10.0.7" targetFramework="net481" />` and `<package id="Microsoft.Extensions.TimeProvider.Testing" version="9.0.0" targetFramework="net481" />` to `QuickFiler.Test/packages.config`. Acceptance: both package entries present; file valid XML.
- [x] [P1-T4] Add to `QuickFiler.Test/QuickFiler.Test.csproj` `<Reference>` items (with `<HintPath>` to `..\packages\Microsoft.Bcl.TimeProvider.10.0.7\lib\net462\Microsoft.Bcl.TimeProvider.dll` and `..\packages\Microsoft.Extensions.TimeProvider.Testing.9.0.0\lib\net462\Microsoft.Extensions.TimeProvider.Testing.dll`). Acceptance: both references present with exact HintPaths; project XML valid.
- [x] [P1-T5] Run `nuget restore TaskMaster.sln` to fetch the newly declared `Microsoft.Bcl.TimeProvider` 10.0.7 (QuickFiler) and `Microsoft.Extensions.TimeProvider.Testing` 9.0.0 (QuickFiler.Test); capture to `evidence/qa-gates/p1-restore.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: EXIT_CODE 0 AND both `packages\Microsoft.Bcl.TimeProvider.10.0.7\lib\net462\Microsoft.Bcl.TimeProvider.dll` and `packages\Microsoft.Extensions.TimeProvider.Testing.9.0.0\lib\net462\Microsoft.Extensions.TimeProvider.Testing.dll` exist; and no version downgrade/conflict between `Bcl.TimeProvider` 10.0.7 and `TimeProvider.Testing` 9.0.0 (the Testing package's `TimeProvider` dependency constraint must be satisfied by 10.0.7). If a package cannot be obtained from the configured feed, record `DEPENDENCY-BLOCKED` with the missing package id/version and halt.
- [x] [P1-T6] Build the solution (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`) to confirm the `TimeProvider` and `FakeTimeProvider` types resolve in QuickFiler and QuickFiler.Test; capture result to `evidence/qa-gates/p1-dependency-resolution.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: build succeeds (EXIT_CODE 0) with the new references present.

### Phase 2 — Seam Introduction

- [x] [P2-T1] Add `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;` to `QuickFiler/Controllers/QfcDatamodel.cs` (private-variables/fields region of the main partial), and add `using System;` if not already present. Acceptance: property compiles; no call-site changes yet; `QfcDatamodel.cs` remains <= 500 lines.
- [x] [P2-T2] Add `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;` to `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (NOT to `QfcHomeController.cs`, to preserve its 454-line headroom). Add `using System;` to that partial if not already present. Acceptance: property compiles; `QfcHomeController.Metrics.cs` remains <= 500 lines; `QfcHomeController.cs` line count unchanged.
- [x] [P2-T3] Add an optional `TimeProvider timeProvider = null` parameter to the static `LaunchAsync` factory in `QuickFiler/Controllers/QfcHomeController.cs` and set `controller.TimeProvider = timeProvider ?? TimeProvider.System;` immediately after `controller` is constructed. Do not alter `IQfcHomeController` or `IQfcDatamodel`. Acceptance: signature is backward-compatible (optional parameter); existing callers compile unchanged; `QfcHomeController.cs` remains <= 500 lines.
- [x] [P2-T4] Run `csharpier .` then `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; capture to `evidence/qa-gates/p2-seam-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: build succeeds; seam properties present in both classes.

### Phase 3 — Call-site Replacement (eight sites, behavior-preserving)

- [x] [P3-T1] Replace `await Task.Delay(5)` with `await TimeProvider.Delay(TimeSpan.FromMilliseconds(5))` at the `ToggleOfflineMode` site in `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`. Acceptance: no `Task.Delay` remains in this file; 5 ms duration preserved; file compiles.
- [x] [P3-T2] Replace `await Task.Delay(200)` with `await TimeProvider.Delay(TimeSpan.FromMilliseconds(200), token)` at the `WaitForQueue` site in `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, passing the in-scope `token`. Acceptance: no `Task.Delay` remains in this file; 200 ms duration preserved; existing cancellation semantics preserved; file compiles.
- [x] [P3-T3] Replace `DateTime.Now.ToString("mm:ss.fff")` at the `LaunchAsync` catch-block log site in `QuickFiler/Controllers/QfcHomeController.cs` with `controller.TimeProvider.GetLocalNow().LocalDateTime.ToString("mm:ss.fff")`. Acceptance: no active `DateTime.Now` remains in `QfcHomeController.cs`; format string `mm:ss.fff` preserved; file compiles and remains <= 500 lines.
- [x] [P3-T4] In `QuickFiler/Controllers/QfcHomeController.Metrics.cs` `QuickFileMetrics_WRITE`, replace `var now = DateTime.Now` with `var now = TimeProvider.GetLocalNow().LocalDateTime`. Acceptance: this assignment uses the seam; downstream `now` formatting unchanged; file compiles.
- [x] [P3-T5] In `QuickFiler/Controllers/QfcHomeController.Metrics.cs` `WriteMetricsAsync`, introduce `var now = TimeProvider.GetLocalNow().LocalDateTime;` and use it for all three reads: `curDateText = now.ToString("MM/dd/yyyy")`, `curTimeText = now.ToString("hh:mm")`, and `OlEndTime = now` (replacing the three `DateTime.Now` reads at the former lines 100, 102, 114). Acceptance: no active `DateTime.Now` remains in this method; all three format strings/semantics preserved; behavior-preserving consolidation to a single clock read; file compiles.
- [x] [P3-T6] Replace `await Task.Delay(20)` at the `NonBlockingProducer` catch-branch site in `QuickFiler/Controllers/QfcHomeController.Metrics.cs` with `await TimeProvider.Delay(TimeSpan.FromMilliseconds(20))`. Acceptance: no `Task.Delay` remains in this file; 20 ms duration preserved; file compiles.
- [x] [P3-T7] Verify zero remaining active (non-commented) banned-API usages in the four target files via grep for `DateTime.Now` and `Task.Delay`; write `evidence/qa-gates/p3-banned-api-sweep.md` with `Timestamp:`, the grep command, and results showing only commented-out references remain. Acceptance: artifact shows no active banned-API match in the four files; out-of-scope files untouched.
- [x] [P3-T8] Confirm `BannedSymbols.txt`, `.editorconfig`, `.globalconfig`, and `.claude/rules/csharp.md` are unmodified (RS0030 not suppressed/weakened); write `evidence/qa-gates/p3-policy-unchanged.md` with `Timestamp:` and `git status` / diff result for those files. Acceptance: artifact confirms no changes to policy/config files.
- [x] [P3-T9] Run `csharpier .` then the analyzer build then `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`; capture to `evidence/qa-gates/p3-callsite-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: all builds succeed; no new RS0030 warnings for the touched sites.

### Phase 4 — Deterministic Tests (MSTest + Moq + FluentAssertions)

- [x] [P4-T1] Add a timestamp-seam test to `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` for `WriteMetricsAsync` using `Mock<TimeProvider>` with `Setup(x => x.GetLocalNow())` returning a fixed `DateTimeOffset`; mock `IFileSystemFolderPaths.SpecialFolders` to an empty dictionary so the method returns before file I/O; assert `curDateText`/`curTimeText`/`OlEndTime`-derived output reflects the injected clock (`MM/dd/yyyy`, `hh:mm`). Acceptance: test compiles, is deterministic, no live COM, no temp files, and fails if the seam is bypassed.
- [x] [P4-T2] Add a timestamp-seam test to `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` for `QuickFileMetrics_WRITE` covering the `now` read via `Mock<TimeProvider>` with the empty-`SpecialFolders` early-return pattern; assert the formatted `dataLineBeg` segment uses the injected clock. Acceptance: deterministic test that asserts injected-clock usage; AAA structure; FluentAssertions.
- [x] [P4-T3] Add a delay-seam test to `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` for the `NonBlockingProducer` 20 ms delay path using `FakeTimeProvider`; assert the awaited task does not complete until `fake.Advance(TimeSpan.FromMilliseconds(20))` is called. Acceptance: deterministic test proving the injected delay (not wall-clock) is honored; no `Thread.Sleep`/real waits.
- [x] [P4-T4] Add a delay-seam test to `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` for `ToggleOfflineMode` (5 ms) using `FakeTimeProvider`, injecting via the internal `TimeProvider` property; assert the delayed path does not complete until `fake.Advance(TimeSpan.FromMilliseconds(5))`. Acceptance: deterministic test; COM `ExecuteMso` isolated/mocked or the no-delay branch documented; correctness-only (class is `[ExcludeFromCodeCoverage]`).
- [x] [P4-T5] Add a delay-seam test to `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` for `WaitForQueue` (200 ms) using `FakeTimeProvider`; verify the loop awaits the injected delay and that the passed `token` cancellation is honored. Acceptance: deterministic test; loop release driven by `fake.Advance(TimeSpan.FromMilliseconds(200))`; no real waits.
- [x] [P4-T6] (Optional, include if achievable without live COM) Add a `LaunchAsync` catch-block test to `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (or the appropriate existing test class) passing `timeProvider:` and triggering `OperationCanceledException` via an overridden loader; assert the logged timestamp uses the injected clock. Acceptance: if COM-isolation is feasible the test asserts injected-clock log output; otherwise record a documented exclusion note in `evidence/regression-testing/launchasync-test-scope.md` with `Timestamp:` and rationale. One binary outcome: test added OR scope-exclusion dossier written.
- [x] [P4-T7] Confirm each new/changed test file remains <= 500 lines; if a file would exceed the limit, split into a cohesive sibling test class file. Acceptance: line count of each touched test file recorded in `evidence/qa-gates/p4-test-file-line-counts.md` and all <= 500.

### Phase 5 — Final QA Loop and Coverage Verification

- [x] [P5-T1] Run `csharpier .`; if it changes files, restart the loop from this step. Capture to `evidence/qa-gates/final-format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: formatter reports no changes in the final pass (EXIT_CODE 0).
- [x] [P5-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; capture to `evidence/qa-gates/final-analyzer.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: build succeeds with no analyzer errors; no new RS0030 for the eight former sites.
- [x] [P5-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; capture to `evidence/qa-gates/final-nullable.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: build succeeds with no nullable/type warnings-as-errors.
- [x] [P5-T4] Run `vstest.console.exe` against the QuickFiler.Test assembly with `/EnableCodeCoverage`; capture to `evidence/qa-gates/final-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including post-change numeric coverage: repo-wide line coverage percent, QfcHomeController new/changed-code coverage percent, and the new tests' pass count. Acceptance: all tests pass; numeric coverage recorded (no placeholders). If any step P5-T1..P5-T4 changes files or fails, restart the loop from P5-T1.
- [x] [P5-T5] Write `evidence/qa-gates/coverage-comparison.md` with `Timestamp:` reporting baseline coverage (from P0-T9), post-change coverage (from P5-T4), and new/changed-code coverage for `QfcHomeController`. Acceptance: QfcHomeController new/changed code >= 90%; repo-wide line coverage >= 80%; no regression on changed lines; `QfcDatamodel` sites noted as `[ExcludeFromCodeCoverage]` and exempt. If thresholds are not met, mark verdict remediation-required (not PASS).
- [x] [P5-T6] Verify each touched production and test file remains <= 500 lines and write the final counts to `evidence/qa-gates/final-line-counts.md`. Acceptance: every touched file <= 500 lines.
- [x] [P5-T7] Map each `spec.md` `## Acceptance Criteria` item (AC1–AC8) to the satisfying task/evidence artifact in `evidence/qa-gates/ac-traceability.md` with `Timestamp:`. Acceptance: all eight ACs mapped to concrete evidence; any unmet AC flagged.

## Acceptance Criteria Mapping (from spec.md)

- AC1 (all 8 sites replaced) → P3-T1..P3-T7
- AC2 (no new banned-API; RS0030 not suppressed; policy files unchanged) → P3-T7, P3-T8, P5-T2
- AC3 (behavior preserved: 5/200/20 ms; formats) → P3-T1..P3-T6, P4-T1..P4-T5
- AC4 (seams injected via construction paths; public surfaces unchanged) → P2-T1..P2-T3
- AC5 (every touched file <= 500 lines) → P2-T1..T3, P4-T7, P5-T6
- AC6 (focused MSTest+Moq+FluentAssertions tests; no live COM/temp files) → P4-T1..P4-T6
- AC7 (>= 90% new code; no regression; >= 80% floor) → P5-T4, P5-T5
- AC8 (toolchain passes in order) → P5-T1..P5-T4

## Rollback / Contingency

All changes are additive (a seam property, an optional parameter, in-place call-site
swaps, new tests, and project-reference entries). Revert is a single branch reset.

Missing package binaries on disk are NOT a halt condition by themselves: `packages\` is a
NuGet restore artifact, and restore is the remedy. The Phase 0 restore (P0-T1) and the
Phase 1 restore (P1-T5) are the means by which `Microsoft.Bcl.TimeProvider` 10.0.7 and
`Microsoft.Extensions.TimeProvider.Testing` 9.0.0 are obtained. A pre-restore absence of
the not-yet-declared Testing package is recorded by P0-T4 as `RESTORE-REQUIRED`, not
`DEPENDENCY-BLOCKED`. Halt only if a `nuget restore` (P0-T1 or P1-T5) cannot obtain a
required package from the configured feed; in that case record `DEPENDENCY-BLOCKED` with
the missing package id/version and report the feed unavailability. Do not add or change
package sources without approval.

## Open Questions / Notes

- The `LaunchAsync` catch-block test (P4-T6) depends on isolating the cancellation path
  from live COM; if not feasible deterministically, a documented scope-exclusion dossier
  is the acceptable outcome (the production site is still covered by the seam swap).
- `QfcDatamodel` is `[ExcludeFromCodeCoverage]`; its delay-site tests are required for
  correctness per the unit-test policy but do not contribute to the coverage denominator.
