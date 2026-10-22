# P8-T3 Fan-In Gate

Timestamp: 2026-10-06T17-55
Command: git merge-base --is-ancestor f8ea1b5dcc6514bc0088bc80965c188bfd717557 HEAD; git merge-base --is-ancestor 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD; git merge-base --is-ancestor HEAD f8ea1b5dcc6514bc0088bc80965c188bfd717557; git merge-base HEAD origin/main (each one git -C <worktree> invocation); then CMD-FANIN with ORIGIN-MAIN-SHA f8ea1b5dcc6514bc0088bc80965c188bfd717557 (pwsh -NoProfile -Command, first statement Set-Location to the worktree)
EXIT_CODE: 0 (scoped to the CMD-FANIN payload, its process exit code)
ITERATION: 1
Output Summary: ancestry holds in both required directions and the reversed control is 1; the new merge base with main is ORIGIN-MAIN-SHA; the branch adds to main only its own paths (FANIN-OUTSIDE 0); the two planned deletions are the only D rows; both paths changed on both sides show additions only relative to ORIGIN-MAIN-SHA (SHARED-WITH-LOSS 0); every negative control fires (MAIN-ONLY-PATHS 280, MAIN-ONLY-HAS-QFT-CSPROJ True, CONTROL-OUTSIDE-IF-UNFILTERED 277, LOSS-CHECK-CONTROL True); MAIN-TOUCHED-WRITE-SET-CODE is empty; porcelain empty before this artifact was written.

ANCESTRY-MAIN-EXIT: 0
ANCESTRY-BASE-EXIT: 0
ANCESTRY-CONTROL-EXIT: 1
MERGE-BASE-AFTER: f8ea1b5dcc6514bc0088bc80965c188bfd717557

## CMD-FANIN printed lines

