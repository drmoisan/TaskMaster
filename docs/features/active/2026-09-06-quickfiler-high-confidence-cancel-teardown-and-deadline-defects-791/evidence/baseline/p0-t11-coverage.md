# [P0-T11] Baseline coverage over the nine first-party test assemblies

Timestamp: 2026-09-06T14-28

Command:

```
dotnet-coverage collect --output coverage\791-baseline.cobertura.xml --output-format cobertura --settings coverage\791-effective-coverage.config -- $vstest <nine test assemblies> '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p0-t11' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

The nine assemblies are named explicitly (D15): `QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`,
`TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`
and `VBFunctions.Test`, each as `<project>\bin\Debug\<project>.dll`. A path never enumerated cannot
be loaded, so no worktree under a `.claude` segment can enter the run.

`$vstest` was re-bound inside this command block by the two R10 resolution lines; the resolved value
reduced per R3 is `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

EXIT_CODE: 0

BASELINE-TOTAL-TESTS: 7000
BASELINE-FAILED-TESTS: 0

Output Summary: `A total of 9 test files matched the specified pattern.` then
`Test Run Successful. Total tests: 7000, Passed: 7000, Total time: 48.9533 Seconds.` and
`Code coverage results: coverage\791-baseline.cobertura.xml.` The four shell-icon exclusion clauses
in the filter are the known local hang set in `UtilitiesCS.Test`; they are excluded here so the run
terminates, and CI covers them.

## Derived coverage config

`coverage\791-effective-coverage.config` is `coverage.config` with one appended
`<ModulePath>` exclusion for `.*\.Test\.dll$`, because `coverage.config` carries no such entry
and the test assemblies must not enter the denominator. The same derived file is reused by
[P3-T5], so both sides of the comparison are produced by one collector, one configuration, one
selection and one filter.

## Aggregated first-party counters

Aggregated from `coverage\791-baseline.cobertura.xml` by the pinned all-descendant `.//line`
selection over each first-party `<package>`. The document contains fourteen packages; the five
non-first-party ones (`log4net`, `Mono.Reflection`, `Microsoft.IO.RecyclableMemoryStream`,
`System.Linq.Async`, `System.Interactive`) are excluded by the first-party name list.

```powershell
$CoberturaPath = 'coverage\791-baseline.cobertura.xml'
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

printed, verbatim:

```text
LINES_COVERED=112355 LINES_VALID=132961 BRANCHES_COVERED=26496 BRANCHES_VALID=33480
```

BASELINE-LINES-COVERED: 112355
BASELINE-LINES-VALID: 132961
BASELINE-BRANCHES-COVERED: 26496
BASELINE-BRANCHES-VALID: 33480
BASELINE-LINE-PERCENT: 84.50
BASELINE-BRANCH-PERCENT: 79.14

BASELINE_FLOOR: MET

The 84.50 percent first-party line rate is at or above the CLAUDE.md UT2 80 percent floor, so the
floor is met at `BASE-SHA`. Per the task, the plan continues regardless of this determination; a
pre-existing repository floor never halts it.

## Status of the collected document

`coverage\791-baseline.cobertura.xml` is git-ignored by `.gitignore` line 144 (`coverage/*`). It is
a local output of this run, not committed evidence. The TRX under `TestResults\791-p0-t11` is
covered by `.gitignore` line 39. No TRX content is reproduced here (R3); only the parsed totals are
recorded.
