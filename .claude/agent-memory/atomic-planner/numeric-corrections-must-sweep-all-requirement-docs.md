---
name: numeric-corrections-must-sweep-all-requirement-docs
description: A factual/numeric correction applied to plan.md and spec.md must also be swept through user-story.md and issue.md, but only where the number denotes the corrected quantity — homonym numbers must be left alone
metadata:
  type: feedback
---

When a preflight delta corrects a quantity (sibling-child count, file count, phase count), apply it to **all four** requirement documents in the feature folder, not just `plan.md` and `spec.md`.

**Why:** Under Work Mode `full-feature`, `issue.md`, `spec.md`, and `user-story.md` are all authoritative requirements sources; `user-story.md` is an AC source that late-phase AC-mapping tasks write to, and Phase 0 typically makes the executor read all three *in full*. On #497 a corrected sibling count (fourteen -> fifteen) landed in `plan.md` and `spec.md` only, leaving `user-story.md` and `issue.md` each containing both numbers for the same quantity — a self-contradiction inside a document the executor is required to read, which cost a full preflight iteration as a Blocking finding.

**How to apply:**
- After any numeric delta, grep the whole feature folder for both the old and new number-words (`fourteen|fifteen`, not just `fourteen`) across `issue.md`, `spec.md`, `user-story.md`, `plan.*.md`.
- Classify each hit before editing. On #497 the same word "fifteen" appeared as an unrelated *coverage-point* delta ("fifteen-point phantom improvement") in three places that had to stay untouched. Read the surrounding clause; a bare `replace_all` corrupts homonyms.
- Never change acceptance-criterion text or checkbox state in `issue.md` / `user-story.md` as a side effect of a numeric sweep — those are separate deltas and require their own authorization.

Related: [[plan-aggregate-claims-must-be-rederived-after-deltas]], [[ac-source-sweep-definition-of-done]], [[pre-applied-deltas-reconcile-to-stated-wording]].
