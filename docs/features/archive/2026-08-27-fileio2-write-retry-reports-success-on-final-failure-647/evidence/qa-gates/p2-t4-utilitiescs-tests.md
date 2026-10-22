# P2-T4 — UtilitiesCS.Test After the Seam Change

Timestamp: 2026-08-31T19-30
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /Logger:trx /ResultsDirectory:coverage\testresults\p2-t4-rerun
EXIT_CODE: 0
ExpectedExitCode: 0

`vstest.console.exe` is not on PATH and was resolved through `vswhere.exe` at the explicit Installer path, as the plan's execution rules require. `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook` were both passed. TRX output was directed to a per-task subdirectory under `coverage\testresults`, which is gitignored, so the raw TRX is transient and only the numeric summary is transcribed here.

## Accepted run

- Total: 4765
- Passed: 4765
- Failed: 0
- Skipped: 0
- Failed test names: none

`WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` is reported **Passed**, in 10 s. That duration is itself corroborating evidence that the seam preserved behavior: the pre-change loop performs 100 open attempts and 99 delays of 100 milliseconds, a window of approximately 9.9 seconds, and the post-seam run reproduces it. The delay now flows through the production default `delay ?? ((ms, t) => Task.Delay(ms, t))` rather than a direct `Task.Delay(100)`, and the writer now comes from the production default factory rather than a direct constructor call, yet the observable timing and the observable outcome are unchanged.

## Acceptance evaluation against the recorded baseline

The set of Failed test names is empty. `BASELINE_FAILURE_SET:` recorded in `evidence/baseline/p0-t19-baseline-failure-set.md` is the literal word `none`, so the required subset relation is the empty set being a subset of the empty set, which holds, and the clause that then applies requires `EXIT_CODE:` to be 0. It is 0.

CARRIED_BASELINE_FAILURES: not applicable. The recorded baseline is `none` rather than a name list, so no carried-failure branch is available and no non-zero test-run exit code was authorized. None was needed.

## First run of this task, recorded for completeness

An earlier invocation of the identical command, with TRX in `coverage\testresults\p2-t4`, reported Total 4765, Passed 4763, Failed 2, exit code 1. The two Failed tests were:

- `UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.MethodBodyReader_Tests.Constructor_WithSimpleMethod_ParsesInstructions` — `System.IndexOutOfRangeException` raised inside `SDILReader.MethodBodyReader.ReadInt32` while walking a method body's IL.
- `UtilitiesCS.Test.Extensions.AsyncSerialization_Tests.ReadTextAsync_WithLargeExistingFile_ReturnsTextAndReportsProgress` — `Expected progress.Reports not to be empty.`, a progress-report timing assertion.

Both were characterized rather than assumed. Re-run in isolation through the same runner with a `FullyQualifiedName` filter naming exactly those two methods, both **Passed**, in 46 ms and 491 ms respectively, with exit code 0. The full assembly was then re-run unchanged and both **Passed**, giving the accepted run above.

Attribution: neither test has any dependency on `FileIO2`, on the writer seam, or on any file in this change's footprint. `MethodBodyReader_Tests` reflects over IL in `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs`; `AsyncSerialization_Tests` asserts on `IProgress<T>` callback delivery. Both are load-sensitive under the assembly's `[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]` setting, which resolves Workers to the processor count — 24 on this machine, as the runner's own `Test Parallelization enabled ... (Workers: 24, Scope: ClassLevel)` line reports. Their pass-in-isolation and pass-on-rerun behavior against unchanged source is the evidence that they are load-sensitive rather than a regression introduced by the seam.

Output Summary: The seam preserved behavior. The full `UtilitiesCS.Test` assembly passes 4765 of 4765 with exit code 0, and the behavior-preservation test for the pre-fix contract passes.
