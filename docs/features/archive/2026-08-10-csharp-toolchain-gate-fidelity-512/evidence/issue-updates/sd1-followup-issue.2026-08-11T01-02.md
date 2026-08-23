# SD1 follow-up — GitHub issue created ([P7-T2])

Timestamp: 2026-08-11T01-02
Command: `gh issue create --title "Codex/Copilot instruction mirrors still document the CSharpier v0 command and the unpassable nullable command" --body-file docs/features/potential/2026-08-11-codex-copilot-instruction-mirrors-document-defective-csharp-toolchain-commands.md`
EXIT_CODE: 0

## Result

| Field | Value |
|---|---|
| **Issue number** | **#535** |
| **Issue URL** | **https://github.com/drmoisan/TaskMaster/issues/535** |
| Title | Codex/Copilot instruction mirrors still document the CSharpier v0 command and the unpassable nullable command |
| `PostedAs` | **body** |
| `IssueUpdatedAt` | 2026-08-11T01-02 |
| Promoted file path | `docs/features/potential/promoted/2026-08-11-codex-copilot-instruction-mirrors-document-defective-csharp-toolchain-commands.md` |

The entry file was moved from `docs/features/potential/` into
`docs/features/potential/promoted/` after creation, and its header was updated to record
`Status: Promoted -> GitHub issue #535`, `Issue: #535` and the issue URL, matching the convention of
the existing promoted entries.

## Route used

| Route | Availability | Used |
|---|---|---|
| `mcp__drm-copilot__potential_to_issue` (preferred) | **`MCP_TOOL_UNAVAILABLE: potential_to_issue`** — not in the `atomic-executor` toolset, which exposes only the four PoshQC functions | no |
| Escalation to the orchestrator (which holds the promotion MCP tools) | not reachable from an executor subagent mid-run | no |
| `gh issue create --title "..." --body-file <path>` (permitted only when both routes above are unavailable) | available (`gh auth status` -> logged in as `drmoisan`, token scopes `gist, read:org, repo, workflow`) | **YES** |

**Route actually used: `gh issue create`.** Both preferred routes were unavailable, which is the
condition the plan sets for this fallback.

## Acceptance

Prose in the feature folder is not sufficient; **the issue exists** at
https://github.com/drmoisan/TaskMaster/issues/535. `PROMOTION BLOCKED` is **not** recorded, and AC6's
follow-up obligation is **satisfied**, not remediation-required.

## Output Summary

The SD1 follow-up was promoted to GitHub issue **#535**
(https://github.com/drmoisan/TaskMaster/issues/535), posted as the issue **body**, via
`gh issue create` after both MCP routes were recorded unavailable. The source entry now lives at
`docs/features/potential/promoted/2026-08-11-codex-copilot-instruction-mirrors-document-defective-csharp-toolchain-commands.md`
with its header updated to reference #535. This issue number back-fills the
`SD1FollowUpIssue: pending` line in
`FEATURE/evidence/qa-gates/site-inventory-reconciled.2026-08-11T00-18.md` at [P7-T8].
