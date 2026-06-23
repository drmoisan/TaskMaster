# Issue #211 — Phase 3 Engines-Phase Attribution Instrumentation Plan

- **Issue:** #211
- **Work Mode:** full-bug
- **Plan Timestamp:** 2026-06-23T14-30
- **Scope:** Phase 3 only (AC7, AC8, AC9). Phase 4 (the fix, AC10) is explicitly NOT planned here; it is evidence-gated on the AC9 maintainer non-debugger re-capture.
- **Feature folder:** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/`
- **Evidence root (canonical, non-overridable):** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/<kind>/`

## Objective

Add behavior-preserving per-engine attribution instrumentation to `AppItemEngines.InitAsync()` so the dominant `Engines`-phase startup cost (measured at `1:52.59` of a `1:58.79` total in the non-debugger capture) can be distributed to individual engines and to the upfront `Globals.AF.Manager.Configuration` deserialize. The instrumentation is diagnosis-only and mirrors the already-delivered Phase 1 `YieldWithContinuationProbeAsync` probe. It does not change phase order, engine set, or load semantics.

## Expected Outcome

- `AppItemEngines.InitAsync()` emits one structured `[engine-init-config]` line for the `Configuration` await and one `[engine-init]` line per active engine, each via the existing `log4net` logger, capturing engine name, wall-clock duration (`Stopwatch`, F1 ms), resolving thread id, an engine-null flag, and a coarse `costHint` (AC7).
- A deterministic MSTest (MSTest + Moq + FluentAssertions) covering the extracted pure timing/emission seam, asserting per-engine emission occurs once per active engine, in order, with correct names and fields; no live COM, no live timer, no network/filesystem, no temporary files; new seam meets the >=90% coverage policy with no repository-wide regression (AC8).
- Maintainer non-debugger re-capture instructions plus an evidence placeholder so the runtime capture (AC9) can be recorded under `evidence/other/`. Running Outlook is NOT performed by this plan.

## Design Decision — Testable Seam (drives AC8)

`AppItemEngines` is `[ExcludeFromCodeCoverage]` (it constructs COM-bound `IConditionalEngine<MailItemHelper>` instances), so instrumentation written inline inside `InitAsync` would be uncoverable and could not satisfy AC8's >=90%-on-new-code requirement. Therefore the per-engine timing/emission logic is extracted into a small, pure, coverable helper class `EngineInitTimingProbe` (NOT marked `[ExcludeFromCodeCoverage]`) with a narrow injected log-sink delegate seam (`Action<string>`), defaulting to the existing `log4net` logger in production. `AppItemEngines.InitAsync()` becomes a thin caller of that helper. The helper takes the engine name and a `Func<Task<IConditionalEngine<MailItemHelper>?>>` (the bound factory call), times it with `Stopwatch`, emits the structured line through the sink, and returns the engine. This keeps all timing/emission logic in a unit-testable class while the COM-bound factory invocation stays in `AppItemEngines`.

The same helper provides a `Configuration`-await timing method (`[engine-init-config]`) so the upfront `Globals.AF.Manager.Configuration` deserialize cost is isolated (research Candidate 2).

## Scope Lock (files this plan may touch)

- CREATE `TaskMaster/AppGlobals/EngineInitTimingProbe.cs` — new pure, coverable helper (timing + structured-line emission via injected `Action<string>` sink). Must compile into `TaskMaster.csproj`.
- MODIFY `TaskMaster/TaskMaster.csproj` — add `<Compile Include="AppGlobals\EngineInitTimingProbe.cs" />` so the new file builds into `TaskMaster.dll` (legacy non-SDK project; explicit `<Compile Include>` required — verify in Phase 0).
- MODIFY `TaskMaster/AppGlobals/AppItemEngines.cs` — `InitAsync()` only: time the `Configuration` await and route each per-engine factory call through `EngineInitTimingProbe`. Behavior-preserving; no change to phase order, engine set, filter predicate, or dictionary semantics.
- CREATE `TaskMaster.Test/AppGlobals/EngineInitTimingProbeTests.cs` — deterministic MSTest for the helper.
- MODIFY `TaskMaster.Test/TaskMaster.Test.csproj` — add `<Compile Include="AppGlobals\EngineInitTimingProbeTests.cs" />` (legacy non-SDK project; explicit `<Compile Include>` required — verify in Phase 0).
- CREATE evidence artifacts under `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/<kind>/` only.

## Guardrails (must hold for every implementation task)

