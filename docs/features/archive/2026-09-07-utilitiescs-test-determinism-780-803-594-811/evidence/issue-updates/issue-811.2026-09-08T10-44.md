# Issue update mirror — #811

Timestamp: 2026-09-08T10-44
POSTING BLOCKED: GitHub posting is gated to the orchestrator by repository hooks
PostedAs: unknown

## Deviation from the plan's predicted values

P8-T10's acceptance predicted `Checked off (delivered): 5` and `Remaining (unchecked): 0`. The
observed state is 4 delivered and 1 remaining, because AC4's ten-run gate recorded one failing run
(see `evidence/regression-testing/p8-t6-ac4-ten-run.md`). The true figures are written below. This
task is therefore left unchecked in the plan: its acceptance condition as written cannot be
satisfied without misreporting the result, and the fail-closed evidence rule makes misreporting the
worse outcome.

---

## Text intended for the issue

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/spec.md`
- Total AC items: 5
- Checked off (delivered): 4
- Remaining (unchecked): 1
- Items remaining: AC4: A full nine-assembly `/InIsolation` run with `TestCategory!=LiveOutlook` reports zero failures on ten consecutive runs, recorded as evidence.

| AC | State | Evidence |
|---|---|---|
| AC1 | delivered | `fail-before-exception.2026-09-08T09-50.md`, `p4-t4-ac1-pass-after.md`, `p8-t6-ac4-ten-run.md` |
| AC2 | delivered | `p1-t9-seam-scoped-run.md`, `p2-t4-ac2-fail-before.md`, `p3-t2-ac2-pass-after.md` |
| AC3 | delivered | `p5-t10-ac3-pass-after.md` |
| AC4 | **not delivered** | `p8-t6-ac4-ten-run.md` — 9 of 10 runs clean |
| AC5 | delivered | `p7-t10-ac5-timing-hack-search.md` |

### The ten-run gate

Ten consecutive full nine-assembly `/InIsolation` runs on the unchanged source commit `03b7bd57`,
24 class-level workers, 7162 tests per run.

| Run | total | executed | passed | failed | error | aborted | timeout | seconds |
|---|---|---|---|---|---|---|---|---|
| 1 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 70.9 |
| 2 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 54.2 |
| 3 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 55.0 |
| 4 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 51.9 |
| 5 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 51.3 |
| 6 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 70.7 |
| 7 | 7162 | 7162 | 7161 | 1 | 0 | 0 | 0 | 73.9 |
| 8 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 69.4 |
| 9 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 54.2 |
| 10 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 58.8 |

The single failure, on run 7, was
`UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.MethodBodyReader_Tests.GetBodyCode_ReturnsConcatenatedInstructions`.
It is a different defect from the three this item repairs: an unsynchronised process-wide static in
`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`, whose `LoadOpCodes()` reassigns
`singleByteOpCodes` to a fresh all-default array before filling it, raced by two test classes that
both call it and neither of which carries `[DoNotParallelize]`. Neither file is in this item's
write set and neither was modified here. It needs its own issue.

All 13 tests this change adds, renames or repairs read `Passed` in every one of the ten runs (130
assertions, 0 not-passed), including both intermittent-failure sentinels
`TryAddValuesAsync_UpdatesExistingValue` and
`GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`. On this evidence the three
defects #811 targets did not recur.

The local filter extends `TestCategory!=LiveOutlook` with the documented shell-icon exclusion, an
environmental workstation issue covered by CI. The CI run on the pull request supplies the
unfiltered single-run form; one CI run is not ten.

### Follow-ups recorded

- `docs/features/potential/2026-09-08-etl-deadline-mechanics-follow-ups.md`
- `docs/features/potential/2026-09-08-console-out-aggressors-and-banned-symbol-promotion.md`

A third follow-up is needed for the `ILGlobals` static race described above; it is reported to the
orchestrator rather than filed here, because authoring it was not in this plan's write set.

### Closure pointers

#780, #803 and #594 are superseded by #811.

---

## Acceptance evaluation

- The file exists at the canonical path. PASS
- Contains `Total AC items: 5`. PASS
- Contains `Checked off (delivered): 5`. **FAIL** — the observed value is 4 and the artifact records
  the observed value rather than the predicted one.
- Contains no host token. PASS (re-verified by P8-T12)
