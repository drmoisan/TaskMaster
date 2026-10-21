# P0-T7 Test Baseline with Coverage-Instrument Observation

Timestamp: 2026-09-29T09-05
Task: P0-T7
Command: mcp__drm-copilot__run_poshqc_test with workspace_root = <repo-root> and scan_folders = ["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]; artifacts/pester/pester-junit.xml and artifacts/pester/powershell-coverage.xml read with the Grep tool; then CMD-PESTER-DIRECT (Route C) through the Bash tool with the two substitutions recorded below; then git status --porcelain -uall. The EXIT_CODE row is derived per D15 from the bundled JUnit root failures plus errors.
EXIT_CODE: 0

## CMD-TEST (bundled MCP route)

- Payload: {"ok":true,"tool":"run_poshqc_test","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC test against '<repo-root>' with 4 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG: true

### JUnit document (artifacts/pester/pester-junit.xml; derived figures only)

- TESTS_BASELINE: 320
- Root failures: 0
- Root errors: 0
- testsuite element count: 26 (7 under tests/scripts/dependencies, 19 under tests/scripts/vscode)
- Per testsuite (leaf file name: tests), every one with failures 0 and errors 0:
  - tests/scripts/dependencies:
    - AnalyzerItemRepair.Tests.ps1: 13
    - ConsistencyVerifier.Tests.ps1: 12
    - DependabotConfig.Tests.ps1: 17
    - PackageCompatibility.Tests.ps1: 8
    - PackageGraph.Tests.ps1: 32
    - ProjectConsistency.Tests.ps1: 18
    - Repair-PackageManifestConsistency.Tests.ps1: 31
  - tests/scripts/vscode:
    - Install-RepoDotNetSdk.Tests.ps1: 6
    - Invoke-MSTest.AssemblyDiscovery.Tests.ps1: 5
    - Invoke-MSTest.Main.Tests.ps1: 12
    - Invoke-MSTest.ResultsDirectory.Tests.ps1: 3
    - Invoke-MSTest.RunSettings.Tests.ps1: 28
    - Invoke-MSTest.TrxSummary.Tests.ps1: 7
    - Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1: 5
    - Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1: 12
    - Invoke-MSTestWithCoverage.FirstParty.Tests.ps1: 7
    - Invoke-MSTestWithCoverage.Helpers.Tests.ps1: 20
    - Invoke-MSTestWithCoverage.Merge.Tests.ps1: 6
    - Invoke-MSTestWithCoverage.PackageRate.Tests.ps1: 2
    - Invoke-MSTestWithCoverage.Projection.Tests.ps1: 11
    - Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1: 9
    - Invoke-MSTestWithCoverage.Threshold.Tests.ps1: 13
    - Invoke-Restore.Tests.ps1: 7
    - Invoke-VSBuild.Tests.ps1: 16
    - Sync-PackageReferences.Tests.ps1: 15
    - TestProcessCleanup.Tests.ps1: 5
- Sum of per-suite tests: 320 (equals the root value).

### Bundled coverage document (artifacts/pester/powershell-coverage.xml; derived figures only)

- BUNDLED_LINE_COVERED: 0
- BUNDLED_LINE_MISSED: 9294
- Package names (repository-relative), 13 packages: .claude/hooks, .claude/lib/blast-radius, .claude/lib/cleanup-manifest, .claude/lib/codex-routing, .claude/lib/discovery-validation, .claude/lib/hook-payload, .claude/lib/mermaid, .claude/lib/model-routing, .claude/lib/orchestrator-state, .claude/lib/project-file-merge, .claude/lib/worktree-resolution, .codex/hooks, .codex/scripts
- BUNDLED_NAMES_ENTRY_POINT: 0
- No package lies under scripts/dependencies or scripts/vscode, so Route A does not apply.

## Coverage route

COVERAGE-ROUTE: C

CMD-PESTER-DIRECT was run through the Bash tool with exactly two substitutions, everything else verbatim:

1. `$o` assigned `coverage/p0-t7-pester-coverage.jacoco.xml` (the substitution this task names).
2. The argument of `Set-Location -LiteralPath` changed from `(git rev-parse --show-toplevel)` to the literal worktree path, transcribed here as `"<repo-root>"`, so the run is pinned to this worktree regardless of the pwsh process start directory (substitution directed by the orchestrator for this run).

The JaCoCo output is written under the repository coverage directory (ignored by .gitignore line 144); it is not committed and is not copied into the feature folder. Console lines printed by the tests themselves are not transcribed.

- DIRECT_PESTER_EXIT: 0
- PESTER_COUNTS: passed=320 failed=0 skipped=0
- POPULATION_LINE: covered=1613 missed=94
- FILE_LINE: Invoke-MSTest.ps1 total=56 uncovered=7 uncovered_lines=243,245,246,248,252,256,261
- FILE_LINE: Invoke-MSTestWithCoverage.ps1 total=126 uncovered=13 uncovered_lines=149,171,187,229,315,323,327,340,346,352,414,430,438
- (No FILE_LINE for Invoke-MSTestWithCoverage.Scope.ps1: the file does not exist at the base anchor.)
- BASELINE_POPULATION_LINE_PERCENT: 94.49 (100 x 1613 / (1613 + 94) = 94.493..., rounded to two decimals)

## Tree hygiene

- git status --porcelain -uall after both runs: no line reads `?? coverage.xml`, so no file was removed. Porcelain lines present: the pre-existing agent-memory changes under .claude/agent-memory (3 modified, 3 untracked; not item work), the modified plan file, and the P0-T5 and P0-T6 evidence artifacts under the feature folder. No line names a path under scripts/ or tests/.

Output Summary: PASS. Bundled MCP test run ok true; JUnit root tests 320, failures 0, errors 0, 26 testsuites (19 under tests/scripts/vscode plus 7 under tests/scripts/dependencies). The bundled coverage document instruments only 13 packages under .claude and .codex (LINE covered 0, missed 9294) and never names the entry point, so COVERAGE-ROUTE: C was taken. The direct Pester 5.6.1 run over the CI population passed 320 of 320 (DIRECT_PESTER_EXIT 0) and reported POPULATION_LINE covered 1613, missed 94, giving BASELINE_POPULATION_LINE_PERCENT 94.49. Per-file baseline: Invoke-MSTestWithCoverage.ps1 126 lines with 13 uncovered; Invoke-MSTest.ps1 56 lines with 7 uncovered.