- Behavior-preserving: do not change phase order, the engine set, the `config.Value.Engine` filter, the `EngineInitializer` lookup, or `InboxEngines` population semantics. Instrumentation only.
- `Stopwatch` only for timing. NO banned APIs: `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay` (enforced by BannedApiAnalyzers `BannedSymbols.txt`).
- net48: no positional `record struct`; if a small value carrier is needed use a plain class or a non-positional `struct`/`record` with explicit members.
- All touched files must remain `<= 500` lines (`AppItemEngines.cs` baseline is 263 lines; verify in Phase 0 and confirm post-change in final QA).
- Use the existing `log4net.ILog logger` already declared in `AppItemEngines.cs` for production emission; the helper accepts the sink as an injected delegate so tests do not depend on a live appender.
- Tests: MSTest + Moq + FluentAssertions only; deterministic; no live COM, no live timer, no network/filesystem, no temporary files.
- New seam (`EngineInitTimingProbe`) must NOT be `[ExcludeFromCodeCoverage]` and must reach `>= 90%` line coverage; no repository-wide coverage regression.
- C# toolchain order is mandatory and restarts from step 1 on any failure or file change: CSharpier -> .NET analyzers -> nullable/TWAE -> MSTest with coverage (gated `/TestCaseFilter:"TestCategory!=LiveOutlook"`).

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in required order and write `evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Binary outcome: file exists with all three fields populated.
- [x] [P0-T2] Capture the current line count of `TaskMaster/AppGlobals/AppItemEngines.cs` and `TaskMaster/AppGlobals/ApplicationGlobals.cs` and write `evidence/baseline/baseline-file-size-2026-06-23T14-30.md` with `Timestamp:`, `Command:` (the line-count command), `EXIT_CODE:`, and `Output Summary:` recording each file's line count and the `<= 500` headroom. Binary outcome: artifact records `AppItemEngines.cs` line count (expected 263) and confirms headroom for additions.
- [x] [P0-T3] Confirm `TaskMaster/TaskMaster.csproj` and `TaskMaster.Test/TaskMaster.Test.csproj` are legacy non-SDK projects using explicit `<Compile Include>` items (no glob) and record the exact existing sibling `<Compile Include>` lines (e.g., `AppGlobals\ContinuationProbeSequenceTests.cs`) into `evidence/baseline/baseline-csproj-wiring-2026-06-23T14-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: artifact confirms explicit-include style and lists the insertion anchor for each csproj.
- [x] [P0-T4] Run CSharpier in check mode at repo root and write `evidence/baseline/baseline-csharpier-2026-06-23T14-30.md` with `Timestamp:`, `Command:` (`dotnet tool run csharpier . --check`), `EXIT_CODE:`, `Output Summary:` (clean or list of pre-existing unformatted files). Binary outcome: artifact records the formatter baseline state.
- [x] [P0-T5] Run the analyzer build and write `evidence/baseline/baseline-analyzers-2026-06-23T14-30.md` with `Timestamp:`, `Command:` (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), `EXIT_CODE:`, `Output Summary:` (pass/fail and pre-existing diagnostic count). Binary outcome: artifact records the analyzer baseline.
- [x] [P0-T6] Run the nullable/TWAE build and write `evidence/baseline/baseline-nullable-2026-06-23T14-30.md` with `Timestamp:`, `Command:` (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`), `EXIT_CODE:`, `Output Summary:`. Binary outcome: artifact records the type-check baseline.
- [x] [P0-T7] Run the MSTest suite with coverage gated to exclude live Outlook tests and write `evidence/baseline/baseline-tests-coverage-2026-06-23T14-30.md` with `Timestamp:`, `Command:` (`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`), `EXIT_CODE:`, and `Output Summary:` including numeric baseline repo-wide line-coverage percent and the passed/failed counts. Binary outcome: artifact records numeric baseline coverage (not a placeholder) and the test pass/fail tally; record any known pre-existing flake (e.g., the `UtilitiesCS TimedAsyncTask_Tests` real-interval timer flake) explicitly.

### Phase 1 — Extract Testable Timing/Emission Seam

- [x] [P1-T1] Create `TaskMaster/AppGlobals/EngineInitTimingProbe.cs` defining a non-`[ExcludeFromCodeCoverage]` class `EngineInitTimingProbe` with: a constructor taking an injected `Action<string>` emit sink (production passes `s => logger.Debug(s)`); a method `Task<IConditionalEngine<MailItemHelper>?> TimeEngineAsync(string engineName, Func<Task<IConditionalEngine<MailItemHelper>?>> factory)` that starts a `Stopwatch`, awaits `factory()`, stops the stopwatch, emits one `[engine-init] engineName=<name> engineMs=<F1> engineNull=<bool> threadId=<ManagedThreadId> costHint=<Deserialization|Skip>` line via the sink (`costHint=Skip` when the engine is null, else `Deserialization`), and returns the engine; and a method `void EmitConfigTiming(double configMs, int threadId)` (or a `TimeConfigAsync` wrapper) that emits one `[engine-init-config] configMs=<F1> threadId=<id>` line. Use `Stopwatch` only; no banned APIs; net48-compatible. Wire the file into `TaskMaster/TaskMaster.csproj` with `<Compile Include="AppGlobals\EngineInitTimingProbe.cs" />`. Binary outcome: file exists, is wired into the csproj, and compiles into `TaskMaster.dll`.
- [x] [P1-T2] Verify (via reading the file) that `EngineInitTimingProbe.cs` is `<= 500` lines, contains no banned API token (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`), and uses no positional `record struct`. Binary outcome: a Grep over the file for those tokens returns zero matches and the line count is recorded as `<= 500`.

