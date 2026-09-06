---
name: bash-discipline-block-in-every-child-prompt
description: Every child delegation prompt must carry the verbatim BASH DISCIPLINE block — the permission engine splits compound commands and requires EVERY segment to match an allow rule, so one unmatched grep/sed/cd segment prompts the whole line
metadata:
  type: feedback
---

Put the BASH DISCIPLINE block verbatim at the top of every child delegation prompt, above the
worktree directive, and instruct the child to pass it verbatim to every subagent it delegates.

**Why:** Operator directive, 2026-09-02, run `bugs-2026-09-02`, after a child orchestrator issued
`cd "<worktree>" && grep -n ... | sed ...` and stalled the run on a manual approval prompt. The
mechanism is stronger than "a leading cd breaks the prefix match" (see
[[never-prefix-commands-with-cd]], which stated the weaker version): the permission engine
**splits a compound command on `&&`, `;`, and `|` and requires EVERY segment to match an allow
rule**. The verified allow list in `.claude/settings.json` is exactly `Bash(git *)`,
`Bash(poetry run *)`, `Bash(pwsh *)`, and the three `.claude/lib/bash/*.sh` scripts. So `cd`
matches nothing, `grep` matches nothing, `sed` matches nothing — and a single unmatched segment
prompts the entire line. There is no `settings.local.json` and no user-level Bash allow rule, so
nothing rescues these shapes. A child that pipes or chains anything stalls the run.

**How to apply.** Paste this block verbatim, above the worktree directive:

> BASH DISCIPLINE (binding): Never use `cd`. Address the item worktree with
> `git -C <absolute worktree path> ...` for every git command, and pass absolute paths to every
> other tool. Never invoke grep, sed, awk, cat, head, tail, find, cp, mv, rm, or echo through
> Bash; use the Grep, Read, Glob, Edit, and Write tools with absolute paths instead. The only
> Bash forms permitted are single commands (no `&&`, `;`, or `|` chaining) whose first token is
> `git`, `pwsh`, or `poetry`, plus the three `.claude/lib/bash/*.sh` scripts. If a shell step is
> genuinely needed, run it as one `pwsh -NoProfile -Command "..."` invocation with the worktree
> path inside the command string, never as a `cd`. Pass this block verbatim to every subagent you
> delegate (atomic-executor, feature-review, pr-author).

Two further notes:

- **`gh` is NOT on the allow list.** The block above names `git`, `pwsh`, and `poetry` only. A
  child needs `gh` for pull-request creation, so expect that one to prompt regardless; keep it a
  single bare command so the merge gate reads the right number, per
  [[issue-merge-and-removal-commands-bare]].
- **The chaining rule covers the parent too.** My own resume used `cmd && cmd` shapes and a
  `for` loop with embedded `echo`; both are the same defect seen from the parent side. Use one
  `git -C ...` call per fact, or one `pwsh -NoProfile -Command` when several facts are genuinely
  needed at once.
- **A prompt cannot be retrofitted into a running child**, and this agent has no SendMessage
  tool. Get the block into the prompt at launch; there is no later correction path short of
  stopping and relaunching the child, which is destructive when the worktree is live.
