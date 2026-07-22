# Duplicate Identity Fail-Before

Timestamp: 2026-07-21T22-31Z
Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbDuplicateIdentityTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~FolderBreadcrumbAssetContractTests" /Logger:"console;Verbosity=normal"`
EXIT_CODE: 1
Output Summary: Expected-failure gate accepted. VSTest resolved through the repository-standard Visual Studio discovery path and discovered 23 filtered tests across both assemblies. All 11 newly named duplicate-identity regressions failed for duplicate logical identity, first-match activation/commit, wrong collapsed probability selection, or duplicate active-option behavior. All 12 pre-existing asset-contract controls passed. No build, discovery, tool-resolution, environmental, or unrelated test failure occurred.

## Resolution and totals

- Resolved VSTest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
- Assemblies matched: 2.
- Total tests: 23.
- Passed: 12.
- Failed: 11.
- Skipped: 0.
- Test time: 2.5567 seconds.

## Newly named expected failures

1. `BreadcrumbDuplicateIdentityTests.SetSuggestionFallbacks_DuplicateSuggestionAndRecentPathsHaveDistinctIdentities` — failed because both selectable occurrences used identity `\Inbox\Shared`.
2. `BreadcrumbDuplicateIdentityTests.SetSuggestionsAsync_ResolvedUpgradePreservesDistinctFallbackIdentities` — failed because the resolved identities remained duplicated as `\Inbox\Shared`.
3. `BreadcrumbDuplicateIdentityTests.ClosedMoveNext_DuplicateOutputPathsCommitsSecondLogicalRow` — failed because the second row identity equaled the first identity.
4. `BreadcrumbDuplicateIdentityTests.OpenMoveNextThenCommit_DuplicateOutputPathsCommitsSecondLogicalRow` — failed because committing the pending duplicate returned `false`.
5. `BreadcrumbDuplicateIdentityTests.Activate_SecondDuplicateIdentityCommitsExactLogicalRow` — failed because activating the second duplicate identity returned `false`.
6. `BreadcrumbDuplicateIdentityTests.OpenCommit_CollapsedReadbackUsesSecondDuplicateSuggestionProbability` — failed because committing the second scored duplicate returned `false` before the required row/probability readback.
7. `BreadcrumbDuplicateIdentityIntegrationTests.ClosedDown_DuplicateSuggestionAndRecentCommitsRecentOccurrence` — failed because the committed identity remained `\Inbox\Shared`, the same logical identity as the first occurrence.
8. `BreadcrumbDuplicateIdentityIntegrationTests.OpenDownThenEnter_DuplicateSuggestionAndRecentCommitsPendingOccurrence` — failed because the two published selectable identities were not unique.
9. `BreadcrumbDuplicateIdentityIntegrationTests.ActivateSelector_SecondPublishedIdentityCommitsExactDuplicateOccurrence` — failed because exact second-occurrence activation returned `false`.
10. `BreadcrumbDuplicateIdentityIntegrationTests.CollapsedReadback_SecondDuplicateSuggestionRetainsItsProbability` — failed because the two published identities were not unique before the selected duplicate's percentage could be verified.
11. `FolderBreadcrumbAssetContractTests.ExpandedDuplicatePathState_YieldsExactlyOneActiveAriaSelectedOption` — failed because two selectable options matched the pending identity instead of exactly one.

These failures directly prove the planned pre-fix first-match and duplicate-active defects.

## Existing controls that passed

1. `FolderBreadcrumbAssetContractTests.CompiledResource_RemainsSelfContainedAndThemeAware`.
2. `FolderBreadcrumbAssetContractTests.CollapsedMode_RendersOnlyTheCommittedSelectedDataRow`.
3. `FolderBreadcrumbAssetContractTests.Percentage_UsesVisibleHostSuppliedPercentTextWithoutRecomputation`.
4. `FolderBreadcrumbAssetContractTests.CollapsedDocumentAndList_HideVerticalOverflowWithoutScrollControls`.
5. `FolderBreadcrumbAssetContractTests.Markup_ContainsExactlyOneAccessibleDropDownButton`.
6. `FolderBreadcrumbAssetContractTests.SelectorView_UpdatesModeAndAccurateAriaExpandedState`.
7. `FolderBreadcrumbAssetContractTests.ExpandedRows_ExposeListboxOptionsAndOneActiveSelectedOption`.
8. `FolderBreadcrumbAssetContractTests.ActiveRow_ScrollsIntoViewOnlyInExpandedMode`.
9. `FolderBreadcrumbAssetContractTests.SelectorKeys_PreventBrowserScrollingAndPostNativeKeyMessages`.
10. `FolderBreadcrumbAssetContractTests.ButtonAndRows_PostToggleAndStableIdentityActivationMessages`.
11. `FolderBreadcrumbAssetContractTests.LeftAndRightBreadcrumbMessages_RemainSupported`.
12. `FolderBreadcrumbAssetContractTests.ModeAndThemeHooks_RemainIndependentAndFocusTheActiveListTarget`.
