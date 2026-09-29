# Baseline 04: MSTest with coverage ([P0-T12])

Timestamp: 2026-09-29T08-58
Command: CMD-COVERAGE with STAGE = baseline: pwsh -NoProfile -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.ps1; function Get-DotnetCoverageArgumentList { param([string]$OutputPath, [string]$CoverageConfig, [string]$VsTestPath, [string[]]$TestAssembly, [string]$RunSettingsPath, [string]$ResultsDirectory, [string]$LogFileName) return @("collect", "--output", $OutputPath, "--output-format", "cobertura", "--settings", $CoverageConfig, "--", $VsTestPath) + @($TestAssembly) + @("/Settings:$RunSettingsPath", "/InIsolation", "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests", "/ResultsDirectory:$ResultsDirectory", "/Logger:trx;LogFileName=$LogFileName", "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None") }; $runner = "COMPLETED"; try { Invoke-MSTestWithCoverageMain -SearchRoot . -Configuration Debug -CoverageOutput "coverage\930-baseline.cobertura.xml" -ResultsDirectory "coverage\test-results\930-baseline" -LogFileName "930-baseline.trx" -ScriptRoot scripts/vscode 2>&1 | Tee-Object -FilePath coverage/930-baseline-coverage.log } catch { $runner = "THREW"; $_.Exception.Message | Tee-Object -FilePath coverage/930-baseline-coverage.log -Append }; "RUNNER_RESULT=$runner"; "COBERTURA_EXISTS=$(Test-Path -LiteralPath coverage/930-baseline.cobertura.xml)"; "PROJECTION_EXISTS=$(Test-Path -LiteralPath coverage/930-baseline.cobertura.jacoco.xml)"; "SUMMARY_EXISTS=$(Test-Path -LiteralPath coverage/test-results/930-baseline/930-baseline.summary.txt)"; "SEQUENCE_FILES=$(@(Get-ChildItem -Recurse -File -LiteralPath coverage/test-results/930-baseline -Filter "Sequence_*.xml").Count)"'
Command: CMD-COVERAGE-PARSE with STAGE = baseline; CMD-RETURN-LINE; CMD-TRX-NAMES with STAGE = baseline
EXIT_CODE: 0
Output Summary:
- RUNNER_RESULT=COMPLETED
- COBERTURA_EXISTS=True, PROJECTION_EXISTS=True, SUMMARY_EXISTS=True
- SEQUENCE_FILES=0
- Runner output: Using vstest.console: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe; Discovered 9 test assemblies.
- Coverage output: REPO-ROOT\coverage\930-baseline.cobertura.xml
- Console: Test Run Successful. Total tests: 7320. Passed: 7320.
- First-party coverage: lines 56079/65737 (85.31%), branches 13593/17052 (79.71%)
- Coverage projection: REPO-ROOT\coverage\930-baseline.cobertura.jacoco.xml
- Test-result summary: REPO-ROOT\coverage\test-results\930-baseline\930-baseline.summary.txt
- Done. Coverage artifact: REPO-ROOT\coverage\930-baseline.cobertura.xml
- DOC_LINE_RATE=0.853081 DOC_BRANCH_RATE=0.79715 DOC_LINES_VALID=65737 DOC_LINES_COVERED=56079
- Summary text: Test run outcome: Completed / Total 7320, executed 7320, passed 7320, failed 0. / Skipped 0, derived as total minus executed rather than reported by the test platform. / Failed tests: none
- BASELINE-SUITE-TOTAL: 7320
- Excluded classes (Decision D3, identical in baseline and final runs): UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests, UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests, UtilitiesCS.Test.HelperClasses.SysImageListHelperTests, UtilitiesCS.Test.EmailIntelligence.OSBrowser_Tests. The repository mstest-coverage workflow runs these four classes unfiltered on every pull request.
- Committed projections: EVIDENCE/baseline/baseline-coverage.jacoco.xml (copy of the runner's JaCoCo projection) and EVIDENCE/baseline/baseline-test-summary.txt (copy of the runner's trx-derived summary). Nothing else was copied.
- CMD-RETURN-LINE: RETURN_LINE_MATCHES=1 RETURN_LINE_NUMBER=197; GUARD_COUNT=1
- BASELINE-RETURN-LINE: 197
- BASELINE-RETURN-HITS: 1
- BASELINE-RETURN-COND: 100% (2/2)
- BASELINE-UITHREAD-UNCOVERED: 3
- BASELINE-ILGLOBALS-UNCOVERED: 2

