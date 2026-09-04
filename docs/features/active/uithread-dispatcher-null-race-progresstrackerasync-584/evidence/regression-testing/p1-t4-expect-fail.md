# P1-T4 [expect-fail] — The two new tests against the UNFIXED production code

Timestamp: 2026-09-03T08-31

Command:
```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p1-t4 /TestCaseFilter:"FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize|FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance"
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

Console summary block, verbatim:

```text
Test Run Failed.
Total tests: 2
     Passed: 1
     Failed: 1
 Total time: 1.7757 Seconds
```

- **Total tests: 2**
- **Passed: 1**
- **Failed: 1**

`Failed` is read from the console here, which is correct for this task: the aggregate `Failed:` line
is printed precisely because this run's failure counter is non-zero, by construction. Constraint 5 of
"Shell constraints measured in this worktree" restricts console-sourced `Failed` counts to exactly
this case.

### Failing test

`Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`

Verbatim FluentAssertions failure message:

```text
Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.
```

The failure is a runtime assertion failure at
`UtilitiesCS.Test/Threading/UiThread_Tests.cs:line 150`, inside
`FluentAssertions.Specialized.DelegateAssertions.Throw<TException>`. It is not a compile failure —
P1-T3 recorded a clean `0 Error(s)` build immediately before this run — and it is not a harness
failure, because the sibling positive test passed in the same run.

### Passing test

`Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance` — **Passed [23 ms]**.

Its passing here is expected and required: it proves the reflection arrangement (`typeof(UiThread)
.GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Static)`), the field write, and the
`finally` restore path all work against the unfixed production code. The red in the negative test is
therefore attributable to the defect in the `Dispatcher` accessor and not to the test harness.

### Fail-before evidence established

Against the unfixed accessor `get => _dispatcher;`, reading `UiThread.Dispatcher` with a null backing
field returns `null` silently instead of throwing. That is the defect issue #584 reports: the
`NullReferenceException` then surfaces later and elsewhere, at the consumer's dereference site, with
no indication that `UiThread.Init()` was never called.

Results-file reference is deliberately redacted: the TRX under `TestResults/p1-t4/` is identified by
its repository-relative results directory only. `vstest.console.exe` composes the default TRX
filename from the host account name and the machine name and prints it inside a full absolute host
path, so neither the filename nor the `Results File:` console line is recorded here.
