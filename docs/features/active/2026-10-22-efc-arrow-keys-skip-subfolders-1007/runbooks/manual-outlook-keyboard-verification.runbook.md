# Human-Exception Runbook: Manual Outlook Verification of EFC and QFC Folder-Breadcrumb Keyboard Behaviour (Issue #1007)

- Issue: #1007 (https://github.com/drmoisan/TaskMaster/issues/1007)
- Branch: `bug/efc-arrow-keys-skip-subfolders-1007`
- Feature folder: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/`
- Authored: 2026-10-25
- Performed by: the maintainer, in Outlook desktop

## Cue

Act on this runbook when the orchestrator has recorded an `exception` response for the requirement "manual verification in Outlook desktop of the EFC and QFC folder-breadcrumb keyboard behaviour" for issue #1007.

The automated suites cover the router, session, coordinator and page-script logic (AC12, AC13). They cannot observe a live WebView2 viewport, real keyboard focus, window paint order, WebView2 double-click timing or key auto-repeat. The research record (`research/2026-10-23T10-40-arrow-key-subfolder-navigation-research.md`, section "Automation Feasibility") lists these as human-only items. The spec's Rollout section makes this verification a condition of merging to `main`.

## Prerequisites

### Accounts, tools and data

- A Windows 11 workstation with Outlook desktop and a mailbox the TaskMaster add-in can use.
- Visual Studio build tools, so that `msbuild` is available (for example, a Developer PowerShell for Visual Studio).
- A checkout of branch `bug/efc-arrow-keys-skip-subfolders-1007` at its latest commit.
- At least one disposable test message in the Inbox. Check E4 files a message with Enter, so do not use a message that matters.

### Build and load the add-in

1. Close Outlook normally: **File** > **Exit**, or close the main window. Do not end the process in Task Manager. Outlook locks the add-in's build output while it runs, and killing the process can cause Outlook to disable the add-in on the next start.
2. Wait until no `OUTLOOK.EXE` process is listed in Task Manager (**Details** tab). Only watch the list here; do not end the process.
3. From the repository root of the branch checkout, run:

   ```powershell
   msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
   ```

   The build must finish with no errors. A VSTO add-in build writes the per-user registry entries that Outlook needs to load the add-in from this build output.
4. If more than one working tree of this repository exists on the machine, the registration points at whichever tree built last. Re-run step 3 in this branch's tree immediately before starting Outlook.
5. Start Outlook. Confirm that the **Taskmaster** ribbon tab is present and contains the **Sort Email** and **Quick Filer** buttons.
6. If the tab is missing, check whether the add-in has been disabled. Go to **File** > **Options** > **Add-ins**. In the **Manage** box, select **COM Add-ins** and then **Go**. Select the TaskMaster add-in's check box and select **OK**. If the add-in is listed under **Disabled Items** instead, select **Disabled Items** in the **Manage** box, select **Go**, select the add-in, select **Enable**, and then restart Outlook.

### Folder hierarchy fixture

Create the hierarchy below under the configured archive root, or choose existing folders with the same shape. The breadcrumb rows show paths relative to the archive root, so the first segment shown (`Archive` in the examples) is one level below the root.

```
<archive root>
  Archive
    Projects
      Alpha
        Specs          (no subfolders)
        Invoices
          2025         (no subfolders)
          2026         (no subfolders)
        Notes          (no subfolders)
    Clients
      Acme             (no subfolders)
      Beta             (no subfolders)
      Gamma            (no subfolders)
    Misc               (no subfolders)
