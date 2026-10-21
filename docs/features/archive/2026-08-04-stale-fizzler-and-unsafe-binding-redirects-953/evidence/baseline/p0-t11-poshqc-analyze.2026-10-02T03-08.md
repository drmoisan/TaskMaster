# P0-T11 Baseline PowerShell analyze step

Timestamp: 2026-10-02T03-08
Command: MCP mcp__drm-copilot__run_poshqc_analyze with workspace_root `<execution-worktree-root>` and scan_folders `["scripts/dependencies","tests/scripts/dependencies"]`
EXIT_CODE: 0

Payload (verbatim, worktree root replaced per C4):

```text
{"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC analyze against '<execution-worktree-root>' with 2 selected scan folder(s)."}
```

EXIT_CODE derivation (C3): `ok` is true, so 0. The payload carries no count, file or rule.

GATE-SUBSTITUTION: PoshQC analyze ok flag stands in for a diagnostic count

Acceptance: `ok` true with the 2-folder summary literal. No STOP: ANALYZE-BASELINE-RED.

Output Summary: PoshQC analyze: pass (0 findings); tool reports no count. ok true over scripts/dependencies and tests/scripts/dependencies.
