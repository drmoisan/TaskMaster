---
name: mcp-promotion-route-plan-seams
description: Follow-up-issue tasks routed through the MCP promotion path have three planner traps - a separate bug entry point, two required promotion arguments, and untracked artifacts under docs/features/potential/ that pathspec-scoped clean-tree gates miss
metadata:
  type: project
---

`gh issue create` / `gh issue new` are denied by the PreToolUse hook `.claude/hooks/enforce-promotion-mcp-only.ps1`, so every follow-up-issue task must route through the MCP promotion path. Three seams that preflight catches on that route:

1. **There are two entry points, not one.** `.claude/skills/feature-promotion-lifecycle/SKILL.md:24` defines a distinct BUG entry point alongside the feature one. A task filing a *defect* must name the bug tool; naming the feature tool for a defect is a blocking finding. Classify each follow-up by what it actually is: a behavioural defect is a bug, pre-existing file-size or debt cleanup is a feature.
2. **The promotion call requires two arguments.** `SKILL.md:25` requires `promotion_type` and `work_mode` on the `potential_to_issue` call. A plan that names the tool without them is incomplete.
3. **The route writes files the clean-tree gate does not stage.** It creates a potential entry under `docs/features/potential/` and a promoted copy under `docs/features/potential/promoted/`. Every clean-tree gate is pathspec-scoped (correctly, because `.claude/agent-memory/**` is tracked and dirty — see [[agent-memory-is-tracked-scope-git-gates]]), and a feature-folder pathspec does NOT cover `docs/features/potential`. Add it as a second pathspec to BOTH the `git add` and the `git status --porcelain` of the committing task, or the new files strand in the worktree at plan end.

**Do not assert an undocumented return value.** The lifecycle skill documents that tool's INPUTS only and never documents a return shape. The observed behaviour is a promotion summary plus a destination path, with the issue URL read from the generated issue file rather than from the tool payload. An acceptance that asserts "the GitHub issue URL returned by `potential_to_issue`" claims a return the tool is not documented to produce. Write the acceptance to accept the URL from either the payload or the generated issue file, with the source stated explicitly in the mirror.

**Why:** all four points were blocking findings against the #446 family plan at preflight round 3 (2026-08-25); points 1 and 2 were verified directly against `feature-promotion-lifecycle/SKILL.md:24-25`.

**How to apply:** whenever a plan carries follow-up-issue tasks. An MCP-tool task also has no exit code, so its evidence artifact needs the `Timestamp:`/`Tool:`/`Result:`/`Output Summary:` variant rather than `Command:`/`EXIT_CODE:`. See [[project_446_quickfiler_bug_family_plan_seams]].
