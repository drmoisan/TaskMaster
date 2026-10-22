# Acceptance-criteria status summary (P6-T15)

Task: [P6-T15]
Timestamp: 2026-09-13T03-53
Source: `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/spec.md`, section `## Acceptance Criteria` (work mode `full-bug`; five checkbox criteria AC1-AC5, of which AC3 carries components (a) and (b) tracked here as AC3A and AC3B). Evidence paths are relative to the feature folder.

| Identifier | Verdict | Implementing task(s) | Verifying test or command | Evidence artifact path(s) and recorded figures |
|---|---|---|---|---|
| AC1 | RATIFIED NEGATIVE RESULT — NOT A PASS (corrected 2026-09-13; this row read PASS and was wrong) | P0-T11, P1-T6, P1-T7, P1-T9, P1-T10, P1-T11, P4-T1 | `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` in both regimes plus the `GATECOUNTERS` triples (serial `acquisitions=11 releases=10 contended=0`; parallel `19/18/14`). Both regimes recorded `timeout=0`: no expiry occurred, so neither H-COST nor H-LEAK was discriminated and H-LEAK is NOT rejected. Per AC1's own final sentence this is a recorded negative result, not a pass. The spec checkbox is `- [ ]` and stays so. The maintainer ratified the negative result on 2026-09-13 under four conditions, accepting the item despite the negative result rather than finding the result positive. | `evidence/baseline/ac1-observable-declaration.2026-09-12T16-30.md`; `evidence/baseline/ac1-serial-measurement.2026-09-12T17-00.md`; `evidence/baseline/ac1-parallel-measurement.2026-09-12T17-00.md`; `evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md` (see its Correction 2); `evidence/other/maintainer-ratification-ac1.2026-09-13T18-00.md` |
| AC2 | PASS | P2-T1, P2-T2, P2-T3, P2-T6, P2-T7, P3-T1, P3-T2 | `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs` method `ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups`: fail-before 3 of 3 runs (`InvalidCastException`) in P2-T9, pass-after 3 of 3 runs in P3-T6, determinism audit empty match list and exactly 5 `[Timeout(` attributes in P3-T7 | `evidence/regression-testing/ac2-fail-before-three-runs.2026-09-12T17-30.md`; `evidence/regression-testing/ac2-pass-after-three-runs.2026-09-12T18-00.md`; `evidence/regression-testing/ac2-determinism-audit.2026-09-12T18-00.md`; spec check-off P6-T11 |
| AC3A | PASS | P3-T1, P3-T8 | `ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer` and `ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface`, from P3-T6 run 1 (synchronous dispatcher double, zero `WinFormsPumpHost` occurrences, viewer not assignable to the concrete type) | `evidence/regression-testing/ac3a-deterministic-efficacy.2026-09-12T18-00.md`; spec check-off P6-T12 |
| AC3B | PASS | P5-T1 | 62-run SERIAL-regime streak over the seam test class named in AC2: `RUNS=62 FAILURES=0`, N = 62, p-value (20/21)^62 = 0.048558; targeted scope only, not the full multi-assembly suite; base-rate interval caveat recorded | `evidence/regression-testing/ac3b-consecutive-runs.2026-09-12T19-00.md`; spec check-off P6-T12 |
| AC4 | PASS | P0-T9, P6-T5, P6-T6 | Per-file Cobertura extraction for the two controller partials, same session and same command: ViewerSetup.cs 0.904762 (210/190) to 0.906103 (213/193), delta +3 valid and +3 covered, accounted; Initialization.cs 0.950382 (262/249) unchanged; every section 7 disposition-table test passed in the P6-T5 serial run (1400/1400) | `evidence/baseline/phase0-coverage-prechange.2026-09-12T16-30.md`; `evidence/qa-gates/ac4-coverage-comparison.2026-09-12T19-30.md`; `evidence/qa-gates/final-serial-test-run.2026-09-12T19-30.md`; spec check-off P6-T13 |
| AC5 | PASS | P5-T2 | `gh --version` exit code 0 selected the posting branch; two comments posted, `PostedAs: comment`, URLs https://github.com/drmoisan/TaskMaster/issues/511#issuecomment-5652002368 and https://github.com/drmoisan/TaskMaster/issues/571#issuecomment-5652002536; elements (a), (b), (c) present; Designer `EndInit` pair cited at lines 6165 and 6166 | `evidence/issue-updates/issue-511-and-571-reconciliation.2026-09-12T19-00.md`; spec check-off P6-T14 |

Summary (CORRECTED 2026-09-13; the superseded summary is retained immediately below): six rows; five PASS
(AC2, AC3A, AC3B, AC4, AC5); one RATIFIED NEGATIVE RESULT (AC1); zero BLOCKED. Four of the five spec
checkboxes are checked (`- [x]`): AC2, AC3, AC4, AC5. AC1's checkbox is `- [ ]` and remains unchecked by
maintainer ruling, because it records what was measured. The maintainer's ratification is recorded
separately, in `evidence/other/maintainer-ratification-ac1.2026-09-13T18-00.md`, and is deliberately not
expressed as a checkbox: an unchecked box plus a recorded ratification states two different things, and
checking the box would collapse them into a false one.

Superseded summary, retained for the audit trail:

> Summary: six rows; six PASS; zero PARTIAL; zero BLOCKED. All five spec checkboxes AC1-AC5 are checked (`- [x]`) as of P6-T14; checkbox characters only were changed.

That summary was accurate to the tree at P6-T15 and became wrong at commit 9170499b4, which unchecked
AC1 without updating this artifact.
