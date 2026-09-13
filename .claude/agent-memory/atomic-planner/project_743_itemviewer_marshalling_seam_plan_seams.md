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

**Preflight round 1 seams (2026-09-12, 12 deltas, all confirmed against the tree):**

7. **Converting `_itemViewer.UiDispatcher.InvokeAsync` to `_uiDispatcher` MUST carry null tolerance** (spec 6.2 second risk bullet; shape = `NotifyMoveFailure`, MailActions partial 35-46). `HarnessController` calls the protected parameterless ctor (Initialization 27), which assigns nothing; the field is only set at Initialization 59/391/438/480. Existing test `AssignControlsAsync_DispatchesAssignThroughViewerDispatcher` (ViewerSetupTests 308-344, file at 498 lines so untouchable) never injects `_uiDispatcher` and would NRE. "No null guard because it hides a bug" was the wrong call — check every existing test that reaches the site through the harness.
8. **`Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (FixtureTests 204-262) starts a second `BeginTransactionAsync` while the first is held** — it contributes 0-or-1 contended acquisition by race and pollutes any "serial contended count == 0" observable. Exclude with `&FullyQualifiedName!~...` in the filter. It is also the #823 known-intermittent; never put it in an N-run streak.
9. **Exactly six `ThroughThePumpHost` tests exist** (Part3 40/83/131/175/245 + ViewerSetupTests 426). An "at least eight" floor was unsatisfiable; count the token before writing any duration-table floor.
10. **`Invoke-MSTestWithCoverage.ps1` accepts a single-assembly `-SearchRoot`** (line 296 wraps discovery in `@(...)`) — unlike `Invoke-MSTest.ps1`. Use `-SearchRoot QuickFiler.Test` to dodge the four UtilitiesCS shell-icon stalls. Its two non-zero paths differ: a vstest failure throws at line 236 BEFORE post-processing (line 342), leaving absolute filenames; the 80% assert (line 344, message from Threshold.ps1 54) runs after. Plan the manual `ConvertTo-KoverageCoberturaXml` fallback (Helpers.ps1 406) for the first case.
11. **A plan-status gate that greps the plan for `Status: Executed` matches its own task line**; the header form is `- **Status:** X`, so anchor `^- \*\*Status:\*\* Executed`.
12. **`acceptance-criteria-tracking` forbids appending pointers to criterion text** — check-off tasks change only the checkbox; PARTIAL figures and evidence pointers go in the status-summary artifact rows.
13. **Blast-radius extractor harvests backticked forward-slash tokens with a recognised extension even inside command spans**; write script paths in commands with backslashes (`scripts\vscode\Foo.ps1`) and name out-of-set files in plain prose. `.runsettings` switch tokens were not harvested.

**How to apply:** For any QfcItemController seam test plan, check `_globals` consumption in the member under test; for any "convert marshal" task, count the literal in comments too; run the opening-line path grep before ending the turn.