### Phase 2 — Instrument AppItemEngines.InitAsync (Behavior-Preserving)

- [x] [P2-T1] In `TaskMaster/AppGlobals/AppItemEngines.cs` `InitAsync()` only, wrap the `await Globals.AF.Manager.Configuration` await with a `Stopwatch` and emit the `[engine-init-config]` line via an `EngineInitTimingProbe` instance constructed with the existing `logger.Debug` sink. Do not change the assignment of `configs` or any downstream filter/select logic. Binary outcome: the `Configuration` await is timed and one `[engine-init-config]` line is emitted; `configs` value and type are unchanged.
- [x] [P2-T2] In the same `InitAsync()` `.SelectAwait` lambda, route the existing `await tup.EngineFunc(Globals)` call through `EngineInitTimingProbe.TimeEngineAsync(tup.Key, () => tup.EngineFunc(Globals))` so each active engine emits one `[engine-init]` line, then return `(tup.Key, Engine: engine)` exactly as before. Do not change the `.Where(config => config.Value.Engine)`, `EngineInitializer` lookup, the null filters, or `ToConcurrentDictionaryAsync`. Binary outcome: each active engine's factory call is timed and emits one `[engine-init]` line; `InboxEngines` population semantics are byte-for-behavior identical (same keys, same engine instances, same null filtering, same order).
- [x] [P2-T3] Verify (via reading the file) that `AppItemEngines.cs` remains `<= 500` lines after the additions, the engine set / phase order / filter predicates are unchanged from the Phase 0 baseline, and no banned API token was introduced. Binary outcome: line count recorded `<= 500` and a Grep for banned tokens over the diffed method returns zero matches.

### Phase 3 — Deterministic Seam Test (AC8)

- [x] [P3-T1] Create `TaskMaster.Test/AppGlobals/EngineInitTimingProbeTests.cs` (MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, Arrange-Act-Assert) and wire it into `TaskMaster.Test/TaskMaster.Test.csproj` with `<Compile Include="AppGlobals\EngineInitTimingProbeTests.cs" />`. The test class captures emitted lines into a `List<string>` sink (the injected `Action<string>`), uses stub factories returning a `Mock<IConditionalEngine<MailItemHelper>>().Object` (or `null`), and uses no live COM, no live timer, no network/filesystem, no temporary files. Binary outcome: file exists, is wired into the csproj, and compiles into the test assembly.
- [x] [P3-T2] Add a test asserting that for three stub engines invoked in order (`Spam`, `Triage`, `Actionable`), `TimeEngineAsync` emits exactly one `[engine-init]` line per engine, in the same order, each containing `engineName=<name>`, an `engineMs=` numeric F1 field, `engineNull=False`, a `threadId=` field, and `costHint=Deserialization`. Binary outcome: test passes and fails if a line is missing, duplicated, out of order, or missing any field.
- [x] [P3-T3] Add a test asserting that when the factory returns `null`, the emitted `[engine-init]` line contains `engineNull=True` and `costHint=Skip`, and that `TimeEngineAsync` returns `null`. Binary outcome: test passes and asserts both the field values and the return value.
- [x] [P3-T4] Add a test asserting `EmitConfigTiming` (or `TimeConfigAsync`) emits exactly one `[engine-init-config]` line containing a `configMs=` numeric F1 field and a `threadId=` field. Binary outcome: test passes and asserts the single line and its fields.
- [x] [P3-T5] Add a negative/edge test asserting that a factory throwing an exception propagates out of `TimeEngineAsync` (fail-fast; instrumentation does not swallow engine-init failures) and confirm the design choice matches the pre-instrumentation behavior where a throwing factory propagated through `.SelectAwait`. Binary outcome: test asserts the exception propagates and that behavior is unchanged relative to the un-instrumented path.

### Phase 4 — AC9 Maintainer Capture Instructions and Evidence Placeholder

