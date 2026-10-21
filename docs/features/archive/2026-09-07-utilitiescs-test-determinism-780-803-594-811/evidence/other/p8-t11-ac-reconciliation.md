# P8-T11 — Final acceptance-criteria reconciliation

Timestamp: 2026-09-08T10-44
Task: [P8-T11]
Command: `Select-String` over `<FEATURE>/spec.md` lines 292-296 for `- [x] AC` and `- [ ] AC`
EXIT_CODE: 0

## Observed state of spec.md lines 292-296

| Line | Criterion | State |
|---|---|---|
| 292 | AC1 | `- [x]` |
| 293 | AC2 | `- [x]` |
| 294 | AC3 | `- [x]` |
| 295 | AC4 | `- [ ]` |
| 296 | AC5 | `- [x]` |

`Select-String -SimpleMatch '- [x] AC'` counts **4**; `'- [ ] AC'` counts **1**.

The task's acceptance requires 5 and 0. It is therefore not met, and this task is left unchecked.
The counts above are the observed values; the fail-closed evidence rule makes recording the true
state the required outcome, and checklist state must not contradict evidence on disk.

## Evidence that discharged each criterion

| AC | Discharged by | Verdict |
|---|---|---|
| AC1 | `evidence/regression-testing/fail-before-exception.2026-09-08T09-50.md` (structural proof: `CancelAfter(` and `CreateLinkedTokenSource` both 1 to 0, positive control 0 to 1), `evidence/regression-testing/p4-t4-ac1-pass-after.md` (both dictionary tests pass; the #780 test runs in 2.76 ms), `evidence/regression-testing/p8-t6-ac4-ten-run.md` (`TryAddValuesAsync_UpdatesExistingValue` passes in all ten 24-worker runs) | DELIVERED |
| AC2 | `evidence/regression-testing/p1-t9-seam-scoped-run.md` (static seams replaced; `TableEtlInvoker` count 0 in both the production and the test file), `evidence/regression-testing/p2-t4-ac2-fail-before.md` (RED: `NullReferenceException` where `InvalidOperationException` was asserted), `evidence/regression-testing/p3-t2-ac2-pass-after.md` (GREEN after the guard, same test unchanged) | DELIVERED |
| AC3 | `evidence/regression-testing/p5-t10-ac3-pass-after.md` (six named tests pass; `Console.SetOut` totals 0 across the five converted test files; solution build clean) | DELIVERED |
| AC4 | `evidence/regression-testing/p8-t6-ac4-ten-run.md` (9 of 10 runs clean; run 7 failed one test) | **NOT DELIVERED** |
| AC5 | `evidence/qa-gates/p7-t10-ac5-timing-hack-search.md` (all eleven patterns count 0 over 737 added lines; the pre-existing `Returns(120)` tolerance retired) | DELIVERED |

## Why AC4 is not delivered

AC4 reads: "A full nine-assembly `/InIsolation` run with `TestCategory!=LiveOutlook` reports zero
failures on ten consecutive runs, recorded as evidence."

Ten consecutive runs were performed and recorded. Nine reported zero failures. Run 7 reported one:

```
UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.MethodBodyReader_Tests.GetBodyCode_ReturnsConcatenatedInstructions
```

The criterion is a property of the whole run, not of the tests this item touches, so a single
failure anywhere defeats it regardless of cause. It is left unchecked.

The cause is an unsynchronised process-wide static in
`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` raced by `ILGlobals_Tests` and
`MethodBodyReader_Tests`, neither of which carries `[DoNotParallelize]` and neither of which is in
this item's 20-path write set. Full derivation in `evidence/regression-testing/p8-t4-ac4-runs.md`.
It is a pre-existing latent defect in the same family as #811 but a distinct instance, and it needs
its own issue. Fixing it here would require editing files outside the declared write set.

The distinction against AC1 matters and is recorded in
`evidence/other/p4-t5-ac1-checkoff-note.md`: AC1's parallel-determinism clause names one test,
which passed ten times out of ten, whereas AC4's clause covers every test in the run.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/spec.md`
- Total AC items: 5
- Checked off (delivered): 4
- Remaining (unchecked): 1
- Items remaining: AC4: A full nine-assembly `/InIsolation` run with `TestCategory!=LiveOutlook` reports zero failures on ten consecutive runs, recorded as evidence.

## Output Summary

Four of the five acceptance criteria are delivered and checked off in spec.md with named evidence.
AC4 is not, because one of the ten required runs failed on a defect outside this item's write set.
The three defects #811 targets did not recur in any of the ten runs.
