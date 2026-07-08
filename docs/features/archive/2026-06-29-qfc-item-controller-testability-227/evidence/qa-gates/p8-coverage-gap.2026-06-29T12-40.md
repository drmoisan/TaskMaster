# Phase 8 — Coverage Gap Analysis (P8-T1)

Timestamp: 2026-06-29T12-40
Command: dotnet-coverage merge <P7-T12 .coverage> -f cobertura -o scratch.cobertura.xml ; per-cluster line tally
EXIT_CODE: 0

## Basis

The VS `.coverage` collector does not honor `[ExcludeFromCodeCoverage]` on async state
machines, so the testable non-exempt denominator is computed by excluding the annotated
method source-line ranges (brace-matched) per production partial. The figures below are the
per-cluster non-exempt coverage measured from the P7-T12 run, before the P8-T2 uplift.

## Per-cluster non-exempt coverage at start of Phase 8 (P7-T12 run)

| Cluster file | non-exempt covered/total | % | Gap to 80% |
|---|---|---|---|
| QfcItemController.cs (Properties/INotify) | 38/130 | 29.23% | yes — largest |
| QfcItemController.Conversation.cs | 70/100 | 70.00% | yes |
| QfcItemController.EventWiring.cs | 186/242 | 76.86% | marginal |
| QfcItemController.FolderHandling.cs | 24/59 | 40.68% | yes |
| QfcItemController.MailActions.cs | 24/24 | 100.00% | no |
| QfcItemController.Navigation.cs | 28/28 | 100.00% | no |
| QfcItemController.ViewerSetup.cs | 0/2 | 0.00% | GetItemSummary (COM) |
| EventHandlers / FocusAndTheme / Initialization | 0/0 | n/a | fully exempt |
| AGGREGATE | 370/585 | 63.25% | below 80% |

## Prioritized list of untested testable members (uplift targets for P8-T2)

1. `QfcItemController.cs` main-file properties (92 uncovered) — the highest-yield, lowest-cost
   target: the simple value getters/setters (`ConvOriginID`, `CounterEnter`, `CounterComboRight`,
   `IsChild`, `IsActiveUI`, `Token`, `IsExpanded`, `SelectedFolder`, `Buttons`,
   `ConversationResolver`, tip-collection getters, `TableLayoutPanels`, `Parent`, `ItemHelper`),
   plus the viewer-routed `ItemNumber`/`ItemNumberDigits` digit-formatting branches and `Height`
   (testable via `Mock<IItemViewer>`).
2. `FolderHandling.AssignFolderComboBox` selection path (lines 154–173): the `SetFolderItems` →
   predetermined-vs-index-1 selection routing through the narrowed intent members, testable by
   injecting a `FolderPredictor` seeded with a known `FolderArray` (no COM) and a
   `Mock<IItemViewer>`.
3. `Conversation.LoadConversationResolverAsync` catch blocks (cancellation rethrow; non-cancel
   fault swallow), testable via the `DoLoadConversationResolverCoreAsync` seam.

## Residual (not testable without deferred seams / live host)

- `EventWiring.RegisterFocusAsyncActions` / `RegisterExpandedAsyncActions` inline async-registration
  lambda **bodies** (~56 lines): each closure invokes Outlook/WebView2/UI operations and runs only
  on a live key-press. The registration control flow (the `.Add` calls) is covered; the closure
  bodies are inline (cannot carry `[ExcludeFromCodeCoverage]`) and are not unit-testable.
- `PopulateConversationAsync` non-null path → `RenderConversationCountAsync` (`UiThread.Dispatcher`
  static): blocked by the injectable-`Dispatcher` seam deferred to #197.
- `ViewerSetup.GetItemSummary` (2 lines): reads COM-computed `MailItemHelper` properties.

Numeric headline: aggregate non-exempt 370/585 = 63.25% at start of Phase 8; gap to 80% driven by
main-file properties (92 uncovered) and FolderHandling selection path (35 uncovered).
