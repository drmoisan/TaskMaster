---
name: malformed-pwsh-payload-surfaces-as-unrelated-hook-block
description: An unterminated quote in a pwsh -Command payload can be rejected by an unrelated PreToolUse hook (e.g. EPIC_WORKTREE_REMOVAL_BLOCKED with an empty path) instead of a syntax error — do not read that as real repository state
metadata:
  type: project
---

A `pwsh -NoProfile -Command '...'` payload whose closing single quote is missing does not come back as
a PowerShell parse error. The Bash tool hands the mangled string to the hook layer, and a hook can
match on it and refuse the call. Observed on 2026-09-13 during item 871 Phase 2: a payload missing its
final `'` returned

```
EPIC_WORKTREE_REMOVAL_BLOCKED: git worktree remove for '' requires either an epic checkpoint ...
```

The command had nothing to do with worktrees, no worktree was removed, and the empty `''` in the
message is the tell: the hook parsed no path because there was no path to parse.

**Why:** the diagnostic names a governance gate rather than the actual defect, so the natural next
move is to go hunting for a checkpoint or a stale epic record that has no bearing on the problem. That
is a long detour from a one-character fix.

**How to apply:** when a hook refusal names a resource your command never mentioned, or quotes an
empty path, re-read your own payload for balanced quotes before investigating the hook. Fix the
quoting and re-run; the refusal disappears. Related: [[pwsh-command-quoting-boundary]],
[[pwsh-nested-quotes-in-subexpression-fail-to-parse]].
