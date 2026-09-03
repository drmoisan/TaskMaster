# Coverage delta against the Phase 0 baseline (P6-T6)

Timestamp: 2026-09-02T23-56

EXIT_CODE: 0

Command:

```
$b = [xml](Get-Content -Raw -Encoding UTF8 'docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\baseline\coverage-baseline.cobertura.xml')
$p = [xml](Get-Content -Raw -Encoding UTF8 'docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\qa-gates\coverage-final.cobertura.xml')
# root attributes read with $doc.DocumentElement.GetAttribute(<name>) for
# line-rate, branch-rate, lines-covered, lines-valid, branches-covered, branches-valid;
# per-file aggregation over $doc.SelectNodes('//class'), grouping by the class filename
# attribute and counting $class.SelectNodes('lines/line') children, a line being covered
# when its hits attribute is greater than zero.
```

No test run was executed by this task. Every value below is read from the two Cobertura
documents already on disk. Per the task text, selecting a favourable run from a set of runs is
prohibited, so no additional coverage run was performed or considered.

## Processing-state precondition

- `CoberturaProcessingState:` recorded by P0-T11 for the baseline: `processed`
- `CoberturaProcessingState:` recorded by P6-T5 for the post-change run: `processed`

The two declarations agree, so both documents are on the same denominator and the comparison is
valid. **No conversion was performed**, and none was required: `ConvertTo-KoverageCoberturaXml`
was not called, because neither side is the raw form.

## Counting-method validation

The per-file aggregation reproduces both documents' root counters exactly, which confirms the
counting method matches the one the root attributes were computed from:

| Measure | Aggregated from `<class>` elements | Root attribute | Agree |
|---|---|---|---|
| Baseline total lines | 64575 | 64575 | yes |
| Baseline covered lines | 55138 | 55138 | yes |
| Post-change total lines | 64578 | 64578 | yes |
| Post-change covered lines | 55139 | 55139 | yes |

Both documents contain 561 distinct `filename` values, and the union of the two key sets is also
561, so no file entered or left the measured set.

## The eight required values

The four line-count fields aggregate every `<class>` element whose `filename` attribute ends with
`NonBlockingDelay.cs`. Exactly one such `<class>` element exists in each document.

BaselineLineRate: 0.85386

PostChangeLineRate: 0.853836

BaselineBranchRate: 0.794589

PostChangeBranchRate: 0.794529

BaselineCoveredLines: 17

PostChangeCoveredLines: 20

BaselineTotalLines: 17

PostChangeTotalLines: 20

## Repository-wide root counters

| Measure | Baseline | Post-change | Delta |
|---|---|---|---|
| `lines-covered` | 55138 | 55139 | +1 |
| `lines-valid` | 64575 | 64578 | +3 |
| `branches-covered` | 13187 | 13186 | -1 |
| `branches-valid` | 16596 | 16596 | 0 |

BaselineLinesValid: 64575

PostChangeLinesValid: 64578

LinesValidDelta: 3

LinesValidChangedFiles:

```
TaskMaster/AppGlobals/NonBlockingDelay.cs : BASE instrumented 17 -> POST instrumented 20 (difference +3)
```

That is the complete list. Every other one of the 561 measured files has an identical
instrumented-line count on both sides. The sum of the per-file differences listed above is `+3`,
which equals `LinesValidDelta:`.

NegativeMovementFiles:

```
UtilitiesCS/Interfaces/IWinForm/PropertyStore.cs : BASE 565/663 -> POST 559/663 (covered-line movement -6)
```

That is the complete list. It is the only file in either document whose aggregated covered-line
count is lower post-change than at baseline.

For completeness, the two files whose covered-line count rose:

```
TaskMaster/AppGlobals/NonBlockingDelay.cs                 : BASE 17/17   -> POST 20/20   (covered-line movement +3)
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs : BASE 267/297 -> POST 271/297 (covered-line movement +4)
```

The arithmetic closes exactly: `55138 + 3 - 6 + 4 = 55139`, and `64575 + 3 = 64578`.

LineRateDelta: -0.000024

BranchRateDelta: -0.000060

## Counterfactual line rate

The net covered-line movement contributed by files that are **not** in this plan's Complete
file-write inventory is `-6` from `UtilitiesCS/Interfaces/IWinForm/PropertyStore.cs` plus `+4`
from `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`, a net of `-2`. Removing that
movement from the post-change covered-line count gives `55139 - (-2) = 55141`, over the
post-change denominator of `64578`.