```

The fixture must provide:

- a top-level row with nested subfolders two levels deep (`Archive > Projects > Alpha`);
- a three-segment row whose middle segment has several children (`Archive > Clients > Beta`);
- at least one folder with no subfolders that appears as a top-level row (`Archive > Misc`);
- enough rows that the list is taller than the viewer for checks H1 and E10. Expanding Alpha and Invoices is usually enough; resize the window smaller if it is not.

If you use other folder names, write the mapping in the Notes column of the results table. The rest of this runbook uses the fixture names.

### Opening the viewers

- **EFC (Sort Email viewer):** select the test message in the Inbox, then select **Taskmaster** > **Sort Email**. The viewer has a **Search Text:** box and the **Matched Folders:** breadcrumb list. Type search text, or use the suggestions, until the list shows at least the rows `Archive > Projects > Alpha`, `Archive > Clients > Beta` and `Archive > Misc`. In the checks:
  - **R-next** is the top-level row directly below the Alpha row;
  - **First row** is the top-most row in the list;
  - **Last entry** is the bottom-most visible entry. If the last row is expanded, this is its last visible subfolder.
- **EFC reset:** a double-click and Ctrl+Left change a row's shape. To restore the original row shapes, close the viewer with **Cancel** and open it again.
- **QFC (Quick Filer item viewer):** select **Taskmaster** > **Quick Filer**. Each item in the viewer has a folder breadcrumb control. In the checks:
  - **open list:** the control's folder list is shown;
  - **closed list:** the control has keyboard focus and its list is not shown. To reach this state, open the list by clicking the control, then press Escape. Escape closes the list and keeps the committed choice.
  - **committed choice:** the folder the item will be filed to, shown by the closed control;
  - **pending highlight:** the entry highlighted in the open list.

### Terms used in the checks

- **Selected (EFC):** the row or subfolder shown with the selection highlight. This is the folder Enter files to.
- **Expanded / collapsed:** a folder with subfolders shows `[+]` when collapsed and `[−]` when expanded. Click the affordance to change the state.
- **Visible order:** the top-level rows plus every subfolder currently shown, in screen order. The children of a collapsed folder are not part of it.

## Step-by-step Instructions

Run the checks in order. Each check gives a starting state, the keys to press and the expected result. If a step's result differs from the expected result, record the check as Fail with what you observed, finish the remaining checks, and then follow "On a failure" under Verification.

### EFC checks

**E1. AC1 (Down in tree order, collapsed children skipped)**

- Start: EFC open. Alpha expanded and Invoices expanded, so that Specs, Invoices, 2025, 2026 and Notes are shown below Alpha. Alpha row selected.
- Keys: Down six times.
- Expected: the selection moves through Specs, Invoices, 2025, 2026, Notes and R-next, in that order. Each subfolder is selected when it is reached; none is skipped.
- Then: click Invoices' `[−]`, select the Alpha row and press Down three times.
- Expected: the selection moves through Specs, Invoices and Notes. 2025 and 2026 are skipped because Invoices is collapsed.

**E2. AC1 (Up into an expanded block)**

- Start: Alpha expanded, Invoices expanded, R-next selected.
- Keys: Up four times.
- Expected: the selection moves through Notes, 2026, 2025 and Invoices. The first Up lands on Notes, the last visible subfolder of the Alpha block, not on the Alpha row.

**E3. AC2 (no wrap at either end)**

- Start: the last entry selected.
- Keys: Down.
- Expected: nothing changes. The last entry stays selected, the selection does not move to the first row, and no message appears.
- Then: select the first row and press Up.
- Expected: keyboard focus moves to the **Search Text:** box and the text cursor is shown there. The selection does not move to the last entry. Check H2 confirms the focus move in more detail.

**E4. AC3 (an arrow selects as a click does; Enter files there)**

- Start: Alpha expanded, Alpha row selected, the test message is the item being sorted.
- Keys: Down twice, so that Invoices is selected.
- Expected: Invoices shows the same selection highlight that a mouse click on Invoices shows. To compare, click Specs, then Invoices, then press Up and Down again.
- Then: with Invoices selected by the keyboard, press Enter.
- Expected: the test message is filed to `Archive > Projects > Alpha > Invoices`. Check this in the Outlook folder pane. Afterwards, select **Taskmaster** > **Undo Sort** to return the message, or move it back by hand.

**E5. AC5 (Right expands, then moves to the first child; shortened row lists children)**

- Start: Alpha collapsed and selected.
- Keys: Right.
- Expected: Alpha expands to show Specs, Invoices and Notes. The selection stays on Alpha.
- Keys: Right.
- Expected: Specs is selected.
- Then: select Invoices (collapsed) and press Right, then Right again.
- Expected: Invoices expands to show 2025 and 2026, then 2025 is selected.
- Then: reset EFC. Double-click the `Clients` segment of `Archive > Clients > Beta`, so the row reads `Archive > Clients [+]` with Clients selected. Press Right.
- Expected: Acme, Beta and Gamma are listed below the row as subfolders. The row is not restored to `Archive > Clients > Beta`. The selection stays on Clients.
- Keys: Right.
- Expected: Acme is selected.

**E6. AC6 (Right on a folder with no subfolders, EFC)**

- Start: Alpha expanded, Specs selected.
- Keys: Right.
- Expected: nothing changes. No dialog opens.
- Then: select the `Archive > Misc` row and press Right.
- Expected: nothing changes. No dialog opens.
- The QFC part of AC6 is check H3.

**E7. AC7 (Left collapses only; never selects a parent; hidden selection moves to the nearest visible ancestor)**

- Start: Alpha expanded, Invoices expanded, Invoices selected.
- Keys: Left.
- Expected: Invoices collapses. Alpha stays expanded. The selection stays on Invoices.
- Keys: Left.
- Expected: nothing changes.
- Then: select Specs and press Left.
- Expected: nothing changes. The selection does not move to Alpha.
- Then: collapse Alpha, select the Alpha row and press Left.
- Expected: nothing changes. The row still reads `Archive > Projects > Alpha` with no segment hidden, and Alpha stays selected. The selection does not move to Projects.
- Then: expand Alpha and Invoices, select 2025, and click Invoices' `[−]` with the mouse.
- Expected: Invoices collapses and the selection moves to Invoices.
- Then: expand Invoices, select 2025, and click the Alpha row's `[−]`.
- Expected: Alpha collapses and the selection moves to the Alpha row.

**E8. AC8 (Ctrl+Left selects the parent; Ctrl+Right is Right)**

- Start: EFC reset. `Archive > Clients > Beta` selected.
- Keys: Ctrl+Left.
- Expected: the row reads `Archive > Clients [+]` and Clients is selected. This is the same result as a double-click on `Clients`.
- Keys: Ctrl+Left.
- Expected: the row reads `Archive [+]` and Archive is selected.
- Keys: Ctrl+Left.
- Expected: nothing changes. The archive root is never selected.
- Then: reset EFC. Expand Alpha and Invoices, select 2025 and press Ctrl+Left.
- Expected: Invoices is selected and stays expanded (2025 and 2026 still shown).
- Keys: Ctrl+Left.
- Expected: the Alpha row is selected and stays expanded.
- Then: collapse Invoices, select it and press Ctrl+Right.
- Expected: Invoices expands, as plain Right does in E5.

**E9. AC9 (segment double-click in EFC)**

- Start: EFC reset. Any row other than `Archive > Clients > Beta` selected.
- Action: double-click the `Clients` segment of `Archive > Clients > Beta` at normal speed.
- Expected: the row reads `Archive > Clients [+]` and Clients is selected. `Beta` is no longer shown in the row.
- Then: click the `[+]`.
- Expected: Acme, Beta and Gamma are listed below the row. The selection stays on Clients.
- Keys: Right.
- Expected: Acme is selected.

**E10. AC11 (Home and End)**

- Start: EFC open. Alpha expanded, Invoices expanded, 2026 selected.
- Keys: Home.
- Expected: the first row is selected. Keyboard focus stays in the breadcrumb list, not in the **Search Text:** box. To confirm, press Down: the selection moves to the next entry.
- Keys: End.
- Expected: the last entry is selected. If the bottom row is expanded, End selects its last visible subfolder, not the row.
- Note: to test End onto a subfolder, scroll to the bottom row, expand it if it has subfolders, then select the first row and press End.

**E11. AC14 (no `[+]` on a folder without subfolders; both viewers)**

- Start: EFC open with the fixture rows shown. Alpha expanded, Invoices expanded.
- Action: look at every row and every shown subfolder.
- Expected: Specs, 2025, 2026, Notes and `Archive > Misc` show no `[+]` and no `[−]`.
- Action: click every `[+]` shown in the list, one at a time.
- Expected: each one lists at least one subfolder. No folder shows `[−]` with nothing below it.
- Then: repeat the inspection and the expansions in the QFC open list.
- Expected: the same results in QFC.

**E12. AC11 (EFC with nothing selected)**

- Start: EFC open with no row selected and keyboard focus in the breadcrumb list. If the viewer always opens with a row selected and no keyboard route clears the selection, record this check as Not reachable (not as Fail) and describe the starting state in Notes.
- Keys, one case at a time (return to the no-selection state before each case): Down; Home; Up; End.
- Expected: Down selects the first row. Home selects the first row. Up selects the last entry. End selects the last entry. No case leaves the list with nothing selected.

### QFC open-list checks

**Q1. AC4 and AC1 (pending model in the open list)**

- Start: QFC open. One item's committed choice is a top-level row C, for example `Archive > Misc`. The list for that item is open. Alpha expanded and Invoices expanded.
- Keys: Down or Up until the pending highlight is on Alpha, then Down five times.
- Expected: the pending highlight moves through Specs, Invoices, 2025, 2026 and Notes. The list stays open.
- Keys: Escape.
- Expected: the list closes and the committed choice is still C.
- Then: open the list, move the pending highlight to Invoices and press Enter.
- Expected: the list closes and the committed choice is `Archive > Projects > Alpha > Invoices`.
- Then: open the list, move the pending highlight to Specs (not expanded) and press Left.
- Expected: the list closes and the committed choice is `Archive > Projects > Alpha > Specs`.

**Q2. AC4 (pending highlight is visible)**

- Start: QFC open list, with a committed choice that is not Alpha. Alpha and Invoices expanded.
- Keys: move the pending highlight onto the Alpha row, then onto Invoices (a subfolder one level down), then onto 2025 (two levels down).
- Expected: at each step, exactly one entry shows the pending highlight. It is clearly visible on the top-level row and on both subfolder levels, and it can be told apart from the committed choice. When the pending entry is below the visible area, the list scrolls so it can be seen.

**Q3. AC8 (Ctrl+Left in QFC; Escape restores the row shape)**

- Start: QFC open list. Committed choice `Archive > Clients > Beta`. Pending highlight on that row.
- Keys: Ctrl+Left.
- Expected: the pending highlight moves to Clients and the row reads `Archive > Clients [+]`. The list stays open.
- Keys: Escape.
- Expected: the list closes. When reopened, the row reads `Archive > Clients > Beta` and the committed choice is still `Archive > Clients > Beta`.
- Then: close the list, keeping keyboard focus on the control, and press Ctrl+Left.
- Expected: the committed choice changes at once to `Archive > Clients`, without the list opening.

### QFC closed-list check

**Q4. AC10 (closed list)**

- Start: QFC closed list for an item whose committed choice is Specs (`Archive > Projects > Alpha > Specs`). Keyboard focus on the control.
- Keys: Right.
- Expected: the list opens. If Alpha is expanded so that Specs is shown, the pending highlight is on Specs. Otherwise it is on the Alpha row. Record which case occurred.
- Then: press Escape to close the list, keeping focus on the control.
- Keys: Left.
- Expected: nothing changes. The list does not open and the committed choice does not change.
- Keys: Down, then Up.
- Expected: Down commits the next top-level row directly, and Up commits the previous one. The list stays closed for both keys.
- Keys: Home, then End.
- Expected: Home commits the first top-level row and End commits the last top-level row. The list stays closed.
- Keys: PageDown, then PageUp.
- Expected: nothing changes.

### Human-only checks

**H1. AC11 (PageUp and PageDown against the real viewport; both viewers)**

- Start: EFC open with the list taller than the viewer (expand Alpha and Invoices, or shrink the window). First row selected. Count N, the number of entries that are fully visible (partly cut-off entries do not count).
- Keys: PageDown.
- Expected: the selection moves N entries down in visible order.
- Keys: PageDown repeatedly.
- Expected: the selection stops on the last entry without wrapping.
- Keys: PageUp repeatedly.
- Expected: each press moves N entries up, and the selection stops on the first row. Focus does not move to the **Search Text:** box.
- Then: resize the window so N changes, count again and repeat one PageDown.
- Expected: the move matches the new N.
- Then: repeat in the QFC open list.
- Expected: the same distances. Only the pending highlight moves and the committed choice does not change until Enter.

**H2. AC2 and AC11 (focus moves to the search box only on Up from the first row)**

- Start: EFC open. Alpha is the first row, expanded, with Specs selected.
- Keys: Up.
- Expected: the Alpha row is selected and focus stays in the list.
- Keys: Up.
- Expected: focus moves to the **Search Text:** box. Typing a character puts it in the box. Delete the character afterwards.
- Then: click a subfolder in the list and press Home.
- Expected: the first row is selected and focus stays in the list. The search box does not get focus.

**H3. AC6 (QFC: the list closes before the "Pop Out Item or Enumerate Conversation?" dialog)**

- Start: QFC open list. Committed choice C. Pending highlight on Specs, which has no subfolders.
- Keys: Right.
- Expected, in this order:
  1. The folder list closes.
  2. The "Pop Out Item or Enumerate Conversation?" dialog opens.
  The list is never painted over or behind the dialog.
- Then: close the dialog without choosing an action.
- Expected: the committed choice is still C, not Specs.
- Then: with the list closed and a top-level leaf such as `Archive > Misc` committed, press Right.
- Expected: the list opens. No dialog appears.

**H4. AC9 (double-click and single-click timing in WebView2; both viewers)**

- EFC:
  - Start: EFC reset.
  - Action: double-click the `Clients` segment of `Archive > Clients > Beta` three times, with a reset before each. Use a fast double-click, a normal one, and a slow one that is still within the Windows double-click interval.
  - Expected: each time, the row reads `Archive > Clients [+]` and Clients is selected. The row is never left shortened with a different folder selected.
- QFC double-click:
  - Start: QFC open list showing `Archive > Clients > Beta`.
  - Action: double-click the `Clients` segment at normal speed.
  - Expected: the committed choice becomes `Archive > Clients` and the list closes.
- QFC single click:
  - Start: QFC open list, reopened.
  - Action: click the `Archive` segment once.
  - Expected: the pending highlight moves to that row and the list stays open.
  - Action: click the `Beta` (leaf) segment once.
  - Expected: the choice is committed and the list closes.
- Log: after these steps, open the add-in debug log (`logs\debug_<yyyy-MM-dd>.log` under the TaskMaster project's `bin\Debug` output folder). It must not contain an error-level entry for a second click on an already-active segment.

**H5. AC5 (key auto-repeat on Right)**

- EFC:
  - Start: EFC open. Alpha collapsed and selected.
  - Keys: hold Right for about two seconds, then release it.
  - Expected: Alpha ends expanded, not collapsed. The selection has moved down the first-child chain and stopped on a folder with no subfolders (Specs). No folder shows `[−]` with nothing below it.
- QFC:
  - Start: QFC open list. Pending highlight on a collapsed Alpha.
  - Keys: hold Right.
  - Expected: Alpha expands and the pending highlight reaches Specs. Then the list closes and exactly one "Pop Out Item or Enumerate Conversation?" dialog opens. Close the dialog. The committed choice is unchanged.

## Verification

### Pass/fail recording

Fill in one row per check. Status is Pass, Fail or Not reachable. Not reachable is allowed only for E12, under the condition stated in that check. Record the observed behaviour in Notes for every Fail and Not reachable, and record any fixture name substitution.

| Check | AC / item | Status | Date | Notes |
|---|---|---|---|---|
| E1 | AC1 Down, tree order | | | |
| E2 | AC1 Up into expanded block | | | |
| E3 | AC2 No wrap | | | |
| E4 | AC3 Select on move, Enter files | | | |
| E5 | AC5 Right | | | |
| E6 | AC6 Right on leaf (EFC) | | | |
| E7 | AC7 Left | | | |
| E8 | AC8 Ctrl+Left / Ctrl+Right (EFC) | | | |
| E9 | AC9 Segment double-click (EFC) | | | |
| E10 | AC11 Home / End | | | |
| E11 | AC14 No `[+]` without subfolders (EFC and QFC) | | | |
| E12 | AC11 No selection (EFC) | | | |
| Q1 | AC4 / AC1 Pending model (QFC open) | | | |
| Q2 | AC4 Pending highlight visible (QFC open) | | | |
| Q3 | AC8 Ctrl+Left and Escape (QFC) | | | |
| Q4 | AC10 Closed list (QFC) | | | |
| H1 | Real viewport page size | | | |
| H2 | Focus to search box | | | |
| H3 | List closes before pop-out dialog | | | |
| H4 | Double-click timing in WebView2 | | | |
| H5 | Key auto-repeat | | | |

Tester: ____________________  Outlook version (File > Office Account > About Outlook): ____________________  Branch commit: ____________________

### Success criteria

The verification is complete when every row is Pass, or Not reachable for E12 with the reason recorded. Commit the completed table with the feature folder, then report completion to the orchestrator so the human-interaction requirement can be closed and the branch can proceed to merge.

### On a failure

1. Do not merge the branch.
2. Open a GitHub issue that references #1007, for example titled "#1007 follow-up: <check ID> <short description>". Include:
   - the check ID and AC;
   - the starting state, the keys pressed, the expected result and the observed result;
   - the branch commit and Outlook version;
   - a screenshot where the defect is visual (H3, Q2, E11);
   - the relevant lines of the add-in debug log for the session (`logs\debug_<yyyy-MM-dd>.log` under the TaskMaster project's `bin\Debug` output folder).
3. Add the follow-up issue number to the Notes column of the failed row.
4. A defect that is unrelated to the files changed for #1007 is promoted through the repository's issue promotion lifecycle as a separate issue, per the spec's Rollout section. It does not block this item.

### Cleanup

- Return the E4 test message to its original folder if Undo Sort did not.
- Delete any fixture folders created only for this runbook, if they are not wanted.
- To rebuild later, close Outlook normally first (Prerequisites, steps 1 and 2).

## Source and Citation

MCP-first sourcing was not available: the repository has no callable MCP documentation tool. The third-party (Outlook and VSTO) steps were sourced web-second from Microsoft Learn on the capture date below. The TaskMaster UI labels and expected behaviours come from this repository.

- Build and load (Prerequisites, step 3: a VSTO add-in build writes the registry entries needed to load the add-in; build output is `bin\debug`). Microsoft Learn, "Build Office solutions". Source URL: https://learn.microsoft.com/en-us/visualstudio/vsto/building-office-solutions. Page updated_at: 2026-04-24. Captured: 2026-10-25.
- Re-enabling a disabled add-in (Prerequisites, step 6: **File** > **Options** > **Add-ins**, **Manage** > **COM Add-ins** or **Disabled Items** > **Go**). Microsoft Learn, "Re-enable a VSTO Add-in that has been disabled". Source URL: https://learn.microsoft.com/en-us/visualstudio/vsto/how-to-re-enable-a-vsto-add-in-that-has-been-disabled. Page updated_at: 2026-04-24. Captured: 2026-10-25.
- Build command (Prerequisites, step 3). Repository `CLAUDE.md`, "C# Toolchain". The analyzer and nullable switches are left out because this build only produces the add-in for manual use. Captured: 2026-10-25.
- Closing Outlook before a rebuild, never killing it (Prerequisites, steps 1 and 2). Maintainer operating guidance relayed in the orchestrator task for #1007. Captured: 2026-10-25.
- Ribbon labels **Taskmaster**, **Sort Email**, **Quick Filer** and **Undo Sort**: `TaskMaster/Ribbon/RibbonExplorer.xml`. EFC labels **Search Text:** and **Matched Folders:**: `QuickFiler/Viewers/EfcViewer.Designer.cs`. Dialog title "Pop Out Item or Enumerate Conversation?": `QuickFiler/Controllers/KeyboardHandler.cs` (`BreadcrumbArrowFallThrough`). Log file pattern `logs\debug_<yyyy-MM-dd>.log`: `TaskMaster/log4net.config`. All read on branch `bug/efc-arrow-keys-skip-subfolders-1007`. Captured: 2026-10-25.
- Expected behaviour for every check: `spec.md` (AC1 to AC11 and AC14, decisions D-Q1 to D-Q29, Test Strategy "Manual validation steps"); `grill-transcript.md` ("Shared Understanding" rules 1 to 11 and Round 4); `issue.md` ("Steps to Reproduce"); `research/2026-10-23T10-40-arrow-key-subfolder-navigation-research.md` ("Automation Feasibility"). All in this feature folder. Captured: 2026-10-25.
