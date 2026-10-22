# Issue Update Mirror — follow-up issue #655 (P9-T2)

Timestamp: 2026-08-27T23-37

PostedAs: body

Issue URL: https://github.com/drmoisan/TaskMaster/issues/655

IssueState: OPEN, confirmed with `gh issue view 655`

## Purpose

P9-T2 requires filing a follow-up issue for research section 6.2 option C (a non-re-entrant upgrade-lifetime guard), explicitly out of scope for this feature.

## Exact text posted

The issue body below is the promoted content of the potential entry, reproduced verbatim. The promotion
tooling carried every section through, so no section was dropped and no supplementary comment was
needed.

```
- Work Mode: full-feature
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

## Acceptance Criteria
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

## Test Conditions
- [ ] Unit coverage for a nested population attempt under one lifetime
- [ ] Unit coverage for the existing non-nested path, proving no behavior change
- [ ] Regression coverage for every audited caller

## Source
From: docs/features/potential/2026-08-27-breadcrumb-nonreentrant-upgrade-lifetime-guard.md
```

## Route deviation recorded

The plan task names `gh issue create` as the mechanism. That call was DENIED by a repository PreToolUse
hook:

```
PROMOTION_MCP_ONLY_BLOCKED: Direct GitHub issue creation via `gh` bypasses the approved
drm-copilot MCP promotion path.
```

The issue was therefore filed through the approved MCP promotion lifecycle instead, in two steps:
`mcp__drm-copilot__new_potential_entry` to create the potential entry, then `mcp__drm-copilot__potential_to_issue` with `promotion_type` `refactor` to promote it.

No wording was altered to evade the hook; only the route changed, to the one the repository mandates.
The approved route is also strictly better for durability, because it leaves a permanent promoted
record at `docs/features/potential/promoted/2026-08-27-breadcrumb-nonreentrant-upgrade-lifetime-guard.md` in addition to the GitHub issue.

The plan task's acceptance is met in full: this mirror artifact exists, carries a
`https://github.com/drmoisan/TaskMaster/issues/` URL, and records `PostedAs: body`.
