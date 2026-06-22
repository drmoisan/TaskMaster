# Plan — Issue #211 Phase 1: Continuation-Latency Attribution Probe

- Issue: #211
- Feature folder: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/`
- Work Mode: full-bug
- Scope: Phase 1 only (AC1-AC4). AC5 is a maintainer runtime capture (not CI-automatable). AC6 (Phase 2) is evidence-gated and NOT in this plan.
- Language: C# / net48 Outlook VSTO add-in (TaskMaster)
- Requirements source: `spec.md` (AC1-AC6); research: `artifacts/research/2026-06-22-intelconfig-continuation-stall-211-research.md`

## Scope-Lock (files this plan may modify)

- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (replace yield method; update five call sites)
- `TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs` (NEW test file)
- `TaskMaster.Test/TaskMaster.Test.csproj` (single `<Compile Include>` for the new test file; legacy non-SDK `packages.config` project uses explicit includes, no glob)

Out of scope: Phase 2 changes (`ConfigureAwait(false)` / `UiThread.UiSyncContext` re-marshal), any COM-bound phase body, `IntelligenceConfig`, Teams, `LoadIntelConfigAsync` body.

## Confirmed planning facts (verified by source inspection)

- `ApplicationGlobals.cs` is 248 lines. The change adds approximately 25 lines (well under the 500-line cap).
- `YieldBetweenStartupPhasesAsync()` is `protected internal virtual` at `ApplicationGlobals.cs:172-175`; body is `await Task.Yield();`.
- The five inter-phase call sites are at `ApplicationGlobals.cs:140,143,146,149,152`. Preceding-phase names in order: `IntelConfig`, `OlObjects`, `ToDo`, `AutoFile`, `Engines`.
- All phase methods are `protected internal virtual` and proven overridable to no-ops by `ApplicationGlobalsStartupTimingTests.cs` (existing `TestableApplicationGlobals`), which already drives `LoadSequentialAsync` deterministically with no live COM and asserts yield count == 5. The unit-test seam is feasible.
- `ApplicationIdleTimer.IsIdle` (bool), `CurrentCPUUsage` (double), `CurrentGUIActivity` (double) are public static properties in `UtilitiesCS.Threading`, already imported at `ApplicationGlobals.cs:12`. They are reachable directly with no new accessor. No fallback or limitation is required for signal reachability.
- The `logger` field exists at `ApplicationGlobals.cs:18-20` (log4net `ILog`); the probe emits via `logger.Debug(...)`.
- `TaskMaster.Test.csproj` uses explicit `<Compile Include>` items (`packages.config`, no glob); the new test file requires explicit wiring (insert near line 263 alongside the existing AppGlobals test includes).

## Evidence location invariant

All evidence for this plan is written under the canonical feature evidence root:
`docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/<kind>/`. Any non-canonical evidence path (e.g., `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`) is rejected and replaced with the canonical path.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in required order and record the read evidence to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/baseline/phase0-instructions-read.md`. Acceptance: artifact contains `Timestamp:`, `Policy Order:`, and the explicit list of files read: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
- [x] [P0-T2] Record the current line count of `TaskMaster/AppGlobals/ApplicationGlobals.cs` to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/baseline/file-size-applicationglobals.md`. Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` stating the baseline line count (expected 248) and confirming headroom under the 500-line cap.
- [x] [P0-T3] Run CSharpier format-check baseline (`dotnet tool run csharpier --check .`) and record to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/baseline/baseline-csharpier.md`. Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail status.
- [x] [P0-T4] Run the analyzer build baseline (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) and record to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/baseline/baseline-analyzers.md`. Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with build result and warning/error counts.
- [x] [P0-T5] Run the nullable/TWAE build baseline (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`) and record to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/baseline/baseline-nullable-twae.md`. Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with build result.
- [x] [P0-T6] Run the MSTest coverage baseline (`vstest.console.exe <TaskMaster.Test assembly> /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`) and record to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/baseline/baseline-mstest-coverage.md`. Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed test counts and the numeric repository-wide line coverage headline percentage (baseline percent).

---

### Phase 1 — Continuation-Resume Probe Implementation (AC1, AC2)

- [x] [P1-T1] In `TaskMaster/AppGlobals/ApplicationGlobals.cs`, replace the `YieldBetweenStartupPhasesAsync()` method (lines 172-175) with `protected internal virtual async Task YieldWithContinuationProbeAsync(string priorPhaseName)`. Body: `var sw = Stopwatch.StartNew(); await Task.Yield(); sw.Stop();` then emit exactly one `logger.Debug(...)` line with the literal tag `[continuation-resume]` and fields `priorPhase={priorPhaseName}`, `waitMs={sw.Elapsed.TotalMilliseconds:F1}`, `resumeThreadId={System.Threading.Thread.CurrentThread.ManagedThreadId}`, `resumeSyncContext={System.Threading.SynchronizationContext.Current?.GetType().FullName ?? "null"}`, `staIsIdle={UtilitiesCS.Threading.ApplicationIdleTimer.IsIdle}`, `staCpuUsage={UtilitiesCS.Threading.ApplicationIdleTimer.CurrentCPUUsage:F3}`, `staGuiActivity={UtilitiesCS.Threading.ApplicationIdleTimer.CurrentGUIActivity:F1}`. Acceptance: method signature is `protected internal virtual async Task YieldWithContinuationProbeAsync(string priorPhaseName)`; uses `Stopwatch` only (no `DateTime.Now`/`UtcNow`/`Random.Shared`/`Thread.Sleep`/`Task.Delay`); emits exactly one `[continuation-resume]` line with all seven fields; `YieldBetweenStartupPhasesAsync` no longer exists. (AC1, AC2)
- [x] [P1-T2] In `TaskMaster/AppGlobals/ApplicationGlobals.cs` `LoadSequentialAsync`, replace the call at line 140 `await YieldBetweenStartupPhasesAsync();` with `await YieldWithContinuationProbeAsync("IntelConfig");`. Acceptance: the post-IntelConfig boundary calls the probe with `"IntelConfig"`; phase order/count unchanged. (AC1, AC2)
- [x] [P1-T3] In `LoadSequentialAsync`, replace the call at line 143 with `await YieldWithContinuationProbeAsync("OlObjects");`. Acceptance: the post-OlObjects boundary calls the probe with `"OlObjects"`. (AC1, AC2)
- [x] [P1-T4] In `LoadSequentialAsync`, replace the call at line 146 with `await YieldWithContinuationProbeAsync("ToDo");`. Acceptance: the post-ToDo boundary calls the probe with `"ToDo"`. (AC1, AC2)
- [x] [P1-T5] In `LoadSequentialAsync`, replace the call at line 149 with `await YieldWithContinuationProbeAsync("AutoFile");`. Acceptance: the post-AutoFile boundary calls the probe with `"AutoFile"`. (AC1, AC2)
- [x] [P1-T6] In `LoadSequentialAsync`, replace the call at line 152 with `await YieldWithContinuationProbeAsync("Engines");`. Acceptance: the post-Engines boundary calls the probe with `"Engines"`; exactly five probe call sites remain in `LoadSequentialAsync`, preserving the original yield order and count (5). (AC1, AC2)
- [x] [P1-T7] Update the existing `TestableApplicationGlobals.YieldBetweenStartupPhasesAsync` override in `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs` (lines 268-272) to override `YieldWithContinuationProbeAsync(string priorPhaseName)` instead, preserving the `YieldCount++` increment and calling `await base.YieldWithContinuationProbeAsync(priorPhaseName)`. Acceptance: the existing startup-timing tests compile against the renamed seam and the yield-count assertion (== 5) is preserved; no other behavior in that file changes. (AC2)

---

### Phase 2 — Deterministic Probe-Sequence Test (AC3)

- [x] [P2-T1] Create `TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs` with an MSTest `[TestClass]` containing a `private sealed class` subclass of `ApplicationGlobals` that overrides `LoadBasicMethod` (no-op, set `_loadBasicElapsed` via reflection to a fixed span as in the existing test), overrides all phase methods (`LoadIntelConfigPhaseAsync`, `LoadOlObjectsPhaseAsync`, `LoadToDoPhaseAsync`, `LoadAutoFilePhaseAsync`, `LoadEventsPhaseAsync`) to return `Task.CompletedTask`, mocks `Engines` (`IAppItemEngines`) via Moq, and overrides `YieldWithContinuationProbeAsync(string priorPhaseName)` to record `priorPhaseName` into a `List<string>` WITHOUT calling base (so no static `ApplicationIdleTimer` reads occur in the unit test). Acceptance: file compiles; uses MSTest + Moq + FluentAssertions; no live COM, no live timer, no network/filesystem, no temporary files. (AC3)
- [x] [P2-T2] Add a `[TestMethod] [DoNotParallelize]` to `ContinuationProbeSequenceTests.cs` that drives `await sut.LoadAsync(parallel: false)` and asserts the recorded `priorPhaseName` list equals exactly `["IntelConfig", "OlObjects", "ToDo", "AutoFile", "Engines"]` in that order. Acceptance: test passes and fails if any name or ordering is changed; uses FluentAssertions `.Should().Equal(...)`. (AC3)
- [x] [P2-T3] Add a `[TestMethod]` to `ContinuationProbeSequenceTests.cs` asserting the probe is invoked exactly once per inter-phase boundary (recorded list count == 5). Acceptance: test passes; assertion is `.Should().HaveCount(5)`. (AC3)
- [x] [P2-T4] Wire the new test file into `TaskMaster.Test/TaskMaster.Test.csproj` by adding `<Compile Include="AppGlobals\ContinuationProbeSequenceTests.cs" />` adjacent to the existing AppGlobals test includes (near line 263). Acceptance: the csproj contains exactly one `<Compile Include>` for the new file; the project compiles with the new test discovered. (AC3)

---

### Phase 3 — Final QC Loop (AC4)

Run the full C# toolchain in order. If any step changes files or fails, fix and restart from the first step.

- [x] [P3-T1] Run CSharpier format (`dotnet tool run csharpier .`) and record to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/qa-gates/final-csharpier.md`. Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; EXIT_CODE 0 and no residual unformatted files. (AC4)
- [x] [P3-T2] Run the analyzer build (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) and record to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/qa-gates/final-analyzers.md`. Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors and no banned-API (RS0030) violation for the new probe. (AC4)
- [x] [P3-T3] Run the nullable/TWAE build (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`) and record to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/qa-gates/final-nullable-twae.md`. Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no nullable warnings-as-errors. (AC4)
- [x] [P3-T4] Run MSTest with coverage gated to exclude LiveOutlook (`vstest.console.exe <TaskMaster.Test assembly> /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`) and record to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/qa-gates/final-mstest-coverage.md`. Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all tests pass including the new probe-sequence tests; numeric post-change repository-wide line coverage recorded. (AC4)
- [x] [P3-T5] Record a coverage delta/threshold verification to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/qa-gates/coverage-delta.md` comparing baseline coverage (P0-T6), post-change coverage (P3-T4), and new-code coverage for `YieldWithContinuationProbeAsync`. Acceptance: artifact reports all three numeric values; repository-wide line coverage remains >= 80% (no regression); the new probe method's executable lines reachable from the unit-test seam meet the >= 90% new-code obligation, with any COM/Dispatcher-bound or static-`ApplicationIdleTimer`-read lines explicitly noted as COM/VSTO-exempt by inspection (the unit-test seam overrides the probe and does not execute the static reads; those reads are verified only by the maintainer runtime capture AC5).
- [x] [P3-T6] Verify and record the post-change line count of `TaskMaster/AppGlobals/ApplicationGlobals.cs` to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/qa-gates/final-file-size.md`. Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; `ApplicationGlobals.cs` and the new test file are each <= 500 lines. (AC4)

---

## Acceptance Criteria Mapping

- AC1 (one `[continuation-resume]` line per inter-phase boundary with all seven fields): P1-T1 through P1-T6.
- AC2 (behavior-preserving; replaces `Task.Yield()` yields without changing order/count/outcomes; Stopwatch only; no banned API; net48): P1-T1 through P1-T7.
- AC3 (deterministic MSTest via subclass override verifying invocation once per boundary in correct order with correct names; no live COM/timer/network/filesystem/temp files): P2-T1 through P2-T4.
- AC4 (full toolchain passes in order; new seam meets coverage policy; no repo regression; touched files <= 500 lines): P3-T1 through P3-T6.
- AC5 (maintainer runtime capture): out of plan scope; not CI-automatable.
- AC6 (Phase 2 evidence-gated): out of plan scope.
