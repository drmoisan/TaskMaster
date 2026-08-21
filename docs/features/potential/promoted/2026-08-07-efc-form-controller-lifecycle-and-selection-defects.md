# efc-form-controller-lifecycle-and-selection-defects (Issue #465)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-form-controller-lifecycle-and-selection-defects/ (Issue #465)
- Work Mode: full-bug

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #465
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/465
- Last Updated: 2026-08-08
## Summary

Four independent defects in `EfcFormController`: a non-idempotent `Cleanup()` reachable twice from one
user gesture, a cross-thread WinForms control read inside `Task.Run`, duplicate "Trash to Delete" rows
accumulating on repeated delete actions, and an inconsistent banner-prefix test across three sites.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in
- UI path: `QuickFiler/Controllers/EfcFormController.cs`
- Data source or fixture: an Email Filer session with folder suggestions populated

## Steps to Reproduce

**A — double cleanup.** Trigger the OK action with the Return key while the OK button `Click`
subscription is also live; observe the second `ActionOkAsync` dereference a nulled field.

**C — duplicate trash rows.** Invoke the delete action twice (via the `'T'` keyboard action or the
delete button) and observe two `"Trash to Delete"` rows in the folder list.

## Expected Behavior

- `Cleanup()` is idempotent, or is guarded so it cannot run twice for one gesture.
- WinForms control properties are read only on the UI thread.
- The trash row appears at most once regardless of how many times delete is invoked.
- A row's banner-prefix classification is identical everywhere it is tested.

## Actual Behavior

**A — `Cleanup()` is not idempotent and has no re-entrancy guard (`EfcFormController.cs:189-196`).**
A second invocation dereferences the already-nulled `_globals` at `:191`. Two independent paths can
invoke the OK action for a single user gesture: the always-on Return key binding (`:365`,
`new KaKeyAsync("Collection", Keys.Return, (k) => ActionOkAsync())`) and the OK button `Click`
subscription (`:391`). If both fire, the second `ActionOkAsync` throws `NullReferenceException` at
`:705` on the nulled `_formViewer`. `EfcHomeController.TryBeginExecuteMoves`
(`EfcHomeController.ExecuteMoves.cs:48-57`) shows the `_isExecuting`-style guard this path lacks.

**B — cross-thread control read (`EfcFormController.cs:800-803`).** `RefreshSuggestionsAsync`
evaluates `_formViewer.SearchText.Text` **inside** the `Task.Run` lambda. Reading `Control.Text` off
the UI thread is an illegal cross-thread control access. `SearchText_TextChanged` (`:558`) reads the
same property correctly on the UI thread.

**C — duplicate trash rows (`EfcFormController.cs:742-750`).** `ActionDeleteAsync` reads `_folderRows`,
inserts `"Trash to Delete"` at index 0, and rebinds. `BindFolderRows` (`:881`) then stores the *result*
— which now contains the trash row — back into `_folderRows`. A second invocation inserts a second
trash row. No dedupe guard exists.

**D — inconsistent banner-prefix detection.** `IsValidSelection` (`:1049`) tests
`Substring(0, 3) == "==="` (three characters); `ActionOkAsync` (`:708`) tests `StartsWith("====")`
(four); `BreadcrumbRowBuilder.BannerPrefix` (`BreadcrumbRowBuilder.cs:19`) is `"===="` (four). A row
beginning with exactly three `=` is rejected by `IsValidSelection` but accepted by `ActionOkAsync` and
classified as a suggestion by the row builder.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence recorded above (verified 2026-08-07 against the working tree).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Defect A is an unhandled exception on the primary filing gesture. Defect B is an illegal cross-thread
access whose symptom is intermittent and environment-dependent, which makes it hard to attribute.

## Suspected Cause / Notes

Defects A and C share a root shape: an action that mutates shared state without a guard against being
run twice. Defect B is a `Task.Run` lambda that captured more than the intended work — only the
computation needed to move off the UI thread, not the control read that feeds it.

Defect D is a literal-duplication problem; `BreadcrumbRowBuilder.BannerPrefix` already exists as the
single source of truth and should be referenced by both controller sites.

Discovered during preparation of issue #452 (epic #136) per-file coverage research. Out of scope there
under that feature's no-behavior-change constraint.

## Proposed Fix / Validation Ideas

- [ ] Add an `_isExecuting`-style re-entrancy guard mirroring `EfcHomeController.TryBeginExecuteMoves`
- [ ] Hoist the `SearchText.Text` read out of the `Task.Run` lambda onto the UI thread
- [ ] Add a dedupe guard before inserting the trash row, or stop writing the bound result back to `_folderRows`
- [ ] Reference `BreadcrumbRowBuilder.BannerPrefix` from both controller sites
- [ ] Unit coverage: double OK gesture; refresh-suggestions threading; repeated delete; three- and four-`=` rows
- [ ] Manual verification: repeated delete shows one trash row; filing via Return and via OK button both succeed

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
