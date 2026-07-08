# debug-startup-timing-instrumentation — Plan

- **Issue:** #202
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-15T12-15
- **Status:** Ready for Execution
- **Version:** 1.0
- **Work Mode:** full-feature
- **Plan file (in place, all revisions):** `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/plan.2026-06-15T12-15.md`

## Required References

Apply repository policies in the order defined by `policy-compliance-order`:

1. `CLAUDE.md` (standing instructions)
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md` (C# toolchain, analyzer stack, banned APIs, coverage floors)

Authoritative inputs:
- Spec: `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/spec.md`
- Issue / Acceptance Criteria: `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/issue.md`
- User story: `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/user-story.md`
- Research: `artifacts/research/2026-06-15T14-00-startup-timing-instrumentation-202.md`

**All work must comply with these policies; do not duplicate their content here.**

## Evidence Location Invariant

All evidence artifacts MUST be written under the canonical feature evidence root:
`docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/<kind>/`
where `<kind>` is one of `baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, `other/`.

Writing evidence to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any other
non-canonical location is a policy violation per `evidence-and-timestamp-conventions` and is rejected
by the `enforce-evidence-locations.ps1` PreToolUse hook. Timestamps use `yyyy-MM-ddTHH-mm`.

EVIDENCE_LOCATION_OVERRIDE_REJECTED note: the legacy stub referenced `artifacts/` evidence paths; this
plan substitutes the canonical `<FEATURE>/evidence/<kind>/` scheme for all evidence tasks.

## Acceptance Criteria (source: issue.md `## Acceptance Criteria (early draft)` reconciled with spec/user-story)

- **AC1** — A flag exists that enables or disables startup timing instrumentation; when disabled there is no behavioral or output change to startup.
- **AC2** — When enabled, each startup sub-component's elapsed wall-clock time is captured during startup.
- **AC3** — When enabled, a formatted plain-text table of sub-component names and elapsed times (plus a total row) is emitted to the output after startup completes.
- **AC4** — The timing recorder/formatter is a testable unit (no Outlook/COM dependency) with MSTest coverage meeting the repository floor for new code (>= 90%).
- **AC5** — Instrumentation uses existing logging/output infrastructure and existing approved dependencies; it does not change functional startup behavior.

AC-to-task map is maintained in the final QA phase (Phase 5).

---

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture & Policy Read

