# P8-T2 — Rebuild, then scoped verification of the re-pinned search-dismissal class

Timestamp: 2026-09-07T15-01
Task: [P8-T2]
Issue: #796
Channel used: A

## Step 1 — mandatory rebuild

The rebuild is part of this task and is not optional. P8-T1 edits test source, so a run
against QuickFiler.Test/bin/Debug/QuickFiler.Test.dll as task P7-T4 left it would measure
a stale binary and report the pre-update result.

Command: the P0-T8 command form with the log path
`TestResults\796\p8-t2\analyzer-rebuild.log`.

RunStartedUtc: 2026-09-07T19:00:52.2216349Z

EXIT_CODE: 0

Build summary, verbatim:

```
    3 Warning(s)
    0 Error(s)

Time Elapsed 00:00:18.65
```

CscTaskCount=36
CscToolCount=36

| Assembly | LastWriteTimeUtc | At or later than RunStartedUtc |
|---|---|---|
| QuickFiler/bin/Debug/QuickFiler.dll | 2026-09-07T19:01:01.2395024Z | yes |
| QuickFiler.Test/bin/Debug/QuickFiler.Test.dll | 2026-09-07T19:01:05.1808112Z | yes |

Both touched assemblies were rebuilt after RunStartedUtc, so the run below measured the
post-P8-T1 binary rather than the one Phase 7 left.

The three warnings are recorded here rather than passed over. All three are `MSB3061`
raised by the `CoreClean` target of TaskMaster/TaskMaster.csproj, reporting that three
native payload files under TaskMaster/bin/Debug could not be deleted because a running
Microsoft Outlook process holds them open. None is an analyzer diagnostic, none names a
source file, and none arises in QuickFiler or QuickFiler.Test. The immediately preceding
P8-T1 run of the identical command form recorded 0 Warning(s), which places the cause
outside this item's diff. This task's acceptance turns on `EXIT_CODE: 0` only and makes
no warning-total comparison; the warning-total comparison against the P0-T8 baseline is
made at task P9-T3.

Raw log (gitignored): TestResults/796/p8-t2/analyzer-rebuild.log

## Step 2 — scoped run

Command: the P1-T11 command form with the results directory `TestResults\796\p8-t2`, the
log file name `p8-t2.trx`, and the filter
`FullyQualifiedName~QfcItemController_SearchDismissalTests`.

EXIT_CODE: 0

Run summary, verbatim:

```
Test Run Successful.
Total tests: 6
     Passed: 6
 Total time: 1.5434 Seconds
```

Total is 6, which is this class's `[TestMethod]` count. A Total of 0 would mean the
filter selected nothing and any other Total would mean the class's membership changed;
neither occurred.

## Named results, all six

```
Passed TextBoxSearchKeyDown_EscapeWhileDropDownOpen_RoutesExactlyOneCloseIntent [234 ms]
Passed TextBoxSearchKeyDown_EscapeWhileDropDownClosed_RoutesNoIntentAndLeavesKeyUnhandled [1 ms]
Passed TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent [< 1 ms]
Passed TextBoxSearchLeave_WhileDropDownClosed_RoutesNoIntent [< 1 ms]
Passed TextBoxSearchLeave_AfterDownArrowHandoff_SuppressesExactlyOneClose [< 1 ms]
Passed TextBoxSearchKeyDown_DownArrow_StillOpensAndFocusesTheDropDown [< 1 ms]
```

`TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent` is named explicitly
among the Passed results above rather than inferred from the absence of a failure. No
test is recorded as Failed. No expect-fail test is declared in this class anywhere in
this plan, so no carve-out applies and any Failed result would have failed this gate
outright.

## The gate was demonstrably able to fail

Before P8-T1, this exact class was measured RED. The whole-assembly run recorded in
evidence/other/phase7-blocking-finding-out-of-write-set-test.md reports Total 6, Passed
5, Failed 1, with `TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent`
failing on `Expected invocation on the mock once, but was 0 times`. Only the P8-T1
update turns it green, and a later regression in the AC4 latch turns it red again.

Raw TRX (gitignored, never committed): TestResults/796/p8-t2/p8-t2.trx

Output Summary: rebuild EXIT_CODE 0 with both touched assemblies refreshed; scoped run
EXIT_CODE 0, Total 6, Passed 6, Failed 0, with the re-pinned test named explicitly among
the Passed results. AC4 may now be checked off at P8-T6.
