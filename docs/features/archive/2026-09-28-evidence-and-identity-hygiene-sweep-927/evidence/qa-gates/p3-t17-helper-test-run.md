# P3-T17 Helper test run (Invoke-MSTestWithCoverage.Helpers.Tests.ps1)

Timestamp: 2026-09-29T19-52
Command: mcp__drm-copilot__run_poshqc_test with workspace_root <repo-root> and scan_folders ["tests/scripts/vscode"]; then a read-only derivation over artifacts/pester/pester-junit.xml (see POSHQC-SUBSTITUTION below)
EXIT_CODE: 0
Output Summary:
- PoshQC MCP result: ok=true (summary: ran the bundled PoshQC test with 1 selected scan folder).
- Derived from pester-junit.xml (written 2026-09-29T19-51, 20 seconds before the derivation read the clock, so it belongs to this run): testsuites tests=211 failures=0 errors=0 disabled=0; testcases=211, FAILED=0, SKIPPED=0.
- Test cases attributed to the helper test file: 20.
- IT| ConvertTo-KoverageCoberturaXml.strips active and stale TaskMaster roots while preserving already relative paths | Passed
- IT| ConvertTo-KoverageCoberturaXml.normalizes stale TaskMaster roots before merging duplicate production class entries | Passed
- Derived counts line (equivalent of the plan's PESTER counts line, derived rather than reported): Passed=211 Failed=0 Skipped=0 Total=211.
- EXIT_CODE 0 is the derived verdict: the MCP tool returned ok=true and the JUnit document carries zero failures and zero errors. The MCP tool does not report a numeric exit code.
- Project-file porcelain (git status --porcelain -- "*.csproj") before and after the run: both empty, identical.

POSHQC-SUBSTITUTION:
- Plan command: PESTER-HYGIENE with Run.Path set to the single file tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 (a direct Invoke-Pester run).
- Substituted with: mcp__drm-copilot__run_poshqc_test over the narrowest scan folder containing that file, tests/scripts/vscode, per the binding local PowerShell gate directive. That folder holds more test files than the one the plan names, so the run covered 211 test cases, of which 20 belong to the helper test file.
- Derivation: the IT| lines, the pass/fail counts and the statuses were read from artifacts/pester/pester-junit.xml (JUnit testcase name and failure/skipped children). That document and artifacts/pester/powershell-coverage.xml are gitignored raw documents carrying absolute host paths; neither is committed or copied into the evidence tree. Only the derived lines above are recorded.
