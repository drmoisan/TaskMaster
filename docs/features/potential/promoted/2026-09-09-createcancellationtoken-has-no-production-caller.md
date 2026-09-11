# createcancellationtoken-has-no-production-caller (Issue #839)

- Date captured: 2026-09-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/createcancellationtoken-has-no-production-caller/ (Issue #839)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #839
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/839
- Last Updated: 2026-09-10
## Summary

`QfcHomeController.CreateCancellationToken()` has no production caller, so the synchronous `Init()` path passes a null `_tokenSource` downstream and item loading silently does nothing.

## Environment

- OS/version: Windows 11, Outlook VSTO host
- Python version: n/a (C#, net48)
- Command/flags used: n/a, runtime path
- Data source or fixture: `QuickFiler/Controllers/QfcHomeController.cs`

## Steps to Reproduce

1. Enter QuickFiler through `RibbonController.LoadQuickFiler()` at lines 97 to 110, which takes the synchronous `Init()` path at `QfcHomeController.cs:102`.
2. Observe that `CreateCancellationToken()` is never called on that path, leaving `_tokenSource` null.
3. Observe `QfcFormControllerLoader` receiving the null source, and `QfcFormController.Actions.cs` at lines 38, 75 and 131 early-returning on a null `_tokenSource`.

## Expected Behavior

`LoadItems` and `LoadItemsAsync` run, or the missing token source is reported as an error at construction time.

## Actual Behavior

`LoadItems` and `LoadItemsAsync` silently do nothing. The failure is a no-op with no log line and no exception.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none. The defect is the absence of any signal.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Surfaced during preparation of feature 821 in the review-residuals-2026-09-08 epic and recorded there as observation O-4, severity HIGH. Left out of that feature's blast radius deliberately. The early-return guards are individually defensible; the defect is that nothing establishes the invariant they guard.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: the `QfcHomeController.Init()` synchronous path, and `QfcFormController.Actions` guard behaviour with a non-null source
- [ ] Integration scenario to retest: QuickFiler launched from the ribbon
- [ ] Manual verification notes: confirm items load when entered through `RibbonController.LoadQuickFiler()`

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
