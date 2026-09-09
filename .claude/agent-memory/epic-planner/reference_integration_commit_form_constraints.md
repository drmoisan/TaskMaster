---
name: integration-commit-form-constraints
description: The issue-539 staging exemption denies any git add/commit command text containing an angle bracket, dollar or backtick, denies any non -m option, and denies a cd-chained segment; combined with agent cwd reset this forces the integration branch to be checked out in the primary working directory
metadata:
  type: reference
---

`.claude/hooks/enforce-orchestration-preimplementation-gate-helpers.ps1` is the authority on the
form an epic-planner `git add` / `git commit` may take before any child checkpoint exists. Read it
before the first commit; four of its rules cost a denied attempt each on 2026-09-08.

1. **No `$`, backtick, `>` or `<` anywhere in the command text.** `UnresolvableCommandCharacters`
   is tested with `IndexOfAny` across the whole line, before any parsing. This makes the
   `Co-Authored-By: Name <email>` trailer unexpressible on this path. Measured: the identical
   commit succeeded with the trailer removed and was denied with it present. `git commit -F file`
   is not an escape (see 3), and `core.editor` is not either — Claude Code sets `GIT_EDITOR` in the
   environment, which overrides `core.editor`, so a `commit.template` plus a copying editor
   produces "Aborting commit due to empty commit message". Record the omission in the commit body.
2. **`git` must be token 0 and `add`/`commit` token 1.** `git -C <path> commit` is denied because
   token 1 is `-C`.
3. **`-m` / `--message` is the ONLY modelled option.** Any other dash-leading token before the
   `--` separator denies, including `-q`. A `-q` was the actual cause of two of the four denials.
4. **Every chain segment must independently be a recognized `git add`/`git commit`.** The line is
   split on `;`, `&`, `|` and newline outside quotes, so `cd X && git commit ...` denies on
   segment 1, and `git commit ... && git push` denies on segment 2. One git command per Bash call.
5. **At least one pathspec operand is required**, and every operand must sit under one of the five
   exempt trees. `git commit` with no operand denies, so a staged-index commit is impossible; use
   `git add <paths>` first, then `git commit -- <paths>` naming the same paths (the pathspec form
   fails with "did not match any file(s) known to git" if the file is not yet in the index).

**The consequence that matters most:** agent threads reset their Bash cwd to the primary working
directory between calls, so rule 4 means an integration branch checked out in a SEPARATE worktree
can never be committed to. Check the integration branch out in the primary session worktree
instead, and run bare `git add` / `git commit` with no `cd`. Verify the session branch holds no
unique work first; if it equals `origin/main`, switching costs nothing.

All of this applies only until a ready `artifacts/orchestration/orchestrator-state.json` exists in
the same worktree. Once it does, `Test-OrchestrationReady` allows the commit by the ordinary path
and none of these five rules apply — which is why preparation children, which seed that checkpoint
early, are unaffected.

Related: [[concurrent-prep-children-worktree-isolation]], [[epic-planner-state-required-fields]].
