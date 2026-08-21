# efc-item-controller-cleanup-nre-and-timer-leak (Issue #460)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-item-controller-cleanup-nre-and-timer-leak/ (Issue #460)
- Work Mode: full-bug

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #460
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/460
- Last Updated: 2026-08-08
## Summary

`EfcItemController.Cleanup()` throws `NullReferenceException` for controllers built through one of the
public constructors, leaks an armed `System.Threading.Timer`, and leaves the `Subject` property in an
inconsistent post-cleanup state relative to its sibling properties.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in
- UI path: `QuickFiler/Controllers/EfcItemController.cs` item teardown path
- Data source or fixture: n/a

## Steps to Reproduce

1. Construct an `EfcItemController` through the `(globals, homeController, parent, itemViewer, token)`
   constructor.
2. Call `Cleanup()` without first calling `InitializeWithoutData()` or `InitializeDataFields()`.
3. Observe `NullReferenceException`.

For the timer leak:

1. Expand an unread item so the mark-as-read timer is armed.
2. Clean up the item while it is still expanded and unread.
3. Observe that the `System.Threading.Timer` is dropped without being disposed.

## Expected Behavior

- `Cleanup()` is safe to call on any constructed controller regardless of which initializer ran.
- Disposable resources owned by the controller are disposed, not merely dereferenced.
- Property accessors behave consistently with each other after cleanup.

## Actual Behavior

**A — unconditional dereference (`EfcItemController.cs:257`).** `Cleanup()` dereferences `Buttons`
(backing field `_buttons`). `_buttons` is only assigned in `ResolveControlGroups` (`:341`), which the
`(globals, homeController, parent, itemViewer, token)` constructor never runs. `Cleanup` also never
nulls `_buttons` while nulling 15 sibling fields, and `_itemViewer = null` is written twice (`:264`
and `:276`).

**B — timer leaked (`EfcItemController.cs:277`).** `_timer = null` is assigned **without disposing**
the timer, leaking an armed `System.Threading.Timer` whenever the item is cleaned up while expanded
and unread.

**C — inconsistent post-cleanup property behavior (`EfcItemController.cs:610-613` vs `:595-598`).**
`Subject` reads `_itemViewer.LblSubject.Text` while `Sender` and `To` read `_itemInfo`. After
`Cleanup()` nulls `_itemViewer` (`:264`), `Subject` throws while `Sender` still works. Before
`PopulateControls` runs, `Subject` returns the Designer placeholder rather than the mail subject.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence recorded above (verified 2026-08-07 against the working tree).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Defect A is an unhandled exception on a teardown path. Defect B leaks a live OS timer per affected
item, which accumulates across a filing session.

## Suspected Cause / Notes

`Cleanup()` appears to have been written against the fully-initialized object graph produced by one
constructor path and never revisited when the lighter constructor overloads were added. The duplicated
`_itemViewer = null` assignment suggests the method has been edited incrementally without review of the
whole body.

Reading `Subject` from the viewer rather than the model (defect C) is the underlying inconsistency; the
model-backed read used by `Sender`/`To` is the more robust shape.

Discovered during preparation of issue #452 (epic #136) per-file coverage research. Out of scope there
under that feature's no-behavior-change constraint.

## Proposed Fix / Validation Ideas

- [ ] Guard or null-condition the `Buttons` dereference and null `_buttons` alongside its siblings
- [ ] Dispose `_timer` before nulling it
- [ ] Read `Subject` from `_itemInfo` for consistency with `Sender`/`To`, or document why it differs
- [ ] Remove the duplicated `_itemViewer = null` assignment
- [ ] Unit coverage: cleanup after each constructor overload; double cleanup; cleanup while timer armed
- [ ] Manual verification: filing session with many expanded unread items shows no timer growth

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
