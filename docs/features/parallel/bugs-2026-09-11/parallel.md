---
parallel: bugs-2026-09-11
mode: open
max_concurrency: 3
created_at: "2026-09-13T04:01:56Z"
items:
  - issue_num: 583
    feature_folder: docs/features/active/kastringasync-keyequals-contains-offset-583
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - "QuickFiler.Test/Controllers/KaStringAsyncTests.cs"
        - "QuickFiler/Controllers/KaStringAsync.cs"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 602
    feature_folder: docs/features/active/2026-09-12-host-identifier-leakage-sweep-602
    kind: bug
    state: withdrawn
    blast_radius:
      paths:
        - ".claude/agent-memory/**/*.md"
        - ".claude/agent-memory/atomic-executor/project_bash_heredoc_collapses_doubled_backslashes.md"
        - ".claude/agent-memory/atomic-planner/MEMORY.md"
        - ".claude/agent-memory/atomic-planner/project_602_preparation_artifacts_shift_population_figures.md"
        - ".claude/agent-memory/orchestrator/bash-filter-refuses-the-word-parallel-in-a-git-pathspec.md"
        - ".claude/agent-memory/orchestrator/bash-tool-rejects-complex-commands-in-isolated-worktree.md"
        - ".claude/agent-memory/orchestrator/bootstrapping-orchestrator-state-json-first-write.md"
        - ".claude/agent-memory/orchestrator/byte-exact-copy-via-git-plumbing.md"
        - ".claude/agent-memory/orchestrator/new-active-feature-folder-date-prefix.md"
        - ".claude/agent-memory/orchestrator/subagent-self-reported-correction-can-be-false.md"
        - ".claude/skills/cleanup-merged-worktrees/SKILL.md"
        - ".claude/state/powershell-batch-budget.default.json"
        - ".gitignore"
        - ".vscode/settings.json"
        - "docs/features/**/*.trx"
        - "docs/features/**/evidence/**/*.process-tree.json"
        - "docs/features/**/evidence/**/*.txt"
        - "docs/features/**/evidence/**/*.xml"
        - "docs/features/active/**/*.md"
        - "docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/redaction-sweep.2026-08-26T22-44.md"
        - "docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/policy-audit.2026-09-04T02-11.md"
        - "docs/features/archive/**/*.md"
        - "docs/features/epics/**/*.md"
        - "docs/research/**/*.md"
        - "scripts/dev-tools/Repair-HostIdentifierLeak.ps1"
        - "scripts/dev-tools/run-actionlint.ps1"
        - "TaskMaster/TaskMaster.csproj"
        - "test-output.txt"
        - "tests/scripts/dev-tools/Repair-HostIdentifierLeak.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 742
    feature_folder: docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - "QuickFiler.Test/Controllers/EfcItemControllerTests.cs"
        - "QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs"
        - "QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs"
        - "QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs"
        - "QuickFiler.Test/Controllers/QuickFilerInvariantCultureIssue742Tests.cs"
        - "QuickFiler.Test/QuickFiler.Test.csproj"
        - "QuickFiler/Controllers/EfcHomeController.Metrics.cs"
        - "QuickFiler/Controllers/EfcItemController.cs"
        - "QuickFiler/Controllers/QfcCollectionController.cs"
        - "QuickFiler/Controllers/QfcHomeController.Metrics.cs"
        - "QuickFiler/Controllers/QfcItemController.cs"
        - "QuickFiler/Controllers/QfcItemController.ViewerSetup.cs"
        - "QuickFiler/Interfaces/IQfcItemController.cs"
        - "QuickFiler/Properties/AssemblyInfo.cs"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 743
    feature_folder: docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - ".claude/agent-memory/atomic-executor/MEMORY.md"
        - ".claude/agent-memory/atomic-executor/project_743_seam_conversion_breaks_untouchable_test_and_r4_designed_contention.md"
        - ".claude/agent-memory/atomic-planner/MEMORY.md"
        - ".claude/agent-memory/atomic-planner/project_743_itemviewer_marshalling_seam_plan_seams.md"
        - ".claude/agent-memory/atomic-planner/reference_invoke_mstest_single_searchroot_defect.md"
        - ".claude/agent-memory/orchestrator/blast-radius-audit-must-cover-the-plan-too.md"
        - ".claude/agent-memory/orchestrator/MEMORY.md"
        - ".claude/agent-memory/orchestrator/model-routing-feature-review-is-always-fable.md"
        - ".claude/agent-memory/orchestrator/recovering-a-dead-agent-worktree-via-shared-git.md"
        - ".claude/agent-memory/task-researcher/MEMORY.md"
        - ".claude/agent-memory/task-researcher/project_pump_timeout_743.md"
        - "QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs"
        - "QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs"
        - "QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs"
        - "QuickFiler.Test/QuickFiler.Test.csproj"
        - "QuickFiler/Controllers/QfcItemController.ViewerSetup.cs"
        - "QuickFiler/Viewers/IItemViewer.cs"
        - "QuickFiler/Viewers/ItemViewer.cs"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 792
    feature_folder: docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - "QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue792Tests.cs"
        - "QuickFiler.Test/Controllers/BreadcrumbOutboundQueueIssue792Tests.cs"
        - "QuickFiler.Test/Controllers/EfcDataModelIssue792CarryTests.cs"
        - "QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs"
        - "QuickFiler.Test/Controllers/EfcFormControllerTests.cs"
        - "QuickFiler.Test/Controllers/QfcCollectionControllerIssue792PopOutTests.cs"
        - "QuickFiler.Test/Helper Classes/EfcViewerQueueIssue792Tests.cs"
        - "QuickFiler.Test/QuickFiler.Test.csproj"
        - "QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs"
        - "QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs"
        - "QuickFiler/Controllers/BreadcrumbBridgeRouter.cs"
        - "QuickFiler/Controllers/BreadcrumbOutboundQueue.cs"
        - "QuickFiler/Controllers/EfcDataModel.Carry.cs"
        - "QuickFiler/Controllers/EfcDataModel.cs"
        - "QuickFiler/Controllers/EfcFormController.Actions.cs"
        - "QuickFiler/Controllers/EfcFormController.Breadcrumb.cs"
        - "QuickFiler/Controllers/EfcFormController.cs"
        - "QuickFiler/Controllers/EfcFormController.EventHandlers.cs"
        - "QuickFiler/Controllers/EfcFormController.Helpers.cs"
        - "QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs"
        - "QuickFiler/Controllers/EfcHomeController.cs"
        - "QuickFiler/Controllers/EfcItemController.cs"
        - "QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs"
        - "QuickFiler/Controllers/QfcCollectionController.cs"
        - "QuickFiler/Controllers/QfcCollectionController.PopOut.cs"
        - "QuickFiler/Controllers/QfcItemController.cs"
        - "QuickFiler/Controllers/QfcItemController.ViewerSetup.cs"
        - "QuickFiler/Helper Classes/EfcViewerQueue.cs"
        - "QuickFiler/QuickFiler.csproj"
        - "QuickFiler/Viewers/WebView2BreadcrumbHost.cs"
        - "QuickFiler/Viewers/WebView2EnvironmentContract.cs"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 816
    feature_folder: docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - ".claude/agent-memory/atomic-executor/MEMORY.md"
        - ".claude/agent-memory/atomic-executor/project_caller_supplied_fact_list_can_be_abbreviated_and_look_like_a_plan_defect.md"
        - ".claude/agent-memory/atomic-executor/project_gate_cites_a_baseline_count_the_baseline_task_never_records.md"
        - ".claude/agent-memory/atomic-planner/MEMORY.md"
        - ".claude/agent-memory/atomic-planner/project_816_iscompleted_branch2_ac5_plan_seams.md"
        - ".claude/agent-memory/orchestrator/bash-tool-rejects-complex-commands-in-isolated-worktree.md"
        - ".claude/agent-memory/orchestrator/commit-between-preflight-rounds-so-reviewer-can-diff.md"
        - ".claude/agent-memory/orchestrator/MEMORY.md"
        - ".claude/agent-memory/orchestrator/parallel-marker-blocks-preparation-mode-delegation.md"
        - "docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/ac05-mta-initialize-measurement.md"
        - "docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/spec.md"
        - "UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs"
        - "UtilitiesCS.Test/Threading/UiThread_Tests.cs"
        - "UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs"
        - "UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs"
        - "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
        - "UtilitiesCS/Threading/SyncContextForm.cs"
        - "UtilitiesCS/Threading/UiThread.cs"
        - "UtilitiesCS/UtilitiesCS.csproj"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 838
    feature_folder: docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - "UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs"
        - "UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncFailureContractTests.cs"
        - "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
        - "UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs"
        - "UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.Failures.cs"
        - "UtilitiesCS/UtilitiesCS.csproj"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 839
    feature_folder: docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - "coverage/839-STAGE.cobertura.xml"
        - "QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs"
        - "QuickFiler.Test/Controllers/QfcHomeControllerTests.cs"
        - "QuickFiler/Controllers/QfcHomeController.cs"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 869
    feature_folder: docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - ".claude/state/powershell-batch-budget.default.json"
        - ".github/workflows/_mstest-coverage.yml"
        - ".github/workflows/_pester.yml"
        - ".github/workflows/ci.yml"
        - ".github/workflows/README.md"
        - "coverage/coverage.cobertura.xml"
        - "coverage/pester-coverage.xml"
        - "docs/features/potential/2026-09-11-ci-coverage-threshold-and-pester-gates.md"
        - "docs/features/potential/promoted/2026-09-11-ci-coverage-threshold-and-pester-gates.md"
        - "scripts/dev-tools/run-actionlint.ps1"
        - "scripts/vscode/Invoke-MSTestWithCoverage.ps1"
        - "scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1"
        - "scripts/vscode/Invoke-Restore.ps1"
        - "scripts/vscode/Invoke-VSBuild.ps1"
        - "tests/scripts/vscode/fixtures/sync-package-references/SyncFixture.Test.csproj"
        - "tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1"
        - "tests/scripts/vscode/Invoke-Restore.Tests.ps1"
        - "tests/scripts/vscode/Invoke-VSBuild.Tests.ps1"
        - "tests/scripts/vscode/TestProcessCleanup.Tests.ps1"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 870
    feature_folder: docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - "CLAUDE.md"
        - "docs/features/potential/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md"
        - "docs/features/potential/promoted/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 871
    feature_folder: docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - "docs/features/potential/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams.md"
        - "docs/features/potential/2026-09-12-qfcqueue-enqueueasync-jobsrunning-counter-leak.md"
        - "docs/features/potential/promoted/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams.md"
        - "QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs"
        - "QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs"
        - "QuickFiler.Test/QuickFiler.Test.csproj"
        - "QuickFiler/Controllers/QfcQueue.cs"
        - "QuickFiler/Controllers/QfcQueue.Enqueue.cs"
        - "QuickFiler/Controllers/QfcQueue.Tlp.cs"
        - "QuickFiler/Controllers/QfcQueue.UiIdle.cs"
        - "QuickFiler/Interfaces/IUiIdleDispatcher.cs"
        - "QuickFiler/QuickFiler.csproj"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 872
    feature_folder: docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - "docs/features/potential/2026-09-11-minor-audit-trio-gate-log-assertion-cts-disposal-dormant-tracker.md"
        - "docs/features/potential/promoted/2026-09-11-minor-audit-trio-gate-log-assertion-cts-disposal-dormant-tracker.md"
        - "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs"
        - "TestResults/coverage/coverage-baseline.cobertura.xml"
        - "TestResults/coverage/coverage-postchange.cobertura.xml"
        - "UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs"
        - "UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs"
        - "UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs"
        - "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
        - "UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs"
        - "UtilitiesCS/Threading/ProgressPackage.cs"
        - "UtilitiesCS/Threading/ProgressTrackerAsync.cs"
        - "UtilitiesCS/UtilitiesCS.csproj"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
  - issue_num: 873
    feature_folder: docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - ".claude/agent-memory/_shared_no_absolute_host_paths.md"
        - ".claude/agent-memory/epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md"
        - ".claude/agent-memory/feature-review/project_464-review-residuals.md"
        - ".claude/agent-memory/feature-review/project_488-review-residuals.md"
        - ".claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md"
        - ".claude/agent-memory/orchestrator/collect-pr-context-lands-in-main-checkout.md"
        - ".vscode/settings.json"
        - "CLAUDE.md"
        - "docs/features/potential/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling.md"
        - "docs/features/potential/promoted/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling.md"
        - "scripts/vscode/Invoke-MSTest.ps1"
        - "scripts/vscode/Invoke-MSTest.TrxSummary.ps1"
        - "scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1"
        - "scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1"
        - "scripts/vscode/Invoke-MSTestWithCoverage.ps1"
        - "TaskMaster/TaskMaster.csproj"
        - "tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1"
        - "tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-13T04:01:56Z"