CounterfactualLineRate: 0.85386664

CounterfactualLineRateDelta: +0.00000664

The counterfactual is recorded whether or not it is favourable. Here it is favourable: with
untouched-file churn removed, the post-change line rate is `0.00000664` **above** the baseline
rate, which is `+0.00066` percentage points. The whole of the observed repository-wide line-rate
drop is attributable to the net `-2` covered-line movement in the two untouched files named
above, whose `lines-valid` figures are identical on both sides and which this change never
writes.

## The four acceptance clauses

### Clause 1 — deterministic denominator gate: PASS

| Condition | Values | Result |
|---|---|---|
| `PostChangeTotalLines` greater than 0 | 20 > 0 | PASS |
| `LinesValidDelta:` equals the sum of the per-file differences in `LinesValidChangedFiles:` | 3 = +3 | PASS |
| Every `filename` in `LinesValidChangedFiles:` appears in the Complete file-write inventory | `TaskMaster/AppGlobals/NonBlockingDelay.cs` is the sole entry and is listed under "Production source (exactly one file)" in that inventory | PASS |

### Clause 2 — changed-file no-regression gate: PASS

`PostChangeCoveredLines / PostChangeTotalLines` = 20/20 = 1.0, and
`BaselineCoveredLines / BaselineTotalLines` = 17/17 = 1.0. `1.0 >= 1.0` holds.
`TaskMaster/AppGlobals/NonBlockingDelay.cs` is at exactly 100 percent line coverage with no
uncovered line: every one of its 20 measured lines has `hits` greater than zero. The file grew
from 17 to 20 measured lines because the 2-arg overload was added, and all three added lines are
covered.

### Clause 3 — write-set attribution gate: PASS

`NegativeMovementFiles:` contains exactly one entry,
`UtilitiesCS/Interfaces/IWinForm/PropertyStore.cs`. That path does not appear anywhere in this
plan's Complete file-write inventory: it is not the single production source entry, it is not one
of the three modified test sources, it is not one of the two created test sources, it is not one
of the four modified project files or package manifests, it is not one of the seventeen deleted
files, and it is not feature documentation or evidence. No file this plan writes shows a
covered-line decrease.

### Clause 4 — repository-wide tolerance gate: PASS

| Condition | Values | Result |
|---|---|---|
| `BaselineLineRate` minus `PostChangeLineRate` no greater than `0.0005` | 0.85386 - 0.853836 = 0.000024; 0.000024 <= 0.0005 | PASS |
| `BaselineBranchRate` minus `PostChangeBranchRate` no greater than `0.0005` | 0.794589 - 0.794529 = 0.000060; 0.000060 <= 0.0005 | PASS |

The observed line-rate drop is 4.8 percent of the stated band and the branch-rate drop is 12
percent of it.

## What this artifact does not claim

It does not claim that the `-6` covered-line movement in
`UtilitiesCS/Interfaces/IWinForm/PropertyStore.cs` has been root-caused. Distinguishing
run-to-run variance from a consequence of the `[DoNotParallelize]` scheduling change made by
P5-T1 and P5-T2 would require at least one additional full-suite coverage run, which this task
does not authorize. What is established is attribution rather than cause: the file is untouched
by this change, its instrumented-line count is identical on both sides, and it is absent from the
Complete file-write inventory, so clause 3 is decided on the write set rather than on a
root-cause claim.

Output Summary: All four acceptance clauses pass. Clause 1: `PostChangeTotalLines` 20 > 0,
`LinesValidDelta:` 3 equals the sole per-file difference of +3, and that file
(`TaskMaster/AppGlobals/NonBlockingDelay.cs`) is in the Complete file-write inventory. Clause 2:
20/20 = 1.0 is greater than or equal to 17/17 = 1.0. Clause 3: the only negative-movement file is
`UtilitiesCS/Interfaces/IWinForm/PropertyStore.cs`, which is not in the write inventory. Clause
4: the line-rate drop is 0.000024 and the branch-rate drop is 0.000060, both within the stated
`0.0005` band. `CounterfactualLineRate: 0.85386664`, which is `+0.00000664` above the baseline
line rate once untouched-file churn is removed.
