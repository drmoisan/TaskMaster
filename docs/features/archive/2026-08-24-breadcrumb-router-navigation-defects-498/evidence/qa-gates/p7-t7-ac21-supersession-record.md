# P7-T7 — Decision-D1 Supersession Record (AC-21)

Timestamp: 2026-08-26T11-08

Command: `pwsh -NoProfile -Command 'Select-String -LiteralPath "docs/features/active/breadcrumb-router-navigation-defects-498/spec.md" -SimpleMatch -Pattern "AC-9 supersession record" | ForEach-Object { "{0}:{1}" -f $_.LineNumber, $_.Line }; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**PASS.** The fixed-string search for the literal `AC-9 supersession record` returned **2 matches** in
`docs/features/active/breadcrumb-router-navigation-defects-498/spec.md`, at least one of which is a
section heading, as required:

```
304:#### #400 AC-9 supersession record (reviewer-findable)
1099:      "#400 AC-9 supersession record") naming exactly which clause of #400 AC-9
```

Line 304 is the reviewer-findable section heading. Line 1099 is the AC-21 criterion text itself, which
names the heading it requires.

### The four disposition-table rows, quoted verbatim from `spec.md:306-311`

| Clause of #400 AC-9 (`spec.md:247`) | Disposition under this feature |
|---|---|
| "Left and Right preserve the existing breadcrumb **expand, collapse** ... behavior in both view modes" | **RETRACTED IN PART.** Retracted only for rows whose resolved ancestor chain has more than one segment, and only to the extent that a new tree transition is attempted first. Where no tree transition is available, the existing expand/collapse behavior runs unchanged. |
| "... and **unhandled-key** behavior ..." | **PRESERVED.** The `unhandledArrow` message shape and its downstream fall-through are unchanged. |
| "... and do not mutate the **committed/original/pending selector session**." | **PRESERVED.** `BreadcrumbSelectionSession` is not written by this feature. |
| #400 AC-5 through AC-8 (`spec.md:243-246`), the Up/Down/Enter/Escape selector contract | **PRESERVED.** Untouched by #440 (research §Q4c). |

The table names **one retracted clause** (the expand/collapse clause, retracted in part) and **three
preserved clauses** (unhandled-key behavior, selector-session immutability, and the #400 AC-5 through
AC-8 Up/Down/Enter/Escape selector contract), which is exactly the shape the acceptance condition
requires.

Corroborating evidence for the three preserved rows, all recorded in this same execution:

- unhandled-key behavior — `p7-t4-ac19-message-shapes.md` (15/15 passed, including
  `LeftAndRightBreadcrumbMessages_RemainSupported`)
- selector session not mutated — `p7-t5-ac20-selector-session.md` (9/9 passed;
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` absent from the change set, confirmed
  again by `p7-t3-ownership-diff.md`)
- #400 AC-5 through AC-8 selector contract — `p7-t6-ac22-400-residual.md` (32/32 passed)

**AC-21 disposition: SATISFIED.**
