# R-1 Follow-Up Promotion COMPLETED (supersedes the P5-T12 POSTING BLOCKED artifact)

Timestamp: 2026-08-27T14-53
Task: [P5-T12] — completed by the orchestrator, not by the executor
Command: mcp__drm-copilot__new_potential_bug_entry -> mcp__drm-copilot__potential_to_issue
EXIT_CODE: 0
Output Summary: The R-1 follow-up bug was promoted through the two-step MCP promotion chain and
exists as GitHub issue #648, state OPEN. The sibling artifact
`issue-r1-followup.2026-08-27T12-13.md` recorded `EXIT_CODE: BLOCKED` because the executor session
did not expose the promotion MCP tools; the orchestrator session does expose them, so the block was
resolved rather than accepted. That earlier artifact is retained unedited as the audit record of the
block.

PostedAs: body
IssueUrl: https://github.com/drmoisan/TaskMaster/issues/648
IssueNumber: 648
IssueState: OPEN
CommentUrl: https://github.com/drmoisan/TaskMaster/issues/648#issuecomment-5440929711

## Why the executor was blocked and the orchestrator was not

The executor's tool set exposed only the four `run_poshqc_*` MCP tools, so neither
`mcp__drm-copilot__new_potential_bug_entry` nor `mcp__drm-copilot__potential_to_issue` was callable
there, and `.claude/hooks/enforce-promotion-mcp-only.ps1` correctly denies the `gh issue create`
fallback. The executor took the task's authorized blocked branch and preserved the intended title and
body verbatim. The orchestrator session exposes the full promotion tool set, so it ran the chain. No
`gh issue create` was used at any point.

## Raw MCP receipt payloads

### Step 1 — potential bug entry

```json
{
  "ok": true,
  "tool": "new_potential_bug_entry",
  "workspace_root": "<repo-root>",
  "summary": "Created a new potential bug entry for 'wpfuidispatchertests-ungated-static-swap'.",
  "artifacts": [
    "<repo-root>/docs/features/potential/2026-08-27-wpfuidispatchertests-ungated-static-swap.md"
  ]
}
```

### Step 2 — promotion to issue

```json
{
  "ok": true,
  "tool": "potential_to_issue",
  "workspace_root": "<repo-root>",
  "summary": "Promoted '<repo-root>/docs/features/potential/2026-08-27-wpfuidispatchertests-ungated-static-swap.md' as a bug workflow in minor-audit mode.",
  "artifacts": ["https://github.com/drmoisan/TaskMaster/issues/648"],
  "destination_path": "<repo-root>/docs/features/potential/promoted/2026-08-27-wpfuidispatchertests-ungated-static-swap.md",
  "target_repository": "drmoisan/TaskMaster"
}
```

## Promotion integrity checks

- `docs/features/potential/promoted/2026-08-27-wpfuidispatchertests-ungated-static-swap.md` exists
  after promotion (the promoted record was retained, per `feature-promotion-lifecycle` step 4b).
- `docs/features/potential/2026-08-27-wpfuidispatchertests-ungated-static-swap.md` no longer exists;
  a source resolved directly under `docs/features/potential/` is MOVED by the tooling, which is the
  documented behavior for that source location.
- No active feature folder was created. This is a follow-up capture only; issue #648 is left for
  independent scheduling and `new_active_feature_folder` was deliberately not called.
- Work mode recorded on the issue: `minor-audit`, matching the bounded single-test-file scope.

## Fidelity note on the promotion mapping

`potential_to_issue` maps a fixed subset of potential-entry headings into the issue body and drops
the remainder without warning. Sections `## Suspected Cause / Notes`,
`## Proposed Fix / Validation Ideas` and `## Next Step` were dropped. The proposed fix had been
deliberately restated inside `## Summary`, so the actionable content did survive into the body; the
three dropped sections were additionally posted verbatim as the comment linked above. Verified by
reading the live issue body back with `gh issue view 648 --json body`.

## Relationship to this feature's acceptance criteria

None of AC-1 through AC-10 depends on this promotion. Issue #648 captures accepted residual risk R-1,
which `spec.md` § Risks & Mitigations records as out of scope for #493 because
`WpfUiDispatcherTests.cs` is not in the owned file set. AC-2 and AC-4 are both scoped by their own
wording to "owned files", so the existence of an ungated mutator in an unowned file does not
contradict either criterion.
