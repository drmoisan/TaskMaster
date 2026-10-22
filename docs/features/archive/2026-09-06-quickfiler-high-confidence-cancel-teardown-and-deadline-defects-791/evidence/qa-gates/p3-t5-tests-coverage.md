# [P3-T5] Final test run with coverage

Timestamp: 2026-09-06T15-06

Command:

```
dotnet-coverage collect --output artifacts\csharp\coverage.xml --output-format cobertura --settings coverage\791-effective-coverage.config -- $vstest <nine test assemblies> '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p3-t5' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

`$vstest` was re-bound inside this command block by the two R10 resolution lines; the resolved value
reduced per R3 is `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
The nine assemblies, the runsettings, the isolation and blame switches, the filter, the collector and
the settings file are all identical to [P0-T11], so the two sides of the [P3-T8] comparison are
produced by one collector, one configuration, one selection and one filter (D13, D14).

EXIT_CODE: 0

FINAL-TOTAL-TESTS: 7023
FINAL-FAILED-TESTS: 0

Output Summary: `A total of 9 test files matched the specified pattern.` then
`Test Run Successful. Total tests: 7023, Passed: 7023, Total time: 48.6519 Seconds.` and
`Code coverage results: artifacts\csharp\coverage.xml.`

`artifacts/csharp/coverage.xml` exists on disk (verified by `Test-Path`, which returned `True`),
which is AC4's substantive requirement. `artifacts/` is git-ignored at `.gitignore` line 57, so the
document is a local tool output rather than committed evidence, and the acceptance is on-disk
existence and the recorded counters rather than `git ls-files`.
`.claude/hooks/enforce-evidence-locations.ps1` lines 22-26 name `artifacts/csharp/` as an explicitly
permitted path and it is absent from the forbidden prefix list at lines 64-74.

## Test-count comparison against [P0-T11]

| Measure | Baseline [P0-T11] | This run | Delta |
|---|---|---|---|
| Total | 7000 | 7023 | +23 |
| Passed | 7000 | 7023 | +23 |
| Failed | 0 | 0 | 0 |

The +23 is exactly the tests this plan added, all in `QuickFiler.Test`, and matches the +23 [P2-T15]
observed on that assembly alone.

## Aggregated first-party counters

Aggregated from `artifacts/csharp/coverage.xml` by the same pinned all-descendant `.//line`
selection over the same nine first-party package names [P0-T11] used:

```powershell
$CoberturaPath = 'artifacts\csharp\coverage.xml'
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
LINES_COVERED=112551 LINES_VALID=133187 BRANCHES_COVERED=26584 BRANCHES_VALID=33568
```

FINAL-LINES-COVERED: 112551
FINAL-LINES-VALID: 133187
FINAL-BRANCHES-COVERED: 26584
FINAL-BRANCHES-VALID: 33568
FINAL-LINE-PERCENT: 84.51
FINAL-BRANCH-PERCENT: 79.19

All four `FINAL-` counter lines are numeric. [P3-T8] performs the comparison against the [P0-T11]
baseline counters, including the `lines-valid` comparability precondition.

## Collector substitution (D13)

AC4's toolchain step 4 names `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`.
`/EnableCodeCoverage` writes a binary `.coverage` file, not the Cobertura XML AC4 also requires at
`artifacts/csharp/coverage.xml`, and the two collectors conflict when combined. The run therefore
uses `dotnet-coverage collect --output-format cobertura -- <vstest> ...`, wrapping the same
`vstest.console.exe` with the same assemblies and switches. The substantive requirement — a
Cobertura document at that path, produced by running the full suite — is met. This substitution is
recorded as a deviation by [P3-T17] and cited in the AC4 check-off.

This is step 4 of the uninterrupted toolchain pass; [P3-T6] records the closure.
