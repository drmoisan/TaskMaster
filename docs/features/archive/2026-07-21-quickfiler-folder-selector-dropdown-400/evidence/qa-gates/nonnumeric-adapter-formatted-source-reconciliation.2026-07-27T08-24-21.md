# P9-T12 formatted-source reconciliation

Timestamp: 2026-07-27T08:24:21Z
Predecessor evidence: `evidence/qa-gates/nonnumeric-adapter-final-csharpier.2026-07-27T08-24.md`
Inspection: retained CSharpier hash ledger; static source, project-include, exclusion, test-declaration, and scope inspection; `git diff --check`.
Commands not run: CSharpier, build, analyzer, nullable, VSTest, and coverage commands.

## Retained formatter delta

The retained P9-T16 artifact records that CSharpier `format` changed exactly
the five P9-T12/P9-T13 authorized C# files and that its subsequent `check`
returned exit code 0. At review start, each file hash matched the artifact's
recorded post-format SHA-256 value:

| Path | Retained post-format SHA-256 | Review-start match |
| --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | `32FC3630C813E14DF55C702876AE2D5FCB0B713B0314D666B703F6BCBD892F31` | yes |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | `187827EE6093B1B6797BBDB56CC4D92C6CC7778A3BA064E4C1ADFEAA99774170` | yes |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | `9964C68C70D66F287D5A3CEDF88362CD7B70CE16F33BC24027BD28BD3699AFF4` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | `7446D6EC089348E0ED2D03C7B4D158921F6490EE3AD13D1E231142E5C709EFF0` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | `37C6D4656AF85D2A03B3C2D2A43CF9BA11911822C01247A3E425F568AB78BB22` | yes |

The retained five-file CSharpier delta is therefore formatting-only. Static
review found one pre-existing P9-T12 contract defect in the retained popup
source: the seventh excluded method, `NavigateToDocument`, included three
null-validation branches. This was not created by formatting, but it was not
permitted by the P9-T11 thin-direct-adapter requirement.

The minimal correction retained the `NavigateToDocument` signature, moved
those validation branches to unexcluded `NavigateToDocumentCore`, and moved
only the direct production event-binding implementation to excluded
`BindProductionNavigation`. The popup file now has SHA-256
`0BE8FAAE1A774332A2B8E0B3A2C99292996D8C5165058D1A7D7B4717EFDD7F8`.
Because this is a source correction after P9-T16, P9-T16 and every downstream
gate must restart.

## P9-T11/P9-T12/P9-T13 static contract

| Invariant | Result |
| --- | --- |
| P9 source/test contract | Exactly the three authorized production sources and two authorized test sources were inspected. |
| Project includes | One adjacent production include and two adjacent test includes are present. |
| File-size cap | Coordinator 481, ItemViewer breadcrumb 292, popup 476, lifecycle tests 234, adapter tests 208 physical lines; all are at most 500. |
| Coverage exclusions | `ItemViewer.cs` retains its sole historical type-level exclusion; `ItemViewer.Breadcrumb.cs` and the coordinator have none; popup has exactly seven exclusions at lines 97, 372, 375, 382, 386, 404, and 439. |
| Thin popup boundary | Validation is unexcluded in `NavigateToDocumentCore`; `BindProductionNavigation` contains the exact CoreWebView2/owner event handler add/remove bindings and exact delegate cleanup. |
| Lifecycle ownership | The coordinator retains stored host handler identity, one subscribe and one exact unsubscribe path, generation invalidation, hub/messenger lifecycle ownership, and host replacement behavior. |
| Required tests | The two P9-T13 source files declare exactly ten `[TestMethod]` members with the ten approved names. Discovery/execution remains the P9-T14 responsibility. |
| Production seams | Tests invoke the coordinator and `BreadcrumbPopupLifecycleOperations` production seam paths; no prohibited live WebView2/Control/Panel, ambient synchronization context, wait, delay, retry, temporary-file, Ignore, or DoNotParallelize construct was found. |
| Whitespace | `git diff --check` exit code 0. |

The declared test names are:

1. `HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder`
2. `CandidateFailure_CleansMessengerAndReadiness`
3. `ResetDispose_LateCallbackDoesNotReattach`
4. `SelectorDelegation_UsesCoordinator`
5. `QueuedGeometryAndFocusGuards_RunOnCreatorThread`
6. `CoreProbe_AbsentAndPresentPaths`
7. `Initializer_ThrowAndNullTaskPaths`
8. `MessengerConstructionFailure_DisposesReadiness`
9. `NavigationBinder_TranslatesDetachesAndCleansOnThrow`
10. `TwoResourceCleanup_ReportsFirstFailureAfterAllCleanup`

RESULT: PASS

Required next step: run a fresh P9-T14 Debug/Any CPU build and focused-test
gate. P9-T16 and every downstream gate must then be rerun from the corrected
source before any later QA conclusion.
