# Reduced-Audit Handoff (P5-T11)

Timestamp: 2026-09-03T03-42
Task: [P5-T11]
Issue: #735 — ribbon-engine-toggle-defects
Work Mode: full-bug (acceptance criteria from `spec.md` only)
Command: not applicable — this task assembles pointers and records the follow-up list; it runs no command.
EXIT_CODE: 0

## Pointers for the audit

| Artifact | Path (relative to the feature folder) | Headline |
|---|---|---|
| Coverage delta | `evidence/qa-gates/coverage-delta.2026-09-02T12-04.md` | coordinator 98.52% to 100.00%; gate class 100.00%; added-line coverage 18/18 |
| Footprint scope | `evidence/qa-gates/footprint-scope.2026-09-02T12-04.md` | 12 source paths, all authorized; prohibited paths absent |
| Toolchain closure | `evidence/qa-gates/toolchain-loop-closure.2026-09-02T12-04.md` | one clean pass, all ten steps exit 0 |
| Acceptance status | `evidence/issue-updates/ac-status.2026-09-02T12-04.md` | 24 of 25 checked |
| Evidence completeness | `evidence/qa-gates/evidence-completeness.2026-09-02T12-04.md` | verdict PASS |
| Evidence sanitisation | `evidence/qa-gates/evidence-sanitization.2026-09-02T12-04.md` | zero residual token occurrences |
| Scope amendment | `evidence/qa-gates/coordinator-size-contingency.2026-09-02T12-04.md` | P4-T3 branch B taken |

## Manual-verification status

**OPERATOR-ACTION-REQUIRED.**

Source: the `ManualVerificationStatus:` field of
`evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md`.

This executor has no live Outlook host, so neither observation in the two-step procedure was made.
Acceptance criterion F2-AC8 is consequently the single unchecked item of the twenty-five. The
procedure is recorded in full in that dossier and an operator can close the criterion by performing
it, recording the two outcomes and flipping the checkbox.

## Scope amendment carried into the audit

The P4-T3 branch B contingency was triggered: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`
measured 515 lines after formatting, above the 500-line ceiling. Two paths beyond the plan's original
ten write-set paths were therefore created:

- `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` (157 lines, 94.87% line coverage)
- `TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs` (213 lines, 9 tests)

Both are registered as compile items. The coordinator is now 415 lines. The extraction is
behavior-preserving: all 134 ribbon tests and all 6982 first-party tests pass after it. The plan
authorizes this branch and requires it to be reported; the full record is in the contingency
artifact.

## Follow-ups the spec defers — promotion is the orchestrator's action, not this plan's

All three are recorded in the spec's Scope & Non-Goals and Rollout & Follow-up sections, and all
three were confirmed untouched by this change in `evidence/qa-gates/callsite-edit-scope.2026-09-02T12-04.md`
and `evidence/qa-gates/footprint-scope.2026-09-02T12-04.md`.

1. **The eight QuickFiler-settings unguarded-globals sites** on the Intelligence partial — the
   move-entire-conversation, save-attachments, save-pictures and save-email-copy query and toggle
   members, plus the high-confidence mode and threshold members. They dereference the globals chain
   with no guard, the same defect class as finding 2. Repairing the four callback bindings in
   finding 1 makes four of them reachable through a second entry point, but it opens no NEW crash
   window: the sibling pressed-state callbacks for the same four controls already dereference the
   same chain unguarded and already fire when the menu is opened in the pre-initialization window.
   These belong to issue #524's site table rather than to #735's finding 2. Located at lines 29 to 50
   of `TaskMaster/Ribbon/RibbonController.Intelligence.cs`; confirmed untouched by any diff hunk.

2. **The orphaned handler `BuildFolderClassifier_Click`** on the viewer type — public, correctly
   signatured, and referenced by no `onAction` anywhere in the CustomUI document. This is the inverse
   of finding 1: an orphaned handler rather than an orphaned binding, and harmless. The enumeration
   test added by this change deliberately asserts only the XML-to-code direction, because that is the
   direction that produces silent user-facing breakage. Promote separately if the reverse assertion
   is wanted. `TaskMaster/Ribbon/RibbonViewer.cs` is unchanged by this change, so the handler is
   untouched.

3. **The three `NotImplementedException`-throwing bound handlers** — `TestSpamVerbose`,
   `SpamMetrics` and `SpamInvestigateErrors` on the Intelligence partial, each bound to a live ribbon
   button. Their names resolve correctly, so they are outside finding 1, but they are user-reachable
   unhandled exceptions. Located at lines 267, 272 and 277 of the post-change Intelligence partial,
   after the last diff hunk at line 264; confirmed untouched.

Promotion of these three into their own issues is out of this plan's scope and is the orchestrator's
action.

## Delivery summary for the audit

Three findings closed, each with fail-before evidence where a failing run was possible and a
schema-valid exception dossier where it was not:

- **Finding 1** — four CustomUI action-callback values renamed from the `_Clicked` to the `_Click`
  spelling and one dead button element deleted, verified reflow-independently by element and
  attribute multiset comparison. Two new reflection tests, both demonstrated failing pre-fix on five
  and four names respectively.
- **Finding 2** — the Clear Spam Manager globals dereference extracted into a host-neutral
  `SpamManagerResetGate` with nine tests at 100% line coverage. No inline null guard, no new coverage
  exemption. A fail-before run is structurally impossible and the exception dossier records why.
- **Finding 3** — monotonic ticket plus compare-and-apply on the toggle-state cache, canceled-prime
  handling repaired (CR-2), and the previously untested engines-unavailable guard covered (CR-3). Six
  new tests, three of which were demonstrated failing pre-fix.

Output Summary: The reduced-audit handoff records pointers to the coverage delta, footprint scope,
toolchain closure, acceptance status, completeness and sanitisation artifacts; the
manual-verification status of OPERATOR-ACTION-REQUIRED; the P4-T3 branch B scope amendment; and all
three follow-ups the spec defers, each confirmed untouched by this change.
