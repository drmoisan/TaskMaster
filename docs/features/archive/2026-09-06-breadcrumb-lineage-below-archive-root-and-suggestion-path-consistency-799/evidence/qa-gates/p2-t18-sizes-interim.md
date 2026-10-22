# [P2-T18] Interim (pre-format) line counts of every file this plan has edited or created

Timestamp: 2026-09-07T07-40

Command: `(Get-Content -LiteralPath <path>).Count` for each path below

EXIT_CODE: 0

ExpectedExitCode: 0

CEILING: 500 (applies to *.cs only)

These counts are taken BEFORE the [P3-T1] CSharpier pass. The formatter can change line counts, so
[P3-T10] re-measures the same set afterwards and is the gating measurement.

## Production `.cs`

| Path | [P0-T14] baseline | Now | D11 budget | Verdict |
|---|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` | NOT PRESENT | 63 | 500 | met |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` | NOT PRESENT | 92 | 500 | met |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 141 | 300 | 500 | met |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 1003 | 1002 | 1003 | met |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 312 | 295 | 500, and [P2-T10] additionally requires at or below its 312 baseline | met |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 304 | 407 | 500 | met |
| `QuickFiler/Controllers/EfcFormController.cs` | 1320 | 1321 | 1322 | met |
| `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` | NOT PRESENT | 41 | 500 | met |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 500 | 467 | 500 (hard) | met |

## Test `.cs`

| Path | [P0-T14] baseline | Now | Budget | Verdict |
|---|---|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` | NOT PRESENT | 176 | 500 | met |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs` | NOT PRESENT | 217 | 500 | met |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` | NOT PRESENT | 344 | 500 | met |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` | NOT PRESENT | 213 | 500 | met |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` | NOT PRESENT | 425 | 500 | met |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` | 479 | 480 | 483 ([P1-T13] +4) | met |
| `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` | 354 | 363 | 500 | met |

## PROJECT-FILE (exempt)

Recorded as exempt observations rather than asserted against the ceiling, per R8: the 500-line cap
in .claude/rules/general-code-change.md covers production code, test code and reusable script files
and does not reach project files, and .csharpierignore lines 9-14 record that project files are
owned by Visual Studio and are not C# source.

- PROJECT-FILE (exempt): `UtilitiesCS/UtilitiesCS.csproj` = 1317 (baseline 1315, +2 Compile Include)
- PROJECT-FILE (exempt): `QuickFiler/QuickFiler.csproj` = 606 (baseline 605, +1 Compile Include)
- PROJECT-FILE (exempt): `UtilitiesCS.Test/UtilitiesCS.Test.csproj` = 980 (baseline 976, +4 Compile Include)
- PROJECT-FILE (exempt): `QuickFiler.Test/QuickFiler.Test.csproj` = 530 (baseline 529, +1 Compile Include)

## Files within ten lines of their budget (named explicitly, with remaining headroom)

- `QuickFiler/Controllers/EfcFormController.cs` — 1321 against a budget of 1322. Remaining headroom:
  1 line. The single added lazy root-accessor argument cost exactly one line, which is inside the
  at-most-two-line allowance D11 derives for it.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — 1002 against a no-growth budget of 1003.
  Remaining headroom: 1 line. The net figure is the sum of the D11-derived collapses (the
  `ProjectSuggestionPath` body and the include-children branch of `GetOlSubpath`) against the two
  recents projections and one added using directive.

No other `.cs` file in this set is within ten lines of its budget. The next closest are
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` at 480 against 483
(3 lines of headroom, so it IS within ten and is named here for completeness) and
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` at 467 against 500 (33 lines).

Corrected enumeration of the within-ten set, so the list above is not read as exhaustive:
`EfcFormController.cs` (1), `FolderPredictor.cs` (1) and
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` (3).

## Output Summary

Every listed `.cs` count satisfies its D11 budget. The four project-file counts are recorded under
the exempt heading and are not asserted against the ceiling. Three `.cs` files are within ten lines
of their budget and are named above with their remaining headroom. The R9 ordering constraint held:
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` never passed through 501 lines, because
[P2-T3] removed 33 lines from it before [P2-T14] added the constructor argument to the relocated
member in the new partial.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