- [x] [P0-T1] Record the policy-read evidence artifact at `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/baseline/phase0-instructions-read.2026-06-15T12-15.md`
  - Acceptance: Artifact contains `Timestamp:`, `Policy Order:`, and an explicit list of files read in order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`. No code is modified before this task completes.

- [x] [P0-T2] Capture the CSharpier formatting baseline by running `dotnet tool run csharpier --check .` and writing the result to `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/baseline/format-baseline.2026-06-15T12-15.md`
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (clean-or-dirty status). No source files are modified by this task.

- [x] [P0-T3] Capture the analyzer (lint) build baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and writing the result to `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/baseline/analyzer-baseline.2026-06-15T12-15.md`
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build pass/fail and analyzer warning count).

- [x] [P0-T4] Capture the nullable/TreatWarningsAsErrors type-check baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and writing the result to `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/baseline/typecheck-baseline.2026-06-15T12-15.md`
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and any promoted-warning errors).

- [x] [P0-T5] Capture the MSTest test + coverage baseline by running `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` (include at minimum the `TaskMaster.Test` assembly) and writing the result to `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/baseline/test-coverage-baseline.2026-06-15T12-15.md`
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric values: total tests passed/failed, repository-wide line coverage percent, and the current `ApplicationGlobals` line coverage percent. Coverage values must be numeric (not placeholders).

- [x] [P0-T6] Record a design-resolution note for the startup timing recorder's table-formatting reuse at `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/other/timing-recorder-format-reuse.2026-06-15T12-15.md`
  - Preconditions: `UtilitiesCS/HelperClasses/SegmentStopWatch.cs` and `UtilitiesCS/HelperClasses/PrettyPrint.cs` reviewed.
  - Acceptance: Note records the selected design (no alternatives): `StartupTimingRecorder` maintains its OWN ordered collection of `(string phaseName, TimeSpan elapsed)` pairs in insertion order and does NOT wrap or call `SegmentStopWatch`. The note states the verified reason: `SegmentStopWatch.GetDurations()` (`UtilitiesCS/HelperClasses/SegmentStopWatch.cs` line 90) builds the TOTAL row from the watch's own `this.Elapsed`, which is `TimeSpan.Zero` for an injected-span watch, so wrapping `SegmentStopWatch` for injected spans yields an always-zero TOTAL and is unsatisfiable. The note records the genuinely reusable formatting primitive to call: `UtilitiesCS.HelperClasses.PrettyPrinters.ToFormattedText(this string[][] jagged, string[] headers = null, Enums.Justification[] justifications = default, string title = null)` defined in `UtilitiesCS/HelperClasses/PrettyPrint.cs` lines 179-184 (the same jagged `string[][]` overload `SegmentStopWatch.GetDurations` calls), invoked with headers `["Duration", "Action"]` and justifications `[Enums.Justification.Right, Enums.Justification.Left]` consistent with the existing convention. The note states the recorder computes a TOTAL row whose duration equals the SUM of all recorded spans (no reimplementation of column alignment). No production code is modified by this task.

### Phase 1 — Settings Flag

- [x] [P1-T1] Add the `StartupTimingEnabled` boolean user setting (default `False`) to `TaskMaster/Properties/Settings.settings`
  - Acceptance: `Settings.settings` contains a `<Setting Name="StartupTimingEnabled" Type="System.Boolean" Scope="User">` entry with `<Value Profile="(Default)">False</Value>`, following the existing `EventsHooked` entry pattern.

- [x] [P1-T2] Add the generated `StartupTimingEnabled` property (default `False`, `[UserScopedSetting]`, `[DefaultSettingValue("False")]`) to `TaskMaster/Properties/Settings.Designer.cs`
  - Acceptance: `Settings.Designer.cs` exposes `public bool StartupTimingEnabled { get; set; }` consistent with the existing generated boolean settings; the generated value matches the `.settings` entry.

- [x] [P1-T3] Run the full C# toolchain loop for Phase 1 in order and record the gate artifact at `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/phase1-toolchain.2026-06-15T12-15.md`
  - Steps in order: (1) `dotnet tool run csharpier .`; (2) `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; (3) `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; (4) `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. Restart from step 1 if any step fails or changes files.
  - Acceptance: Artifact records each step's `Command:`, `EXIT_CODE:`, and `Output Summary:`; final pass shows all four steps clean in a single pass.

### Phase 2 — Recorder Abstraction

- [x] [P2-T1] Create the internal recorder interface `internal interface IStartupTimingRecorder` in new file `TaskMaster/AppGlobals/IStartupTimingRecorder.cs`
  - Acceptance: Interface declares `void RecordPhase(string phaseName, TimeSpan elapsed)`, `string FormatTable()`, and `void EmitTable(log4net.ILog logger)` with XML doc comments stating contracts (non-null `phaseName`, call-order recording, pure deterministic `FormatTable`, `[Startup timing]`-prefixed emission). File is < 500 lines.

- [x] [P2-T2] Create `internal sealed class StartupTimingRecorder : IStartupTimingRecorder` in new file `TaskMaster/AppGlobals/StartupTimingRecorder.cs` that maintains its OWN ordered `(string phaseName, TimeSpan elapsed)` collection (does NOT wrap or call `SegmentStopWatch`)
  - Preconditions: Design note from P0-T6.
  - Acceptance: `RecordPhase(string phaseName, TimeSpan elapsed)` appends the supplied pre-measured pair to an internal ordered collection in insertion order. `FormatTable()` builds the table by reusing the existing public formatting primitive `UtilitiesCS.HelperClasses.PrettyPrinters.ToFormattedText(this string[][] jagged, string[] headers = null, Enums.Justification[] justifications = default, string title = null)` (`UtilitiesCS/HelperClasses/PrettyPrint.cs` lines 179-184) — reuse this primitive; do not reimplement column alignment. `FormatTable()` passes headers `["Duration", "Action"]` and justifications `[Enums.Justification.Right, Enums.Justification.Left]` consistent with the existing convention, one row per recorded phase, and a final `TOTAL` row whose duration equals the SUM of all recorded spans (not `TimeSpan.Zero`). `EmitTable` emits the formatted table via `logger.Info(...)` with the `[Startup timing]` prefix. No Outlook/COM, filesystem, or network access. File is < 500 lines.

