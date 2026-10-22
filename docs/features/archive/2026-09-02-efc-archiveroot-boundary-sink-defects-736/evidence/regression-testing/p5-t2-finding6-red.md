# P5-T2 — Finding 6 deliberate-stop rewrite: recorded RED before the seam exists

Timestamp: 2026-09-04T00-14

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcDataModelArchiveRootTests.MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce" "/Logger:trx;LogFileName=p5-t2.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p5-t2
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 1
ExpectedExitCode: 1

## TRX results

Total **1**, passed **0**, failed **1**.

## The recorded failure message

```
Test method QuickFiler.Test.Controllers.EfcDataModelArchiveRootTests.MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce
threw exception: System.NullReferenceException: Object reference not set to an instance of an object.
```

`NullReferenceException` is the incidental collaborator crash the rewrite exists to stop depending
on. It is raised several frames downstream at EmailFiler.cs:133, where
`MailHelpers.FirstOrDefault()!.FolderInfo!.OlFolder!` dereferences a `FolderInfo` the test's
`TestableEfcDataModel` leaves null. It has nothing to do with archive-root resolution. Now that the
test no longer asserts that exception, and the filer-invocation seam does not yet exist, the crash
surfaces as an unhandled test failure — which is the correct red for this step.

## P5-T1's recorded observations

P5-T1 is a source edit that writes no evidence artifact of its own. Its four counted observations,
measured in `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` after the rewrite:

| Observation | Value |
|---|---|
| Occurrences of the token `NullReferenceException` | **0** |
| Occurrences of the fixed string `null reference` | **0** |
| Occurrences of the token `ThrowAsync<COMException>` | **1** (unchanged) |
| `[TestMethod]` count | **11** (unchanged) |

The now-unused `Func<Task> act` local was removed, the awaited call to the shared `MoveAsync` helper
is direct, and `olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once());` is the method's
only assertion. The test's XML summary was rewritten to describe the deliberate stop at the filer
seam and to cite issue #699's framing, and no longer describes the incidental downstream crash as
the barrier.

## TRX inventory

Exactly **one** TRX file exists under this task's results directory: `p5-t2.trx`. The empty MSTest
deployment directory the failing run created beside it was removed immediately afterwards, for the
D10 reason recorded in P1-T7.

Output Summary: build exited 0; the single-method run exited 1 as expected with TRX total 1, passed
0, failed 1, and the failure names `System.NullReferenceException`. P5-T1 left zero
`NullReferenceException` tokens and zero `null reference` phrases in the test file, kept the single
`ThrowAsync<COMException>` occurrence, and kept the `[TestMethod]` count at 11.
