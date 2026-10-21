# P9-T5 - Coverage delta and thresholds (#614; AC23)

Timestamp: 2026-08-26T19-50

Inputs:
- Baseline filtered Cobertura: `coverage\coverage.cobertura.filtered.p0-t9.xml` (P0-T9)
- Post-change filtered Cobertura: `coverage\coverage.cobertura.filtered.p9-t4.xml` (P9-T4 clean pass)

Both files live in the gitignored `coverage/` tree and are never copied under `evidence/`.

Counting method (reproduced from the P0-T9 baseline method so the two figures are commensurable):
Koverage post-processing pre-merges per-file `<class>` entries and rewrites filenames with
backslashes. Per-file and per-method figures below deduplicate `<line>` elements by
(filename, line-number) across every `<class>` and `<method>`, so a line that appears both under a
`<method>` and in the class-level `<lines>` block is counted once. Compiler-generated async and
lambda classes are aggregated into their owning `filename`, as AC23 requires.

## (a) Baseline filtered figure

**84.7797%** (53769 / 63422) line coverage; 78.6938% (12676 / 16108) branch coverage.

The plan's stated reference was 84.8099% (53627 / 63232). The measured merge-base figure on this
machine is 0.0302 points lower; the P0-T9 artifact records that divergence and attributes it to the
known dotnet-coverage run-to-run denominator nondeterminism. No source file changed between the two
measurements.

## (b) Post-change filtered figure - GATE MET

**84.8696%** (53972 / 63594) line coverage; 78.8331% (12741 / 16162) branch coverage.

| Comparison | Result |
| --- | --- |
| Post-change vs the plan's fixed floor `>= 84.80` | **84.8696 >= 84.80 - PASS** |
| Post-change vs the measured merge-base baseline 84.7797% | **+0.0899 points - no regression, PASS** |
| Branch: post-change vs baseline 78.6938% | **+0.1393 points - no regression** |

The gate is met on the first measurement, so the plan's re-measurement remediation path (for a
marginal miss under dotnet-coverage nondeterminism) was not needed and was not used.

## (c) New-code coverage - GATE `>= 90%` MET on every item

| Item | Line coverage | Gate |
| --- | --- | --- |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` (file) | **100.0000%** (51 / 51) | PASS |
| &nbsp;&nbsp;`IsFullOutlookPath` | 100.0000% (10 / 10) | PASS |
| &nbsp;&nbsp;`RequireArchiveRelativeStem` | 100.0000% (16 / 16) | PASS |
| &nbsp;&nbsp;`TryMakeArchiveRelative` | 100.0000% (25 / 25) | PASS |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` (file) | **100.0000%** (9 / 9) | PASS |
| &nbsp;&nbsp;`IsValidFilingSelection` | 100.0000% (9 / 9) | PASS |
| `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` (file, optional new guard) | **100.0000%** (20 / 20) | PASS |
| &nbsp;&nbsp;`RequireResolvedArchiveRoot` | 100.0000% (20 / 20) | PASS |
| `EfcDataModel.ToArchiveRelativeStem` | **100.0000%** (12 / 12) | PASS |
| `EmailFilerConfig.ResolvePaths` (both overloads aggregated) | **100.0000%** (25 / 25) | PASS |
| `EmailFilerConfig.GetStem` | **100.0000%** (9 / 9) | PASS |
| `EmailFilerConfig.IsDeleteRelevant` | **100.0000%** (10 / 10) | PASS |
| `FolderConverter.ToFsFolderpath` (all overloads aggregated) | **100.0000%** (60 / 60) | PASS |
| `FolderConverter.ResolveOlRoot` | **100.0000%** (23 / 23) | PASS |

Additional new production helpers introduced by this change, all likewise at 100%:
`FolderConverter.FindInvalidSegmentRule` (22 / 22), `FolderConverter.IsReservedDeviceName` (5 / 5),
`FolderConverter.RemoveIllegalCharacters` (4 / 4),
`AppFileSystemFolderPaths.ResolveOneDriveRoot` (14 / 14),
`BreadcrumbBridgeRouter.SelectRow` (18 / 18), `.SelectHierarchyPath` (14 / 14),
`.CommitSelection` (8 / 8), `.ToHierarchyPath` (10 / 10).

### Remediation performed during this task

The first P9-T5 measurement found `ArchiveStemContract.TryMakeArchiveRelative` at 92.0000%
(23 / 25), with lines 120-121 - the `root.Length == 0` guard after `TrimEnd` - uncovered. That is a
PURE branch introduced by this change and it is reachable (a root consisting only of separators is
not whitespace, so it passes the emptiness guard and then trims to length zero). Per AC23's "every
pure branch introduced must be covered", the test
`ArchiveStemContractTests.TryMakeArchiveRelative_SeparatorOnlyRoot_ReturnsFalse` was added and the
Phase 9 loop was restarted from step 1. The method is now at 100%.

## (d) Changed-line coverage for COM-adjacent wiring

Changed lines are the `+` lines of `git diff -U0 <merge-base>` for each production file, restricted
to lines the coverage instrument measures.

