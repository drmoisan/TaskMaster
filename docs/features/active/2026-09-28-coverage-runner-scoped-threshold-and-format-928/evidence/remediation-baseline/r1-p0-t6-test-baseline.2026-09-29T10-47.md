# R1 P0-T6 Test Baseline on the Unmodified Tree (R-1 Reproduction)

Timestamp: 2026-09-29T10-47
Task: P0-T6 (remediation-plan.2026-09-29T10-00.md)
Command: mcp__drm-copilot__run_poshqc_test with workspace_root = <repo-root> and scan_folders = ["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]; artifacts/pester/pester-junit.xml read with the Grep tool; then CMD-PESTER-DIRECT through the Bash tool with the two substitutions recorded below; then git -C <repo-root> status --porcelain -uall. The EXIT_CODE row is derived per R10 from the bundled JUnit root failures plus errors.
EXIT_CODE: 0

## CMD-TEST (bundled MCP route)

- Payload: {"ok":true,"tool":"run_poshqc_test","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC test against '<repo-root>' with 4 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG: true
- TESTS_START: 334
- Root failures: 0
- Root errors: 0
- testsuite element count: 27
- Testsuite Invoke-MSTestWithCoverage.Scope.Tests.ps1 (leaf name): tests 14, failures 0

## CMD-PESTER-DIRECT

Substitutions (exactly two): (1) `$o` assigned `coverage/r1-p0-t6-pester-coverage.jacoco.xml`; (2) the argument of `Set-Location -LiteralPath` changed from `(git rev-parse --show-toplevel)` to the literal worktree path in double quotes, transcribed as `"<repo-root>"`. Everything else verbatim. The JaCoCo document is written under the ignored coverage directory (.gitignore line 144) and is not copied into the feature folder. Console lines printed by the tests themselves are not transcribed.

- DIRECT_PESTER_EXIT: 0
- PESTER_COUNTS: passed=334 failed=0 skipped=0
- POPULATION_LINE: covered=1620 missed=95
- FILE_LINE: Invoke-MSTest.ps1 total=56 uncovered=7 uncovered_lines=243,245,246,248,252,256,261
- FILE_LINE: Invoke-MSTestWithCoverage.ps1 total=129 uncovered=14 uncovered_lines=149,171,187,229,333,341,345,358,364,370,408,441,457,465
- FILE_LINE: Invoke-MSTestWithCoverage.Scope.ps1 total=5 uncovered=0 uncovered_lines=
- START_POPULATION_LINE_PERCENT: 94.46 (100 x 1620 / (1620 + 95) = 94.460..., two decimals)
- R-1-REPRODUCED: yes (408 is listed among the entry point's uncovered_lines)

## Comparison baseline (R7)

Copied from evidence/baseline/p0-t7-test-baseline.2026-09-29T09-05.md (original P0-T7, Route C, identical command):

- BASELINE_POPULATION_LINE_PERCENT: 94.49
- BASELINE_POPULATION_LINE: covered=1613 missed=94

## Tree hygiene

- git status --porcelain -uall after both runs: no line reads `?? coverage.xml`, so nothing was removed. Lines present: the pre-existing .claude/agent-memory paths (3 modified, 3 untracked), the modified remediation plan file (check-offs), and the P0-T1 to P0-T5 evidence artifacts under evidence/remediation-baseline. No line names a path under scripts/ or tests/.

Output Summary:
- PASS. Bundled run ok true; JUnit 334 tests, 0 failures, 0 errors, 27 testsuites; the Scope suite 14 of 14.
- Direct Pester 5.6.1 run: 334 passed, 0 failed, 0 skipped; exit 0; POPULATION_LINE covered 1620, missed 95; START_POPULATION_LINE_PERCENT 94.46 (the expected value).
- R-1 reproduced: entry-point line 408 is uncovered. Comparison baseline fixed at 94.49 (1613/94).
