# Phase 5 Flag-and-Stop Deviation — ProjectEntry change-confirmation branch

- Timestamp: 2026-06-14T15-10
- Task: [P5-T3] (ToDoModel.Test ProjectEntry dialog branches)
- Status: PARTIAL — malformed-ID branch and CompareTo length tie-break covered; change-confirmation branch FLAGGED-AND-STOPPED (not covered).

## Summary

P5-T3 intended to cover three groups of `ProjectEntry` dialog-dependent branches via the
authorized `MyBox.DialogInvoker` seam (exposed to `ToDoModel.Test` by the P5-T2 UtilitiesCS
`InternalsVisibleTo` attribute):

1. `SetProjectId` malformed-ID validation (`newID.Length != 4`) — COVERED.
2. `SetProjectId`/`ChangeId` change-confirmation (Yes/No, with/without `_idUpdate`) — NOT COVERED (flag-and-stop).
3. `CompareTo(IProjectEntry)` length tie-break (shorter/longer comparand) — COVERED.

Groups 1 and 3 are reachable through the authorized seam and are covered by
`ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs` (3 passing tests).

## Why the change-confirmation branch cannot be covered within the authorized scope

`SetProjectId` routes a valid-id-to-different-valid-id change through `ProjectEntry.ChangeId`
(`ToDoModel/Data Model/Project/ProjectEntry.cs` lines 141-170). `ChangeId` uses the seamed
`MyBox.ShowDialog` for its own confirmation prompts, but it COMMITS the change by assigning
`ProjectID = newID` (line 166). That assignment executes the `ProjectID` **property setter**
(lines 36-77). The setter's `else if (_projectID != value)` arm (lines 49-76) calls a RAW,
un-seamed `System.Windows.Forms.MessageBox.Show(...)` — it does NOT go through the
`MyBox.DialogInvoker` seam:

```
else if (_projectID != value)
{
    var response = MessageBox.Show(   // raw WinForms modal, not seamed
        $"Are you sure you want to change {nameof(ProjectID)} from{_projectID} to {value}",
        "Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    ...
}
```

Because this MessageBox is raw, injecting the `MyBox.DialogInvoker` stub cannot suppress it.
Any test that drives the change-confirmation path to commit a changed id triggers a real modal
dialog on the STA test thread with no message pump to dismiss it, which deadlocks the test host.

### Verification

- `vstest.console.exe ToDoModel.Test.dll /InIsolation /TestCaseFilter:"...SetProjectId_MalformedId..."`
  passed in 553 ms (malformed branch — returns false before assigning ProjectID, no setter MessageBox).
- `vstest.console.exe ... /TestCaseFilter:"...SetProjectId_ChangeConfirmedYes_NoUpdateAction..."`
  timed out (EXIT 124) — hung on the raw `MessageBox.Show` in the property setter when committing
  `ProjectID = "WXYZ"`.
- The same `MyBox`-seam pattern runs cleanly in `UtilitiesCS.Test/Dialogs/MyBox_ShowDialog_Tests.cs`
  (15 tests, ~2 s), confirming the hang is the un-seamed property-setter MessageBox, not the seam.

## Authorized-scope boundary

Covering the change-confirmation branch would require replacing the raw `MessageBox.Show` calls in
the `ProjectID` property setter (lines 40-43, 51-57, 62-68) with the `MyBox` seam — a THIRD
production change in a THIRD production file, beyond the two seams authorized for Phase 5
(`UtilitiesCS/Properties/AssemblyInfo.cs` attribute and the `AppFileSystemFolderPaths`
pure-helper extraction). Per the plan Flag-and-Stop Rule and the Phase 5 hard constraint limiting
production changes to the two authorized seams, this change was NOT made.

## Recommended follow-up (maintainer direction required)

Route the `ProjectID` property setter's confirmation/validation dialogs through `MyBox.ShowDialog`
(matching `SetProjectId`/`ChangeId`), then add change-confirmation coverage. This is a separate,
maintainer-authorized production change and is out of scope for the two seams approved for Phase 5.

## Net effect on AC1

AC1's previously-deferred `ProjectEntry` dialog gaps are now PARTIALLY closed by Phase 5: the
malformed-ID dialog branch and the CompareTo length tie-break are covered; the change-confirmation
branch remains an authorized-scope flag-and-stop pending the follow-up above.
