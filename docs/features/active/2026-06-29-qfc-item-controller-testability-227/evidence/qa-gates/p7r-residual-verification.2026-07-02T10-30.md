# Phase 7 Gate — Residual Exemption Verification (P7-T2)

Timestamp: 2026-07-02T10-30
Command: grep -rn "ExcludeFromCodeCoverage" QuickFiler/Controllers/QfcItemController*.cs plus per-member source review; verified each attribute is immediately preceded by a per-member justification comment.
EXIT_CODE: 0

## P7-T1 usage disposition of the four raw-WinForms-parameter overloads

`grep -rn` across the solution found ZERO live call sites for all four overloads (every reference was
commented out; the only `ToggleCheckboxAsync(CheckBox)` implementation elsewhere belongs to the
unrelated `EfcFormController`). They were dead after the cycle-1 Seam B narrowing and were **removed**
(P7-T1):

- `ToggleCbMenuItemAsync(ToolStripMenuItemCb)` — removed.
- `ToggleCbMenuItemAsync(ToolStripMenuItemCb, Enums.ToggleState)` — removed.
- `ToggleCheckboxAsync(CheckBox)` — removed.
- `ToggleCheckboxAsync(CheckBox, Enums.ToggleState)` — removed.

None were declared on `IQfcItemController`/`IItemControler`, so removal is non-breaking. This dropped
the Navigation-partial exemption count from 9 to 5.

## Final residual `[ExcludeFromCodeCoverage]` inventory (each individually justified, no blanket/category basis)

### QfcItemController partials — 38 members

**Initialization.cs (7)** — orchestration of concrete control-tree construction:
1. `Initialize(...9-arg private...)` — funnels into `Initialize(bool)`.
2. `Initialize(bool async)` — calls `ResolveControlGroups((ItemViewer)_itemViewer)`, `SetupThemes((ItemViewer)...)`, `WireEvents`.
3. `InitializeAsync` — async control-tree orchestration + `InitializeWebViewAsync`.
4. `InitializeGraphicsAsync` — same concrete orchestration.
5. `InitializeSequentialAsync` — same concrete orchestration.
6. `CreateAsync` (static factory) — constructs + awaits `InitializeAsync`; barrier inherited.
7. `CreateSequentialAsync` (static factory) — constructs + awaits `InitializeSequentialAsync`.

**ViewerSetup.cs (5)**:
8. `InitializeWebViewAsync` — WebView2 SDK calls now seamed via `IWebViewCoreInitializer`, but retains the concrete `((ItemViewer)_itemViewer).L0v2h2_WebView2` access + `await _itemViewer.UiSyncContext`; `IItemViewer` exposes no WebView-core-init intent by design. **Reclassified in Phase 6.**
9. `ResolveControlGroups(ItemViewer)` — control-tree traversal is the method's entire purpose.
10. `ResolveControlGroupsAsync(ItemViewer)` — async control-tree traversal.
11. `PopulateControls(MailItem, int)` — builds `MailItemHelper` from a live COM `MailItem`.
12. `PopulateControlsAsync` — `MailItemHelper.FromMailItemAsync(mailItem, ...)` from a live COM `MailItem`.

**FolderHandling.cs (4)** — COM-bound `FolderPredictor` (out-of-scope collaborator):
13. `LoadFolderHandler` — constructs `FolderPredictor`.
14. `LoadFolderHandlerAsync` — constructs `FolderPredictor`.
15. `PopulateFolderComboBox` — transitively via `LoadFolderHandler`.
16. `PopulateFolderComboBoxAsync` — transitively via `LoadFolderHandlerAsync`.

**EventWiring.cs (4)**:
17. `WireEvents` — delegates to the concrete `WireControlTreeEvents`.
18. `WireControlTreeEvents` — `ForAllControls` traversal + `Buttons`/`MenuItems` concrete-control loops.
19. `WebView2Control_CoreWebView2InitializationCompleted` — thin async-void shell; substantive body extracted to the tested `HandleWebViewInitializedAsync`.
20. `RegisterExpandedActions` — the `'B'`/`'D'` lambdas focus the concrete `L0v2h2_WebView2`/`TopicThread` (no intent member by design).

