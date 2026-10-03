---
name: removal-gates-false-positive-on-readonly-pwsh-payloads
description: Both worktree-removal gates deny plan-mandated read-only pwsh payloads that contain no worktree removal; neither is covered by a standing approval, so each block halts the item and goes to the coordinator
metadata:
  type: project
---

On run `bugs-2026-09-28` (2026-10-03) two read-only plan payloads were denied by worktree-removal hooks
even though neither contains `git worktree remove`:

- 959 P4-T5 CMD-DELETE: `enforce-epic-worktree-removal-gate.ps1`, `TARGET_WORKTREE_NOT_DERIVABLE`. The payload
  deletes one file and reads porcelain status.
- 973 P3-T24 CMD-VERBATIM-MOVE: `enforce-parallel-worktree-removal-gate.ps1`, `TARGET_WORKTREE_NOT_DERIVABLE`
  with an empty path. The payload runs only `git -C $repo show` and `git -C $repo diff`, and it defines
  variables named `$removed*`. The suspected cause is that `Test-CommandLineInvocation` misreads a
  wrapper-led pwsh segment. This has not been confirmed.

**Why:** a plan that contains `git` together with a remove-like token inside one pwsh string can trip these
gates. The children correctly refuse to reword the command, and a hook bypass is user-granted only, so
every hit costs a halt plus a round trip to the coordinator.

**How to apply:** when a plan's command catalogue holds a pwsh payload that combines `git` with the
substrings `remove`, `delete` or `worktree`, flag it before launch rather than after the halt. Do not
reword the payload yourself. Record each block in `items[].execution_halt` together with three options:
the coordinator runs the payload unchanged, a plan revision with approval, or an upstream guard fix.
Both hooks are push-down-owned. See [[children-rephrase-edits-past-hooks]] and
[[hook-bypass-is-always-one-time]].
