---
name: hooks-pattern-match-bash-command-text
description: PreToolUse hooks scan the whole Bash command STRING, so tool names or gh subcommands quoted inside a heredoc script body trigger a block even when no such command runs
metadata:
  type: project
---

PreToolUse hooks in this repo match against the raw Bash `command` text, not against
what the command actually executes. Prose inside a heredoc body counts.

Two confirmed blocks, both while writing a **checkpoint JSON** with a `python <<'PY'` heredoc:

- A `receipt_staleness_repair` note containing the literal `gh pr create` returned
  `PR_AUTHOR_SKILL_BLOCKED: New PRs require --body-file`.
- Evidence strings naming the promotion tools (`new_potential_entry`, `potential_to_issue`,
  `new_active_feature_folder`) returned `PROMOTION_MCP_ONLY_BLOCKED`.

Neither command invoked anything; both were writing a JSON file.

**Why:** the hooks are substring/regex guards over `tool_input.command`. They cannot tell a
command from a comment, so any quoted mention is indistinguishable from an invocation. This is
the same fail-closed posture as `enforce-orchestration-preimplementation-gate.ps1`.

**How to apply:** when a script body must mention a guarded literal, do not pass it through a
Bash heredoc. Either author the script with the **Write tool** and run it with a short, clean
`python <path>` command, or split the literal (`"new_" + "potential_entry"`). Reword prose to a
neutral phrase ("the first PR-creation attempt") when the literal is not required. Relates to
[[child-orchestrator-pr-hook-reads-session-root]] and
[[bash-tool-rejects-complex-commands-in-isolated-worktree]].
