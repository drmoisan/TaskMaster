# Remove the orphan `QuickFiler.Interfaces.IQfcFormController`

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Potential — not promoted
- Captures: **follow-up candidate 5** of `## Follow-up Candidates` in
  `docs/features/active/qfc-collection-controller-defects-468/spec.md`
- Origin: issue **#468** defect family, task `[P14-T5]`
- Origin feature folder: `docs/features/active/qfc-collection-controller-defects-468`

## Summary

Three types in this repository are named `IQfcFormController`:

| Type | Path and line | Status |
|---|---|---|
| `QuickFiler.Controllers.IQfcFormController` | `QuickFiler/Controllers/IQfcFormController.cs:13` | live; derives from `IFilerFormController` |
| `QuickFiler.Interfaces.IQfcFormController` | `QuickFiler/Interfaces/IQfcFormController.cs:7` | **orphan** — no implementer |
| `QuickFiler.Notes.IQfcFormController` | `QuickFiler/Notes/notes_interfaces.cs:13` | not compiled |

The orphan in the `QuickFiler.Interfaces` namespace has no implementing type. Its only referent is
`QuickFiler/Interfaces/IQfcHomeController.cs:9`, which declares
`IQfcFormController FrmCtrlr { get; }` and, being in the `QuickFiler.Interfaces` namespace, binds to
the orphan by same-namespace preference rather than to the live interface.

## Why this matters

It is a name-collision trap. Any future file placed in the `QuickFiler.Interfaces` namespace that
writes an unqualified `IQfcFormController` binds to the orphan silently and compiles, producing a
type that satisfies no production implementation. Issue #468's spec required an explicit
disambiguation note before any editing of `QfcCollectionController.cs` for exactly this reason.

## Why it was deferred rather than absorbed

Both files are outside the issue #468 branch's owned file set, and removing the orphan forces a
decision about `IQfcHomeController.FrmCtrlr` — whether it should bind to the live interface instead,
which is a contract change on a second interface. That is more than a deletion.

## Proposed approach when promoted

1. Confirm the orphan has no implementer: search for `: IQfcFormController` and for
   `QuickFiler.Interfaces.IQfcFormController` across the solution.
2. Decide the fate of `IQfcHomeController.FrmCtrlr`: either retype it to
   `QuickFiler.Controllers.IQfcFormController` (likely correct) or remove it.
3. Delete `QuickFiler/Interfaces/IQfcFormController.cs` and its `Compile Include` entry.
4. Consider whether `QuickFiler/Notes/notes_interfaces.cs` should also be deleted; it is not compiled
   and contributes a third same-named type to any future search.

## Acceptance ideas (for the promoted entry to refine)

- Exactly one compiled type is named `IQfcFormController`.
- `IQfcHomeController.FrmCtrlr` binds to the live interface, and every implementer compiles.
- The full solution builds with 0 errors and the full test suite stays green.
