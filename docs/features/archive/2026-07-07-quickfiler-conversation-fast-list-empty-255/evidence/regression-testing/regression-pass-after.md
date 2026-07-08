# Regression Test — Pass After Fix (Issue #255)

Timestamp: 2026-07-07T13-22

Test: QfcItemController_ConversationTests.PopulateConversationAsync_DeferredLoad_PublishesConversationToFastList
File: QuickFiler.Test/Controllers/QfcItemController.ConversationTests.cs

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:PopulateConversationAsync_DeferredLoad_PublishesConversationToFastList /InIsolation

EXIT_CODE: 0

Output Summary:
- Total tests: 1, Passed: 1.
- After the fix in QfcItemController.Conversation.cs (PopulateConversationAsync publishes the resolved conversation to the fast list in the deferred loadAll == false path), `IItemViewer.SetConversationItems` is invoked once with the 3-item conversation list.
- Confirms the fast list is populated on expand rather than showing "The fast list is empty".

Fix applied: QuickFiler/Controllers/QfcItemController.Conversation.cs — in PopulateConversationAsync(CancellationTokenSource, token, loadAll), the deferred (loadAll == false) branch now calls SetTopicThread(ConversationResolver.ConversationInfo.Expanded). The genuinely-empty case is preserved because ConversationResolver.LoadConversationInfo returns a single-item fallback when Count.Expanded <= 0.
