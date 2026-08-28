# qfc-unregister-navigation-count-mismatch-orphan (Issue #644)

- Date captured: 2026-08-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-unregister-navigation-count-mismatch-orphan/ (Issue #644)
- Discovered during: issue #444 / #472 / #482 (`quickfiler-keyboard-action-defects`), epic `quickfiler-bug-family`
- Recorded in: `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md`, `### Downstream notes` item 3

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #644
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/644
- Last Updated: 2026-08-27
## Summary

`QfcCollectionController.UnregisterNavigation` bounds its unregister loop with the *current*
`_itemGroups.Count`, while `RemoveSpecificControlGroup(int)` mutates `_itemGroups` with no
unregister/register bracket around the mutation. When a group is removed through that unbracketed
path, the count the unregister loop later reads no longer matches the count in force when the
navigation keys were registered, so the loop stops short and leaves orphaned `KbdActions`
navigation registrations behind. Every production call site discards `KbdActions.Remove`'s `bool`
result, so the divergence is silent until a later `Add` or `Find` throws
(`ArgumentException` or `InvalidOperationException`). The unbracketed mutation is reachable from
`RemoveBelowThresholdAsync` via the `RemoveGroupByEntryId` seam, and from the `'R'` char action in
`QfcItemController.EventWiring.cs`. This is a distinct defect from the register/unregister
digit-width mismatch filed as #472: #472 concerns the *format* of the keys removed, this concerns
the *number* of them. Fixing it requires the key-ledger design — recording the exact set of keys
registered and replaying that set on unregistration — which changes the outcome of the existing
characterisation tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, a file at
the 500-line ceiling whose `[TestMethod]` count issue #468 froze. It was therefore deliberately
kept out of #472's scope under the `CLAUDE.md` Bugfix Workflow rule that a deeper design problem
uncovered mid-fix opens a new issue instead of widening scope.

## Environment

- OS/version: Windows 11, Outlook VSTO add-in host
- Python version: n/a (C# / .NET Framework)
- Command/flags used: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
- Data source or fixture: `QfcCollectionController` with an `_itemGroups` collection crossing a group removal

## Steps to Reproduce

1. Bring up the QuickFiler collection surface with enough item groups that navigation keys are
   registered for each group.
2. Remove a group through an unbracketed path — either `RemoveBelowThresholdAsync` (which reaches
   `RemoveSpecificControlGroup` through the `RemoveGroupByEntryId` seam) or the `'R'` char action
   wired in `QfcItemController.EventWiring.cs`. No `UnregisterNavigation` / `RegisterNavigation`
   bracket surrounds this mutation.
3. Trigger `UnregisterNavigation`. Its loop bound is now the reduced `_itemGroups.Count`, so it
   iterates fewer times than the registration did.
4. Re-register navigation, or press a navigation key that resolves against the stale registry.

## Expected Behavior

Unregistration removes exactly the set of navigation keys that registration added, regardless of any
`_itemGroups` mutation that occurred in between. A subsequent registration succeeds, and a navigation
keypress resolves against exactly one handler.

## Actual Behavior

One or more navigation registrations are orphaned in the `KbdActions` registry. Because every call
site discards `Remove`'s `bool` return, nothing reports the failure at the point it happens. The
symptom surfaces later as an `ArgumentException` from a duplicate `Add`, or an
`InvalidOperationException` from a `Find` that resolves against a multi-element match set.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not captured. The defect was established by static reading of the control flow during
  #472's root-cause analysis, not by a captured runtime trace. The residual orphan it produces IS
  observable: #472's width-fidelity regression test in
  `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` asserts the residual
  entry explicitly and attributes it, by XML documentation comment, to this follow-up issue rather
  than silently absorbing it.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: the failure is silent at the point of divergence and surfaces later as a thrown exception in
a keyboard path. In the QuickFiler surface the resulting exception is caught and logged in
`KeyboardHandler`, so the user-visible symptom is a dead navigation key rather than a crash. It is
not a blocker because the unbracketed removal paths are not on the common navigation flow.

## Suspected Cause / Notes

- `QfcCollectionController.UnregisterNavigation` — loop bound reads the live `_itemGroups.Count`.
- `QfcCollectionController.RemoveSpecificControlGroup(int)` — mutates `_itemGroups` with no
  unregister/register bracket.
- `QfcCollectionController.RemoveBelowThresholdAsync` — reaches the above through the
  `RemoveGroupByEntryId` seam.
- `QuickFiler/Controllers/QfcItemController.EventWiring.cs` — the `'R'` char action reaches it too.
- Compounding factor: all 42 production call sites of `KbdActions.Remove` discard its `bool` result.
  31 of those are in `QfcItemController.EventWiring.cs`. That cross-cutting question is recorded
  separately in `### Downstream notes` item 5 of the #444 spec and is not this issue.
- Line-number citations are deliberately omitted here: the #444/#472/#482 work and epic sibling #468
  both edit `QfcCollectionController.cs`, so any line number recorded today is stale on arrival.
  Every anchor above is a member name.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: a key-ledger in `QfcCollectionController` that records the exact
  `(SourceId, Key)` set produced by `RegisterNavigation` and replays that recorded set in
  `UnregisterNavigation`, making unregistration total and independent of any intervening
  `_itemGroups` mutation. This supersedes both the count bound and the `_registeredDigits` width
  field that #472 introduces.
- [x] Integration scenario to retest: remove a group through `RemoveBelowThresholdAsync` and through
  the `'R'` char action, then unregister and re-register navigation, asserting the registry is empty
  between the two.
- [x] Manual verification notes: the key-ledger design changes the outcome of the existing
  characterisation tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`. That file
  is at the 500-line ceiling and its `[TestMethod]` count was frozen by issue #468, so this fix must
  either be scheduled after that freeze is lifted or place its tests in a new file.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