| File | Added lines | Measurable | Covered | Uncovered |
| --- | ---: | ---: | ---: | --- |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 54 | 42 | **42** | none |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | 38 | 9 | **9** | none |
| `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` | 62 | 20 | **20** | none |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` | 37 | 22 | **22** | none |
| `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | 133 | 98 | **98** | none |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` | 147 | 51 | **51** | none |
| `QuickFiler/Controllers/EfcFormController.cs` | 2 | 2 | 0 | 706, 1038 |
| `QuickFiler/Controllers/EfcDataModel.cs` | 31 | 13 | 12 | 345 |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 20 | 5 | 0 | 259-263 |
| `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` | 69 | 33 | 23 | 30-35, 37-39, 265 |

**Every pure branch introduced by this change is covered.** All 18 uncovered changed lines are
host-bound wiring, listed individually with justification:

| Line | Statement | Why it is host-bound |
| --- | --- | --- |
| `EfcFormController.cs:706` | `if (!EfcSelectionGuard.IsValidFilingSelection(selectedFolder))` | Inside `ActionOkAsync`, which reads `SynchronizationContext.Current`, calls `_formViewer.Hide()` and `MessageBox.Show`, and drives the WinForms filing pipeline. UI event glue. The predicate it delegates to is 100% covered by `EfcSelectionGuardTests`. |
| `EfcFormController.cs:1038` | `internal bool IsValidSelection => EfcSelectionGuard.IsValidFilingSelection(SelectedFolder);` | Reads `SelectedFolder` off the WinForms viewer. UI event glue; same fully covered predicate. |
| `EfcDataModel.cs:345` | `var folderpath = ToArchiveRelativeStem(folder.FolderPath, olAncestor);` | Inside `MoveToFolderAsync(MAPIFolder folder, ...)`; `folder.FolderPath` is a COM property access on a live `MAPIFolder`. The extracted pure helper is 100% covered by `EfcDataModelIssue614Tests`. |
| `AppOlObjects.cs:259-263` | the `ArchiveRootPathGuard.RequireResolvedArchiveRoot(Path.Combine(Root.FolderPath, "Archive"), ArchiveRoot?.FolderPath, ...)` call | COM property access: `Root.FolderPath` and `ArchiveRoot.FolderPath` both require a live Outlook store. The pure guard it calls is 100% covered by `AppOlObjectsArchiveRootValidationTests`. |
| `AppFileSystemFolderPaths.cs:30-35, 37-39` | the internal test-seam constructor body | Constructing the type runs `LoadFolders`, which reads machine-specific `Environment.GetFolderPath` special folders. Exercising it would make the test machine-dependent and therefore non-deterministic, which repository test policy prohibits. The seam's purpose - deterministic OneDrive resolution - is instead covered through the pure `ResolveOneDriveRoot` at 100%. |
| `AppFileSystemFolderPaths.cs:265` | `() => [_readEnvironmentVariable("OneDrivePersonal")]` | The `OneDrivePersonal` fallback lambda inside `LoadFolders`, reached only when `OneDriveConsumer` is unset in the host environment. Environment-dependent host wiring; the priority-order logic is covered deterministically by `ResolveOneDriveRoot`. |

## (e) No changed line lost coverage

Per-file covered counts, baseline versus post-change:

| File | Baseline covered / valid | Post covered / valid | Covered delta | Uncovered delta |
| --- | --- | --- | ---: | ---: |
| `BreadcrumbBridgeRouter.cs` | 368 / 376 | 365 / 373 | -3 | **0** |
| `EfcFormController.cs` | 81 / 721 | 81 / 713 | 0 | **-8** |
| `EfcDataModel.cs` | 124 / 250 | 136 / 258 | **+12** | -4 |
| `AppOlObjects.cs` | 71 / 213 | 71 / 217 | 0 | +4 |
| `AppFileSystemFolderPaths.cs` | 120 / 204 | 136 / 197 | **+16** | -23 |
| `EmailFilerConfig.cs` | 105 / 112 | 117 / 124 | **+12** | **0** |
| `FolderConverter.cs` | 126 / 128 | 212 / 214 | **+86** | **0** |

**Explicit statement: no changed line lost coverage relative to the merge-base baseline.** In every
file the covered count is unchanged or higher. Where the covered count fell
(`BreadcrumbBridgeRouter.cs`, -3) the uncovered count is unchanged at 8 and the valid count fell by
the same 3, i.e. three covered executable lines were DELETED (the removed `ToArchiveRelativePath`
helper and the restructured selection methods), not left uncovered; all 42 measurable added lines
in that file are covered. The only file whose uncovered count rose is `AppOlObjects.cs` (+4), and
those four lines are the COM-bound `ArchiveRootPath` guard call justified in section (d); the lines
they replaced were themselves uncovered at baseline, so nothing that was covered became uncovered.

The eight lines uncovered in `BreadcrumbBridgeRouter.cs` (371-375, 416-417, 580) are pre-existing
and untouched by this change: the `key == null` branch of `ExpandLeafAsync`, the
`activeSegment == null` guard of `ActivateSegment`, and the `return -1;` of `IndexOf`. The two in
`FolderConverter.cs` (168-169) and the seven in `EmailFilerConfig.cs` (227-233) are likewise
pre-existing and outside this change's hunks.

## Outcome

All five sections are populated with numeric values; no required value is unavailable. Every gate
is met. Outcome: **PASS**.
