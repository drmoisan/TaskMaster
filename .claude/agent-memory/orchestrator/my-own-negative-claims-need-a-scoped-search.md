---
name: my-own-negative-claims-need-a-scoped-search
description: I asserted a defect narrative was wrong based on a grep scoped to the wrong file, and a subagent correctly overturned it - the converse of the "subagent correction can be false" trap
metadata:
  type: feedback
---

Before recording that some construct does NOT exist, run a search whose SCOPE actually covers the
file that would contain it. Inferring absence from constructs you happened to see while reading a
neighbouring file is not evidence of absence.

**Why.** On issue #670, 2026-09-02, I recorded a "factual imprecision" finding against `issue.md`
and `spec.md`: they describe issue #488's D5 path as throwing `ObjectDisposedException`, and I
claimed no such throw exists on that path. I had greped `ObjectDisposedException` against
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` only, then separately read two
`InvalidOperationException` throws in `ItemViewer.Breadcrumb.cs` while chasing a different symbol,
and concluded from those two observations that the documents were wrong. I never searched
`ItemViewer.Breadcrumb.cs` for the exception type I was making a claim about.

The `feature-review` agent contradicted the premise. Direct verification proved the reviewer right:
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:391-393` is
`if (IsDisposed || Disposing) { throw new ObjectDisposedException(nameof(ItemViewer)); }` inside
`EnsureBreadcrumbResourceOwnership`, and the comment at :383-384 names it as "Issue #488 defect D5's
ObjectDisposedException throw". The two sites I cited were genuine but were different defects — D4
(`ThrowIfOffUiBoundary`) and D3 (different-provider) — both of which legitimately throw
`InvalidOperationException`. The requirement documents were correct throughout and needed no edit.

**How to apply.** This is the converse of
[[subagent-self-reported-correction-can-be-false]]: that memory says re-derive a subagent's
correction before accepting it, and this one says the re-derivation can just as easily vindicate the
subagent. Run it either way and let the tree decide. Two concrete habits:

- When claiming a construct is absent, state the SCOPE searched, as
  `evidence-and-timestamp-conventions` already requires for negative evidence claims. Writing the
  scope down is what exposes that the scope was wrong — I would have caught this by having to write
  "SearchScope: QfcItemController.ViewerSetup.cs" next to a claim about `ItemViewer.Breadcrumb.cs`.
- A defect narrative that names a specific exception type, error code, or symbol is a citation.
  Verify it in the file that would raise it before contradicting it, and never propagate the
  contradiction into a delegation prompt first — I put mine in the reviewer's prompt as an assertion,
  which risked the reviewer deferring to it.

Related: [[reconcile-plan-numbers-against-your-own-measurements]],
[[verify-subagent-capability-claims]], [[epic-kickoff-facts-need-independent-measurement]].
