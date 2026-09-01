# Coverage comparison, baseline versus post-change (P7-T9)

Timestamp: 2026-09-01T11-13
Task: [P7-T9]
Working directory: WORKTREE

Sources: `coverage\baseline.cobertura.xml` (P0-T10 run, classified by P0-T11) and
`coverage\post-change.cobertura.xml` (P7-T6 clean run, classified by P7-T7).

## Denominator classifications

| Run | Classification | Package count | Vendored third-party name present |
|---|---|---|---|
| Baseline (P0-T11) | **FILTERED** | 9 | no |
| Post-change (P7-T7) | **FILTERED** | 9 | no |

Both classify as `FILTERED`, and the nine package names are identical in both files. Both runs were
green. The comparison below is therefore between two figures taken over the same filtered denominator,
not a red-run figure compared against a green-run figure. The
`COMPARISON: NOT PERFORMED — mixed denominators` branch was not taken.

## Repository-wide comparison

| Measure | Baseline | Post-change | Delta |
|---|---|---|---|
| `line-rate` | 0.853172 | 0.853910 | +0.000738 |
| Percentage | 85.32 % | 85.39 % | **+0.07** |
| `lines-covered` | 54882 | 54973 | +91 |
| `lines-valid` | 64327 | 64378 | +51 |
| `branch-rate` | 0.793172 | 0.794014 | +0.000842 |
| `branches-covered` | 13081 | 13106 | +25 |
| `branches-valid` | 16492 | 16506 | +14 |

Repository-wide line coverage did not regress. It rose by 0.07 percentage points. The denominator grew
by 51 lines, which is the production code this change added, and the numerator grew by 91, so the added
production code is covered and some previously uncovered existing code became covered as well.

## Per-file rates for the two changed production files

Computed as the plan specifies: the count of `line` elements with `hits` greater than 0 divided by the
total count of `line` elements, taken over the union of every `class` element whose `filename` attribute
names that file. The `class` elements' `line-rate` attributes were **not** averaged; an async method can
compile to its own state-machine class, so a single source file can appear as several `class` elements
with different denominators and the mean of their rates is not the file's rate. In this artifact only
one `class` element matched each file, so the union is a single element, but the computation was still
performed over `line` elements rather than read off the attribute.

Two counting variants are reported because the plan's phrase "the total count of `line` elements" is
ambiguous for a Cobertura file, which carries each covered line twice under a `class` — once under
`class/lines` and once under `class/methods/method/lines`. Measure A counts the `class/lines/line`
direct children only. Measure B counts every descendant `line` element, which is the literal reading.
Both are reported so the result does not depend on resolving the ambiguity.

### `QuickFiler/Controllers/FilerQueue.cs`

| Measure | Baseline | Post-change |
|---|---|---|
| A — `class/lines/line` | 18 / 49 = 0.3673 | **96 / 96 = 1.0000** |
| B — all descendant `line` | 28 / 69 = 0.4058 | **146 / 146 = 1.0000** |
| Distinct uncovered line numbers | 31 | **0** |

**The post-change per-file rate is 1.0000 on both measures, which is at least 0.90 as the acceptance
condition requires.** Every line element in the file is hit and the uncovered set is empty.

This is the file whose added and modified members AC20 requires to reach at least 90 percent:
`WhenDrainedAsync`, both `Enqueue` overloads, `ConsumeAsync`, and the `CompleteItem` helper. All are
fully covered, which the seven queue-level tests added by P5-T2 through P5-T8 account for — before this
change the `Enqueue`/`ConsumeAsync` path was deliberately unexercised, which is why the baseline rate
was 0.3673.

### `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`

| Measure | Baseline | Post-change |
|---|---|---|
| A — `class/lines/line` | 113 / 249 = 0.4538 | 125 / 253 = **0.4941** |
| B — all descendant `line` | 129 / 298 = 0.4329 | 142 / 302 = **0.4702** |
| Distinct uncovered line numbers | 136 | 128 |

This file's coverage also rose, by about 4 percentage points on both measures, and its uncovered line
count fell from 136 to 128. The five ordering tests exercise `BackGroundMoveAsync` including both guard
branches, which was previously reached only by two vacuous tests that returned at the `_groups` guard.
No per-file threshold is asserted for this file by any acceptance criterion; AC20 names only
`FilerQueue.cs`.

Output Summary: Both denominators are `FILTERED`. Repository-wide line coverage moved from 85.32 percent
to 85.39 percent, a delta of +0.07 percentage points, so coverage did not regress.
`QuickFiler/Controllers/FilerQueue.cs` reaches a per-file rate of 1.0000, comfortably above the 0.90
floor AC20 sets for the members this change adds and modifies. Both changed production files improved.

This artifact supplies part of the evidence for the AC20 check-off in P8-T24; P7-T10 supplies the
changed-line half.
