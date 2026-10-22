# Phase 2 — AC5 boundary behaviour tests (fail-before)

Timestamp: 2026-09-07T02-08
Task: [P2-T9] [expect-fail]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens, including inside quoted stack traces.

## Tests added

All six sit under `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests`:

- `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionSucceeds_InvokesNeitherSink`
- `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_InvokesLogSinkOnce`
- `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_InvokesPresentationSinkOnce`
- `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_DoesNotPropagateToCaller`
- `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate`
- `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail`

The sixth asserts the message passed to the presentation sink contains the inner exception's message
and is not the literal `One or more errors occurred.` alone.

The TaskMaster test assembly does not reference the QuickFiler assembly, and no test in this file
depends on any QuickFiler type: the boundary is exercised entirely through injected sinks and a
supplied `Func<Task>`.

## Build

Command: msbuild TaskMaster.Test\TaskMaster.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Test run

Command: `<vstest> TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t9 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~RibbonCommandBoundaryTests&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p2-t9\<user>_<machine>_2026-09-07_02_08_31_net481.trx`

- total: 6
- passed: 1
- failed: 5
- total run time: 2.1238 s

## Observed status

| Test | Observed status | Duration |
|---|---|---|
| `RunAsync_ActionSucceeds_InvokesNeitherSink` | Passed | 48 ms |
| `RunAsync_ActionThrows_InvokesLogSinkOnce` | Failed | 113 ms |
| `RunAsync_ActionThrows_InvokesPresentationSinkOnce` | Failed | 1 ms |
| `RunAsync_ActionThrows_DoesNotPropagateToCaller` | Failed | 12 ms |
| `RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate` | Failed | 1 ms |
| `RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail` | Failed | 1 ms |

`RunAsync_ActionSucceeds_InvokesNeitherSink` is recorded as passed and the other five as failed,
matching the plan.

Failure messages, read from the TRX and quoted verbatim:

```
Expected logged to contain a single item because the boundary must log the failure exactly once, but the collection is empty.
```

```
Expected presented to contain a single item because the user must be told once that the command failed, but the collection is empty.
```

```
Did not expect any exception because an async void ribbon callback cannot observe a propagated failure, but found System.InvalidOperationException: command failure
   at ...
   at TaskMaster.RibbonCommandBoundary.<RunAsync>d__3.MoveNext() in <repo-root>\TaskMaster\Ribbon\RibbonCommandBoundary.cs:line 58
```

```
Did not expect any exception because a failing presentation sink must not escape the boundary, but found System.InvalidOperationException: command failure
   at ...
   at TaskMaster.RibbonCommandBoundary.<RunAsync>d__3.MoveNext() in <repo-root>\TaskMaster\Ribbon\RibbonCommandBoundary.cs:line 58
```

```
Expected presented to contain a single item because the failure must be presented once, but the collection is empty.
```

Every failure has the same root cause and it is the one the plan predicts: the pass-through
`RunAsync` created by P1-T5 awaits the action and does not catch, so neither sink is invoked and the
exception escapes to the caller. Line 58 of `TaskMaster/Ribbon/RibbonCommandBoundary.cs`, named in
two of the messages, is the unguarded `await action();`.

None of the failures is incidental. The two propagation tests failed on their `NotThrowAsync`
assertion with the injected command failure, and the three sink tests failed on an empty sink
collection rather than on a wrong-content assertion. No test hung; the four-minute blame hang timeout
did not fire.

Output Summary: 6 total, 1 passed, 5 failed. `RunAsync_ActionSucceeds_InvokesNeitherSink` passed; the
other five failed because the boundary does not yet catch, which is the AC5 fail-before condition.
EXIT_CODE 1 matches ExpectedExitCode 1.
