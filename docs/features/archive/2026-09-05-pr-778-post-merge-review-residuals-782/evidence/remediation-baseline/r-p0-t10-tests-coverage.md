# [P0-T10] Baseline — full nine-assembly test run with coverage

Timestamp: 2026-09-06T01-34

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

dotnet-coverage collect --output coverage\782-r1-baseline.cobertura.xml --output-format cobertura `
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
    '/ResultsDirectory:TestResults\782-r1-baseline' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

Both semicolon-bearing switches are written in single quotes, so PowerShell does not truncate them at
the first semicolon: `'/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None'` and, although it
carries no semicolon, `'/TestCaseFilter:...'` is quoted for the same reason its ampersands would
otherwise be read by the shell. `/InIsolation` is mandatory; without it the app.config binding
redirects are not loaded and roughly 1700 tests fail with empty messages and sub-millisecond
durations. `/EnableCodeCoverage` is never passed: `dotnet-coverage` performs the instrumentation, and
the two collectors conflict.

The nine assembly paths are given explicitly. That is how the requirement that assembly discovery
exclude any path containing a `.claude` worktree segment is satisfied — a path that is never
enumerated cannot be loaded.

EXIT_CODE: 0

Output Summary:

The counts below are read from the TRX `ResultSummary/Counters` element in
`TestResults\782-r1-baseline`, which contains exactly one `.trx` file. The element records
`outcome="Completed"` with `executed="7000"`, `error="0"`, `timeout="0"`, `aborted="0"`,
`inconclusive="0"`, and `notExecuted="0"`.

```text
Total tests: 7000
Passed: 7000
Failed: 0
```

BASELINE-LINES-COVERED: 112351
BASELINE-LINES-VALID: 132961
BASELINE-BRANCHES-COVERED: 26498
BASELINE-BRANCHES-VALID: 33480

**These are locally-filtered figures and not CI figures.** The `/TestCaseFilter` expression excludes
`TestCategory!=LiveOutlook` and the four shell-icon test classes
`HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`,
`HelperClasses.SysImageListHelperTests`, and `EmailIntelligence.OSBrowser_Tests`, which issue
`SHGetFileInfo` with `SHGFI_ICON` and stall process-wide on this workstation. That stall reproduces
against `origin/main`, so it is environmental, and CI covers those four classes. A CI run therefore
reports a larger total than 7000.

## Reproducing the four counters

The four first-party counters are aggregated from `coverage\782-r1-baseline.cobertura.xml` by the
pinned all-descendant `.//line` selection over each first-party `<package>`, which is the selection
`evidence/baseline/p0-t7-coverage.md` pins as load-bearing under SD22. A reader reproduces them by
running:

```powershell
$CoberturaPath = 'coverage\782-r1-baseline.cobertura.xml'
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath $CoberturaPath).Path)
$firstParty = @('Tags','ToDoModel','TaskVisualization','UtilitiesCS','QuickFiler','TaskTree','TaskMaster','SVGControl','VBFunctions')
$lc = 0; $lv = 0; $bc = 0; $bv = 0
foreach ($pkg in $doc.SelectNodes('/coverage/packages/package')) {
    if ($firstParty -notcontains $pkg.GetAttribute('name')) { continue }
    foreach ($ln in $pkg.SelectNodes('.//line')) {
        $lv++
        $h = $ln.GetAttribute('hits')
        if ($h -and [int]$h -gt 0) { $lc++ }
        $cc = $ln.GetAttribute('condition-coverage')
        if ($cc -and $cc -match '\((\d+)/(\d+)\)') { $bc += [int]$Matches[1]; $bv += [int]$Matches[2] }
    }
}
"LINES_COVERED=$lc LINES_VALID=$lv BRANCHES_COVERED=$bc BRANCHES_VALID=$bv"
```

which prints, verbatim:

```text
LINES_COVERED=112351 LINES_VALID=132961 BRANCHES_COVERED=26498 BRANCHES_VALID=33480
```

## Status of the collected document

`coverage\782-r1-baseline.cobertura.xml` is git-ignored by `.gitignore:144` (`coverage/*`). It is a
local artifact of this run and is neither staged nor cited as committed evidence. The same holds for
the TRX under `TestResults\782-r1-baseline`, which `.gitignore:39` covers.

## Relation to the delivery's own Phase 0 baseline

This is the remediation's own baseline, collected at the delivered `HEAD`, not a re-collection of the
delivery's `evidence/baseline/p0-t7-coverage.md` figures, which were taken at the re-anchored base
`736c2cf2` with the six Write Set files temporarily restored. The two are not comparable and are not
compared: the only comparison this plan makes is between this artifact's four counters and the four
that [P4-T5] records after the two test-file assertion edits, and [P4-T6] performs it.
