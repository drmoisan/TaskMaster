# Remediation cycle 1, P0-T5: PowerShell format baseline (unchanged tree)

Timestamp: 2026-10-06T20-28
Command: MCP mcp__drm-copilot__run_poshqc_format workspace_root=<execution-worktree-root> scan_folders=["scripts/dependencies","tests/scripts/dependencies"], bracketed by git -C <execution-worktree-root> hash-object --no-filters -- scripts/dependencies/BindingRedirectVerification.psm1 tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 (before and after) and git -C <execution-worktree-root> status --porcelain --untracked-files=all -- scripts/dependencies tests/scripts/dependencies
EXIT_CODE: 0

Payload (verbatim, worktree root replaced):
{"ok":true,"tool":"run_poshqc_format","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC format against '<execution-worktree-root>' with 2 selected scan folder(s)."}

Hash set before:
HASH scripts/dependencies/BindingRedirectVerification.psm1 51d6664b281cad0b6c8cd01e78c6bc8491a75862
HASH tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 bd93c01d5b587b8fe67f16f1efa3562b3f89ee7d

Hash set after:
HASH scripts/dependencies/BindingRedirectVerification.psm1 51d6664b281cad0b6c8cd01e78c6bc8491a75862
HASH tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 bd93c01d5b587b8fe67f16f1efa3562b3f89ee7d

FORMAT-REWROTE: none
PORCELAIN: (empty)
TESTFILE-LINES: 402
TESTFILE-CR: 402
MODULE-HASH-UNCHANGED: True (equals evidence/qa-gates/poshqc-format.md line 15)

Output Summary:
- PoshQC format ok true with the 2-folder summary literal (EXIT_CODE derived per plan C3).
- Hash sets before and after identical; porcelain empty: the formatter rewrote nothing on the unchanged tree.
- Test file 402 lines, 402 carriage returns (CRLF, terminated last line).
- No BASELINE-FORMAT-DRIFT.