CMD-TRX-NAMES (STAGE = baseline):
```
NAME IsCompleted_WhenTheAwaiterContextIsAForeignDispatcherContextAndNoUiDispatcherWasCaptured_ReturnsFalse COUNT=0 OUTCOME=absent
NAME PublicStaticFields_AreAllInitOnly COUNT=0 OUTCOME=absent
NAME PublicStaticFields_AreExactlyTheTwoOpCodeTables COUNT=0 OUTCOME=absent
NAME Cache_IsInitialized COUNT=1 OUTCOME=Passed
NAME IsCompleted_WhenContextIsNotCurrent_ReturnsFalse COUNT=1 OUTCOME=Passed
NAME IsCompleted_WhenContextMatchesCurrent_ReturnsTrue COUNT=1 OUTCOME=Passed
NAME IsCompleted_WhenAmbientContextIsTheCapturedInstance_ReturnsTrue COUNT=1 OUTCOME=Passed
NAME IsCompleted_WhenAmbientContextIsNullAndCapturedContextIsNotNull_ReturnsFalse COUNT=1 OUTCOME=Passed
NAME IsCompleted_WhenUiThreadIdIsTheMinusOneSentinel_ReturnsFalse COUNT=1 OUTCOME=Passed
NAME IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue COUNT=1 OUTCOME=Passed
NAME IsCompleted_WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse COUNT=1 OUTCOME=Passed
NAME IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse COUNT=1 OUTCOME=Passed
NAME IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue COUNT=1 OUTCOME=Passed
NAME IsCompleted_OnDefaultAwaiterOnAContextFreeThread_ReturnsTrue COUNT=1 OUTCOME=Passed
NAME IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse COUNT=1 OUTCOME=Passed
NAME IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse COUNT=1 OUTCOME=Passed
TOTAL_RESULTS=7320
```