- [x] [P2-T3] Create `internal sealed class NullStartupTimingRecorder : IStartupTimingRecorder` (co-located in `TaskMaster/AppGlobals/StartupTimingRecorder.cs` or a separate `NullStartupTimingRecorder.cs`)
  - Acceptance: `RecordPhase` returns immediately, `FormatTable` returns `string.Empty`, `EmitTable` emits nothing. No external dependencies. File(s) remain < 500 lines.

- [x] [P2-T4] Create the recorder unit test class `StartupTimingRecorderTests` at `TaskMaster.Test/AppGlobals/StartupTimingRecorderTests.cs` (MSTest, FluentAssertions; AAA structure)
  - Acceptance: New test file exists with `[TestClass]`/`[TestMethod]` and no temp files or external dependencies.

- [x] [P2-T5] Add a `StartupTimingRecorder` test verifying named-span capture for positive durations and call ordering
  - Acceptance: Test records multiple phases with distinct positive `TimeSpan` values; `FormatTable()` output contains each phase name in recorded order. Asserted with FluentAssertions; clear failure messages.

- [x] [P2-T6] Add a `StartupTimingRecorder` test verifying zero-duration span handling
  - Acceptance: Recording a phase with `TimeSpan.Zero` is captured and rendered without error; the phase name appears in `FormatTable()` output. Asserted with FluentAssertions.

- [x] [P2-T7] Add a `StartupTimingRecorder` test verifying the formatted table contains the `Duration` and `Action` column headers, each phase name, and a `TOTAL` row whose duration equals the sum of the injected spans
  - Acceptance: After recording multiple phases with distinct non-zero `TimeSpan` values injected deterministically, `FormatTable()` output contains the strings `Duration`, `Action`, and `TOTAL`, and contains each recorded phase name. The test parses/asserts the rendered TOTAL row duration and verifies it is non-zero and equals the sum of the injected non-zero phase spans (TOTAL reflects aggregate elapsed time). Because spans are injected deterministically (no `Stopwatch` in the recorder), this assertion is deterministic. Asserted with FluentAssertions.

- [x] [P2-T8] Add a `NullStartupTimingRecorder` test verifying no-op behavior
  - Acceptance: After `RecordPhase` calls, `FormatTable()` returns an empty string and `EmitTable(...)` produces no logged table (verified via a captured/mocked `log4net.ILog` confirming no `Info` table emission). Asserted with FluentAssertions/Moq.

