# Phase 0 — baseline file sizes and coverage exemptions

Timestamp: 2026-08-27T23-38
Task: [P0-T15]
Command: `wc -l <path>` for each of the five files, and `grep -c 'ExcludeFromCodeCoverage' <path>` with `grep -n` for the locations, at `BASELINE_SHA` = `002335989830ba9f3ad802858ef0b794f6281750`
EXIT_CODE: 0

Each of the five files ends with a newline byte (`0x0A`), so `wc -l` is an exact line count and not an
undercount.

## Baseline line counts

| File | Plan's expected count | Observed at BASELINE_SHA | Agrees? |
|---|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | 1084 | **1073** | **NO — deviation** |
| `QuickFiler/Controllers/EfcItemController.cs` | 1170 | 1170 | yes |
| `QuickFiler/Viewers/EfcViewer.cs` | 162 | 162 | yes |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 430 | **499** | **NO — deviation** |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | 168 | 168 | yes |

### Discrepancies, recorded and reported before Phase 1 begins

Two of the five deviate, and **both deviations were predicted in advance** by
`plan-base-drift-addendum.2026-08-27T21-01.md`, which re-read every locator against the real execution
base. The addendum is authoritative for where things are on disk; the plan's figures were measured
against a different commit.

1. **`EfcFormController.cs` is 1073 lines, not 1084.** The addendum records the same figure. The
   difference is upstream churn between the plan's authoring commit and this base.

   *Consequence for the size gate.* The `spec.md` cross-cutting criterion states this file must end at
   "at most 1204 lines — its 1084-line merge-base count plus at most 120 net lines". With a true
   merge-base count of 1073, the derived ceiling is 1073 + 120 = **1193**. Per the addendum, the
   **stricter** gate of **1193** is held, so that both readings of the criterion pass. `[P10-T9]` records
   the delivered count against 1193 and itemises the net delta per remedy against the true 1073.

2. **`QfcItemController.ViewerSetup.cs` is 499 lines, not 430.** The addendum records the same figure and
   attributes it to merged feature #484, which the base carries.

   *Consequence for `[P4-T5]`.* That task's acceptance states "the file's line count is still 430". The
   substantive constraint is that the diff over the file is **exactly one added and one deleted line**,
   which is what `git diff --numstat BASELINE_SHA` measures and what the `spec.md` criterion asserts. The
   post-change line count must therefore be **499**, unchanged, not 430. The 430 in the plan is a stale
   pre-change measurement, not an independent requirement.

   *Consequence for the incognito locator.* The addendum places the literal at `ViewerSetup.cs:61`, not
   `:55`. `[P4-T5]` resolves the site by content, and the delivered line number is recorded in that
   task's evidence.

## BASELINE_EXEMPTIONS

`ExcludeFromCodeCoverage` occurrence counts in the four files `[P10-T11]` audits:

```
BASELINE_EXEMPTIONS:
  QuickFiler/Controllers/EfcFormController.cs            0
  QuickFiler/Controllers/EfcItemController.cs            1
  QuickFiler/Viewers/EfcViewer.cs                        1
  QuickFiler/Controllers/QfcItemController.ViewerSetup.cs 2
```

Observed locations:

| File | Line | Form |
|---|---|---|
| `QuickFiler/Controllers/EfcItemController.cs` | `:25` | `[ExcludeFromCodeCoverage]` (class level) |
| `QuickFiler/Viewers/EfcViewer.cs` | `:20` | `[ExcludeFromCodeCoverage]` (class level) |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | `:47` | `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | `:137` | `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` |

All four **counts** match the plan's expected figures 0, 1, 1, 2 exactly, so `BASELINE_EXEMPTIONS` as
recorded above is both the observed and the expected set.

One **locator** deviates: the plan says one of the two `ViewerSetup.cs` attributes is at `:41`; the
observed pair is at `:47` and `:137`. This is the same +69-line drift that moved the file from 430 to 499
lines and is attributable to merged #484. The count, which is what `[P10-T11]` asserts, is unaffected.

Constraint C5 stands unchanged: no new `[ExcludeFromCodeCoverage]` attribute may be introduced anywhere
in this feature's diff, and `[P10-T11]` asserts these four totals are unchanged.

Output Summary: Three of five baseline line counts match the plan (EfcItemController.cs 1170,
EfcViewer.cs 162, EfcFormControllerTests.cs 168). Two deviate exactly as the base-drift addendum
predicted: EfcFormController.cs is 1073 (not 1084), giving a stricter derived size gate of 1193, and
QfcItemController.ViewerSetup.cs is 499 (not 430), which is the count the file must still have after the
one-line edit. BASELINE_EXEMPTIONS is 0/1/1/2, matching the plan's expected counts exactly; only the two
ViewerSetup.cs attribute line numbers drifted (:47 and :137, not :41).
