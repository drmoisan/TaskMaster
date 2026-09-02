# P2-T5 — Post-change MSTest coverage run, remediation cycle 1

Timestamp: 2026-09-02T01-35

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`
EXIT_CODE: 0

## Why a second, scoped run is issued in this same task

That script builds its inner vstest argument list and passes no `/Logger:trx`, no
`/ResultsDirectory` and no console verbosity override, so its output names **failing** tests
and prints run totals but never names a **passing** test. A per-test pass list cannot be read
from it. The twelve-name confirmation is therefore taken from a second, scoped run issued
here, using Derivation D7 with `/ResultsDirectory:TestResults\p2-t5`.

D7's pre-run `/t:Build` step is not issued for that second run, because P2-T3 and P2-T4 have
already rebuilt the solution in this same pass and no source has changed since. That waiver
is granted by the task text itself and applies only to the optional pre-build; the scoped
vstest command was executed and its exit code recorded.

## Output Summary — full-suite run

The run printed the literal `Done. Coverage artifact:`, so both the Koverage post-processing
step and the on-disk write succeeded and the report at `coverage/coverage.cobertura.xml` is a
post-processed document.

```
Test Run Successful.
Total tests: 6949
     Passed: 6949
```

| Metric | Value |
|---|---|
| Total | 6949 |
| Passed | 6949 |
| Failed | 0 |
| Skipped | 0 |

The runner prints a `Failed:` line and a `Skipped:` line only when those counts are non-zero;
neither appears, and the header is `Test Run Successful.`

## Output Summary — scoped confirmation run

```
A total of 1 test files matched the specified pattern.
Total tests: 12
Test Run Successful.
```

Scoped run EXIT_CODE: 0. All twelve named individually as passed:

```
  Passed ResolveCarriedHandler_WhenEntryIdMatchesACarrier_ReturnsThatCarriersHandler [204 ms]
  Passed AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder [212 ms]
  Passed AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates [209 ms]
  Passed ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull [< 1 ms]
  Passed LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory [23 ms]
  Passed LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory [12 ms]
  Passed AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder [1 ms]
  Passed ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection [< 1 ms]
  Passed AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder [< 1 ms]
  Passed LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation [1 ms]
  Passed RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary [348 ms]
  Passed RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue [18 ms]
```

## Acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | full-suite `EXIT_CODE:` recorded | PASS — 0 |
| 2 | states whether the full-suite run printed `Done. Coverage artifact:` | PASS — it did |
| 3 | full-suite total, passed, failed, skipped recorded numerically | PASS — 6949 / 6949 / 0 / 0 |
| 4 | failing-test set is a subset of `R_BASELINE_FAILURE_SET` | PASS — see below |
| 5 | full-suite total is at least `R_BASELINE_TOTALS` total + 3 | PASS — see below |
| 6 | scoped run reports exactly 12 discovered and executed with 0 failed, TRX names all twelve as passed | PASS |

Clause 4 detail. `R_BASELINE_FAILURE_SET` from P0-T8 is the **empty set**. The post-change
failing set is also empty, and the empty set is a subset of the empty set, so the clause
holds. The subset form is used deliberately because a repository-wide zero-failures assertion
is not satisfiable in general when a baseline carries failures; at this particular baseline
the subset form is equivalent to and as strong as a zero-failures assertion, because the only
subset of the empty set is the empty set.

Clause 5 detail. `R_BASELINE_TOTALS` total is **6946**. The required floor is 6946 + 3 =
**6949**. The observed total is **6949**, which meets the floor exactly. The added count of 3
is the number of `[TestMethod]` declarations this cycle added: one by P1-T1
(`RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary`) and two by
P1-T6 (`AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder`
and `LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation`). The
exact match confirms no test was lost as well as none added beyond the three.

Clause 6 detail. `TestResults\p2-t5` was deleted before the run, so exactly one TRX exists in
it. None of the twelve filter substrings is a substring of another, so each `~` clause
selected exactly the test it names and the count of 12 is not inflated by a prefix collision.
