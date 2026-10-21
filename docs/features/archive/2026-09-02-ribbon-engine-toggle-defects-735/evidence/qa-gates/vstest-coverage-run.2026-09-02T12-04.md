# Phase 4 — Coverage-Enabled Test Gate (P4-T7)

Timestamp: 2026-09-03T03-18
Task: [P4-T7]
Command: `pwsh -NoProfile -File <worktree>/scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration 'Debug' -CoverageOutput 'docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\qa-gates\coverage-final.2026-09-02T12-04.cobertura.xml'`
EXIT_CODE: 0

Cobertura document written to
`docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/coverage-final.2026-09-02T12-04.cobertura.xml`.

The run completed Koverage post-processing (`Post-processing coverage XML for Koverage
compatibility...` followed by `Done. Coverage artifact: ...`), so this document is in the same
post-processed state as the P0-T9 baseline and the two are comparable on equal terms.

## Test result

```
Test Run Successful.
Total tests: 6982
     Passed: 6982
 Total time: 31.4923 Seconds
```

**Zero failed tests.** The population moved from the P0-T9 baseline of 6955 to 6982, an increase of
27, which is exactly this change's new tests: 2 XML-consistency tests (Finding 1), 9 gate tests
(Finding 2), 6 race tests (Finding 3) and 10 cache tests (P4-T3 branch B). 2 + 9 + 6 + 10 = 27.

The same arithmetic holds within the ribbon namespace: the P4-T3 ribbon run measured 134 against the
P0-T8 baseline of 107, a delta of 27. All 27 new tests are declared in `TaskMaster.Test.Ribbon`, so
the namespace-scoped delta and the suite-wide delta agree exactly. No test was removed and none was
skipped.

CORRECTION (recorded 2026-09-03T09-20, after feature review). As first written, this section stated
the cache fixture contributed 9 tests, summed the four groups to 26, and then explained the
resulting one-test discrepancy by asserting that
`GetPressed_WhenEnginesAccessorReturnsNull_ReturnsFalseAndStartsNothing` was a pre-existing test
absent from the P0-T8 baseline filter. Both parts of that were wrong. The cache fixture
`TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs` declares 10 `[TestMethod]` members,
not 9, so the four groups sum to 27 and no discrepancy ever existed; and that named test is in fact
present in the P0-T8 baseline TRX at
`docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/p0-t8/p0-t8.trx`,
so the explanation offered for the phantom discrepancy was not merely unnecessary but false. The
underlying measurements — 6982 tests run, 6982 passed, 0 failed — are unchanged and were never in
question; only the reconciliation prose was defective.

The script always applies `/TestCaseFilter:TestCategory!=LiveOutlook` to its inner vstest
invocation, so this run started no external Outlook process, exactly as the baseline did.

## Discovered-assembly scope check

Nine test assemblies discovered, identical to the P0-T9 baseline set. Listed relative to the
workspace root recorded in P0-T11,
`<WORKSPACE_ROOT>` = `<REPOS_ROOT>/TaskMaster/.claude/worktrees/agent-a3324f355df219b0e`:

1. `<WORKSPACE_ROOT>/QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
2. `<WORKSPACE_ROOT>/SVGControl.Test/bin/Debug/SVGControl.Test.dll`
3. `<WORKSPACE_ROOT>/Tags.Test/bin/Debug/Tags.Test.dll`
4. `<WORKSPACE_ROOT>/TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
5. `<WORKSPACE_ROOT>/TaskTree.Test/bin/Debug/TaskTree.Test.dll`
6. `<WORKSPACE_ROOT>/TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll`
7. `<WORKSPACE_ROOT>/ToDoModel.Test/bin/Debug/ToDoModel.Test.dll`
8. `<WORKSPACE_ROOT>/UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
9. `<WORKSPACE_ROOT>/VBFunctions.Test/bin/Debug/VBFunctions.Test.dll`

Every one is under the workspace root and none contains a further `worktrees` segment relative to
that root, which is the check applied — the workspace root itself sits beneath a `.claude` segment,
so a "contains no `.claude`" filter would reject all nine legitimate assemblies.

## Numeric headline from the root `coverage` element

| Attribute | Baseline (P0-T9) | Final (this run) | Movement |
|---|---|---|---|
| `line-rate` | 0.853867 | **0.854109** | +0.000242 |
| `branch-rate` | 0.794649 | **0.794984** | +0.000335 |
| `lines-covered` | 55141 | 55225 | +84 |
| `lines-valid` | 64578 | 64658 | +80 |

Repository-wide line coverage is 85.4109%, above the repository floor. Both rates moved upward: the
change added 80 instrumented lines and covered 84 more than before, so it is net coverage-positive
rather than merely non-regressive.

Output Summary: The coverage gate passed with EXIT_CODE 0 and zero failed tests across 6982 tests in
9 discovered assemblies, all under the workspace root. Root `line-rate` is 0.854109 and `branch-rate`
is 0.794984, both above the P0-T9 baseline figures of 0.853867 and 0.794649.