expected_conflict_components:
  - name: quickfiler-item-controllers
    members:
      - 742
      - 743
      - 792
  - name: coverage-tooling
    members:
      - 869
      - 873
---

# Parallel Run: bugs-2026-09-11

Thirteen thematically unrelated TaskMaster defects, prepared to preflight clearance and scheduled by
computed blast-radius contention. There is no integration branch: each item opens its own pull
request against `main`.

Planning state: `artifacts/orchestration/parallel-planner-state.json`.
Plan-home branch: `parallel/bugs-2026-09-11-plan`. Base: `2405a829d`.

## Reading the blast radii

Every radius below is the `declared` radius, derived from the item's approved plan and spec text and
then corrected by hand where the extractor could not see a genuine write. The corrections are
recorded per item in the planner checkpoint under `radius_hand_appended`, with a measured reason.
Three correction classes occurred in this run and each one, left uncorrected, would have
co-scheduled two items onto the same file:

- paths under `scripts/vscode/` and `.claude/agent-memory/`, removed by the `mandate_reads`
  exclusion, which is right for a citation and wrong for a commit;
- the separator-free root token `CLAUDE.md`, which derivation admits only as an exact member of the
  configured shared-surface list;
- paths whose directory segment contains a space, which the whitespace-free token extractor splits
  into fragments naming no tracked file.

