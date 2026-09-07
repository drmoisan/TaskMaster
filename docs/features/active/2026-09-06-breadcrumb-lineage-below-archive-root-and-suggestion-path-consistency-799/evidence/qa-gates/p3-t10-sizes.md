# [P3-T10] Post-format line counts of every file this plan edited or created

Timestamp: 2026-09-07T08-12

Command: `(Get-Content -LiteralPath <path>).Count` for each path below

EXIT_CODE: 0

ExpectedExitCode: 0

CEILING: 500 (applies to *.cs only)

This audit runs AFTER the [P3-T1] CSharpier pass, because the formatter can change line counts (R9). It supersedes
the [P2-T18] interim measurement, which was taken before the formatter ran, and it is the gating measurement.

## Production `.cs`

| Path | [P0-T14] baseline | [P2-T18] pre-format | [P3-T10] post-format | D11 budget | Headroom | Verdict |
|---|---|---|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` | NOT PRESENT | 63 | 63 | 500 | 437 | met |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` | NOT PRESENT | 92 | 93 | 500 | 407 | met |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 141 | 300 | 302 | 500 | 198 | met |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 1003 | 1002 | 1002 | 1003 (no growth) | 1 | met |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 312 | 295 | 295 | 500, and [P2-T10] additionally requires at or below its 312 baseline | 205 | met |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 304 | 407 | 407 | 500 | 93 | met |
| `QuickFiler/Controllers/EfcFormController.cs` | 1320 | 1321 | 1321 | 1322 | 1 | met |
| `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` | NOT PRESENT | 41 | 41 | 500 | 459 | met |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 500 | 467 | 467 | 500 (hard) | 33 | met |

## Test `.cs`

| Path | [P0-T14] baseline | [P2-T18] pre-format | [P3-T10] post-format | Budget | Headroom | Verdict |
|---|---|---|---|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` | NOT PRESENT | 176 | 176 | 500 | 324 | met |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs` | NOT PRESENT | 217 | 217 | 500 | 283 | met |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` | NOT PRESENT | 344 | 346 | 500 | 154 | met |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` | NOT PRESENT | 213 | 212 | 500 | 288 | met |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` | NOT PRESENT | 425 | 463 | 500 | 37 | met |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` | 479 | 480 | 480 | 483 ([P1-T13] +4) | 3 | met |
| `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` | 354 | 363 | 363 | 500 | 137 | met |

## PROJECT-FILE (exempt)

Recorded as exempt observations rather than asserted against the ceiling, per R8: the 500-line cap in
.claude/rules/general-code-change.md covers production code, test code and reusable script files and does not
reach project files, and .csharpierignore lines 9-14 record that project files are owned by Visual Studio and are
not C# source. CSharpier does not process them either, so these counts are unchanged from [P2-T18].

- PROJECT-FILE (exempt): `UtilitiesCS/UtilitiesCS.csproj` = 1317 (baseline 1315, +2 Compile Include)
- PROJECT-FILE (exempt): `QuickFiler/QuickFiler.csproj` = 606 (baseline 605, +1 Compile Include)
- PROJECT-FILE (exempt): `UtilitiesCS.Test/UtilitiesCS.Test.csproj` = 980 (baseline 976, +4 Compile Include)
- PROJECT-FILE (exempt): `QuickFiler.Test/QuickFiler.Test.csproj` = 530 (baseline 529, +1 Compile Include)

## What the formatter changed

The [P3-T1] pass moved five of the sixteen `.cs` files in this set, by a net +42 lines overall:

- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` 425 to 463 (+38)
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` 344 to 346 (+2)
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` 300 to 302 (+2)
- `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` 92 to 93 (+1)
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` 213 to 212 (-1)

The two files carrying only one line of headroom — `QuickFiler/Controllers/EfcFormController.cs` at 1321 against
1322 and `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` at 1002 against 1003 — were NOT rewritten by the
formatter and are unchanged from their pre-format counts, so neither budget was breached. This was the identified
risk in this task and it did not materialise. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` was
likewise untouched at 467.

## Smallest remaining headroom

SMALLEST-REMAINING-HEADROOM: 1 line, on two files.

- `QuickFiler/Controllers/EfcFormController.cs` — 1321 against its D11 budget of 1322.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — 1002 against its D11 no-growth budget of 1003.

Next after those: `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` at 480 against
its 483 budget (3 lines), then `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` at 463
against 500 (37 lines).

## The three disclosed pre-existing over-ceiling files and their budgets (D11)

R8 and D11 disclose three files that were ALREADY over the 500-line ceiling before any change in this plan. They
are gated by a per-file budget rather than by the ceiling, because a blanket "at or below 500" assertion would be
unsatisfiable on them:

| Path | Pre-existing count | D11 budget | Post-format count | Verdict |
|---|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 1003 | 1003, no growth | 1002 | met, and one line below the pre-existing count |
| `QuickFiler/Controllers/EfcFormController.cs` | 1320 | 1322, that is baseline plus at most two | 1321 | met |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs | 1066 | not touched by this plan | 1066, unchanged | not in the Write Set; disclosed only |

Neither over-ceiling file is repaired here and neither grew beyond its budget. `FolderPredictor.cs` in fact ends
one line SMALLER than it began, so this change moves it toward the ceiling rather than away from it.

## Acceptance conditions

- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` at or below 500: 467, met.
- `QuickFiler/Controllers/EfcFormController.cs` at or below 1322: 1321, met.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` at or below 1003: 1002, met.
- Every other listed `.cs` file at or below 500: the largest is
  `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` at 463, met.
- Exempt project-file counts recorded but not asserted against the ceiling: done.
- Smallest remaining headroom stated, together with the three disclosed pre-existing over-ceiling files and their
  budgets: done.

Output Summary: Every `.cs` file in this plan's footprint satisfies its D11 budget after the final format pass. No
budget was breached. The two files that entered this task with one line of headroom were not touched by the
formatter and remain at 1321 and 1002 against budgets of 1322 and 1003. The R9 ordering constraint held:
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` finishes at 467 and never passed through 501, because
[P2-T3] removed the relocated member before [P2-T14] added the constructor argument to it in the new partial.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
