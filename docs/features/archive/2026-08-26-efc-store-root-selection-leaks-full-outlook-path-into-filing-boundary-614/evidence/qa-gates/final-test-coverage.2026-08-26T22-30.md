# P5-T4 — Final QC step 4: Full Test-and-Coverage Run (remediation cycle 1, issue #614)

Timestamp: 2026-08-26T22-30

Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`

EXIT_CODE: 0

`ExpectedExitCode` is deliberately omitted: the run is fully green, so the default expectation of 0
matches the observed exit code. No rule-6 flake (#594 / #592 / #586 / #584) was observed.

This is the FINAL clean-pass run. The toolchain loop required two attempts, and the restart was
triggered at P5-T1 by a formatter rewrite, before any of P5-T2 / P5-T3 / P5-T4 had been executed.
P5-T1 through P5-T4 therefore executed exactly once each in one uninterrupted clean sequence.

## Output Summary — test results

Runner verdict line: `Test Run Successful.`

| Metric | P0-T9 baseline (this cycle) | Review reference | This run |
| --- | ---: | ---: | ---: |
| Total tests | 6569 | 6569 | **6587** |
| Passed | 6569 | 6569 | **6587** |
| Failed | 0 | 0 | **0** |
| Skipped | 0 | 0 | **0** |
| Exit code | 0 | 0 | **0** |
| Total time | 32.9965 s | 37.72 s | 35.9063 s |

- Delta **+18 tests, all passing** — exactly the 18 `EfcSelectionGuardTests` methods this cycle
  adds (the class grew from 9 to 27 methods). Every one of the 6587 result lines is `Passed`; the
  runner printed 6587 `Passed` lines.
- Lines matching `^\s*Failed ` in the full runner log: **0**. Lines matching `^\s*Skipped `: **0**.
- **No NEW failure** relative to either the P0-T9 baseline or the 6569 / 6569 / 0 reference.

### Must-stay-green set (plan rule 8) — each verified PASSED in this run

| Test | Result |
| --- | --- |
| `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` | Passed (file unedited) |
| `Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` | Passed, in its P3-T4-corrected form from the delivery cycle (file unedited) |
| `Issue439ActiveAncestorChildrenAndEmbeddedBridgeUseTypedStoppedActivation` | Passed |
| `Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild` | Passed |
| `Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability` | Passed |
| `Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter` | Passed |
| `Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows` | Passed |
| `Issue439ResolvedFullHierarchyRetainsOriginalFilingTargetAndScore` | Passed |
| `Issue439ResolvedLineageUsesUnicodeArrowSeparators` | Passed |
| `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection` | Passed |
| `Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome` | Passed |
| `Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce` | Passed |
| `Issue609_FolderPredictor_ProjectsCaseVariantInRootFullSuggestionPath` | Passed |
| `Issue609_FolderPredictor_ProjectsOnlyInRootFullSuggestionPaths` | Passed |
| `Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget` | Passed |
| `Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget` | Passed |
| `Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget` | Passed |
| all `Issue614_*` tests added by the #614 delivery | Passed (7 named in the log, all green) |
| `TryMakeArchiveRelative_SeparatorOnlyRoot_ReturnsFalse` | Passed |
| all 27 `EfcSelectionGuardTests` methods | Passed |

### The two CR fixes, verified in the full-suite run

| Test | Result |
| --- | --- |
| `IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted` | Passed (CR-1) |
| `IsValidFilingSelection_SingleCharacterRelativeStem_IsAccepted` | Passed (CR-1) |
| `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted` | Passed (CR-2) |
| `IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted` | Passed (CR-2) |
| `IsValidFilingSelection_StoreRootedSelection_IsRejected` | Passed (D1/D9 intact) |
| `IsValidFilingSelection_RootedTargetAboveArchiveRoot_IsRejected` | Passed (D1 intact) |
| `IsValidFilingSelection_CrossStoreRootedTarget_IsRejected` | Passed (D4 intact) |
| `IsValidFilingSelection_SeparatorBoundaryNearMiss_IsRejected` | Passed (D9 intact) |
| `IsValidFilingSelection_RootedTargetWithUnavailableRoot_IsRejected` | Passed (degrade path) |
| `IsValidCreationSelection_TwoCharacterSelection_IsRejected` | Passed (creation length rule retained) |

## Output Summary — coverage

The run was green, so the runner performed its in-place `ConvertTo-KoverageCoberturaXml` rewrite and
the raw pre-post-processing figure is unavailable for this run, as on the P0-T9 baseline. That
figure is informational and gates nothing.

`coverage\coverage.cobertura.xml` was copied to `coverage\coverage.cobertura.filtered.p5-t4.xml`
(gitignored `coverage/` tree, never under `evidence/`), which is the P5-T5 post-change input.

| Figure | P0-T9 baseline | Review reference | This run |
| --- | ---: | ---: | ---: |
| Filtered first-party line coverage | 84.8712% (53973 / 63594) | 84.8696% | **84.8790% (54000 / 63620)** |
| Filtered branch coverage | 78.8454% (12743 / 16162) | 78.8331% | **78.8523% (12752 / 16172)** |

Both figures are ABOVE the P0-T9 baseline and above the review reference. No coverage regression.

Allowlist packages present in the filtered artifact (9): QuickFiler, UtilitiesCS, TaskVisualization,
SVGControl, ToDoModel, Tags, TaskMaster, TaskTree, VBFunctions.

The pre-existing repo-wide shortfall against the 85% floor is reported, unchanged in character, and
explicitly NOT gated to 85% this cycle per the remediation inputs.

Raw runner log contains absolute host paths including the machine account name; it was written to
the session scratchpad outside the repository and is not copied under `evidence/`.