- [x] [P2-T9] Run the full C# toolchain loop for Phase 2 in order and record the gate artifact at `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/phase2-toolchain.2026-06-15T12-15.md`
  - Steps in order: (1) `dotnet tool run csharpier .`; (2) analyzer msbuild; (3) nullable/TreatWarningsAsErrors msbuild; (4) `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. Restart from step 1 if any step fails or changes files.
  - Acceptance: Artifact records each step's `Command:`, `EXIT_CODE:`, `Output Summary:`; `Output Summary:` includes the new recorder classes' line coverage percent (target >= 90%). All four steps clean in the final single pass.

### Phase 3 — ApplicationGlobals Wiring

- [x] [P3-T1] Add `using TaskMaster.Properties;` to `TaskMaster/AppGlobals/ApplicationGlobals.cs`
  - Acceptance: The directive is present; the file compiles in the Phase 3 toolchain run.

- [x] [P3-T2] Add the private field `private IStartupTimingRecorder _timingRecorder;` (or nullable equivalent) to `ApplicationGlobals`
  - Acceptance: Field is declared as a private instance member; nullable-state is consistent with the C# nullable gate.

- [x] [P3-T3] In `ApplicationGlobals.LoadAsync`, read `Settings.Default.StartupTimingEnabled` once before the sequential/parallel branch and assign `_timingRecorder` to a `StartupTimingRecorder` when enabled or a `NullStartupTimingRecorder` when disabled
  - Preconditions: P3-T2.
  - Acceptance: The flag is read exactly once in `LoadAsync`; on the disabled path `_timingRecorder` is the no-op recorder and no spans are recorded; follows the `Settings.Default.EventsHooked` consumption pattern. Parallel path behavior is unchanged.

- [x] [P3-T4] Instrument `LoadBasicMethod()` itself with a single `System.Diagnostics.Stopwatch` (start at method entry, stop after the last assignment) and store the measured elapsed into a new private field `private TimeSpan _loadBasicElapsed;` on `ApplicationGlobals`; then, when the recorder is initialized in `LoadAsync` (flag on), record `("LoadBasic", _loadBasicElapsed)` as the FIRST phase before the six sequential phases
  - Preconditions: P3-T3. `ApplicationGlobals.cs` constructor/`BasicLoaded` Lazy reviewed.
  - Acceptance: `LoadBasicMethod()` measurement is UNCONDITIONAL (it does not branch on the flag) because the verified construction path runs `ForceBasicLoad()` inside the `ApplicationGlobals(Application, loadBasic: true)` constructor (line 43), materializing the `BasicLoaded` `Lazy<bool>` BEFORE `LoadAsync` runs; measuring around `ForceBasicLoad()` inside `LoadAsync` would record ~0. A single `Stopwatch` start/stop with no allocation is negligible overhead and satisfies the "negligible overhead when flag off" constraint — state this rationale in the implementation. When the flag is on, `("LoadBasic", _loadBasicElapsed)` is recorded exactly once as the FIRST recorded phase and reflects real construction time; when off, the no-op recorder is used and nothing is recorded. No COM-thread affinity or async restructuring is introduced. Uses `System.Diagnostics.Stopwatch` (not `DateTime.Now`/`UtcNow`).

- [x] [P3-T5] In `LoadSequentialAsync`, record each of the six awaited phases (`IntelConfig`, `OlObjects`, `ToDo`, `AutoFile`, `Engines`, `Events`) by measuring elapsed time around each `await ...PhaseAsync()` and calling `_timingRecorder.RecordPhase(<phaseName>, elapsed)` after the await
  - Preconditions: P3-T3, P3-T4 (the `LoadBasic` phase is recorded first; these six follow it in order).
  - Acceptance: Each of the six phases records exactly one span, in startup order, after the `LoadBasic` phase from P3-T4; `YieldBetweenStartupPhasesAsync` calls are not recorded; phase ordering and existing functional behavior are unchanged. Timing uses `Stopwatch`. The recorder reference is invoked unconditionally (no-op recorder absorbs the flag-off case).

- [x] [P3-T6] At the end of `LoadAsync` (after the sequential coordinator returns), emit the table once via `_timingRecorder.EmitTable(logger)`
  - Acceptance: On the flag-on sequential path, exactly one `[Startup timing]` table is emitted via the `ApplicationGlobals` log4net `logger`; on the flag-off path nothing is emitted (no-op recorder). The parallel path emits via the same `EmitTable` call without new phase instrumentation (parallel phase recording is out of scope per the user story Non-Goals).

- [x] [P3-T7] Verify `ApplicationGlobals.cs` remains under the 500-line file-size limit
  - Acceptance: File line count is < 500.

- [x] [P3-T8] Run the full C# toolchain loop for Phase 3 in order and record the gate artifact at `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/phase3-toolchain.2026-06-15T12-15.md`
  - Steps in order: (1) csharpier; (2) analyzer msbuild; (3) nullable/TreatWarningsAsErrors msbuild; (4) `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. Restart from step 1 if any step fails or changes files.
  - Acceptance: Artifact records each step's `Command:`, `EXIT_CODE:`, `Output Summary:`; all four steps clean in the final single pass.

### Phase 4 — ApplicationGlobals Wiring Tests

- [x] [P4-T1] Extend `TestableApplicationGlobals` in `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` to expose a seam that lets tests inject/observe `_timingRecorder` and drive `LoadAsync`/`LoadSequentialAsync` deterministically without a live Outlook process
  - Preconditions: Existing `TestableApplicationGlobals` override pattern (visited-stages) at lines ~386–438.
  - Acceptance: The test double supports observing recorded phase names/order and the chosen recorder type, reusing the existing per-phase override seam; no temp files, no external dependencies, deterministic.

