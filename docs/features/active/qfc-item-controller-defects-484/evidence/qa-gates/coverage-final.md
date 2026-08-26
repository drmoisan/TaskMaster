# Final QC — post-change repository-wide coverage (Cobertura)

Timestamp: 2026-08-26T13-59
Task: [P7-T6]

Command (under `pwsh -NoProfile`, run from this worktree root):

```
.\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\qfc-item-controller-defects-484\evidence\qa-gates\coverage-final.cobertura.xml
```

EXIT_CODE: 0

## Cobertura artifact

`docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/coverage-final.cobertura.xml`
exists. (It is matched by the `*.cobertura.xml` pattern in the feature `evidence/.gitignore`, so the
numeric values are recorded here as the evidence of record, per the standing convention against
committing raw coverage artifacts.)

## Root `<coverage>` element attributes

```xml
<coverage line-rate="0.848323" branch-rate="0.788057" complexity="25081" version="1.9"
          timestamp="1787752807" lines-covered="53905" lines-valid="63543"
          branches-covered="12735" branches-valid="16160">
```

| Metric | Value |
|---|---|
| **`line-rate`** | **0.848323** (84.8323 percent) |
| **`branch-rate`** | **0.788057** (78.8057 percent) |
| `lines-covered` / `lines-valid` | 53905 / 63543 |
| `branches-covered` / `branches-valid` | 12735 / 16160 |

The `QuickFiler` package element records `line-rate="0.7722789115646258"` and
`branch-rate="0.7349223546406645"`, against the baseline package figures `0.768582968118931` and
`0.7269046742730954`.

## Test run inside the coverage collection

```
Test Run Successful.
Total tests: 6503
     Passed: 6503
 Total time: 1.4531 Minutes
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

This is the same set of 9 assemblies the `[P0-T14]` baseline discovered.

Output Summary: EXIT_CODE 0. Repository-wide `line-rate` 0.848323 and `branch-rate` 0.788057 across the
same 9 discovered test assemblies as the baseline, with 6503 of 6503 tests passing.
