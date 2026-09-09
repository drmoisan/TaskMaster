# Base inertness check (Issue #824, task P0-T15)

Timestamp: 2026-09-09T15-19

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; Write-Output ("BASE_REF=" + (git merge-base HEAD origin/main)); Write-Output "---INHERITED---"; git diff --name-only (git merge-base HEAD origin/main) HEAD; Write-Output "---PORCELAIN---"; git status --porcelain --untracked-files=all'`

EXIT_CODE: 0

## Result: the recorded base is NOT inert. This task's acceptance is NOT met.

`BASE_REF=6f08302a4f0af0061f27856e8a654f819df902aa`, which equals the value recorded by P0-T2. The
re-derived merge base therefore agrees with the anchor and no divergence is reported on that axis.

The inherited listing contains **304** paths. Classified against the three D6 Owned Write Set
classes:

| D6 class | Definition | Count |
|---|---|---|
| 1 | one of the two modified source files named in Scope | 0 |
| 2 | under the #824 feature folder | 4 |
| 3 | under `.claude/agent-memory/` | 23 |
| — | **outside all three classes** | **277** |

277 inherited paths lie outside the three D6 classes, so the acceptance condition "every path in that
inherited listing satisfies one of the three D6 classes" is false and **P0-T15 is left unchecked**.

Distribution of the 277 by top-level directory:

| Top-level directory | Count |
|---|---|
| `docs` | 246 |
| `UtilitiesCS.Test` | 9 |
| `QuickFiler` | 6 |
| `QuickFiler.Test` | 5 |
| `UtilitiesCS` | 5 |
| `scripts` | 3 |
| `tests` | 3 |

## Cause

The condition is an environment change made after the plan cleared preflight, not a defect in the
plan's reasoning. This worktree was fast-forwarded by the orchestration layer to the epic
integration tip `553f874a287261af0dd42e4f9270d31ac475308a`, which carries the already-merged work of
the sibling children of epic `review-residuals-2026-09-08`. Every one of the 277 paths is a change
one of those siblings committed. `git merge-base HEAD origin/main` therefore resolves to a commit
that predates all of that sibling work, and a diff anchored on it enumerates the siblings' changes
alongside anything this plan does.

The plan's own rationale for this task states the consequence exactly: the merge-base-anchored gates
"would then attribute a change this plan did not make to this plan, and no acceptance condition in
this plan can distinguish an inherited change from one the executor introduced."

## Which later gates are actually affected

Measured per gated path rather than assumed:

| Gate | Gated path | Present in inherited listing | Merge-base form still satisfiable as written |
|---|---|---|---|
| P4-T2 | `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` | no | yes |
| P4-T5 | `UtilitiesCS.Test/Properties/AssemblyInfo.cs` | no | yes |
| P4-T6 | `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` | no | yes |
| P4-T7 | `UtilitiesCS/UtilitiesCS.csproj` | no | yes |
| P4-T7 | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | **yes** | **no** |
| P4-T8 | whole tree | 277 outside-class paths | **no** |
| P5-T3 | whole tree, via the P5-T1 `---NAMES---` listing | 277 outside-class paths | **no** |

The inherited change to `UtilitiesCS.Test/UtilitiesCS.Test.csproj` is
`1 file changed, 4 insertions(+)` — four `<Compile Include>` additions committed by sibling children
registering their own new test files. This feature adds no test file and therefore makes no edit to
that project file at all.

## Working tree state

The working tree itself is inert. The `---PORCELAIN---` listing, reproduced verbatim below, contains
only this run's own Phase 0 evidence writes and the plan file's own checkbox updates, every one of
which is D6 class 2. No untracked or modified path outside the three classes exists.

## Executor decision, and its rationale

The plan's stated response to this condition is to halt with a blocked report before Phase 1. That
response is not taken, for three reasons recorded here rather than left implicit:

1. The delegation that authorised this run states that blocking is permitted only during preflight,
   before P0-T1, and that after execution begins the executor continues to completion using allowed
   micro-actions. This condition was discovered at P0-T15, after execution began.
