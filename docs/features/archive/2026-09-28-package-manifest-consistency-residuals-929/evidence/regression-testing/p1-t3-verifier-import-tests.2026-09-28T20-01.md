# P1-T3 — Two Import-kind verifier tests and sibling comment updates

Timestamp: 2026-09-30T10-04
Command: Edit tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1; pwsh -NoProfile -Command '[DateTime]::UtcNow.ToString("o")' (RUN-START); MCP mcp__drm-copilot__run_poshqc_test (workspace_root <execution-worktree-root>, scan_folders ["tests/scripts/dependencies"]); CMD-JUNIT-READ; Select-String -SimpleMatch for the four stale phrases; Select-String for test names matching AC followed by a digit; @(Get-Content).Count
EXIT_CODE: 1
ExpectedExitCode: 1
WORKER: atomic-executor edited the file inline; no sub-agent dispatch tool is available to this executor, so the powershell-typed-engineer hand-off was performed within exactly the P1-T3 bounds.
Output Summary:
- Edits: the comment at lines 57 to 59 now states that the fixture reproduces the shape QuickFiler.Test/QuickFiler.Test.csproj carried at lines 8 and 537 until issue 929 removed both imports, keeps the no-hard-coded-exception sentence, and drops the "Tracked separately" sentence; the comment formerly at line 275 now reads "the shape QuickFiler.Test carried before issue 929"; two It blocks were added in the Context 'Package absent from the manifest' after the It block that began at line 228:
  - 'reports an Import whose package the manifest does not declare, with Kind Import'
  - 'reports no Import finding when the manifest declares the imported package'
- RUN-START: 2026-09-30T13:25:29.0090061Z
- MCP payload (host prefix replaced): {"ok": false, "tool": "run_poshqc_test", "workspace_root": "<execution-worktree-root>", "summary": "Command exited with code 4."}
- JUNIT-WRITTEN=2026-09-30T13:26:06.8242883Z (later than RUN-START)
- JUNIT-ROOT tests=137 failures=4 errors=0 disabled=0
- JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0 (12 existing plus 2)
- JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=4 skipped=0 (red by design until P1-T4 to P1-T9; decision D13)
- Every other suite at failures=0: AnalyzerItemRepair 13, DependabotConfig 17, PackageCompatibility 8, PackageGraph 32, ProjectConsistency 18, Repair-PackageManifestConsistency 31
- The four JUNIT-NOTPASSED lines all name the RepositoryTreeConsistency tests
- Stale phrases ("carries a live instance", "live shape", "lines 8 and 514", "Tracked separately") over the file: 0 lines
- Test names matching AC followed by a digit: 0
- File line count: 337 (at most 500)

GATE-SUBSTITUTION: JUnit per-file counts stand in for a direct Pester run
