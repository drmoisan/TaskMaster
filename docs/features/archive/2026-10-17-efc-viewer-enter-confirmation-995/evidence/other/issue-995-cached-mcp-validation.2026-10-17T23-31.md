# Issue 995 cached MCP validation

Timestamp: 2026-10-17T22-42
Command: native mcp__drm_copilot__validate_orchestration_artifacts artifact_type=orchestrator-state artifact_path=artifacts/orchestration/orchestrator-state.json require_complete=false require_codex_topology=true require_codex_model_routing=true workspace_root=current-checkout
EXIT_CODE: 1
Output Summary: Cached native MCP tool returned model mismatch errors for routing receipts 0 through 6. Each error was repeated twice.

ConfiguredPackage: @danmoisan/drm-copilot-mcp@1.1.21
ObservedCachedProcessPackage: @danmoisan/drm-copilot-mcp@1.1.20
ValidationErrors: Checkpoint codex_model_routing_receipts[0..6].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.

## Redacted MCP transaction
RedactionTimestamp: 2026-10-17T23-31
Replaces: evidence/other/issue-995-cached-mcp-validation.2026-10-17T22-42.md
RawResponseSha256: 24755af6647f46477a6ffb0b60eea0afdb3ce47dbfb11ab9d991ee44c765f6d0
RawRetention: SCRATCH/issue-995-proof-raw-issue-995-cached-mcp-validation.2026-10-17T22-42.json
Redaction: Only the host workspace root is replaced with WORKSPACE_ROOT; protocol identity, arguments, validation flags, response fields and errors remain unchanged. Complete original bytes are retained under SCRATCH.

```json
{
  "content": [
    {
      "type": "text",
      "text": "{\n  \"ok\": false,\n  \"tool\": \"validate_orchestration_artifacts\",\n  \"workspace_root\": \"WORKSPACE_ROOT\",\n  \"summary\": \"Validation failed for orchestrator-state artifact at 'artifacts/orchestration/orchestrator-state.json':\\nCheckpoint codex_model_routing_receipts[0].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[1].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[2].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[3].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[4].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[5].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[6].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[0].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[1].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[2].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[3].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[4].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[5].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\\nCheckpoint codex_model_routing_receipts[6].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\"\n}"
    }
  ],
  "structuredContent": {
    "ok": false,
    "tool": "validate_orchestration_artifacts",
    "workspace_root": "WORKSPACE_ROOT",
    "summary": "Validation failed for orchestrator-state artifact at 'artifacts/orchestration/orchestrator-state.json':\nCheckpoint codex_model_routing_receipts[0].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[1].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[2].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[3].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[4].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[5].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[6].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[0].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[1].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[2].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[3].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[4].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[5].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'.\nCheckpoint codex_model_routing_receipts[6].model must be 'gpt-5.6-terra', found 'gpt-6.1-sol'."
  },
  "isError": true
}
```
