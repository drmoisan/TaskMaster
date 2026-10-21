# Issue #481 — `UnwireEvents()` call order inside the delivered `Cleanup()`

Timestamp: 2026-08-26T11-00
Task: [P5-T18]

Command (run from the worktree root):

```
grep -n "UnwireEvents();\|_itemViewer = null;\|_kbdHandler = null;" QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
```

EXIT_CODE: 0

## Delivered source line numbers in `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`

| Statement | Delivered line |
|---|---|
| `UnwireEvents();` | **458** |
| `_itemViewer = null;` (first occurrence) | **460** |
| `_kbdHandler = null;` | **473** |
| `_itemViewer = null;` (second, pre-existing duplicate) | 476 |

The `UnwireEvents()` call at line 458 precedes the statement that nulls the item viewer (line 460, and
its pre-existing duplicate at line 476) and precedes the statement that nulls the keyboard handler
(line 473). Constraint C4 rule 1 therefore holds in the delivered source.

The `spec.md` criterion cites the pre-change locators `ViewerSetup.cs:407` (item viewer) and
`:420` (keyboard handler), which are anchored to `<BASE_SHA>` per the "Line-citation anchor" paragraph
at the top of `spec.md`. Those citations are not renumbered; the delivered numbers are recorded here
instead, as the plan's constraint C2 rule 7 scope paragraph requires.

Output Summary: `UnwireEvents()` is at line 458; `_itemViewer = null;` is at 460 and 476;
`_kbdHandler = null;` is at 473. The call precedes both nulling statements in source order.
