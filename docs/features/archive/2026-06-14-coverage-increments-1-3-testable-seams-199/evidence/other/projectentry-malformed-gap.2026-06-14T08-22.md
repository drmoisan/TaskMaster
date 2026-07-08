# Flag-and-Stop Gap — ProjectEntry dialog-dependent branches (P1-T3)

Timestamp: 2026-06-14T08-22

Task: [P1-T3] ProjectEntry tests (ToDoModel.Test)

## Summary

The following ProjectEntry branches cannot be exercised from ToDoModel.Test without introducing a
NEW production change, so per the feature Flag-and-Stop rule they are intentionally NOT covered.
No production seam was added.

## Affected branches

1. ProjectEntry.SetProjectId(string) — ToDoModel/Data Model/Project/ProjectEntry.cs line 102
   - `case string s when s.Length != 4` (line 116): malformed-id path calls
     `MyBox.ShowDialog(...)` (static dialog) and returns false.
   - `case string s when s != ProjectID` (line 129): change path delegates to
     `ChangeId(newID)` (line 141), which calls `MyBox.ShowDialog(...)`.

2. ProjectEntry.CompareTo length tie-break — ProjectEntry.cs lines ~197-204
   - Reached only when `string.CompareOrdinal` returns 0 for two ids of DIFFERENT length, which
     requires constructing an entry with a non-4-character ProjectID. Every accessible constructor
     routes ProjectID through the validating setter (line 36), which shows a `MessageBox` for a
     non-null id whose length != 4. The tie-break is therefore not reachable without a dialog.

## Why a new production seam would be required

- `MyBox.ShowDialog` does have an injectable seam, `MyBox.DialogInvoker`
  (UtilitiesCS/Dialogs/MyBox.cs line 39), BUT it is declared `internal static` and UtilitiesCS
  exposes `InternalsVisibleTo("UtilitiesCS.Test")` only — there is NO
  `InternalsVisibleTo("ToDoModel.Test")` on UtilitiesCS.
- Reaching the dialog seam from ToDoModel.Test would require adding
  `InternalsVisibleTo("ToDoModel.Test")` to UtilitiesCS production source (a production change),
  or adding a new injectable seam to ProjectEntry. Both are prohibited as silent edits.
- Even with seam access, `MyBox.ShowDialog` constructs a `MyBoxViewer` (WinForms control) before
  invoking the seam, which conflicts with the no-WinForms constraint.

## Coverage delivered instead (dialog-free branches)

SetProjectId:
- empty/null current id + non-empty new id -> set, return true (positive).
- `case null` -> set null, return true (negative).
- new id equal to existing valid id -> break -> return false (edge).

CompareTo(IProjectEntry) / CompareTo(object) — fully covered:
- null comparand -> 1; null this.ProjectID -> -1; equal ids -> 0; ordinal different ids -> signed;
  CompareTo(object) null -> 1; IProjectEntry object -> typed compare; non-IProjectEntry -> throws
  ArgumentException.

## Disposition

This is a known, accepted coverage gap consistent with the plan's Flag-and-Stop rule. It does NOT
block the plan (the rule directs restricting the test and recording the gap, which is done). No
production source, project, or config file was changed for ProjectEntry coverage.
