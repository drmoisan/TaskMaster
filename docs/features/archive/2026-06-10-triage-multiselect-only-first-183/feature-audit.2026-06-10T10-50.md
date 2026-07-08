# Feature Audit: Triage_OlLogic multi-select UDF fix (Issue #183) — Cycle-1 Exit Reaudit

**Audit Date:** 2026-06-10
**Work Mode:** `minor-audit`
**AC Source:** `docs/features/active/2026-06-10-triage-multiselect-only-first-183/issue.md` `## Acceptance Criteria` (AC1–AC5) — the ONLY AC source for `minor-audit`.

## Scope and Baseline

- **Branch:** `bug/triage-multiselect-only-first-183`
- **Base:** `main`, merge-base `c8feca8c`
- **Cycle:** Cycle-1 exit reaudit. The cycle-entry review (`feature-audit.2026-06-10T09-43.md`) found AC1–AC5 all PASS but raised one policy-conformance blocker (R1: 553-line test file). This reaudit re-verifies AC1–AC5 after the remediation split.
- **This cycle's changes:** Test-organization split only — `Triage_OlLogicTests.cs` (270 lines), new `Triage_OlLogicTests.TrainSelection.cs` (300 lines), one csproj `<Compile Include>`. No production file changed; the fix behavior in `Triage_OlLogic.cs` (269 lines) is unchanged from the implementation cycle.
- **Baseline behavior:** Pre-fix, `TrainSelectionAsync` deduped the entire selection loop by `ConversationID`, suppressing the UDF write for every item after the first in a conversation. The committed fix decouples the UDF write (per item) from training (deduped per ConversationID).

## Acceptance Criteria Inventory

| ID | Criterion (abbreviated) |
|----|--------------------------|
| AC1 | UDF `SetUdf("Triage", triageId)` written to every selected `MailItem` sharing a `ConversationID`, not only the first |
| AC2 | #137 training dedup preserved: classifier trained at most once per distinct `ConversationID`; `TotalEmailCount`/`MatchEmailCount` increment exactly once for a multi-item single-conversation selection |
| AC3 | Deterministic MSTest regression test in `Triage_OlLogicTests` proves AC1; existing #137 dedup tests continue to pass unchanged |
| AC4 | Fix confined to triage selection path (`Triage_OlLogic.cs` + its test file); no unrelated production behavior change |
| AC5 | Full C# toolchain passes in a single clean pass; changed-line coverage does not regress |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence (post-split re-verification) |
|----|---------|----------------------------------------|
| AC1 | PASS | Regression test `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_WritesTriageUdfToEveryItem` (now `Triage_OlLogicTests.TrainSelection.cs` lines 222-298) verifies `mockMailItem1.Verify(m => m.Save(), Times.Once)` and `mockMailItem2.Verify(m => m.Save(), Times.Once)` — the `Save()` proxy for the swallowed `SetUdf` write on both same-conversation items. Test passes in the post-split run. |
| AC2 | PASS | Same test asserts `TotalEmailCount.Should().Be(emailCountBefore + 1)`; plus `...TotalEmailCountIncrementsOnce` and `...MatchEmailCountIncrementsOnce` (#137) assert single increment. All pass post-split. |
| AC3 | PASS | The regression test and the two #137 dedup tests are deterministic (Moq-only, no I/O/clock) and pass unchanged after the move. Method assertions are byte-identical to the committed baseline. |
| AC4 | PASS | No production file changed this cycle (`git status` non-test `.cs` = none). The earlier `Triage_OlLogic.cs` change is confined to the triage selection path; no unrelated production behavior change. The split touches only test files + csproj. |
| AC5 | PASS | Full C# toolchain re-ran in order with clean first-party pass: CSharpier EXIT_CODE 0, analyzer build EXIT_CODE 0, nullable/TWAE build EXIT_CODE 0, MSTest 21/21 in scope. Coverage 87.23% post-change vs 87.23% baseline (+1 covered line; not-covered unchanged) — no regression. Evidence: `evidence/qa-gates/remediation-coverage-comparison.2026-06-10T09-43.md`. |

The single pre-existing unrelated failure (`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`) is identical at baseline, passes in isolation, and does not affect any AC verdict.

## Acceptance Criteria Check-off

All five criteria are already checked `[x]` in `issue.md` (lines 68-72) from the prior cycle and remain satisfied after the split. No checkbox change is required; no item is downgraded.

- [x] AC1 — PASS (re-verified post-split)
- [x] AC2 — PASS (re-verified post-split)
- [x] AC3 — PASS (re-verified post-split)
- [x] AC4 — PASS (re-verified post-split)
- [x] AC5 — PASS (re-verified post-split)

### Acceptance Criteria Status
- Source: `docs/features/active/2026-06-10-triage-multiselect-only-first-183/issue.md`
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

## Summary

All five acceptance criteria (AC1–AC5) remain PASS after the cycle-1 remediation split. The split changed only test organization (two partial-class files under 500 lines, 21 `[TestMethod]` preserved verbatim) plus a one-line csproj include; it introduced no production change and no test weakening. The #183 regression test and the #137 dedup tests pass unchanged, and the full C# toolchain passes in order with no coverage regression. The cycle-entry blocking finding R1 is resolved.

**Feature-audit verdict: PASS. Blocking findings in this artifact: 0.**