- [x] [P4-T2] Add a test verifying the flag-off path records nothing and emits no table
  - Acceptance: With `StartupTimingEnabled` resolved to off, after driving the sequential load no spans are recorded and no `[Startup timing]` table is emitted (verified via mocked/captured `log4net.ILog` or the observed recorder). Asserted with FluentAssertions/Moq. AAA structure.

- [x] [P4-T3] Add a test verifying the flag-on path records all instrumented phases in startup order, with `LoadBasic` first
  - Preconditions: P3-T4 (LoadBasic recorded first), P3-T5.
  - Acceptance: With the flag on, the recorded phase sequence equals `["LoadBasic", "IntelConfig", "OlObjects", "ToDo", "AutoFile", "Engines", "Events"]` (or the subset driven by the sequential overrides, but always with `LoadBasic` as the first element), in order. The test explicitly asserts the first recorded phase name is `LoadBasic`. Asserted with FluentAssertions.

- [x] [P4-T4] Add a test verifying the flag-on path emits exactly one formatted table containing each phase name and a TOTAL row
  - Acceptance: After the flag-on sequential load, the captured `[Startup timing]` emission occurs once and its text contains each recorded phase name and `TOTAL`. Asserted with FluentAssertions.

- [x] [P4-T5] Add a test verifying instrumentation preserves existing phase ordering and behavior (regression guard)
  - Acceptance: The existing phase-visit ordering and yield-count behavior are unchanged when timing is on versus off; the test asserts the visited-stages sequence is identical in both modes. Asserted with FluentAssertions.

- [x] [P4-T6] Run the full C# toolchain loop for Phase 4 in order and record the gate artifact at `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/phase4-toolchain.2026-06-15T12-15.md`
  - Steps in order: (1) csharpier; (2) analyzer msbuild; (3) nullable/TreatWarningsAsErrors msbuild; (4) `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. Restart from step 1 if any step fails or changes files.
  - Acceptance: Artifact records each step's `Command:`, `EXIT_CODE:`, `Output Summary:`; `Output Summary:` includes the post-change `ApplicationGlobals` line coverage percent. All four steps clean in the final single pass.

### Phase 5 — Final QA Loop, Coverage Verification & AC Check-off

- [x] [P5-T1] Run the final formatting gate `dotnet tool run csharpier .` and record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/final-format.2026-06-15T12-15.md`
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; no files changed by the final run.

- [x] [P5-T2] Run the final analyzer gate `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/final-analyzer.2026-06-15T12-15.md`
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build passes with no new analyzer regressions.

- [x] [P5-T3] Run the final type-check gate `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/final-typecheck.2026-06-15T12-15.md`
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build passes with no promoted-warning errors.

- [x] [P5-T4] Run the final test+coverage gate `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` and record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/final-test-coverage.2026-06-15T12-15.md`
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric values: all tests pass, repository-wide line coverage percent, new recorder classes' coverage percent, and post-change `ApplicationGlobals` coverage percent.
  - Note: If any of P5-T1..P5-T4 changes files or fails, restart the loop from P5-T1 per the toolchain restart rule.

- [x] [P5-T5] Record the coverage delta/threshold verification at `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/coverage-delta.2026-06-15T12-15.md`
  - Preconditions: P0-T5 (baseline) and P5-T4 (post-change).
  - Acceptance: Artifact reports baseline coverage, post-change coverage, and new/changed-code coverage; confirms repository-wide line coverage remains >= 80%, new recorder classes reach >= 90%, and changed `ApplicationGlobals` lines show no coverage regression. If any required value is unavailable or a threshold is unmet, the outcome is recorded as remediation-required (not PASS).

- [x] [P5-T6] Map each acceptance criterion to its implementing/verifying tasks per `acceptance-criteria-tracking` and record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/issue-updates/ac-checkoff.2026-06-15T12-15.md`
  - Acceptance: Artifact contains the AC-to-task mapping below with each AC marked PASS only when its verification task and evidence exist:
    - AC1 (flag on/off, no change when off) → P1-T1, P1-T2, P3-T3, P3-T4, P3-T6 (impl); P4-T2, P4-T5 (verify)
    - AC2 (per-sub-component elapsed captured when on) → P3-T4, P3-T5 (impl); P4-T3 (verify)
    - AC3 (formatted table with TOTAL emitted after startup) → P2-T2, P3-T6 (impl); P2-T7, P4-T4 (verify)
    - AC4 (testable recorder, >= 90% new-code coverage) → P2-T1, P2-T2, P2-T3 (impl); P2-T4..P2-T8, P5-T4, P5-T5 (verify)
    - AC5 (existing logging/deps only, no functional change) → P2-T2, P3-T6 (impl); P4-T5, P5-T2, P5-T3 (verify)

