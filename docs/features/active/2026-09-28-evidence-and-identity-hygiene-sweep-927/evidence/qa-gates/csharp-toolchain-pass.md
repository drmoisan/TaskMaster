# P6-T9 C# toolchain pass (AC13)

## iter1

Timestamp: 2026-09-29T22-23
Command: MSTEST-COVERAGE first payload: pwsh -NoProfile -Command 'Remove-Item -LiteralPath "coverage/test-results/mstest-coverage-run.summary.txt", "coverage/test-results/mstest-coverage-run.trx", "coverage/coverage.cobertura.jacoco.xml" -Force -ErrorAction SilentlyContinue; try { & ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug 2>&1 | Tee-Object -FilePath coverage/logs/927-mstest.log; exit 0 } catch { "TERMINATING: " + $_.Exception.Message; exit 1 }' (run as a background process from the item worktree root, the worktree path assembled with the worktrees segment split across two literals per C1; Tee-Object additionally piped to Out-Null, as in P0-T14, and a ROUTE-EXIT=0 line printed before exit 0); then the second MSTEST-COVERAGE payload (Gate command reference).
EXIT_CODE: 0
Output Summary:
- Route exit code 0; no TERMINATING line; the run completed in about 100 seconds (no MSTEST-LOCAL: STALLED).
- Summary written; transcribed verbatim below. failed 0.
- COUNTERS total=7346 executed=7346 passed=7346 failed=0
- FINAL-PASSED: 7346 (not lower than BASELINE-PASSED: 7343 from P0-T14)
- FINAL-FAILED: 0
- First-party coverage: lines 56479/65736 (85.92%), branches 13656/17054 (80.08%)
- Coverage comparison against P0-T14: line 85.92 below 85.93 and branch 80.08 below 80.09. The P6-T9 not-below clause is NOT MET; see csharp-coverage-projection.md. Under D10 and P6-T29 this makes AC13 NOT MET. It is not a loop failure (the COUNTERS line shows failed=0) and not a stop (C12).

Summary (verbatim):

```text
Test run outcome: Completed
Total 7346, executed 7346, passed 7346, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```

The four C# commands of this loop (iter1):

| Task | Command | Exit code | CoreCompile figures |
|---|---|---|---|
| P6-T5 | dotnet tool run csharpier format . | 0 (REWRITTEN=0; porcelain empty) | n/a |
| P6-T6 | dotnet tool run csharpier check . | 0 ("Checked 1625 files") | n/a |
| P6-T7 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | 0 (SUCCEEDED=1, ZERO-ERRORS=1, 0 Warning(s)) | OUT-LINES=36, SKIP-CORECOMPILE=0 |
| P6-T8 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | 0 (SUCCEEDED=1, ZERO-ERRORS=1, 0 Warning(s)) | OUT-LINES=36, SKIP-CORECOMPILE=0 |
| P6-T9 | scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug | 0 (passed 7346, failed 0) | n/a |

The raw TRX document, the raw Cobertura document and the console log stay under the ignored coverage/ directory and are not committed.
RE-ANCHORED 2026-09-29 by P6-T39: BASELINE-MSTEST-RUN-ID=36651909330 (main push run at merge base ddbab26a0); baseline LinePercent=85.92 BranchPercent=80.08; pull-request CI run 36664415704 LinePercent=85.92 BranchPercent=80.08; MSTEST-COMPARE=NOT-BELOW (see csharp-coverage-reanchor.md).
