# Remediation Requirements Map

Timestamp: 2026-07-21T18-51Z
Command: Read remediation-inputs.2026-07-21T18-19.md, spec.md, plan.2026-07-21T10-41.md, policy-audit.2026-07-21T18-19.md, code-review.2026-07-21T18-19.md, feature-audit.2026-07-21T18-19.md, artifacts/pr_context.summary.txt, and artifacts/pr_context.appendix.txt; run git cat-file and git merge-base reconciliation for df5ad49c909f6b739edef45d0336151f44e827a6 and b38a87751669f3522928dd01ac0f4f97b82572ed
EXIT_CODE: 0
Output Summary: The full-bug requirements bundle is complete. All 19 acceptance criteria are present. The reviewed base and head exist, and their merge base is df5ad49c909f6b739edef45d0336151f44e827a6.

## Requirements Authority

- Work mode: `full-bug`.
- Authoritative acceptance source: `spec.md`.
- Acceptance criteria count: 19.
- Reviewed base: `df5ad49c909f6b739edef45d0336151f44e827a6`.
- Reviewed head: `b38a87751669f3522928dd01ac0f4f97b82572ed`.
- Base commit lookup exit: 0.
- Reviewed-head commit lookup exit: 0.
- Merge base: `df5ad49c909f6b739edef45d0336151f44e827a6`.
- Base-is-ancestor exit: 0.

## Acceptance Criteria Inventory

1. AC-1: Collapsed selector renders the committed scored row and supplied formatted probability without recomputation.
2. AC-2: Collapsed page has no vertical scrolling controls and has one correctly accessible drop-down button.
3. AC-3: Activation opens an ItemViewer-owned native ToolStrip popup that is not globally topmost.
4. AC-4: Popup placement follows the active-monitor fit, side-choice, tie, and clamp rules.
5. AC-5: Closed Up and Down commit the adjacent selectable folder exactly once without scrolling.
6. AC-6: Open navigation changes only pending selection, skips separators, clamps, and keeps the active option visible.
7. AC-7: Enter or activation commits once, closes, renders committed state, and returns focus.
8. AC-8: Escape and uncommitted automatic close restore the original selection without publishing pending state.
9. AC-9: Left and Right preserve existing breadcrumb behavior without selector-session mutation.
10. AC-10: Immediate, resolved, unresolved, empty-chain, and provider-failure paths retain score, identity, and selection.
11. AC-11: Issue #398 atomic replacement, readback, in-flight selection, and stale-completion protections remain intact.
12. AC-12: Closed and popup surfaces receive one consistent state update and route each inbound event once.
13. AC-13: Automated evidence proves theme, accessibility, pending focus, and deterministic focus return.
14. AC-14: Popup creation is lazy, uses the existing environment, is reused, and is safely reset/disposed without callbacks.
15. AC-15: Empty, invalid, initialization-failure, zero-space, repeated-lifecycle, and provider-failure edges are deterministic and leak-free.
16. AC-16: Intended defects have deterministic fail-before/pass-after MSTest evidence without prohibited dependencies.
17. AC-17: Legacy project includes, 500-line limits, generated-file boundary, package, and configuration constraints hold.
18. AC-18: The uninterrupted C# QA sequence and numeric coverage thresholds pass without exclusion or threshold weakening.
19. AC-19: Existing regressions and the complete semantic contract pass through automated tests.

## Remediation Finding to Plan Mapping

| Remediation finding | Plan tasks |
|---|---|
| Popup attachment and cached replay occur before document readiness | P1-T1 through P1-T3; P2-T3; P2-T5 through P2-T8 |
| Pending popup creation is not serialized or lifecycle-invalidated | P1-T4 through P1-T8; P2-T1, P2-T2, P2-T4, P2-T5 |
| Missing native-close rollback and inbound Up composition tests | P3-T1 through P3-T5 |
| Literal AC-18 and bounded nonnumeric adapter accounting require reconciliation | P0-T8; P4-T1 through P4-T4; P5-T4, P5-T6, P5-T7; P6-T1, P6-T2 |
| Ordered QA, artifact validation, and independent review must be repeated | P5-T1 through P5-T8; P6-T1 through P6-T6 |

No requirement-source absence, history mismatch, or unresolved mapping gap was found.
