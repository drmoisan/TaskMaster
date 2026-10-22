# P1-T2 — Tree consistency tests fail before the fix [expect-fail]

Timestamp: 2026-09-30T10-02
Command: pwsh -NoProfile -Command '[DateTime]::UtcNow.ToString("o")' (RUN-START); MCP mcp__drm-copilot__run_poshqc_test (workspace_root <execution-worktree-root>, scan_folders ["tests/scripts/dependencies"]); CMD-JUNIT-READ
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- RUN-START: 2026-09-30T13:24:02.1235733Z
- MCP payload (host prefix replaced): {"ok": false, "tool": "run_poshqc_test", "workspace_root": "<execution-worktree-root>", "summary": "Command exited with code 4."} (ok false, as expected)
- JUNIT-WRITTEN=2026-09-30T13:24:39.4564261Z (later than RUN-START)
- JUNIT-ROOT tests=135 failures=4 errors=0 disabled=0 (131 + 4)
- JUNIT-SUITE AnalyzerItemRepair.Tests.ps1 tests=13 failures=0 skipped=0
- JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=12 failures=0 skipped=0
- JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
- JUNIT-SUITE PackageCompatibility.Tests.ps1 tests=8 failures=0 skipped=0
- JUNIT-SUITE PackageGraph.Tests.ps1 tests=32 failures=0 skipped=0
- JUNIT-SUITE ProjectConsistency.Tests.ps1 tests=18 failures=0 skipped=0
- JUNIT-SUITE Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0 skipped=0
- JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=4 skipped=0

Not-passed test cases (exactly four, all under the Describe 'Repository tree consistency (issue 929)'):

1. JUNIT-NOTPASSED Repository tree consistency (issue 929).reports no Import element whose package the sibling manifest omits, for every project directory that carries a manifest
   JUNIT-MESSAGE Expected 0, because these Import elements name a package the sibling manifest omits: QuickFiler.Test.csproj: line 8 altcover.8.6.45; QuickFiler.Test.csproj: line 537 altcover.8.6.45, but got 2.
2. JUNIT-NOTPASSED Repository tree consistency (issue 929).names in the SVGControl binding redirects the assembly version the SVGControl project reference declares
   JUNIT-MESSAGE Expected strings to be the same, because the Fizzler redirect must name the referenced assembly version, but they were different. String lengths are both 7. Strings differ at index 4. Expected: '1.3.1.0' But was: '1.3.0.0'
3. JUNIT-NOTPASSED Repository tree consistency (issue 929).passes client-id and not app-id to the create-github-app-token step of the repair workflow
   JUNIT-MESSAGE Expected 1, because the token step must pass exactly one client-id input read from a secret, but got 0.
4. JUNIT-NOTPASSED Repository tree consistency (issue 929).instructs the maintainer to store the Client ID in the secret the repair workflow reads by name
   JUNIT-MESSAGE Expected a value, because the workflow client-id line must name the secret name the runbook has to match, but got $null or empty.

Message literal checks: message 1 contains QuickFiler.Test.csproj and altcover.8.6.45; message 2 contains 1.3.0.0 and 1.3.1.0; message 3 contains client-id; message 4 contains secret name. No failure occurred in any other suite.

GATE-SUBSTITUTION: JUnit per-file counts stand in for a direct Pester run