- [x] [P5-T7] Update the feature `issue.md` and `spec.md` acceptance-criteria checkboxes to reflect verified status and mirror the update per `evidence-and-timestamp-conventions`
  - Acceptance: `issue.md` and `spec.md` AC checkboxes reflect the verified state from P5-T6; a mirror artifact exists under `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/issue-updates/`.

## Test Plan

- Unit (recorder): `TaskMaster.Test/AppGlobals/StartupTimingRecorderTests.cs` — named-span capture (positive, zero-duration, ordering), table format (headers + TOTAL), null recorder no-op. MSTest + Moq + FluentAssertions.
- Unit (wiring): `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` (extend `TestableApplicationGlobals`) — flag-off records nothing/emits nothing, flag-on records all phases in order, single-table emission with phase names + TOTAL, phase-ordering regression guard.
- Integration: not applicable (no live Outlook process; COM-bound paths are stubbed via existing seams).
- Manual/CLI: none (feature has no CLI surface).
- Coverage evidence:
  - Baseline: `evidence/baseline/test-coverage-baseline.2026-06-15T12-15.md`
  - Post-change: `evidence/qa-gates/final-test-coverage.2026-06-15T12-15.md`
  - Comparison: `evidence/qa-gates/coverage-delta.2026-06-15T12-15.md`

## Open Questions / Notes

- Selected design (no alternatives): `StartupTimingRecorder` maintains its OWN ordered `(string phaseName, TimeSpan elapsed)` collection and does NOT wrap `SegmentStopWatch`. Verified reason: `SegmentStopWatch.GetDurations()` (`UtilitiesCS/HelperClasses/SegmentStopWatch.cs` line 90) builds the TOTAL row from the watch's own `this.Elapsed`, which is `TimeSpan.Zero` for an injected-span watch, making the wrapping approach yield an always-zero TOTAL. The recorder instead reuses the genuinely reusable formatting primitive `UtilitiesCS.HelperClasses.PrettyPrinters.ToFormattedText(this string[][] jagged, ...)` (`UtilitiesCS/HelperClasses/PrettyPrint.cs` lines 179-184) — the same overload `GetDurations` calls — with headers `["Duration", "Action"]` and right/left justifications, and computes a summed TOTAL row. Captured in design-resolution task P0-T6; constrains P2-T2 and P2-T7.
- LoadBasic measurement is taken inside `LoadBasicMethod()` (not around `ForceBasicLoad()` in `LoadAsync`). Verified reason: when `ApplicationGlobals` is constructed with `loadBasic: true`, `ForceBasicLoad()` runs in the constructor (`ApplicationGlobals.cs` line 43) and materializes the `BasicLoaded` `Lazy<bool>` before `LoadAsync` executes, so measuring inside `LoadAsync` records ~0. The recorder records `("LoadBasic", _loadBasicElapsed)` as the first phase when the flag is on. Captured in P3-T4; P3-T5 and P4-T3 depend on LoadBasic being first.
- Parallel path (`LoadParallelAsync`) phase recording is out of scope (user-story Non-Goals). `EmitTable` is still called at the end of `LoadAsync`; on the parallel path no sequential phase spans are recorded, so the emitted table contains the `LoadBasic` row (recorded unconditionally) plus a summed TOTAL row when the flag is on.
- No new dependencies: `PrettyPrinters.ToFormattedText`, `Stopwatch`, and `log4net` are already approved and present. `Stopwatch`-based timing does not trigger the BannedApiAnalyzers `DateTime.Now`/`UtcNow` rule.
