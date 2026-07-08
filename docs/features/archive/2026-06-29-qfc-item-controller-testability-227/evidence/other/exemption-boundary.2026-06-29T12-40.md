# Phase 8 — Exemption Boundary (P8-T3)

Timestamp: 2026-06-29T12-40

Method-level `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` was applied to the
unresolvably COM/Outlook/WinForms-bound members enumerated in research §6.4 and to the
control-host-bound concrete-bound paths established by the P2-T4 seam. Each exemption is at
method granularity. This artifact records the exempt/non-exempt boundary for maintainer
ratification at review (AC5).

## Rationale categories

- **(A) Construction / control-tree wiring** — builds or resolves the live WinForms control tree
  and the WebView core; cannot run without a constructed `ItemViewer` and Outlook host.
- **(B) Outlook/WebView2 core-init** — `EnsureCoreWebView2Async` and the init-completed handler.
- **(C) `async void` UI event handlers** — Outlook ribbon/button/textbox event handlers bound to
  `MailItem.Reply/ReplyAll/Forward`, deletion, pop-out, double-click.
- **(D) `_itemViewer.Invoke`-only focus/theme/navigation** — methods whose entire body marshals to
  the UI thread and mutates concrete controls, the theme engine, or WebView HTML.
- **(E) Concrete-bound control-host paths (P2-T4 seam)** — consume raw concrete control members
  with no intent-member substitute: `SetupThemes` theme-setup call path
  (`ResolveControlGroups`/`ResolveControlGroupsAsync`), WebView core-init `InitializeWebViewAsync`,
  and the expanded-action registration lambdas in `RegisterExpandedActions`/`RegisterExpandedAsyncActions`.
- **(F) `UiThread.Dispatcher`-bound async render** — `PopulateConversation`/`RenderConversationCount(Async)`
  paths whose body is a `Dispatcher` closure; testable only via the injectable-`Dispatcher` seam
  deferred to #197.

## Exempted members (by partial)

### QfcItemController.Initialization.cs — category A
Constructors (`protected QfcItemController()`, the three public ctors), `Initialize` (x2 overloads),
`InitializeAsync`, `InitializeGraphicsAsync`, `InitializeSequentialAsync`, `SaveParameters`,
`CreateAsync`, `CreateSequentialAsync`.

### QfcItemController.ViewerSetup.cs — categories A, B, E
`InitializeWebViewAsync` (B/E, `EnsureCoreWebView2Async`), `ResolveControlGroups` /
`ResolveControlGroupsAsync` (E), `PopulateControls` (x2), `PopulateControlsAsync`,
`AssignControlsAsync`, `AssignControls`, `Cleanup`.

### QfcItemController.Conversation.cs — categories D, F
`PopulateConversation()` / `PopulateConversation(ConversationResolver)` / `PopulateConversation(int)`
(F), `DoLoadConversationResolverCoreAsync` (test seam; exempt because its body is the static
`ConversationResolver.LoadAsync` COM call — overridden in tests), `RenderConversationCount()` and
`RenderConversationCountAsync` (F), `SetTopicThread` (D).

### QfcItemController.FolderHandling.cs — category A/D
`LoadFolderHandler`, `LoadFolderHandlerAsync` (build `FolderPredictor` from COM `MailItemHelper`),
`PopulateFolderComboBox`, `PopulateFolderComboBoxAsync` (`Dispatcher`/`Invoke` marshaling).

### QfcItemController.EventWiring.cs — categories A, B, E
`WireEvents` (A, `ForAllControls`), `WebView2Control_CoreWebView2InitializationCompleted` (B),
`RegisterFocusActions` (E, concrete-bound focus targets), `RegisterExpandedActions` (E),
`UnregisterFocusActions`, `UnregisterExpandedActions`.

### QfcItemController.EventHandlers.cs — category C
`CbxConversation_CheckedChanged`, `BtnFlagTask_Click`, `BtnPopOut_Click`, `BtnDelItem_Click`,
`BtnReply_Click`, `BtnReplyAll_Click`, `BtnForward_Click`, `TxtboxBody_DoubleClick`,
`Button_MouseEnter`/`MouseLeave`, `MenuItem_MouseEnter`/`MouseLeave`, `TextBoxSearch_TextChanged`,
`TextBoxSearch_KeyDown`, `TopicThread_ItemSelectionChanged`, `CbxEmailCopy_CheckedChanged`,
`CboFolders_SelectedIndexChanged`, `CbxAttachments_CheckedChanged`.

### QfcItemController.Navigation.cs — category D
`JumpToFolderDropDown(Async)`, `JumpToSearchTextbox`, `JumpToAsync`, `MenuDropDown`, `Reply`,
`ReplyAll`, `Forward`, `ToggleCbMenuItemAsync` (x2), `ToggleCheckboxAsync` (x2),
`ToggleConversationCheckbox` (x2), `ToggleExpansion`/`ToggleExpansionAsync` (x2 each),
`ToggleExpansionOff`, `ToggleExpansionOn`.

### QfcItemController.FocusAndTheme.cs — category D
`ToggleFocus`/`ToggleFocusAsync` (x2 each), `ToggleFocusOnAsync`, `ToggleFocusOffAsync`,
`ToggleNavigation` (x2)/`ToggleNavigationAsync`, `ToggleTips`/`ToggleTipsAsync`, `InvokeBeginInvoke`,
`ToggleSaveAttachments`, `ToggleSaveCopyOfMail`, `SetThemeDark`, `HtmlDarkConverter`,
`SetThemeLight`, `ApplyReadEmailFormat`.

### QfcItemController.MailActions.cs — categories C, D
`CollapseConversation`, `EnumerateConversation`/`EnumerateConversationAsync`, `MoveMailAsync`,
`FlagAsTask`, `FlagAsTaskAsync`, `MarkItemForDeletionAsync`.

## Testable seams explicitly NOT exempted (must meet the floor)

- `FolderHandling.AssignFolderComboBox` and `FolderHandling.PopulateAndSelectFolder` — covered by
  `QfcItemController_FolderHandlingTests`.
- `Conversation.LoadConversationResolverAsync`, `PopulateConversationAsync` (both overloads),
  `RenderConversationCount(int)` — covered by `QfcItemController_ConversationTests`.
- `EventWiring.RegisterFocusAsyncActions`, `RegisterExpandedAsyncActions`,
  `UnregisterFocusAsyncActions`, `UnregisterExpandedAsyncActions` — covered structurally by
  `QfcItemController_EventWiringTests`. The inline async lambda **bodies** registered inside these
  methods remain uncovered (UI/COM-bound, inline, un-exemptable) and are the dominant residual gap.
- `Navigation.PackageItems`, `MailActions.MarkItemForDeletion` — covered by
  `QfcItemController_NavigationTests` / `QfcItemController_MailActionsTests`.
- `MailActions.KbdExecuteAsync` (both overloads) — covered by `QfcItemController_MailActionsTests`.
- All main-file properties and `NotifyPropertyChanged`, `TopFolderScore` — covered by
  `QfcItemController_PropertiesTests`.
- `ViewerSetup.GetItemSummary` — NOT exempted per plan; remains uncovered (2 lines) because it reads
  COM-computed `MailItemHelper` properties. Recorded as a residual non-exempt gap, not exempted.

## Boundary summary

Exempt method count applied this cycle: Initialization 12, ViewerSetup 9, Conversation 7,
FolderHandling 4, EventWiring 6, EventHandlers 18, Navigation 20, FocusAndTheme 18, MailActions 9
(total 103). Non-exempt testable denominator after exemptions: 585 lines.
