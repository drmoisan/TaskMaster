# R1 P1-T3 Expect-Fail Run (Before the Production Edits)

Timestamp: 2026-09-29T10-50
Task: P1-T3 [expect-fail] (remediation-plan.2026-09-29T10-00.md)
Command: mcp__drm-copilot__run_poshqc_test with workspace_root = <repo-root> and scan_folders = ["tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1"]; artifacts/pester/pester-junit.xml read with the Grep tool. EXIT_CODE derived per R10 from the JUnit root failures plus errors, capped to 1.
EXIT_CODE: 1
ExpectedExitCode: 1

## MCP result

- Payload: {"ok":false,"tool":"run_poshqc_test","workspace_root":"<repo-root>","summary":"Command exited with code 6."}
- MCP_RESULT_OK_FLAG: false (expected)

## JUnit root

- tests 22, failures 6, errors 0

## Failing testcases (6), by It text

- It 17 `throws when the repository root is a relative path`: "Expected an exception with message like 'RepoRoot must be an absolute path: repo' to be thrown, but no exception was thrown."
- It 18 `throws when the resolved search root is a relative path`: "Expected an exception with message like 'ResolvedSearchRoot must be an absolute path: QuickFiler.Test' to be thrown, but no exception was thrown."
- It 19 `does not throw on a scoped run whose document is below both floors`: "Expected no exception to be thrown, but an exception "The term 'Assert-CoberturaCoverageThresholdForRun' is not recognized as a name of a cmdlet, function, script file, or executable program. ..."
- It 20 `writes exactly one warning naming the scoped search root`: "CommandNotFoundException: The term 'Assert-CoberturaCoverageThresholdForRun' is not recognized as a name of a cmdlet, function, script file, or executable program. ..."
- It 21 `throws the line threshold message on an unscoped run below the line floor`: "Expected an exception with message like 'Cobertura line coverage 40% is below the required 80% threshold.' to be thrown, but the message was 'The term 'Assert-CoberturaCoverageThresholdForRun' is not recognized ..."
- It 22 `throws the branch threshold message on an unscoped run at the line floor and below the branch floor`: "Expected an exception with message like 'Cobertura branch coverage 50% is below the required 75% threshold.' to be thrown, but the message was 'The term 'Assert-CoberturaCoverageThresholdForRun' is not recognized ..."

## Passing testcases (16), by It text

1. returns false when the resolved search root equals the repository root
2. returns false for a dot search root beneath the repository root
3. returns false for a dot-backslash search root beneath the repository root
4. returns false when only a trailing separator differs
5. returns false when only letter case differs
6. returns true for a subdirectory search root
7. returns true for a sibling directory whose name extends the repository root name
8. returns true for the parent directory of the repository root
9. completes without error on a scoped run whose post-processed document is below both floors
10. writes exactly one warning naming the skipped assertions and the scoped search root
11. still terminates with an error when collection returns a non-zero exit code on a scoped run
12. throws the line threshold message when the search root is omitted and the line rate is below 80 percent
13. throws the branch threshold message for a dot search root when the branch rate is below 75 percent
14. documents the scoped-run behavior on the SearchRoot parameter
15. throws when the repository root is an empty string (control)
16. throws when the resolved search root is an empty string (control)

(Numbering follows the Test Specification; It 1 to 14 are the existing blocks.)

Output Summary:
- Expected partition observed: 6 failing (It 17 to 22) and 16 passing (It 1 to 16); JUnit tests 22, failures 6, errors 0.
- It 17 and 18 fail with "no exception was thrown" (the predicate resolved the relative input); It 19 to 22 fail because `Assert-CoberturaCoverageThresholdForRun` is not recognized.
- The quoted messages contain no fixture drive-letter root and no absolute repository path.
