---
name: subagent-cites-harness-gitstatus-of-wrong-checkout
description: A subagent can cite the harness-supplied git status block as an observation about YOUR worktree when it actually describes the session checkout; verify any untracked/dirty claim yourself before it lands in a plan
metadata:
  type: feedback
---

A delegated agent running against an isolated agent worktree may cite the **harness-supplied
`gitStatus` context block** as if it were a measurement of that worktree. That block describes the
**session checkout**, which in this repository is routinely a different path with different
untracked files.

**Observed 2026-08-29 (issue #637 preparation, parallel run bugs-638-644-647).** `atomic-planner`
rejected a preflight reviewer's premise, asserting that
`docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440` was
"present and untracked in this worktree" and writing that observation into the plan as the stated
justification for narrowing seven `git add` / `git status --porcelain` pathspecs. Measured directly
in the agent worktree, `git status --porcelain -- docs/features/active` listed only the feature's own
folder: the 440 folder was **tracked**, having merged into `main` at the branch's own base commit
`ecdb1c84` (subject: "Merge pull request #689 from drmoisan:bug/...-440"). The claim was true only of
the session worktree `TaskMaster-wt/2026-08-29T00-11`, whose start-of-session status block listed 440
as untracked.

**Why this is worth catching rather than tolerating:** the *action* was correct — narrowing a broad
`docs/features/active` pathspec is right defensively, because the executor runs later than the
planning pass and a concurrent sibling can introduce an untracked folder in between (see
[[feedback_git_add_a_sweeps_unrelated_queued_promotions]] in the user-scope memory for the real
incident this guards against). Only the *justification* was false. A correct fix resting on a false
citation is the worst shape to ship: it survives review because the fix looks right, and the false
fact then propagates into every later document that cites the plan. It also inverted the record —
the reviewer's original premise was accurate and was overridden.

**How to apply:**
- Before accepting any subagent claim about tracked/untracked/dirty state, branch topology, or file
  presence, re-measure it yourself with `git -C <agent-worktree-abs-path> ...`. Cheap, and it is the
  only authoritative source.
- When a subagent *rejects* a reviewer premise on environment grounds, treat that as a high-value
  verification trigger rather than as evidence of diligence. Two premise rejections in the same run
  were correct and upheld (`Helpers.ps1` line citation, `branch="True"` capitalization); this third
  was not, and they read identically in the report.
- Arm delegation prompts for isolated worktrees with: name the authoritative worktree path, and state
  that any environment fact not measured by a command run inside that path is unverified and must be
  marked as such rather than inferred from a supplied status block.
- Keep the defensive fix when the action is right and the reason is wrong; correct the reason in place
  rather than reverting the edit.

Related: [[agent-worktree-hooks-resolve-to-agent-cwd]] and
[[collect-pr-context-lands-in-main-checkout]] — both are the same underlying hazard, a tool or agent
resolving "here" to a checkout other than the one the caller means.
