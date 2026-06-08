# Phase 3 Instrumented Hotspot Summary

Timestamp: 2026-05-07T21:01:18.6861570-04:00
Source Tests:
- LoadAsync_EmitsStartupInboxTimingEnvelopeBeforeDeferredProcessingWindow
- ProcessNewInboxItemsAsync_BatchesMailboxProcessingAfterInteractiveCheckpoint
- HandleSelectionChangedAsync_CapturesSelectionSnapshotBeforeBackgroundModelLoad
- CreateAsync_StagesSnapshotLoadBeforeBackgroundInitialization
- LoadDfAsync_ConsumesConversationSnapshotsWithoutRepeatedUiPublishes
- GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform
- GetConversationDfAsync_CapturesConversationTableSnapshotBeforeBackgroundTransform
- FromMailItemAsync_MaterializesComDataBeforeAsyncProjection
- GetTableInViewAsync_ReturnsSnapshotWithoutTaskRunWrappedComAccess
Dominant Startup Segment: AppEvents
Dominant FirstSelection Segment: OlTableExtensions
Repeated Ui Publish Count: 0
Contingent Promotion Needed: false
Promoted Contingent Files: none
Focused Suite Command: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe across TaskMaster.Test.dll (2 tests), QuickFiler.Test.dll (3 tests), and UtilitiesCS.Test.dll (4 tests)
Output Summary:
- TaskMaster.Test focused suite: 2 passed, 0 failed, 0 skipped.
- QuickFiler.Test focused suite: 3 passed, 0 failed, 0 skipped.
- UtilitiesCS.Test focused suite: 4 passed, 0 failed, 0 skipped.
- Total focused suite: 9 passed, 0 failed, 0 skipped.
Notes:
- `Dominant Startup Segment` and `Promoted Contingent Files` match `instrumentation-verdict.2026-05-07T20-10-44-04-00.md`.
- `Repeated Ui Publish Count: 0` reflects the current hotspot-summary assessment after the focused source-based regression suite; the source now tracks UI publication cadence explicitly in `ConversationResolver`.
- No contingent promotion was required because the latest Phase 1 verdict recorded `Promoted Contingent File Count: 0`.
