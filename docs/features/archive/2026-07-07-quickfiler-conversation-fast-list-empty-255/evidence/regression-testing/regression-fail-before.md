# Regression Test — Fail Before Fix (Issue #255)

Timestamp: 2026-07-07T13-20

Test: QfcItemController_ConversationTests.PopulateConversationAsync_DeferredLoad_PublishesConversationToFastList
File: QuickFiler.Test/Controllers/QfcItemController.ConversationTests.cs

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:PopulateConversationAsync_DeferredLoad_PublishesConversationToFastList /InIsolation

EXIT_CODE: 1

Output Summary:
- Total tests: 1, Failed: 1.
- Failing assertion:
  `Expected invocation on the mock once, but was 0 times: v => v.SetConversationItems(It.Is<IList>(l => l != null && l.Count == 3))`
- Interpretation: in the deferred (loadAll == false) path, PopulateConversationAsync loads the resolver and renders the count badge but never publishes the resolved multi-item conversation to the fast list, so `IItemViewer.SetConversationItems` is never called. This is exactly the reported divergence (non-zero count, empty fast list) and confirms the root cause documented in root-cause.md.
- The test does not touch a live Outlook process, BackgroundWorker, a real form, the static UiThread.Dispatcher, or temp files (it uses the SeamController + BuildSyncDispatcher harness and a pre-populated ConversationInfo so the COM-bound loaders are not invoked).
