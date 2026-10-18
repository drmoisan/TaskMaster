# Issue 995 published MCP validation

Timestamp: 2026-10-17T22-42
Command: node SCRATCH/issue-995-mcp-client.mjs validate_orchestration_artifacts SCRATCH/issue-995-validator-arguments.json SCRATCH/issue-995-validator-response.json
EXIT_CODE: 0
Output Summary: Actual MCP server drmCopilotExtension 1.1.21 returned ok=true for the orchestrator checkpoint with Codex topology and model routing required.

Transport: MCP SDK StdioClientTransport; initialize followed by tools/call. No direct validator code was imported.
PublishedPackage: @danmoisan/drm-copilot-mcp@1.1.21
ServerName: drmCopilotExtension
ServerVersion: 1.1.21
ValidationErrors: none

DelegationAttestation: atomic-executor-c3; model gpt-6.1-sol; reasoning high; routing_valid=true; launch_authority_valid=true; provenance_valid=true.
AttestationKey: 8017dd03b94c608710d7b8334308661156a6f9a5ffda1be2669c13b32bb429fe
AgentId: 01a14c06-2cf2-7bd0-a257-420480df4c5f
ProfileSha256: 2460c2bdf20fa8b88136c16492ee3a810441f60be697432b1975882e8832d46a

## Redacted MCP transaction
RedactionTimestamp: 2026-10-17T23-31
Replaces: evidence/other/issue-995-published-mcp-validation.2026-10-17T22-42.md
RawResponseSha256: 06f4a18b4f65877cb0d6c2d2a2f0dafa0b067a266d00f50a48dae4dfb995f295
RawRetention: SCRATCH/issue-995-proof-raw-issue-995-published-mcp-validation.2026-10-17T22-42.json
Redaction: Only the host workspace root is replaced with WORKSPACE_ROOT; protocol identity, arguments, validation flags, response fields and errors remain unchanged. Complete original bytes are retained under SCRATCH.

```json
{
  "timestamp": "2026-10-17T22:41:57.224Z",
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
    "artifact_type": "orchestrator-state",
    "artifact_path": "artifacts/orchestration/orchestrator-state.json",
    "workspace_root": "WORKSPACE_ROOT",
    "require_complete": false,
    "require_codex_topology": true,
    "require_codex_model_routing": true
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
