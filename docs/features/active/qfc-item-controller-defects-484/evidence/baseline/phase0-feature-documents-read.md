# Phase 0 — Feature Documents Read

Timestamp: 2026-08-26T08-26
Task: [P0-T2]
Command: (read-only file reads; no shell gate command)
EXIT_CODE: 0

## Documents read

| Document | Lines | Read |
|---|---|---|
| `docs/features/active/qfc-item-controller-defects-484/issue.md` | 96 | yes |
| `docs/features/active/qfc-item-controller-defects-484/spec.md` | 1062 | yes |
| `docs/features/active/qfc-item-controller-defects-484/research/research.2026-08-24T09-45.md` | 1155 | yes |

## Resolved work mode

`full-bug`, taken from the persisted marker `- Work Mode: full-bug` in
`docs/features/active/qfc-item-controller-defects-484/issue.md`.

## Acceptance-criteria source

`docs/features/active/qfc-item-controller-defects-484/spec.md` is the **sole** acceptance-criteria source
for this feature. `user-story.md` is intentionally absent and its absence is not a blocker under
`full-bug`.

## Total acceptance-criterion count

**50** checkbox criteria, distributed across seven `###` sections of `spec.md`:

| Section | Criteria |
|---|---|
| Issue #480 — `ToggleNavigation(bool)` double toggle | 5 |
| Issue #481 — event unwiring path | 9 |
| Issue #483 — `MoveMailAsync` error handling and cancellation | 7 |
| Issue #484 — `Cleanup()` timer disposal and stale `_mailActions` | 6 |
| Issue #485 — WebView2 handler unguarded inputs | 6 |
| Upstream contract and scope discipline | 5 |
| File-size, toolchain, and coverage | 12 |
| **Total** | **50** |

Verification command run for the count: a regular-expression match of `^- \[[ x]\]` over `spec.md`
returned 50 lines.

## Precedence facts recorded from the reading

- `spec.md` carries a superseding clause: where `spec.md` and the research disagree on a figure, a design
  detail, or a line citation, `spec.md` governs.
- Two named divergences: the research's 22 additional `Cleanup()` detachments against the delivered 23,
  and the research's description of the `QuickFiler.Test.csproj` item group as alphabetically ordered.
- Research section 8.5's illustrative routing of the #480 `async: true` test into
  `QfcItemController.EventWiringTests.cs` is superseded by the plan's constraint C2 capacity table, which
  routes it to `QfcItemController.MailActionsTests.cs`.
- Every `file:line` citation in `spec.md` is anchored to the pre-change source at `<BASE_SHA>`, except the
  two the plan's capacity rule C2.7 explicitly preserves.

Output Summary: All three feature documents were read. Work mode resolves to `full-bug`; `spec.md` is the
sole acceptance-criteria source and contains exactly 50 acceptance-criterion checkboxes, all currently
unchecked.