CMD-COVERAGE-PARSE (STAGE = baseline), per-file blocks:
```
FILE Threading/UiThread[.]cs$ CLASS_NODES=1 LINES_VALID=133 LINES_COVERED=130 UNCOVERED=3
  LINE 25 HITS=1 COND=none
  LINE 30 HITS=1 COND=none
  LINE 31 HITS=1 COND=100% (2/2)
  LINE 32 HITS=1 COND=none
  LINE 33 HITS=1 COND=none
  LINE 36 HITS=1 COND=none
  LINE 37 HITS=1 COND=50% (1/2)
  LINE 38 HITS=0 COND=none
  LINE 39 HITS=0 COND=none
  LINE 40 HITS=0 COND=none
  LINE 41 HITS=1 COND=100% (2/2)
  LINE 42 HITS=1 COND=none
  LINE 43 HITS=1 COND=none
  LINE 44 HITS=1 COND=none
  LINE 45 HITS=1 COND=none
  LINE 51 HITS=1 COND=none
  LINE 52 HITS=1 COND=none
  LINE 53 HITS=1 COND=100% (2/2)
  LINE 54 HITS=1 COND=none
  LINE 55 HITS=1 COND=none
  LINE 57 HITS=1 COND=none
  LINE 58 HITS=1 COND=none
  LINE 59 HITS=1 COND=none
  LINE 60 HITS=1 COND=none
  LINE 65 HITS=1 COND=none
  LINE 66 HITS=1 COND=none
  LINE 70 HITS=1 COND=none
  LINE 72 HITS=1 COND=none
  LINE 73 HITS=1 COND=none
  LINE 74 HITS=1 COND=none
  LINE 75 HITS=1 COND=none
  LINE 78 HITS=1 COND=none
  LINE 79 HITS=1 COND=none
  LINE 80 HITS=1 COND=none
  LINE 81 HITS=1 COND=none
  LINE 82 HITS=1 COND=none
  LINE 87 HITS=1 COND=100% (2/2)
  LINE 88 HITS=1 COND=none
  LINE 89 HITS=1 COND=50% (1/2)
  LINE 90 HITS=1 COND=none
  LINE 91 HITS=1 COND=none
  LINE 92 HITS=1 COND=none
  LINE 93 HITS=1 COND=none
  LINE 94 HITS=1 COND=none
  LINE 95 HITS=1 COND=none
  LINE 96 HITS=1 COND=none
  LINE 97 HITS=1 COND=none
  LINE 99 HITS=1 COND=none
  LINE 100 HITS=1 COND=none
  LINE 109 HITS=1 COND=none
  LINE 111 HITS=1 COND=none
  LINE 123 HITS=1 COND=none
  LINE 124 HITS=1 COND=none
  LINE 125 HITS=1 COND=none
  LINE 126 HITS=1 COND=none
  LINE 127 HITS=1 COND=none
  LINE 128 HITS=1 COND=none
  LINE 129 HITS=1 COND=none
  LINE 130 HITS=1 COND=none
  LINE 131 HITS=1 COND=none
  LINE 132 HITS=1 COND=none
  LINE 133 HITS=1 COND=none
  LINE 134 HITS=1 COND=none
  LINE 135 HITS=1 COND=100% (2/2)
  LINE 136 HITS=1 COND=none
  LINE 142 HITS=1 COND=none
  LINE 147 HITS=1 COND=none
  LINE 148 HITS=1 COND=100% (2/2)
  LINE 149 HITS=1 COND=none
  LINE 150 HITS=1 COND=none
  LINE 152 HITS=1 COND=none
  LINE 153 HITS=1 COND=none
  LINE 158 HITS=1 COND=none
  LINE 159 HITS=1 COND=none
  LINE 160 HITS=1 COND=100% (2/2)
  LINE 161 HITS=1 COND=none
  LINE 162 HITS=1 COND=none
  LINE 167 HITS=1 COND=100% (2/2)
  LINE 168 HITS=1 COND=none
  LINE 169 HITS=1 COND=none
  LINE 171 HITS=1 COND=100% (4/4)
  LINE 172 HITS=1 COND=none
  LINE 173 HITS=1 COND=none
  LINE 182 HITS=1 COND=100% (6/6)
  LINE 183 HITS=1 COND=none
  LINE 184 HITS=1 COND=none
  LINE 185 HITS=1 COND=none
  LINE 186 HITS=1 COND=none
  LINE 187 HITS=1 COND=none
  LINE 188 HITS=1 COND=none
  LINE 189 HITS=1 COND=none
  LINE 190 HITS=1 COND=none
  LINE 191 HITS=1 COND=none
  LINE 197 HITS=1 COND=100% (2/2)
  LINE 198 HITS=1 COND=none
  LINE 199 HITS=1 COND=none
  LINE 200 HITS=1 COND=none
  LINE 201 HITS=1 COND=none
  LINE 202 HITS=1 COND=none
  LINE 206 HITS=1 COND=none
  LINE 208 HITS=1 COND=none
  LINE 212 HITS=1 COND=none
  LINE 213 HITS=1 COND=none
  LINE 214 HITS=1 COND=none
  LINE 219 HITS=1 COND=none
  LINE 220 HITS=1 COND=100% (2/2)
  LINE 221 HITS=1 COND=none
  LINE 222 HITS=1 COND=none
  LINE 223 HITS=1 COND=none
  LINE 225 HITS=1 COND=none
  LINE 226 HITS=1 COND=none
  LINE 227 HITS=1 COND=none
  LINE 233 HITS=1 COND=none
  LINE 234 HITS=1 COND=none
  LINE 236 HITS=1 COND=none
  LINE 247 HITS=1 COND=none
  LINE 267 HITS=1 COND=none
  LINE 270 HITS=1 COND=none
  LINE 271 HITS=1 COND=100% (2/2)
  LINE 272 HITS=1 COND=none
  LINE 277 HITS=1 COND=none
  LINE 279 HITS=1 COND=none
  LINE 280 HITS=1 COND=none
  LINE 281 HITS=1 COND=none
  LINE 293 HITS=1 COND=none
  LINE 294 HITS=1 COND=100% (2/2)
  LINE 295 HITS=1 COND=none
  LINE 296 HITS=1 COND=none
  LINE 297 HITS=1 COND=none
  LINE 298 HITS=1 COND=50% (1/2)
  LINE 299 HITS=1 COND=none
  LINE 300 HITS=1 COND=none
  LINE 302 HITS=1 COND=none
FILE SDIL Reader/ILGlobals[.]cs$ CLASS_NODES=1 LINES_VALID=40 LINES_COVERED=38 UNCOVERED=2
  LINE 113 HITS=1 COND=none
  LINE 131 HITS=1 COND=none
  LINE 140 HITS=1 COND=none
  LINE 141 HITS=1 COND=none
  LINE 142 HITS=1 COND=none
  LINE 143 HITS=1 COND=none
  LINE 144 HITS=1 COND=100% (2/2)
  LINE 145 HITS=1 COND=none
  LINE 146 HITS=1 COND=none
  LINE 147 HITS=1 COND=100% (2/2)
  LINE 148 HITS=1 COND=none
  LINE 151 HITS=1 COND=none
  LINE 152 HITS=1 COND=none
  LINE 153 HITS=1 COND=100% (2/2)
  LINE 154 HITS=1 COND=none
  LINE 155 HITS=1 COND=none
  LINE 156 HITS=1 COND=none
  LINE 158 HITS=1 COND=none
  LINE 159 HITS=1 COND=50% (1/2)
  LINE 160 HITS=0 COND=none
  LINE 161 HITS=0 COND=none
  LINE 163 HITS=1 COND=none
  LINE 164 HITS=1 COND=none
  LINE 165 HITS=1 COND=none
  LINE 166 HITS=1 COND=none
  LINE 167 HITS=1 COND=none
  LINE 168 HITS=1 COND=none
  LINE 169 HITS=1 COND=none
  LINE 178 HITS=1 COND=none
  LINE 179 HITS=1 COND=none
  LINE 180 HITS=1 COND=none
  LINE 192 HITS=1 COND=none
  LINE 193 HITS=1 COND=none
  LINE 194 HITS=1 COND=none
  LINE 199 HITS=1 COND=none
  LINE 200 HITS=1 COND=none
  LINE 204 HITS=1 COND=none
  LINE 205 HITS=1 COND=none
  LINE 207 HITS=1 COND=none
  LINE 208 HITS=1 COND=none
```
