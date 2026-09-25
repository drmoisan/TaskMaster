# R5 Pass-After and the Criterion Sibling Check

- Timestamp: 2026-09-20T08-51-10
- Task: [P2-T4]
- Finding: R5, decision D1
- Command: CMD-PESTER-FILTERED, three invocations
- EXIT_CODE: 0 for all three

## Run 1 — The R5 Pass-After

`<FILE>` = `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1`
`<FILTER>` = `*R5- preserves a Reference assembly version*`

```
PESTER Passed=1 Failed=0 Skipped=0 Executed=1 Total=12 NotRun=11
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `Executed` = `Passed + Failed + Skipped` | 1 | **1** | PASS |
| `Passed` | 1 | **1** | PASS |
| `EXIT_CODE` | 0 | **0** | PASS |

This is the pass-after half of the [P2-T2] fail-before. The same test, the same fixture, the same
filter: red at exit 1 before [P2-T3], green at exit 0 after it.

| Stage | Task | `Executed` | `Passed` | `Failed` | `EXIT_CODE` |
|---|---|---|---|---|---|
| Fail-before | [P2-T2] | 1 | 0 | **1** | 1 |
| Pass-after | [P2-T4] | 1 | **1** | 0 | 0 |

## Run 2 — The `AC16-` Criterion Suite

`<FILE>` = `tests/scripts/dependencies/ProjectConsistency.Tests.ps1`
`<FILTER>` = `*AC16-*`

```
PESTER Passed=2 Failed=0 Skipped=0 Executed=2 Total=17 NotRun=15
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `Executed` | 2 | **2** | PASS |
| `Failed` | 0 | **0** | PASS |

## Run 3 — The `AC21-` Criterion Suite

`<FILE>` = `tests/scripts/dependencies/ProjectConsistency.Tests.ps1`
`<FILTER>` = `*AC21-*`

```
PESTER Passed=1 Failed=0 Skipped=0 Executed=1 Total=17 NotRun=16
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `Executed` | 1 | **1** | PASS |
| `Failed` | 0 | **0** | PASS |

## Why `Executed` and Not `Total`

Every `Executed` figure above is `Passed + Failed + Skipped`. `Total` and `NotRun` are recorded
only as context. Per **gate rule 2**, `TotalCount` on a `Filter.FullName` run counts the
filtered-out tests as `NotRun`, so it reports the whole file's `It` count — 17 for
`ProjectConsistency.Tests.ps1` in both runs 2 and 3 — and is invariant under the filter,
including a filter matching nothing. An assertion on `Total` would pass for a filter that
selected zero tests.

## The Sibling Check Decision D1 Turns On

Runs 2 and 3 are not decoration. Decision **D1** removes the Reference rewrite from the module
entry point, and the risk that creates is a **residual** disagreement: a `Reference` line left
unrewritten could, in principle, turn a success result into a failure result at the post-repair
detection stage.

These two runs are the check that it did not.

- The two `AC16-` cases assert that the verifier repairs freely and fails only on residual
  inconsistency, in both directions, and their `Kind` assertions cover `Import` and `HintPath`,
  not `Reference`.
- The `AC21-` case is the #908 three-way divergence regression fixture, and its `Kind` assertions
  cover `Import`, `Error` and `Analyzer`, not `Reference`.

None of the three depends on the removed rewrite, and all three stay green.

This is consistent with the structural reason D1 gives: `Find-VersionDisagreement` in
`scripts/dependencies/ConsistencyVerifier.psm1` states in its own description that a
`<Reference>` is **outside the detector**, because its `Include` carries an assembly version that
need not track the package version. Leaving a Reference line unrewritten therefore creates no
residual divergence and cannot turn an `IsSuccess` result into a failure. The two criterion runs
confirm empirically what that reading predicts.

## Output Summary

Three runs, all exit 0. The R5 test is green after the fix, completing the fail-before and
pass-after pair. Both delivered criterion suites — `AC16-` at 2 executed and `AC21-` at 1
executed — remain green, so the D1 discharge re-based no criterion evidence.
