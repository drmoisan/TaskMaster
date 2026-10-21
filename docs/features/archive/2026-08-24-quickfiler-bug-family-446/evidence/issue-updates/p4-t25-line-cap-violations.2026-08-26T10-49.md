# [P4-T25] Follow-up Issue Mirror - Pre-existing 500-line cap violations in QuickFiler

Timestamp: 2026-08-26T10-49

Task: [P4-T25]
Feature: docs/features/active/quickfiler-bug-family-446
Promotion type: feature
Work mode: minor-audit

## MCP Calls

Tool: mcp__drm-copilot__new_potential_entry
Result: ok=true
Raw payload:

```json
{"ok":true,"tool":"mcp__drm-copilot__new_potential_entry","workspace_root":"REDACTED-REPO-ROOT","summary":"Created a new potential entry for 'quickfiler-500-line-cap-violations'.","artifacts":["REDACTED-REPO-ROOT/docs/features/potential/2026-08-26-quickfiler-500-line-cap-violations.md"]}
```

Potential-entry path returned by the potential-entry tool:
`docs/features/potential/2026-08-26-quickfiler-500-line-cap-violations.md`

Tool: mcp__drm-copilot__potential_to_issue
Result: ok=true
Raw payload:

```json
{"ok":true,"tool":"potential_to_issue","workspace_root":"REDACTED-REPO-ROOT","summary":"Promoted 'REDACTED-REPO-ROOT/docs/features/potential/2026-08-26-quickfiler-500-line-cap-violations.md' as a feature workflow in minor-audit mode.","artifacts":["https://github.com/drmoisan/TaskMaster/issues/623"],"destination_path":"REDACTED-REPO-ROOT/docs/features/potential/promoted/2026-08-26-quickfiler-500-line-cap-violations.md","target_repository":"drmoisan/TaskMaster"}
```

## GitHub Issue

- Issue number: 623
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/623
- Source of the number and URL: taken directly from the `artifacts` field of the
  `mcp__drm-copilot__potential_to_issue` result payload quoted above. It was not inferred from a
  generated issue file and not read back from any other source.
- Promoted record retained at: `docs/features/potential/promoted/2026-08-26-quickfiler-500-line-cap-violations.md`

## Issue Body Summary

QfcCollectionController.cs (2349), QfcFormControllerTests.cs (827) and QfcQueue.cs (610) all exceed the repository 500-line cap.

## Section Mapping

The feature template mapped completely: `Problem / Why`, `Implementation Intent`, `Acceptance Criteria`, `Dependencies / Risks`, `Verification Steps` and `Evidence Checklist` all appear in the issue body. No section was dropped, so no restoration comment was needed.

## Path Redaction Note

The two payloads above are reproduced verbatim except that the absolute workspace path was replaced
with `REDACTED-REPO-ROOT`. The repository artifact-hygiene rule forbids absolute host paths, account
names and machine names in any committed artifact, and the raw payloads embed the absolute worktree
path in `workspace_root`, `artifacts` and `destination_path`. No other character was altered; the
`ok`, `tool`, `summary`, `artifacts`, `destination_path` and `target_repository` fields are otherwise
as returned.

EXIT_CODE: 0

Output Summary: Both MCP calls returned ok=true. The potential entry was created at
`docs/features/potential/2026-08-26-quickfiler-500-line-cap-violations.md`, promoted to feature issue #623 (https://github.com/drmoisan/TaskMaster/issues/623) in minor-audit mode, and the promoted record was
retained at `docs/features/potential/promoted/2026-08-26-quickfiler-500-line-cap-violations.md`. Verified on disk that the promoted record exists and the pre-promotion original
no longer remains under `docs/features/potential/`.
