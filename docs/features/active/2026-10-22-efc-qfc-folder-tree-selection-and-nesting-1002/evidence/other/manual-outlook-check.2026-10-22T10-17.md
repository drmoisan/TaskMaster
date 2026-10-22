# Manual Outlook Check (#1002)

Timestamp: 2026-10-22T10-17
Status: PENDING HUMAN INTERACTION

Executed by an agent: no. No agent executed any step below; this record holds the instructions for the maintainer. The check is non-blocking for completion (spec AC26).

## Why it cannot be automated

The check needs a live Outlook desktop process with the TaskMaster VSTO add-in loaded, a mailbox whose folder tree has at least three nested levels, and the WebView2 runtime rendering the EFC and QFC breadcrumb surfaces. None of these is available to an unattended agent or to CI, and pixel layout and WebView2 message delivery are not observable from unit tests.

## Steps (from spec Test Strategy, "Manual validation")

EFC (Efficient Filing Console):

1. Open EFC on a mailbox with a three-level folder tree.
2. Expand a suggested folder, then expand a child that has subfolders.
3. Select a grandchild; confirm it is highlighted.
4. Press OK and confirm the filing destination is that grandchild.
5. Reopen EFC and press the trash button; confirm the "Trash to Delete" row is highlighted and that OK targets it.

QFC (Quick Filer):

6. Open QFC, expand a suggested folder, then expand a child that has subfolders.
7. Confirm the +/- follows the folder name on every nested row.
8. Confirm each nested level is indented one step further than its visible parent.

Result: not yet performed.
