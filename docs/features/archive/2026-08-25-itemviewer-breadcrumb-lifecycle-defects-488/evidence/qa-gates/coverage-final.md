# Final Repository-Wide Coverage ([P8-T6])

Timestamp: 2026-08-28T06-27

Command (under `pwsh -NoProfile` from this worktree root):

```
.\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\qa-gates\coverage-final.cobertura.xml
```

EXIT_CODE: 0

## Cobertura file

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/qa-gates/coverage-final.cobertura.xml`
exists.

## Root `coverage` element attributes

```
<coverage line-rate="0.85283" branch-rate="0.792255" complexity="25354" version="1.9"
          timestamp="1787898394"
          lines-covered="54692" lines-valid="64130"
          branches-covered="13012" branches-valid="16424">
```

| Measure | Value |
| --- | --- |
| **Post-change raw line-rate** | **0.85283** (85.283%) |
| **Post-change raw branch-rate** | **0.792255** (79.2255%) |
| Lines covered / valid | 54692 / 64130 |
| Branches covered / valid | 13012 / 16424 |

Both clear the uniform repository floors of `>= 85%` line and `>= 75%` branch, and the line figure also
clears the `>= 80%` floor in `CLAUDE.md` § UT2. `[P8-T7]` performs the baseline comparison and computes
the testable-denominator figures.

## Test run inside the coverage collection

| Measure | Value |
| --- | --- |
| Total tests | **6821** |
| Passed | **6821** |
| Failed | **0** |
| Run result | `Test Run Successful.` |

The baseline run in `[P0-T14]` recorded 6812 total and 6812 passed. The delta is **+9**, exactly the net
new-test delta of ten added methods minus one deleted method, and it matches the `[P8-T5]` figure of
1201 versus a 1192 baseline in `QuickFiler.Test`. Every added test is in the aggregate run and passes.

The runner applies `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook` to every assembly,
matching CI and preventing a real Outlook process from being launched.

## Discovered test assemblies

**Total discovered: 9.** The runner's own output line reads `Discovered 9 test assemblies.` The verbatim
list, re-derived by applying the runner's discovery predicate from this worktree root:

```
./QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
./SVGControl.Test/bin/Debug/SVGControl.Test.dll
./Tags.Test/bin/Debug/Tags.Test.dll
./TaskMaster.Test/bin/Debug/TaskMaster.Test.dll
./TaskTree.Test/bin/Debug/TaskTree.Test.dll
./TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll
./ToDoModel.Test/bin/Debug/ToDoModel.Test.dll
./UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll
./VBFunctions.Test/bin/Debug/VBFunctions.Test.dll
```

Nine entries, agreeing with the runner's count and identical to the `[P0-T14]` baseline list.

### Every entry is under this worktree root, and none carries a nested `worktrees` segment

Each path is rendered relative to this worktree root, which is what establishes that every discovered
assembly lies beneath it. **None contains a `worktrees` path segment below this worktree root.** That
also holds structurally: this worktree's `.claude/` directory contains no `worktrees` subdirectory, so
recursive discovery from this root cannot reach a sibling agent worktree. The worktree root's own
absolute path contains a `worktrees` segment because this worktree lives under the primary checkout's
`.claude/worktrees/`; that segment is above the root, not below it.

Output Summary: EXIT_CODE 0. Post-change repository-wide **line-rate 0.85283** and **branch-rate
0.792255** (54692/64130 lines, 13012/16424 branches). **9** test assemblies discovered, all under this
worktree root with no nested `worktrees` segment. 6821 tests ran, 6821 passed, 0 failed — a **+9** delta
against the 6812-test baseline, matching the net new-test count exactly.
