---
name: epic-worktree-removal-hook-fires-on-plain-file-delete
description: The epic worktree-removal PreToolUse:Bash gate refuses a pwsh payload that deletes one source file with Remove-Item and also runs git status; plans that delete files hit it
metadata:
  type: project
---

Observed on #959 (2026-10-03, parallel run bugs-2026-09-28, task P4-T5). The plan's `CMD-DELETE`
payload was one `pwsh -NoProfile -Command` that ran `Test-Path`, `Remove-Item -LiteralPath <file>`,
`Test-Path` and `git status --porcelain -- <file>`. No `git worktree` command was present. The hook
refused it with `EPIC_WORKTREE_REMOVAL_BLOCKED: TARGET_WORKTREE_NOT_DERIVABLE` ("git worktree remove
for '' requires ... an epic checkpoint ... or a parallel-orchestrator checkpoint"). The likely trigger
is the command text containing both `git` and `Remove`; this was not tested.

**Why:** the run's HOOK RULE forbids rewording a hook-blocked command, and the standing approval
covered only enforce-promotion-mcp-only false positives, so the item halted at 45/111 tasks inside
an open compile-red span.

**How to apply:** at preflight, flag any plan payload that combines `Remove-Item` (or `rm`) with a
`git` invocation in the same Bash tool call, and have the planner split the git observation into a
separate `git -C` call before execution starts. Changing it after a block is a reword the HOOK RULE
forbids. See [[hooks-pattern-match-bash-command-text]].
