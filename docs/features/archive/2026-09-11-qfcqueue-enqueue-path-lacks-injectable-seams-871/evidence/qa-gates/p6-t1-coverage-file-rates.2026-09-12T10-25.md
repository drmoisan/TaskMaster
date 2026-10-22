# P6-T1 — File-level line-rate comparison

Timestamp: 2026-09-13T16-57
Command: dot-source of the coverage helpers file under the vscode scripts directory, then Get-CoberturaClassLineSummary over the class elements of the P0-T12 and P5-T5 Cobertura documents
EXIT_CODE: 0

## Property-name mapping for every class-level figure in this artifact

The plan asks for "the `LineRate`, `LinesCovered` and `LinesValid` from
`Get-CoberturaClassLineSummary`". That helper emits none of those three names. Its returned object
carries `LineMap`, `TotalLines`, `CoveredLines`, `TotalBranches` and `CoveredBranches`, and emits no rate
property at all; the count naming is inverted relative to the words the plan uses. Only the package-level
helper `Get-CoberturaPackageLineSummary` emits `LineRate`, `LinesCovered` and `LinesValid` under exactly
those names. The mapping applied throughout this artifact, established by P0-T12 and reproduced in full
here so a reviewer meets the same explanation at every site that reports a class-level figure, is:

- `LinesCovered` is the helper's `CoveredLines`.
- `LinesValid` is the helper's `TotalLines`.
- `LineRate` is read from the class element's own line-rate attribute.

Every rate below was cross-checked by recomputing `CoveredLines` divided by `TotalLines` to six places
and comparing against the attribute; the two agree exactly in every case. The plan is not edited; the
mapping is recorded.

## Labelled numeric lines the acceptance condition requires

SpecPreChangeEnqueueRate: 0.152941
P0T12MeasuredEnqueueRate: 0.152941
P5T5MeasuredEnqueueRate: 1
SpecPreChangeBasePartRate: 0.503205
P0T12MeasuredBasePartRate: 0.496795
P5T5CombinedBaseTlpUiIdleRate: 0.540299

The spec records its two pre-change figures at lines 42 and 43 and restates them at lines 492, 493, 685
and 687.

## Supporting counts

| Document | File | LinesCovered | LinesValid | LineRate |
|---|---|---|---|---|
| P0-T12 baseline | `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 13 | 85 | 0.152941 |
| P5-T5 post-change | `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 85 | 85 | 1 |
| P0-T12 baseline | `QuickFiler/Controllers/QfcQueue.cs` (pre-split) | 155 | 312 | 0.496795 |
| P5-T5 post-change | `QuickFiler/Controllers/QfcQueue.cs` | 109 | 155 | 0.703226 |
| P5-T5 post-change | `QuickFiler/Controllers/QfcQueue.Tlp.cs` | 67 | 151 | 0.443709 |
| P5-T5 post-change | `QuickFiler/Controllers/QfcQueue.UiIdle.cs` | 5 | 29 | 0.172414 |

## How the combined post-change rate is computed

The Phase 1 split moved two regions out of the pre-split base part into two new partial parts, so the
post-change counterpart of the single pre-split class element is the union of three class elements. The
combined rate is therefore the sum of the `LinesCovered` values divided by the sum of the `LinesValid`
values that `Get-CoberturaClassLineSummary` returns for the three class elements of the P5-T5 document
whose filename attributes name those three files. It is not an average of the three rates, which would
weight a 29-line file equally with a 155-line one.

```
CombinedLinesCovered = 109 + 67 + 5 = 181
CombinedLinesValid   = 155 + 151 + 29 = 335
CombinedLineRate     = 181 / 335 = 0.540299
```

The post-change valid-line total of 335 exceeds the pre-split 312 by 23, which is the same rise of 23
that the QuickFiler package's valid count shows between P0-T12 and P5-T5. The seams, the adapter class
and the interface forwarding added executable statements; the split itself moves statements without
creating them.

## Gate, clause by clause

### Clause 1 — the enqueue part must rise strictly

The post-change rate for `QuickFiler/Controllers/QfcQueue.Enqueue.cs` must be strictly greater than
both 0.152941 and the P0-T12 measurement for that file.

```
1 > 0.152941        (spec literal)          TRUE
1 > 0.152941        (P0-T12 measurement)    TRUE
```

Clause1Verdict: PASS

The two comparands happen to be the same number here — the spec's recorded figure and the P0-T12
measurement agree exactly — so the clause is tested against one value twice rather than against two.
Both comparisons are recorded separately anyway, because the plan names them separately and their
agreement is an observation rather than a premise. The file moved from 13 of 85 covered lines to 85 of
85, a rise of 72 covered lines with the denominator unchanged.

### Clause 2 — the combined base-part rate must not fall

The post-change combined rate must not be below either 0.503205 or the P0-T12 measurement for the
pre-split file.

```
0.540299 >= 0.503205   (spec literal, the binding comparand)   TRUE
0.540299 >= 0.496795   (P0-T12 measurement)                    TRUE
```

Clause2Verdict: PASS

These two comparands are different numbers and the difference is material. The spec's recorded figure of
0.503205 is higher than what P0-T12 actually measured for the same file at the anchor, by 0.006410. The
spec literal is the binding one and it is the harder of the two to clear, so both comparisons are
performed and reported rather than the easier one being taken as representative. Both pass, so the
divergence does not change the verdict here; it is recorded because a smaller improvement could have
cleared one comparand and failed the other, and in that case the failure would be reported and not
waived.

The divergence itself is not resolved by this task. The most likely explanation is that the spec's
figure was taken from a measurement of a different run or a different tracked-file state than the one
P0-T12 captured against the re-derived post-merge anchor, but this artifact does not assert that; it
records the two values and compares against both.

## Overall verdict

P6T1Verdict: PASS

Both clauses pass. The enqueue part rose from 0.152941 to 1, and the combined base-part rate rose from
0.496795 to 0.540299, clearing the higher spec literal of 0.503205 as well.

Output Summary: Clause 1 PASS — the enqueue part measures 1 over 85 of 85 lines, strictly greater than
both the spec literal 0.152941 and the P0-T12 measurement 0.152941. Clause 2 PASS — the combined
post-change rate for the base, Tlp and UiIdle parts is 0.540299 over 181 of 335 lines, at or above both
the spec literal 0.503205 and the P0-T12 pre-split measurement 0.496795; those two comparands differ by
0.006410 and both were tested. The class-level helper property-name mapping is reproduced in full and
every rate was cross-checked against its class element's line-rate attribute. Acceptance met.
