# Relocate the `ReadyForMove` presentation to the caller

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Potential — not promoted
- Captures: **follow-up candidate 3** of `## Follow-up Candidates` in
  `docs/features/active/qfc-collection-controller-defects-468/spec.md`
- Origin: issue **#474** defect 2, task `[P14-T5]`
- Origin feature folder: `docs/features/active/qfc-collection-controller-defects-468`

## Summary

Issue #474 defect 2 was that move readiness could not be evaluated without presenting a modal dialog:
the `ReadyForMove` property called `MessageBox.Show` on its false path, so any caller that merely
wanted to *ask* whether the collection was ready also *told the user* it was not.

Issue #468's branch fixed the testability half of this by splitting the evaluation from the
notification inside the controller: `internal bool TryGetMoveReadiness(out string notifications)`
carries the evaluation, and a private injectable delegate `_notifyNotReady` carries the notification,
defaulting to the unchanged modal call. The property still shows the dialog, so production behaviour
is unchanged.

The preferred end state — recorded in
`2026-08-07-qfc-collection-controller-coupling-and-modal-getter.md:52-54` — goes further: the dialog
should not live in the collection controller at all.

## Proposed approach when promoted

1. Add `bool TryGetMoveReadiness(out string notifications)` to `IQfcCollectionController`.
2. Remove the dialog from the `ReadyForMove` getter, and remove the `_notifyNotReady` delegate along
   with it.
3. Move the `MessageBox` presentation into the `else` branch of `ActionOkAsync` in
   `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, where a form-owning controller is the
   appropriate place to present a modal.

## Why it was deferred rather than absorbed

Two reasons, both structural. It edits `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
which is outside the issue #468 branch's owned file set. And it changes the
`IQfcCollectionController` contract, which the branch's scope lock forbids: the branch's task text
for `[P13-T1]` says explicitly "Do not add `TryGetMoveReadiness` to `IQfcCollectionController`."

## Unresolved prerequisite

Research did not exhaustively verify whether any `Mock<IQfcCollectionController>` exists in
`QuickFiler.Test`. Adding a member to that interface breaks every hand-written test double that
implements it, while Moq-generated mocks auto-implement the new member. That search must be run
before committing to the contract change, and its result determines the size of the test-side diff.

## Acceptance ideas (for the promoted entry to refine)

- `MessageBox.Show` does not occur in `QuickFiler/Controllers/QfcCollectionController.cs` at all.
- `IQfcCollectionController` declares `TryGetMoveReadiness(out string notifications)`.
- A test drives `ActionOkAsync`'s not-ready branch through an injected presentation seam and asserts
  the notification text, with no dialog presented.
- The two existing readiness tests in
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` are retargeted at the
  interface member and still pass.
