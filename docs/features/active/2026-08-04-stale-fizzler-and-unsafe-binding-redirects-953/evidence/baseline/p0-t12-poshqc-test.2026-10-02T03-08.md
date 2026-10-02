# P0-T12 Baseline PowerShell test step

Timestamp: 2026-10-02T03-08
Command: pwsh -NoProfile -Command 'Remove-Item -LiteralPath <execution-worktree-root>/artifacts/pester/pester-junit.xml -ErrorAction SilentlyContinue' (CMD-JUNIT-DELETE); Glob `artifacts/pester/pester-junit.xml`; MCP mcp__drm-copilot__run_poshqc_test with workspace_root `<execution-worktree-root>` and scan_folders `["tests/scripts/dependencies"]`; CMD-JUNIT-READ Greps (`<testsuites `, `<testsuite `, `status="Failed"` with -A 2) over `<execution-worktree-root>/artifacts/pester/pester-junit.xml`
EXIT_CODE: 0
ExpectedExitCode: 0

CMD-JUNIT-DELETE observations: the pwsh invocation returned exit code 1 as observed (the Bash tool reported `Exit code 1` with no message). With `-ErrorAction SilentlyContinue`, `Remove-Item` on a path that does not exist records a non-terminating error and the `-Command` host then exits 1, so the observation is consistent with no pre-existing document; it is recorded as observed and not relied on. The Glob for `artifacts/pester/pester-junit.xml` returned no files after the delete.

GLOB-BLIND: after the test run the same Glob again returned `No files found` although the Grep tool read the document by absolute path (lines below). The plan's round 3 observation that Glob returns this gitignored file with a worktree root as its path did not reproduce at this worktree. The existence precondition of CMD-JUNIT-READ was therefore established by the successful Greps, and the document's freshness rests on the delete step plus the document content: a full 8-suite run with the worktree path in every suite name.

Payload (verbatim, worktree root replaced per C4):

```text
{"ok":true,"tool":"run_poshqc_test","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC test against '<execution-worktree-root>' with 1 selected scan folder(s)."}
```

EXIT_CODE derivation (C3): payload `ok` true and JUNIT root `failures=0`, so 0.

CMD-JUNIT-READ:

```text
JUNIT-ROOT tests=137 failures=0 errors=0 disabled=0
JUNIT-SUITE AnalyzerItemRepair.Tests.ps1 tests=13 failures=0 skipped=0
JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0
JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
JUNIT-SUITE PackageCompatibility.Tests.ps1 tests=8 failures=0 skipped=0
JUNIT-SUITE PackageGraph.Tests.ps1 tests=32 failures=0 skipped=0
JUNIT-SUITE ProjectConsistency.Tests.ps1 tests=18 failures=0 skipped=0
JUNIT-SUITE Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0 skipped=0
JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=0 skipped=0
JUNIT-NOTPASSED: none
```

The `<testsuites ` Grep (a) matched exactly one line (line 2). The `<testsuite ` Grep (b) matched 8 lines (lines 3, 29, 56, 86, 107, 152, 183, 227), none reported as omitted. The `status="Failed"` Grep (c) returned no match.

Baseline suite counts:

```text
BASELINE-SUITE AnalyzerItemRepair.Tests.ps1=13
BASELINE-SUITE ConsistencyVerifier.Tests.ps1=14
BASELINE-SUITE DependabotConfig.Tests.ps1=17
BASELINE-SUITE PackageCompatibility.Tests.ps1=8
BASELINE-SUITE PackageGraph.Tests.ps1=32
BASELINE-SUITE ProjectConsistency.Tests.ps1=18
BASELINE-SUITE Repair-PackageManifestConsistency.Tests.ps1=31
BASELINE-SUITE RepositoryTreeConsistency.Tests.ps1=4
```

The suite counts sum to 137, equal to the JUNIT-ROOT tests figure.

COVERAGE-MEASUREMENT: deferred to the CI Pester job (_pester.yml); no local figure
COVERAGE-EXCEPTION: D8 (no local Pester coverage route instruments scripts/dependencies)

Acceptance: `ok` true; JUNIT-ROOT failures=0; exactly 8 JUNIT-SUITE lines with the expected leaf names (all but BindingRedirectVerification.Tests.ps1), each failures=0 skipped=0; `JUNIT-NOTPASSED: none` recorded after (a) matched exactly one line; the two coverage literal lines are present.

Output Summary: PoshQC test ok true; 8 suites, 137 tests, 0 failures, 0 errors, 0 skipped. Coverage is deferred to the CI Pester job under D8.
