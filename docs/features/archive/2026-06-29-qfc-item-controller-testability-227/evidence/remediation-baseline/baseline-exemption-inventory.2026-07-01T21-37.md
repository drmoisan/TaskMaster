# Baseline — [ExcludeFromCodeCoverage] Inventory (Cycle-2 Remediation)

Timestamp: 2026-07-01T21-37
Command: grep -c "ExcludeFromCodeCoverage" on each QfcItemController*.cs partial; grep -A1 to extract member declarations
EXIT_CODE: 0

## Per-partial exemption counts

| Partial | Count |
|---|---:|
| QfcItemController.cs | 0 |
| QfcItemController.Initialization.cs | 12 |
| QfcItemController.ViewerSetup.cs | 9 |
| QfcItemController.Conversation.cs | 7 |
| QfcItemController.FolderHandling.cs | 4 |
| QfcItemController.EventWiring.cs | 6 |
| QfcItemController.EventHandlers.cs | 18 |
| QfcItemController.Navigation.cs | 20 |
| QfcItemController.FocusAndTheme.cs | 18 |
| QfcItemController.MailActions.cs | 9 |
| **TOTAL** | **103** |

## Reconciliation against denied boundary

The denied boundary `evidence/other/exemption-boundary.2026-06-29T12-40.md` recorded 101 methods + 2
properties = 103 exempted members. The current source carries exactly **103** `[ExcludeFromCodeCoverage]`
attributes. NO DRIFT from 103. The 2 properties are `RightKeyActions` and `RightKeyActionsAsync`
(MailActions.cs); the remaining 101 are methods/constructors.

## Per-member list (grouped by partial)

### Initialization.cs (12)
protected QfcItemController(); 3 public ctors (primary, +predeterminedFolder, +bool async);
private Initialize(...,bool async); public Initialize(bool async); InitializeAsync;
InitializeGraphicsAsync; InitializeSequentialAsync; SaveParameters; CreateAsync; CreateSequentialAsync

### ViewerSetup.cs (9)
InitializeWebViewAsync; ResolveControlGroups(ItemViewer); ResolveControlGroupsAsync(ItemViewer);
PopulateControls(MailItem,int); PopulateControls(MailItemHelper,int); PopulateControlsAsync;
AssignControlsAsync; AssignControls; Cleanup

### Conversation.cs (7)
PopulateConversation(); PopulateConversation(ConversationResolver); DoLoadConversationResolverCoreAsync;
PopulateConversation(int); RenderConversationCount(); RenderConversationCountAsync; SetTopicThread

### FolderHandling.cs (4)
LoadFolderHandler; LoadFolderHandlerAsync; PopulateFolderComboBox; PopulateFolderComboBoxAsync

### EventWiring.cs (6)
WireEvents; WebView2Control_CoreWebView2InitializationCompleted; RegisterFocusActions;
RegisterExpandedActions; UnregisterFocusActions; UnregisterExpandedActions

### EventHandlers.cs (18)
CbxConversation_CheckedChanged; BtnFlagTask_Click; BtnPopOut_Click; BtnDelItem_Click; BtnReply_Click;
BtnReplyAll_Click; BtnForward_Click; TxtboxBody_DoubleClick; Button_MouseEnter; MenuItem_MouseEnter;
Button_MouseLeave; MenuItem_MouseLeave; TextBoxSearch_TextChanged; TextBoxSearch_KeyDown;
TopicThread_ItemSelectionChanged; CbxEmailCopy_CheckedChanged; CboFolders_SelectedIndexChanged;
CbxAttachments_CheckedChanged

### Navigation.cs (20)
JumpToFolderDropDown; JumpToFolderDropDownAsync; JumpToSearchTextbox; JumpToAsync(Control); MenuDropDown;
Reply; ReplyAll; Forward; ToggleCbMenuItemAsync(x2); ToggleCheckboxAsync(x2); ToggleConversationCheckbox(x2);
ToggleExpansion(); ToggleExpansionAsync(); ToggleExpansion(ToggleState); ToggleExpansionAsync(ToggleState);
ToggleExpansionOff; ToggleExpansionOn

### FocusAndTheme.cs (18)
ToggleFocus(ToggleState); ToggleFocusAsync(ToggleState); ToggleFocus(); ToggleFocusAsync();
ToggleFocusOnAsync; ToggleFocusOffAsync; ToggleNavigation(bool); ToggleNavigation(bool,ToggleState);
ToggleNavigationAsync; ToggleTips; ToggleTipsAsync; InvokeBeginInvoke; ToggleSaveAttachments;
ToggleSaveCopyOfMail; SetThemeDark; HtmlDarkConverter; SetThemeLight; ApplyReadEmailFormat

### MailActions.cs (9)
CollapseConversation; EnumerateConversation; EnumerateConversationAsync; RightKeyActions (property);
RightKeyActionsAsync (property); MoveMailAsync; FlagAsTask; FlagAsTaskAsync; MarkItemForDeletionAsync

Output Summary: Exact starting exemption count = 103 (101 methods/ctors + 2 properties). No drift from
the denied boundary. This is the reduction baseline for AC8/AC10; Phases 5-6 will de-exempt the
bucket-(i) and bucket-(ii) members and Phase 7 will confirm the small residual bucket-(iii) set.
