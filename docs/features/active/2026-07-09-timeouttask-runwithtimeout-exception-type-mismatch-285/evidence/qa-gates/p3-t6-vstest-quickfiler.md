# P3-T6 — Full `QuickFiler.Test` Assembly with Coverage (QC loop stage 4)

Timestamp: 2026-09-01T08-26

## Command

```text
<resolved-vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\p3-t6 /TestCaseFilter:TestCategory!=LiveOutlook
```

`<resolved-vstest>` is the vswhere-resolved path recorded in P0-T10. The assembly was run on its own,
not as part of an aggregate assembly list. The `/TestCaseFilter:TestCategory!=LiveOutlook` operand
was present.

EXIT_CODE: 0

## Invocation Count

**One invocation.** The test host did not report a crash, so the plan's conditional single re-run
branch was not taken.

## Output Summary

vstest's trailing summary:

```text
Test Run Successful.
Total tests: 1272
     Passed: 1272
 Total time: 13.8253 Seconds
```

| Count | Post-change (P3-T6) | Baseline (P0-T11) | Delta |
| --- | --- | --- | --- |
| Total tests | 1272 | 1272 | 0 |
| **Passed** | **1272** | 1272 | **0** |
| **Failed** | **0** | 0 | 0 |
| **Skipped** | **0** | 0 | 0 |

vstest omits the `Failed:` and `Skipped:` summary lines when those counts are zero; the
`Test Run Successful.` header and `Passed` equal to `Total tests` fix both at 0.

The counts are identical to the P0-T11 baseline. This assembly gains no test from this change (the
new regression test lives in `UtilitiesCS.Test`), so an unchanged count is the expected result.

## BASELINE_FAILURE_SET Subtraction

**The QuickFiler BASELINE_FAILURE_SET recorded by P0-T11 was EMPTY (cardinality 0).** This is stated
explicitly as the task requires.

- Reported `Failed` count: 0.
- After subtracting the empty baseline set: **0**.
- **No failure occurred that is not a member of that set, so there is no regression to record or
  report.**

## Why This Assembly Is in Scope

`QuickFiler.Test` is in the regression scope because
`QuickFiler.Test/Controllers/QfcItemControllerTests.cs` line 62 documents a dependency on the
`RunWithTimeout` path affected by this change's behavioral consequence: the previously dead retry
ladder at `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.Formatting.cs` line 80
(`timeoutMs: 1000`, `maxAttempts: 3`) becomes live. All 1272 tests pass unchanged, so that
behavioral change did not disturb any existing QuickFiler expectation.

Acceptance: met. The reported `Failed` count is 0 after subtracting the QuickFiler
BASELINE_FAILURE_SET, and the artifact states explicitly that that set was empty. No failure occurred
that is not a member of that set.
