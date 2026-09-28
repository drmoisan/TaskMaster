---
name: footprint-ac-forbids-onbranch-followup-promotion
description: A checked footprint acceptance criterion makes the standing "promote every report-only defect into a real issue" rule unexecutable on that branch, because a potential-entry file falsifies the already-committed evidence
metadata:
  type: feedback
---

When an acceptance criterion asserts that the branch diff lists **only** a named path set, you cannot
run the MCP promotion lifecycle for follow-up defects on that branch. `new_potential_bug_entry` writes
under `docs/features/potential/`, which is outside every scoped feature's path set, so the promotion
turns a criterion that is currently true and whose evidence artifact is already committed into a false
one. You would have to uncheck a criterion the reviewer independently verified.

**Why this is a real bind, not a technicality:** the standing instruction is that out-of-scope and
report-only defects must go through promotion into a real GitHub issue, because prose in a feature
folder loses visibility once the folder is archived. Feature-review reliably produces four or five such
non-blocking findings on a healthy change, so the conflict fires on nearly every `full-bug` item that
carries a footprint AC — which is most of them, because footprint ACs are the standard defence against
scope creep in a parallel cohort.

**How to apply.** Do not resolve it by weakening either side. Order it instead:

1. Ship the branch with the footprint AC intact, and name the deferred follow-ups explicitly in the PR
   body so a reviewer sees them at review time rather than discovering them in an archived folder.
2. Record them in the checkpoint under a `deferred_followups` key with a
   `followup_deferral_reason` stating the AC conflict, so the next agent does not read the absence of
   an issue as an oversight.
3. File the consolidated follow-up issue **from a different branch**, after the PR is open. One issue
   per production file beats one per finding when the findings all cluster in the same file and a
   file-split is the natural vehicle for them.

Do NOT promote first and then argue the AC was "about source paths only" — the criterion text says
*only* the named paths, and the reviewer checks it literally.

A related distinction worth keeping: a *documentation* correction to a file already inside the scope
boundary is free and should be made inline. On #285 the reviewer found the spec's Risks section
claiming a slow COM call timed out after one second, while the same spec's Test Design section
correctly said an already-started delegate is never cancelled. Fixing that contradiction cost two
lines in `spec.md`, needed no toolchain re-run, and preserved the footprint AC — whereas the
source-level findings from the same review would each have cost a full C# gate cycle.

**The same bind fires on `.claude/agent-memory/`, and that one is self-inflicted.** Agent memory is
TRACKED in this repo, so an orchestrator that commits its own memory files onto an item branch puts
those paths into `git diff BASE HEAD` and makes a footprint AC permanently unsatisfiable. On issue #839
(2026-09-12) I committed five agent-memory paths mid-run as a rate-limit safety measure; preflight's
first blocking defect was that AC10 ("lists only paths matching the three Write Set entries") could
never pass, and it proposed carving agent-memory out of the gate. Carving it out is the wrong fix — it
weakens a real scope criterion to accommodate an avoidable error, and on a parallel cohort it also ships
`MEMORY.md` edits to main through the item's PR, which is exactly the sibling merge-conflict hazard that
[[parallel-epic-children-conflict-on-agent-memory-index]] describes.

The fix is to keep agent memory **uncommitted** in an item worktree. Modified-and-untracked memory files
are the normal residue state for a parallel item and are covered by a porcelain residue rule; committed
ones are a footprint violation. Undoing it: `git reset --hard` is blocked by `validate-bash`, but
`git reset <sha>` (mixed, the default) is allowed and is what you want anyway, because it drops the commit
while leaving every file on disk. Force-push is also blocked, so remove it from origin with
`git push origin --delete <branch>` followed by a plain `git push -u origin <branch>`. Save the blob SHAs
(`git diff-tree -r --no-commit-id --format= <sha>`) to the scratchpad first, since deleting the remote ref
makes those objects unreachable.

**How to apply:** never include `.claude/agent-memory` in a commit pathspec on an item branch. Write the
memory files, leave them dirty, and report their paths so the owning session harvests them.

Related: [[whole-repo-ci-gate-not-out-of-scope]], [[orchestrator-state-json-is-tracked-in-git]],
[[feedback_commit_before_ci_gate]], [[validate-bash-blocks-force-with-lease-too]],
[[parallel-epic-children-conflict-on-agent-memory-index]].
