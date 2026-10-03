# P2-T17 AC2 negative-control run (issue #973) [expect-fail]

Timestamp: 2026-10-03T11-25
Command: CMD-JUNIT-DELETE (pwsh -NoProfile -Command 'Remove-Item -LiteralPath <execution-worktree-root>/artifacts/pester/pester-junit.xml -ErrorAction SilentlyContinue'); MCP mcp__drm-copilot__run_poshqc_test workspace_root <execution-worktree-root> scan_folders ["tests/scripts/dependencies"]; CMD-JUNIT-READ over <execution-worktree-root>/artifacts/pester/pester-junit.xml with the Read tool at lines 54-63 for the three failure messages
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: run made after Parts A and B (the redirect sweep and the ADAL deletion) and before any packages.config or csproj edit; BindingRedirectVerification.Tests.ps1 tests=16 failures=3; the main It's count assertion passed and its unverifiable assertion fails naming only netstandard and System.Linq.AsyncEnumerable (no ADAL); It (a) fails with exactly the 15 System.Linq.AsyncEnumerable records; It (b) fails as before. The install half is load-bearing: the sweep alone does not make the gate green.

PRECONDITION git -C <execution-worktree-root> status --porcelain -- '*packages.config' '*.csproj': (empty)
JUNIT-DELETE: exit 0 recorded (prior document removed; not gated)

Payload (C3, C4):
{"ok": false, "tool": "run_poshqc_test", "workspace_root": "<execution-worktree-root>", "summary": "Command exited with code 3."}

JUNIT-ROOT tests=153 failures=3 errors=0 disabled=0
JUNIT-SUITE AnalyzerItemRepair.Tests.ps1 tests=13 failures=0 skipped=0
JUNIT-SUITE BindingRedirectVerification.Tests.ps1 tests=16 failures=3 skipped=0
JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0
JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
JUNIT-SUITE PackageCompatibility.Tests.ps1 tests=8 failures=0 skipped=0
JUNIT-SUITE PackageGraph.Tests.ps1 tests=32 failures=0 skipped=0
JUNIT-SUITE ProjectConsistency.Tests.ps1 tests=18 failures=0 skipped=0
JUNIT-SUITE Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0 skipped=0
JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=0 skipped=0

JUNIT-NOTPASSED Repository binding redirects (issue 953).reports exactly the recorded known-debt set and unverifiable set over every app.config against every csproj Reference
JUNIT-NOTPASSED Repository binding redirects (issue 953).bounds every corrected redirect range at its newVersion across the repository app.config files
JUNIT-NOTPASSED Repository binding redirects (issue 953).carries an Aliases child on every System.Linq.AsyncEnumerable project Reference

JUNIT-MESSAGE Expected 'netstandard', because only the deliberate netstandard redirect may be unverifiable; observed: netstandard; System.Linq.AsyncEnumerable, but got @('netstandard', 'System.Linq.AsyncEnumerable').

JUNIT-MESSAGE Expected 0, because every corrected redirect must bound its range at a csproj Reference version; observed: QuickFiler:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; QuickFiler.Test:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; Tags:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; Tags.Test:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; TaskMaster:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; TaskMaster.Test:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; TaskTree:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; TaskTree.Test:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; TaskVisualization:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; TaskVisualization.Test:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; ToDoModel:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; ToDoModel.Test:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; UtilitiesCS:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; UtilitiesCS.Test:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7; VBFunctions.Test:System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7, but got 15.

JUNIT-MESSAGE Expected strings to be the same, because exactly the five projects that install System.Linq.Async carry the aliased Reference, but they were different. / Expected length: 60 / Actual length:   0 / Strings differ at index 0. / Expected: 'QuickFiler,TaskMaster,ToDoModel,UtilitiesCS,UtilitiesCS.Test' / But was:  '' / ^ (the attribute's CRLF line breaks are shown here as " / ")

Acceptance check: main It message does not begin `Expected 0` (the count assertion passed); it begins `Expected 'netstandard', because only the deliberate netstandard redirect may be unverifiable; observed: `, contains `, but got @('netstandard', 'System.Linq.AsyncEnumerable')`, contains System.Linq.AsyncEnumerable and does not contain Microsoft.IdentityModel.Clients.ActiveDirectory (met); It (a) ends `, but got 15.` and every one of the 15 entries contains `System.Linq.AsyncEnumerable=0.0.0.0-10.0.0.7/10.0.0.7` (met); It (b) prefix and `But was:` (met); other suites equal P0-T10 with failures 0 (met).

COVERAGE-MEASUREMENT: deferred to the CI Pester job (_pester.yml); no local figure
