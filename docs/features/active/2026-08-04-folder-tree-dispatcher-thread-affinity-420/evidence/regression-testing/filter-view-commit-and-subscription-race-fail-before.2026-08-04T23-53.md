# P5-T17 candidate-view and subscription race red evidence

Timestamp: 2026-08-04T23:53:00-04:00
Command: `Get-Process vstest`; `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerRefreshDisposalTests.DisposeDuringCandidateViewCommit_DoesNotRetainViewOrSubscription,UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerRefreshDisposalTests.DisposeDuringSnapshotSubscription_DoesNotRetainViewOrSubscription`
EXIT_CODE: 1
Output Summary: No active `vstest` process was present. Both deterministic barrier tests failed. Candidate-view commit retained a non-null `FolderTreeView` after `SetController` reentrantly closed the viewer. Subscription commit retained a non-null `FolderTreeView` after the `SnapshotChanged` add callback reentrantly closed the viewer before the controller committed the subscription state.

The candidate-view barrier invokes `Close` during `SetController`; the subscription barrier invokes `Close` inside the actual `SnapshotChanged` event add accessor. Both tests also require zero refresh notification and zero retained service handlers. Current failures prove the lifecycle path does not linearize either barrier against disposal.
