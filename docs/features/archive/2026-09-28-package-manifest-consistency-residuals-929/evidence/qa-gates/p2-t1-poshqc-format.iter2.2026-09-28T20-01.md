# P2-T1 — PowerShell QC step 1, format (iteration 2)

Timestamp: 2026-09-30T10-55
Command: Get-FileHash -Algorithm SHA256 over every .ps1, .psm1 and .psd1 file under scripts/dependencies and tests/scripts/dependencies; MCP mcp__drm-copilot__run_poshqc_format (workspace_root <execution-worktree-root>, scan_folders ["scripts/dependencies","tests/scripts/dependencies"]); re-hash; git status --porcelain --untracked-files=all -- scripts/dependencies tests/scripts/dependencies; @(Get-Content -LiteralPath "scripts/dependencies/ConsistencyVerifier.psm1").Count
EXIT_CODE: 0
Output Summary:
- Iteration 2 follows the iteration 1 P2-T7 failure (qa-gates/p2-t7-mstest-coverage.iter1.2026-09-28T20-01.md).
- MCP payload (host prefix replaced): {"ok":true,"tool":"run_poshqc_format","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC format against '<execution-worktree-root>' with 2 selected scan folder(s)."}
- scan_folders: ["scripts/dependencies","tests/scripts/dependencies"]
- Hash sets: 14 entries before and 14 after (6 production, 8 test files); every hash identical, and identical to the iteration 1 set
- Write Set rewrite count: 0
- REVERT-SET: empty
- Post-revert porcelain over the two folders: empty
- ConsistencyVerifier.psm1 line count: 499 (equals VERIFIER-LINES)
