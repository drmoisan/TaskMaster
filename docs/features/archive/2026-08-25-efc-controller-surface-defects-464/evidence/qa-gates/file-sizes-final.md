# [P10-T9] Final file-size audit

Timestamp: 2026-08-28T02-07
Task: [P10-T9]
Command: `wc -l <path>` for each of the eight files `[P10-T1]` formats, run after that formatting pass
EXIT_CODE: 0

Both `QuickFiler/Controllers/EfcFormController.cs` and `QuickFiler/Controllers/EfcItemController.cs` end
with a newline byte (`0x0A`), verified with `tail -c 1 | od -An -c`, so `wc -l` is an exact line count
and not an undercount.

## Delivered line counts against their gates

| File | Delivered | Gate | Verdict |
|---|---|---|---|
| `QuickFiler/Viewers/EfcViewer.cs` | **169** | at most 500 | PASS |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | **499** | "still 430" (stale figure — see below) | PASS on the substantive constraint |
| `QuickFiler.Test/Controllers/EfcItemControllerTests.cs` | **470** | at most 500 | PASS |
| `QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs` | **260** | at most 500 | PASS |
| `QuickFiler.Test/Controllers/EfcViewerTests.cs` | **164** | at most 500 | PASS |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | **485** | at most 500 | PASS |
| `QuickFiler/Controllers/EfcFormController.cs` | **1189** | at most 1204 (plan) / **1193** (stricter derived) | PASS under both |
| `QuickFiler/Controllers/EfcItemController.cs` | **1117** | strictly fewer than 1170 | PASS, 53 lines below |

**No value under 500 is asserted for `EfcFormController.cs` or `EfcItemController.cs`.** Both are
pre-existing violations of the repository's 500-line limit whose splitting `spec.md` places out of scope.

## Recorded deviation — the `QfcItemController.ViewerSetup.cs` "still 430" figure

The task states the acceptance for this file as "is still 430". The file is **499** and was **499** at
`BASELINE_SHA`; `[P0-T15]` recorded the same figure and flagged the plan's 430 as a stale pre-#484
measurement taken against a different commit. The base-drift addendum records the same 499.

The substantive constraint on this file is that its diff is exactly one changed line, which
`[P9-T5]` and `[P10-T2]` both measure as **1 added / 1 deleted** under both bases. The line count must
therefore be 499, unchanged — a delivered count of 430 would mean 69 lines had been destroyed. The
deviation is a stale plan figure, not an unmet requirement, and is recorded rather than absorbed.

## `EfcFormController.cs` — which size gate is used, and why

| Measure | Value |
|---|---|
| `spec.md` cross-cutting criterion's stated merge-base count | 1084 |
| **True merge-base count at `BASELINE_SHA`** (verified by `[P0-T15]`) | **1073** |
| Plan's literal ceiling (1084 + 120) | 1204 |
| **Stricter derived ceiling held** (1073 + 120) | **1193** |
| **Delivered count** | **1189** |
| **Net delta against the true merge base** | **+116** |
| Headroom against the stricter gate | 4 lines |

The base-drift addendum directs the executor to hold the stricter derived gate of 1193 so that **both**
readings of the criterion pass. 1189 is at most 1193 and at most 1204, and +116 is within the 120-line
allowance, so the file passes under the plan's literal gate, under the stricter derived gate, and under
the criterion's own "at most 120 net lines" clause.

### Net delta by remedy

| Remedy | Members added or changed | Net lines |
|---|---|---|
| RC1 (#460 A/C, #464 A, #465 A) | guarded `Cleanup()`, guarded `ActiveTheme`, `LoadTheme`, `DarkMode` | +26 |
| RC3-B (#464 B) | `BoundaryErrorSink` plus the five extracted `internal async Task` members, less the five `throw;` statements | +42 |
| RC3-C (#464 C) | `try`/`catch` around `PopulateFolderCombobox` | +8 |
| RC8 (#465 B) | `MatchesForSearchText`, hoisted control read | +14 |
| RC9 (#465 C) | `TrashRowText`, `WithTrashRow`, `ApplyDeleteGesture`, `BindSourceFolderRows`, less the `ActionDeleteAsync` body and the `BindFolderRows` write-back | +19 |
| RC7 (#465 D) | `IsBannerRow`, `IsSelectableFolder`, recomposed `ActionOkAsync` guard | +17 |
| Comment compression during Phase 7 | comments only, no member removed | −10 |
| **Total** | | **+116** |

1073 + 116 = **1189**, which reconciles with the measured count exactly.

The itemisation is carried forward unchanged from `file-sizes-after-p7.md`. Phase 8 edited only
`QuickFiler/Viewers/EfcViewer.cs`, and `[P10-T1]` rewrote no file (identical SHA-256 before and after),
so `EfcFormController.cs` has not changed size since that record.

## Change since the Phase 8 boundary

| File | Phase 8 boundary | Final | Delta |
|---|---|---|---|
| `EfcFormController.cs` | 1189 | 1189 | 0 |
| `EfcItemController.cs` | 1117 | 1117 | 0 |
| `EfcViewer.cs` | 169 | 169 | 0 |
| `EfcFormControllerTests.cs` | 485 | 485 | 0 |
| `EfcItemControllerTests.cs` | 470 | 470 | 0 |
| `EfcItemController.CleanupTests.cs` | 260 | 260 | 0 |
| `EfcViewerTests.cs` | 164 | 164 | 0 |
| `QfcItemController.ViewerSetup.cs` | 499 | 499 | 0 |

Every file is byte-identical to its Phase 8 boundary state, which is what `[P10-T1]`'s zero-rewrite
result predicts.

Output Summary: PASS. `EfcViewer.cs` 169, `EfcItemControllerTests.cs` 470,
`EfcItemController.CleanupTests.cs` 260, `EfcViewerTests.cs` 164 and `EfcFormControllerTests.cs` 485 are
all at most 500. `EfcFormController.cs` is **1189** against the stricter derived gate of **1193** (true
merge base **1073**, net delta **+116** itemised per remedy, within the 120-line allowance) and also
within the plan's literal 1204. `EfcItemController.cs` is **1117**, strictly below its 1170 merge-base
count. `QfcItemController.ViewerSetup.cs` is 499, not the plan's stale 430; the substantive one-line-diff
constraint holds and the discrepancy is recorded as a stale plan figure.
