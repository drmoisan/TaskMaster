# Phase 0 — Baseline Repository-Wide Coverage

Timestamp: 2026-08-26T08-41
Task: [P0-T14]

Command (run under `pwsh -NoProfile` from this worktree root):

```
.\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\qfc-item-controller-defects-484\evidence\baseline\coverage-baseline.cobertura.xml
```

EXIT_CODE: 0

## Cobertura artifact

`docs/features/active/qfc-item-controller-defects-484/evidence/baseline/coverage-baseline.cobertura.xml`
exists.

## Root `<coverage>` element attributes

```xml
<coverage line-rate="0.84775" branch-rate="0.786876" complexity="25028" version="1.9"
          timestamp="1787747744" lines-covered="53766" lines-valid="63422"
          branches-covered="12675" branches-valid="16108">
```

| Metric | Value |
|---|---|
| **`line-rate`** | **0.84775** (84.775 percent) |
| **`branch-rate`** | **0.786876** (78.6876 percent) |
| `lines-covered` / `lines-valid` | 53766 / 63422 |
| `branches-covered` / `branches-valid` | 12675 / 16108 |

For reference in the `[P7-T7]` delta, the `QuickFiler` package element records
`line-rate="0.768582968118931"` and `branch-rate="0.7269046742730954"`.

## Test run inside the coverage collection

```
Test Run Successful.
Total tests: 6482
     Passed: 6482
 Total time: 42.7930 Seconds
```

## Discovered test assemblies

Count reported by the script: **9**.

```
Discovered 9 test assemblies.
```

Verbatim discovered-assembly list, reproduced with the script's own discovery predicate
(`*.Test.dll` under `\bin\Debug\`, excluding `\obj\` and `\ref\`), rendered relative to this worktree
root:

```
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
SVGControl.Test\bin\Debug\SVGControl.Test.dll
Tags.Test\bin\Debug\Tags.Test.dll
TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
TaskTree.Test\bin\Debug\TaskTree.Test.dll
TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

Every entry is a path under this worktree root, and **no entry contains a nested `worktrees` segment
below it**. This is independently confirmed by the absence of a `.claude/worktrees` directory inside this
worktree: `ls -d .claude/worktrees` reports `No such file or directory`, so no sibling agent worktree is
reachable from the search root and no relative-path exclusion filter was required.

Output Summary: Baseline repository-wide coverage is `line-rate` **0.84775** and `branch-rate`
**0.786876**, measured over 9 discovered test assemblies with all 6482 tests passing. The baseline line
rate is above the 80 percent policy floor.
