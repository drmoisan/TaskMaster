# Baseline — #584 Plan Line Re-derivation (P0-T10, SD11 item 2, AC12)

Timestamp: 2026-09-05T19-38

Command:

```text
Read docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md lines 936-946
Read docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md lines 1064-1086
```

EXIT_CODE: 0

Output Summary:

## Line 941, verbatim

```text
  2. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` is exempt from clause 1 because it is 514 lines at BASE, above the limit before this plan touches it. Its acceptance is instead that its post-change count is less than or equal to its P0-T13 baseline count plus 1. The plan's intent is a count unchanged at 514, achieved by the combined attribute list in P1-T5; the plus-one tolerance exists solely because a later `csharpier format .` pass may split that attribute list onto two lines, which is a formatter decision this plan does not control. The artifact MUST carry the line `PRE-EXISTING FILE-SIZE OVERAGE: UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` and state that the overage exists at BASE and is not introduced by this change. If the post-change count exceeds baseline plus 1, that is a real regression in this file and the task fails.
```

The line contains the token `PRE-EXISTING FILE-SIZE OVERAGE:` and it contains the phrase stating
that the post-change count must be no greater than the P0-T13 baseline count plus one. Both
acceptance conditions on this location hold.

## Lines 1068-1084, verbatim

The P4-T1 task line and its command block:

````text
- [x] [P4-T1] Format, with the formatter's write scope restricted to the six paths this plan owns. Run, from the worktree root:

  ```text
  git status --porcelain
  dotnet tool run csharpier format UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
  git status --porcelain
  ```

  This plan's owned file set is exactly the six paths named on that command line:
````

The six-path owned-file list at lines 1078-1083, verbatim:

```text
  - `UtilitiesCS/Threading/UiThread.cs`
  - `UtilitiesCS.Test/Threading/UiThread_Tests.cs`
  - `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`
  - `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`
  - `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`
  - `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`
```

Line 1085 records why the sixth operand is double-quoted: its directory is spelled `Helper Classes`,
and an unquoted operand would be split by the shell into two paths that do not exist.

## Result

The command recorded at lines 1068-1084 is a `dotnet tool run csharpier format` invocation whose
operands are six explicit paths. It does **not** carry `.` as its operand. Both acceptance
conditions on this location hold.

This artifact is the sole basis on which Phase 5 may quote these two locations.
