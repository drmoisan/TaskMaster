# [P0-T13] Coverage measurability of the Write Set production files

Timestamp: 2026-09-07T07-14

Command: separator-anchored `class` `filename` query over coverage\799-baseline.cobertura.xml
(the [P0-T13] command block, matching `$f.EndsWith('\' + $n) -or $f.EndsWith('/' + $n)`)

EXIT_CODE: 0

## Class-element counts the determination was made from

```
OutlookFolderHierarchyProvider.cs classElements=6
FolderPredictor.cs classElements=13
QfcItemController.FolderHandling.cs classElements=6
BreadcrumbBridgeRouter.cs classElements=6
EfcFormController.cs classElements=43
QfcItemController.ViewerSetup.cs classElements=9
ArchiveStemContract.cs classElements=1
```

## Determination

MEASURABLE: UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs
MEASURABLE: UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs
MEASURABLE: QuickFiler/Controllers/QfcItemController.FolderHandling.cs
MEASURABLE: QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
MEASURABLE: QuickFiler/Controllers/EfcFormController.cs
MEASURABLE: QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
MEASURABLE: UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs

Seven `MEASURABLE:`/`UNMEASURABLE:` lines are recorded: one for each of the six existing Write Set production
paths, plus ArchiveStemContract.cs. ArchiveStemContract.cs is not modified by this plan; its measurability is
recorded because [P2-T1], [P2-T2] and [P2-T9] all route through it, so a zero-class-element result there would
explain an otherwise puzzling [P3-T7] outcome. It reports one class element, so that explanation does not apply.

## Files this plan creates (measured for the first time by [P3-T9])

NEW: UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs
NEW: UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs

## Match anchoring

The trailing-name match is anchored on a directory separator, so an unanchored suffix cannot over-select a sibling
whose name merely ends with the same characters. The concrete case this protects against in this Write Set is
UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs, a sibling-owned file whose name ends with the
characters of BreadcrumbBridgeRouter.cs but is preceded by `r` rather than by a separator, so it is correctly
excluded from the count of 6 recorded for QuickFiler/Controllers/BreadcrumbBridgeRouter.cs.

Output Summary: All seven queried production files are measurable in the baseline Cobertura document, with class
element counts ranging from 1 to 43 and no zero result. No Write Set production file is invisible to the coverage
harness, so the [P3-T7] changed-line coverage determination has a source of data for every file it must inspect.
