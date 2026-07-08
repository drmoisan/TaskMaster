# Phase 4 — Toolchain Gate (ApplicationGlobals Wiring Tests) (Issue #202)

Timestamp: 2026-06-15T12-15

All four steps passed in a single final pass. The loop was restarted earlier in the phase to
(a) add `using System.Linq;`, (b) remove a new `object?`/`List<string>?` nullable annotation that
triggered CS8632, (c) add a `protected internal virtual LoadBasicMethod()` test seam so
`LoadAsync` can be driven deterministically without live COM collaborator construction, and
(d) add `[DoNotParallelize]` to the four new flag-mutating tests to remove a rare cross-class
flake caused by the shared `Settings.Default` singleton and the process-global `ApplicationGlobals`
log4net logger. Values below are from the final clean pass.

## Step 1 — Format

Command: `csharpier format .` (CSharpier v1.3.0)
EXIT_CODE: 0
Output Summary: `Formatted 1057 files`. `csharpier check .` clean (EXIT 0).

## Step 2 — Analyzer (Lint) Build

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s). The only residual analyzer diagnostics in the test
file are two PRE-EXISTING CS8632 warnings on the original `TestableApplicationGlobals`
`IList<string>?` declarations (lines 606, 610), which predate #202. The new test code adds zero
diagnostics.

## Step 3 — Nullable / TreatWarningsAsErrors Build

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Warning(s), 0 Error(s). Run in the established way (plain
Debug build keeps outputs current, then incremental nullable Build passes 0/0).

## Step 4 — Test + Coverage

Command: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation /ResultsDirectory:TestResults/p4c`
EXIT_CODE: 0
Output Summary: Total tests 102, Passed 102, Failed 0 (98 + 4 new wiring tests). Determinism
confirmed by three additional runs (102/102 each).

New wiring tests (all pass):
- `LoadAsync_WhenTimingDisabled_RecordsNothingAndEmitsNoTable` (P4-T2)
- `LoadAsync_WhenTimingEnabled_RecordsAllPhasesInStartupOrderWithLoadBasicFirst` (P4-T3)
- `LoadAsync_WhenTimingEnabled_EmitsExactlyOneTableWithPhaseNamesAndTotal` (P4-T4)
- `LoadAsync_PreservesPhaseOrderingAndYieldCount_WhenTimingOnVersusOff` (P4-T5)

Post-change `ApplicationGlobals` coverage (from merged Cobertura `TestResults/p4.cobertura.xml`):

- `ApplicationGlobals.cs` aggregate line coverage (primary class + async state machines, deduped
  by line): 70.9% (95 / 134 lines), UP from the baseline 60.75% (65 / 107).
- `TaskMaster.ApplicationGlobals` primary class line-rate: 72.37% (baseline 74.24%). The nominal
  primary-class line-rate dip is a denominator effect (new instrumented lines added); the
  CHANGED lines are covered.
- New recorder `StartupTimingRecorder.cs`: 100% (30 / 30 lines).

Changed-line coverage check: the only uncovered lines within the changed region are the
PRE-EXISTING parallel-startup branch (`if (parallel) { await LoadParallelAsync(); }`) and the
`LoadParallelAsync` body, which are explicitly OUT OF SCOPE (user-story Non-Goals: parallel path
not instrumented) and were uncovered in the baseline as well. Every NEW timing-instrumentation
line (flag read, recorder selection, LoadBasic Stopwatch, per-phase recording, StopAndRestart,
EmitTable call) is covered. No coverage regression on changed lines.

## Implementation notes

- `TestableApplicationGlobals` gained: a `TimingRecorder` observation property (reads private
  `_timingRecorder`), and an overridden `LoadBasicMethod()` that sets `_loadBasicElapsed`
  deterministically and skips live COM construction (P4-T1 seam).
- `StartupTimingRecorder` gained an internal `RecordedPhaseNames` accessor for test observation.
- `ApplicationGlobals.LoadBasicMethod()` changed from `private` to `protected internal virtual`
  to provide the test seam; production behavior unchanged.
