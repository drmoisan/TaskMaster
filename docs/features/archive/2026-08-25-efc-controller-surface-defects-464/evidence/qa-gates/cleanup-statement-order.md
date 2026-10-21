# [P5-T13] Delivered `Cleanup()` statement order against constraint C6

Timestamp: 2026-08-28T01-01
Task: [P5-T13]
Command: source inspection of the delivered `QuickFiler/Controllers/EfcFormController.cs` and
`QuickFiler/Controllers/EfcItemController.cs` with `awk` line numbering; no build or test invoked
EXIT_CODE: 0

Constraint C6, consumed as written from `484/spec.md:385-398` and `:400-408`:

1. Every event detach must precede the nulling of the field it detaches from.
2. `_timer?.Dispose()` must precede `_timer = null`.
3. `Cleanup()` must remain callable on a partially constructed controller and must be idempotent.
4. The 484 swallow-and-continue shape must **not** be copied into `EfcItemController`.

## `EfcFormController.Cleanup()` — declared at `:192`, body `:193-213`

| Statement | Delivered line |
|---|---|
| `DarkMode_Changed` detach (`globals.Ol.PropertyChanged -= DarkMode_Changed;`) | `:199` |
| `_globals` nulling (`_globals = null;`) | `:201` |
| `_parentCleanup` nulling (`_parentCleanup = null;`) | `:208` |
| guarded parent invocation (`parentCleanup.Invoke();`) | `:211` |

**C6 item 1 satisfied:** the detach at `:199` **precedes** the nulling of `_globals` at `:201`
(199 < 201).

**Single-invocation ordering satisfied:** the nulling of `_parentCleanup` at `:208` **precedes** the
guarded invocation at `:211` (208 < 211). The field is captured into a local at `:207` and cleared at
`:208` before the local is invoked, so a second `Cleanup()` call finds a null field and the guard at
`:209` skips the invocation.

**Unguarded dereferences: none.** The `_globals` read is captured into a local at `:196` and tested with
`globals?.Ol is not null` at `:197`, so neither `_globals` nor `_globals.Ol` is dereferenced on the
torn-down path. `_parentCleanup` is tested at `:209`.

**Count of `_parentCleanup` invocations in the method body: exactly 1**, at `:211`, guarded by the null
test at `:209`. Pre-change the body carried one unguarded invocation at `:193`.

## `EfcItemController.Cleanup()` — declared at `:227`, body `:228-265`

| Statement | Delivered line |
|---|---|
| mouse-handler detach loop (`foreach` over the captured button list) | `:234-238` (`MouseEnter` detach `:236`, `MouseLeave` detach `:237`) |
| `_buttons` nulling (`_buttons = null;`) | `:240` |
| `DarkMode_Changed` detach (`globals.Ol.PropertyChanged -= DarkMode_Changed;`) | `:245` |
| `_globals` nulling (`_globals = null;`) | `:247` |
| single `_itemViewer` nulling (`_itemViewer = null;`) | `:248` |
| timer disposal (`_timer?.Dispose();`) | `:263` |
| timer nulling (`_timer = null;`) | `:264` |

**C6 item 1 satisfied, both detaches:**
- the mouse-handler detach loop at `:234-238` **precedes** the nulling of `_buttons` at `:240`
  (238 < 240);
- the `DarkMode_Changed` detach at `:245` **precedes** the nulling of `_globals` at `:247` (245 < 247).

**C6 item 2 satisfied:** the timer disposal at `:263` **precedes** the timer nulling at `:264`
(263 < 264).

**Unguarded dereferences: none.** `_buttons` is captured into a local at `:231` and tested at `:232`,
replacing the pre-change unconditional `Buttons.ForEach` that threw `ArgumentNullException` on a
partially constructed controller. `_globals` is captured at `:242` and tested with `globals?.Ol is not
null` at `:243`.

**C6 item 4 satisfied:** the delivered body contains no `try`, no `catch`, and no continue-on-error
construct. The 484 swallow-and-continue shape was not copied.

## `_itemViewer` assignment count

| Measure | Value |
|---|---|
| Assignments of `_itemViewer` in the delivered `Cleanup()` body | **1**, at `:248` |
| Assignments of `_itemViewer` in the pre-change `Cleanup()` body | **2**, at `:264` and `:276` of the plan-cited pre-change text (`:231` and `:243` on this execution base) |

The duplicate second assignment was deleted, so exactly one assignment of `_itemViewer` remains, as
`spec.md` requires.

Output Summary: PASS. Both delivered `Cleanup()` methods satisfy every applicable clause of constraint
C6. In `EfcFormController` the detach at `:199` precedes the `_globals` nulling at `:201`, and the
`_parentCleanup` nulling at `:208` precedes the single guarded invocation at `:211`. In
`EfcItemController` the mouse-handler detach loop at `:234-238` precedes the `_buttons` nulling at
`:240`, the `DarkMode_Changed` detach at `:245` precedes the `_globals` nulling at `:247`, and the timer
disposal at `:263` precedes the timer nulling at `:264`. `EfcItemController.Cleanup()` contains exactly
one assignment of `_itemViewer`, against a pre-change count of two.
