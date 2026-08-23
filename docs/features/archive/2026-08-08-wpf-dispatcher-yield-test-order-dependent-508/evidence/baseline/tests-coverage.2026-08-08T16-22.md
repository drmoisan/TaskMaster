# Baseline Full-Suite Test Run With Coverage (toolchain step 4)

Timestamp: 2026-08-08T16-22

Task: [P0-T10]

Command: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/baseline/coverage-baseline.cobertura.xml"`

EXIT_CODE: 0

## MSTest discovery assertion (required by the plan's `## MSTest Discovery Caveat`)

The runner's discovery filter (`Invoke-MSTestWithCoverage.ps1:296-302`) was reproduced exactly
(`*.Test.dll` under `\bin\Debug\`, excluding `\obj\` and `\ref\`) and the resulting set asserted:

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

- ASSERTION 1 PASS: all 9 discovered paths begin with the workspace-root prefix
  `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0\`.
- ASSERTION 2 PASS: no discovered path contains a `\.claude\worktrees\` segment **after** that
  prefix, so no stale sibling agent-worktree build was picked up.

The runner independently reported `Discovered 9 test assemblies.`, matching the assertion set.

## Test result

```
Test Run Successful.
Total tests: 6293
     Passed: 6293
```

Total 6293 / Passed 6293 / Failed 0 / Skipped 0.

Note on the defect under repair: `YieldAsync_WithoutDispatcher_RemainsStrict` **passed** in this
particular baseline run. That is expected and is itself the shape of the defect — the test is
order-dependent, not deterministically failing. The issue records two consecutive baseline runs at
this same merge-base with `Failed: 2` and `Failed: 1`. A green baseline run does not contradict the
defect; it demonstrates why a single green run is insufficient evidence (AC7 requires three).

## Repository-wide coverage headline (root `<coverage>` element)

```xml
<coverage line-rate="0.858162" branch-rate="0.792118" complexity="24646" version="1.9"
          timestamp="1786220438" lines-covered="95274" lines-valid="111021"
          branches-covered="22070" branches-valid="27862">
```

| Metric | Value |
|---|---|
| line-rate | 0.858162 (85.8162%) |
| branch-rate | 0.792118 (79.2118%) |
| lines-covered | 95274 |
| lines-valid | 111021 |
| branches-covered | 22070 |
| branches-valid | 27862 |

This is the baseline comparand for the P2-T11 non-regression gate. 85.8162% is above the
`.claude/rules/csharp.md` repository floor of 80%, so no pre-existing repo-wide coverage shortfall
exists and the escalation condition on that point does not trigger.

## VSTO-runtime condition (explicitly stated per the plan's execution note)

The plan warns that an absent Office Tools v4.0 VSTO runtime would produce four `CS0234`
diagnostics in `ThisAddIn.Designer.cs`, preventing `TaskMaster.Test` and `UtilitiesCS.Test` from
building and deflating the repository-wide line rate. **That condition did not occur.** Both
`TaskMaster.Test.dll` and `UtilitiesCS.Test.dll` are present in the discovered set and the P0-T8
build reported 0 errors. The 85.8162% figure is therefore a full-denominator measurement, not a
deflated one.

## Artifact

`docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/baseline/coverage-baseline.cobertura.xml`

Output Summary: PASS, EXIT_CODE 0. All 9 test assemblies discovered inside the workspace root with
zero stale sibling-worktree paths. Full suite: Total 6293, Passed 6293, Failed 0. Repository-wide
baseline line-rate 0.858162 (85.8162%), branch-rate 0.792118 (79.2118%), lines-covered 95274 of
111021. No VSTO CS0234 deflation. The order-dependent test happened to pass in this run, which is
consistent with the intermittent defect rather than contradicting it.
