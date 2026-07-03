# Phase 8 — Tests + Coverage (P8-T7)

Timestamp: 2026-06-29T12-40
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
Coverage conversion: dotnet-coverage merge <.coverage> -f cobertura -o scratch-p8.cobertura.xml
EXIT_CODE: 0

## Test result

- Total tests: 233; Passed: 233; Failed: 0. (Baseline 201 preserved + 32 new per-cluster tests:
  23 from Phase 7 plus 9 added in P8-T2 — 6 Properties, 2 FolderHandling selection, 2 Conversation
  catch-block, net of formatting.) `EXIT_CODE: 0`.

## Coverage — affected testable non-exempt denominator (gate metric)

The VS `.coverage` collector does not honor `[ExcludeFromCodeCoverage]` on async state machines, so
the non-exempt denominator is computed by excluding the annotated method source-line ranges
(brace-matched) per production partial.

| Cluster file | non-exempt covered/total | % |
|---|---|---|
| QfcItemController.cs (Properties/INotify) | 124/130 | 95.38% |
| QfcItemController.Conversation.cs | 70/100 | 70.00% |
| QfcItemController.EventWiring.cs | 186/242 | 76.86% |
| QfcItemController.FolderHandling.cs | 52/59 | 88.14% |
| QfcItemController.MailActions.cs | 24/24 | 100.00% |
| QfcItemController.Navigation.cs | 28/28 | 100.00% |
| QfcItemController.ViewerSetup.cs | 0/2 | 0.00% |
| **AGGREGATE (affected testable non-exempt denominator)** | **484/585** | **82.74%** |

- **Affected testable non-exempt denominator: 82.74% >= 80% — MET.** Up from 63.25% at P8 start;
  the P8-T2 uplift raised the main-file properties (29.23% -> 95.38%) and the FolderHandling
  selection path (40.68% -> 88.14%).

## Coverage — new/extracted code >= 90% sub-target

- **Aggregate extracted non-exempt code: 82.74% < 90% — NOT MET.**
- The genuinely-new logic introduced by the narrowing (the `AssignFolderComboBox` intent-method
  rewrite, the `RenderConversationCount(int)` intent-member routing, and the property routing
  through the narrowed `IItemViewer`) IS covered at/above 90%. The shortfall is concentrated in
  verbatim-extracted code whose uncovered lines are structurally un-coverable:
  1. `EventWiring.RegisterFocusAsyncActions` / `RegisterExpandedAsyncActions` inline
     async-registration lambda **bodies** (56 uncovered lines = 242 - 186): each closure calls
     Outlook/WebView2/UI operations and executes only on a live key-press; the closures are inline
     and cannot carry `[ExcludeFromCodeCoverage]`, and invoking them in a unit test is not reliably
     possible without a live host. This is the binding constraint.
  2. `Conversation` async paths (30 uncovered): the `PopulateConversationAsync` non-null path routes
     through `RenderConversationCountAsync` (`UiThread.Dispatcher`), and the async-method `catch`
     lines do not map back to source lines under the `.coverage` collector. The `Dispatcher` paths
     require the injectable-`Dispatcher` seam.
  3. `ViewerSetup.GetItemSummary` (2 uncovered): reads COM-computed `MailItemHelper` properties.

## Disposition — REMEDIATION-REQUIRED for the 90% sub-target

Per P8-T7: the 80% affected-testable-non-exempt-denominator gate is MET (82.74%); the 90%
new/extracted sub-target is **unmet after exhausting testable seams**, and the injectable-`Dispatcher`
deferral remains in force. The injectable-`Dispatcher` seam (Non-Goal, deferred to #197) would not
close the gap to 90%: the binding constraint is the un-exemptable inline async-registration lambda
bodies in `EventWiring`, not the `Dispatcher` paths; with the `Dispatcher` paths fully covered the
achievable aggregate is approximately (484+24)/585 = 86.8%, still below 90%. Introducing the
`Dispatcher` seam is therefore not warranted by this gate. The outcome is recorded as
**remediation-required for the 90% new/extracted sub-target** rather than PASS, with the residual
gap above. The 80% testable-denominator floor is satisfied.

Numeric headline: 233 passed; affected testable non-exempt denominator 484/585 = 82.74% (>=80% MET);
new/extracted aggregate 82.74% (<90% — remediation-required; residual gap = EventWiring inline
async-registration lambda bodies + Dispatcher-bound Conversation render + GetItemSummary COM read).
