# [P9-T7] No pre-existing test was weakened

Timestamp: 2026-08-28T01-51
Task: [P9-T7]
Command: `git show 002335989830ba9f3ad802858ef0b794f6281750:QuickFiler.Test/Controllers/EfcFormControllerTests.cs` compared against the delivered file; `git diff --numstat` and `git diff` over the same path
EXIT_CODE: 0

## The two pre-existing methods from `[P0-T13]`

| Method name | Present at `BASELINE_SHA` | Present in delivered file | Declaration line, pre-change → delivered |
|---|---|---|---|
| `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel` | yes (`:39`) | **yes** (`:41`) | `:39` → `:41` |
| `Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter` | yes (`:60`) | **yes** (`:62`) | `:60` → `:62` |

Both are still declared, with **unchanged names**. Both shifted down by exactly 2 lines because this
feature added two `using` directives above them; neither declaration text changed.

Both recorded outcome `Passed` in the Phase 0 baseline TRX (`named-tests.md`) and both pass in the
delivered runs.

## Both bodies are unchanged, proved by the diff shape

```
git diff --numstat 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler.Test/Controllers/EfcFormControllerTests.cs
```

```
317	0	QuickFiler.Test/Controllers/EfcFormControllerTests.cs
```

**317 added lines, 0 deleted lines.** Counting the removed lines of the full diff (lines beginning `-`,
excluding the `---` file header) returns **0**.

A zero-deletion diff is a complete proof of the acceptance condition for this file, and a stronger one
than a body-by-body reading would be:

- **No pre-existing method body was modified.** Modifying a body requires deleting at least one line;
  zero lines were deleted.
- **No pre-existing test was deleted.** Deleting a test requires deleting its lines.
- **No pre-existing test was renamed.** Renaming requires deleting the old declaration line.
- **No assertion in a pre-existing test was weakened.** Weakening an assertion requires deleting or
  replacing it.

The delivered diff over this file therefore consists **only of added members**.

## Delivered method inventory

The file now declares 16 test methods (15 `[TestMethod]` and 1 `[DataTestMethod]`): the 2 pre-existing
methods plus the 14 this feature added. The `[DataTestMethod]`,
`AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow` at `:245-251`, carries five `[DataRow]`
attributes and therefore contributes five test results rather than one.

## Second half of this task — Edit Filters subscription refresh

The post-Phase-7 refresh was appended to
`docs/features/active/efc-controller-surface-defects-464/evidence/qa-gates/edit-filters-survival.md`
under the heading "Appended by [P9-T7] — post-Phase-7 refresh". Summary of what was recorded there:

| Measure | Phase 2 record | Delivered, post-Phase-7 |
|---|---|---|
| `_formViewer.EditFiltersMenuItem.Click += EditFiltersMenuItem_Click;` | `:398` | **`:427`** |
| `EditFiltersMenuItem_Click` declaration | `:559` | **`:597`** |

The subscription line is byte-identical to its pre-change text at `BASELINE_SHA:398`, including leading
whitespace (exact-string comparison result `True`). Only its line number moved, because Phases 5 through
7 added members above it.

Output Summary: PASS. Both pre-existing `EfcFormControllerTests` methods are still declared with
unchanged names at `:41` and `:62`. The diff over that file against `BASELINE_SHA` is 317 added and **0
deleted** lines, so no pre-existing body was modified, no test was deleted or renamed, and no assertion
was weakened — the diff consists only of added members. The Edit Filters subscription was re-read and is
byte-identical at its new location `EfcFormController.cs:427`, with its handler at `:597`; that refresh
was appended to `edit-filters-survival.md`.
