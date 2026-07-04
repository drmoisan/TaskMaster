Timestamp: 2026-07-03T17:26:34-04:00
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys,RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException,SwapItemGroups_ThenGuardedZeroItemFlow_LeavesExactlyOneEntryPerIncomingKey
EXIT_CODE: 1
Output Summary:
- Targeted navigation regression tests ran after rebuilding `TaskMaster.sln`.
- Total tests: 3.
- Passed: 1.
- Failed: 2.
- Expected fail-before failures:
  - `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys` failed because stale key `2` remained registered.
  - `SwapItemGroups_ThenGuardedZeroItemFlow_LeavesExactlyOneEntryPerIncomingKey` failed because incoming key registration was absent on the current direct `ActivateQueuedItemGroups` path.
- Control test passed: `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException`.
