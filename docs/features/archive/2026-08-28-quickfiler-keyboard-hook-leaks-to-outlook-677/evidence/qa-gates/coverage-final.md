# Final QA Gate 5 — Full-Suite Coverage Run (P5-T5)

Timestamp: 2026-08-28T16-09
Command (CR-COVERAGE, fully expanded):

```
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/qa-gates/coverage-final.cobertura.xml
```

EXIT_CODE: 0

## Output Summary

### Coverage (root `<coverage>` element of `coverage-final.cobertura.xml`)

- `line-rate` = **0.852804**
- `branch-rate` = **0.792300**
- `lines-covered` = 54721
- `lines-valid` = 64166
- `branches-covered` = 13027
- `branches-valid` = 16442

Cobertura artifact present at
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/qa-gates/coverage-final.cobertura.xml`
(10,758,364 bytes). The script's `Assert-CoberturaLineCoverageThreshold` post-processing step
completed without throwing, so the repository line-coverage threshold is satisfied post-change.

### Test counts

```
Discovered 9 test assemblies.
Total tests: 6838
     Passed: 6838
```

- Total: **6838** (= the 6821 baseline total plus exactly the 17 new tests)
- Passed: **6838**
- Failed: **0**
- Skipped: **0** (vstest emitted no `Skipped:` line, which it prints only for a non-zero count)

### Zero failures not in `BASELINE_FAILURE_SET`

`BASELINE_FAILURE_SET` from P0-T9 is empty and this run recorded 0 failures, so the count of
failures not present in `BASELINE_FAILURE_SET` is **0**.

### Discovered-assembly assertion (same as P0-T9)

`Discovered 9 test assemblies.` The same discovery predicate reproduced independently yields
exactly these nine paths, all rooted at the workspace root and none containing a `\.claude\`
segment:

```
<WORKSPACE_ROOT>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
<WORKSPACE_ROOT>\SVGControl.Test\bin\Debug\SVGControl.Test.dll
<WORKSPACE_ROOT>\Tags.Test\bin\Debug\Tags.Test.dll
<WORKSPACE_ROOT>\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
<WORKSPACE_ROOT>\TaskTree.Test\bin\Debug\TaskTree.Test.dll
<WORKSPACE_ROOT>\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
<WORKSPACE_ROOT>\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
<WORKSPACE_ROOT>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
<WORKSPACE_ROOT>\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

- Paths containing a `\.claude\` segment: **0**
- Paths not starting with the workspace root: **0**
