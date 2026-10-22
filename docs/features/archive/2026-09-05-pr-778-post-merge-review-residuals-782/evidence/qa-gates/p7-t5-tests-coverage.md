# QA Gate — Final Toolchain Pass, Step 5: Tests with Coverage (P7-T5)

Timestamp: 2026-09-05T23-10

Command:

```powershell
# Derived coverage configuration, built exactly as in P0-T7
$derived = 'coverage\782-effective-coverage.config'
[xml]$cfg = Get-Content -LiteralPath 'coverage.config'
$excl = $cfg.Configuration.CodeCoverage.ModulePaths.Exclude
$node = $cfg.CreateElement('ModulePath'); $node.InnerText = '.*\.Test\.dll$'
$null = $excl.AppendChild($node); $cfg.Save((Join-Path (Get-Location) $derived))

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe |
    Select-Object -First 1

dotnet-coverage collect --output coverage\782-p7-final.cobertura.xml --output-format cobertura `
    --settings coverage\782-effective-coverage.config -- $vstest `
    QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    SVGControl.Test\bin\Debug\SVGControl.Test.dll `
    Tags.Test\bin\Debug\Tags.Test.dll `
    TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
    TaskTree.Test\bin\Debug\TaskTree.Test.dll `
    TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll `
    ToDoModel.Test\bin\Debug\ToDoModel.Test.dll `
    UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
    VBFunctions.Test\bin\Debug\VBFunctions.Test.dll `
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
    '/ResultsDirectory:TestResults\782-p7' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

The derived configuration is the repo-root `coverage.config` with one
`<ModulePath>.*\.Test\.dll$</ModulePath>` appended to
`/Configuration/CodeCoverage/ModulePaths/Exclude`. The `/Blame:` switch is written in single quotes
so PowerShell does not truncate it at the first semicolon. `/EnableCodeCoverage` is not passed;
`dotnet-coverage` performs the instrumentation and the two collectors conflict (SD17).

EXIT_CODE: 0

Output Summary:

## Test run

```text
Test Run Successful.
Total tests: 7000
     Passed: 7000
 Total time: 44.9116 Seconds
```

TRX `ResultSummary/Counters`: `total=7000 passed=7000 failed=0 notExecuted=0`.

`Failed: 0` and `Skipped: 0` — the TRX counters supply both directly, and vstest prints neither line
when the corresponding count is zero.

These are **locally-filtered nine-assembly figures**, not CI figures. The four shell-icon classes
excluded by the `/TestCaseFilter` stall process-wide on this workstation; the stall reproduces
against `origin/main`, so it is environmental and CI covers those classes.

`evidence/baseline/p0-t6-vstest.md` records `BASELINE_TOTAL_TESTS: 6997`. This delivery adds three
tests and removes none, so the expected minimum is 7000. The observed total is exactly 7000.

## Coverage counting method (SD22, pinned, reproduces P0-T7 exactly)

Cobertura `<package>` elements in this document carry `line-rate` and `branch-rate` attributes but
carry **no** `lines-covered`, `lines-valid`, `branches-covered`, or `branches-valid` attributes, so
those four figures are aggregated from `<line>` elements and the denominator depends entirely on the
selection used.

**The selection used is the all-descendant `.//line` selection over each first-party `<package>`,
and only that one.** A `<line>` counts as covered when its `hits` attribute is greater than zero.
Branch figures are summed from the `(numerator/denominator)` pair inside each `condition-coverage`
attribute over the same all-descendant line set.

Two narrower selections are rejected by name and by figure, and are re-derived here against **this**
document rather than carried forward from the baseline artifact:

| Rejected selection | Figure against this document | Figure recorded against the superseded baseline document |
|---|---|---|
| `classes/class/lines/line` | 65896 | 65899 |
| `classes/class/methods/method/lines/line` | 67065 | 67068 |

A figure produced by either of those is not comparable to the baseline and must not be substituted
here.

The first-party allowlist is the nine production assembly names: `Tags`, `ToDoModel`,
`TaskVisualization`, `UtilitiesCS`, `QuickFiler`, `TaskTree`, `TaskMaster`, `SVGControl`,
`VBFunctions`. Vendored packages in the document are excluded from the first-party figures.

## First-party figures (comparable to policy)

| Figure | Value |
|---|---|
| `lines-covered` | 112363 |
| `lines-valid` | 132961 |
| line percentage | 84.51% |
| `branches-covered` | 26500 |
| `branches-valid` | 33480 |
| branch percentage | 79.15% |

## Root all-modules figures

| Figure | Value |
|---|---|
| root `lines-covered` | 58433 |
| root `lines-valid` | 83068 |
| root line percentage | 70.34% |
| root `branches-covered` | 14323 |
| root `branches-valid` | 24195 |
| root branch percentage | 59.20% |

Only the first-party figure is comparable to policy. The root figure includes vendored assemblies
this repository does not own. The root element's counts are deduped, which is why the root
`lines-valid` is smaller than the first-party all-descendant `lines-valid` even though the
first-party set is a subset of the modules; the two are produced by different counting methods and
must not be compared with each other.

## First-party per-package breakdown

| Package | Lines covered | Lines valid | Branches covered | Branches valid |
|---|---|---|---|---|
| `QuickFiler` | 20135 | 25134 | 4728 | 6154 |
| `UtilitiesCS` | 78550 | 88474 | 18462 | 22222 |
| `TaskVisualization` | 2899 | 3230 | 666 | 800 |
| `SVGControl` | 1757 | 3712 | 600 | 1276 |
| `ToDoModel` | 2193 | 3819 | 496 | 1016 |
| `Tags` | 1428 | 1540 | 348 | 380 |
| `TaskMaster` | 4801 | 6424 | 1012 | 1428 |
| `TaskTree` | 592 | 620 | 188 | 204 |
| `VBFunctions` | 8 | 8 | 0 | 0 |
| **Total** | **112363** | **132961** | **26500** | **33480** |

## The five named tests, read from the TRX

| Fully-qualified name | Outcome | Duration |
|---|---|---|
| `UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` | Passed | 0.4 ms |
| `UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance` | Passed | 1.8 ms |
| `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` | Passed | 1.0 ms |
| `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_ProductionFallbackWithoutDispatcher_ThrowsNamingInit` | Passed | 1.0 ms |
| `UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests.InitializeAsync_WhenDispatcherNotCaptured_ThrowsInvalidOperationException` | Passed | 1.3 ms |

These five outcomes are recorded here so that later tasks can cite this artifact rather than the
results tree, which P8-T20 deletes.

The TRX was written to `TestResults\782-p7\` under a filename generated by vstest from the local
account and machine names; that filename is deliberately not reproduced here, and no absolute host
path appears in this artifact.