**EventHandlers.cs (7)** — thin WinForms-event shells / COM:
21. `BtnFlagTask_Click` — thin shell → `FlagAsTask`.
22. `BtnPopOut_Click` — thin async-void shell (core `BtnPopOutCore` tested).
23. `BtnReply_Click` — thin async-void shell (core `BtnReplyCore` tested).
24. `BtnReplyAll_Click` — thin async-void shell (core `BtnReplyAllCore` tested).
25. `BtnForward_Click` — thin async-void shell (core `BtnForwardCore` tested).
26. `TxtboxBody_DoubleClick` — thin async-void shell (core `TxtboxBodyDoubleClickCore` tested).
27. `TextBoxSearch_TextChanged` — `_folderHandler.FindFolder(objItem: Mail)` (COM FolderPredictor + live Mail).

**Navigation.cs (5)**:
28. `JumpToAsync(Control)` — raw `Control` parameter; called only with concrete WebView2/TopicThread targets.
29. `ToggleExpansion(Enums.ToggleState)` — `virtual` test seam; production body calls the TlpCellSnapShot-bound `ToggleExpansionOn/Off`.
30. `ToggleExpansionAsync(Enums.ToggleState)` — `virtual` test seam; same TlpCellSnapShot barrier.
31. `ToggleExpansionOff` — `TlpCellSnapShot.ApplyState((ItemViewer)_itemViewer)` (out-of-scope; see P7-T5).
32. `ToggleExpansionOn` — `TlpCellSnapShot.ApplyState((ItemViewer)_itemViewer)` (out-of-scope; see P7-T5).

**FocusAndTheme.cs (5)** — out-of-scope `Theme` collaborator (handle-bound controls, no Theme seam this cycle):
33. `ToggleFocus(Enums.ToggleState)` — body inside one `_itemViewer.Invoke` terminating in `Theme.SetQfcTheme(async:false)`.
34. `ToggleFocusAsync(Enums.ToggleState)` — awaits `Theme.SetQfcThemeAsync()`.
35. `ToggleFocus()` — same `Theme.SetQfcTheme` barrier.
36. `ToggleFocusAsync()` — same `Theme.SetQfcThemeAsync` barrier.
37. `ApplyReadEmailFormat` — COM writes now seamed via `IMailItemActions`, but calls `Theme.SetMailRead(async:true)` → `_lblSender.BeginInvoke` on a handle-bound control. **Reclassified in Phase 6.**

**Conversation.cs (1)**:
38. `DoLoadConversationResolverCoreAsync` — deliberate `virtual` override seam wrapping the static `ConversationResolver.LoadAsync`; overridden in tests, production body intentionally never exercised.

### DI-seam adapter shims — 3 classes

39. `WpfUiDispatcher` — thin 1:1 forwarder to the static `UiThread.Dispatcher` (requires a live WPF pump).
40. `WebView2CoreInitializer` — thin 1:1 forwarder to the WebView2 SDK (requires the WebView2 runtime).
41. `MailItemActionsAdapter` — thin 1:1 forwarder to a live Outlook `MailItem` COM object.

## Result

- Total residual `[ExcludeFromCodeCoverage]`: **41** (38 controller members + 3 adapter shims).
- Every residual is named individually with a specific, technical, non-category reason and carries an
  inline per-member justification comment immediately preceding the attribute (verified programmatically:
  "ALL residual exemptions have a preceding comment line").
- No member remains exempt on a blanket/category (per-partial-file) basis.
- The plan's directional estimate (~6–8 + shims) is superseded by this honest per-member tally, as the
  seam-redesign research §4 explicitly anticipated ("the coverage-gate artifacts, not this plan's
  estimates, are the final tally"). The larger count reflects genuine bucket-(iii) barriers:
  control-tree orchestration/traversal, COM-bound collaborators (`MailItem`, `FolderPredictor`,
  `FlagTasks`), the out-of-scope `TlpCellSnapShot` and `Theme` collaborators, deliberate virtual test
  seams, and thin WinForms-event/DI-adapter shims.

Output Summary: 4 dead raw-parameter overloads removed (P7-T1); 41 residual exemptions remain (38
controller members + 3 adapter shims), each individually justified with a per-member technical reason
and inline comment. No blanket/category exemption remains.
