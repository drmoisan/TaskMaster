# Phase 5, attempt 1 — aborted at P5-T6 (environmental), diagnosis and restart

Timestamp: 2026-08-08T21-30

This artifact records an **aborted** Phase 5 pass and the diagnosis that led to restarting the
phase at P5-T1. It is not a QA gate. The authoritative Phase 5 evidence is the second,
uninterrupted pass recorded in `<FEATURE>\evidence\qa-gates\` at timestamps `2026-08-08T21-3x`,
summarized by `toolchain-clean-pass.<TS>.md`.

## What happened

Attempt 1 ran P5-T1 (format, EXIT 0), P5-T2 (repo-wide check, EXIT 0), P5-T3 (size audit, PASS),
P5-T4 (analyzer `/t:Rebuild`, EXIT 0, 18 `csc.exe`), and P5-T5 (type-check `/t:Rebuild`, EXIT 0).
P5-T6 then failed:

```
Discovered 9 test assemblies.
Total tests: 6435   Passed: 6433   Failed: 2
Test Run Failed.
```

The two failures:

```
QuickFiler.Controllers.Tests.QfcItemController_InitializationTests.InitializeBool_ThroughThePumpHost_CompletesAndInitializesState
QuickFiler.Controllers.Tests.QfcItemController_InitializationTests.InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates
```

Both with the same exception:

```
System.InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the
window handle has been created.
   at System.Windows.Forms.Control.MarshaledInvoke(...)
   at QuickFiler.Controllers.QfcItemController.InvokeBeginInvoke(...)  QfcItemController.FocusAndTheme.cs:256
   at QuickFiler.Controllers.QfcItemController.Initialize(Boolean async) QfcItemController.Initialization.cs:185
   at QuickFiler.Test.TestSupport.WinFormsPumpHost.<>c__DisplayClass19_0.<InvokeAsync>b__0(...)  WinFormsPumpHost.cs:95
```

Neither test is a member of the P0-T10 recorded pre-existing set, so the plan's rule required
investigation before proceeding.

## Diagnosis: environmental, not a regression from this change

Four independent lines of evidence:

1. **No dependency path exists.** `QuickFiler.csproj` references only `SVGControl`,
   `TaskVisualization`, `ToDoModel`, and `UtilitiesCS`. `QuickFiler.Test.csproj` references only
   `QuickFiler`, `UtilitiesCS`, and `TaskVisualization`. **Neither references `TaskMaster`.** This
   change touches only `TaskMaster` and `TaskMaster.Test`, so there is no mechanism by which it can
   alter `QuickFiler.Test` behavior.
2. **The failing binaries are merge-base binaries.** `QuickFiler` and `QuickFiler.Test` have a
   zero-line diff against `<MERGE_BASE>`, so the assemblies under test were compiled from source
   identical to the merge-base.
3. **The failure reproduces with none of this change loaded.** Running
   `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation` with a filter
   selecting only those two tests failed 4 times out of 4. In that configuration neither
   `TaskMaster.dll` nor `TaskMaster.Test.dll` is loaded at all.
4. **The failure is load-driven and reverses when load is removed.** These tests drive a real
   WinForms message pump through `WinFormsPumpHost` against a real `ItemViewer`; the failure is the
   classic handle-creation race, and this test class is known to be CPU-load sensitive. Seventeen
   idle `MSBuild.exe` node-reuse processes from the P5-T4/P5-T5 `/m` parallel rebuilds were still
   resident, on a 24-logical-processor box reporting 62% load. After terminating those node-reuse
   processes, the identical isolated command **passed 4 times out of 4**:

   ```
   --- run 1 (exit 0) --- Total tests: 2   Passed: 2
   --- run 2 (exit 0) --- Total tests: 2   Passed: 2
   --- run 3 (exit 0) --- Total tests: 2   Passed: 2
   --- run 4 (exit 0) --- Total tests: 2   Passed: 2
   ```

Conclusion: the two failures are an environmental flake in a test class this change does not
touch and cannot reach. Consistent with this, the P0-T9 merge-base full-suite run passed both
(6399/6399).

## Action taken

The cause (resident MSBuild node-reuse processes saturating the box) was removed rather than
worked around. No test was weakened, no assertion relaxed, no retry or sleep added, and no
`QuickFiler` source was modified — that would be out of scope under plan rule 17.

Per the Phase 5 loop rule ("if any task fails, fix the cause and restart the phase from P5-T1"),
the phase was restarted from P5-T1. Attempt 1's P5-T1, P5-T2, and P5-T3 artifacts
(timestamps `2026-08-08T21-19`, `21-20`, `21-21`) and P5-T4/P5-T5 artifacts (`21-23`, `21-24`) are
**superseded** by the second pass and are retained only as an audit trail.

## Follow-up disposition

The load-sensitivity of `QfcItemController_InitializationTests` is a pre-existing property of the
#230 `WinFormsPumpHost` test seam, outside the scope of #505/#506/#518. It is recorded as an
out-of-scope observation in `<FEATURE>\evidence\issue-updates\research-defect-promotions.<TS>.md`
for promotion by the orchestrator, not fixed here.
