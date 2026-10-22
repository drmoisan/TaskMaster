# P1-T3 Expect-Fail Run (before the fix)

Timestamp: 2026-09-29T09-14
Task: P1-T3 [expect-fail]
Command: mcp__drm-copilot__run_poshqc_test with workspace_root = <repo-root> and scan_folders = ["tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1"]; then Read and Grep over artifacts/pester/pester-junit.xml
EXIT_CODE: 1
ExpectedExitCode: 1
MCP_RESULT_OK_FLAG: false

MCP payload (transcribed, workspace_root replaced by <repo-root>):
- ok: false
- tool: run_poshqc_test
- workspace_root: <repo-root>
- summary: Command exited with code 11.

Output Summary:
- JUnit root (testsuites): tests 14, failures 11, errors 0 (expected tests 14, failures 11, errors 0).
- One testsuite, leaf name Invoke-MSTestWithCoverage.Scope.Tests.ps1: tests 14, failures 11, errors 0, skipped 0.
- EXIT_CODE 1 is derived per D15 (root failures plus errors = 11, greater than zero). The MCP summary exit code 11 equals the failure count.
- Partition observed equals the partition expected by the Test Specification: It blocks 1 to 10 and 14 failing; It blocks 11, 12 and 13 passing.
- The bug is reproduced: on a scoped run (-SearchRoot 'QuickFiler.Test') the document-level line assertion ran and terminated the run (It 9 and It 10).

## Failing testcases (11)

1. returns false when the resolved search root equals the repository root
   Failure: `CommandNotFoundException: The term 'Test-CoverageRunIsScoped' is not recognized as a name of a cmdlet, function, script file, or executable program.`
2. returns false for a dot search root beneath the repository root
   Failure: `CommandNotFoundException: The term 'Test-CoverageRunIsScoped' is not recognized ...` (same text)
3. returns false for a dot-backslash search root beneath the repository root
   Failure: `CommandNotFoundException: The term 'Test-CoverageRunIsScoped' is not recognized ...` (same text)
4. returns false when only a trailing separator differs
   Failure: `CommandNotFoundException: The term 'Test-CoverageRunIsScoped' is not recognized ...` (same text)
5. returns false when only letter case differs
   Failure: `CommandNotFoundException: The term 'Test-CoverageRunIsScoped' is not recognized ...` (same text)
6. returns true for a subdirectory search root
   Failure: `CommandNotFoundException: The term 'Test-CoverageRunIsScoped' is not recognized ...` (same text)
7. returns true for a sibling directory whose name extends the repository root name
   Failure: `CommandNotFoundException: The term 'Test-CoverageRunIsScoped' is not recognized ...` (same text)
8. returns true for the parent directory of the repository root
   Failure: `CommandNotFoundException: The term 'Test-CoverageRunIsScoped' is not recognized ...` (same text)
9. completes without error on a scoped run whose post-processed document is below both floors
   Failure: `Expected no exception to be thrown, but an exception "Cobertura line coverage 40% is below the required 80% threshold." was thrown from scripts\vscode\Invoke-MSTestWithCoverage.Threshold.ps1:54`
10. writes exactly one warning naming the skipped assertions and the scoped search root
    Failure: `RuntimeException: Cobertura line coverage 40% is below the required 80% threshold.` (thrown at Assert-CoberturaLineCoverageThreshold, Invoke-MSTestWithCoverage.Threshold.ps1:54)
14. documents the scoped-run behavior on the SearchRoot parameter
    Failure: `Expected 1, but got 0.` at `$key.Count | Should -Be 1` (test file line 226): the SearchRoot parameter has no help entry.

## Passing testcases (3, controls)

11. still terminates with an error when collection returns a non-zero exit code on a scoped run
12. throws the line threshold message when the search root is omitted and the line rate is below 80 percent
13. throws the branch threshold message for a dot search root when the branch rate is below 75 percent

No fixture drive-letter root appears in any quoted message above; repository file locations are given repository-relative.
