# [P0-T13] Baseline coverage collection

Timestamp: 2026-09-08T00-43

Command:

```
dotnet-coverage collect --output coverage\809-p0-baseline.cobertura.xml --output-format cobertura --settings coverage\809-effective-coverage.config -- $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx;LogFileName=p0t13.trx' '/ResultsDirectory:TestResults\809-p0t13' '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

The derived settings file `coverage\809-effective-coverage.config` was built by loading repository-root `coverage.config` and appending one `<ModulePath>.*\.Test\.dll$</ModulePath>` to `/Configuration/CodeCoverage/ModulePaths/Exclude`. `coverage/` is matched by `.gitignore:144`, so neither the settings file nor the Cobertura document enters the tree; the numeric findings recorded here are the retained evidence.

EXIT_CODE: 0

## Counting method (reproduced by name)

The selection is the all-descendant `.//line` selection over each first-party `<package>`, and only that one. The two narrower selections `classes/class/lines/line` and `classes/class/methods/method/lines/line` are **rejected by name and were not substituted**. A `<line>` counts as covered when its `hits` attribute is greater than zero. Branch figures are summed from the `(numerator/denominator)` pair inside each `condition-coverage` attribute over the same line set. `GetAttribute` was used rather than property access so a `<line>` lacking an attribute yields an empty string instead of throwing under `Set-StrictMode`.

Cobertura `<package>` elements produced by `dotnet-coverage` carry `line-rate` and `branch-rate` but carry no `lines-covered`, `lines-valid`, `branches-covered` or `branches-valid` attributes, so those four figures are aggregated from `<line>` elements and the denominator depends entirely on the selection.

The first-party allowlist is the nine production assembly names `Tags`, `ToDoModel`, `TaskVisualization`, `UtilitiesCS`, `QuickFiler`, `TaskTree`, `TaskMaster`, `SVGControl`, `VBFunctions`.

## Output Summary

Test run: `Test Run Successful.`, `Total tests: 7120`, `     Passed: 7120`. TRX selected `p0t13.trx`, `LastWriteTimeUtc` `2026-09-08T04:21:12.8174136Z`; `failed` 0; `total` minus `executed` is 0.

BASELINE_FIRSTPARTY_LINES_COVERED: 113361
BASELINE_FIRSTPARTY_LINES_VALID: 134023
BASELINE_FIRSTPARTY_BRANCHES_COVERED: 26880
BASELINE_FIRSTPARTY_BRANCHES_VALID: 33880
BASELINE_FIRSTPARTY_LINE_PCT: 84.58
BASELINE_FIRSTPARTY_BRANCH_PCT: 79.34

Per-package breakdown, same selection:

| Package | Lines covered | Lines valid | Line % | Branches covered | Branches valid | Branch % |
|---|---|---|---|---|---|---|
| QuickFiler | 20549 | 25572 | 80.36 | 4892 | 6330 | 77.28 |
| UtilitiesCS | 79008 | 88958 | 88.81 | 18652 | 22414 | 83.22 |
| TaskVisualization | 2899 | 3230 | 89.75 | 666 | 800 | 83.25 |
| SVGControl | 1757 | 3712 | 47.33 | 600 | 1276 | 47.02 |
| ToDoModel | 2193 | 3819 | 57.42 | 496 | 1016 | 48.82 |
| Tags | 1428 | 1540 | 92.73 | 348 | 380 | 91.58 |
| TaskMaster | 4927 | 6564 | 75.06 | 1038 | 1460 | 71.10 |
| TaskTree | 592 | 620 | 95.48 | 188 | 204 | 92.16 |
| VBFunctions | 8 | 8 | 100.00 | 0 | 0 | 0.00 |

These are locally-filtered nine-assembly figures, not CI figures. `/EnableCodeCoverage` was not passed: `scripts/vscode/TaskMaster.cli.runsettings` carries no data collector, and `scripts/vscode/Invoke-MSTestWithCoverage.ps1:19-26` records that the omission is deliberate because the outer `dotnet-coverage` instrumentation and the built-in Code Coverage collector conflict.
