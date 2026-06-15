---
name: projectentry-setter-raw-messagebox-blocks-coverage
description: ProjectEntry.ChangeId change-confirmation branch is uncoverable via the MyBox.DialogInvoker seam because the commit runs the ProjectID property setter's RAW un-seamed MessageBox.Show
metadata:
  type: project
---

In `ToDoModel/Data Model/Project/ProjectEntry.cs`, the `SetProjectId` -> `ChangeId` change-confirmation path commits by assigning `ProjectID = newID`, which runs the `ProjectID` **property setter** (~lines 36-77). The setter's `_projectID != value` arm calls a RAW `System.Windows.Forms.MessageBox.Show` — NOT routed through the seamed `MyBox.DialogInvoker`. So injecting the `MyBox` stub does NOT suppress this dialog; any test driving a changed-id commit triggers a real modal on the STA test thread with no message pump and deadlocks the test host (vstest EXIT 124).

The malformed-ID branch and the `CompareTo` length tie-break ARE reachable via the `MyBox.DialogInvoker` seam (exposed to `ToDoModel.Test` by `InternalsVisibleTo` on UtilitiesCS, Issue #199 Phase 5) and are covered.

**Why:** Covering change-confirmation needs a THIRD production seam (route the setter through MyBox), which the #199 Phase-5 maintainer authorization did not cover (only the UtilitiesCS attribute + AppFileSystemFolderPaths pure-helper extraction were authorized). The executor correctly flagged-and-stopped.

**How to apply:** When auditing any future #199 follow-up that claims full ProjectEntry dialog coverage, verify the property setter was actually seamed before accepting a change-confirmation coverage claim. The spec AC1 prose ("change-confirmation ... now fully covered by Phase 5") is inaccurate — treat it as overstated, not as evidence. Related: [[koverage-analyzer-finding-misattributed]] (another #199-area spec/evidence overstatement).