## Resolved scheduling limitation — item 602 withdrawn

Item 602 conflicted with all twelve other items, so Welsh-Powell coloured it first and it occupied
generation-0 cohort 0 alone, executing FIRST. It was withdrawn on 2026-09-13 before execution
started, and the unstarted subgraph was recoloured at `recolor_generation: 1` into three cohorts:

| cohort | items |
| --- | --- |
| 0 | 583, 743, 838, 839, 871, 872, 873 |
| 1 | 742, 816, 869, 870 |
| 2 | 792 |

Because 602 occupied a cohort alone either way, the withdrawal costs no parallelism; it removes a
serialized cohort and leaves the other twelve items partitioned exactly as before, shifted down one
index. 602's twelve conflict edges are retained in the planner checkpoint as derived record and are
inert, since a neighbour holding no current-generation cohort constrains nothing.

### Why 602 must not run first

Six of 602's fifteen spec criteria — AC1, AC2, AC3, AC4, AC5, and AC7's reconciliation premise —
are repository-wide, present-tense assertions over the tracked tree, of the form "lists no tracked
file". They are unbounded by 602's own diff. A sibling that merges after 602 and adds a file
carrying an identifier therefore *falsifies* those criteria on `main` rather than merely dating
them, leaving 602 checked off against a condition that no longer holds.

