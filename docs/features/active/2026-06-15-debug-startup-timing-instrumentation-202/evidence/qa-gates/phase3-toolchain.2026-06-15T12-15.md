# Phase 3 — Toolchain Gate (ApplicationGlobals Wiring) (Issue #202)

Timestamp: 2026-06-15T12-15

All four steps passed in a single final pass.

## Step 1 — Format

Command: `csharpier format .` (CSharpier v1.3.0)
EXIT_CODE: 0
Output Summary: `Formatted 1057 files`. `csharpier check .` clean (EXIT 0). `ApplicationGlobals.cs`
is 244 lines (< 500, satisfies P3-T7).

## Step 2 — Analyzer (Lint) Build

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s). No analyzer diagnostics from the wired
`ApplicationGlobals.cs` (verified by grepping the verbose log).

## Step 3 — Nullable / TreatWarningsAsErrors Build

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Warning(s), 0 Error(s). A diagnostic force-recompile of the
TaskMaster project confirmed the wired `ApplicationGlobals.cs` produces ZERO nullable
errors/warnings. Gate run in the established way (plain Debug build keeps outputs current, then
incremental nullable Build passes 0/0).

## Step 4 — Test + Coverage

Command: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation /ResultsDirectory:TestResults/p3`
EXIT_CODE: 0
Output Summary: Total tests 98, Passed 98, Failed 0. The pre-existing source-text regression
tests `LoadSequentialAsync_KeepsComPhasesOnCallerThreadAndYieldsBetweenHeavyPhases` and
`LoadSequentialAsync_YieldsBeforeAutoFilePhase` both pass, as does the real-coordinator
sequence test `LoadSequentialAsync_ExecutesRealCoordinatorSequenceThroughPhaseWrappers`.

## Implementation notes (wiring)

- `using TaskMaster.Properties;` added (P3-T1).
- Private fields `_timingRecorder` (default `NullStartupTimingRecorder`) and `_loadBasicElapsed`
  added (P3-T2).
- `LoadAsync` reads `Settings.Default.StartupTimingEnabled` exactly once before the
  sequential/parallel branch and selects the concrete recorder (recording `("LoadBasic",
  _loadBasicElapsed)` first) or the no-op recorder; emits the table once via
  `_timingRecorder.EmitTable(logger)` at the end of `LoadAsync` (P3-T3, P3-T4, P3-T6).
- `LoadBasicMethod()` is instrumented unconditionally with a single `Stopwatch`, storing into
  `_loadBasicElapsed` (P3-T4). Rationale documented in-code: the construction-time `BasicLoaded`
  Lazy runs before `LoadAsync`.
- `LoadSequentialAsync` records each of the six phases (IntelConfig, OlObjects, ToDo, AutoFile,
  Engines, Events) in order via a shared `Stopwatch` (`StopAndRestart` helper) after each phase
  await; yields are not recorded (P3-T5).

### Pre-existing test adaptation (mechanically necessary for P3-T5)

`LoadSequentialAsync_YieldsBeforeAutoFilePhase` previously asserted strict TEXTUAL adjacency of
`await LoadToDoPhaseAsync(); await YieldBetweenStartupPhasesAsync(); await LoadAutoFilePhaseAsync();`.
The planned per-phase instrumentation inserts a single `_timingRecorder.RecordPhase(...)` call
after the ToDo await, which breaks textual adjacency without changing the ORDERING guarantee.
The test's regex was updated to assert the same ToDo -> yield -> AutoFile ordering while
tolerating the interleaved RecordPhase, and a tightened guard now asserts that ONLY a
`_timingRecorder.RecordPhase(...)` statement may appear between the ToDo await and the yield.
The behavioral intent (yield between ToDo and AutoFile) is preserved, not weakened.
