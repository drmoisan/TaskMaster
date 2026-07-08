# IItemViewer Final Shape — Interface Audit (P6-T5)

Timestamp: 2026-06-29T12-05

Final IItemViewer member classification (QuickFiler/Viewers/IItemViewer.cs):

## Intent members (added in Phases 3-6)
- Display-state (Seam B/2a): SenderText, SubjectText, BodyText, TriageText, SentOnText, ActionableText, ItemNumberText, FolderText, ConversationCountText, ConversationCountBackColor, event BodyDoubleClick, FocusSubject() — intent
- Button/menu commands (Seam B/2b): events DeleteItemClicked, FlagTaskClicked, PopOutClicked, ReplyClicked, ReplyAllClicked, ForwardClicked, ConversationModeChanged, EmailCopyChanged, AttachmentsChanged, PicturesChanged; bool ConversationModeChecked/EmailCopyChecked/AttachmentsChecked/PicturesChecked; FlagTaskDialogResult, FlagTaskBackColor — intent
- Folder/search (Seam C/2c): SetFolderItems, GetSelectedFolder, SetFolderSelectedIndex, SetFolderSelectedItem, SetFolderDroppedDown, ClearFolderItems, FocusFolderDropDown, FolderContains, GetFolderItems, event FolderSelectionChanged, event FolderKeyDown, SearchText, event SearchTextChanged, event SearchKeyDown, FocusSearch() — intent  (GetFolderItems added beyond research §3.3 to satisfy P5-T4 EnumerateConversation folder-list read)
- WebView/topic-thread (Seam D/2d): NavigateToString, event WebViewInitializationCompleted, SetConversationItems, SortConversationByDate, GetSelectedConversationItems, event ConversationItemSelectionChanged, ShowMoveOptionsMenu — intent

## Retained members
- UiDispatcher, UiScheduler, UiSyncContext — retained-threading
- InvokeRequired, Invoke(Delegate), BeginInvoke(Delegate), Height — retained-dispatch (added P2-T1)
- RemoveControlsColsRightOf(Control) — retained-structural
- LblSearch — retained-label (bounded blast radius this cycle)
- Controller (IItemControler) — retained-backref (bounded blast radius this cycle)
- MenuItems (IList<Component>) — retained-component (WireEvents hover loop, line 1276; component list, not raw clickable control)

## Deferred members
- TipsLabels, LeftTipsLabels, ExpandedTipsLabels (IList<Label>) — deferred-label (tip-collection abstraction deferred this cycle)
- LblAcBody, LblAcDelete, LblAcFolder, LblAcFwd, LblAcMoveOptions, LblAcOpen, LblAcPopOut, LblAcReply, LblAcReplyAll, LblAcSearch, LblAcTask, LblCaptionPredicted, LblCaptionTriage — deferred-label (accelerator labels)

## Absent control types (acceptance check — all must be absent; confirmed)
- ButtonSVG: ABSENT
- ComboBox: ABSENT
- WebView2: ABSENT
- TextBox: ABSENT
- FastObjectListView: ABSENT
- OLVColumn (Sender/SentDate/Infolder): ABSENT
- MenuStrip (MoveOptionsStrip): ABSENT
- ToolStripMenuItem / ToolStripMenuItemCb (MoveOptionsMenu/ConversationMenuItem/Save*MenuItem): ABSENT
- TableLayoutPanel (L0vh_Tlp/L1h0L2hv3h_TlpBodyToggle): ABSENT
- Panel (L1h1L2v1h3Panel): ABSENT

Output Summary: All ten prohibited raw control types are absent from IItemViewer. LblSearch, Controller, and MenuItems are present and classified retained-label / retained-backref / retained-component respectively. Audit PASS.
