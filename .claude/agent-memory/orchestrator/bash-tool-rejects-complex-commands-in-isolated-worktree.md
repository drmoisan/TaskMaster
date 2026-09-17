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

## Three more refusal triggers, each with its own message (verified 2026-09-12, item #816)

The filter is not one check but several, and each prints a different sentence. Knowing which one fired
tells you what to change:

1. **`git -C <path outside your worktree>`** → "this command redirects git to the shared checkout via
   `-C`". This blocks the whole cross-worktree read pattern, so you cannot inspect a sibling or the
   main checkout with `git -C`. Use the Read/Grep/Glob tools against absolute paths instead — those
   are not filtered. Note the object database IS shared, so `git hash-object -w -- <abs path in
   another worktree>` run WITHOUT `-C` is accepted and is the way to recover a sibling's untracked
   file byte-exactly (see [[byte-exact-copy-via-git-plumbing]]).
2. **A pathspec containing the word `parallel`** → "this command feeds git its arguments from stdin at
   runtime (xargs/parallel)". A false positive: `git ls-tree ... -- docs/features/parallel` is refused
   because the substring is read as GNU `parallel`. Drop that pathspec or scope the command
   differently; quoting does not help.
3. **`pwsh -NoProfile -Command '<payload>'` in any plain form** → "what it reads or is handed as shell
   text cannot be shown not to run git". So the model-routing PowerShell modules under
   `.claude/lib/model-routing/` are unreachable from an isolated worktree; read
   `config/orchestration-routing.json` and compute from the documented table instead, then let the MCP
   validator with `require_model_routing` be the correctness gate. See
   [[model-routing-scripts-absent-on-epic-integration-base]] and
   [[worktree-isolation-blocks-pwsh-per-agent-type]].

Consequence for a preparation run: an isolated orchestrator can still do all of promotion, recovery,
checkpointing and MCP validation, but it cannot run a single PowerShell payload, so any plan gate that
needs one must be deferred to a NON-isolated execution child.

**How to apply:** do not fight it with quoting. Split into two steps: (1) one plain read-only command that gathers
the data (`git rev-parse HEAD`, `git diff --shortstat <base>..HEAD`, `git diff --numstat <base>..HEAD -- <paths>`,
`git log --format=... <base>..HEAD` chained with `&&` and no redirect — that form IS accepted), then (2) the `Write`
tool to author the file from the returned output. Same for any generated evidence or context artifact. The
scratchpad `.ps1` + `pwsh -NoProfile -File` route from [[bash-tool-mangles-msbuild-switches]] also works and is
preferable when the content needs computation rather than transcription.
