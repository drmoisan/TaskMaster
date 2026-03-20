# triage-null-classifier-group (Potential Bug)

- Date captured: 2026-03-20
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`Triage.CreateNewTriageClassifierGroupAsync()` creates a `new BayesianClassifierGroup()` with an empty `Classifiers` dictionary and serializes it without seeding classifiers A, B, and C. On next startup, `HasValidTriageManagerAsync()` fails validation, `CreateAsync()` returns null, `AppItemEngines.InitAsync()` stores the null engine, and any Triage click handler (e.g., `TriageSetA_Click`) throws `NullReferenceException`.

## Environment

- OS/version: Windows
- .NET framework: 4.x (Outlook VSTO add-in)
- Trigger: Click "Clear Triage" button, then restart Outlook; click any Triage Set A/B/C button.

## Steps to Reproduce

1. Click the "Clear Triage" ribbon button (calls `ClearTriageAync()`).
2. Restart the Outlook add-in (or trigger engine re-initialization).
3. Click "Triage Set A" ribbon button.

## Expected Behavior

After clearing triage, the persisted classifier group should contain empty but valid classifiers A, B, and C so that subsequent load/train operations succeed.

## Actual Behavior

`System.NullReferenceException` at `RibbonViewer.TriageSetA_Click` (line 264) because `_controller.Triage` is null. Startup log shows: "Triage classifier group cannot find classifier named A. Skipping load."

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:
```
System.NullReferenceException
  HResult=0x80004003
  Message=Object reference not set to an instance of an object.
  Source=TaskMaster
  StackTrace:
   at TaskMaster.RibbonViewer.<TriageSetA_Click>d__56.MoveNext() in RibbonViewer.cs:line 264
```

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

`CreateNewTriageClassifierGroupAsync()` in `Triage.cs` (line ~286) uses `new BayesianClassifierGroup()` instead of `CreateClassifier()` (line ~199) which properly seeds classifiers A, B, C. The existing `CreateClassifier()` static method already does the right thing.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: Test that `CreateNewTriageClassifierGroupAsync` produces a group containing classifiers A, B, C.
- [x] Integration scenario to retest: Clear triage → re-init engine → verify non-null Triage engine with valid classifiers.
- [ ] Manual verification notes

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
