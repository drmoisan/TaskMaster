# Confirmed Root Cause — Issue #255 (QuickFiler conversation fast list empty)

Timestamp: 2026-07-07T13-15

## Symptom

On expanding an item in the QuickFiler "Quick File" dialog, the conversation ("fast list" / TopicThread) panel shows "The fast list is empty" while the conversation count badge shows a non-zero value (8 in the screenshot).

## Confirmed cause (verified by code reading)

The deferred conversation-info UI publish is never triggered in the `loadAll == false` initialization path.

Trace (file:line):

1. Item viewer initializes conversation display at `QuickFiler/Controllers/QfcItemController.Initialization.cs:248` via `await PopulateConversationAsync(_tokenSource, Token, loadAll: false)` — the item-init path always uses `loadAll == false`.
2. `PopulateConversationAsync` (`QuickFiler/Controllers/QfcItemController.Conversation.cs:94-109`) calls `LoadConversationResolverAsync` -> `DoLoadConversationResolverCoreAsync` (`QfcItemController.Conversation.cs:79-92`) -> `ConversationResolver.LoadAsync(..., loadAll: false, SetTopicThread)`, then only calls `RenderConversationCountAsync` (the count badge). It never publishes the conversation list to the fast list.
3. In `ConversationResolver.LoadAsync` (MailItemHelper overload `QuickFiler/Helper Classes/ConversationResolver.cs:126-160`; MailItem overload `:86-124` identical in shape), the `loadAll == false` branch executes only `await resolver.LoadDfAsync(token, loadAll)` then subscribes `resolver.PropertyChanged += resolver.Handler_PropertyChanged` (`:154-156`). It never calls `LoadConversationInfoAsync`.
4. `LoadConversationInfoAsync` (`QuickFiler/Helper Classes/ConversationResolver.Loading.cs:75-154`, publish at `:140-151`) is the sole path that invokes `UpdateUI(pair.Expanded)` — i.e. `SetTopicThread` -> `_itemViewer.SetConversationItems(...)` (`QfcItemController.Conversation.cs:207-219`) -> `TopicThread.SetObjects(...)` (`QuickFiler/Viewers/ItemViewer.WebViewThread.cs:23`).
5. The intended deferred trigger — `Handler_PropertyChanged` reacting to the `Df` PropertyChanged event to run `BackgroundInitInfoItemsAsync` -> `LoadConversationInfoAsync` (`ConversationResolver.Loading.cs:304-315` and `ConversationResolver.cs:210-226`) — cannot fire, because the `Df` assignment happens inside `LoadDfAsync` (`ConversationResolver.Loading.cs:252`) BEFORE the handler is subscribed. This ordering is intentional per the inline comments at `ConversationResolver.cs:118` and `:154`.
6. The `UpdateUI` property-change branch of `Handler_PropertyChanged` (`ConversationResolver.Loading.cs:316-324`) is guarded by `if (FullyLoaded)`; `FullyLoaded` only becomes true after `BackgroundInitInfoItemsAsync` completes (which never runs), so this path is also dead in the `loadAll == false` flow.

## Net effect

In the `loadAll == false` path, `TopicThread` is never populated, so it renders `EmptyListMsg = "The fast list is empty"`. The count badge populates independently from `Count.SameFolder` via `RenderConversationCountAsync` (`QfcItemController.Conversation.cs:104-108, 180-205`), which reads `Df` directly and shows the true count. This is the reported divergence (count 8, list empty). Corresponds to suspected cause (a): async UI publish ordering vs. viewer binding — specifically, the deferred publish is never triggered.

## Not the cause (ruled out)

The `SentOn != ""` filter and `FilterConversation` filters in `LoadDfAsync` (`ConversationResolver.Loading.cs:246-250`) reduce `Df.Expanded`/`Df.SameFolder` together; because `Count.SameFolder` (the badge) is a subset of `Df.Expanded`, a non-zero badge implies `Df.Expanded` is non-empty, so the empty list is not caused by the dataframe filters.

## Genuinely-empty behavior to preserve

`LoadConversationInfo` (`ConversationResolver.Loading.cs:37-73`) returns a single-item fallback (the current mail item) when `Count.Expanded <= 0` — this covers the Junk E-mail case where `FilterConversation` removes all rows. The fix must not alter this behavior.
