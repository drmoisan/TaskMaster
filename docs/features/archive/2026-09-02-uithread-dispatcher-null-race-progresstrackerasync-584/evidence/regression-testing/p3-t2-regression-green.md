# P3-T2 — The two new tests against the FIXED production code

Timestamp: 2026-09-03T08-34

Command:
```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p3-t2 /TestCaseFilter:"FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize|FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance"
```

EXIT_CODE: 0

## Output Summary

Console summary block, verbatim:

```text
Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.8335 Seconds
```

- **Total tests: 2** (console summary block)
- **Passed: 2** (console summary block)
- **Failed: 0** (read from the `failed` attribute of the single `<Counters .../>` element in the TRX
  written under `TestResults/p3-t2/`; a green run prints no aggregate `Failed:` line at all, per
  constraint 5 of "Shell constraints measured in this worktree")

TRX `<Counters .../>`: `total="2" executed="2" passed="2" failed="0"`.

This task records no `Skipped` figure, so the `total` minus `executed` derivation does not apply and
the `notExecuted` attribute was not read.

### Individual test outcomes (console-observed)

```text
  Passed Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize [99 ms]
  Passed Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance [24 ms]
```

- `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` — **Passed**.
  This test failed in P1-T4 against the unfixed accessor with
  `Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.` It now
  passes, and its `WithMessage("*UiThread.Initialize()*")` clause confirms the thrown message names
  `UiThread.Initialize()`.
- `Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance` — **Passed**, as it also did
  before the fix, confirming the fix did not change the populated-field path.

`TestResults/p3-t2/` held exactly one `.trx` at the moment this artifact was written, so no selection
between multiple files was required. The file is identified by its repository-relative results
directory only; its own name is not recorded and the run's `Results File:` console line is not
quoted.

## Acceptance

`EXIT_CODE: 0` as observed from the shell; `Total tests: 2` and `Passed: 2` as observed in the
console summary block; `Failed: 0` read from the TRX `failed` attribute. All three clauses satisfied.

Together with P1-T4 this establishes the fail-before / pass-after pair required by AC1.
