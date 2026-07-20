---
name: potential-to-issue-needs-absolute-path
description: mcp potential_to_issue rejected a workspace-relative potential_path with "not found"; the same absolute path worked
metadata:
  type: feedback
---

`mcp__drm-copilot__potential_to_issue` returned `Potential file not found` when given a workspace-relative `potential_path` (e.g. `docs/features/potential/2026-07-18-<slug>.md`) even though the file existed and `workspace_root` was set to the worktree. Passing the FULL absolute path to the same file succeeded.

**Why:** the tool's path resolution did not join `potential_path` against `workspace_root` for this call; the relative form resolved against a different base.

**How to apply:** when calling `potential_to_issue` (and likely other promotion MCP tools that take a file path) from an isolated worktree, pass the absolute path from the `new_potential_entry` receipt's `artifacts[0]`, not a workspace-relative path. Related: [[promotion-potential-md-may-not-persist]], [[potential-to-issue-creates-github-issue]].
