# Full-Suite Coverage Baseline (P0-T9)

Timestamp: 2026-08-28T15-48
Command (CR-COVERAGE, fully expanded):

```
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/baseline/coverage-baseline.cobertura.xml
```

Inner invocation (built by the script): `dotnet-coverage collect --output <cobertura> --output-format cobertura --settings <derived coverage.config> -- <vstest.console.exe> <9 test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`

EXIT_CODE: 0

## Output Summary

### Coverage (root `<coverage>` element of `coverage-baseline.cobertura.xml`)

- `line-rate` = **0.852721**
- `branch-rate` = **0.792255**
- `lines-covered` = 54685
- `lines-valid` = 64130
- `branches-covered` = 13012
- `branches-valid` = 16424

Cobertura artifact present at
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/baseline/coverage-baseline.cobertura.xml`
(10,749,834 bytes). The script's `Assert-CoberturaLineCoverageThreshold` post-processing step
completed without throwing, so the repository line-coverage threshold was satisfied at baseline.

### Test counts

```
Test Run Successful.
Total tests: 6821
     Passed: 6821
 Total time: 39.4922 Seconds
```

- Total: **6821**
- Passed: **6821**
- Failed: **0**
- Skipped: **0** (vstest emitted no `Skipped:` line, which it prints only for a non-zero count)

BASELINE_FAILURE_SET: (empty — zero failing tests and zero skipped tests at baseline)

### Discovered-assembly assertion

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
