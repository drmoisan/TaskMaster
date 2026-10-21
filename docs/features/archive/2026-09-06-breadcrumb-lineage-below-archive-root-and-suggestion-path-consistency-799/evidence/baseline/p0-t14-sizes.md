# [P0-T14] Baseline line counts of the files this plan edits or creates

Timestamp: 2026-09-07T07-16

Command: (Get-Content -LiteralPath <path>).Count for each path below

EXIT_CODE: 0

CEILING: 500 (applies to *.cs only)

## Existing Write Set production paths (.cs)

- UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs = 141
- UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs = 1003
- QuickFiler/Controllers/QfcItemController.FolderHandling.cs = 312
- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs = 304
- QuickFiler/Controllers/EfcFormController.cs = 1320
- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs = 500

## Retargeted Write Set test paths (.cs)

- UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs = 479
- QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs = 354

## NO HUNK test paths from D8 (.cs)

- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs = 455
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs = 253

## Files this plan creates

Each of the eight paths below was probed with `Test-Path` and returned False, so none exists at baseline and none
has a measurable baseline line count. They are recorded as NOT PRESENT rather than as a count of zero, and their
first measurement is made by [P3-T10] after the final CSharpier pass.

- NOT PRESENT AT BASELINE: UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs
- NOT PRESENT AT BASELINE: UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs
- NOT PRESENT AT BASELINE: QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs
- NOT PRESENT AT BASELINE: UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs
- NOT PRESENT AT BASELINE: UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs
- NOT PRESENT AT BASELINE: UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs
- NOT PRESENT AT BASELINE: UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs
- NOT PRESENT AT BASELINE: QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs

## PROJECT-FILE (exempt)

Project files are recorded as exempt observations rather than asserted against the ceiling, per R8:
.claude/rules/general-code-change.md caps production code, test code and reusable script files at 500 lines and
does not reach project files, and `.csharpierignore` lines 9-14 record that project files are owned by Visual
Studio and are not C# source.

- PROJECT-FILE (exempt): UtilitiesCS/UtilitiesCS.csproj = 1315
- PROJECT-FILE (exempt): QuickFiler/QuickFiler.csproj = 605
- PROJECT-FILE (exempt): UtilitiesCS.Test/UtilitiesCS.Test.csproj = 976
- PROJECT-FILE (exempt): QuickFiler.Test/QuickFiler.Test.csproj = 529

## PRE-EXISTING OVER CEILING

Three files are already over the 500-line ceiling before any change in this plan. They are disclosed here, not
repaired, and are gated by the D11 per-file budgets rather than by the ceiling:

- UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs = 1003 — in the Write Set. D11 budget: at or below 1003
  (no growth).
- QuickFiler/Controllers/EfcFormController.cs = 1320 — in the Write Set. D11 budget: at or below 1322, that is
  baseline plus at most two lines, because the single added constructor argument is formatted by CSharpier as an
  additional line.
- UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs = 1066 — NOT touched by this plan.

## Ceiling-relevant statements required by the acceptance condition

- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is EXACTLY 500 lines, at the ceiling and not merely
  near it. This is the file R9's hard ordering constraint protects: [P2-T3] must relocate the breadcrumb pipeline
  helper into the new partial before [P2-T14] adds the constructor argument, or the file passes through a 501-line
  intermediate state. D11 budget: at or below 500 (hard).
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` is 479 lines and therefore has
  21 lines of headroom against the 500-line ceiling. That headroom is why D9 places the no-accessor companion
  case in the new file OutlookFolderHierarchyProviderTrimTests.cs rather than in this file, and why the Write Set
  gives this file a budget of +4 lines (ceiling 483) for the [P1-T13] retarget.

Output Summary: All ten existing `.cs` paths have numeric baseline counts, and every count reproduces the figure
the plan's citation table records. Three files are over the ceiling before any change and are disclosed above with
their D11 budgets; the four project files are recorded under the exempt heading with the R8 reason. The two
ceiling-relevant statements the acceptance condition requires are recorded: ViewerSetup.cs is exactly 500, and
OutlookFolderHierarchyProviderTests.cs has 21 lines of headroom. [P3-T10] re-measures these counts after the final
CSharpier pass, because the formatter can change line counts.
