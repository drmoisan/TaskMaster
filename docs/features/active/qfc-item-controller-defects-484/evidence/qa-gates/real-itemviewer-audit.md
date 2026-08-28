# Real `QuickFiler.ItemViewer` construction audit

Timestamp: 2026-08-26T14-12
Task: [P7-T10]

Command (run from the worktree root):

```
grep -n "new QuickFiler.ItemViewer\|new ItemViewer(" QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
```

EXIT_CODE: 0

Output (verbatim):

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:237:                var viewer = new QuickFiler.ItemViewer();
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:328:                var viewer = new QuickFiler.ItemViewer();
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:433:                var viewer = new QuickFiler.ItemViewer();
QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:395:                var viewer = new QuickFiler.ItemViewer();
QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:433:                        new QuickFiler.ItemViewer()
```

| File | `[P0-T16]` baseline | Measured now |
|---|---|---|
| `QfcItemController.FocusAndThemeTests.cs` | 0 | 0 |
| `QfcItemController.EventWiringTests.cs` | 2 | **3** |
| `QfcItemController.ViewerSetupTests.cs` | 2 | 2 |
| `QfcItemController.MailActionsTests.cs` | 0 | 0 |
| `QfcItemController.TestSupport.cs` | 0 | 0 |
| **Total** | **4** | **5** |

The total is exactly **5**, which is the `[P0-T16]` baseline of 4 plus one.

## The single added construction

The added construction is at `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:433`,
inside `UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers`. It is the only
real `ItemViewer` construction added by this feature. That test:

- calls no `Show()` — the viewer is never made visible;
- starts no message pump and no worker thread;
- saves the ambient `SynchronizationContext` into a local, installs a plain `SynchronizationContext`,
  and restores the saved value in a `finally` block;
- raises the control events it needs through the protected `Control.On*` methods by reflection rather
  than through a live input path.

It mirrors the pre-existing headless fixture in the same file, whose two constructions at lines 237 and
328 are unchanged: constraint C2 rule 5 forbids refactoring, renaming, or shortening those two tests,
and neither was touched, which keeps this arithmetic deterministic.

Output Summary: 5 real `QuickFiler.ItemViewer` constructions across the five owned test files, the
baseline 4 plus exactly one added inside
`UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers`, which is headless.
