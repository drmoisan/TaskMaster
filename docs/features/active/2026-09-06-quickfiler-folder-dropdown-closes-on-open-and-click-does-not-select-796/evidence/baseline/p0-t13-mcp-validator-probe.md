# P0-T13 — Plan validator MCP tool probe

Timestamp: 2026-09-07T14-15
Task: [P0-T13]
Issue: #796

Intended invocation: `mcp__drm-copilot__validate_orchestration_artifacts` with
`artifact_type: "plan"` and `artifact_path` set to
docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md

VALIDATOR NOT RUN: tool absent from this session's tool surface

## Enumeration of the drm-copilot MCP tools actually available to this session

- `mcp__drm-copilot__run_poshqc_format`
- `mcp__drm-copilot__run_poshqc_analyze`
- `mcp__drm-copilot__run_poshqc_analyze_autofix`
- `mcp__drm-copilot__run_poshqc_test`

`mcp__drm-copilot__validate_orchestration_artifacts` is not among them, so the
invocation could not be attempted. The absence is recorded rather than worked around;
no substitute validator was run and no human-readable summary is offered in its place.

## Incidental observation

While establishing the available tool surface, one read-only
`mcp__drm-copilot__run_poshqc_analyze` call was issued against this worktree scoped
to the feature folder. It returned `ok: true` with the summary
`Ran bundled PoshQC analyze against ... with 1 selected scan folder(s)`. PoshQC
analyze is read-only and rewrites no tracked file; the P0-T14 porcelain capture that
follows this task is the record of the tree state after it. This call is not offered
as a substitute for the plan validator and satisfies no acceptance condition.

EXIT_CODE: not applicable; the tool was not invoked.

Output Summary: The plan validator MCP tool is absent from this session's tool
surface. This is a record-and-continue probe and not a halt gate, so execution
proceeds to P0-T14.
