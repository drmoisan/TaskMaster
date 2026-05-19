## [P2-T6] Minor-Audit End State

- Timestamp: `2026-04-06T12:03:00-04:00`
- Final Touched Production Files:
  - `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`
  - `TaskMaster\AppGlobals\AppOlObjects.cs`
- Final Touched Test Files:
  - `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs`
  - `TaskMaster.Test\AppGlobals\AppOlObjectsTests.cs`
- Phase 0 Artifacts:
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/phase0-instructions-read.md`
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/phase0-feature-folder-scope.md`
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/p0-t3-csharpier.md`
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/p0-t4-analyzers.md`
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/p0-t5-nullable.md`
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/p0-t6-mstest-coverage.md`
- Phase 2 Artifacts:
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t1-csharpier.md`
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t2-analyzers.md`
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t3-nullable.md`
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t4-mstest-coverage.md`
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t5-coverage-delta.md`
  - `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t6-end-state.md`
- Final QC Summary:
  - Formatter: pass
  - Analyzer build: pass
  - Nullable build: pass
  - MSTest coverage run: `3916` total, `3914` passed, `2` skipped, `0` failed
  - Coverage delta: touched production files passed no-regression; repo coverage threshold remained `FAIL` at `78.11%`

### Acceptance Criteria Coverage

- `[x]` The add-in exposes a clear UI entry point where the user can review the current `Junk Email` and `Junk Potential` folder selections without waiting for a folder-resolution failure.
  - Evidence: `docs/features/active/2026-04-05-select-junk-folders-119/evidence/other/p1-t1-scope.md`; `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`; `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs` (`PopulateWithCurrent_ShowsCurrentJunkSelectionsInViewer`)
- `[x]` The user can change the confirmed junk folder and the potential junk folder independently, using the standard Outlook folder picker for each field.
  - Evidence: `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`
- `[x]` Choosing `Save` persists the selected folders to the existing settings keys used by `AppOlObjects` (`OlJunkCertain` and `JunkPotential`) in the relative-path format expected by the current folder-loading logic.
  - Evidence: `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`; `TaskMaster\AppGlobals\AppOlObjects.cs`; `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs` (`SaveChanges_PersistsBothSettingsAndRefreshesActiveJunkFolders`)
- `[x]` After saving, subsequent junk or potential-junk operations use the updated folder selections without requiring the user to trigger the existing "folder not found" recovery path first.
  - Evidence: `TaskMaster\AppGlobals\AppOlObjects.cs`; `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs` (`SaveChanges_PersistsBothSettingsAndRefreshesActiveJunkFolders`)
- `[x]` Choosing `Cancel` leaves both stored settings and active folder selections unchanged.
  - Evidence: `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`; `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs` (`ButtonCancel_Click_LeavesStoredSettingsAndActiveFoldersUnchanged`)
- `[x]` If a previously saved folder no longer exists or cannot be resolved, the UI shows a clear re-selection path and does not overwrite the stored value with an invalid selection.
  - Evidence: `TaskMaster\AppGlobals\AppOlObjects.cs`; `TaskMaster.Test\AppGlobals\AppOlObjectsTests.cs` (`LoadJunkCertain_KeepsStoredValue_WhenReplacementSelectionIsCancelled`)
- `[x]` Unit coverage areas: reading existing settings into the UI model, saving updated `OlJunkCertain` and `JunkPotential` values, cancel behavior, cache refresh/reset behavior, and invalid or empty selection guards.
  - Evidence: `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs`; `TaskMaster.Test\AppGlobals\AppOlObjectsTests.cs`; `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t4-mstest-coverage.md`; `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t5-coverage-delta.md`
- `[ ]` Integration scenarios: open the entry point with valid existing folders, change only one folder, change both folders, save and reopen to confirm persistence, and recover after one of the saved folders is deleted, renamed, or moved.
  - Evidence: No dedicated integration execution artifact was added in this minor-audit plan; remains unchecked.
- `[ ]` CLI/API examples: none expected for this feature unless a diagnostic or developer-only command is added later; the primary surface is an in-add-in UI flow.
  - Evidence: No CLI/API surface was introduced; remains unchecked.
- `[ ]` Promote to GitHub issue (feature request template)
  - Evidence: Administrative workflow item; not part of this execution plan.
- `[ ]` Create `docs/features/active/select-junk-folders/` folder from the template
  - Evidence: Administrative workflow item; not part of this execution plan.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-04-05-select-junk-folders-119/issue.md`
- Total AC items: `6`
- Checked off (delivered): `6`
- Remaining (unchecked): `0`
