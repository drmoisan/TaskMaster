# breadcrumb-right-descent-non-goals-follow-up (Potential)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Draft

## Problem / Why

Issue #440's spec (`docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440/spec.md`, "Boundary decisions" subsection, ~lines 799-804) deliberately records two known divergences between the Qfc and Efc breadcrumb Right-arrow behavior as non-goals rather than fixing them, to avoid breaching #498 decision D1 and reopening the #400 selector session's scope:

1. **Right-descent commit asymmetry.** Efc commits a filing target on Right-arrow descent; Qfc only moves a highlight. Making Qfc commit would breach #498 decision D1.
2. **Single-level Right descent limit.** Neither surface descends two levels with a single Right-arrow press.

These were explicitly deferred rather than silently dropped - the spec says to "report to the maintainer rather than silently expanding scope." This potential entry is that report, so the deferred decisions have a durable tracking record instead of living only inside a merged spec's prose.

## Proposed Behavior

Not yet decided. Two independent questions need a maintainer decision:

- Should Qfc's Right-arrow descent be changed to commit a filing target (matching Efc), and if so, how does that interact with #498 decision D1 and the #400 selector session scope it would reopen?
- Should either surface support multi-level Right-arrow descent in a single press, or is single-level-per-press the intended permanent behavior?

## Acceptance Criteria (early draft)

- [ ] A maintainer decision is recorded on whether Qfc Right-arrow descent should commit a filing target like Efc, with explicit reference to #498 decision D1
- [ ] A maintainer decision is recorded on whether multi-level Right-arrow descent is in scope for either surface
- [ ] If either is approved as a change, a full spec is written scoping the #498/#400 interaction before implementation begins

## Constraints & Risks

- Changing Qfc Right-descent to commit would breach #498 decision D1 as currently recorded and would pull the #400 selector session back into scope - any implementation needs an explicit supersession decision for D1, not a silent override.
- A reviewer unfamiliar with #498 may reopen these boundary decisions without realizing they were deliberate; #440's spec mitigates this locally via its AC-13 and Boundary Decisions subsection, but that context does not travel with this potential entry unless linked here.

## Test Conditions to Consider

- [ ] If approved, regression coverage should assert Right-descent behavior parity (or intentional divergence) between Qfc and Efc
- [ ] If multi-level descent is approved, coverage for exactly-two-level and boundary (root/leaf) cases on both surfaces

## Next Step

- [ ] Bring to the maintainer for a scope decision before any promotion to an active feature folder
- [ ] If approved, promote to GitHub issue (feature request template) referencing #498 decision D1 and #400
- [ ] If declined, close this potential entry with the decision recorded for future reference

