# P2-T7 — Coverage comparison against this cycle's own Phase 0 baseline

Timestamp: 2026-09-02T01-40

## Baseline used

The baseline is **P0-T9 of this remediation cycle** (`evidence/remediation-baseline/coverage-baseline.md`).
**No figure from `plan.2026-08-31T21-12.md` was used.** That plan's Phase 0 figures describe a
tree that changed when commits `8782db56` and `d1f51e3a` landed and are not this cycle's
baseline.

## Clause 1 — repository-wide figures and their differences

| Attribute | Baseline (P0-T9) | Post-change (P2-T6) | Difference |
|---|---|---|---|
| `line-rate` | 0.853964 (85.40%) | 0.853967 (85.40%) | **+0.000003** |
| `lines-covered` | 55073 | 55086 | +13 |
| `lines-valid` | 64491 | 64506 | +15 |
| `branch-rate` | 0.794373 (79.44%) | 0.794522 (79.45%) | **+0.000149** |
| `branches-covered` | 13158 | 13170 | +12 |
| `branches-valid` | 16564 | 16576 | +12 |

Both rates moved **up**. Neither denominator is empty.

## Clause 2 — changed-line coverage, both ref operands

Derivation D5 was run twice, joined to Derivation D6 after replacing `/` with `\` in the git
paths, because Cobertura `filename` values carry native separators while git reports forward
slashes.

| Range | Ref operand | Covered / total | Percentage | Non-executable added lines excluded |
|---|---|---|---|---|
| **This cycle** (gate) | `4b43e31d042da2b3f670d131bc225fdb30972069` | **34 / 34** | **100.00%** | 89 |
| Whole branch (informational) | `807fb0bb6e5e49f43efa6b256b05960bf078ca19` | 112 / 184 | 60.87% | 490 |

Neither denominator is zero, so no `NOT APPLICABLE` row is required.

**Only the cycle-anchored figure is a pass or fail gate.** The branch-wide figure is recorded
for information. Gating on it would let a line the previous cycle already shipped and already
audited fail this cycle: the 72 uncovered branch-wide lines are all in
`QuickFiler/Controllers/QfcQueue.Enqueue.cs`, in the `EnqueueAsync` and
`LoadControllersViewersAsync` bodies that the previous cycle added, and none of them is a line
this cycle touched.

## Clause 3 — the cycle-anchored figure shows no unexplained reduction

The cycle-anchored figure is 100.00%, which is higher than the branch-wide 60.87%, so there
is no reduction to explain in that direction. Every one of this cycle's 34 added executable
production lines is covered; the uncovered set is empty.

## Clause 4 — non-executable exclusion counts

Stated in the clause-2 table: **89** for the cycle-anchored range and **490** for the
branch-wide range. An added line with no `LineMap` entry is non-executable — a brace, comment,
attribute or declaration — and is excluded from the changed-line denominator. The cycle's 89
excluded lines are dominated by the XML documentation blocks that R1, R2 and R3 rewrote, which
is expected for a remediation whose footprint is largely comment text.

## Clause 5 — per-member figures for each new or modified member in a non-exempt file

The per-method view of the post-processed report is partial: `Merge-CoberturaClassesByFilename`
merges async state-machine classes into one entry per file, which leaves only `.cctor` as a
`<method>` element in some files, and no `<method>` element at all for an async member. Each
figure below is therefore derived from the **class-level** line map (Derivation D6, the same
map D3 summarises) restricted to that member's line span in the current source, with every
span verified against the file on disk.

| Member | Covered / total | Percentage | Verdict vs 90% |
|---|---|---|---|
| `QfcPreScoredItem.ResolveCarrier` | 20 / 20 | 100.00% | **PASS** |
| `QfcPreScoredItem.ReconcileCarriersToItems` | 9 / 9 | 100.00% | **PASS** |
| `QfcQueue.ResolveCarriedHandler` | 1 / 1 | 100.00% | **PASS** |
| `QfcHomeController.RunAsync` | 39 / 39 | 100.00% | **PASS** |
| `QfcItemController.ProjectPredeterminedFolder` | 11 / 11 | 100.00% | **PASS** |
| `QfcItemController.AssignFolderComboBox` | 29 / 32 | 90.62% | **PASS** |
| `QfcItemController.LoadFolderHandlerAsync` | 71 / 75 | 94.67% | **PASS** |

All seven are at or above 90 percent, so **no member is recorded as `REMEDIATION-REQUIRED`**.

The two members below 100 percent have their uncovered lines named, and in both cases those
lines are pre-existing and are **not** lines this cycle added:

- `AssignFolderComboBox`, uncovered lines **195, 196, 197** — the `if (_itemViewer.InvokeRequired)`
  marshalling guard and its `Invoke` / `return` body. Unreachable in a unit test, which never
  produces a cross-thread call. This cycle's edits to that member are at lines 233 and 254,
  both covered.
- `LoadFolderHandlerAsync`, uncovered lines **121, 122, 123, 124** — the inner
  `catch (System.Exception e2)` that logs and rethrows when the empty-predictor fallback itself
  throws. This cycle's edits to that member are at lines 70 through 78, all covered.

Cross-check against the cycle-anchored D5 line set for
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, which is
`70,71,72,73,74,75,76,77,78,233,254,257,...,270,274`: none of `121,122,123,124,195,196,197`
appears in it.

## Clause 6 — modified members in a class carrying `[ExcludeFromCodeCoverage]`

| Member | Class | Reason for exemption | Nature of this cycle's change |
|---|---|---|---|
| `QfcDatamodel.DequeueWithHighConfidenceGateWithOutcomeAsync` | `QfcDatamodel` | class-level `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcDatamodel.cs:25` | **comment-only** — P1-T4 rewrote its XML documentation block and changed no executable line |

This is the only expected entry and the only actual one. The attribute is pre-existing; this
cycle neither added nor removed it (P2-T8 asserts that invariant independently).

## Clause 7 — per-file comparison against `coverage-per-file-baseline.md` (P0-T10)

| Path | Baseline | Post-change | Movement |
|---|---|---|---|
| `QuickFiler\Controllers\QfcHighConfidencePreFilter.cs` | 44 / 44 | **73 / 73** | +29 covered, +29 total; still 100.00% |
| `QuickFiler\Controllers\QfcQueue.Enqueue.cs` | 28 / 100 | **13 / 85** | -15 covered, -15 total |
| `QuickFiler\Controllers\QfcHomeController.cs` | 179 / 232 | **179 / 232** | unchanged |
| `QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs` | NOT PRESENT IN REPORT | **NOT PRESENT IN REPORT** | unchanged |
| `QuickFiler\Controllers\QfcItemController.FolderHandling.cs` | 165 / 172 | **166 / 173** | +1 covered, +1 total |

One file shows a reduction in covered lines, and it **is** explained by a line deletion in
that file. `QfcQueue.ResolveCarriedHandler` had a 26-line body; P1-T3 rewrote it as a
single-expression delegation to `QfcPreScoredItem.ResolveCarrier`. The covered-line drop and
the total-line drop are **both exactly 15**, so every executable line removed from that file
was a line that had been covered; no line became uncovered. The same logic now lives in
`QfcHighConfidencePreFilter.cs`, whose covered and total counts each rose by 29. The coverage
moved between files rather than being lost, and the repository-wide line rate rose.

`QfcDatamodel.QueueProcessing.cs` remains absent from the report because `QfcDatamodel` still
carries its class-level `[ExcludeFromCodeCoverage]`; a search of the post-change derivation
output for `QfcDatamodel.QueueProcessing` returns 0 rows.

## Clause 8 — non-vacuity control for the D6 pass

`@($doc.SelectNodes('//class[@filename]')).Count` = **561**, an integer greater than zero.

The control is what makes the `NOT PRESENT IN REPORT` row above, and any empty per-member or
per-file table, distinguishable from a derivation that ran with an unassigned `$doc`. D1, D2,
D3 and D6 were issued in one `pwsh` session so that `$doc` and the dot-sourced helpers were
assigned before D3 and D6 read them.

## Output Summary

Repository-wide line rate 85.40% -> 85.40% (+0.000003) and branch rate 79.44% -> 79.45%
(+0.000149); both moved up. Cycle-anchored changed-line coverage **34/34 = 100.00%** with 89
non-executable lines excluded; branch-wide 112/184 = 60.87% with 490 excluded, recorded for
information only. All seven new or modified non-exempt members are at or above 90 percent
(100.00, 100.00, 100.00, 100.00, 100.00, 90.62, 94.67); no member is `REMEDIATION-REQUIRED`.
The single exempt member's change is comment-only. One per-file reduction, in
`QfcQueue.Enqueue.cs`, is fully explained by a 15-line deletion whose covered and total drops
are equal. Non-vacuity control 561.
