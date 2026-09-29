# Final 06: MSTest with coverage ([P2-T6])

Timestamp: 2026-09-29T09-22
Command: CMD-COVERAGE with STAGE = final (identical to the [P0-T12] command with baseline replaced by final, including the same four-class exclusion set and blame collector; Decision D3)
Command: CMD-COVERAGE-PARSE with STAGE = final; CMD-RETURN-LINE; CMD-TRX-NAMES with STAGE = final
EXIT_CODE: 0
Iteration: 1
Output Summary:
- RUNNER_RESULT=COMPLETED (the runner 80 percent line and 75 percent branch first-party assertions passed and the projection reconciled)
- COBERTURA_EXISTS=True, PROJECTION_EXISTS=True, SUMMARY_EXISTS=True
- SEQUENCE_FILES=0
- Runner output: Using vstest.console: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe; Discovered 9 test assemblies.
- Coverage output: REPO-ROOT\coverage\930-final.cobertura.xml
- Console: Test Run Successful. Total tests: 7322. Passed: 7322.
- First-party coverage: lines 56084/65736 (85.32%), branches 13597/17054 (79.73%)
- Coverage projection: REPO-ROOT\coverage\930-final.cobertura.jacoco.xml
- Test-result summary: REPO-ROOT\coverage\test-results\930-final\930-final.summary.txt
- Done. Coverage artifact: REPO-ROOT\coverage\930-final.cobertura.xml
- DOC_LINE_RATE=0.85317 DOC_BRANCH_RATE=0.797291 DOC_LINES_VALID=65736 DOC_LINES_COVERED=56084
- Summary text: Test run outcome: Completed / Total 7322, executed 7322, passed 7322, failed 0. / Skipped 0, derived as total minus executed rather than reported by the test platform. / Failed tests: none
- Total 7322 equals BASELINE-SUITE-TOTAL 7320 plus 2 (one #889 test added; two #863 tests added and one deleted).
- Decision D13 repeat: not used (first run completed with failed 0).
- Excluded classes (Decision D3, identical to the baseline run): UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests, UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests, UtilitiesCS.Test.HelperClasses.SysImageListHelperTests, UtilitiesCS.Test.EmailIntelligence.OSBrowser_Tests. The repository mstest-coverage workflow runs these four classes unfiltered on every pull request.
- Committed projections: EVIDENCE/qa-gates/final-coverage.jacoco.xml and EVIDENCE/qa-gates/final-test-summary.txt (copies of the runner outputs). Nothing else was copied.
- CMD-RETURN-LINE: RETURN_LINE_MATCHES=1 RETURN_LINE_NUMBER=198; GUARD_COUNT=2
- FINAL-RETURN-LINE: 198
- FINAL-RETURN-HITS: 1
- FINAL-RETURN-COND: 100% (4/4)
- FINAL-UITHREAD-UNCOVERED: 3
- FINAL-ILGLOBALS-UNCOVERED: 2

CMD-TRX-NAMES (STAGE = final):
```
NAME IsCompleted_WhenTheAwaiterContextIsAForeignDispatcherContextAndNoUiDispatcherWasCaptured_ReturnsFalse COUNT=1 OUTCOME=Passed
NAME PublicStaticFields_AreAllInitOnly COUNT=1 OUTCOME=Passed
NAME PublicStaticFields_AreExactlyTheTwoOpCodeTables COUNT=1 OUTCOME=Passed
NAME Cache_IsInitialized COUNT=0 OUTCOME=absent
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
TOTAL_RESULTS=7322
```

CMD-COVERAGE-PARSE (STAGE = final), per-file blocks:
```
FILE Threading/UiThread[.]cs$ CLASS_NODES=1 LINES_VALID=134 LINES_COVERED=131 UNCOVERED=3
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
  LINE 198 HITS=1 COND=100% (4/4)
  LINE 199 HITS=1 COND=none
  LINE 200 HITS=1 COND=none
  LINE 201 HITS=1 COND=none
  LINE 202 HITS=1 COND=none
  LINE 203 HITS=1 COND=none
  LINE 204 HITS=1 COND=none
  LINE 208 HITS=1 COND=none
  LINE 210 HITS=1 COND=none
  LINE 214 HITS=1 COND=none
  LINE 215 HITS=1 COND=none
  LINE 216 HITS=1 COND=none
  LINE 221 HITS=1 COND=none
  LINE 222 HITS=1 COND=100% (2/2)
  LINE 223 HITS=1 COND=none
  LINE 224 HITS=1 COND=none
  LINE 225 HITS=1 COND=none
  LINE 227 HITS=1 COND=none
  LINE 228 HITS=1 COND=none
  LINE 229 HITS=1 COND=none
  LINE 235 HITS=1 COND=none
  LINE 236 HITS=1 COND=none
  LINE 238 HITS=1 COND=none
  LINE 249 HITS=1 COND=none
  LINE 269 HITS=1 COND=none
  LINE 272 HITS=1 COND=none
  LINE 273 HITS=1 COND=100% (2/2)
  LINE 274 HITS=1 COND=none
  LINE 279 HITS=1 COND=none
  LINE 281 HITS=1 COND=none
  LINE 282 HITS=1 COND=none
  LINE 283 HITS=1 COND=none
  LINE 295 HITS=1 COND=none
  LINE 296 HITS=1 COND=100% (2/2)
  LINE 297 HITS=1 COND=none
  LINE 298 HITS=1 COND=none
  LINE 299 HITS=1 COND=none
  LINE 300 HITS=1 COND=50% (1/2)
  LINE 301 HITS=1 COND=none
  LINE 302 HITS=1 COND=none
  LINE 304 HITS=1 COND=none
FILE SDIL Reader/ILGlobals[.]cs$ CLASS_NODES=1 LINES_VALID=38 LINES_COVERED=36 UNCOVERED=2
  LINE 137 HITS=1 COND=none
  LINE 138 HITS=1 COND=none
  LINE 139 HITS=1 COND=none
  LINE 140 HITS=1 COND=none
  LINE 141 HITS=1 COND=100% (2/2)
  LINE 142 HITS=1 COND=none
  LINE 143 HITS=1 COND=none
  LINE 144 HITS=1 COND=100% (2/2)
  LINE 145 HITS=1 COND=none
  LINE 148 HITS=1 COND=none
  LINE 149 HITS=1 COND=none
  LINE 150 HITS=1 COND=100% (2/2)
  LINE 151 HITS=1 COND=none
  LINE 152 HITS=1 COND=none
  LINE 153 HITS=1 COND=none
  LINE 155 HITS=1 COND=none
  LINE 156 HITS=1 COND=50% (1/2)
  LINE 157 HITS=0 COND=none
  LINE 158 HITS=0 COND=none
  LINE 160 HITS=1 COND=none
  LINE 161 HITS=1 COND=none
  LINE 162 HITS=1 COND=none
  LINE 163 HITS=1 COND=none
  LINE 164 HITS=1 COND=none
  LINE 165 HITS=1 COND=none
  LINE 166 HITS=1 COND=none
  LINE 175 HITS=1 COND=none
  LINE 176 HITS=1 COND=none
  LINE 177 HITS=1 COND=none
  LINE 189 HITS=1 COND=none
  LINE 190 HITS=1 COND=none
  LINE 191 HITS=1 COND=none
  LINE 196 HITS=1 COND=none
  LINE 197 HITS=1 COND=none
  LINE 201 HITS=1 COND=none
  LINE 202 HITS=1 COND=none
  LINE 204 HITS=1 COND=none
  LINE 205 HITS=1 COND=none
```
