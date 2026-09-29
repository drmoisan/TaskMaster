# P0-T14 C# test-and-coverage baseline

Timestamp: 2026-09-29T09-02
Command: pwsh -NoProfile -Command 'Remove-Item -LiteralPath "coverage/test-results/mstest-coverage-run.summary.txt", "coverage/test-results/mstest-coverage-run.trx", "coverage/coverage.cobertura.jacoco.xml" -Force -ErrorAction SilentlyContinue; try { & ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug 2>&1 | Tee-Object -FilePath coverage/logs/927-mstest.log; exit 0 } catch { "TERMINATING: " + $_.Exception.Message; exit 1 }'; then the second MSTEST-COVERAGE payload (Gate command reference) reading the summary, the TRX Counters element, the first-party coverage line and the projection
EXIT_CODE: 0
Output Summary:
- First payload exit code 0; no TERMINATING line. The run was launched as a background process and completed in under 45 minutes (no MSTEST-LOCAL: STALLED). The Tee-Object output was additionally piped to Out-Null so that the console carried no per-test lines; the full console output is in the ignored log coverage/logs/927-mstest.log (7374 lines).
- Summary file written (coverage/test-results/mstest-coverage-run.summary.txt), transcribed verbatim below.
- COUNTERS total=7343 executed=7343 passed=7343 failed=0
- First-party coverage line transcribed verbatim below.

Summary (verbatim):

```text
Test run outcome: Completed
Total 7343, executed 7343, passed 7343, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```

Console coverage line (verbatim):

```text
First-party coverage: lines 56486/65737 (85.93%), branches 13657/17052 (80.09%)
```

BASELINE-LINE-PERCENT: 85.93
BASELINE-BRANCH-PERCENT: 80.09
BASELINE-LINES-VALID: 65737
BASELINE-BRANCHES-VALID: 17052
BASELINE-LINES-COVERED: 56486
BASELINE-BRANCHES-COVERED: 13657
BASELINE-PASSED: 7343
BASELINE-FAILED: 0

Package-level projection (coverage/coverage.cobertura.jacoco.xml, embedded verbatim):

```xml
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4207" covered="39217" />
    <counter type="BRANCH" missed="1796" covered="9473" />
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
```
