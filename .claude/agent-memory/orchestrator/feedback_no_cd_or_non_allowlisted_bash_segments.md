---
name: feedback_no_cd_or_non_allowlisted_bash_segments
description: Never issue `cd X && ...` or grep/sed/cat/find via Bash in this repo — every compound segment must match the settings allow list (git *, pwsh *, poetry run *, three lib scripts) or the user gets a manual approval prompt
metadata:
  type: feedback
---

On 2026-09-02 the operator was interrupted by a permission prompt for a child orchestrator's
command `cd "<item worktree>" && grep -n "..." <plan.md> | sed 's/.../'` and said: "stop
sending commands that trigger manual approval. please diagnose and don't do it again".

**Why:** `.claude/settings.json` allows exactly `Bash(git *)`, `Bash(poetry run *)`,
`Bash(pwsh *)`, and `Bash(bash .claude/lib/bash/{compute-cohorts,compute-concurrency-batches,
validate-parallel-manifest}.sh*)`. The permission engine splits a Bash line on `&&`, `;`, and
`|` and requires EVERY segment to match an allow rule. `cd <path>`, `grep`, `sed`, `cat`,
`head`, `tail`, `find`, `cp`, `mv`, `rm`, and `echo` match nothing, so any line containing one
of them prompts — including `cd X && git ...`, where the `cd` segment alone is unmatched. There
is no settings.local.json and no user-level Bash allow rule to rescue these shapes.

**How to apply:**
- Never `cd`. Use `git -C <absolute worktree path> <subcommand>` for every git call and pass
  absolute paths to every tool.
- Never grep/sed/cat/head/tail/find via Bash. Use the Grep, Read, Glob, Edit, and Write tools
  with absolute paths; they work on paths outside the session cwd without prompting.
- Keep every Bash call a single command (no `&&`, `;`, `|`) whose first token is `git`,
  `pwsh`, or `poetry`, or one of the three allowed lib scripts.
- If a shell step is unavoidable, run it as ONE `pwsh -NoProfile -Command "..."` invocation with
  the worktree path inside the command string.
- Copy this block verbatim into every subagent delegation prompt (atomic-executor,
  feature-review, pr-author); their Bash goes through the same allow list.

Related: [[bash-tool-rejects-complex-commands-in-isolated-worktree]],
[[hooks-pattern-match-bash-command-text]], [[agent-worktree-hooks-resolve-to-agent-cwd]].
