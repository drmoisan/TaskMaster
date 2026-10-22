# P5-T4 — AC4 green transition (stack-preserving rethrow)

Timestamp: 2026-09-07T03-07
Task: [P5-T4]
Base commit: c431dc32
ExpectedExitCode: 1

The expectation is non-zero by design. Phase 5 turns the single AC4 member of
`BASELINE_FAILURE_SET` green and leaves its six AC5 members failing, because the ribbon command
boundary does not land until Phase 6. A zero exit code at this position in the plan would mean
the AC5 fail-before evidence had been invalidated.

Absolute host paths are redacted to `<repo-root>`, `<user>` and `<host>` tokens throughout.

---

## Step 1 — Formatting

Command: `dotnet tool run csharpier format .`

EXIT_CODE: 0

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Success-case summary line, quoted verbatim:

```
Checked 1593 files in 6574ms.
```

Output Summary: the check command exits 0 and reports 1593 files checked.

### Did the format pass rewrite any file?

No. Two independent observations establish this, because neither one alone is sufficient:
a `git status --porcelain` comparison cannot detect the rewrite of a file that was already
marked `M`, since it stays `M` either way, and the csharpier summary line reports the number of
files **scanned** rather than rewritten.

1. **Pre-format check.** `dotnet tool run csharpier check .` was run *before* the format pass and
   exited 0, reporting `Checked 1593 files in 6167ms.` A clean check means every file already
   matched formatter output, so the subsequent format pass had nothing to rewrite.
2. **Modification times.** The two files edited in this phase carry mtimes of `03:02:56`
   (`QuickFiler/Controllers/QfcDatamodel.cs`) and `03:02:42`
   (`QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`). Both predate the format pass, which
   ran at approximately `03:04`. Had the formatter written either file, its mtime would follow
   the format pass rather than precede it.

Because no file was rewritten, the toolchain loop did not restart at step 1.

---

## Step 2 — Analyzer build

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

EXIT_CODE: 0

Summary lines, quoted verbatim:

```
0 Warning(s)
0 Error(s)
```

Output Summary: `0 Error(s)`. The warning count is 0, matching the P0-T6 baseline of 0. No
diagnostic id increased.

`/t:Rebuild` was used, not `/t:Build`: MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so a warm `/t:Build` would return exit 0 with `CoreCompile` skipped on
every project and would run no analyzers.

---

## Step 3 — Nullable build

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true

EXIT_CODE: 0

Summary lines, quoted verbatim:

```
0 Warning(s)
0 Error(s)
```

Output Summary: `0 Error(s)`. The warning count is 0, matching the P0-T7 baseline of 0.

`/p:Nullable=enable` was not supplied. No project in this repository carries a `<Nullable>`
element and there is no repository-root build property file, so the property would be a
solution-wide opt-in conscripting every file that has never adopted the `#nullable enable`
pragma. CI omits it deliberately, and this command is character-for-character CI's.

---

## Step 4 — Test run over the three affected assemblies

Command: `<vstest>` `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p5-ac4 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"

EXIT_CODE: 1

The filter carries the shell-icon extension because P0-T8 recorded the verdict
`SHELL_ICON_EXCLUSION: REQUIRED`. `/InIsolation` is present because without it vstest runs
in-process, never loads each assembly's configuration file, and assemblies fail to load.

Results file: `<repo-root>\coverage\trx\p5-ac4\<user>_<host>_2026-09-07_03_06_39_net481.trx`

### Counts

| Metric | Value |
|---|---|
| Total tests | 6570 |
| Passed | 6564 |
| Failed | 6 |
| Skipped | 0 |
| Total time | 49.7864 seconds |

The run printed no `Skipped:` line, which for this reporter means a skipped count of 0.

Output Summary: 6570 total, 6564 passed, 6 failed, 0 skipped. Exit code 1, matching
`ExpectedExitCode`.

### Movement against the inherited baseline

The last run before this phase recorded 6570 total, 6563 passed, 7 failed. This run records one
additional pass and one fewer failure. The single test that changed status is the AC4 member.

### AC4 acceptance clause

```
Passed GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack [60 ms]
```

`QuickFiler.Controllers.Tests.QfcDatamodelRethrowTests.GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack`
now passes. P2-T8 recorded it as failing before P5-T1, because `throw e;` reset the stack and
erased the originating frame. Replacing it with `throw;` preserves the original stack, so the
originating frame is present in the thrown exception or one of its inner exceptions.

### Remaining failures — all six are AC5 members

```
Failed RunAsync_ActionThrows_InvokesLogSinkOnce [56 ms]
Failed RunAsync_ActionThrows_InvokesPresentationSinkOnce [1 ms]
Failed RunAsync_ActionThrows_DoesNotPropagateToCaller [8 ms]
Failed RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate [< 1 ms]
Failed RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail [1 ms]
Failed NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary [3 ms]
```

All six sit under `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests`. All six are members of
`BASELINE_FAILURE_SET` recorded by P2-T11, and all six are expected to fail at this position: the
first five require the catching boundary that P6-T1 implements, the sixth requires the inner
exception rendering that P6-T2 adds, and the shape pin requires the boundary field that P6-T3
adds. Phase 6 turns all six green.

No test outside `BASELINE_FAILURE_SET` failed. In particular the issue #780 sporadic
`TryAddValuesAsync_UpdatesExistingValue` did not fail in this run, so no rerun was required.

---

## Verdict

AC4 is delivered. The toolchain completed in a single pass with no restart: no step failed and
no step rewrote a file. The test-run exit code of 1 equals the declared `ExpectedExitCode` and is
attributable entirely to the six AC5 members that Phase 6 addresses.
