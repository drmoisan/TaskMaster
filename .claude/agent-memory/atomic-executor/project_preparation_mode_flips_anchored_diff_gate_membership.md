---
name: preparation-mode-flips-anchored-diff-gate-membership
description: In preparation mode the feature folder is committed to the branch BEFORE execution, so every base-anchored git diff lists issue.md/spec.md/research/plan.md alongside the code paths — an equality gate over "the N footprint paths" becomes unsatisfiable.
metadata:
  type: project
---

A plan authored in "preparation mode" is committed together with its whole feature
folder onto the branch, and a later run executes it from that pushed commit. Every
`git diff --name-only <base> -- .` in the plan therefore lists the four feature-folder
paths (`issue.md`, `spec.md`, `research/*.md`, `plan.*.md`) **in addition to** the code
footprint, because the base commit predates the preparation commit.

**Why:** an anchored footprint gate written as an equality over the N code paths is
unsatisfiable in that state, and a preflight round that does not know about preparation
mode will verify it as correct against the pre-commit tree. Observed on #644, where
`[P4-T8]`/`[P5-T20]` had to be rewritten between round 1 and round 2 for exactly this.

**How to apply:**
- Write the repository-wide clause as a **membership test** over the non-code paths
  ("contains all six code paths and no path other than those six and the four
  feature-folder paths"), never as an equality over all ten. The membership form holds
  in both states: if the preparation commit was not made, the four are untracked and
  `git diff` cannot list them, and an unexpected formatter rewrite still fails.
- `git diff <commit>` compares the commit tree to the **working tree** and ignores
  untracked files, so evidence artifacts written under the feature folder during
  execution are correctly absent until they are staged or committed.
- A path staged with `git add` becomes tracked, so `git diff <commit> --name-only` does
  list a newly created file once staged — this is the G8b companion that makes the
  name-listing diff complete.
- The terminal commit task must flip **its own** plan checkbox before staging, because
  the plan file lives inside the folder the commit covers; flipping after re-dirties the
  tree the clean-tree clause asserts. Same reason it must write no evidence artifact.
- Keep `':!.claude/agent-memory'` on every repository-wide span. See
  [[agent-memory-tracked-breaks-unscoped-git-gates]].
