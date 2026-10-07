---
name: worktree-removal-hook-matches-git-plus-remove-in-variable-names
description: enforce-epic-worktree-removal-gate.ps1 refuses any Bash command holding "git" plus the substring "remove" — including read-only payloads whose variable names or labels contain "removed"; preflight must flag such payloads
metadata:
  type: project
---

The PreToolUse hook enforce-epic-worktree-removal-gate.ps1 refuses a Bash tool call with EPIC_WORKTREE_REMOVAL_BLOCKED / TARGET_WORKTREE_NOT_DERIVABLE whenever the command text contains `git` and the substring `remove` (case-insensitive), even when the payload removes nothing. Observed on issue #959 twice: CMD-DELETE at P4-T5 (`git status` + `Remove-Item`, a real file delete) and CMD-TST-IDENTITY at P6-T13 (read-only `git diff` + variable `$removed1` and labels `TST1-REMOVED-*`).

**Why:** the hook does a substring scan of the full command string; it cannot tell a variable name from a `git worktree remove` invocation. Under the delegation HOOK RULE any block other than an enforce-promotion-mcp-only false positive halts the item, so a payload whose identifiers merely contain "remove" stops the whole run.

**How to apply:** at preflight, scan every pwsh payload that also invokes git for the substring `remove` (identifiers, labels, comments, cmdlets) and request a plan delta that renames them (e.g. `$dropped1`, `TST1-DROPPED-*`) or splits the git call out. During execution never reword a blocked payload; write a stop record, commit and push (with a commit message that itself avoids the substring), and report. See [[pwsh-payload-hook-containment]].
