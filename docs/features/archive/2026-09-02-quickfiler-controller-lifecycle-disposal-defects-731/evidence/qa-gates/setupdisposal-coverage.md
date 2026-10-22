# Finding 5 — QfcFormController.SetupDisposal.cs coverage re-measurement

Timestamp: 2026-09-03T14-40

Task: [P5-T8]
Issue: #731

## Command

The `[P5-T6]` extraction rule, which is `[P0-T10]`'s separator-anchored, de-duplicated per-line map rule, applied to `coverage/postchange.cobertura.processed.xml`:

- enumerate every `class` element whose `filename` attribute ends with a directory separator immediately followed by `QfcFormController.SetupDisposal.cs` — all such elements, not the first, because `Merge-CoberturaClassesByFilename` merges per-filename groups only within a single `package` element;
- within each, enumerate `./lines/line` first and then `./methods/method/lines/line`;
- key every line by its `number` attribute, and where a key repeats, keep the maximum `hits`;
- the total is the map's entry count and the covered count is the subset whose kept `hits` is greater than zero.

The descendant-axis `.//line` selection was **not** used. That is the point of the rule: Cobertura repeats every source line under `methods/method/lines` and again in the class-level `lines` rollup, so a descendant-axis count doubles every line (issue #441). The uncovered-line count below is the map's total entries less its covered entries, so it cannot be doubled by that duplication.

All XML attributes were read through `GetAttribute('...')`. The separator anchor is an ordinal `EndsWith` test against the filename prefixed by `[char]92`, and alternatively by the forward slash.

EXIT_CODE: 0

## Output Summary

Post-change measurement for `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`:

- Map entries (total lines): **182**
- Entries with hits greater than zero (covered lines): **136**
- Whole-file line coverage: **74.73 percent** (136 / 182 = 74.7253 percent)
- Uncovered lines: **46** (182 total less 136 covered)

## Comparison against the issue-#683 baseline

`[P0-T4]` quoted line 33 of `docs/features/potential/promoted/2026-08-28-qfcformcontroller-setupdisposal-coverage-debt.md`, which records the issue-#683 baseline for this file as **70.70** percent whole-file line coverage with **46** lines uncovered. `[P0-T10]` independently reproduced that baseline from this harness's own Phase 0 document: 111 covered of 157 total, which is 70.70 percent with 46 uncovered.

| Measurement | Issue-#683 baseline | Post-change | Change |
|---|---|---|---|
| Whole-file line coverage | 70.70 percent | 74.73 percent | +4.03 percentage points |
| Uncovered lines | 46 | 46 | unchanged |
| Covered lines | 111 | 136 | +25 |
| Total lines | 157 | 182 | +25 |

The percentage rose by 4.03 percentage points while the uncovered-line count stayed at exactly 46. Both figures move consistently: the file gained 25 executable lines from the `Cleanup()` rewrite at `[P2-T4]`, and all 25 are covered by the seven `QfcFormControllerCleanupTests` methods, so the covered count and the total rose by the same 25 and the uncovered count did not move. `EVIDENCE/qa-gates/coverage-delta.md` records each of the 26 changed executable lines with `post_hits=1`; the 26th is one line that the merged per-line map already counted at baseline.

The improvement is therefore a side effect of covering the new code, not a reduction of the pre-existing #683 gap. All 46 lines that were uncovered before this change remain uncovered after it.

## Residual gap assignment

Any residual coverage gap on `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` remains assigned to issue **#683**, and reaching any specific percentage on that file is **not** an acceptance criterion of issue #731; this task re-measures and records the figure, and the 46 remaining uncovered lines stay tracked under #683 rather than being brought into this issue's scope.
