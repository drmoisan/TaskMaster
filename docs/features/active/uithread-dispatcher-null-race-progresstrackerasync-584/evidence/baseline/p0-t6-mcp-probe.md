# P0-T6 — MCP validator availability probe

Timestamp: 2026-09-03T08-20

Command:
```text
mcp__drm-copilot__validate_orchestration_artifacts
  artifact_type: "plan"
  artifact_path: "docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md"
```

EXIT_CODE: non-zero (tool invocation error; no exit code is returned by the MCP transport)

## Output Summary

MCP VALIDATOR UNAVAILABLE

Verbatim error text returned by the tool call:

```text
Error: No such tool available: mcp__drm-copilot__validate_orchestration_artifacts
```

The `drm-copilot` MCP server is reachable from this executing agent's session, but it exposes only
the four PoshQC tools (`run_poshqc_format`, `run_poshqc_analyze`, `run_poshqc_test`,
`run_poshqc_analyze_autofix`). `validate_orchestration_artifacts` is not among the tools available to
this session, so the probe could not be completed.

Per this task's own text, an unavailable validator is recorded and execution continues; this task
never halts the plan. The delegating orchestrator separately reported having run
`validate_orchestration_artifacts` against this same plan file after the revision-round-14
re-anchoring delta and having received `{"ok":true}`. That report is recorded here as a statement
received from the caller, not as an observation made by this agent.
