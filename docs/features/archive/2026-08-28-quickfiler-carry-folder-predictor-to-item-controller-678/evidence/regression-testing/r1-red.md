# R1 — Red run (expect-fail)

- Timestamp: 2026-09-02T01-15
- Issue: #678
- Task: [P1-T2] `[expect-fail]`
- Test: `RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary`

Command (Derivation D7; this is the invocation the `EXIT_CODE:` below reports):

```
vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary" /Logger:trx "/ResultsDirectory:TestResults\p1-t2"
```

EXIT_CODE: 1
ExpectedExitCode: 1

`TestResults\p1-t2` was deleted before the run, so exactly one TRX exists in it and an
"exactly one TRX" reading cannot be confused by a re-run's second timestamped file. The TRX
file name is redacted below because it embeds the host account and machine name.

## Clause 1 — the pre-run build exits 0

Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
→ exit code **0**.

Recorded here inside `Output Summary:` rather than as the artifact's own `Command:` and
`EXIT_CODE:`, because `ExpectedExitCode:` is a per-file field: a build recorded as the
artifact's command would be normalised against the declared expectation of 1 and a
successful build would then read as a failure.

Without this step the scoped run would read whatever `QuickFiler.Test.dll` a previous task
produced, and a newly added test would not be discovered at all. `/t:Build` rather than
`/t:Rebuild` is correct here because this is a build-for-test, not an analyzer or nullable
gate; MSBuild's up-to-date check does invalidate on a changed source timestamp, and the
vacuity hazard applies only to a `/p:` property change.

## Clause 2 — discovery control

```
A total of 1 test files matched the specified pattern.
Total tests: 1
```

Exactly **1** test was discovered and executed. This is the control that distinguishes a
real failure from a test that never ran: a filter that matched nothing would report 0 and
the run would exit non-zero for a different reason entirely.

## Clause 3 — the test is reported as failed

```
  Failed RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary [475 ms]
Test Run Failed.
     Failed: 1
 Total time: 1.6740 Seconds
```

## Clause 4 — the failure is a stage-two FluentAssertions failure on the captured carrier list

Recorded failure message, verbatim:

```
Expected loaded[0].MailItem to refer to Mock<MailItem:2>.Object because the substitute left
the master queue and is lost for the session unless it is displayed, but found
Mock<MailItem:1>.Object.
```

Stack frame, with the absolute host path replaced by the repository-relative path:

```
at QuickFiler.Controllers.Tests.QfcHomeControllerRunAsyncTests
   .<RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary>d__3.MoveNext()
   in QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs:line 225
```

This satisfies every part of the clause:

- It **is** a FluentAssertions assertion failure (`ReferenceTypeAssertions.BeSameAs`, thrown
  through `AssertionChain.FailWith`), and it is on `loaded`, which is the captured carrier
  list — that is, on a **stage-two** assertion.
- It is **not a stage-one assertion failure**. This is the load-bearing distinction: the four
  stage-one assertions all passed, which means the real `TryUnhookOrReplace` throw branch did
  produce the divergence (`Items = [substitute]`, `PreScored = [carrier(failed)]`). Had stage
  one failed, the test would prove nothing about leg A. Line 225 sits in the stage-two
  assertion block; the stage-one assertions end well before it.
- It is **not a build error**: the pre-run build exited 0 and the analyzer build at P1-T1
  also exited 0.
- It is **not an assembly-load error**: the runner reported
  `A total of 1 test files matched the specified pattern`, executed the test, and reported a
  475 ms duration rather than a sub-millisecond load failure with an empty message.
- It is **not a `NullReferenceException`**: the mocked `LoadItemsAsync` returns
  `Task.CompletedTask` and the `ProgressTracker` comes from `SetupMockProgressTracker`, so
  neither `RunAsync`'s first `progress.Report(0, ...)` statement nor the load call can
  dereference null.

`Mock<MailItem:1>` is the failed item (created first) and `Mock<MailItem:2>` is the
substitute. The message therefore states the R1 defect exactly: leg A displayed the item
whose `UnhookItem` call threw and dropped the substitute that had already left the master
queue.

## Output Summary

Pre-run build exit 0. The scoped run discovered and executed exactly 1 test and reported it
as failed; run exit code 1, which equals the declared `ExpectedExitCode`. The failure is a
FluentAssertions `BeSameAs` failure on `loaded[0].MailItem`, a stage-two assertion at
`QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs:225`, reporting that leg A displayed
the failed item instead of the substitute. All four stage-one assertions passed, so the
divergence the test asserts against was produced by the real `TryUnhookOrReplace` throw
branch. Exactly one TRX was written to `TestResults\p1-t2`.
