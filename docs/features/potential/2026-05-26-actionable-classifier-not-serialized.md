# actionable-classifier-not-serialized (Potential Bug)

- Date captured: 2026-05-26
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`SerializeFolderManagerAsync` in `EmailFiler` only serializes the `Folder` classifier; the `Actionable` classifier is never written to disk, so `ManagerActionable.json` remains empty after every run. Additionally, `TrainActionableAsync` was training on `"None"` label entries, diluting the actionable classifier model.

## Environment

- OS/version: Windows / Outlook VSTO add-in
- Command/flags used: SortAsync() called from EmailFiler
- Data source or fixture: ManagerActionable.json in %APPDATA%

## Steps to Reproduce

1. Run the Outlook add-in and process emails through SortAsync.
2. Inspect `%APPDATA%\...\ManagerActionable.json`.

## Expected Behavior

`ManagerActionable.json` is written after each SortAsync call, containing the trained Bayesian classifier state.

## Actual Behavior

`ManagerActionable.json` is always empty / missing. Trained actionable state is discarded on process exit.

## Logs / Screenshots

- [x] Root cause confirmed via code inspection

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

`SerializeFolderManagerAsync` calls `(await Globals.AF.Manager["Folder"]).Serialize()` but never calls `(await Globals.AF.Manager["Actionable"]).Serialize()`. Additionally, `TrainActionableAsync` trains on all Actionable values including `"None"`, which dilutes classifier accuracy.

## Proposed Fix / Validation Ideas

- [x] Add `(await Globals.AF.Manager["Actionable"]).Serialize()` to `SerializeFolderManagerAsync`
- [x] Guard `TrainActionableAsync` to skip training when `mailHelper.Actionable == "None"`
- [x] Unit test: `TrainActionableAsync_WhenActionableIsNone_DoesNotTrainClassifier`
- [x] Existing `CallSerializeFolderManagerAsync` test already exercises both serialize paths

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
