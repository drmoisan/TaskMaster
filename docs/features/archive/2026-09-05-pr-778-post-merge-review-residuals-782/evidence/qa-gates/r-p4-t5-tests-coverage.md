# [P4-T5] Final QC step 5 — full nine-assembly test run with coverage

Timestamp: 2026-09-06T01-54

Command:

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"

$derived = 'coverage\782-effective-coverage.config'
[xml]$cfg = Get-Content -LiteralPath 'coverage.config'
$excl = $cfg.Configuration.CodeCoverage.ModulePaths.Exclude
$node = $cfg.CreateElement('ModulePath'); $node.InnerText = '.*\.Test\.dll$'
$null = $excl.AppendChild($node); $cfg.Save((Join-Path (Get-Location) $derived))

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe |
    Select-Object -First 1

dotnet-coverage collect --output coverage\782-r1-final.cobertura.xml --output-format cobertura `
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
    '/ResultsDirectory:TestResults\782-r1-final' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

Both switches that carry a semicolon or an ampersand are written in single quotes, so PowerShell does
not truncate `'/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None'` at its first semicolon and
does not read the `/TestCaseFilter:` ampersands as operators. `/InIsolation` is mandatory.
`/EnableCodeCoverage` is not passed; `dotnet-coverage` performs the instrumentation and the two
collectors conflict.

EXIT_CODE: 0

Output Summary:

The counts below are read from the TRX `ResultSummary/Counters` element in
`TestResults\782-r1-final`, which contains exactly one `.trx` file and records `outcome="Completed"`
with `executed="7000"`, `error="0"`, `timeout="0"`, `aborted="0"`, `inconclusive="0"`, and
`notExecuted="0"`.

```text
Total tests: 7000
Passed: 7000
Failed: 0
```

FINAL-LINES-COVERED: 112351
FINAL-LINES-VALID: 132961
FINAL-BRANCHES-COVERED: 26498
FINAL-BRANCHES-VALID: 33480

**These are locally-filtered figures and not CI figures.** The `/TestCaseFilter` expression excludes
`TestCategory!=LiveOutlook` and the four shell-icon test classes
`HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`,
`HelperClasses.SysImageListHelperTests`, and `EmailIntelligence.OSBrowser_Tests`, which issue
`SHGetFileInfo` with `SHGFI_ICON` and stall process-wide on this workstation. The stall reproduces
against `origin/main`, so it is environmental and CI covers those four classes. A CI run reports a
larger total than 7000.

## The known flake did not fire

`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`,
tracked as issue #780, passed in this run. `Failed: 0` means no re-run was required, and this
artifact records a single run.

## Aggregation method

The four first-party counters are aggregated from `coverage\782-r1-final.cobertura.xml` with the same
pinned all-descendant `.//line` selection over the same nine-name first-party allowlist that
[P0-T10] used, so the two sides are produced by one collector, one configuration, one selection, and
one filter. The printed line, verbatim:

```text
LINES_COVERED=112351 LINES_VALID=132961 BRANCHES_COVERED=26498 BRANCHES_VALID=33480
```

`coverage\782-r1-final.cobertura.xml` is git-ignored by `.gitignore:144` and is a local artifact. The
TRX under `TestResults\782-r1-final` is git-ignored by `.gitignore:39`. Neither is staged.

[P4-T6] performs the comparison against [P0-T10].
