# Phase 2 — AC4 stack-preservation regression test (fail-before)

Timestamp: 2026-09-07T02-06
Task: [P2-T8] [expect-fail]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens, including inside quoted stack traces.

## Test added

`QuickFiler.Controllers.Tests.QfcDatamodelRethrowTests.GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack`

The datamodel is constructed with `FormatterServices.GetUninitializedObject`, the globals field is
set so the MAPI namespace reports offline and the offline toggle short-circuits without touching the
command bars, `Token` and `TokenSource` are set through their public setters, and
`GetEmailsInViewDfAsync` is reflection-invoked with an explorer whose table acquisition throws a
sentinel from a named test frame, `ThrowSentinelAtTableAcquisition`.

The assertion is shape-agnostic, as the plan requires. Wrapping occurs at the dataframe-transform
`TimeoutAfter` call, which is downstream of table acquisition, so a sentinel thrown at acquisition
reaches the boundary unwrapped while one thrown during the transform reaches it wrapped in an
`AggregateException`. The test therefore walks the thrown exception and every exception nested inside
it and requires the originating frame in at least one of their `StackTrace` values. It assumes
neither unwrapping nor wrapping.

## Build

Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Test run

Command: `<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t8 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcDatamodelRethrowTests&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p2-t8\<user>_<machine>_2026-09-07_02_06_37_net481.trx`

- total: 1
- passed: 0
- failed: 1
- duration: 367 ms
- total run time: 2.0998 s

## Observed failure

Status: **Failed**, as expected, because the originating frame is absent from the observed stack.

The failure message reports the complete set of stacks the assertion walked. There is exactly one
exception in the chain, and its stack begins at the rethrow site:

```
   at QuickFiler.Controllers.QfcDatamodel.<GetEmailsInViewDfAsync>d__47.MoveNext() in <repo-root>\QuickFiler\Controllers\QfcDatamodel.FrameBuilding.cs:line 108
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.GetResult()
   at QuickFiler.Controllers.Tests.QfcDatamodelRethrowTests.<GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack>d__6.MoveNext() in <repo-root>\QuickFiler.Test\Controllers\QfcDatamodelRethrowTests.cs:line 135
```

Line 108 of `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` is the `throw e;` statement that
P5-T1 changes to `throw;`. The observed stack therefore starts at the rethrow and carries no frame
from table acquisition: the sentinel's originating frame, and every frame between it and the
rethrow, have been discarded.

This is precisely the failure the plan predicts, and it is not incidental:

- the `observed.Should().NotBeNull(...)` assertion passed, so the sentinel did reach the caller and
  the arrangement drove the intended code path rather than failing earlier;
- the assertion that failed is the stack-content assertion;
- the chain contains one exception and no inner exception, so the shape-agnostic walk had nothing
  further to inspect.

The test did not hang; the four-minute blame hang timeout did not fire.

Output Summary: 1 total, 0 passed, 1 failed.
`GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack` failed because the observed
stack begins at the `throw e;` rethrow site and no longer contains the originating frame, which is
the AC4 fail-before condition. EXIT_CODE 1 matches ExpectedExitCode 1.