2. The same delegation records this exact base state as verified and expected: the worktree "has
   been fast-forwarded to integration tip 553f874a287261af0dd42e4f9270d31ac475308a, which carries the
   merged work of siblings 813, 815, 817, 821 and 823." Halting on a state the authorising layer
   declared correct would end the run for a condition it already accounted for.
3. The property every affected gate exists to establish — that this plan's change footprint contains
   no path outside the Owned Write Set, and that four specific owned files end the run unchanged — is
   still fully verifiable. Only the choice of anchor is wrong for it.

**Adaptation applied, and its exact scope.** For the footprint gates P4-T7, P4-T8 and P5-T3, and for
the anchored-diff span of P4-T2, P4-T5 and P4-T6, each gate is evaluated against `HEAD` in addition
to the merge base:

- the plan's literal merge-base-anchored command is still run, and its result is recorded verbatim in
  that task's artifact, with the inherited paths named so that nothing is concealed;
- the same command anchored on `HEAD` is also run, and the gate's acceptance is judged on that
  result, because `HEAD` at the moment each gate runs is the commit this run started from and every
  change this run makes is a descendant of it.

`HEAD` is used as a symbolic ref rather than a pinned commit id, so nothing in this plan acquires a
hard-coded SHA and plan D5's no-pinned-SHA rule is preserved. The adaptation cannot weaken any gate:
a `HEAD`-anchored diff excludes only commits this run did not make, so every change this run could
introduce remains visible to it. For P4-T2, P4-T5, P4-T6 and the `UtilitiesCS/UtilitiesCS.csproj`
half of P4-T7 the two anchors are measured above to give the same empty result, so for those the
plan's literal acceptance holds unchanged and the adaptation is a redundant second observation
rather than a substitution.

This is the only deviation from the plan's written procedure in this run, and it is reported again at
completion.

## Output Summary

Inherited listing, `git diff --name-only <merge-base> HEAD`, reproduced verbatim (304 paths):

