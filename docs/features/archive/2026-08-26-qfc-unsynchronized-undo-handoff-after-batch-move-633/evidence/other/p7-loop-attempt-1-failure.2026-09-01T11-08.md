# Phase 7 loop, attempt 1 — one unrelated test failure, loop restarted

Timestamp: 2026-09-01T11-08
Task: [P7-T6], first attempt
Working directory: WORKTREE

This artifact records a failed Phase 7 attempt and the analysis that established its cause. The Phase 7
restart rule was then applied and the loop was restarted from P7-T1. It is filed under `evidence/other/`
rather than `evidence/qa-gates/` because it records a failed attempt, not a passing gate.

## What failed

Command:

```
pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\post-change.cobertura.xml
```

EXIT_CODE: 1

```
Test Run Failed.
Total tests: 6924
     Passed: 6923
     Failed: 1
 Total time: 28.9989 Seconds
```

The wrapper then threw at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:236`:
`MSTest with coverage failed with exit code 1`.

Total tests rose from the baseline's 6912 to 6924, an increase of exactly 12: the seven queue-level
tests added by P5-T2 through P5-T8 plus the five ordering tests in the new file.

## The single failure

`UtilitiesCS.Test.OutlookObjects.FilterDASL.DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole`,
at `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs:115`.

Failure message:

```
Expected writer.ToString() "" to contain "AND".
```

The captured writer was empty, and the expected text appeared instead in the run's Debug Trace section:

```
AND
  A
  B
```

## Diagnosis: a pre-existing parallelism flake in a different assembly

The test redirects process-global `Console.Out` to its own `StringWriter` at
`DASLFilterParserTests.cs:102`, calls `parser.PrintTree(...)`, restores the original writer in a
`finally` at `:111`, and asserts on what its writer captured.

`Console.SetOut` is process-wide, not per-test. More than thirty test classes in `UtilitiesCS.Test` call
`Console.SetOut(new DebugTextWriter())` in their `[TestInitialize]`. `TaskMaster.runsettings` sets a
class-level parallel scope, so when one of those classes initializes while this test holds its redirect,
the sibling's `DebugTextWriter` replaces this test's `StringWriter` mid-test. The output then goes to
Debug — exactly what the Debug Trace above shows — and the assertion sees an empty string.

This hazard is already known and already documented in this repository. The sibling class
`UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` carries `[DoNotParallelize]` at line 19 with a
four-line comment at lines 14-18 explaining precisely this mechanism. `DASLFilterParserTests` uses the
same `Console.SetOut` pattern but carries no `[DoNotParallelize]` attribute, so it retains the
unmitigated latent flake.

## Evidence that it is not a regression from this change

1. **It passes in isolation.** A scoped single-assembly run of the test alone exits 0 with one passed
   result and zero failures:

   ```
   vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~PrintTree_WritesIndentedTreeToConsole" ...
   VSTEST_EXIT: 0
   OUTCOME_PASSED_COUNT: 1
   OUTCOME_FAILED_COUNT: 0
   ```

   A genuine regression would not pass when run alone.

2. **No dependency path reaches it.** The failing test lives in `UtilitiesCS.Test` and exercises
   `DASLFilterParser`, a DASL filter-string parser. This change touches only
   `QuickFiler/Controllers/FilerQueue.cs`, `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
   and three files under `QuickFiler.Test/`. Nothing in the change is referenced by `UtilitiesCS`,
   which sits below `QuickFiler` in the dependency graph.

3. **The mechanism is scheduling, and this change perturbs scheduling.** Adding 12 tests changes how
   work is distributed across the 24 parallel workers, which changes which class happens to initialize
   while the DASL test holds its redirect. That makes a latent order-dependent defect more or less
   likely to manifest on any given run; it does not create one.

## Action taken

No attempt was made to fix this test. Two independent constraints forbid it: the delegating instruction
not to fix unrelated pre-existing failures, and AC16, which confines the diff to the two named
production files, `QuickFiler.Test/`, and `docs/`. `UtilitiesCS.Test/` is outside the authorized blast
radius, so adding `[DoNotParallelize]` to `DASLFilterParserTests` — which is the fix this defect needs —
would itself violate an acceptance criterion.

Per the Phase 7 restart rule, the loop was restarted from P7-T1. The second attempt's artifacts carry
later timestamps and are the ones the AC19 single-uninterrupted-pass claim rests on.

This defect is reported to the delegating agent as a candidate for its own issue: `DASLFilterParserTests`
should carry `[DoNotParallelize]` for the same reason `PrettyPrint_Tests` already does.
