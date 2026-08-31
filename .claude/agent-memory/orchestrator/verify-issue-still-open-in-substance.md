---
name: verify-issue-still-open-in-substance
description: An OPEN issue admitted to a parallel/epic run may already be fixed in substance; grep the source for the issue number before preparing, and check whether the residual is already a separate issue
metadata:
  type: feedback
---

Before preparing any item whose scope was written from earlier research, verify the issue is still
open **in substance**, not merely open on GitHub. An issue stays open for clerical reasons long after
the work lands.

**Why:** on the #469 preparation run (2026-08-29) the delegation prompt described four live defects in
`QuickFiler/Controllers/QfcCollectionController.cs`. Three were already remediated and merged, each
with a deterministic regression test, and the fourth had been resolved by a third route the issue did
not anticipate. Had the prompt been taken at face value, three of the acceptance criteria would have
passed on an empty diff — the vacuous-gate class that [[preflight-catches-vacuous-gates]] exists to
stop, except that here the whole item, not one gate, was vacuous.

**How to apply:**

- **Grep the production source for the issue number first.** One `git grep -n "#<issue>" -- '*.cs'`
  answers it in seconds. Remediation commits in this repository leave `Issue #<n> defect <k>:`
  comments at the fix site, so the fix announces itself. Do this before promotion, not after.
- **Then `git log --all --grep=<issue>` and confirm ancestry** with `git merge-base --is-ancestor`.
  Commit subjects name the defect they closed, which maps them to the issue's numbered list.
- **Search `docs/features/potential/promoted/` for the residual before scoping it.** #469's one
  genuinely open item — removing a now-inert parameter — was already promoted as its own OPEN issue
  (#629). Preparing it under #469 would have duplicated that issue and breached a documented scope
  lock protecting the exact file #629 owns. A deferral recorded in a merged doc comment ("removing it
  is a follow-up candidate, not part of this change") is a strong signal that a successor issue exists.
- **Verify the successor's state yourself with `gh issue view`.** A subagent without shell access can
  only cite the in-repo promoted record, which proves the issue was created, not that it is still open.
- **Report the premise change loudly and let the caller decide.** Narrowing scope honestly and saying
  so is right; silently preparing the narrowed item under the original label is not, because the
  downstream PR body would inherit the false framing.

**The residual is usually stale prose.** A merged fix that does not sweep its consumers leaves comments
asserting the old behavior. Those survive precisely because the fix commit never touched the consuming
file — confirm with `git show --stat` on the fix commits. See [[stale-figure-sweep-by-changed-file-set]].

Related: [[prepared-epic-child-invalidated-by-sibling-merge]] is the same failure at epic scale;
[[feedback_verify_repro_before_bugfix_cycle]] is the single-item form of the same discipline.
