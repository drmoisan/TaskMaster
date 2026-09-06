# Baseline — Coverage (P0-T7)

Timestamp: 2026-09-05T19-33

Command:

```powershell
$derived = 'coverage\782-effective-coverage.config'
[xml]$cfg = Get-Content -LiteralPath 'coverage.config'
$excl = $cfg.Configuration.CodeCoverage.ModulePaths.Exclude
$node = $cfg.CreateElement('ModulePath'); $node.InnerText = '.*\.Test\.dll$'
$null = $excl.AppendChild($node); $cfg.Save((Join-Path (Get-Location) $derived))

dotnet-coverage collect --output coverage\782-p0-baseline.cobertura.xml --output-format cobertura `
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
    '/ResultsDirectory:TestResults\782-p0-coverage' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

The `/Blame:` switch is written in single quotes so PowerShell does not truncate it at the first
semicolon. `/EnableCodeCoverage` is not passed; `dotnet-coverage` performs the instrumentation and
the two collectors conflict.

EXIT_CODE: 0

## Counting method (load-bearing; P7-T5 and P7-T7 must reproduce it)

The `<package>` elements in this Cobertura document carry `line-rate` and `branch-rate` attributes
but carry **no** `lines-covered`, `lines-valid`, `branches-covered`, or `branches-valid` attributes,
so those four figures are aggregated from `<line>` elements rather than read off the package.

The aggregation is over **every `<line>` descendant** of a first-party `<package>`, selected by the
XPath `.//line`. That is the method that reproduces the plan's tabled `lines-valid` of 132967
exactly. Two narrower selections were measured against the same document and do not reproduce it:
`classes/class/lines/line` yields 65899 and `classes/class/methods/method/lines/line` yields 67068.
The all-descendant selection counts a line both at class level and inside its method, so the
denominator is roughly twice the deduped one; that is a property of the baseline's counting method
and is preserved deliberately so the Phase 7 comparison is like-for-like.

Line coverage counts a `<line>` as covered when its `hits` attribute is greater than zero. Branch
figures are summed from the `(numerator/denominator)` pair inside each `condition-coverage`
attribute over the same all-descendant line set.

The first-party allowlist is the nine production assembly names: `Tags`, `ToDoModel`,
`TaskVisualization`, `UtilitiesCS`, `QuickFiler`, `TaskTree`, `TaskMaster`, `SVGControl`,
`VBFunctions`. The document also contains the packages `log4net`, `Mono.Reflection`,
`Microsoft.IO.RecyclableMemoryStream`, `System.Linq.Async`, and `System.Interactive`, which are
vendored and are excluded from the first-party figures. The repo-root `coverage.config` does not
exclude vendored assemblies, so this allowlist is what performs that stripping.

Output Summary:

### First-party figures (comparable to policy)

| Figure | Value |
|---|---|
| `lines-covered` | 112359 |
| `lines-valid` | 132967 |
| line percentage | 84.50% |
| `branches-covered` | 26496 |
| `branches-valid` | 33480 |
| branch percentage | 79.14% |

Against the plan's tabled baseline of line 112357/132967 = 84.50% and branch 26496/33480 = 79.14%:
`lines-valid`, `branches-covered`, and `branches-valid` reproduce exactly; `lines-covered` is
112359 against a tabled 112357, a difference of two covered lines, which moves the line percentage
by 0.0015 percentage points. Both percentages are therefore within the 0.05-percentage-point
tolerance the acceptance condition allows, and no deviation record is required. The Phase 7 gate
compares against the observed figures recorded here.

Per-package first-party breakdown:

| Package | lines covered / valid | branches covered / valid |
|---|---|---|
| QuickFiler | 20135 / 25134 | 4728 / 6154 |
| UtilitiesCS | 78546 / 88480 | 18458 / 22222 |
| TaskVisualization | 2899 / 3230 | 666 / 800 |
| SVGControl | 1757 / 3712 | 600 / 1276 |
| ToDoModel | 2193 / 3819 | 496 / 1016 |
| Tags | 1428 / 1540 | 348 / 380 |
| TaskMaster | 4801 / 6424 | 1012 / 1428 |
| TaskTree | 592 / 620 | 188 / 204 |
| VBFunctions | 8 / 8 | 0 / 0 |
| **Total** | **112359 / 132967** | **26496 / 33480** |

### Root all-modules figures (not comparable to policy)

Read directly from the document root element, which does carry the four count attributes:

| Figure | Value |
|---|---|
| `lines-covered` | 58429 |
| `lines-valid` | 83071 |
| line percentage | 70.34% |
| `branches-covered` | 14319 |
| `branches-valid` | 24195 |
| branch percentage | 59.18% |

The plan's Environment Facts section states the raw all-modules figure as line 70.42% / branch
59.19%; the observed 70.34% / 59.18% differ from those by 0.08 and 0.01 percentage points. No
acceptance condition reads the root figures beyond requiring that they be recorded, and they are
recorded here. The root element's counts are deduped, which is why `lines-valid` at the root (83071)
is smaller than the first-party all-descendant `lines-valid` (132967) even though the first-party set
is a subset of the modules; the two figures are produced by different counting methods and must not
be compared with each other.

**Only the first-party figure is comparable to policy.** The root all-modules figure includes
vendored assemblies that this repository does not own and cannot be held to the coverage floor.

### Test run

The collected run reported `Test Run Successful.`, `Total tests: 6992`, `Passed: 6992`, which are
locally-filtered figures over the nine assemblies with the four shell-icon classes excluded, not CI
figures.
