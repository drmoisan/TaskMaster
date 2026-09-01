---
name: plan-checkoff-fixpoint-breaks-terminal-clean-tree-gate
description: A commit task's own check-off necessarily post-dates its commit, so a terminal gate asserting "the only dirty path is this artifact" is unsatisfiable; check off the remaining tasks BEFORE the final commit
metadata:
  type: project
---

A plan task whose acceptance names the exact set of dirty paths after a commit is a fixpoint trap.
Check off the last few tasks **before** the final commit stages, so the final commit carries their
check-offs and the post-commit status is genuinely empty.

**Why:** Checking off a task requires the task to have passed. When the task *is* a commit, its
check-off is necessarily written to the plan file **after** that commit exists, so the plan file is
dirty the instant the commit task completes. On 2026-08-27 (feature 444) `[P5-T31]`'s acceptance read
"the recorded output names at most one path, and if it names one, that path is this artifact itself".
At capture time the one dirty path was `plan.<TS>.md` carrying the `[P5-T30]` check-off, not the
artifact — and no execution path could have made it the artifact, because Phase 5 had no commit
between `[P5-T30]` and `[P5-T32]`. The same phase's `[P5-T29]` presupposed that "the only paths still
uncommitted are this task's own spec edit and this audit artifact", which was false for the same
reason: eleven earlier Phase 5 artifacts were also still uncommitted.

**How to apply:**
- At the final commit task, first flip the remaining plan checkboxes (including that commit task and
  everything after it), then `git add` and commit once. The post-commit `git status --porcelain` is
  then empty and the terminal gate passes outright — "names at most one path" is satisfied by zero.
- When the literal acceptance is unreachable, execute it, record the true capture verbatim, state the
  structural reason it cannot be met, and demonstrate the substantive condition via the next task.
  Do **not** silently reword the artifact so the acceptance appears met, and do not skip the task.
- Flag it in the completion report as a plan-text/mechanics tension so the planner can fix the
  wording next time. Suggested planner fix: word the gate as "names at most one path, and if it names
  one, that path is the plan file", or add an intermediate commit before the terminal capture.
- Second validated planner fix (issue #648 plan, cleared at preflight round 4): order the commit task
  as commit -> **capture and retain** `git status --porcelain <pathspec>` -> write the artifact from
  the retained output -> flip the checkbox -> a second `git add`/commit that the task text explicitly
  declares housekeeping and outside its acceptance. The capture must precede the artifact write
  because the artifact itself lives under the status pathspec; a capture taken afterwards always sees
  it as untracked. This makes the residual genuinely empty at capture time and keeps the unverifiable
  final step out of the acceptance clause. The remaining gap — nothing executes a check after the
  second commit — is the irreducible fixpoint and is acceptable.
- Scope every terminal `git status` by pathspec. `.claude/agent-memory/**` is tracked and other agents
  write to it, so an unscoped clean-tree gate is unsatisfiable for reasons unrelated to the feature.
  See [[project_agent_memory_tracked_breaks_unscoped_git_gates]].
