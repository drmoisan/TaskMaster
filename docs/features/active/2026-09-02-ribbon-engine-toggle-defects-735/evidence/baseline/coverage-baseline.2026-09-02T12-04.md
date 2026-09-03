# Phase 0 — Coverage Baseline, Whole First-Party Suite (P0-T9)

Timestamp: 2026-09-03T01-32
Task: [P0-T9]
Command: `pwsh -NoProfile -File <worktree>/scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration 'Debug' -CoverageOutput 'docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\baseline\coverage-baseline.2026-09-02T12-04.cobertura.xml'`
EXIT_CODE: 0

Cobertura document written to
`docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/coverage-baseline.2026-09-02T12-04.cobertura.xml`.

The run completed Koverage post-processing (`Post-processing coverage XML for Koverage
compatibility...` followed by `Done. Coverage artifact: ...`), so the document on disk is the
POST-PROCESSED Cobertura with workspace-relative `filename` attributes, not the raw dotnet-coverage
output. P4-T7 must reach the same post-processed state for the two documents to be comparable.

## Test run result

```
Test Run Successful.
Total tests: 6955
     Passed: 6955
 Total time: 38.4429 Seconds
```

Zero failed, zero skipped. The script's inner vstest invocation always applies
`/TestCaseFilter:TestCategory!=LiveOutlook`, so this run started no external Outlook process.

## Acceptance, part 1 — discovery scope

Nine test assemblies were discovered. Each is listed below relative to the workspace root recorded
in P0-T11, `<WORKSPACE_ROOT>` = `<REPOS_ROOT>/TaskMaster/.claude/worktrees/agent-a3324f355df219b0e`:

1. `<WORKSPACE_ROOT>/QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
2. `<WORKSPACE_ROOT>/SVGControl.Test/bin/Debug/SVGControl.Test.dll`
3. `<WORKSPACE_ROOT>/Tags.Test/bin/Debug/Tags.Test.dll`
4. `<WORKSPACE_ROOT>/TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
5. `<WORKSPACE_ROOT>/TaskTree.Test/bin/Debug/TaskTree.Test.dll`
6. `<WORKSPACE_ROOT>/TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll`
7. `<WORKSPACE_ROOT>/ToDoModel.Test/bin/Debug/ToDoModel.Test.dll`
8. `<WORKSPACE_ROOT>/UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
9. `<WORKSPACE_ROOT>/VBFunctions.Test/bin/Debug/VBFunctions.Test.dll`

Every one of the nine is under the workspace root. The check applied is that no discovered path
contains a further `worktrees` segment relative to that root; none does. A "contains no `.claude`"
filter is meaningless in this cycle, because the workspace root itself sits beneath a `.claude`
segment and such a filter would reject all nine legitimate assemblies.

## Acceptance, part 2 — numeric headline from the root `coverage` element

| Attribute | Value |
|---|---|
| `line-rate` | 0.853867 (85.3867%) |
| `branch-rate` | 0.794649 (79.4649%) |
| `lines-covered` | 55141 |
| `lines-valid` | 64578 |

## Acceptance, part 3 — per-file figures

Aggregation method, stated so P4-T8 reproduces it exactly: select every `class` element whose
`filename` attribute, normalised to backslash separators, ends with the file's path; collect every
`.//line` descendant of those elements; deduplicate by the `number` attribute; count a line as
covered when its `hits` attribute is greater than zero. Deduplication matters because compiler
generated async state-machine and display classes can emit `line` entries for the same physical
source line, and summing without deduplication double-counts them.

| File | Matching `class` elements | Covered | Total | Line coverage |
|---|---|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | 1 (`TaskMaster.EngineToggleStateCoordinator`) | 133 | 135 | **98.52%** |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | 0 | — | — | **ABSENT — pre-existing type-level ExcludeFromCodeCoverage on the containing type** |
| `TaskMaster/Ribbon/SpamManagerResetGate.cs` | 0 | — | — | **NOT APPLICABLE — file does not exist at baseline** |

No baseline figure is invented for either absent file. `RibbonController.Intelligence.cs` is a
partial of `RibbonController`, which carries a type-level `[ExcludeFromCodeCoverage]`, so the
instrumenter emits no `class` element for it and no number can be read. `SpamManagerResetGate.cs`
is created by P2-T1 and therefore has no baseline at all.

`EngineToggleStateCoordinator.cs` at **98.52%** is the value P4-T8 must meet or exceed after the
Finding 3 changes.

Output Summary: Coverage baseline succeeded with EXIT_CODE 0 over 9 discovered test assemblies and
6955 tests, all passed. Root line-rate 0.853867, branch-rate 0.794649 (55141 of 64578 lines).
`EngineToggleStateCoordinator.cs` baseline line coverage is 98.52% (133/135);
`RibbonController.Intelligence.cs` is ABSENT under a pre-existing type-level exemption; and
`SpamManagerResetGate.cs` is NOT APPLICABLE because it does not yet exist.
