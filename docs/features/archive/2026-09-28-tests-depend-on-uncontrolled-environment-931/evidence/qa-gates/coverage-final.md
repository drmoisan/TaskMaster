# Coverage Final (P4-T7): repository-wide test and coverage run after the fix

Timestamp: 2026-09-29T09-39
Command:
- Route: COVERAGE-ROUTE: DIRECT (fixed by P0-T9, STALL-PROBE: REPRODUCES)
- Run payload (CMD-COVERAGE-DIRECT, STAGE final): dotnet-coverage collect --output coverage\final-931.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-931.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\931\final" "/Logger:trx;LogFileName=final-931.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (vstest.console.exe resolved through vswhere; effective settings derived with ConvertTo-DerivedCoverageSettingsXml from coverage.config)
- Post payload (CMD-COVERAGE-POST, STAGE final, RAW True): Get-TrxRunSummary / Format-TrxRunSummary, ConvertTo-KoverageCoberturaXml, Assert-CoberturaLineCoverageThreshold, Assert-CoberturaBranchCoverageThreshold, Get-CoberturaFirstPartyCoverageReport, ConvertTo-JacocoPackageProjection, Assert-JacocoProjectionReconciliation (the runner's own helpers, dot-sourced)
- Filter applied by the route: the LiveOutlook category exclusion plus the four-class shell-icon exclusion shown above
EXIT_CODE: 0
MEASUREMENT: 1

Output Summary:
- COVERAGE-ROUTE: DIRECT
- RAW: True
- COLLECT_EXIT_CODE: 0
- ASSEMBLY_COUNT: 9
- ASSEMBLY: \QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
- ASSEMBLY: \SVGControl.Test\bin\Debug\SVGControl.Test.dll
- ASSEMBLY: \Tags.Test\bin\Debug\Tags.Test.dll
- ASSEMBLY: \TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
- ASSEMBLY: \TaskTree.Test\bin\Debug\TaskTree.Test.dll
- ASSEMBLY: \TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
- ASSEMBLY: \ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
- ASSEMBLY: \UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
- ASSEMBLY: \VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
- SEQUENCE_FILES: 0
- TRX_PRESENT: True
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56084/65737 (85.32%), branches 13595/17052 (79.73%)
- FAILED-SET: (empty; no test failed)
- NEWLY-FAILING: NONE (the P0-T11 FAILED-SET was also empty)
- Branch decided: (a) exit 0 with FAILED-SET empty and both floors met; the task completes.

SUMMARY-BEGIN
Test run outcome: Completed
Total 7320, executed 7320, passed 7320, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2294" covered="10460" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4608" covered="38816" />
    <counter type="BRANCH" missed="1858" covered="9411" />
  </package>
  <package name="TaskVisualization">
    <counter type="LINE" missed="143" covered="1426" />
    <counter type="BRANCH" missed="67" covered="333" />
  </package>
  <package name="SVGControl">
    <counter type="LINE" missed="977" covered="877" />
    <counter type="BRANCH" missed="338" covered="300" />
  </package>
  <package name="ToDoModel">
    <counter type="LINE" missed="762" covered="1061" />
    <counter type="BRANCH" missed="260" covered="248" />
  </package>
  <package name="Tags">
    <counter type="LINE" missed="56" covered="702" />
    <counter type="BRANCH" missed="16" covered="174" />
  </package>
  <package name="TaskMaster">
    <counter type="LINE" missed="802" covered="2443" />
    <counter type="BRANCH" missed="211" covered="517" />
  </package>
  <package name="TaskTree">
    <counter type="LINE" missed="11" covered="295" />
    <counter type="BRANCH" missed="8" covered="94" />
  </package>
  <package name="VBFunctions">
    <counter type="LINE" missed="0" covered="4" />
    <counter type="BRANCH" missed="0" covered="0" />
  </package>
</report>
PROJECTION-END

Acceptance: EXIT_CODE 0; SEQUENCE_FILES 0 (DIRECT); the projection contains the UtilitiesCS and QuickFiler package elements with LINE and BRANCH counters; the First-party coverage line is present; the summary block begins Test run outcome: and FAILED-SET is empty; NEWLY-FAILING NONE; LINE-FLOOR MET and BRANCH-FLOOR MET as printed by CMD-COVERAGE-POST; the artifact contains no absolute path. All eight hold. coverage\final-931.jacoco.xml remains on disk, git-ignored, for P4-T8. The raw Cobertura document and the TRX are not copied into the feature folder.

## Comparison of MEASUREMENT 1 against coverage-baseline.md (P4-T8, first comparison)

Command: CMD-PACKAGE-COMPARE (coverage\baseline-931.jacoco.xml against coverage\final-931.jacoco.xml as written by MEASUREMENT 1)

PACKAGE UtilitiesCS LINE baseline=38816/43424 rate=0.893884 final=38816/43424 rate=0.893884 NOT-LOWER=True
PACKAGE UtilitiesCS BRANCH baseline=9411/11269 rate=0.835123 final=9411/11269 rate=0.835123 NOT-LOWER=True
PACKAGE QuickFiler LINE baseline=10461/12754 rate=0.820213 final=10460/12754 rate=0.820135 NOT-LOWER=False
PACKAGE QuickFiler BRANCH baseline=2518/3217 rate=0.782717 final=2518/3217 rate=0.782717 NOT-LOWER=True

Result: one PACKAGE line reads NOT-LOWER=False (QuickFiler LINE, one covered line fewer over an identical denominator). Per P4-T8 and D-7, P4-T7 is run once more with the identical command as MEASUREMENT 2 and the comparison is repeated. No test failed in MEASUREMENT 1; this repeats the measurement, not a test.

## MEASUREMENT 2 (P4-T7 re-run with the identical command, per P4-T8)

Timestamp: 2026-09-29T09-42
Command: identical to MEASUREMENT 1 (CMD-COVERAGE-DIRECT with STAGE final, then CMD-COVERAGE-POST with STAGE final and RAW True); coverage\final-931.* documents overwritten by this measurement
EXIT_CODE: 0
MEASUREMENT: 2

- COVERAGE-ROUTE: DIRECT
- RAW: True
- COLLECT_EXIT_CODE: 0
- ASSEMBLY_COUNT: 9 (the same nine ASSEMBLY lines as MEASUREMENT 1)
- SEQUENCE_FILES: 0
- TRX_PRESENT: True
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56085/65737 (85.32%), branches 13595/17052 (79.73%)
- FAILED-SET: (empty; no test failed)
- NEWLY-FAILING: NONE
- Branch decided: (a).

SUMMARY-BEGIN
Test run outcome: Completed
Total 7320, executed 7320, passed 7320, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4608" covered="38816" />
    <counter type="BRANCH" missed="1858" covered="9411" />
  </package>
  <package name="TaskVisualization">
    <counter type="LINE" missed="143" covered="1426" />
    <counter type="BRANCH" missed="67" covered="333" />
  </package>
  <package name="SVGControl">
    <counter type="LINE" missed="977" covered="877" />
    <counter type="BRANCH" missed="338" covered="300" />
  </package>
  <package name="ToDoModel">
    <counter type="LINE" missed="762" covered="1061" />
    <counter type="BRANCH" missed="260" covered="248" />
  </package>
  <package name="Tags">
    <counter type="LINE" missed="56" covered="702" />
    <counter type="BRANCH" missed="16" covered="174" />
  </package>
  <package name="TaskMaster">
    <counter type="LINE" missed="802" covered="2443" />
    <counter type="BRANCH" missed="211" covered="517" />
  </package>
  <package name="TaskTree">
    <counter type="LINE" missed="11" covered="295" />
    <counter type="BRANCH" missed="8" covered="94" />
  </package>
  <package name="VBFunctions">
    <counter type="LINE" missed="0" covered="4" />
    <counter type="BRANCH" missed="0" covered="0" />
  </package>
</report>
PROJECTION-END

## Comparison against coverage-baseline.md

Command: CMD-PACKAGE-COMPARE (coverage\baseline-931.jacoco.xml against coverage\final-931.jacoco.xml as written by MEASUREMENT 2)

PACKAGE UtilitiesCS LINE baseline=38816/43424 rate=0.893884 final=38816/43424 rate=0.893884 NOT-LOWER=True
PACKAGE UtilitiesCS BRANCH baseline=9411/11269 rate=0.835123 final=9411/11269 rate=0.835123 NOT-LOWER=True
PACKAGE QuickFiler LINE baseline=10461/12754 rate=0.820213 final=10461/12754 rate=0.820213 NOT-LOWER=True
PACKAGE QuickFiler BRANCH baseline=2518/3217 rate=0.782717 final=2518/3217 rate=0.782717 NOT-LOWER=True

BASELINE-FIRST-PARTY: First-party coverage: lines 56085/65737 (85.32%), branches 13595/17052 (79.73%)
FINAL-FIRST-PARTY: First-party coverage: lines 56085/65737 (85.32%), branches 13595/17052 (79.73%)
COMPARABILITY: A (the two first-party lines denominators are both 65737, a difference of 0, within 1 percent of the baseline denominator)
CHANGED-PRODUCTION-LINES: 0 (the P2-T10 git show path list contains no production path, so every changed line of this item is test code outside the coverage denominator and the policy's no-regression-on-changed-lines clause has an empty subject)

Observation: MEASUREMENT 1 read the QuickFiler LINE counter one covered line lower (10460/12754) than the baseline and MEASUREMENT 2 with the identical command read it equal (10461/12754). No production file changed, so the one-line difference between two identical measurements is measurement variance of the collector, which D-7 anticipates.

Acceptance: no PACKAGE line reads MISSING; all four PACKAGE lines read NOT-LOWER=True (MEASUREMENT 2); exactly one COMPARABILITY line is present; CHANGED-PRODUCTION-LINES: 0 is present and the P2-T10 path list contains no path under QuickFiler/ or UtilitiesCS/. All four hold. AC18: MET.