```
FANIN-ROWS: 158
FANIN M	.claude/agent-memory/atomic-executor/MEMORY.md
FANIN M	.claude/agent-memory/atomic-executor/project_doubled_backslash_dedoubles_bash_to_native_exe.md
FANIN A	.claude/agent-memory/atomic-executor/project_fluentassertions_truncates_string_diff_breaks_message_substring_gates.md
FANIN A	.claude/agent-memory/atomic-executor/project_preflight_cited_tree_gate_and_span_enumeration.md
FANIN A	.claude/agent-memory/atomic-executor/project_schema_field_contains_check_matches_prefixed_labels.md
FANIN A	.claude/agent-memory/atomic-executor/project_worktree_removal_hook_matches_git_plus_remove_in_variable_names.md
FANIN M	.claude/agent-memory/atomic-planner/MEMORY.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r0_resume_append_point_and_nonexempt_hash_seams.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r10_append_phases_review_residuals_and_pr_time_merge_seams.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r13_two_sided_path_census_and_recorded_not_gated_rate_comparison_seams.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r1_census_initializer_displayname_and_worktree_claude_filter_seams.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r2_pre_existing_evidence_ordering_and_no_git_channel_seams.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r3_checkoff_evidence_precedes_edit_and_line_anchored_field_check_seams.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r4_shared_anchor_controls_native_exit_labels_and_schema_rows_first_seams.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r5_bash_dedoubles_backslashes_in_pwsh_command_payloads.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r6_cs1769_embedded_interop_generic_seam_and_per_task_commit_gates.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r7_ac_by_reference_spec_correction_and_delta_backtick_seams.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r8_fluentassertions_string_diff_truncation_message_gate_seam.md
FANIN A	.claude/agent-memory/atomic-planner/project_959_r9_mid_phase_insertion_renumber_and_iteration2_rerun_seams.md
FANIN M	.claude/agent-memory/feature-review/MEMORY.md
FANIN A	.claude/agent-memory/feature-review/project_959-review-residuals.md
FANIN M	.claude/agent-memory/orchestrator/MEMORY.md
FANIN A	.claude/agent-memory/orchestrator/epic-worktree-removal-hook-fires-on-plain-file-delete.md
FANIN M	.claude/agent-memory/prd-feature/MEMORY.md
FANIN M	.claude/agent-memory/prd-feature/feedback_ac_gates_verify_satisfiability.md
FANIN A	.claude/agent-memory/prd-feature/feedback_issue_md_scope_conflicts_with_orchestrator_decisions.md
FANIN M	.claude/agent-memory/task-researcher/MEMORY.md
FANIN A	.claude/agent-memory/task-researcher/project_sortemail_966_consolidation_959.md
FANIN A	.claude/agent-memory/task-researcher/project_sortemail_latent_defects_959.md
FANIN M	.claude/agent-memory/task-researcher/project_sortemail_split_and_prompt_seam_956.md
FANIN A	QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs
FANIN M	QuickFiler.Test/QuickFiler.Test.csproj
FANIN M	QuickFiler/Controllers/EfcDataModel.cs
FANIN D	ToDoModel/Email Utilities/SortItemsToExistingFolder.cs
FANIN A	UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs
FANIN A	UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs
FANIN M	UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs
FANIN M	UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs
FANIN A	UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs
FANIN M	UtilitiesCS.Test/UtilitiesCS.Test.csproj
FANIN M	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs
FANIN D	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs
FANIN M	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs
FANIN M	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs
FANIN M	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs
FANIN M	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs
FANIN M	UtilitiesCS/UtilitiesCS.csproj
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/code-review.2026-10-06T15-30.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/code-review.2026-10-06T17-40.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/coverage-baseline.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/p0-t12-pre-edit-census.2026-10-03T08-35.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/p0-t2-mode-preconditions.2026-10-03T08-24.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/p0-t3-worktree-context.2026-10-03T08-24.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/p0-t4-channel-and-toolchain.2026-10-03T08-26.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/p0-t5-nuget-restore.2026-10-03T08-26.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/p0-t6-csharpier-check.2026-10-03T08-27.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/p0-t7-msbuild-analyzers.2026-10-03T08-27.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/p0-t8-msbuild-nullable.2026-10-03T08-28.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/p0-t9-stall-probe.2026-10-03T08-29.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/phase0-instructions-read.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/test-run-baseline.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/other/preflight-clearance.2026-10-03T00-28.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-comparison.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-post-change.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/negative-controls.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p1-t1-savecase-tests-census.2026-10-03T08-37.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p1-t10-format-and-build.2026-10-03T08-42.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p1-t2-attachmentsaving-tests-census.2026-10-03T08-37.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p1-t3-test-csproj.2026-10-03T08-38.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p1-t4-scoped-format.2026-10-03T08-38.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p1-t5-test-build.2026-10-03T08-39.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p1-t8-l1-census.2026-10-03T08-41.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p1-t9-l3-census.2026-10-03T08-41.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p2-t1-undo-tests-census.2026-10-03T08-44.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p2-t2-test-csproj.2026-10-03T08-44.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p2-t3-undo-seam-census.2026-10-03T08-45.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p2-t4-scoped-format.2026-10-03T08-46.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p2-t5-test-build.2026-10-03T08-46.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p2-t7-undo-final-census.2026-10-03T08-48.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p2-t8-tst1-census.2026-10-03T08-48.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p2-t9-format-and-build.2026-10-03T08-49.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p3-t1-trysave-tests-census.2026-10-03T08-51.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p3-t2-format-and-build.2026-10-03T08-52.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p3-t4-trysave-census.2026-10-03T08-54.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p3-t5-format-and-build.2026-10-03T08-55.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t1-savecase-final-census.2026-10-03T08-57.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t10-rr-fix.2026-10-03T12-11.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t2-attachmentsaving-final-census.2026-10-03T08-58.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t4-attachmentsaving-extract-census.2026-10-03T09-00.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t5-legacy-deletion.2026-10-03T09-01.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t5-legacy-deletion.2026-10-03T10-17.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t6-tst1-rows-census.2026-10-03T10-19.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t7-format-and-build.2026-10-03T10-21.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t7-format-and-build.2026-10-03T11-25.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t1-sortemail-usings.2026-10-03T12-18.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t10-todomodel-deletion.2026-10-03T12-30.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t11-cr1-spec956.2026-10-03T12-32.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t2-mailitemsort-usings.2026-10-03T12-18.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t3-usings-build.2026-10-03T12-20.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t4-efc-tests-census.2026-10-03T12-21.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t5-qft-csproj.2026-10-03T12-22.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t6-efc-seam.2026-10-03T12-23.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t8-efc-finally.2026-10-03T12-25.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t1-csharpier-format.2026-10-03T12-34.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t10-post-format-census.2026-10-03T12-52.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t12-scope-boundary.2026-10-03T12-55.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t12-scope-boundary.2026-10-06T13-23.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t13-identity-and-sweep.2026-10-03T12-56.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t13-tst1-doc-comment.2026-10-06T13-20.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t14-doc-comment-format-and-census.2026-10-06T13-22.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t16-identity-and-sweep.2026-10-06T15-11.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t2-csharpier-check.2026-10-03T12-35.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t24-ac6-deferred.2026-10-06T15-15.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t3-msbuild-analyzers.2026-10-03T12-36.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t4-msbuild-nullable.2026-10-03T12-37.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t45-ac27-deferred.2026-10-06T15-22.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t46-ac-inventory.2026-10-06T15-23.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t1-pre-edit-observations.2026-10-06T17-06.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t15-scope-boundary.2026-10-06T17-23.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t16-phase7-closure.2026-10-06T17-25.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t2-tas-ss4-edit.2026-10-06T17-07.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t3-tsc-using-edit.2026-10-06T17-07.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t4-scoped-format-and-census.2026-10-06T17-09.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t6-csharpier-format.2026-10-06T17-11.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t7-csharpier-check.2026-10-06T17-12.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t8-msbuild-analyzers.2026-10-06T17-13.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t9-msbuild-nullable.2026-10-06T17-14.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p8-t1-premerge-facts.2026-10-06T17-52.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p8-t2-merge-record.2026-10-06T17-53.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/pr-description-inputs.2026-10-06T15-12.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/toolchain-final-pass.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/compile-red-attachment-saving-seams.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-cleanup-files-phase-one.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-efc-filer-cleanup.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-exception.2026-10-03T12-33.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-redirect-save-folder.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-save-case.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-try-save-retry.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-write-csv.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p2-t11-tst1-run.2026-10-03T08-50.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p4-t12-sortemail-family-run.2026-10-03T12-13.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p4-t13-control-backup.2026-10-03T12-14.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p4-t14-control-applied.2026-10-03T12-15.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p4-t15-control-restored.2026-10-03T12-17.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p4-t8-savecase-and-tst1-runs.2026-10-03T11-27.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p7-t5-attsave-run.2026-10-06T17-10.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/pass-after-regression-tests.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/feature-audit.2026-10-06T15-30.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/feature-audit.2026-10-06T17-40.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/issue.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/plan.2026-10-02T05-07.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/policy-audit.2026-10-06T15-30.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/policy-audit.2026-10-06T17-40.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/research/2026-10-02T05-15-sort-email-latent-logic-defects-research.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/research/2026-10-02T05-50-sort-email-966-consolidation-research.md
FANIN A	docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/spec.md
FANIN M	docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md
FANIN A	docs/features/potential/promoted/2026-10-01-sort-email-latent-logic-defects.md
FANIN-OUTSIDE: 0
FANIN-DELETED: 2
FANIN-WRITE-SET-MISSING: 0
NUMSTAT-QFT-VS-MAIN: 1	0	QuickFiler.Test/QuickFiler.Test.csproj
NUMSTAT-UCT-VS-MAIN: 3	0	UtilitiesCS.Test/UtilitiesCS.Test.csproj
NUMSTAT-UCS-VS-MAIN: 0	1	UtilitiesCS/UtilitiesCS.csproj
MAIN-ONLY-PATHS: 280
MAIN-ONLY-HAS-QFT-CSPROJ: True
MAIN-TOUCHED-WRITE-SET: QuickFiler.Test/QuickFiler.Test.csproj
MAIN-TOUCHED-WRITE-SET-CODE: 
CONTROL-OUTSIDE-IF-UNFILTERED: 277
SHARED-PATHS: 2
SHARED-NUMSTAT 1	0	.claude/agent-memory/orchestrator/MEMORY.md
SHARED-NUMSTAT 1	0	QuickFiler.Test/QuickFiler.Test.csproj
SHARED-WITH-LOSS: 0
LOSS-CHECK-CONTROL: True
PORCELAIN-COUNT: 0
```

## Acceptance

1. ANCESTRY-MAIN-EXIT: 0 and ANCESTRY-BASE-EXIT: 0: PASS
2. ANCESTRY-CONTROL-EXIT: 1: PASS
3. MERGE-BASE-AFTER equals ORIGIN-MAIN-SHA: PASS
4. FANIN-OUTSIDE: 0: PASS
5. FANIN-WRITE-SET-MISSING: 0 and FANIN-DELETED: 2: PASS
6. NUMSTAT-QFT-VS-MAIN `1 0`, NUMSTAT-UCT-VS-MAIN `3 0`, NUMSTAT-UCS-VS-MAIN `0 1`: PASS
7. MAIN-ONLY-PATHS 280 above 0, MAIN-ONLY-HAS-QFT-CSPROJ True, CONTROL-OUTSIDE-IF-UNFILTERED 277 above 0: PASS
8. SHARED-PATHS 2 (the two predicted rows), SHARED-WITH-LOSS: 0, LOSS-CHECK-CONTROL: True: PASS
9. MAIN-TOUCHED-WRITE-SET-CODE recorded (empty) and PORCELAIN-COUNT: 0: PASS
