# breadcrumb-nonreentrant-upgrade-lifetime-guard (Issue #655)

- Date captured: 2026-08-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-nonreentrant-upgrade-lifetime-guard/ (Issue #655)

- Issue: #655
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/655
- Last Updated: 2026-08-27
## Summary

`BreadcrumbCoordinatorUpgradeLifetime` guards population work by entry-time lease currency only, so a
guarded action may re-entrantly begin another population. Research section 6.2 option C proposes a
genuinely non-re-entrant guard. Split out of issue #501, which was explicitly scoped to exclude it.

## Problem / Why

`BreadcrumbCoordinatorUpgradeLifetime` guards population work by entry-time lease currency, through
`TryRunCurrent` and `RunSynchronous`. That verdict is deliberately an entry-time check only: a guarded
action that re-entrantly begins another population, or that calls back into the lifetime, is not
prevented from doing so.

Feature #501 relied on that property and added a test pinning it,
`TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation`, so the present behavior is
specified rather than accidental. Research section 6.2 option C proposes the alternative: a guard that
refuses, rather than permits, a nested population under the same lifetime.

This was out of scope for #501. That issue fixed four ordering and lifetime defects (#462, #500, #501,
#502) without changing the guard's re-entrancy contract. Adopting option C changes the contract for
every caller of the lifetime, which carries its own regression surface; folding it in would have
widened a four-defect correctness fix into an API redesign.

## Proposed Behavior

Decide whether nested population under a single lifetime is ever legitimate. If it is not, make the
guard non-re-entrant so a nested population is refused, and convert the existing entry-time test into
the negative case.

## Acceptance Criteria (early draft)

- [ ] A decision record states whether nested population under one lifetime is legitimate.
- [ ] If refused, `TryRunCurrent` and `RunSynchronous` reject a nested population deterministically.
- [ ] Every `TryRunCurrent` and `RunSynchronous` caller is audited for a nested-population path.
- [ ] `TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation` is updated to match the chosen contract.

## Constraints & Risks

- Changes a contract every caller of the lifetime depends on; the blast radius is the whole breadcrumb
  coordinator surface.
- Feature #501 shipped a test that pins the CURRENT entry-time semantics. That test must be updated
  deliberately, not deleted, or the change will look like a regression.
- Touches files owned by sibling features; coordinate ownership before editing.

## Test Conditions to Consider

- [ ] Unit coverage for a nested population attempt under one lifetime
- [ ] Unit coverage for the existing non-nested path, proving no behavior change
- [ ] Regression coverage for every audited caller

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/breadcrumb-nonreentrant-upgrade-lifetime-guard/` folder from the template

## References

- Split out of #501; see `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md`, `## Rollout & Follow-up`
- Research: `docs/features/active/breadcrumb-coordinator-hub-defects-501/research/2026-08-24T09-12-breadcrumb-ordering-invariants-research.md`, section 6.2 option C
