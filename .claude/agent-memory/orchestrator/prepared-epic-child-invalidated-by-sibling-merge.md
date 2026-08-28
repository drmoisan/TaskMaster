---
name: prepared-epic-child-invalidated-by-sibling-merge
description: A separate workstream merging a fix for an issue a prepared epic child also claims to close silently invalidates that child's plan; open issue state is not evidence, and the child's own descope decisions can become moot
metadata:
  type: project
---

An epic child prepared and preflight-cleared against one HEAD can be invalidated wholesale when an
independent workstream lands a fix for one of the issues that child claims to close. Observed
2026-08-25 on child 498 (`breadcrumb-router-navigation-defects-498`, claiming #439/#440/#498/#499)
after PR #605 landed #439 on `main` and `main` was merged into the epic integration branch.

**Why:** epic children are prepared in parallel against a frozen snapshot, but the repository's other
delivery streams do not pause. A child's plan encodes line citations, assumed file contents, and
remaining-work assumptions, all three of which the sibling merge can falsify at once. Here 67 line
citations went stale in two documents, and six of them named a construct that no longer exists.

**How to apply:**
- **Do not infer scope from GitHub issue state.** #439 stayed OPEN after its PR merged. Determine
  from the CODE and from the landed feature folder's own `spec.md` what actually shipped.
- **Read the landed spec's Scope & Non-Goals, not just its diff.** #439's landed spec scoped itself
  to the Efc surface and explicitly excluded the Qfc/ItemViewer breadcrumb and all #440 keyboard
  work. That boundary is what decides which of the child's remaining items survive.
- **Expect the child's own DESCOPE decisions to become moot, not just its work items.** 498 had
  descoped two items on the grounds that the required files were unowned; the other workstream owned
  those files and shipped both. A descope record whose premise has evaporated must be rewritten as a
  retraction, not silently deleted.
- **Expect the landed work to have REJECTED the child's chosen mechanism.** 498's decision D5 picked
  a provider-side suffix match and explicitly rejected plumbing `ArchiveRootPath` through an unowned
  controller. The landed fix did exactly the rejected thing. The child's mechanism may still be the
  only one available in ITS owned files, so re-justify rather than discard it.
- **Look for new seams the child should now CONSUME.** The landed work added the exact
  selected-node concept the child's research had concluded did not exist and planned to invent. A
  task that re-invents a landed seam is a defect, and so is a task that re-fixes a landed fix.
- **Re-measure file sizes.** The landed work pushed two files past the 500-line limit, converting a
  contingent partial-class authorisation into a mandatory prerequisite.

Related: [[epic-child-plan-phase0-paths-are-stale-in-epic-children]],
[[feedback_reverify_ground_truth_after_user_midcycle_commit]].
