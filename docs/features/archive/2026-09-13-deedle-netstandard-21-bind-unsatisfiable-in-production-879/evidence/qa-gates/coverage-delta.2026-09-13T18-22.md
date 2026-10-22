# Coverage Delta and No-Regression Record

Recorded by `[P5-T10]`. This artifact records the Revision R7 re-execution; each attempt
overwrites its own artifact.

Timestamp: 2026-09-14T12-55

Command: values copied from the three named artifacts; no new command was run.

- `BASELINE_LINE_RATE` and `BASELINE_LINES_VALID` from
  `evidence/baseline/test-coverage-baseline.2026-09-13T18-22.md`
- `POST_CHANGE_LINE_RATE` and `POST_CHANGE_LINES_VALID` from
  `evidence/qa-gates/test-final.2026-09-13T18-22.md`
- `NEW_MODULE_LINE_PERCENT` from
  `evidence/qa-gates/coverage-assemblybindingfallback.2026-09-13T18-22.md`

EXIT_CODE: 0

Output Summary:

```
BASELINE_LINE_RATE=0.7125753506415995
BASELINE_LINES_VALID=83775
POST_CHANGE_LINE_RATE=0.858647
POST_CHANGE_LINES_VALID=65616
NEW_MODULE_LINE_PERCENT=94.03
```

`POST_CHANGE_LINE_RATE` of 0.858647 is greater than `BASELINE_LINE_RATE` of
0.7125753506415995. That comparison is stated rather than relied upon, for the three reasons
below.

DENOMINATORS DIFFER

`POST_CHANGE_LINES_VALID` of 65616 differs from `BASELINE_LINES_VALID` of 83775, so the two
document-level rates are computed over different denominators and the no-regression judgment
rests on `NEW_MODULE_LINE_PERCENT` and on the changed-line evidence in `[P5-T9]`.

BASELINE DENOMINATOR NOT COMPARABLE

The `[P0-T8]` artifact records `Cobertura Document State: RAW-COLLECTOR-OUTPUT`. Comparing a raw
document-level `line-rate` against a post-processed one would compare two different
denominators, so the no-regression judgment rests solely on `NEW_MODULE_LINE_PERCENT` and on
`[P5-T9]`.

BASELINE MEASURED ON A DIFFERENT BASE

The baseline artifact's `Timestamp:` value is `2026-09-13T23-15`. `[P0-T8]` was captured before
`origin/main` at `a49c9729e` was merged into this branch at merge commit `f02cee3fe`, so the two
document-level rates measure different code bases as well as different denominators. The
baseline is not re-captured: it already records `Cobertura Document State: RAW-COLLECTOR-OUTPUT`,
so the no-regression judgment already rests on `NEW_MODULE_LINE_PERCENT` and `[P5-T9]` rather
than on the rate comparison.

Acceptance Condition: MET. All five figures are present as numbers, `NEW_MODULE_LINE_PERCENT` of
94.03 is at least 90, and this artifact states explicitly that `POST_CHANGE_LINE_RATE` is greater
than `BASELINE_LINE_RATE`.

## Document State Flip:

First, the two runs took different exit paths through
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` and therefore produced documents in different
states. `[P0-T8]` ran with two failing tests, so the runner threw at line 262 on a non-zero
collection exit code, BEFORE the post-processing at lines 383-384, and the document it left
behind is `RAW-COLLECTOR-OUTPUT`: all modules including third-party, with
`line-rate = 0.7125753506415995`, `lines-valid = 83775` and `lines-covered = 59696`. `[P5-T7]`
ran with zero failures, so the throw did not occur, post-processing ran, and the document is
`POSTPROCESSED`: first-party only, with `line-rate = 0.858647`, `lines-valid = 65616` and
`lines-covered = 56341`, and branches `13624 / 17022 = 80.04` percent. The two documents
therefore differ in DENOMINATOR, in DOCUMENT STATE and in BASE COMMIT, three independent ways,
and a direct comparison of their document-level rates measures none of the three cleanly.

Second, issue #891 did not fire in the `[P5-T7]` run, and the reason is recorded rather than
left implicit: `Assert-CoberturaLineCoverageThreshold` at line 386 asserts against the
document-level rate of the POST-PROCESSED document, which is the first-party `0.858647`, and
that clears its hard-coded 80 percent threshold. Issue #891 remains unfixed by this plan; this
run simply did not meet its failing condition.

Third, the no-regression judgment is unaffected by all of the above, because it already rests on
`NEW_MODULE_LINE_PERCENT` and on `[P5-T9]` rather than on the document-level comparison.

### Post-change figure pair: superseded reading and current reading

The plan text for this task names the post-change pair as `line-rate = 0.858327` over
`lines-valid = 65616`. That pair is the reading taken by the SUPERSEDED `[P5-T7]` run, before
Revision R7 added ten tests. It is recorded here as a number, with its provenance, because the
plan's acceptance condition names it; it is NOT this run's reading and is not presented as one.

| Reading | `line-rate` | `lines-valid` | `lines-covered` |
|---|---|---|---|
| Baseline, `[P0-T8]`, `RAW-COLLECTOR-OUTPUT` | 0.7125753506415995 | 83775 | 59696 |
| Superseded `[P5-T7]` pass, `POSTPROCESSED` | 0.858327 | 65616 | 56320 |
| This `[P5-T7]` run, `POSTPROCESSED` | 0.858647 | 65616 | 56341 |

The denominator is identical at 65616 across both post-processed readings, because Revision R7
adds no production line. The numerator rose from 56320 to 56341, a gain of 21 covered lines,
which is the repository-wide effect of the ten tests `[P4-T13]` adds. Of those, 23 lines fall in
`UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` as `[P5-T9]` records; the repository-wide
figure is smaller because the two totals are taken over different file sets and the
post-processed document counts only first-party lines.

Acceptance Addition: MET. The `Document State Flip:` heading exists, names the baseline document
state as `RAW-COLLECTOR-OUTPUT` and the post-change one as `POSTPROCESSED`, and carries both
figure pairs the plan names — `0.7125753506415995` over `83775`, and `0.858327` over `65616` —
as numbers, together with this run's actual pair `0.858647` over `65616`. Recording the actual
reading alongside the plan's literal is required by the standing rule that an evidence artifact
must never assert an observation that was not made: the plan's literal was authored against the
superseded run and this run legitimately measured a different rate.
