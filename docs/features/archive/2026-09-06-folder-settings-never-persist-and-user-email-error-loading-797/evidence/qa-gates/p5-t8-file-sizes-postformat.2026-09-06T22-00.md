# P5-T8 — Binding Post-format File-size Audit (Issue #797)

Timestamp: 2026-09-07T10-11

Command: `Get-Content -LiteralPath <path> | measure line count`, over the thirteen C# paths enumerated
in P4-T8, taken after the Phase 5 formatter ran.

EXIT_CODE: 0

| Path | Lines | At or below 500 |
|---|---|---|
| TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs | 90 | yes |
| TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs | 198 | yes |
| UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs | 658 | no — see below |
| UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs | 302 | yes |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs | 388 | yes |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs | 402 | yes |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs | 361 | yes |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs | 416 | yes |
| TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs | 429 | yes |
| UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs | 29 | yes |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs | 173 | yes |
| UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs | 352 | yes |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs | 252 | yes |

Twelve of the thirteen paths are at or below the 500-line cap.

## The single exception

UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs is 658 lines after the
formatter, against its Phase 0 count of 613. It was already 45 lines over the cap before any change
here. Design decision D5 declares this pre-existing violation out of scope for this work item, because
splitting a shared reusable-type-classes file during a parallel run would create merge contention with
concurrently running sibling work items. This change adds the minimum — a shared path guard with its
documentation and the explicit-save entry point body — and does not split the file. It does not
resolve the pre-existing violation and does not introduce a new violation class.

## The two controller files

UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs is 388 lines and
UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs is 173 lines. Each is at or below
500, which is the outcome design decision D4 was written to produce: the controller stood at 478 lines
with 22 lines of headroom while four acceptance criteria landed in it, and the display partial absorbs
the rendering members.

No project file is enumerated in this audit, because the 500-line cap applies to production code, test
code and reusable script files and does not reach project files.

Output Summary: Twelve of thirteen C# paths are within the cap. The serializer at 658 lines is the
single exception, a pre-existing condition D5 declares out of scope, reported with both its Phase 0
count of 613 and its post-change count.
