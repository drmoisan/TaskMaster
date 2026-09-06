# Baseline — Coverage (P0-T7, re-recorded under SD23)

SUPERSEDED BASELINE RE-RECORDED: SD23

RE-ANCHORED BASE: 736c2cf2

Timestamp: 2026-09-05T21-59

## Why the earlier figures are superseded

An external actor rebased the feature branch from `a007f72e` onto `origin/main` at `77c6d314`
during execution. Every prior commit received a new SHA. The base commit the superseded record was
taken at, `b95a5252`, is orphaned and is no longer an ancestor of HEAD, so the figures it carried
describe a tree that is no longer this branch's baseline.

The superseded first-party figures were line **112359/132967 = 84.50%** and branch
**26496/33480 = 79.14%**. The re-measured figures are line **112355/132967 = 84.50%** and branch
**26500/33480 = 79.15%**. The denominator is unchanged by SD23; only the covered counters moved.

## Measurement method and measuring party

This gate was measured by the **orchestrator, not the executor**, at the re-anchored base commit
`736c2cf2`, by the temporary-restore method: the orchestrator restored the six Write Set source
files Phase 1 has changed so far — `UtilitiesCS/Threading/UiThread.cs`,
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, `UtilitiesCS/Threading/ProgressTracker.cs`,
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`,
and `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — to their `pre-782-base` content with
`git checkout pre-782-base -- <those six paths>`, ran the four gates, restored those files to HEAD
in a `finally` block, and left the worktree clean and at HEAD afterwards.

The executor did **not** re-run the coverage collection for this task, and this artifact does not
present the figures as an executor run.

Command (the orchestrator's command):

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

The derived configuration is the repo-root `coverage.config` with one
`<ModulePath>.*\.Test\.dll$</ModulePath>` appended to `/Configuration/CodeCoverage/ModulePaths/Exclude`.
The `/Blame:` switch is written in single quotes so PowerShell does not truncate it at the first
semicolon. `/EnableCodeCoverage` is not passed; `dotnet-coverage` performs the instrumentation and
the two collectors conflict.

EXIT_CODE: 0

That is the exit code the **orchestrator** observed, not an exit code the executor observed.

## Counting method (SD22, load-bearing, unchanged by SD23; P7-T5 and P7-T7 must reproduce it)

The `<package>` elements in this Cobertura document carry `line-rate` and `branch-rate` attributes
but carry **no** `lines-covered`, `lines-valid`, `branches-covered`, or `branches-valid` attributes,
so those four figures are aggregated from `<line>` elements rather than read off the package, and
the denominator depends entirely on the selection used.

**The selection is the all-descendant `.//line` selection over each first-party `<package>`, and
only that one.** It reproduces the tabled first-party `lines-valid` of **132967** exactly, in the
superseded run and in the re-measured run alike. The fact that the same selection reproduces 132967
across both runs is itself the evidence that one selection was used both times.

Two narrower selections are **rejected by name and by figure** so a later reader cannot substitute
one:

| Rejected selection | Figure |
|---|---|
| `classes/class/lines/line` | 65899 |
| `classes/class/methods/method/lines/line` | 67068 |

**Both of those figures were measured against the superseded baseline document and were not
re-derived against the re-measured document.** They are recorded because the selections they name
are what must not be substituted, not because their numeric values are current.

The all-descendant selection counts a line both at class level and inside its method, so the
denominator is roughly twice the deduped one. That doubling is a property of the baseline method and
is preserved deliberately: the only requirement on it is that the baseline and the Phase 7 figure be
produced by one method and therefore be comparable.

A `<line>` counts as covered when its `hits` attribute is greater than zero. Branch figures are
summed from the `(numerator/denominator)` pair inside each `condition-coverage` attribute over the
same all-descendant line set.

The first-party allowlist is the nine production assembly names: `Tags`, `ToDoModel`,
`TaskVisualization`, `UtilitiesCS`, `QuickFiler`, `TaskTree`, `TaskMaster`, `SVGControl`,
`VBFunctions`. The document also contains vendored packages — `log4net`, `Mono.Reflection`,
`Microsoft.IO.RecyclableMemoryStream`, `System.Linq.Async`, `System.Interactive` — which are
excluded from the first-party figures. The repo-root `coverage.config` does not exclude vendored
assemblies, so this allowlist is what performs that stripping.

Output Summary:

### First-party figures, re-measured at `736c2cf2` (comparable to policy)

| Figure | Value |
|---|---|
| `lines-covered` | 112355 |
| `lines-valid` | 132967 |
| line percentage | 84.50% |
| `branches-covered` | 26500 |
| `branches-valid` | 33480 |
| branch percentage | 79.15% |

Aggregated by the all-descendant `.//line` selection pinned above, over only the `<package>`
elements whose name matches one of the nine first-party allowlist assembly names.

**Only the first-party figure is comparable to policy.** The root all-modules figure includes
vendored assemblies that this repository does not own and cannot be held to the coverage floor.

The `lines-valid` of **132967** is recorded here so the Phase 7 comparison in P7-T7 can test
comparability between the two runs' denominators.

The re-measurement supplied totals only. No per-package first-party breakdown was taken at the
re-anchored base, so none is recorded here; the superseded per-package table is not carried forward,
because its rows sum to the superseded totals rather than to the re-measured ones. P7-T6 derives its
per-package rows from the Phase 7 Cobertura document directly.

### Superseded first-party figures, retained for audit and not current

| Figure | Superseded value | Re-measured value |
|---|---|---|
| `lines-covered` | 112359 | 112355 |
| `lines-valid` | 132967 | 132967 |
| line percentage | 84.50% | 84.50% |
| `branches-covered` | 26496 | 26500 |
| `branches-valid` | 33480 | 33480 |
| branch percentage | 79.14% | 79.15% |

Those superseded figures were measured at the orphaned base `b95a5252` and are superseded for the
reason stated at the head of this artifact. A Phase 7 comparison that reads either 112359 or 26496
as its baseline side is invalid.

### Root all-modules figures — not re-measured, and not carried forward as a baseline

**The re-measurement supplied no root all-modules figure.** The superseded run recorded root line
**70.34%** and root branch **59.18%**. Those two values are recorded here as **superseded** and are
explicitly **not** carried forward as though they had been re-measured at the re-anchored base.

No task in this plan consumes a root all-modules baseline: P7-T5 records the root figures from its
own run, and P7-T7 compares first-party figures only. The root element's counts are deduped, which is
why a root `lines-valid` is smaller than the first-party all-descendant `lines-valid` of 132967 even
though the first-party set is a subset of the modules; the two are produced by different counting
methods and must not be compared with each other.

### Test run

The collected baseline run is the same nine-assembly, locally-filtered run recorded in
`evidence/baseline/p0-t6-vstest.md`, which re-records `Total tests: 6997`, `Passed: 6997`,
`Failed: 0`. These are locally-filtered figures with the four shell-icon classes excluded, not CI
figures.
