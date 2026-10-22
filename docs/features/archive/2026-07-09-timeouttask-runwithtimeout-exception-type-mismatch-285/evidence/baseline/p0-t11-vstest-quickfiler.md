# P0-T11 — `QuickFiler.Test` Baseline Test Run

Timestamp: 2026-09-01T08-11 (test run started 2026-09-01T08-10)

## Command

```text
<resolved-vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\p0-t11 /TestCaseFilter:TestCategory!=LiveOutlook
```

`<resolved-vstest>` is the vswhere-resolved path recorded in the P0-T10 artifact:
`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

The assembly was run **alone**, not as part of an aggregate assembly list. The
`/TestCaseFilter:TestCategory!=LiveOutlook` operand was present.

EXIT_CODE: 0

## Invocation Count

**One invocation.** The test host did not report a crash and the run did not hang, so the plan's
conditional single re-run branch was not taken. The run header reads `Test Run Successful.` and
completed in 13.9 seconds.

## Output Summary

vstest's trailing summary, verbatim:

```text
Test Run Successful.
Total tests: 1272
     Passed: 1272
 Total time: 13.8854 Seconds
```

| Count | Value |
| --- | --- |
| Total tests | 1272 |
| **Passed** | **1272** |
| **Failed** | **0** |
| **Skipped** | **0** |

vstest omits the `Failed:` and `Skipped:` summary lines when those counts are zero. The `Passed`
count equals the `Total tests` count, corroborating both zeros. A scan of the captured log for
result lines beginning `Failed ` or `Skipped `, and for crash or error-message markers, returned only
two `Passed` lines whose test names happen to contain the substring `Crash`
(`PopulateConversationAsync_WhenLoadFailsWithNonCancellation_ReturnsWithoutCrash` and
`PopulateConversationAsync_WhenSeamReturnsNullResolver_ReturnsWithoutCrash`). Both passed; neither is
a failure or a host crash.

## QuickFiler BASELINE_FAILURE_SET

**The QuickFiler BASELINE_FAILURE_SET is the EMPTY SET. Cardinality: 0.**

The `Failed` count is 0, so there are no failing test identities to enumerate. Phase 3's P3-T6
subtracts this empty set, so its required post-change `Failed` count is an unqualified 0.

This empty set is also consumed by P4-T11: because both it and the UtilitiesCS BASELINE_FAILURE_SET
are empty, AC11's literal `0 failures` wording is satisfiable with no `REMEDIATION-REQUIRED` entry
arising from either assembly.

This assembly is in the regression scope because `QuickFiler.Test/Controllers/QfcItemControllerTests.cs`
line 62 documents a dependency on the `RunWithTimeout` path affected by the behavioral change this
fix introduces (the previously dead retry ladder becoming live).

Acceptance: met. The artifact records all three integer test counts (1272 / 0 / 0). The
BASELINE_FAILURE_SET is empty, so no failing test identities are enumerated.
