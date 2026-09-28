---
name: footprint-inventory-clause-omits-inherited-promotion-rename
description: A changed-file inventory anchored to the Phase 0 base commit includes the feature-promotion rename from the pre-Phase-0 preparation commit, which enumerated admitting conditions never name
metadata:
  type: project
---

A footprint gate whose acceptance enumerates admitting conditions ("every path is one this plan names
as a backticked path, or sits beneath this feature folder, or sits beneath the executor's agent-memory
directory") will encounter at least one path meeting none of them: the
`docs/features/potential/<slug>.md` -> `docs/features/potential/promoted/<slug>.md` rename that the
feature-promotion lifecycle performs when it opens the active folder.

**Why:** The base anchor is `merge-base(HEAD, origin/main)` captured in Phase 0, but the promotion
rename lands in the preparation commit at the BASE of the branch, which is after the anchor and before
Phase 0. Any diff anchored to that ref therefore reports it, and no plan task authored it, so no plan
task names it. On #873 P7-T9 this was the single path of 100 that the clause could not admit.

**How to apply:**
- Do not silently drop it and do not treat it as a footprint violation. Report it as a classified
  exception in the inventory artifact.
- Establish provenance rather than asserting it:
  `git log --oneline refs/<anchor>..HEAD -- docs/features/potential` names the commit, and
  `git log --oneline refs/<anchor>..HEAD` shows that commit sitting below the Phase 0 commit.
- `git diff --name-only` reports only the rename DESTINATION, while `--name-status` reports `R095
  <old> <new>`. A union built from `--name-only` therefore undercounts by one versus the
  `--name-status` listing; say which you used.
- Staging it is a no-op because it is already committed and clean, so including it in the P7-T16
  pathspec set costs nothing and keeps fidelity to "exactly the path list recorded in the inventory".
  Pass the DESTINATION path; the source path no longer exists and `git add` on it errors.
- At preflight, this is a defect worth reporting: the clause should carry a fourth admitting
  condition for content committed before the Phase 0 commit.

**It is not always a rename.** On #911 P9-T12 the same clause failed on the same class, but
`git diff --name-status <MERGE_BASE>` reported a bare `A` addition of
`docs/features/potential/promoted/<slug>.md`, because the potential entry was authored *and* promoted
inside the branch, so git saw a creation rather than a move. A gate written to look for an `R###`
status therefore misses it. Search the union for any path under `docs/features/potential/` instead of
matching on the status letter, and trace provenance with
`git log --oneline --diff-filter=A -- <path>`, which names the promotion commit directly.

Related: [[project_preflight_mergebase_diff_gates_need_commit_cadence]],
[[project_baseline_sha_diff_conflates_merged_base]],
[[project_epic_child_branch_anchored_diff_lists_inherited_commits]].
