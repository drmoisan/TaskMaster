# Phase 0 — Requirements and evidence base read (P0-T2)

Timestamp: 2026-09-13T00-53
Task: [P0-T2]
Work Mode: full-bug (issue.md line 12). AC source: `spec.md`, section `## Acceptance Criteria` only.

## Files read in full

- `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/spec.md` (561 lines)
- `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/issue.md` (119 lines)
- `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/research/2026-09-12T14-30-itemviewer-ui-marshalling-seam-research.md` (987 lines)
- `evidence/other/issue-reconciliation-and-gh-context.2026-09-12T13-40.md`
- `evidence/other/orchestrator-citation-verification.2026-09-12T13-50.md`
- `evidence/other/orchestrator-constraint-conflict.2026-09-12T14-45.md`
- `evidence/other/orchestrator-gate-contention-mechanism.2026-09-12T14-05.md`
- `evidence/other/orchestrator-run-count-derivation.2026-09-12T14-15.md`
- `evidence/other/orchestrator-seam-design-constraints.2026-09-12T14-25.md`
- `evidence/other/orchestrator-two-execution-regimes.2026-09-12T15-30.md`

The `evidence/other` directory held exactly seven artifacts at read time (Glob over `evidence/other/*.md`).

## Acceptance-criterion identifier mapping (verbatim identifiers used by the plan)

The plan uses six identifiers: AC1, AC2, AC3A, AC3B, AC4, AC5. The spec's `## Acceptance Criteria` section (spec.md lines 409-488) carries exactly five checkbox lines. Mapping:

| Plan identifier | Spec checkbox line | Spec criterion heading (verbatim prefix) | Component |
|---|---|---|---|
| AC1 | spec.md line 413 | `- [ ] **AC1 — Mechanism identified by measurement, not inference.**` | whole criterion |
| AC2 | spec.md line 427 | `- [ ] **AC2 — Deterministic regression test, no sleep, no retry, no timing tolerance.**` | whole criterion |
| AC3A | spec.md line 438 | `- [ ] **AC3 — Efficacy demonstrated, with the run-count scope named.**` | component **(a) BLOCKING, deterministic, single run** (spec.md line 440) |
| AC3B | spec.md line 438 | `- [ ] **AC3 — Efficacy demonstrated, with the run-count scope named.**` | component **(b) SUPPORTING, statistical** (spec.md line 444) |
| AC4 | spec.md line 458 | `- [ ] **AC4 — Coverage of the two named controller partials retained or improved, against a named denominator and named tests.**` | whole criterion |
| AC5 | spec.md line 478 | `- [ ] **AC5 — #511 and #571 reconciled.**` | whole criterion |

Spec AC3's component (a) is tracked by this plan as AC3A and its component (b) as AC3B. The single AC3 checkbox in the spec is checked (P6-T12) only when both components are satisfied.

## Load-bearing corrections carried from the spec (section 2)

- C1: `UiThreadDispatcherGate` and `SwapUiThreadDispatcher` exist in zero `.cs` files; the live mechanism is `UiThreadDispatcherFixture` / `UiThreadDispatcherTransaction` (issue #493).
- C2: `TransactionGate` is still a one-permit `SemaphoreSlim(1,1)` awaited without timeout.
- C3: the executable marshalling sites in the ViewerSetup partial are lines 64, 282, 287, 298, 303 (context path) and 371 (dispatcher path).
- C4: `InitializeWebViewAsync` (line 47 attribute) and the whole `ItemViewer` type (line 20 attribute) are excluded from coverage.
