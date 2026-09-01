# storewrapper-dialog-imprecise-for-genuine-failure (Issue #287)

- Date captured: 2026-07-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/storewrapper-dialog-imprecise-for-genuine-failure/ (Issue #287)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #287
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/287
- Last Updated: 2026-07-09
- Work Mode: full-bug

## Summary

`StoreWrapperController.Launch` (`UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:119-127`) shows the same "not available yet, try again after startup completes" dialog message for every non-`Ready` readiness state, including a genuine/permanent failure case where retrying will not help. The message is imprecise for that case.

## Environment

- OS/version: n/a (UI copy defect)
- Python version: n/a
- Command/flags used: n/a
- Data source or fixture: `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`

## Steps to Reproduce

1. Inspect `StoreWrapperController.Launch` at `StoreWrapperController.cs:119`: `if (readiness.State != StoreLaunchReadinessState.Ready)`.
2. Inside that single branch (lines 121-126), `MyBox.ShowDialog` always shows: "Store settings are not available yet. Please try again after startup completes." (title "Store Settings Unavailable").
3. This branch fires for every non-`Ready` `StoreLaunchReadinessState` value, which per the addressed-issue comment at lines 96-100 includes both a transient "not yet loaded" state and a genuine-failure state (`Globals.Ol.StoresWrapper` populated but permanently unable to resolve, vs. still loading).
4. A user hitting the genuine-failure case sees the same "try again after startup completes" copy as a user hitting the transient case, even though retrying will not resolve a genuine failure.

## Expected Behavior

The dialog copy should distinguish a transient "still starting up, try again shortly" case from a genuine/permanent failure case, so users are not told to retry when retrying cannot help.

## Actual Behavior

Both cases produce the identical message: "Store settings are not available yet. Please try again after startup completes."

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Confirmed directly against source at `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:119-127`. Explicitly flagged as a known, deliberately-deferred follow-up in `docs/features/active/2026-07-07-folder-settings-store-model-null-262/spec.md:60-62`: "Changing the `StoreWrapperController` 'not available yet' dialog copy for the genuine-failure case (imprecise but not required by any AC; documented follow-up only)." That feature (part of the now-merged epic #260, store-lockup-resilience) intentionally left this out of scope. No open GitHub issue currently references this dialog copy (verified via `gh issue list`).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Cosmetic/messaging-accuracy issue only; no functional or data-integrity impact. Confusing but not blocking, since the dialog still prevents the user from proceeding into a non-ready settings dialog either way.

## Suspected Cause / Notes

- `StoreLaunchReadinessState` and `EvaluateLaunchReadiness` (referenced in the doc comments at lines 14-30) already model distinct readiness states; the fix is likely to branch the dialog message on the specific non-`Ready` state rather than introducing new state-detection logic.
- Related work: issue #240 (`StoreWrapperController` null/transient-load handling) and the store-lockup-resilience epic (#260, features #261/#262/#263/#264/#265) own the surrounding readiness/disable/reenable behavior; this entry is scoped only to the dialog copy.

## Proposed Fix / Validation Ideas

- [ ] Add a distinct dialog message (and/or title) for the genuine-failure `StoreLaunchReadinessState` value(s) that does not suggest retrying will help.
- [ ] Add a test asserting the dialog copy shown for the genuine-failure state differs from the transient "still starting up" state.
- [ ] Confirm with the maintainer what the genuine-failure copy should say (e.g. pointing at logs or a support path) before implementing.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
