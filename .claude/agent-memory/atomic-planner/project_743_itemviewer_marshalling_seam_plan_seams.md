---
name: project_743_itemviewer_marshalling_seam_plan_seams
description: Issue #743 (QuickFiler ItemViewer UI-marshalling seam) completion-pass seams — a wildcard test-name comment makes a full-name grep false-before; a commented-out duplicate of a converted literal makes a zero-count gate unsatisfiable; AssignControls needs _globals injected; the SubagentStop hook path check bit two multi-line tasks
metadata:
  type: project
---

Found on 2026-09-12 while completing an orphaned #743 plan (predecessor session died mid-pass, plan body done, no handoff record).

**Seams, each confirmed against the tree:**

1. **ViewerSetup.cs line 275 names the retained pump test only as `ResolveControlGroupsAsync_ThroughThePumpHost_*` (wildcard).** A grep for the full name `..._PopulatesTipsAndControlGroups` in the production file is 0 pre-edit, so a comment-update gate on it IS false-before. The orchestrator assumed it was already 1. Still add a clause on the TEST file (which the plan never writes) so the gate proves the named test exists.
2. **Line 365 is a commented-out copy of the line-371 marshal** (`//    await _itemViewer.UiDispatcher.InvokeAsync(...)`). A "prints 0" gate on `_itemViewer.UiDispatcher.InvokeAsync` after converting 371 is unsatisfiable; the correct post-edit count is 1 (pre-edit 2). Same class as the #442 commented-out-code trap; always grep the whole file for the literal, including comments, before writing a zero-count.
3. **`AssignControls` reads `_globals.QfSettings` (lines 401-410).** `HarnessController` supplies nothing for `_globals`; every existing AssignControls test injects `BuildGlobals(...)` (private static at ViewerSetupTests.cs 36-51, NOT reachable from a new file). A seam test of `AssignControlsAsync` with a mock viewer must build its own `Mock<IApplicationGlobals>` → `Mock<IAppQuickFilerSettings>` and a parameterless `MailItemHelper`.
4. **`validate-planner-output.ps1` path check hit P4-T1 and P6-T14**: both had the path only in bullets/body lines, not on the `- [ ] [P#-T#]` opening line. Grep `^- \[ \] \[P\d+-T\d+\] [^/\\]*$` before handoff; expect 0 hits.
5. **`Panel`-parented labels are accepted by `QfcTipsDetails.CreateAsync`** (ResolveParentType allows exact `TableLayoutPanel` or `Panel`; precedent at UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs 660-711).
6. `CITATION:` path regex in the hook requires at least one `/` — cite root files as `./.gitignore`, `./TaskMaster.runsettings`.

**How to apply:** For any QfcItemController seam test plan, check `_globals` consumption in the member under test; for any "convert marshal" task, count the literal in comments too; run the opening-line path grep before ending the turn.
