# Increment 2 — Coverage Delta

Timestamp: 2026-06-14T08-22

Command: dotnet-coverage merge (TestResults *.coverage) --output-format cobertura -> artifacts/csharp/inc2.cobertura.xml; per-class line analysis

EXIT_CODE: 0

## Baseline

Production-only baseline (post-#197, 197-COV-001): 71.65%.
Pre-feature QuickFiler assembly line-rate (artifacts/csharp/coverage-firstparty.cobertura.xml):
0.2520 (25.20%).

## Covered-line results on the named QuickFiler seams (inc2.cobertura.xml)

| Seam | Line-rate after Inc 2 |
|---|---|
| KaChar | 82.35% |
| KaCharAsync | 81.25% |
| KaKey | 82.35% |
| KaKeyAsync | 81.25% |
| KaStringAsync | 100% |
| KbdActions<> | 87.95% (with existing KbdActionsTests) |
| FilerQueueItem | 100% |
| FilerQueue (pure subset) | 30% |
| QfcQueue (pure subset) | 14.53% |

## New/changed-code coverage

The new code added by this increment is the 6 test files; their executed lines are at 100%
(KbdActionsRemainingBranchesTests, KaStringAsyncTests classes show line-rate 1.0). The targeted
production members are covered:
- Ka* value objects: SourceId/Key/Delegate/KeyEquals and (KaChar) DelegateType are exercised. The
  residual ~18% on KaChar/KaKey/KaCharAsync/KaKeyAsync is the `Update` (Action<string>) get/set
  property, which is unused UI glue not part of the targeted key/delegate/KeyEquals contract.
- KbdActions<>: Find/FindIndex (0/1/ambiguous), Add(instance)+duplicate, Remove (present/absent),
  indexer get/set, enumeration, Keys, FilterKeys, ContainsKey all covered (87.95% overall with the
  pre-existing tests; residual is the logger-bearing Add(sourceId) duplicate path).
- FilerQueueItem: 100%. KaStringAsync: 100%.

## Restricted seams (recorded, not a remediation trigger)

- FilerQueue.Enqueue/ConsumeAsync: dispatches to EmailFiler.SortAsync on a background task
  (Outlook-bound, non-deterministic) — excluded; only FilerQueueItem + the default consumer are
  covered. Low whole-class rate (30%) is expected and acceptable.
- QfcQueue: only the pure Outlook/WinForms-free paths (Count, JobsRunning, TryDequeueAsync empty,
  CompleteAddingAsync/JobsToFinish with no jobs) are covered. The TLP/MailItem/dispatcher majority
  is Outlook/WinForms-bound and out of scope (low whole-class rate 14.53% is expected).

## Disposition

- Covered-line count on the named QuickFiler seams INCREASED across all targets.
- No regression on changed lines (test-only addition).
- New-code (test file) coverage = 100% on executed lines; targeted production-member coverage is
  high. Sub-90% whole-class figures are confined to (a) unused UI-glue properties on the Ka* value
  objects and (b) the explicitly-excluded Outlook/WinForms paths of FilerQueue/QfcQueue, neither of
  which is a remediation trigger.

Outcome: PASS.
