# qfc-collection-navigation-digits-desync (Issue #472)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-collection-navigation-digits-desync/ (Issue #472)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #472
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/472
- Last Updated: 2026-08-08
## Summary

`QfcCollectionController.RegisterNavigation` captures the side-effecting `Digits` property once,
while `UnregisterNavigation` re-evaluates it inside its loop. If the item count crosses the 10-item
boundary between the two calls, keys are unregistered under different names than they were
registered, leaving orphaned key registrations that later collide. This is the same failure family as
issue #232.

## Environment

- OS/version: n/a (logic defect, reproducible wherever QuickFiler runs)
- Python version: n/a
- Command/flags used: n/a
- Data source or fixture: `QuickFiler/Controllers/QfcCollectionController.cs`

## Steps to Reproduce

1. Inspect `RegisterNavigation` at `QuickFiler/Controllers/QfcCollectionController.cs:1330-1341`.
   Line 1332 captures the value once: `var digits = Digits;`, then passes that captured value to
   every `RegisterNavigationAsyncAction` call.
2. Inspect `UnregisterNavigation` at `:1343-1356`. Line 1347 re-evaluates `Digits` **inside** the
   loop, once per iteration.
3. Inspect the `Digits` property at `:114-128`. It reads `_itemGroups?.Count` live and returns 1 for
   counts below 10 and 2 at or above 10. It is side-effecting: it also drives
   `SetVisualDigits` via `_digitRefreshNeeded`.
4. Register navigation while the collection holds 10 or more items, so keys are registered as
   `"01".."09"`.
5. Allow the item count to drop below 10 (for example by filing messages) before
   `UnregisterNavigation` runs.
6. `UnregisterNavigation` now computes `digits == 1` and attempts to remove `"1".."9"`.
7. `KbdActions.Remove` (`QuickFiler/Controllers/KbdActions.cs:123-135`) returns `false` **silently**
   when the key is absent; nothing surfaces the mismatch.
8. The original `"01".."09"` registrations remain. A later `Add` of the same key collides in
   `KbdActions.Add` (`KbdActions.cs:90-98`) and throws `ArgumentException`.

## Expected Behavior

The digit width used to unregister a navigation key must be the same width used to register it, so
that every registered key is removed exactly once and no orphan remains.

## Actual Behavior

When the count crosses the 10-item boundary between register and unregister, the removal silently
no-ops and the stale registrations persist until a later `Add` throws `ArgumentException`.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Confirmed directly against source at `QfcCollectionController.cs:1332` (single capture),
  `:1347` (per-iteration re-evaluation), `:114-128` (the live, side-effecting `Digits` property), and
  `KbdActions.cs:123-135` (the silent `false` return). Discovered during preparation research for
  issue #454 (epic #136, child F11); full analysis in
  `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/qfc-collection-controller.md`
  section E14.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Produces a delayed, hard-to-attribute `ArgumentException` in keyboard registration rather than an
immediate failure at the point of the mismatch. Directly analogous to issue #232, which was a
navigation-key collision on page swap.

## Suspected Cause / Notes

The asymmetry looks unintentional: the two methods are otherwise mirror images, and only the
placement of the `Digits` read differs. A secondary concern is that `Digits` is a property with side
effects, which makes reading it inside a loop hazardous in general.

`KbdActions.Remove` returning `false` silently is what converts this from a loud failure into a
latent one; consider whether that return value should be checked at the call site.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: register with count >= 10, drop the count below 10, unregister, and assert
      no orphaned registrations remain; assert `UnregisterNavigation` uses a single captured digit
      width.
- [x] Integration scenario to retest: fill a page with 10 or more messages, file several, then swap
      pages and confirm no `ArgumentException` in keyboard registration.
- [x] Manual verification notes: reconcile with issue #232's fix so the two do not diverge again.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
