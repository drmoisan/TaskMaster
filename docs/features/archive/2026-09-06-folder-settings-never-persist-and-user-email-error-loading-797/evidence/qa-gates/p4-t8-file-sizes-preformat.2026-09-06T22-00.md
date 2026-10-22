# P4-T8 — Pre-format File-size Census (Issue #797)

Timestamp: 2026-09-07T09-55

Command: `Get-Content -LiteralPath <path> | measure line count`, over the thirteen paths below, run
from the repository root of this worktree.

EXIT_CODE: 0

This is a pre-format census taken before the Phase 5 formatter runs. The binding audit is P5-T8.

| Path | Phase 0 lines | Current lines |
|---|---|---|
| TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs | 75 | 90 |
| TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs | 186 | 198 |
| UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs | 613 | 658 |
| UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs | 233 | 302 |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs | 478 | 388 |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs | 396 | 402 |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs | 216 | 361 |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs | 285 | 416 |
| TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs | 347 | 429 |
| UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs | not yet created | 29 |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs | not yet created | 173 |
| UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs | not yet created | 349 |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs | not yet created | 252 |

Every listed C# path is at or below 500 lines except the serializer.

## The one exception

UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs is 658 lines against the
500-line cap. Its Phase 0 count was 613, already over the cap before any change here. Design decision
D5 declares that pre-existing violation out of scope for this work item: splitting a shared
reusable-type-classes file during a parallel run would create merge contention with concurrently
running sibling work items. This change adds 45 lines to that file — the shared path guard with its
documentation, the explicit-save entry point body, and the comments explaining both — and does not
split it. This change does not resolve the pre-existing violation and does not introduce a new
violation class.

UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs fell from 478 to 388 lines because D4's
display partial took the three rendering members. The partial itself is 173 lines, so both files sit
comfortably under the cap, which is the outcome D4 was designed to produce.

Output Summary: Twelve of the thirteen C# paths are at or below 500 lines. The serializer is at 658,
a pre-existing over-cap condition D5 declares out of scope. No project file is enumerated, because
the cap does not reach project files.
