# Constraint C4 — `Cleanup()` statement order in the delivered source

Timestamp: 2026-08-26T11-10
Task: [P6-T4]

Command (run from the worktree root):

```
grep -n "UnwireEvents();\|_itemViewer = null;\|_kbdHandler = null;\|_emailIsReadTimer?.Dispose();\|_emailIsReadTimer = null;\|BreadcrumbUnhandledArrow -=\|_breadcrumbViewer = null;" QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
```

EXIT_CODE: 0

`Cleanup()` is declared at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:447`.

## The seven recorded statements

| # | Statement | Delivered line |
|---|---|---|
| 1 | `UnwireEvents();` | **458** |
| 2 | `_itemViewer = null;` (first occurrence; a pre-existing duplicate follows at 476) | **460** |
| 3 | `_kbdHandler = null;` | **473** |
| 4 | `_emailIsReadTimer?.Dispose();` | **478** |
| 5 | `_emailIsReadTimer = null;` | **479** |
| 6 | `_breadcrumbViewer.BreadcrumbUnhandledArrow -= OnBreadcrumbUnhandledArrow;` | **454** |
| 7 | `_breadcrumbViewer = null;` | **455** |

## The three constraint C4 rules

1. **The unwire call precedes both nulling statements.** `UnwireEvents()` at line 458 precedes
   `_itemViewer = null;` at line 460 (and its pre-existing duplicate at 476) and precedes
   `_kbdHandler = null;` at line 473. Rule 1 holds.
2. **The timer disposal precedes the timer nulling.** `_emailIsReadTimer?.Dispose();` at line 478
   precedes `_emailIsReadTimer = null;` at line 479. Rule 2 holds.
3. **The breadcrumb detach precedes the breadcrumb-viewer nulling.** The
   `BreadcrumbUnhandledArrow -=` detach at line 454 precedes `_breadcrumbViewer = null;` at line 455,
   unchanged from `BASE_SHA`. Rule 3 holds.

The `spec.md` criteria cite pre-change locators anchored to `<BASE_SHA>` (`ViewerSetup.cs:407`, `:420`,
`:424`) as the "Line-citation anchor" paragraph at the top of `spec.md` states. Those citations are not
renumbered; the delivered line numbers are recorded here instead.

Output Summary: All seven line numbers recorded. The unwire call (458) precedes both the item-viewer
nulling (460, 476) and the keyboard-handler nulling (473); the timer disposal (478) precedes the timer
nulling (479); and the breadcrumb detach (454) precedes the breadcrumb-viewer nulling (455). All three
constraint C4 rules hold.
