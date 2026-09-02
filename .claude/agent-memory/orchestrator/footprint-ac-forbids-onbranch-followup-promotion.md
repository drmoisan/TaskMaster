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

Related: [[whole-repo-ci-gate-not-out-of-scope]], [[orchestrator-state-json-is-tracked-in-git]],
[[feedback_commit_before_ci_gate]].
