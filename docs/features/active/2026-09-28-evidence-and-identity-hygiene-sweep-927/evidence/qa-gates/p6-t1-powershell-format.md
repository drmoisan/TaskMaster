# P6-T1 PowerShell format pass (D11, D12)

## iter1

Timestamp: 2026-09-29T20-10
Command: hashes before (the P6-T1 HASH payload over the six hygiene files and the helper test); mcp__drm-copilot__run_poshqc_format with scan_folders ["scripts/hygiene", "tests/scripts/hygiene"]; mcp__drm-copilot__run_poshqc_format with scan_folders ["tests/scripts/vscode"] (substitution for the direct Invoke-Formatter pass over the helper test; see POSHQC-SUBSTITUTION); the P6-T1 BOM-MISSING payload; hashes after; git status --porcelain -- tests/scripts/vscode scripts/vscode
EXIT_CODE: 0
Output Summary:
- MCP channel line (hygiene folders): POSHQC MCP AVAILABLE ok=true
- MCP channel line (helper test folder): POSHQC MCP AVAILABLE ok=true
- HASH| lines before and after (seven each, identical):
  - scripts/hygiene/Test-RepositoryHygiene.Git.ps1 | EE18B28BC335D3F40BE2B536CB6CC2A6B8446D41B51AD3E4C6D7395B4D7557BE
  - scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 | 78C974D0CBF9C9ED3762625E2117170698B8506FD92A67D499B348664C8329BB
  - scripts/hygiene/Test-RepositoryHygiene.ps1 | 8A28E8F09058D5EB615F92DAA8E50A160C5FA4714AA8D1170CA0222EED3220ED
  - tests/scripts/hygiene/Test-RepositoryHygiene.Git.Tests.ps1 | A96F12B9010FE857A63A9A66A482E2A645346EBF504A265DF95BE1DD375ED67F
  - tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1 | 24F0CD100A16D08FD6051B25B5AD29A381CB95C358444FAF6A73A5AD04B3119F
  - tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1 | 059CEC80967AECA6ACDC441819E1B2A6CACFA21687ED3E1579604B62ADD554C3
  - tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 5FB058BDCD97D2E7C8BA7F10B4DE41CF6D88B629C6FC18BD20C535EE97D9DAEC
- REWRITTEN=0 (hash-differing count across the seven files)
- HELPER-TEST-REWRITTEN=0 (derived from the unchanged helper-test hash)
- BOM-MISSING=0
- Porcelain over tests/scripts/vscode and scripts/vscode printed no line: the folder-wide format pass rewrote no other file, so nothing was restored (RESTORED: none).
- EXIT_CODE 0 is the derived result: both MCP calls returned ok=true and every hash was unchanged. The MCP tool reports no numeric exit code.

POSHQC-SUBSTITUTION:
- Plan command replaced: the direct pwsh payload that runs Invoke-Formatter on the content of tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 and rewrites it only when the output differs.
- Substituted with: mcp__drm-copilot__run_poshqc_format over the narrowest scan folder containing the helper test, tests/scripts/vscode, per the binding local PowerShell gate directive. D11 warns against a folder-wide pass over that folder because the formatter can rewrite an untouched file there; the plan's own restore branch (restore any path other than the helper test and record it under RESTORED:) covers that case, and the porcelain span shows no such rewrite occurred.
- Rewrite observation is by SHA-256 before and after (D12), not by the formatter's output.
