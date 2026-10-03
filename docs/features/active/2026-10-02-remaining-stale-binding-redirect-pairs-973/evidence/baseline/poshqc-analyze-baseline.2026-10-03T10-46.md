# P0-T9 PowerShell analyze baseline (issue #973)

Timestamp: 2026-10-03T10-46
Command: MCP mcp__drm-copilot__run_poshqc_analyze workspace_root <execution-worktree-root> scan_folders ["scripts/dependencies","tests/scripts/dependencies"]
EXIT_CODE: 0
Output Summary: analyze payload ok true with the 2-folder summary literal. No ANALYZE-BASELINE-RED.

Payload (C3, C4):
{"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC analyze against '<execution-worktree-root>' with 2 selected scan folder(s)."}

GATE-SUBSTITUTION: PoshQC analyze ok flag stands in for a diagnostic count
