# efc-form-populate-folder-null-ref (Issue #145)

- Date captured: 2026-05-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-form-populate-folder-null-ref/ (Issue #145)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #145
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/145
- Last Updated: 2026-05-07
- Work Mode: minor-audit

## Summary

`System.NullReferenceException` at `EfcFormController.PopulateFolderCombobox` line 950 (`await _formViewer.UiSyncContext`) caused by a race condition between `Cleanup()` and the async continuation of `PopulateFolderCombobox`.

## Environment

- OS/version: Windows / .NET Framework (VSTO)
- Command/flags used: N/A — triggered at Outlook startup/form open when `Cleanup()` races with an in-flight `PopulateFolderCombobox` call

## Steps to Reproduce

1. Open EFC form, triggering `_ = PopulateFolderCombobox()` fire-and-forget.
2. Before `InitFolderHandlerAsync` completes, `Cleanup()` is called from another execution path.
3. `Cleanup()` sets `_formViewer = null`.
4. `InitFolderHandlerAsync` completes and the continuation resumes at `await _formViewer.UiSyncContext` — `_formViewer` is now null.

## Expected Behavior

When `_formViewer` is null at the post-await resumption point, `PopulateFolderCombobox` should return early without throwing.

## Actual Behavior

`System.NullReferenceException` is thrown at line 950: `await _formViewer.UiSyncContext`.

## Logs / Screenshots

- [x] Debugger evaluation of `_formViewer` at exception frame returned null.
- Snippet: `_formViewer = null` in `Cleanup()` with no coordination with in-flight async operations.

## Impact / Severity

- [x] High

## Suspected Cause / Notes

`PopulateFolderCombobox` is always called fire-and-forget (`_ = PopulateFolderCombobox()`), so no caller awaits it or cancels it before `Cleanup()` runs. `Cleanup()` explicitly nulls `_formViewer` without coordinating with any in-flight async operations.

## Proposed Fix / Validation Ideas

Add a null guard for `_formViewer` immediately after `await _dataModel.InitFolderHandlerAsync(folderList)` in `PopulateFolderCombobox`:

```csharp
if (_formViewer is null) return; // Guard: Cleanup() may have run during the await above
```

- [x] Unit coverage: regression test in `EfcFormControllerTests.cs`

## Acceptance Criteria

- [x] AC1: `PopulateFolderCombobox` does not throw `NullReferenceException` when `_formViewer` is null at the post-await resumption point.
- [x] AC2: A null guard `if (_formViewer is null) return;` is present in `EfcFormController.PopulateFolderCombobox` immediately after `await _dataModel.InitFolderHandlerAsync(folderList)`.
- [x] AC3: A regression test in `EfcFormControllerTests.cs` documents the fix and verifies the maximum unit-testable aspect of the null guard behavior.
- [x] AC4: The full toolchain (csharpier, .NET analyzers, nullable checks, MSTest) passes without new failures.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch