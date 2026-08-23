# Toolchain Step 4 (test with coverage) — FINAL CLEAN PASS (pass 4)

Timestamp: 2026-08-08T16-55

Task: [P2-T5] — final QC loop, pass 4

Command: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/qa-gates/coverage-postchange.cobertura.xml"`

EXIT_CODE: 0

```
Discovered 9 test assemblies.
Test Run Successful.
Total tests: 6295
     Passed: 6295
Done. Coverage artifact: ...\evidence\qa-gates\coverage-postchange.cobertura.xml
```

Total 6295 / Passed 6295 / Failed 0 / Skipped 0. **Fully green.**

## MSTest discovery assertion (required by the plan's `## MSTest Discovery Caveat`)

The runner's filter (`Invoke-MSTestWithCoverage.ps1:296-302`) was reproduced and the set asserted:

```
DISCOVERED_COUNT=9
  ...\agent-ad7090ae544fd0fb0\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
  ...\agent-ad7090ae544fd0fb0\SVGControl.Test\bin\Debug\SVGControl.Test.dll
  ...\agent-ad7090ae544fd0fb0\Tags.Test\bin\Debug\Tags.Test.dll
  ...\agent-ad7090ae544fd0fb0\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
  ...\agent-ad7090ae544fd0fb0\TaskTree.Test\bin\Debug\TaskTree.Test.dll
  ...\agent-ad7090ae544fd0fb0\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
  ...\agent-ad7090ae544fd0fb0\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
  ...\agent-ad7090ae544fd0fb0\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
  ...\agent-ad7090ae544fd0fb0\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll

OUTSIDE_WORKSPACE_ROOT_COUNT=0
NESTED_WORKTREE_SEGMENT_COUNT=0
```

- ASSERTION 1 PASS: all 9 paths begin with the workspace-root prefix
  `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0\`.
- ASSERTION 2 PASS: no path contains a `\.claude\worktrees\` segment **after** that prefix, so no
  stale sibling agent-worktree build was discovered.

The runner independently reported `Discovered 9 test assemblies.`

## Test count reconciliation

| Run | Total | Passed | Failed |
|---|---|---|---|
| Baseline (P0-T10) | 6293 | 6293 | 0 |
| Pass 4 (this run) | 6295 | 6295 | 0 |

Delta +2 total, exactly the two tests added by P1-T10 and P1-T11. No test was deleted, skipped,
`[Ignore]`d, or filtered out.

## The four in-scope tests

```
Passed YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield [1 ms]
Passed YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback [26 ms]
Passed YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher [23 ms]
Passed YieldAsync_WithoutDispatcher_RemainsStrict [2 ms]
```

## Repository-wide coverage headline (root `<coverage>` element)

```xml
<coverage line-rate="0.858328" branch-rate="0.792318" complexity="24661" version="1.9"
          timestamp="1786222160" lines-covered="95325" lines-valid="111059"
          branches-covered="22093" branches-valid="27884">
```

| Metric | Baseline (P0-T10) | Post-change (P2-T5) | Delta |
|---|---|---|---|
| line-rate | 0.858162 | 0.858328 | **+0.000166** |
| branch-rate | 0.792118 | 0.792318 | +0.000200 |
| lines-covered | 95274 | 95325 | +51 |
| lines-valid | 111021 | 111059 | +38 |
| branches-covered | 22070 | 22093 | +23 |
| branches-valid | 27862 | 27884 | +22 |

Both rates moved up. Analyzed at P2-T11.

## Relationship to the earlier failed passes

Passes 1 and 2 of this loop failed with `Failed: 2` — both times the same two out-of-scope
`QuickFiler.Test` tests (`QfcItemController_InitializationTests`, WinForms window-handle race). A
controlled four-run attribution experiment
(`<FEATURE>/evidence/regression-testing/preexisting-failure-attribution.2026-08-08T16-52.md`) proved
those failures are pre-existing: with the change fully reverted to merge-base, the same two tests
fail with `6293 / 6291 / 2`, byte-for-byte matching the "Run 1" figures already recorded at
`<FEATURE>/issue.md:53`. They pass in class isolation (9/9) and in their own assembly (867/867).

In pass 4 those two tests passed, confirming they are intermittent rather than deterministic. No
`[Ignore]`, `[DoNotParallelize]`, test-case filter, or retry was introduced to obtain this green
result — the same unmodified command was rerun per the loop's restart rule.

## VSTO-runtime condition

The plan's execution note warns of four `CS0234` diagnostics in `ThisAddIn.Designer.cs` if the
Office Tools v4.0 VSTO runtime is absent, which would prevent `TaskMaster.Test` and `UtilitiesCS.Test`
from building and deflate the repo-wide rate. **That condition did not occur.** Both assemblies are
in the discovered set and P2-T3 reported 0 errors, so 85.8328% is a full-denominator figure.

Output Summary: PASS, EXIT_CODE 0. Full suite fully green: Total 6295, Passed 6295, Failed 0 — up
exactly +2 from the 6293 baseline, matching the two tests added by P1-T10/P1-T11. All 9 assemblies
discovered inside the workspace root with zero stale sibling-worktree paths. All four
`WpfDispatcherYieldTests` passed. Repository-wide line-rate 0.858328 (up from 0.858162) and
branch-rate 0.792318 (up from 0.792118). No VSTO CS0234 deflation.
