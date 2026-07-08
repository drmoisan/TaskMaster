# Increment 1 Seam Verification — ToDoModel

Timestamp: 2026-06-14T08-22

Command: source inspection of ToDoModel target files (Read/Grep)

EXIT_CODE: 0

## Confirmed seams (file/line; no [ExcludeFromCodeCoverage])

- ToDoLoader.SetAndSave<T> (four overloads) — ToDoModel/Data Model/ToDo/ToDoLoader.cs
  - `internal void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter)` line 33
  - `internal void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter, System.Action objectSaver)` line 48
  - `internal void SetAndSave<T>(T value, Action<T> objectSetter)` line 79
  - `internal void SetAndSave<T>(T value, Action<T> objectSetter, System.Action objectSaver)` line 84
  - Class is `internal`; constructor `ToDoLoader(System.Action olSaver, Func<bool> isReadOnly)` (line 13) is Outlook-free (two delegates). No [ExcludeFromCodeCoverage] on the class or these overloads.
  - Read-only guard: when `_readonly` (i.e. `isReadOnly()` returns true), setter/saver are skipped; null `objectSetter` throws ArgumentNullException only when NOT read-only; null `objectSaver` is guarded (`is not null`).

- IDList Outlook-free constructors + GetNextToDoID(string) — ToDoModel/Data Model/ID/IDList.cs
  - `IDList()` line 23, `IDList(IList<string>)` line 26, `IDList(IEnumerable<string>)` line 29 — all Outlook-free, not exempt.
  - `GetNextToDoID(string strSeed)` line 82 — not exempt. Calls `strSeed.ThrowIfNullOrEmpty()` (line 84). Uses base-36 arithmetic; increments until a non-colliding ID is found. Writes `Properties.Settings.Default.MaxLengthOfID` + `Save()` only when produced length exceeds `_maxIDLength` (lines 101-106). Serializes only when `Filepath is not null` (line 107). DETERMINISM NOTE for P1-T2: snapshot/restore `Settings.Default.MaxLengthOfID` if the length-rollover path is exercised; do not set a Filepath so Serialize() is skipped.
  - Outlook-application constructors and RefreshIDList are [ExcludeFromCodeCoverage] (#197) and are NOT targeted.

- ProjectEntry.SetProjectId / CompareTo — ToDoModel/Data Model/Project/ProjectEntry.cs
  - `SetProjectId(string newID)` line 102 — not exempt. Branches:
    - `ProjectID.IsNullOrEmpty() && !newID.IsNullOrEmpty()` -> set, return true (DIALOG-FREE, positive).
    - `case null` -> set null, return true (DIALOG-FREE, negative).
    - `case string s when s.Length != 4` -> `MyBox.ShowDialog(...)`, return false (DIALOG-DEPENDENT, malformed path).
    - `case string s when s == ProjectID` -> break -> return false (DIALOG-FREE, edge).
    - `case string s when s != ProjectID` -> `ChangeId(newID)` which calls `MyBox.ShowDialog` (DIALOG-DEPENDENT).
  - `CompareTo(IProjectEntry)` line 182 and `CompareTo(object)` line 211 — pure ordinal comparison, not exempt. Branches: null other -> 1; null ProjectID -> -1; ordinal compare with length tie-break.
  - Constructor `ProjectEntry(ProjName, ProjID, ProgName)` (line 86) routes ProjID through the `ProjectID` setter (line 36) which shows a `MessageBox` when length != 4 and non-null; to construct deterministically use a 4-char or null ProjID.
  - FLAG-AND-STOP PRE-NOTE (resolved at P1-T3): The malformed-ID path and the change-path route through static `MyBox.ShowDialog`. `MyBox.DialogInvoker` is an `internal static` seam in UtilitiesCS/Dialogs/MyBox.cs (line 39), BUT UtilitiesCS exposes `InternalsVisibleTo("UtilitiesCS.Test")` only — NOT `InternalsVisibleTo("ToDoModel.Test")`. Therefore the dialog seam is NOT reachable from ToDoModel.Test without a NEW production change (adding InternalsVisibleTo to UtilitiesCS). Per the Flag-and-Stop rule, P1-T3 restricts coverage to the dialog-free branches above and records the gap in evidence/other/; no production seam is added.

- BaseChanger — ToDoModel/Data Model/ID/BaseChanger.cs
  - `public static class BaseChanger`, not exempt. `ToBase(BigInteger, int, int)` line 48, `ToBase10(char, int)` line 79, `ToBase10(string, int)` line 94, `internal ValidateParams(int)` line 20, `internal ValidateParams(int, BigInteger)` line 37. Pure arithmetic; throws ArgumentOutOfRangeException for invalid base/number/char.

- InternalsVisibleTo("ToDoModel.Test") — confirmed in ToDoModel/Data Model/ToDo/ToDoItem.cs line 17. Internal ToDoLoader / SetAndSave / ValidateParams are reachable from the test project without a production change.

## Output Summary
All Increment 1 target members exist with the file/line references above; none of the targeted
members carry [ExcludeFromCodeCoverage]. The internal members are reachable via the existing
ToDoModel InternalsVisibleTo("ToDoModel.Test"). One pre-flagged gap: the ProjectEntry malformed-ID
and change-paths are dialog-dependent and not reachable from ToDoModel.Test without a new
production InternalsVisibleTo on UtilitiesCS; this is the Flag-and-Stop item handled at P1-T3.
