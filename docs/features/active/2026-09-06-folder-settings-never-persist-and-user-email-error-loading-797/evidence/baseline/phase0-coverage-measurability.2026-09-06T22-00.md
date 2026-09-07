# Phase 0 — Coverage Measurability of the Seven Write Set Production Files (Issue #797)

Timestamp: 2026-09-07T09-21

Command: `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Measurability -CoberturaPath coverage/plan797-baseline/coverage.cobertura.xml`

EXIT_CODE: 0

The search looks for a `class` element in the baseline document whose `filename` attribute ends with a
path separator followed by the file name. The match is anchored on a path separator, taken by reading
the leaf of the filename attribute, so a file name cannot also select a differently named sibling that
ends with the same characters — for example `StoreWrapperController.cs` cannot be selected by a
lookup for `Controller.cs`, and `StoreWrapperController.Display.cs` is a distinct leaf from
`StoreWrapperController.cs`.

| Production path | Verdict | Baseline covered lines | Baseline valid lines |
|---|---|---|---|
| TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs | MEASURABLE | 38 | 38 |
| TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs | MEASURABLE | 21 | 68 |
| UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs | MEASURABLE | 317 | 345 |
| UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs | MEASURABLE | 122 | 128 |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs | MEASURABLE | 226 | 235 |
| UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs | NOT MEASURABLE — NOT YET CREATED | n/a | n/a |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs | NOT MEASURABLE — NOT YET CREATED | n/a | n/a |

The last two files do not exist at this point in the plan and are recorded as NOT YET CREATED. They
are re-evaluated against the Phase 5 document in P5-T7. The interface file is expected to remain
without a class element even after creation, because an interface declaration emits no IL; rule R10
directs that such a file be reported as NOT APPLICABLE rather than as a zero.

The five files that exist today are all measurable, so each produces a real per-file changed-line row
in the P5-T7 table. In particular TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs carries no
class-level coverage-exclusion attribute on its partial and is measurable at 21 of 68 lines.

Output Summary: Five of the seven Write Set production files are measurable in the baseline document
with the counts above. The two files this change creates do not yet exist and are recorded as NOT YET
CREATED.
