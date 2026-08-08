# P7-T1 — Post-Change Exemption Census

Issue: #230
Task: [P7-T1]

- Timestamp: 2026-08-07T23-30
- Command: `Select-String -Pattern 'ExcludeFromCodeCoverage' -Path QuickFiler/Controllers/QfcItemController.*.cs`
- EXIT_CODE: 0 (derived from `$?` = `True` per D14)
- Output Summary: **Exactly 11 `[ExcludeFromCodeCoverage]` sites remain**, down from
  the 19 recorded in the P0-T7 pre-change baseline. All 8 target members were
  de-exempted. `QfcItemController.Initialization.cs` now contains **zero**
  exemption sites (was 7).

## Remaining 11 sites

| # | File | Line | Member | Justification category |
|---|---|---|---|---|
| 1 | `QfcItemController.Conversation.cs` | 79 | `DoLoadConversationResolverCoreAsync` | Test seam for the static `ConversationResolver.LoadAsync` call (ratified) |
| 2 | `QfcItemController.EventHandlers.cs` | 60 | `BtnPopOut_Click` | Thin async-void WinForms-event shell; routing tested via its core (ratified) |
| 3 | `QfcItemController.EventHandlers.cs` | 83 | `BtnReply_Click` | Thin async-void shell; tested via `BtnReplyCore` (ratified) |
| 4 | `QfcItemController.EventHandlers.cs` | 97 | `BtnReplyAll_Click` | Thin async-void shell; tested via `BtnReplyAllCore` (ratified) |
| 5 | `QfcItemController.EventHandlers.cs` | 111 | `BtnForward_Click` | Thin async-void shell; tested via `BtnForwardCore` (ratified) |
| 6 | `QfcItemController.EventHandlers.cs` | 125 | `TxtboxBody_DoubleClick` | Thin async-void shell; tested via `TxtboxBodyDoubleClickCore` (ratified) |
| 7 | `QfcItemController.EventWiring.cs` | 99 | `WebView2Control_CoreWebView2InitializationCompleted` | Thin async-void shell forwarding to a testable core (ratified) |
| 8 | `QfcItemController.Navigation.cs` | 173 | `ToggleExpansion(Enums.ToggleState)` | `TlpCellSnapShot`-bound state body, made virtual so routing is tested (ratified) |
| 9 | `QfcItemController.Navigation.cs` | 191 | `ToggleExpansionAsync(Enums.ToggleState)` | Same as #8 (ratified) |
| 10 | `QfcItemController.ViewerSetup.cs` | 41 | `InitializeWebViewAsync` | **Retained with updated justification** — see below |
| 11 | `QfcItemController.ViewerSetup.cs` | 135 | `EnsureBreadcrumbPipeline` | Post-ratification (#351), **out of #230's scope** — see below |

Raw `Select-String` count: `COUNT=11`.

## The 8 members de-exempted by this feature

| Member | File | Removal phase | Covering test |
|---|---|---|---|
| `ResolveControlGroupsAsync(ItemViewer)` | `QfcItemController.ViewerSetup.cs` | P2-T2 | `QfcItemController_ViewerSetupTests.ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups` |
| `InitializeGraphicsAsync()` | `QfcItemController.Initialization.cs` | P3-T3 | `QfcItemController_InitializationTests.InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` |
| `InitializeSequentialAsync()` | `QfcItemController.Initialization.cs` | P3-T3 | `QfcItemController_InitializationTests.InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` |
| `Initialize(...)` (private 9-arg) | `QfcItemController.Initialization.cs` | P4-T3 | `QfcItemController_InitializationTests.InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` |
| `Initialize(bool async)` | `QfcItemController.Initialization.cs` | P4-T3 | `QfcItemController_InitializationTests.InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` |
| `CreateAsync(...)` | `QfcItemController.Initialization.cs` | P5-T5 | `QfcItemController_SeamFactoryTests.CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing` |
| `CreateSequentialAsync(...)` | `QfcItemController.Initialization.cs` | P5-T5 | `QfcItemController_SeamFactoryTests.CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController` |
| `InitializeAsync()` | `QfcItemController.Initialization.cs` | P6-T2 | `QfcItemController_InitializationTests.InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` |

Every attribute was removed in the **same change** as the test that covers its
member, per S-AC7.

## `InitializeWebViewAsync` — retained, justification updated (P6-T3)

The attribute is unchanged and still present at
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:41`. Its comment now
records:

- The **pump barrier is resolved** by the #230 `WinFormsPumpHost` seam; the
  `await _itemViewer.UiSyncContext` on the following line is drainable and tests do
  reach the `IWebViewCoreInitializer` seam call.
- The **residual barrier** is the
  `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2` dependency, which is null
  unless the real WebView2 runtime initialized the control — an external process
  barred by the repository unit-test policy. With the mocked initializer, execution
  must stop at the seam call (controlled fault), so the member cannot be
  meaningfully covered end-to-end.
- The separate **concrete-accessor barrier** (`IItemViewer` intentionally exposes no
  WebView-core-init intent member, so the concrete cast cannot execute against a
  `Mock<IItemViewer>`) is **tracked separately per issue #230**.

## `EnsureBreadcrumbPipeline` — post-ratification, out of scope

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:135` was added by **issue
#351**, after the 2026-07-02 ratification, and is therefore not one of the
controller members in the ratified boundary. It is outside #230's 9-member scope
(spec.md Non-Goal 2) and was **not changed** by this feature. Its existing
justification (host-neutral breadcrumb pipeline created idempotently on the
concrete viewer; the 9101 provider is DI-resolved from the injected globals' folder
-tree service seam; skipped for mock viewers) is unchanged.

## Resulting boundary for maintainer re-ratification

**19 -> 11 sites** within `QuickFiler/Controllers/QfcItemController.*.cs`.

- Pre-change baseline: `evidence/baseline/exclusion-census-pre.2026-08-07T21-50.md`
  (19 sites).
- Ratified boundary of record:
  `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T17-00.md`,
  ratified in the sibling `maintainer-decision.2026-07-02.md`, which pre-authorizes
  #230 as the follow-up for the 9-member "category 1" bucket.
- Of that bucket, 8 members are now de-exempted and covered; the 9th
  (`InitializeWebViewAsync`) is retained for a genuine external-dependency reason.
- **No remaining exemption in the controller partials is blocked solely on missing
  WinForms pump infrastructure.**

Per the spec Implementation Strategy, re-ratification of the reduced boundary is a
maintainer PR-review step, not a plan task.
