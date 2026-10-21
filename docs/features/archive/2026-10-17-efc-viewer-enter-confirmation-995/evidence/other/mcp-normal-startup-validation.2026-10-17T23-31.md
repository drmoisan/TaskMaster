# Issue 995 normal published MCP startup validation

Timestamp: 2026-10-17T23-03
Command: node SCRATCH/issue-995-mcp-client-normal.mjs validate_orchestration_artifacts SCRATCH/issue-995-normal-validation-args.json SCRATCH/issue-995-normal-validation-result.json
EXIT_CODE: 0
Output Summary: Normal npx startup using the default npm cache initialized drmCopilotExtension version 1.1.21. Native MCP validate_orchestration_artifacts accepted the canonical checkpoint with require_codex_topology and require_codex_model_routing true. No validation errors; isError=false and ok=true. No custom cache environment was used.



## Redacted MCP transaction
RedactionTimestamp: 2026-10-17T23-31
Replaces: evidence/other/mcp-normal-startup-validation.2026-10-17T23-03.md
RawResponseSha256: 1c1eeacb5b63ae26a6868b896d3a2e403f470c907f62c45c6c316a249b869a8e
RawRetention: SCRATCH/issue-995-proof-raw-mcp-normal-startup-validation.2026-10-17T23-03.json
Redaction: Only the host workspace root is replaced with WORKSPACE_ROOT; protocol identity, arguments, validation flags, response fields and errors remain unchanged. Complete original bytes are retained under SCRATCH.

```json
{
  "timestamp": "2026-10-17T23:03:05.561Z",
  "connection": {
    "transport": "stdio",
    "package": "@danmoisan/drm-copilot-mcp@1.1.21",
    "server": {
      "name": "drmCopilotExtension",
      "version": "1.1.21"
    }
  },
  "tool": "validate_orchestration_artifacts",
  "arguments": {
    "artifact_path": "artifacts/orchestration/orchestrator-state.json",
    "require_codex_topology": true,
    "artifact_type": "orchestrator-state",
    "require_codex_model_routing": true,
    "workspace_root": "WORKSPACE_ROOT",
    "require_complete": false
  },
  "result": {
    "content": [
      {
        "type": "text",
        "text": "{\n  \"ok\": true,\n  \"tool\": \"validate_orchestration_artifacts\",\n  \"workspace_root\": \"WORKSPACE_ROOT\",\n  \"summary\": \"Validated orchestrator-state artifact at 'artifacts/orchestration/orchestrator-state.json'.\"\n}"
      }
    ],
    "structuredContent": {
      "ok": true,
      "tool": "validate_orchestration_artifacts",
      "workspace_root": "WORKSPACE_ROOT",
      "summary": "Validated orchestrator-state artifact at 'artifacts/orchestration/orchestrator-state.json'."
    },
    "isError": false
  }
}
```
