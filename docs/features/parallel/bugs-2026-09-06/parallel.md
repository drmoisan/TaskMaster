---
parallel: bugs-2026-09-06
mode: open
max_concurrency: 4
created_at: "2026-09-07T04:35:00Z"
items:
  - issue_num: 796
    feature_folder: docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/**
        - QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs
        - QuickFiler.Test/Controllers/QfcItemController.SearchLeaveLatchTests.cs
        - QuickFiler.Test/QuickFiler.Test.csproj
        - QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs
        - QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs
        - QuickFiler/Controllers/QfcFormController.Deactivate.cs
        - QuickFiler/Controllers/QfcItemController.EventHandlers.cs
        - QuickFiler/Interfaces/IQfcFormViewer.cs
        - QuickFiler/QuickFiler.csproj
        - QuickFiler/Resources/FolderBreadcrumb.html
        - QuickFiler/Viewers/BreadcrumbDropDownHost.cs
        - QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs
        - QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
        - QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs
        - QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
        - QuickFiler/Viewers/QfcFormViewer.cs
      modules:
        - QuickFiler
        - QuickFiler.Test
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-07T04:04:58Z"
  - issue_num: 797
    feature_folder: docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/**
        - docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md
        - TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs
        - TaskMaster.Test/TaskMaster.Test.csproj
        - TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs
        - TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs
        - UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs
        - UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs
        - UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs
        - UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs
        - UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs
        - UtilitiesCS.Test/UtilitiesCS.Test.csproj
        - UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs
        - UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
        - UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
        - UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
        - UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs
        - UtilitiesCS/UtilitiesCS.csproj
      modules:
        - TaskMaster
        - TaskMaster.Test
        - UtilitiesCS
        - UtilitiesCS.Test
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-07T04:23:06Z"
  - issue_num: 798
    feature_folder: docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/**
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/coverage-baseline.cobertura.xml
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/csharpier-check-baseline.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/line-cap-preexisting.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/line-count-baseline.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/log4net-capture-probe.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/mcp-validator-probe.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/msbuild-analyzers-baseline.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/msbuild-nullable-baseline.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/nuget-restore.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/per-file-coverage-baseline.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/phase0-instructions-read.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/shell-icon-stall-probe.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/toolchain-bootstrap.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/vstest-coverage-baseline.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/worktree-identity.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/issue-updates/ac-status-summary.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/other/ac6-manual-verification-handoff.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/other/followup-promotions.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-delta.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/final-csharpier.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/final-msbuild-analyzers.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/final-msbuild-nullable.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/final-toolchain-clean-pass.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/final-vstest-coverage.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p1-csharpier.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p1-seam-build-and-scoped-tests.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p3-banned-symbol-and-overload-scope.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p5-ac10-out-of-scope-throws.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p6-ac11-handler-inventory.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac12-inverse-constraints.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-compile-entries.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-line-cap.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-write-set-diff.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac8-timeoutafter-unchanged.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac9-fixed-arity.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-csharpier-final.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/per-file-coverage-final.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac1-cancellation.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac1-loud-failure-fail-before.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac1-nonoverlap-fail-before.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac1-positive-paths.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac2-timing-fail-before.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac3-message-fail-before.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac3-negative-fail-before.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac4-fail-before.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac5-boundary-fail-before.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-ac5-shape-fail-before.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p2-consolidated-fail-before.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p3-ac1-ac2-pass-after.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p4-ac3-pass-after.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p5-ac4-pass-after.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p6-ac5-pass-after.md
        - docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md
        - evidence/baseline/line-cap-preexisting.md
        - evidence/baseline/log4net-capture-probe.md
        - evidence/qa-gates/coverage-delta.md
        - evidence/qa-gates/final-msbuild-analyzers.md
        - evidence/qa-gates/final-toolchain-clean-pass.md
        - evidence/qa-gates/p3-banned-symbol-and-overload-scope.md
        - evidence/qa-gates/p5-ac10-out-of-scope-throws.md
        - evidence/qa-gates/p6-ac11-handler-inventory.md
        - evidence/qa-gates/p7-ac12-inverse-constraints.md
        - evidence/qa-gates/p7-ac13-compile-entries.md
        - evidence/qa-gates/p7-ac13-line-cap.md
        - evidence/qa-gates/p7-ac13-write-set-diff.md
        - evidence/qa-gates/p7-ac8-timeoutafter-unchanged.md
        - evidence/qa-gates/p7-ac9-fixed-arity.md
        - evidence/regression-testing/p2-ac1-loud-failure-fail-before.md
        - evidence/regression-testing/p2-ac1-nonoverlap-fail-before.md
        - evidence/regression-testing/p2-ac2-timing-fail-before.md
        - evidence/regression-testing/p2-ac3-message-fail-before.md
        - evidence/regression-testing/p2-ac3-negative-fail-before.md
        - evidence/regression-testing/p2-ac4-fail-before.md
        - evidence/regression-testing/p2-ac5-boundary-fail-before.md
        - evidence/regression-testing/p2-ac5-shape-fail-before.md
        - evidence/regression-testing/p3-ac1-ac2-pass-after.md
        - evidence/regression-testing/p4-ac3-pass-after.md
        - evidence/regression-testing/p5-ac4-pass-after.md
        - evidence/regression-testing/p6-ac5-pass-after.md
        - QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs
        - QuickFiler.Test/QuickFiler.Test.csproj
        - QuickFiler/Controllers/QfcDatamodel.cs
        - QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs
        - TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs
        - TaskMaster.Test/TaskMaster.Test.csproj
        - TaskMaster/Ribbon/RibbonCommandBoundary.cs
        - TaskMaster/Ribbon/RibbonViewer.cs
        - TaskMaster/TaskMaster.csproj
        - UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs
        - UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs
        - UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs
        - UtilitiesCS.Test/UtilitiesCS.Test.csproj
        - UtilitiesCS/Extensions/DfDeedle.cs
        - UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs
        - UtilitiesCS/UtilitiesCS.csproj
      modules:
        - QuickFiler
        - QuickFiler.Test
        - TaskMaster
        - TaskMaster.Test
        - UtilitiesCS
        - UtilitiesCS.Test
      shared_surfaces: []
      contracts:
        - "Columns.Add"
        - "Columns.Remove"
        - "[Df"
        - "folder.UserDefinedProperties"
        - "timing]"
      source: declared
      computed_at: "2026-09-07T04:25:07Z"
  - issue_num: 799
    feature_folder: docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799/**
        - QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs
        - QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs
        - QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs
        - QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs
        - QuickFiler.Test/QuickFiler.Test.csproj
        - QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
        - QuickFiler/Controllers/EfcFormController.cs
        - QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs
        - QuickFiler/Controllers/QfcItemController.FolderHandling.cs
        - QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
        - QuickFiler/QuickFiler.csproj
        - UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs
        - UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs
        - UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs
        - UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs
        - UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs
        - UtilitiesCS.Test/UtilitiesCS.Test.csproj
        - UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs
        - UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs
        - UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs
        - UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs
        - UtilitiesCS/UtilitiesCS.csproj
      modules:
        - QuickFiler
        - QuickFiler.Test
        - UtilitiesCS
        - UtilitiesCS.Test
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-07T04:12:07Z"
expected_conflict_components:
  - name: csproj-compile-entry-component
    members:
      - 796
      - 797
      - 798
      - 799
---

# Parallel Run: bugs-2026-09-06

Four thematically unrelated QuickFiler, Outlook-store and breadcrumb defects captured on
2026-09-06 and prepared through independent preparation-mode orchestrator runs. Each item owns
its own feature branch cut from `origin/main` at `c431dc32`, carries an approved atomic plan at
`PREFLIGHT: ALL CLEAR`, and opens its own pull request against `main`. There is no integration
branch.

## Cohorts at generation 0

| cohort | item_keys | may run concurrently |
| --- | --- | --- |
| 0 | 798 | no peer |
| 1 | 799 | no peer |
| 2 | 796, 797 | yes, both at once |

Five of the six unordered pairs conflict. The single non-edge is 796 with 797: item 796 is
confined to QuickFiler and QuickFiler.Test, item 797 to TaskMaster, TaskMaster.Test, UtilitiesCS
and UtilitiesCS.Test, so they share no path and no module.

## Why the items contend

Every derived edge rests on the same mechanism, and it is genuine rather than an artifact. Each
item adds at least one new C# file, every project in this repository is non-SDK-style with an
explicit compile-entry list and no wildcard globbing, and so each item must edit the project
file of every assembly it adds a file to. Two items that share no source file still contend on
that project file, correctly. Do not attempt to widen concurrency by narrowing a blast radius.

## Recorded radius defects

Two derivation defects are recorded here rather than corrected, because correcting either would
mean editing a radius rather than the plan text it is derived from.

Item 796 carries one planner hand-append. The extractor drops any path whose extension falls
outside a closed 23-member allow list, and `html` is outside it, so the WebView2 breadcrumb page
the plan modifies seven times was absent from the derived radius. It was appended after
normalization, per the planner obligation.

Item 798 is over-reported. Its declared Write Set names 16 concrete paths; derivation produced 97
paths and five contract tokens, because the plan backticks its own evidence-artifact paths and
because C# member expressions and a split log prefix were harvested as contracts. The
over-report was measured and changes no conflict edge: every 798 edge is independently
determined by a project-file path overlap and a module overlap drawn from the declared 16. It is
therefore recorded as derived rather than corrected through a further preparation round.
