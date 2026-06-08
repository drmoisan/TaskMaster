# Phase 1 Task 3 — Focused regression rerun evidence

Timestamp: 2026-05-07T22:06:16-04:00

Task: P1-T3
Plan Path: `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/remediation-plan.2026-05-07T21-30.md`

## Commands executed

1. Build refresh
   - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU'`
   - EXIT_CODE: 0

2. Focused regression rerun
   - Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:FullyQualifiedName~LoadAsync_EmitsStartupInboxTimingEnvelopeBeforeDeferredProcessingWindow|FullyQualifiedName~ProcessNewInboxItemsAsync_BatchesMailboxProcessingAfterInteractiveCheckpoint|FullyQualifiedName~CaptureSelectionSnapshot_ReturnsIndependentCopyBeforeBackgroundModelLoad|FullyQualifiedName~CreateAsync_StagesSnapshotLoadBeforeBackgroundInitialization|FullyQualifiedName~LoadDfAsync_ConsumesConversationSnapshotsWithoutRepeatedUiPublishes|FullyQualifiedName~GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform|FullyQualifiedName~GetConversationDfAsync_CapturesConversationTableSnapshotBeforeBackgroundTransform|FullyQualifiedName~TryProjectMailItemMembers_UsesMaterializedProjectionValues|FullyQualifiedName~GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot`
   - EXIT_CODE: 0

## Exact test names

- `LoadAsync_EmitsStartupInboxTimingEnvelopeBeforeDeferredProcessingWindow`
- `ProcessNewInboxItemsAsync_BatchesMailboxProcessingAfterInteractiveCheckpoint`
- `CaptureSelectionSnapshot_ReturnsIndependentCopyBeforeBackgroundModelLoad`
- `CreateAsync_StagesSnapshotLoadBeforeBackgroundInitialization`
- `LoadDfAsync_ConsumesConversationSnapshotsWithoutRepeatedUiPublishes`
- `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`
- `GetConversationDfAsync_CapturesConversationTableSnapshotBeforeBackgroundTransform`
- `TryProjectMailItemMembers_UsesMaterializedProjectionValues`
- `GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot`

## Red-to-green summary

- Earlier focused reruns exposed stale assembly discovery and then deterministic fixture failures while replacing the brittle source-text checks.
- The failing states were narrowed to missing compile imports, legacy project compile-item omissions, strict mock return-value gaps for `Columns.Add`, and one resolver test path that incorrectly triggered a background property-change workflow.
- Each failing condition was corrected in the rewritten regression homes or their project includes, followed by a clean rebuild and the final focused rerun above.

## Final output summary

- `VSTest version 18.5.0 (x64)`
- `Total tests: 9`
- `Passed: 9`
- `Failed: 0`
- `Total time: 3.2649 Seconds`

## Conclusion

The rewritten issue `#148` regression homes now execute as runtime-observable focused tests without relying primarily on raw source-file string matching, and the focused rerun is deterministic on the rebuilt Debug assemblies.