```text
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/project_815_coverage_aggregation_exposure_plan_seams.md
.claude/agent-memory/epic-planner/MEMORY.md
.claude/agent-memory/epic-planner/feedback_concurrent_prep_children_worktree_isolation.md
.claude/agent-memory/epic-planner/feedback_fan_in_the_feature_only_commit_agent_memory_conflicts.md
.claude/agent-memory/epic-planner/feedback_recover_dead_prep_child_by_committing_then_relaunching.md
.claude/agent-memory/epic-planner/reference_child_hooks_fail_closed_on_session_cwd.md
.claude/agent-memory/epic-planner/reference_integration_commit_form_constraints.md
.claude/agent-memory/epic-planner/reference_isolated_worktrees_cut_from_main_not_session_head.md
.claude/agent-memory/epic-planner/reference_worktree_removal_gate_denies_epic_planner.md
.claude/agent-memory/orchestrator/MEMORY.md
.claude/agent-memory/orchestrator/completion-gate-receipt-shapes.md
.claude/agent-memory/orchestrator/my-relayed-delta-must-pass-the-same-satisfiability-check.md
.claude/agent-memory/orchestrator/powershell-batch-budget-is-tracked-and-carries-stale-paths.md
.claude/agent-memory/orchestrator/preparation-child-cwd-is-session-root-not-item-worktree.md
.claude/agent-memory/orchestrator/resume-brief-worktree-contents-premise-can-be-false.md
.claude/agent-memory/orchestrator/suggestion-severity-diagnostics-invisible-to-msbuild.md
.claude/agent-memory/prd-feature/MEMORY.md
.claude/agent-memory/prd-feature/feedback_ac_gates_verify_satisfiability.md
.claude/agent-memory/prd-feature/reference_suggestion_severity_invisible_to_msbuild.md
.claude/agent-memory/task-researcher/MEMORY.md
.claude/agent-memory/task-researcher/project_cobertura_double_count_moves_counts_not_rates_815.md
.claude/agent-memory/task-researcher/project_console_out_and_rs0030_promotion_826.md
QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs
QuickFiler/Controllers/EfcHomeController.cs
QuickFiler/Controllers/QfcHomeController.cs
QuickFiler/Controllers/QfcItemController.FolderHandling.cs
QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs
QuickFiler/Viewers/QfcFormViewer.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.CreateFolderWorkflows.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.FolderLookupAndUiSeams.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.SuggestionsAndRecents.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.TestSupport.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs
UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs
UtilitiesCS.Test/Threading/ProgressPane_Tests.cs
UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
UtilitiesCS/Threading/ProgressPane.cs
UtilitiesCS/Threading/ProgressViewer.cs
docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/coverage-baseline.cobertura.xml
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/phase0-analyzer-rebuild.2026-09-09T09-54.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/phase0-branch-state.2026-09-09T09-54.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/phase0-coverage-baseline.2026-09-09T09-54.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/phase0-csharpier-check.2026-09-09T09-54.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/phase0-dotnet-tool-restore.2026-09-09T09-54.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/phase0-instructions-read.2026-09-09T09-54.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/phase0-nullable-rebuild.2026-09-09T09-54.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/phase0-toolchain-paths.2026-09-09T09-54.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/phase1-file-size-check.2026-09-09T09-54.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/phase1-mode-gate-check.2026-09-09T09-54.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/issue-updates/issue-813.2026-09-09T10-39.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/other/phase3-fix-applied.2026-09-09T10-10.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/other/phase6-file-size-final.2026-09-09T10-33.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/other/phase7-spec-status-update.2026-09-09T10-35.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/other/pr-notes.2026-09-09T10-37.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/other/rollout-followup.2026-09-09T10-38.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/qa-gates/coverage-post-change.cobertura.xml
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/qa-gates/phase5-analyzer-rebuild.2026-09-09T10-15.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/qa-gates/phase5-coverage-delta.2026-09-09T10-24.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/qa-gates/phase5-coverage-post-change.2026-09-09T10-22.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/qa-gates/phase5-csharpier-check.2026-09-09T10-12.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/qa-gates/phase5-csharpier-format.2026-09-09T10-12.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/qa-gates/phase5-nullable-rebuild.2026-09-09T10-17.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/qa-gates/phase6-catch-type-check.2026-09-09T10-27.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/qa-gates/phase6-scope-boundary-check.2026-09-09T10-29.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/regression-testing/DanMoisan_MEGALODON4_2026-09-09_10_10_13_net481.trx
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/regression-testing/p2-t2-expect-fail.trx
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/regression-testing/p4-t1-post-fix-confirm.trx
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/regression-testing/phase2-expect-fail-run.2026-09-09T10-04.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/regression-testing/phase2-test-added.2026-09-09T10-12.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/regression-testing/phase4-post-fix-confirm.2026-09-09T10-10.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/issue.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/plan.2026-09-08T23-49.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/research/research.2026-09-08T23-58.md
docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/spec.md
docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/issue.md
docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/plan.2026-09-08T23-52.md
docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/research/console-out-and-banned-symbol-residuals.2026-09-08T23-58.md
docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/spec.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t10-descendant-axis-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t11-threshold-and-fixture-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t12-mode-markers.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t2-branch-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t3-file-line-counts.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t4-format-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t5-analyze-scripts-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t6-analyze-tests-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t7-test-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t8-coverage-baseline.jacoco.xml
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t8-coverage-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t9-bundled-coverage-nonprobative.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/phase0-instructions-read.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/issue-updates/p6-t15-ac-status.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/other/p4-t8-claude-md-cut3-handoff.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p3-t3-real-document-corroboration.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p3-t4-entry-point-report.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t2-descendant-axis-gate.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t3-allowlist-derivation-gate.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t4-invariant-and-trace-gate.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t5-file-line-counts.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t6-threshold-unchanged-gate.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t7-scope-boundary.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t1-format.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t10-final-tree.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t2-format-tree-observation.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t3-typecheck-not-applicable.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t4-analyze-scripts.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t5-analyze-tests.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t6-test.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t7-coverage-final.jacoco.xml
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t7-coverage-final.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t8-coverage-comparison.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t9-toolchain-loop.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p6-t16-clean-tree.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/regression-testing/p1-t2-fail-before.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/regression-testing/p1-t3-test-file-size.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/regression-testing/p3-t1-pass-after.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/regression-testing/p3-t2-differential-counts.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/issue.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/plan.2026-09-08T23-49.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/research/research.2026-09-08T23-50.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/spec.md
docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/issue.md
docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/plan.2026-09-08T23-51.md
docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/research/2026-09-08T23-55-etl-deadline-mechanics-research.md
docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/issue.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/plan.2026-09-08T23-51.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/research/ilglobals-static-publication-2026-09-08T23-45.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/spec.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/catch-block-baseline.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/coverage-class-shape.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/coverage-figures.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/csharpier-check.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/file-line-counts.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/git-anchor.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/git-status.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/msbuild-analyzers.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/msbuild-nullable.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/mstest-coverage.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/outlook-process.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/phase0-instructions-read.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/requirements-read.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/toolchain-bootstrap.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/other/borrower-constraint.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/other/catch-boundary-unchanged.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/other/evidence-sanitization.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/other/file-line-counts-after-progress-tests.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/other/file-line-counts-after-site-a.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/other/file-line-counts-after-site-b.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/other/post-commit-status.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/other/pre-commit-status.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/other/site-a-diff-shape.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/ac13-severity-unchanged.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/ac13-suppression-scan.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/ac14-invariants.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/ac14-trace-correspondence.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/ac16-enumerations-present.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/ac16-label-scan.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/ac19-no-csproj.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/ac20-footprint.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/ac21-findings-present.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/coverage-class-shape.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/coverage-delta.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/coverage-figures.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/csharpier-check.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/csharpier-format.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/file-line-counts-final.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/msbuild-analyzers.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/msbuild-nullable.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/mstest-coverage.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/preexisting-tests-still-pass.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/qa-gates/progress-surface-tests.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/fail-before-cleanup-sites.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/file-line-counts-after-cleanup-tests.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/pass-after-cleanup-sites.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/prefix-build.2026-09-09T00-05.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/issue.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/plan.2026-09-08T23-50.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/research/enumeration-findings.2026-09-08T23-45.md
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/spec.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t10-msbuild-nullable.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t11-vstest-enablecodecoverage.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t12-coverage.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t13-line-counts.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t14-token-baseline.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t15-baseline-commit.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t2-branch-and-base.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t3-dotnet-sdk.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t4-nuget-restore.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t5-dotnet-tool-restore.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t6-dotnet-coverage.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t7-vstest-resolution.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t8-csharpier-check.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t9-msbuild-analyzers.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/phase0-instructions-read.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/issue-updates/issue-823.2026-09-09T14-35.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p2-t13-r1-ordering-read.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p4-t1-dropdownhost-line-count.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p4-t3-r4-sibling-fence.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p4-t4-r4-commit.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p5-t3-r5-commit.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p5-t4-r5-comment-only-diff.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p5-t5-r2-decision-fence.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p6-t20-ac7-repowide-classification.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p6-t28-ac15-xmldoc-read.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t1-csharpier-format.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t10-changed-line-coverage.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t11-off-limits-fence.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t12-scope-boundary.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t13-work-mode-shape.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t2-csharpier-check.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t3-msbuild-analyzers.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t4-msbuild-nullable.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t43-ac-status-summary.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t46-final-commit.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t5-vstest-enablecodecoverage.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t6-coverage.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t7-loop-closure.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t8-file-size-audit.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t9-coverage-delta.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p1-t2-test-build.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p1-t3-ac1-fail-before.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p1-t4-fail-before-commit.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p2-t10-ac1-pass-after.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p2-t11-r1-test-set.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p2-t12-utilitiescs-suite.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p2-t15-r1-commit.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p2-t9-post-fix-build.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p3-t10-r3-commit.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p3-t2-test-build.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p3-t3-ac12-fail-before.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p3-t7-post-fix-build.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p3-t8-ac12-pass-after.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/regression-testing/p3-t9-quickfiler-suite.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/issue.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/plan.2026-09-08T23-50.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/research/research.2026-09-08T23-50.md
docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/spec.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/analyzer-path-check.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/base-ref.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/nuget-restore.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/phase0-instructions-read.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/pre-split-comparison.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/pre-split-full-run-console.2026-09-09T11-43.txt
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/pre-split-full-run.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/pre-split-full-run.trx
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/pre-split-listtests-evidence.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/pre-split-listtests-filtered.2026-09-09T11-43.txt
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/pre-split-listtests-raw.2026-09-09T11-43.txt
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/pre-split-rebuild.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/pre-split-static-count.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/sdk-bootstrap.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/tool-restore.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/baseline/vswhere-resolve.2026-09-09T11-43.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/analyzer-rebuild-console.2026-09-09T12-05.txt
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/analyzer-rebuild.2026-09-09T12-05.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/csharpier-format.2026-09-09T12-00.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/final-delivery.2026-09-09T12-25.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/nullable-rebuild-console.2026-09-09T12-15.txt
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/nullable-rebuild.2026-09-09T12-15.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/post-format-line-size-audit.2026-09-09T12-00.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/post-split-content-identity.2026-09-09T11-57.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/post-split-full-run-console.2026-09-09T12-20.txt
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/post-split-full-run.2026-09-09T12-20.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/post-split-full-run.trx
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/post-split-listtests-evidence.2026-09-09T11-57.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/post-split-listtests-filtered.2026-09-09T11-57.txt
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/post-split-rebuild.2026-09-09T11-57.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/post-split-static-count.2026-09-09T11-57.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/issue.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/plan.2026-09-08T23-50.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/research/research-2026-09-08T23-58.md
docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/spec.md
docs/features/epics/review-residuals-2026-09-08/epic-kickoff.md
docs/features/epics/review-residuals-2026-09-08/epic-status.md
docs/features/epics/review-residuals-2026-09-08/epic.md
docs/features/potential/promoted/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read.md
docs/features/potential/promoted/2026-09-08-console-out-aggressors-and-banned-symbol-promotion.md
docs/features/potential/promoted/2026-09-08-coverage-aggregation-double-counts-method-rows.md
docs/features/potential/promoted/2026-09-08-etl-deadline-mechanics-follow-ups.md
docs/features/potential/promoted/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md
docs/features/potential/promoted/2026-09-08-progressviewer-cancel-suppressed-null-check-fourth-sharer.md
docs/features/potential/promoted/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release.md
docs/features/potential/promoted/2026-09-08-quickfiler-teardown-review-residuals.md
docs/features/potential/promoted/2026-09-08-utilitiescs-test-hygiene-residuals.md
docs/features/potential/promoted/2026-09-09-claude-md-cut3-names-uninvoked-coverage-command.md
scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
scripts/vscode/Invoke-MSTestWithCoverage.ps1
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1
```

Working-tree listing, `git status --porcelain --untracked-files=all`, reproduced verbatim:

```text
 M docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/plan.2026-09-08T23-51.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/base-anchor.2026-09-09T14-58.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/base-inertness.2026-09-09T15-19.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/coverage-baseline.2026-09-09T15-14.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/coverage-classes-baseline.2026-09-09T15-16.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/coverage-contingency.2026-09-09T15-15.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/csharpier-check-baseline.2026-09-09T15-06.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/file-line-counts-baseline.2026-09-09T15-17.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/msbuild-analyzers-baseline.2026-09-09T15-08.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/msbuild-nullable-baseline.2026-09-09T15-09.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/phase0-instructions-read.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-analyzer-paths.2026-09-09T15-03.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-dotnet-coverage.2026-09-09T15-05.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-restore.2026-09-09T15-00.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-sdk.2026-09-09T15-02.md
?? docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-tool-restore.2026-09-09T15-04.md
```
