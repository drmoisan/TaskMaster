# Coverage Baseline (P0-T11)

Timestamp: 2026-10-03T08-33
Command: CMD-COVERAGE-DIRECT (STAGE baseline): dotnet-coverage collect --output coverage\baseline-959.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-959.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\959\baseline" "/Logger:trx;LogFileName=baseline-959.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; then CMD-COVERAGE-POST (STAGE baseline) and CMD-COVERAGE-TEXTS (STAGE baseline, BASELINE-HASH NONE); route DIRECT; filter as shown (the P0-T9 exclusion applied). In the CMD-COVERAGE-TEXTS run the final-stage block guarded by if ("baseline" -ceq "final") was replaced by an equivalent no-op body because its condition is false at this stage; no line of output depends on it.
EXIT_CODE: 0 (the printed COLLECT_EXIT_CODE)
Output Summary: branch (a): exit 0, both floors met, empty FAILED-SET; 7361 of 7361 tests passed; first-party coverage lines 85.36 percent, branches 79.75 percent; SortEmail family 90 valid lines with 4 uncovered (3 exempt in SortEmail.TrySaveAttachment.cs, 1 non-exempt in SortEmail.MailItemSort.cs).

- COVERAGE-ROUTE: DIRECT
- EXCLUSION: &FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- COLLECT_EXIT_CODE: 0
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
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
- First-party coverage: lines 56212/65855 (85.36%), branches 13620/17078 (79.75%)
- FIRST-PARTY-LINE-PERCENT: 85.36
- FIRST-PARTY-BRANCH-PERCENT: 79.75
- BASELINE-FIRST-PARTY-LINE-PERCENT: 85.36
- BASELINE-FIRST-PARTY-BRANCH-PERCENT: 79.75
- FAILED-SET: (empty)
- BASELINE-UCS-LINE: 38909/43508
- BASELINE-UCS-BRANCH: 9434/11293
- BASELINE-QF-LINE: 10461/12754
- BASELINE-QF-BRANCH: 2518/3217

SUMMARY-BEGIN
Test run outcome: Completed
Total 7361, executed 7361, passed 7361, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

PROJECTION-BEGIN
```xml
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4599" covered="38909" />
    <counter type="BRANCH" missed="1859" covered="9434" />
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
    <counter type="LINE" missed="802" covered="2477" />
    <counter type="BRANCH" missed="211" covered="519" />
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
```
PROJECTION-END

CMD-COVERAGE-TEXTS (STAGE baseline) printed lines:
- SORTEMAIL-CLASS baseline UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs valid=5 covered=5 uncovered=0
- SORTEMAIL-CLASS baseline UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs valid=10 covered=10 uncovered=0
- SORTEMAIL-CLASS baseline UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs valid=66 covered=63 uncovered=3
- SORTEMAIL-CLASS baseline UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs valid=8 covered=8 uncovered=0
- SORTEMAIL-CLASS baseline UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs valid=1 covered=0 uncovered=1
- SORTEMAIL-AGG baseline valid=90 covered=86 uncovered=4
- SORTEMAIL-DIR-CLASSES: 17
- GUARD-CONDITION-LINES: (empty)
- EXEMPT-LAMBDA-LINES: 49
- EXEMPT-LAMBDA-COUNT: 1
- EXEMPT-ELSE-BRACE-LINES: 154
- EXEMPT-ELSE-BRACE-COUNT: 1
- EXEMPT-CATCH-BRACE-LINES: 155
- EXEMPT-CATCH-BRACE-COUNT: 1
- EXEMPT-GUARD-BRACE-LINES: (empty)
- EXEMPT-GUARD-BRACE-COUNT: 0
- EXEMPT-LINES: 49,154,155
- TRYSAVE-CLASS-FOUND: True
- EXEMPT-UNCOVERED-LINE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs:49
- EXEMPT-UNCOVERED-LINE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs:154
- EXEMPT-UNCOVERED-LINE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs:155
- NONEXEMPT-UNCOVERED UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs:153 :: await attachments.ForEachAsync(async x => await x.SaveAttachmentAsync());
- EXEMPT-UNCOVERED: 3
- NONEXEMPT-COUNT: 1
- NONEXEMPT-SET-SHA256: 408A19E922396ADCFF033FE8156DCFE3C818FA8A5484469691071CC2176E142D
- NONEXEMPT-SET-MATCHES-BASELINE: False (baseline stage; compared with the literal NONE)
- CONTROL-LINE: 28
- CONTROL-SET-SHA256: D27D0D3C5B00BADEE7352563C5BC2F5BFD03A519092298572D4B3EDB90599DE3
- CONTROL-DIFFERS-BASELINE: True
- BASELINE-NONEXEMPT-HASH: 408A19E922396ADCFF033FE8156DCFE3C818FA8A5484469691071CC2176E142D

Acceptance check: the projection holds UtilitiesCS and QuickFiler with LINE and BRANCH counters; First-party line 85.36 (at least 80) and branch 79.75 (at least 75); the summary's first line begins Test run outcome:; EXIT_CODE 0 equals the default expectation (branch (a)); SORTEMAIL-DIR-CLASSES 17 and TRYSAVE-CLASS-FOUND True; exemption counts 1, 1, 1 and 0; NONEXEMPT-SET-SHA256 is 64 hexadecimal characters with NONEXEMPT-COUNT 1; every SANDBOX value False and no absolute path recorded. All eight hold. coverage\baseline-959.cobertura.xml and coverage\baseline-959.jacoco.xml remain on disk, git-ignored.
