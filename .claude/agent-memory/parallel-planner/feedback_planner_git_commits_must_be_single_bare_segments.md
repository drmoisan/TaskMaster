---
name: planner-git-commits-must-be-single-bare-segments
description: The issue #539 orchestration-bookkeeping exemption only clears a git add/commit that is the ONLY segment on the line, with no cd, no angle brackets, and git as the first token — so the planner must checkout the plan branch in the session worktree
metadata:
  type: feedback
---

Verified 2026-08-29 while committing the `bugs-635-440` run manifest. Every one of these cost a
denied invocation.

**The rule:** a `git add` or `git commit` that touches only exempt orchestration-bookkeeping trees
still gets `PREIMPLEMENTATION_GATE_BLOCKED` unless the whole command line satisfies all four:

1. **Every segment is independently a recognized staging invocation.** `Split-OrchestrationCommandLine`
   splits on chain operators outside quotes and `Test-ExemptOrchestrationStagingCommand` requires
   EVERY segment to pass. So `cd "..." && git add <exempt path>` DENIES — the `cd` segment is not a
   recognized invocation. The git command must stand alone on the line.
2. **`git` is token[0] and `add`/`commit` is token[1].** Row 14 rejects anything in between, so
   `git -C <path> add ...` denies. There is no way to redirect the pathspec base.
3. **No `$`, backtick, `>`, or `<` anywhere on the line** (`$script:UnresolvableCommandCharacters`,
   row 12), tested across the WHOLE line including inside quotes. This forbids the repository's
   standard `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>` trailer in the commit
   message. Use a message with no angle brackets; a bracket-free "trailer" is invalid git trailer
   syntax, so omit it rather than mangle it.
4. **At least one pathspec operand, all operands under one of the five exempt trees**
   (`docs/features/epics/`, `docs/features/parallel/`, `docs/features/active/`,
   `docs/features/potential/`, `artifacts/orchestration/`). The only modelled option is the message
   option on `commit`; any other dash-leading token denies, so `-F <file>` is unavailable.

**Consequence for the planner: you cannot use a dedicated worktree for the plan-home branch.**
Agent threads get their bash cwd reset between calls, so reaching another worktree requires a `cd`
segment, which rule 1 forbids. The working approach is:

1. `git branch parallel/<slug>-plan origin/main` and push it.
2. When it is time to commit, `git switch --detach` any worktree holding the branch, then
   `git checkout parallel/<slug>-plan` in the SESSION worktree (`git checkout` is not gated).
3. Write, `git add <exempt paths>`, `git commit -m "..." -- <exempt paths>`, `git push` — each as
   its own bare invocation.
4. `git checkout <original session branch>` to restore. `artifacts/` is gitignored, so the
   checkpoint and working kickoff copy survive the branch switches untouched.

**Step 2 usually fails the first time on untracked collisions.** The session worktree accumulates
untracked files under `docs/features/active/` from earlier work; when the same paths are TRACKED on
the plan-home branch, `git checkout` aborts with "would be overwritten by checkout". These are
typically byte-identical duplicates of what is already committed on `origin/main`, so nothing is at
risk, but do not delete them: they are untracked on the session branch, so a delete is unrecoverable
there. Move them to the scratchpad, complete the commit, switch back, and move them home. The
colliding set is exactly the indented lines of the failed checkout's own output.

Also confirmed: an agent thread's bash cwd resets between calls to the SESSION WORKTREE ROOT, which
is precisely the base a bare `git add` needs. So rule 1's ban on a `cd` segment costs nothing here —
issue the git command bare and it resolves pathspecs against the right root.

**You cannot clean up a worktree you created.** `git worktree remove` returns
`EPIC_WORKTREE_REMOVAL_BLOCKED` unless an epic checkpoint or a parallel-ORCHESTRATOR checkpoint
(`route_id == "parallel"`) has a matching record with `merge_status` in `{merged,
worktree_removed}`. The planner writes neither, so a planner-created worktree is stranded. Do not
create one.

See [[parallel-artifact-authoring-gotchas]] for the schema-side traps in the same run.