- [x] [P4-T1] Write `evidence/other/ac9-nondebugger-recapture-instructions-2026-06-23T14-30.md` with step-by-step maintainer instructions to produce the non-debugger cold-start capture: build the add-in from this branch, ensure `StartupTimingEnabled` is on and `Debug`-level log4net output is captured (DebugView / OutputDebugString), launch Outlook outside the Visual Studio debugger with Teams installed, and collect the `[engine-init-config]` and per-engine `[engine-init]` lines plus the `[Startup timing]` table. Include the expected field set and how to attribute the `Engines`-phase wall-clock to specific engines/`Configuration`. Binary outcome: instructions file exists and enumerates the exact log markers to collect and the attribution method. This task does NOT run Outlook.
- [x] [P4-T2] Create the AC9 capture placeholder `evidence/other/runtime-capture-engines-nondebugger-PLACEHOLDER.md` stating the capture is pending maintainer execution, with the required schema (`Timestamp:`, environment, the `[engine-init-config]` line, each `[engine-init]` line, the `[Startup timing]` table) to be filled in on capture. Binary outcome: placeholder exists and is clearly marked pending; it does not assert any timing values.

### Phase 5 — Final QA Loop (mandatory order; restart from step 1 on any failure or file change)

- [x] [P5-T1] Run CSharpier formatting (`dotnet tool run csharpier .`) and write `evidence/qa-gates/final-qc-csharpier-2026-06-23T14-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If it changes any file, restart the loop from this task. Binary outcome: formatter reports clean with no remaining changes.
- [x] [P5-T2] Run the analyzer build (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) and write `evidence/qa-gates/final-qc-analyzers-2026-06-23T14-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: build succeeds with no new analyzer diagnostics versus the Phase 0 baseline.
- [x] [P5-T3] Run the nullable/TWAE build (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`) and write `evidence/qa-gates/final-qc-nullable-2026-06-23T14-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: build succeeds with no new nullable warnings-as-errors.
- [x] [P5-T4] Run the MSTest suite with coverage gated to exclude live Outlook (`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`) and write `evidence/qa-gates/final-qc-tests-coverage-2026-06-23T14-30.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording numeric post-change repo-wide line coverage and the passed/failed counts. Binary outcome: all non-live tests pass (pre-existing recorded flake excepted) and coverage numbers are captured.
- [x] [P5-T5] Write `evidence/qa-gates/final-qc-coverage-delta-2026-06-23T14-30.md` reporting `baseline coverage` (from P0-T7), `post-change coverage` (from P5-T4), and `new/changed-code coverage` for `EngineInitTimingProbe.cs` and the modified `InitAsync` lines, confirming `EngineInitTimingProbe` reaches `>= 90%` and there is no repository-wide regression versus baseline. Binary outcome: the delta artifact records all three numeric values and an explicit PASS/REMEDIATION-REQUIRED determination; if new-code coverage `< 90%` or repo-wide regresses, outcome is REMEDIATION-REQUIRED (not PASS).
- [x] [P5-T6] Write `evidence/qa-gates/final-qc-filesize-2026-06-23T14-30.md` confirming `AppItemEngines.cs`, `EngineInitTimingProbe.cs`, and `EngineInitTimingProbeTests.cs` are each `<= 500` lines, with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` listing each file's final line count. Binary outcome: artifact confirms all touched files `<= 500` lines.

## Acceptance Criteria Mapping

- AC7 -> P1-T1, P1-T2, P2-T1, P2-T2, P2-T3 (per-engine and config attribution lines; behavior-preserving; Stopwatch-only; net48; `<= 500` lines).
- AC8 -> P1-T1 (coverable seam), P3-T1..P3-T5 (deterministic MSTest), P5-T4, P5-T5 (coverage `>= 90%` on new code, no repo regression).
- AC9 -> P4-T1, P4-T2 (maintainer non-debugger capture instructions and evidence placeholder; capture itself is maintainer-run, not CI-automatable).
- AC10 (Phase 4 fix) -> intentionally NOT planned here; evidence-gated on the AC9 capture.

## Out of Scope (explicit)

- Any Phase 4 fix (parallelizing engine init, deferring to `IdleAsyncQueue`, pre-warming `Configuration`, or changing `PreserveReferencesHandling`) — these are gated on AC9 and tracked as research Fixes A–D.
- The optional `ManagerAsyncLazy.GetAsyncLazyClassifierLoader` finer-granularity probe (research §5.4) — not required for Phase 3 attribution; not planned.
- Any change to phase order, engine set, or load semantics in `ApplicationGlobals.LoadSequentialAsync`.

## Evidence Location Invariant

All evidence artifacts in this plan resolve to `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/<kind>/` (`baseline/`, `qa-gates/`, `other/`). No `artifacts/` evidence paths are used. This invariant is non-overridable.
