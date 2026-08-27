# Remove the `stackMovedItems` parameter from `MoveEmailsAsync` (Issue #629)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/Remove_the_stackMovedItems_parameter_from_MoveEmailsAsync/ (Issue #629)
- Captures: **follow-up candidate 2** of `## Follow-up Candidates` in
  `docs/features/active/qfc-collection-controller-defects-468/spec.md`
- Origin: issue **#468** defect family, task `[P14-T5]`
- Origin feature folder: `docs/features/active/qfc-collection-controller-defects-468`

- Issue: #629
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/629
- Last Updated: 2026-08-26
## Summary

`MoveEmailsAsync` declares a `SloStack<IMovedMailInfo>` parameter that its body does not use to
populate the undo stack. Issue #469 defect 4 established the truth of the matter: the undo record is
written by `EmailFiler.PushToUndoStack`
(`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:185-189`) onto the same global
stack instance the caller passes, so the parameter is redundant rather than wrong.

Issue #468's branch documented that contract and added an explicit `_ = stackMovedItems;` discard
(`QuickFiler/Controllers/QfcCollectionController.cs:2260`) rather than removing the parameter,
because removal reaches a file outside that feature's owned set.

## Current shape

| Site | Path and line |
|---|---|
| Interface declaration | `QuickFiler/Interfaces/IQfcCollectionController.cs:63` — `Task MoveEmailsAsync(SloStack<IMovedMailInfo> StackMovedItems);` |
| Implementation | `QuickFiler/Controllers/QfcCollectionController.cs:2253` |
| Discard statement | `QuickFiler/Controllers/QfcCollectionController.cs:2260` |
| Only call site | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225` — `await _groups.MoveEmailsAsync(_movedItems);` |

## Why it was deferred rather than absorbed

`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` is outside the owned file set of the
issue #468 branch and is named by that branch's scope lock as must-not-touch. Per decision D11 of
`docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md`, the branch
documents and consumes the parameter instead of removing it. The change buys signature tidiness, not
behaviour, so it did not justify widening the blast radius of a seven-defect bugfix.

## Proposed approach when promoted

1. Delete the parameter from `QuickFiler/Interfaces/IQfcCollectionController.cs:63`.
2. Delete the parameter and the `_ = stackMovedItems;` discard from
   `QuickFiler/Controllers/QfcCollectionController.cs:2253-2260`, retaining the XML doc block's
   statement of how the undo stack is actually populated.
3. Update the single call site to `await _groups.MoveEmailsAsync();`.
4. Search `QuickFiler.Test` for any `Mock<IQfcCollectionController>` whose `Setup` names the
   parameter; adjust the setups.

## Acceptance ideas (for the promoted entry to refine)

- `MoveEmailsAsync` takes zero parameters on the interface and on the implementation.
- `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack`
  (`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`) is retired or
  rewritten, because the argument shapes it distinguishes no longer exist.
- The full `QuickFiler.Test` suite stays green, and undo after a batch move is still exercised.
