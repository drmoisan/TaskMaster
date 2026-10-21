# D3 — Fail-Fast / `SetBridgeCoordinator` Coupling Is Recorded in the Delivered Spec ([P3-T6])

Timestamp: 2026-08-28T05-41

Command: read `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md`, section
`### D3 — fail fast on a different provider`, and its `### Out of scope / non-goals` item 2.
EXIT_CODE: 0

## The delivered spec sentences, quoted verbatim

From the D3 design section, under the heading **"Load-bearing coupling — do not lose this."**:

> The choice of **fail-fast** is what keeps the `SetBridgeCoordinator` replace-without-dispose defect
> (Out of Scope item 2) dormant and therefore out of scope. Under fail-fast,
> `InitializeBreadcrumbPipeline` never constructs a second `BreadcrumbBridgeCoordinator`, so nothing new
> ever reaches `SetBridgeCoordinator`'s replacement branch. **If this spec were amended to adopt
> explicit re-initialization instead, that defect becomes live and MUST be pulled into scope in the same
> change-set.** The scope decision in
> `research/2026-08-25T10-20-orchestrator-comment-crosscheck.md` § "Claim 2" is explicitly contingent on
> this.

The corresponding out-of-scope entry, item 2, states the same coupling from the other side:

> 2. **`SetBridgeCoordinator` replaces without disposing while `Dispose():216` disposes**
>    (`BreadcrumbItemViewerLifecycleCoordinator.cs:62-77`; `UnsubscribeBridge()` at `:306-317` detaches
>    four handlers and disposes nothing). Out of scope because it stays **dormant under the fail-fast D3
>    design** — see the explicit coupling recorded under D3 below. Promote to a new issue.

Together these carry both halves the criterion requires: the replace-without-dispose defect is out of
scope **because** D3 fails fast, and adopting explicit re-initialization instead would require pulling
that defect into scope in the same change-set.

## No task in this plan substituted a re-initialization branch

The delivered guard in `InitializeBreadcrumbPipeline(provider, operations)` has exactly two outcomes
when `BreadcrumbCoordinator` is non-null: it throws `InvalidOperationException` when the supplied
provider is not reference-equal to the retained one, and it returns without effect when it is. There is
no branch that disposes the existing pipeline and rebuilds it, and no branch that constructs a second
`BreadcrumbBridgeCoordinator`.

`[P3-T3]` is the only task in this plan that edits that member, and it delivered the throw. Constraint
C7 forbids substituting an explicit re-initialization branch for the throw, and the substitution was
not made. `[P3-T5]`'s dossier records the corroborating evidence: the complete changed-line set of
`BreadcrumbItemViewerLifecycleCoordinator.cs` contains no line of `SetBridgeCoordinator`, so the
replacement branch that would become live under re-initialization is untouched and stays unreachable
from `InitializeBreadcrumbPipeline`.

The defect itself is carried forward rather than dropped: `[P7-T5]` records it as one of the three
out-of-scope follow-up candidates, with its mechanism and trigger, so the follow-up does not have to
re-derive them.

Output Summary: The delivered `spec.md` records the coupling in two places — the D3 design section's
"Load-bearing coupling — do not lose this" paragraph and out-of-scope item 2 — both quoted verbatim
above. They state that the `SetBridgeCoordinator` replace-without-dispose defect is out of scope
**because** D3 fails fast, and that adopting explicit re-initialization would make it live and require
pulling it into scope in the same change-set. **No task in this plan substituted a re-initialization
branch for the throw.**
