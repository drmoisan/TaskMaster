# P9-T4 - Final QC step 4: full test-and-coverage run (#614; AC19 full-suite, AC24 step 4)

Timestamp: 2026-08-26T19-45

Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`

The repo's canonical coverage runner: full suite, coverage-enabled via `dotnet-coverage`, with
`/InIsolation` and `TestCategory!=LiveOutlook` semantics, discovering `*.Test.dll` under
`\bin\Debug\`. It resolves `vstest.console.exe` via vswhere. This is the same command used for
the Phase 0 baseline. No `/Logger` is passed, so no account/host-named TRX is produced.

This artifact records the FINAL clean-pass run (loop attempt 4). Three earlier attempts are
enumerated in `toolchain-clean-pass.2026-08-26T19-55.md`.

EXIT_CODE: 0

`ExpectedExitCode` is deliberately OMITTED: the run is fully green, so the default expectation of 0
matches the observed exit code.

## Output Summary - test results

Runner verdict line: `Test Run Successful.`

| Metric | Phase 0 baseline (measured) | This run |
| --- | --- | --- |
| Total tests | 6482 | **6569** |
| Passed | 6482 | **6569** |
| Failed | 0 | **0** |
| Skipped | 0 | **0** |
| Exit code | 0 | **0** |
| Total time | 56.87 s | 37.72 s |

- Delta: **+87 tests, all passing** - the tests this change adds (22 + 1 `ArchiveStemContractTests`
  after the separator-only-root case was added at P9-T5 remediation, 8
  `BreadcrumbBridgeRouterIssue614Tests`, 1 AC18 test in `BreadcrumbBridgeRouterTests`, 5 new
  `EmailFilerConfig_Tests`, 9 `EfcSelectionGuardTests`, 19 `FolderConverterIssue614Tests`, 8
  `EfcDataModelIssue614Tests`, 6 `AppOlObjectsArchiveRootValidationTests`, 7
  `AppFileSystemFolderPathsOneDriveResolutionTests`, plus 1 further discovered case).
- **No NEW failure relative to the baseline.** Lines matching `^\s*Failed ` in the full runner
  log: **0**. Lines matching `^\s*Skipped `: **0**.
- Pre-identified flakes #594 / #592 / #586 / #584: none observed. In particular
  `UtilitiesCS.Test.OutlookObjects.FilterDASL.DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole`
  (the #594 Console.Out race) PASSED, as it did on the Phase 0 baseline.

### Must-stay-green set - enumerated, each verified PASSED in this run

| Test | Result |
| --- | --- |
| `Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability` | Passed (unedited) |
| `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` | Passed (unedited) |
| `Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild` | Passed (unedited) |
| `Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows` | Passed (unedited) |
| `Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome` | Passed (unedited) |
| `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection` | Passed (unedited) |
| `Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` | Passed **in its P3-T4-corrected form** - the single documented exception, the D1/D9 analogue of the `FolderConverterTests.cs:329` carve-out |
| `Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce` | Passed (unedited) |
| `Issue609_FolderPredictor*` in `FolderPredictorTests.cs` | 2 tests, both Passed (unedited; `FolderPredictor.cs` is not modified) |

### Test classes added or edited by this change - zero failures

The suite reports zero failures overall, so every added or edited class has zero failures. Named
explicitly: `Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers`
Passed, `Issue614_SegmentActivate_StoreRootSegment_DoesNotStoreFullOutlookPath` Passed,
`SegmentActivate_CrossStoreAncestor_LeavesSelectionUnchangedAndDiagnoses` Passed, and
`TryMakeArchiveRelative_SeparatorOnlyRoot_ReturnsFalse` Passed.

## Output Summary - coverage

The run was fully green, so `Invoke-DotnetCoverageCollection` did not throw and the runner
performed its in-place `ConvertTo-KoverageCoberturaXml` rewrite. The raw pre-post-processing
Cobertura was therefore consumed in place and the unfiltered figure is **unavailable for this
run** - exactly as on the Phase 0 baseline. Per the plan that figure is informational only and
gates nothing (reference value 74.4666%). For reference, the loop-attempt-2 run failed and
therefore left the raw artifact intact; its unfiltered figure was 70.2819% (57459 / 81755).

`coverage\coverage.cobertura.xml` is the allowlist-filtered artifact. It was copied to
`coverage\coverage.cobertura.filtered.p9-t4.xml` (gitignored `coverage/` tree; never under
`evidence/`), which is the P9-T5 post-change input.

| Figure | Phase 0 baseline (measured) | This run |
| --- | --- | --- |
| Filtered first-party line coverage | 84.7797% (53769 / 63422) | **84.8696% (53972 / 63594)** |
| Filtered branch coverage | 78.6938% (12676 / 16108) | **78.8331% (12741 / 16162)** |
| Unfiltered line coverage | unavailable (raw consumed in place) | unavailable (raw consumed in place) |

Allowlist packages present in the filtered artifact (9, matching `Get-KoverageProjectAllowlist`):
QuickFiler, UtilitiesCS, TaskVisualization, SVGControl, ToDoModel, Tags, TaskMaster, TaskTree,
VBFunctions.

Raw runner log (contains absolute host paths, including the machine account name) was written to
the session scratchpad outside the repository and is not copied under `evidence/`.
