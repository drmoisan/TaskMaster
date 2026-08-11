---
name: bash-tool-rejects-complex-commands-in-isolated-worktree
description: In a .claude/worktrees/agent-<id> worktree the Bash tool refuses compound commands it cannot statically prove stay in-worktree (heredoc + redirect + git); build such files with Write or separate plain commands
metadata:
  type: project
---

Inside an isolated `.claude/worktrees/agent-<id>` worktree, the Bash tool statically inspects each command and
**refuses to run** anything it cannot prove stays within the worktree, with:

> "this command is too complex to verify that it stays inside the worktree; break it into plain, separate commands.
> Refusing to run it — a worktree-isolated agent's git operations must target its own worktree."

Verified 2026-08-11 (issue #457, epic `build-ci-coverage-gate-fidelity`). The rejected form was a single command
that assigned shell variables, ran several `git diff`/`git log` calls inside a `{ ... }` group, piped through `awk`,
and redirected the group's stdout into `artifacts/pr_context.summary.txt`. Every path in it was relative and
in-worktree; the refusal is about static verifiability, not an actual escape.

**Why it matters:** the natural way to synthesize `artifacts/pr_context.summary.txt` from the real diff — the step
you must do by hand because `collect_pr_context` leaves a stale file (see
[[collect-pr-context-lands-in-main-checkout]]) — is exactly this shape, so the PR gate is where you hit it.

**How to apply:** do not fight it with quoting. Split into two steps: (1) one plain read-only command that gathers
the data (`git rev-parse HEAD`, `git diff --shortstat <base>..HEAD`, `git diff --numstat <base>..HEAD -- <paths>`,
`git log --format=... <base>..HEAD` chained with `&&` and no redirect — that form IS accepted), then (2) the `Write`
tool to author the file from the returned output. Same for any generated evidence or context artifact. The
scratchpad `.ps1` + `pwsh -NoProfile -File` route from [[bash-tool-mangles-msbuild-switches]] also works and is
preferable when the content needs computation rather than transcription.
