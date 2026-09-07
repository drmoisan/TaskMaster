# [P4-T23] Follow-up Issue Mirror - QfcFormController.Cleanup() disposal ordering defect

Timestamp: 2026-08-26T10-49

Task: [P4-T23]
Feature: docs/features/active/quickfiler-bug-family-446
Promotion type: bug
Work mode: minor-audit

## MCP Calls

Tool: mcp__drm-copilot__new_potential_bug_entry
Result: ok=true
Raw payload:

```json
{"ok":true,"tool":"mcp__drm-copilot__new_potential_bug_entry","workspace_root":"REDACTED-REPO-ROOT","summary":"Created a new potential bug entry for 'qfcformcontroller-cleanup-disposal-ordering'.","artifacts":["REDACTED-REPO-ROOT/docs/features/potential/2026-08-26-qfcformcontroller-cleanup-disposal-ordering.md"]}
```

Potential-entry path returned by the potential-entry tool:
`docs/features/potential/2026-08-26-qfcformcontroller-cleanup-disposal-ordering.md`

Tool: mcp__drm-copilot__potential_to_issue
Result: ok=true
Raw payload:

```json
{"ok":true,"tool":"potential_to_issue","workspace_root":"REDACTED-REPO-ROOT","summary":"Promoted 'REDACTED-REPO-ROOT/docs/features/potential/2026-08-26-qfcformcontroller-cleanup-disposal-ordering.md' as a bug workflow in minor-audit mode.","artifacts":["https://github.com/drmoisan/TaskMaster/issues/621"],"destination_path":"REDACTED-REPO-ROOT/docs/features/potential/promoted/2026-08-26-qfcformcontroller-cleanup-disposal-ordering.md","target_repository":"drmoisan/TaskMaster"}
```

## GitHub Issue

- Issue number: 621
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/621
- Source of the number and URL: taken directly from the `artifacts` field of the
  `mcp__drm-copilot__potential_to_issue` result payload quoted above. It was not inferred from a
  generated issue file and not read back from any other source.
- Promoted record retained at: `docs/features/potential/promoted/2026-08-26-qfcformcontroller-cleanup-disposal-ordering.md`

## Issue Body Summary

QfcFormController.SetupDisposal.cs:216-220 disposes _undoQueue and nulls _globals and _groups without cancelling or awaiting _undoConsumerTask, racing an in-flight consumer iteration during teardown.

## Dropped-Section Restoration

The promotion tool mapped `Summary`, `Environment`, `Steps to Reproduce`, `Expected Behavior`, `Actual Behavior`, `Logs / Screenshots` and `Impact / Severity` into the issue body, but dropped `Suspected Cause / Notes` and `Proposed Fix / Validation Ideas`. Those two sections carry the #446 provenance and the remediation ideas, so they were restored as an issue comment rather than left lost.

- Comment URL: https://github.com/drmoisan/TaskMaster/issues/621#issuecomment-5427083465

## Path Redaction Note

The two payloads above are reproduced verbatim except that the absolute workspace path was replaced
with `REDACTED-REPO-ROOT`. The repository artifact-hygiene rule forbids absolute host paths, account
names and machine names in any committed artifact, and the raw payloads embed the absolute worktree
path in `workspace_root`, `artifacts` and `destination_path`. No other character was altered; the
`ok`, `tool`, `summary`, `artifacts`, `destination_path` and `target_repository` fields are otherwise
as returned.

EXIT_CODE: 0

Output Summary: Both MCP calls returned ok=true. The potential entry was created at
`docs/features/potential/2026-08-26-qfcformcontroller-cleanup-disposal-ordering.md`, promoted to bug issue #621 (https://github.com/drmoisan/TaskMaster/issues/621) in minor-audit mode, and the promoted record was
retained at `docs/features/potential/promoted/2026-08-26-qfcformcontroller-cleanup-disposal-ordering.md`. Verified on disk that the promoted record exists and the pre-promotion original
no longer remains under `docs/features/potential/`.
