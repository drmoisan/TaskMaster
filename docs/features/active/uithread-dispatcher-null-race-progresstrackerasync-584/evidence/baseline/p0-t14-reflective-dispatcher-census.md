# P0-T14 — Reflective `Dispatcher` property and `UiThread` type census

Timestamp: 2026-09-03T21-31

Command:
```text
env -C <worktree-root> git grep -n -F '"Dispatcher"' -- QuickFiler.Test SVGControl.Test Tags.Test TaskMaster.Test TaskTree.Test TaskVisualization.Test ToDoModel.Test UtilitiesCS.Test VBFunctions.Test
env -C <worktree-root> git grep -n -F 'typeof(UiThread)' -- '*.cs'
env -C <worktree-root> git grep -n -F '"Dispatcher"' -- '*.cs'
```

EXIT_CODE:
- command 1 — 0
- command 2 — 0
- command 3 — 0

Aggregate EXIT_CODE: 0

## Output Summary

### REFLECTIVE_PROPERTY_NAME_HITS:

Complete verbatim output of command 1:

```text
QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:14:    /// against a real, running WPF <see cref="Dispatcher"/> hosted on a dedicated STA thread (the
QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:35:                "Dispatcher",
```

Classification, one line per hit:

- `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:14` — DOC-COMMENT. The occurrence is the XML
  `<see cref="Dispatcher"/>` cross-reference in that file's class summary. It invokes nothing.
- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:35` — REFLECTION. The occurrence is the
  name operand of the `typeof(UiThread).GetProperty(` call that begins on line 34.

Clause 1 result: two lines, in exactly the two files the task names. The FILE SET matches. The line
numbers observed (14 and 35) are identical to the round-15 readings the task records, so no
line-number deviation had to be recorded. No unlisted file appears and neither listed file is
missing.

### REFLECTIVE_UITHREAD_TYPE_HITS:

Complete verbatim output of command 2:

```text
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:135:            FieldInfo field = typeof(UiThread).GetField(
QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:34:            typeof(UiThread).GetProperty(
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:469:            var uiThreadType = typeof(UiThread);
UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144:            return typeof(UiThread).GetField(
UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138:                var dispatcherField = typeof(UiThread).GetField(
UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:421:            var dispatcherField = typeof(UiThread).GetField(
UtilitiesCS.Test/Threading/UiThread_Tests.cs:127:            return typeof(UiThread).GetField(
```

Classification, one line per hit. Each classification was read from the member-name operand on the
line immediately following the reflection call.

- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:135` — FIELD:_dispatcher
- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:34` — PROPERTY:Dispatcher
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:469` — FIELD:_uiSyncContext
- `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144` — FIELD:_dispatcher
- `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138` — FIELD:_dispatcher
- `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:421` — FIELD:_dispatcher
- `UtilitiesCS.Test/Threading/UiThread_Tests.cs:127` — FIELD:_dispatcher

Clause 2 result: seven lines, in exactly the seven files the task names, and exactly one of the
seven is classified `PROPERTY:`. That one file is
`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`, which is the file P2-T4 repairs.

`UtilitiesCS.Test/Threading/UiThread_Tests.cs` appears in this set because P1-T2 added the
`DispatcherField()` helper to it. It is not present at BASE, and this census describes the tree as
it stands when this task runs.

`UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` is unrelated to this fix. It
reflects over `_uiSyncContext`, a different static member of `UiThread`, and is recorded here so a
reviewer can see it was examined rather than overlooked.

### REPOSITORY_WIDE_PROPERTY_NAME_HITS:

Complete verbatim output of command 3:

```text
QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:14:    /// against a real, running WPF <see cref="Dispatcher"/> hosted on a dedicated STA thread (the
QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:35:                "Dispatcher",
UtilitiesCS/Threading/IUiDispatcher.cs:13:    /// forwards each member 1:1 to the underlying WPF <see cref="Dispatcher"/>.
UtilitiesCS/Threading/ThreadMonitor.cs:25:    /// <see cref="LockupStallDecider"/>, so it is unit-testable without a live <see cref="Dispatcher"/>
UtilitiesCS/Threading/WpfUiDispatcher.cs:14:    /// against a real, running <see cref="Dispatcher"/> hosted on a dedicated STA
```

Classification, one line per hit:

- `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:14` — DOC-COMMENT
- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:35` — REFLECTION
- `UtilitiesCS/Threading/IUiDispatcher.cs:13` — DOC-COMMENT
- `UtilitiesCS/Threading/ThreadMonitor.cs:25` — DOC-COMMENT
- `UtilitiesCS/Threading/WpfUiDispatcher.cs:14` — DOC-COMMENT

Clause 3 result: five lines, in exactly the five files the task names — the two files clause 1
names plus `UtilitiesCS/Threading/WpfUiDispatcher.cs`, `UtilitiesCS/Threading/ThreadMonitor.cs`, and
`UtilitiesCS/Threading/IUiDispatcher.cs`. All three added files are XML `<see cref="Dispatcher"/>`
cross-references to the WPF type and none invokes anything.

## Conclusion

No production file in this repository reads `UiThread.Dispatcher` reflectively. The blast radius of
P2-T1's guard through the reflective route is therefore confined to the single test file P2-T4
repairs, `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`.

This conclusion rests on the recorded output of commands 2 and 3 together, and both are
repository-wide over `.cs` files:

- Command 3's recorded output shows that the only `.cs` occurrences of the reflective name operand
  `"Dispatcher"` outside the one repaired test file are four documentation cross-references
  (`WpfUiDispatcherTests.cs:14`, `IUiDispatcher.cs:13`, `ThreadMonitor.cs:25`,
  `WpfUiDispatcher.cs:14`), none of which invokes reflection.
- Command 2's recorded output shows that the reflection entry point `typeof(UiThread)` occurs in
  seven files, all seven inside test assemblies, and in no production file at all.

Together the two recorded outputs cover every `.cs` file the conclusion speaks about.

## Acceptance

Clause 1 satisfied: two lines, two files, the stated file set, classified DOC-COMMENT and REFLECTION.
Clause 2 satisfied: seven lines, seven files, the stated file set, exactly one classified `PROPERTY:`.
Clause 3 satisfied: five lines, five files, the stated file set, three added files all DOC-COMMENT.

No BLOCKED condition applies.
