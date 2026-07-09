# qfc-collectioncontroller-removespecificcontrolgroup-counter-leak (Issue #286)

- Date captured: 2026-07-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-collectioncontroller-removespecificcontrolgroup-counter-leak/ (Issue #286)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #286
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/286
- Last Updated: 2026-07-09
## Summary

`QfcCollectionController.RemoveSpecificControlGroupAsync` (`QuickFiler/Controllers/QfcCollectionController.cs:1142-1233`) increments a static reentrancy counter at method entry but only decrements it at the normal exit path, with no `try`/`finally`. Any exception thrown mid-method leaves the counter permanently incremented, which then produces a false-positive "race condition" error log on every subsequent call for the remaining life of the process.

## Environment

- OS/version: n/a (logic defect, reproducible on any platform running QuickFiler)
- Python version: n/a
- Command/flags used: n/a
- Data source or fixture: `QuickFiler/Controllers/QfcCollectionController.cs`

## Steps to Reproduce

1. Inspect `QfcCollectionController.cs:1142`: `private static int removespecificcontrolgroupcounter = 0;`
2. Inspect `RemoveSpecificControlGroupAsync` (line 1144): `Interlocked.Increment(ref removespecificcontrolgroupcounter);` at line 1146 (method entry), followed by ~80 lines of method body (navigation unregister/register, active-item handling), then `Interlocked.Decrement(ref removespecificcontrolgroupcounter);` at line 1232 (method's final statement).
3. Note there is no `try`/`finally` wrapping lines 1146-1232.
4. Trigger any exception between the increment (line 1146) and the decrement (line 1232) — e.g. an unhandled exception from `UnregisterNavigation()`, `RegisterNavigation()`, or any of the active-item/theme handling in between.
5. The decrement at line 1232 never executes; `removespecificcontrolgroupcounter` remains permanently incremented above its pre-call value.
6. Every subsequent call to `RemoveSpecificControlGroupAsync` now sees `removespecificcontrolgroupcounter > 1` at line 1222 and logs `logger.Error("RemoveSpecificControlGroupAsync: Counter is greater than 1. Race Condition Exists")` — a false positive, since no concurrent reentrancy is actually occurring.

## Expected Behavior

The counter should always return to its pre-call value once the method returns or throws, so the race-condition check at line 1222 only fires on genuine concurrent reentrancy, not on a leaked count from an earlier unrelated exception.

## Actual Behavior

The decrement is skipped whenever the method body throws, permanently inflating the counter and causing the race-condition detector to report a false positive on every future call, masking the detector's ability to catch a real race condition later (since the counter is already above 1 regardless of actual concurrency).

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Confirmed directly against source at `QuickFiler/Controllers/QfcCollectionController.cs:1142` (counter field), `:1146` (increment), `:1222-1227` (race-condition check and `logger.Error` call), `:1232` (decrement) — no `try`/`finally` present. Originally identified in `docs/features/archive/2026-07-03-quickfiler-navigation-key-collision-232/evidence/other/follow-up-candidates.md` (item 3, "removespecificcontrolgroupcounter reentrancy-counter hygiene"), explicitly deferred as out of scope for issue #232. No open GitHub issue currently references this (verified via `gh issue list --search "RemoveSpecificControlGroup"`).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The counter is diagnostic/observability-only (it drives a log statement, not control flow), so this does not directly corrupt user-facing behavior. However it permanently degrades the reliability of the race-condition detector after the first unrelated exception, producing misleading error-log noise that could mask or be confused with a genuine future race condition.

## Suspected Cause / Notes

- The counter is also unsynchronized outside the `Interlocked` calls themselves: the comparison `removespecificcontrolgroupcounter > 1` at line 1222 is a plain (non-`Interlocked`) read, so it can itself race with concurrent increments/decrements from other threads, though that is a secondary concern to the primary leak-on-exception defect.
- The original follow-up note scoped this as "a broader reentrancy-hygiene fix beyond what was strictly necessary" for issue #232's reported `ArgumentException`, which was fixed separately without touching this counter logic.

## Proposed Fix / Validation Ideas

- [ ] Wrap the method body (lines 1146-1232) in `try`/`finally`, moving the `Interlocked.Decrement` call into the `finally` block so it always executes.
- [ ] Add a regression test that forces an exception mid-method (e.g. via a mocked dependency) and asserts the counter returns to its pre-call value afterward.
- [ ] Consider whether the plain (non-`Interlocked`) read at line 1222 should also be revisited for correctness, though this may be accepted as a best-effort diagnostic check rather than a strict invariant.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
