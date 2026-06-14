---
name: projectentry-setter-raw-messagebox
description: ProjectEntry.ProjectID setter uses raw un-seamed MessageBox.Show; blocks STA unit tests of SetProjectId/ChangeId change-confirmation
metadata:
  type: project
---

`ToDoModel/Data Model/Project/ProjectEntry.cs` has TWO dialog mechanisms that do NOT share a seam:

- `SetProjectId(string)` and `ChangeId(string)` route their prompts through the seamed
  `MyBox.ShowDialog` (interceptable via the internal `MyBox.DialogInvoker` seam in UtilitiesCS).
- The `ProjectID` **property setter** (the `set` accessor, lines ~36-77) uses RAW
  `System.Windows.Forms.MessageBox.Show` — NOT the MyBox seam.

**Why it matters:** `ChangeId` commits a confirmed change by assigning `ProjectID = newID`, which
runs the property setter. When the old value is non-null and differs, the setter fires its own raw
`MessageBox.Show`. A unit test that injects a `MyBox.DialogInvoker` stub CANNOT suppress this raw
modal, so any STA test driving the change-confirmation path to completion hangs the test host
(verified under vstest: malformed-ID and CompareTo tie-break tests pass; change-confirmation tests
time out). The malformed-ID branch returns false BEFORE assigning, so it does not hit the setter
MessageBox and is testable via the seam.

**How to apply:** To unit-test the ProjectEntry change-confirmation branch you must first route the
`ProjectID` property setter's MessageBox.Show calls through `MyBox.ShowDialog` (a production change).
Under #199 Phase 5 this was out of authorized scope (only 2 seams approved) and was recorded as a
flag-and-stop gap. See [[project_configcontroller_sta_pump_deadlock]] for the broader pattern of
WinForms/STA modal dialogs deadlocking unit tests in this repo. CompareTo's length tie-break
(`string.CompareOrdinal==0` then Length compare) is also unreachable with a plain ProjectEntry
(equal-content strings have equal length) — drive it with a Moq IProjectEntry whose ProjectID
returns an ordinal-equal value on the first read and a length-differing value on later reads
(CompareTo reads other.ProjectID up to 3 times).
