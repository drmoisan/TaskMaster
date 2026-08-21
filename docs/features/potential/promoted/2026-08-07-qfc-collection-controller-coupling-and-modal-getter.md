# qfc-collection-controller-coupling-and-modal-getter (Issue #474)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-collection-controller-coupling-and-modal-getter/ (Issue #474)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #474
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/474
- Last Updated: 2026-08-08
## Summary

Two design defects in `QfcCollectionController` that are structural rather than incidental: a
downcast from an interface-typed field to a concrete sibling controller that will throw for any other
implementation, and a property getter that shows a modal dialog as a side effect.

## Environment

- OS/version: n/a (design defects, reproducible wherever QuickFiler runs)
- Python version: n/a
- Command/flags used: n/a
- Data source or fixture: `QuickFiler/Controllers/QfcCollectionController.cs`

## Steps to Reproduce

**Defect 1 — concrete-type downcast to a sibling controller (`:1232`)**

1. `_parent` is declared as `IFilerFormController` at
   `QuickFiler/Controllers/QfcCollectionController.cs:64`.
2. Line 1232 executes `await ((QfcFormController)_parent).SkipGroupAsync();` — a downcast to the
   concrete type.
3. `SkipGroupAsync` is declared on `QuickFiler.Controllers.IQfcFormController`
   (`QuickFiler/Controllers/IQfcFormController.cs:38`).
4. That is a **different** interface from `QuickFiler.Interfaces.IFilerFormController`
   (`QuickFiler/Interfaces/IFilerFormController.cs:9-24`), which does not declare `SkipGroupAsync`.
5. Any `IFilerFormController` implementation other than `QfcFormController` therefore throws
   `InvalidCastException` at line 1232.
6. The root cause is that two interfaces model the same role and neither is a superset of the other.

**Defect 2 — modal dialog inside a property getter (`:152-194`)**

1. `ReadyForMove`'s getter iterates the item groups.
2. When any group lacks a destination folder, lines 186-191 show a modal `MessageBox`.
3. Reading a property therefore blocks on user interaction and cannot be evaluated on a background
   thread, in a test, or twice without side effects.

## Expected Behavior

1. Either `IFilerFormController` declares the member that `QfcCollectionController` needs, or the
   controller depends on the interface that does. No downcast to a concrete sibling should be
   required.
2. A readiness check should return a result the caller can inspect and act on. Presenting UI is the
   caller's decision, not the property's.

## Actual Behavior

1. `InvalidCastException` for any non-`QfcFormController` parent; the interface-typed field provides
   no real substitutability.
2. Reading `ReadyForMove` displays a modal dialog, which blocks the calling thread and makes the
   property untestable under the repository unit-test policy, which prohibits popups in tests.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Confirmed directly against source at `QfcCollectionController.cs:64` (field type), `:1232`
  (downcast), `IQfcFormController.cs:38` and `IFilerFormController.cs:9-24` (the two-interface split),
  and `QfcCollectionController.cs:152-194` (the getter, with the `MessageBox` at `:186-191`).
  Discovered during preparation research for issue #454 (epic #136, child F11); full analysis in
  `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/qfc-collection-controller.md`
  sections E8 and E10.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Neither defect misbehaves in the current single-implementation production configuration, so this is
latent rather than active. Both are load-bearing for testability and for any future alternative
implementation, and the modal getter is a standing violation of the separation between domain logic
and UI.

## Suspected Cause / Notes

The two-interface split (`QuickFiler.Controllers.IQfcFormController` versus
`QuickFiler.Interfaces.IFilerFormController`) looks like an incomplete consolidation. Resolving it is
the real fix for defect 1; the downcast is a symptom.

Note for scheduling: `QuickFiler/Controllers/IQfcFormController.cs` and
`QuickFiler/Interfaces/IFilerFormController.cs` are owned by child F6 (issue #435) of epic #136.
Whoever schedules this work should reconcile with F6 rather than editing those files independently.

Issue #454 (child F11) introduces injectable delegate seams around both call sites so the current
behavior is preserved bit-for-bit while becoming testable. That work does **not** fix either defect;
it only makes them reachable from tests. The structural fixes are deferred here because they are
behavior changes, which epic #136's no-behavior-change NFR excludes.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: assert the parent interaction works against a substitute
      `IFilerFormController`; assert a readiness check returns a result without presenting UI.
- [x] Integration scenario to retest: attempt a move with a group that has no destination folder and
      confirm the user-facing prompt still appears, driven by the caller.
- [x] Manual verification notes: consolidate the two form-controller interfaces first; the downcast
      removal follows from it.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
