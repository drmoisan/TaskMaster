# Phase 0 — Pre-change Line Counts of Every Write Set C# File (Issue #797)

Timestamp: 2026-09-07T09-20

Command: `Get-Content -LiteralPath <path> | measure line count`, over the nine paths below, run from
the repository root of this worktree.

EXIT_CODE: 0

| Path | Lines |
|---|---|
| TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs | 75 |
| TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs | 186 |
| UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs | 613 |
| UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs | 233 |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs | 478 |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs | 396 |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs | 216 |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs | 285 |
| TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs | 347 |

All nine paths are listed with an integer line count each.

PRE-EXISTING-OVER-CAP:

- UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs — 613 lines, against the
  500-line cap in the general code change policy. This is the expected count and is the single entry.
  Design decision D5 declares this file out of scope for splitting: splitting a shared
  reusable-type-classes file during a parallel run would create merge contention with concurrently
  running sibling work items. This change adds the minimum number of lines to that file and does not
  resolve the pre-existing violation.

The two files this change creates do not appear in this census because they do not yet exist. The
three project files are not enumerated, because the 500-line cap applies to production code, test code
and reusable script files and not to project files.

Output Summary: Nine Write Set C# paths measured. One pre-existing over-cap file, the serializer at
613 lines, recorded under D5 as out of scope. The controller at 478 lines has 22 lines of headroom,
which is why D4 splits it into a display partial.
