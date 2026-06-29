# Coverage Delta vs P0-T5 Baseline (P9-T5)

Timestamp: 2026-06-29T12-50
Command: dotnet-coverage merge <final .coverage> -f cobertura -o scratch-final.cobertura.xml ; covnonexempt/covparse tally
EXIT_CODE: 0

## Repo-wide (first-party testable denominator)

- Baseline (P0-T5): #223-measured first-party testable denominator 73.35%-74.11% (2026-06-28T21-50),
  accepted below the 80% floor under the authority-scoped exception in
  `maintainer-decision.2026-06-29.md`; residual repo-wide uplift tracked under #197.
- Post-change: unchanged at the repo level by this refactor (no production behavior change; net
  test additions are first-party coverage-positive on QfcItemController). The repo-wide floor remains
  satisfied-with-documented-exception under the #223 authority-scoped precedent.
- Single-assembly whole-process line-rate (informational, includes vendored modules): baseline
  10003/76355 = 13.10% -> post-change 10566/75717 = 13.95%.

## QfcItemController production coverage (Cobertura sequence-point basis, excl. *Tests.cs)

- Baseline (P0-T5): 246/3261 = 7.54%.
- Phase 7 (P7-T12): 388/3293 = 11.78%.
- Post-change (P9-T4): covered lines rose with the P8-T2 uplift; the meaningful gate metric is the
  non-exempt testable denominator below.

## Affected testable non-exempt denominator (gate metric, AC5)

| Cluster file | non-exempt covered/total | % |
|---|---|---|
| QfcItemController.cs (Properties/INotify) | 124/130 | 95.38% |
| QfcItemController.Conversation.cs | 70/100 | 70.00% |
| QfcItemController.EventWiring.cs | 186/242 | 76.86% |
| QfcItemController.FolderHandling.cs | 52/59 | 88.14% |
| QfcItemController.MailActions.cs | 24/24 | 100.00% |
| QfcItemController.Navigation.cs | 28/28 | 100.00% |
| QfcItemController.ViewerSetup.cs | 0/2 | 0.00% |
| **AGGREGATE** | **484/585** | **82.74%** |

- **Affected testable non-exempt denominator: 82.74% >= 80% — MET.**

## Changed-line regression check (AC5)

- No changed line shows a coverage regression versus baseline. The split is verbatim (behavior
  preserved); every cluster's non-exempt coverage is at or above its baseline, and the net change is
  strictly additive (main-file properties 29.23% -> 95.38%; FolderHandling selection 40.68% ->
  88.14%; Conversation routing newly covered). No previously-covered line became uncovered.

## New/extracted code >= 90% sub-target — REMEDIATION-REQUIRED

- Aggregate extracted non-exempt code: 82.74% < 90% — NOT MET. Disposition per P8-T7 / P9-T5:
  marked **remediation-required for the 90% sub-target**, NOT PASS. Residual gap, all structurally
  un-coverable after exhausting testable seams with the injectable-`Dispatcher` deferral in force:
  1. `EventWiring` inline async-registration lambda bodies (56 lines) — UI/COM-bound inline closures,
     un-exemptable, executable only on a live key-press. Binding constraint.
  2. `Conversation` `PopulateConversationAsync` non-null render path via `UiThread.Dispatcher`
     (injectable-`Dispatcher` seam deferred to #197) plus async `catch` lines unmapped by the
     `.coverage` collector (~30 lines).
  3. `ViewerSetup.GetItemSummary` (2 lines) — reads COM-computed `MailItemHelper` properties.
- The injectable-`Dispatcher` seam would not reach 90% (best case ~86.8%), so it is not introduced
  this cycle; residual non-exempt uplift on QfcItemController is folded into the #197 follow-up.

Numeric headline: affected testable non-exempt denominator 484/585 = 82.74% (>=80% MET, no
changed-line regression); new/extracted 90% sub-target remediation-required (residual gap recorded).
