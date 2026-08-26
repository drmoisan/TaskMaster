# Consolidate `IFilerFormController` and `IQfcFormController`

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Potential — not promoted
- Captures: **follow-up candidate 4** of `## Follow-up Candidates` in
  `docs/features/active/qfc-collection-controller-defects-468/spec.md`
- Origin: issue **#474** defect 1, task `[P14-T5]`
- Origin feature folder: `docs/features/active/qfc-collection-controller-defects-468`

## Summary

`QuickFiler.Controllers.IQfcFormController` derives from `QuickFiler.Interfaces.IFilerFormController`
(`QuickFiler/Controllers/IQfcFormController.cs:13`) and is a strict superset of it. The two are not
unrelated siblings — a premise in the promoted research document that issue #468's branch verified as
false and corrected.

Issue #474 defect 1 was fixed by the minimal change: retype `QfcCollectionController._parent` and its
constructor parameter from the base interface to the derived one, which removes the runtime downcast
to the concrete `internal QfcFormController`. That fix leaves the two-interface arrangement in place.

Whether the split earns its keep is a separate question. If no type ever implements
`IFilerFormController` without also implementing `IQfcFormController`, the base interface is an
abstraction with one realisation and could be folded in.

## Blast radius

Option (c) of the issue #474 option table. It touches both interface files plus:

- `QuickFiler/Controllers/QfcFormViewer.cs`
- `QuickFiler/Interfaces/IQfcFormViewer.cs`
- `QuickFiler/Interfaces/IFilerHomeController.cs`
- `QuickFiler/Controllers/QfcHomeController.cs`
- `QuickFiler/Controllers/EfcHomeController.cs`
- `QuickFiler/Controllers/EfcFormController.cs`

and roughly eight test files.

## Why it was deferred rather than absorbed

It is a refactor, not a bugfix. CLAUDE.md's Bugfix Workflow requires changing only what is needed to
make the failing test pass and avoiding opportunistic refactors, and the issue #468 branch names
`QuickFiler/Controllers/EfcFormController.cs` explicitly as must-not-touch.

## Proposed approach when promoted

Answer the design question first: enumerate every implementer of `IFilerFormController` and check
whether any is not also an `IQfcFormController`. `EfcFormController` is the obvious candidate for a
second realisation, and if it is one, consolidation is the wrong move and this entry should be closed
as declined rather than implemented.

## Acceptance ideas (for the promoted entry to refine)

- A written enumeration of every implementer of each interface, with the decision it supports.
- If consolidation proceeds: one interface remains, every listed file compiles unchanged in
  behaviour, and the full `QuickFiler.Test` suite stays green.
- If consolidation is declined: the rationale is recorded and the entry is closed without a code
  change.
