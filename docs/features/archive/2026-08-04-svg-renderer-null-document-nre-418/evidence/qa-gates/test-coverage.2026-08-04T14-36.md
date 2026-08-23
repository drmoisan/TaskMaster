# [P2-T7] Full Test Suite with Coverage — Final QC Pass 1

Timestamp: 2026-08-04T20-02

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

EXIT_CODE: 0

Output Summary:

### Test result

| Metric | Value |
|---|---|
| Total tests | **6140** |
| Passed | **6140** |
| **Failed** | **0** |
| Skipped | **0** |
| Total time | 58.2533 s |

`Test Run Successful.` Coverage artifact written to `coverage/coverage.cobertura.xml`, then
post-processed for Koverage compatibility by the wrapper script.

### Assemblies discovered (9)

`QuickFiler.Test.dll`, `SVGControl.Test.dll`, `Tags.Test.dll`, `TaskMaster.Test.dll`,
`TaskTree.Test.dll`, `TaskVisualization.Test.dll`, `ToDoModel.Test.dll`, `UtilitiesCS.Test.dll`,
`VBFunctions.Test.dll`.

Same nine assemblies as the `2026-08-04T21-04` baseline. Test count 6112 -> 6140, a delta of **+28**:
the 27 tests delivered in Phase 1 plus the one added by `[P2-T1]`.

### Repository-wide coverage (numeric)

Read from the `<coverage>` root element of `coverage/coverage.cobertura.xml`, and independently
reproduced by summing per-`<line>` counts across all nine deduped `<package>` elements (exact match).

| Metric | Numerator / Denominator | Percentage |
|---|---|---|
| Line coverage (`line-rate` = `0.853844`) | **93484 / 109486** | **85.3844%** |
| Branch coverage (`branch-rate` = `0.785521`) | **21528 / 27406** | **78.5521%** |

Both are above the `.claude/rules/general-unit-test.md` floors of `>= 85%` line and `>= 75%` branch.

### `SVGControl` package numeric line coverage (required by the task)

**`SVGControl` line coverage: 1648 / 3500 = 47.0857%.** Branch coverage: 544 / 1236 = 44.0129%.

Counted by the same per-`<line>`-descendant method the `2026-08-04T21-04` baseline used, so the figure
is directly comparable to that baseline's `1412 / 3266 = 43.2333%`. The `<package name="SVGControl">`
element's own attributes read `line-rate=0.46409140369967355` (46.4091%) and
`branch-rate=0.435126582278481` (43.5127%); those attributes are computed differently and are recorded
here for completeness only.

### Per-package line coverage, all nine packages

| Package | `line-rate` attribute | `branch-rate` attribute |
|---|---|---|
| VBFunctions | 1.000000 | 1.000000 |
| TaskTree | 0.954839 | 0.921569 |
| Tags | 0.926893 | 0.915789 |
| TaskVisualization | 0.898433 | 0.832500 |
| UtilitiesCS | 0.892018 | 0.830303 |
| QuickFiler | 0.799041 | 0.734570 |
| TaskMaster | 0.674317 | 0.611111 |
| ToDoModel | 0.573106 | 0.488189 |
| **SVGControl** | **0.464091** | **0.435127** |

### Contention note — first attempt aborted, rerun clean

The first invocation of this exact command aborted with
`The active test run was aborted. Reason: Test host process crashed` after 1266 passing tests
(16.19 s), inside `TaskVisualization.Test`, and the wrapper threw
`MSTest with coverage failed with exit code 1`. No test reported `Failed`.

Handled as environmental contention, not a code failure, per the executing directive:

- No stale `testhost`, `vstest.console`, `datacollector`, or `dotnet-coverage` process was left behind
  by the aborted run; the process table was verified clear before rerunning.
- Three foreign `codex.exe` agent processes and their `pwsh` children were alive throughout. **No
  process this executor did not start was terminated.**
- The command was rerun unchanged and returned `EXIT_CODE: 0` with 6140/6140 passing.

No source, test, or configuration file was modified between the two invocations, so the rerun is a
rerun of the same code state, not a retry after a fix. The clean run above is the recorded result.