That is already realized rather than hypothetical. Measured against base `2405a829d`, four of the
twelve sibling preparation branches already add files carrying the account identifier: 742, 871 and
743 in the user-profile-path form, breaking AC1, and 873 as a bare account name inside a
`SearchPatterns` regex, breaking AC2. All four land inside 602's own declared globs
`docs/features/active/**/*.md` and `docs/features/**/evidence/**`, and execution will add more.
602's spec concedes the mechanism in its Known Contention section: "because the criterion is
repository-wide a partial correction leaves it unmet. Whichever item lands second must re-measure
rather than assume."

Raw-evidence-class exposure — `.trx`, `evidence/**/*.xml`, `*.txt`, `*.process-tree.json` — measured
zero across all twelve preparation branches, so AC5 and AC7 exposure is latent and depends on what
execution commits. Re-measure rather than assume.

Withdrawal also resolves the trx-filename leak in the kickoff's Open Decisions: that note recorded
that relying on 602 to sweep the leak afterwards did not work *because* 602 ran first. Re-run 602 as
a follow-on run once these twelve items merge.

### Two corrections to the record

**The stated dependency is not the real one.** The ordering requirement was stated as "602's AC4
depends on the results-directory and log-file-name behavior item 873 delivers". That premise is not
supported by 602's own documents: its `spec.md` AC4 is an 8.3 short-name residual-search criterion
naming neither symbol, its plan references neither symbol nor item 873, and both its `issue.md` and
`spec.md` place that half explicitly out of scope as the sibling's work. Item 873 is indeed the
writer of that behavior, but 602 is not a consumer of it. The same unsupported claim appears in the
planner checkpoint under `unexpressible_ordering` and `ordering_assumption_refuted` and in kickoff
Open Decision 1; it originated in the intake handoff and propagated unchecked. The distinction
matters operationally: the stated reason would justify withdrawing 602 only from a run containing
873, whereas the real reason scales with the number of document-adding siblings and applies to all
twelve.

**602's Ordering Risk section does not mean what it appears to mean.** Its AC13 and its Ordering
Risk prose assert self-consistency "in either landing order", but both rest on a single axis — that
no criterion asserts anything about the test runner's argument list. They do not defend the
tree-wide residual searches in AC1 through AC5, which are exactly the criteria a later merge breaks.
Reading AC13 as general order-independence would have wrongly cleared 602 to run first.

See the planner checkpoint under `items[].withdrawal` and `planner_notes.item_602_withdrawn`.