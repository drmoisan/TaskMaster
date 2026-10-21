# Phase 5 — AC16 enumerations survive in spec.md

Timestamp: 2026-09-09T13-25
Task: [P5-T4]

Target file:
`docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/spec.md`

Command: a `Select-String -SimpleMatch` search for each of the three literals.
EXIT_CODE: 0

Verbatim output:

```text
Derived holder count: 9 => 1 @ 294
Derived count of sites that call => 1 @ 302
exactly ONE additional unguarded compiled site => 1 @ 259
```

## Result

| # | Literal | Matches | spec.md line |
|---|---|---|---|
| 1 | `Derived holder count: 9` | **1** | 294 |
| 2 | `Derived count of sites that call` | **1** | 302 |
| 3 | `exactly ONE additional unguarded compiled site` | **1** | 259 |

Each of the three literals returns at least one match, so all three derived figures survive in the
delivered spec:

- **9 holders** of the token source constructed at `QuickFiler/Controllers/QfcHomeController.cs`
  line 54 — Enumeration 2, derived twice on independent axes with identical member sets.
- **4 sites that call `Cancel()`** on that instance — the narrower family, of which `ProgressViewer`
  is the fourth and last.
- **1 additional unguarded compiled cleanup site beyond Site A** — Enumeration 1, which is
  `QuickFiler/Controllers/EfcHomeController.cs` line 349, the site this fix brought into scope as
  Site A'.

Both enumeration tables remain present in the spec with their derived figures intact, so the next
reviewer inherits the enumeration rather than re-deriving it. This is the half of AC16 concerning
what must survive; `[P5-T3]` records the half concerning what must not appear, at 0 matches for the
wrong label across all `*.cs` files.

Output Summary: all three enumeration literals return at least one match in `spec.md`, at lines 294,
302 and 259 respectively.
