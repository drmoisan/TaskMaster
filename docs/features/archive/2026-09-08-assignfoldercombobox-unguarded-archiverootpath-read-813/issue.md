# assignfoldercombobox-unguarded-archiverootpath-read (Issue #813)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/assignfoldercombobox-unguarded-archiverootpath-read/ (Issue #813)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #813
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/813
- Last Updated: 2026-09-08
- Work Mode: full-bug

## Summary

`QfcItemController.AssignFolderComboBox` reads `_globals.Ol?.ArchiveRootPath` at
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:233` with no `try`, and the method is
reached from the UI dispatcher. The null-conditional operator guards a null `Ol`; it does not guard
an `ArchiveRootPath` getter that throws when no archive root is configured. Guarding
`FolderPredictor` alone therefore does not prevent the failure at this call site.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8 VSTO add-in hosted by Outlook
- Python version: not applicable; this is C# in `QuickFiler`
- Command/flags used: not a command-line defect; reached through the QuickFiler item pane
- Data source or fixture: a profile whose Archive Root is unset, which is the state reported in
  issue 797 (`Archive Root` -> `Outlook` and `File System` both showing "Please select an archive")

## Steps to Reproduce

1. Open a profile whose Archive Root has never been set, so `ArchiveRootPath` has no configured value.
2. Open QuickFiler on a mail item so the folder combo box is populated.
3. Observe the folder-handling path reach `AssignFolderComboBox` with `_folderHandler.FolderArray`
   non-empty.

## Expected Behavior

An unset archive root degrades the suggestion display — the predetermined folder is simply not
preselected — and QuickFiler continues to operate.

## Actual Behavior

The read at `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:233` propagates the getter's
exception out of `AssignFolderComboBox`. Because line 188 of the same file invokes the method through
`_itemViewer.UiDispatcher.InvokeAsync(AssignFolderComboBox)`, the exception surfaces on the UI
dispatcher rather than at a handled boundary.

`AssignFolderComboBox` begins at line 191 and contains no `try` block; the read sits six lines after
`_itemViewer.SetFolderSuggestions(_folderHandler.FolderRowArray)` consumes `FolderRowArray`.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none captured; the finding is from static reading of the call path, not from a runtime
  trace. A runtime trace should be captured as part of the fix.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: it is a user-visible failure on a supported configuration (archive root unset), and it is
the reason issue 812's AC1 outcome can be verified at unit level but not end to end in QuickFiler.

## Suspected Cause / Notes

Found by the preparation child for issue 812 while establishing that item's acceptance criteria, and
independently verified against the tree on 2026-09-08. It is outside 812's frozen write set, so it
could not be fixed or filed from that item's branch without breaching the footprint constraint.

Sequence with 812: 812 hardens the archive-root read path itself. This call site needs its own
handling regardless, because a guarded provider does not make a throwing property read safe at an
unguarded consumer.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a `QfcItemController` folder-handling test in which the globals stub's
      `ArchiveRootPath` getter throws, asserting `AssignFolderComboBox` completes and leaves the
      combo box populated without a preselection.
- [ ] Integration scenario to retest: QuickFiler item pane on a profile with no archive root set.
- [ ] Manual verification notes: confirm the folder combo box still populates and the add-in log
      records the degraded path rather than an unhandled dispatcher exception.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
