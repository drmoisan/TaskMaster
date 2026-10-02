---
name: index-artifact-hygiene-and-misc
description: Sub-index of atomic-executor memories on artifact hygiene, evidence sanitisation, shell and pwsh pitfalls, and miscellaneous execution gotchas (moved out of MEMORY.md on 2026-09-28 to keep the index under its load limit)
metadata:
  type: reference
---

# Artifact hygiene and miscellaneous execution gotchas

- [Never embed absolute host paths](../_shared_no_absolute_host_paths.md) · [Never predict an observation](feedback_never_predict_an_observation_into_an_artifact.md)
- [<TS> drifts ahead of write time](project_evidence_timestamp_labels_drift_ahead_of_write_time.md) · [Probe literal trips the NEXT sweep](project_selftest_probe_literal_trips_the_next_sweep_pass.md)
- [TRX sanitisation is case-insensitive](project_trx_sanitisation_must_be_case_insensitive.md) · [TRX/msbuild need a sanitisation micro-action](project_vstest_trx_evidence_needs_sanitisation_task.md)
- [MSBuild logs leak TWO roots](project_msbuild_log_has_two_absolute_path_leak_classes.md) · [Deploy_ leaks tokens on FAILING runs](project_mstest_deploy_dir_leaks_tokens_on_failing_runs.md)
- [vstest leaves TWO .coverage files](project_vstest_emits_two_coverage_files_per_run.md) · [Green run prints no Failed/Skipped line](project_vstest_success_run_prints_no_failed_or_skipped_line.md)
- [PS budget hook blocks scratch .ps1](project_powershell_scratch_script_budget_hook_blocks_helpers.md) · [Plan-mandated .ps1 + frozen porcelain gate](project_plan_mandated_ps1_helpers_collide_with_budget_cap_and_frozen_porcelain_gate.md)
- [2nd pass must not qualify schema fields](project_appending_a_second_pass_must_not_qualify_schema_fields.md) · [Bash resets cwd; use `env -C`](project_bash_cwd_resets_use_env_dash_c.md)
- [global.json cwd-search vs no-cd discipline](project_dotnet_global_json_cwd_search_vs_bash_discipline.md) · [pwsh -File starts in the SESSION root](project_pwsh_file_starts_in_session_root_needs_workingdirectory.md)
- [pwsh stdin is a REPL](project_pwsh_stdin_repl_mode_and_nonascii_mangling.md) · [Isolation guard refuses pwsh from Bash](project_worktree_isolation_guard_refuses_pwsh_from_bash.md)
- [Changed-line branch gate invalidated by the fix](project_changed_line_coverage_branch_gate_invalidated_by_the_fix.md) · [ExpectedExitCode keyed off the baseline](project_expectedexitcode_declared_from_baseline_not_observed_run.md)
- [CSharpier forces a blank line before a comment](project_csharpier_requires_blank_line_before_comment_breaking_numstat_bounds.md) · [ExcludeFromCodeCoverage misses `this`-capturing lambdas](project_excludefromcodecoverage_misses_this_capturing_lambdas.md)
- [Koverage -RepoRoot needs native separators](project_koverage_reporoot_needs_native_separators.md) · [FakeTimeProvider zero due time fires at creation](project_faketimeprovider_zero_duetime_fires_at_creation.md)
- [Reflective property read escapes a member grep](project_reflective_property_read_escapes_member_expression_grep.md) · [Preparation mode flips anchored-diff membership](project_preparation_mode_flips_anchored_diff_gate_membership.md)
- [git grep -c with an empty pattern is the line-count oracle](project_git_grep_c_empty_is_the_allowlisted_line_count_oracle.md) - Read renders a phantom trailing line
- [Contingency fallback orphans downstream paths](project_contingency_fallback_orphans_downstream_hardcoded_paths.md) · [Stale-citation gate literal is per-comment](project_stale_citation_gate_literal_must_match_the_comments_legitimate_citations.md)
- [TimeoutAfter IsCompleted loses to the Task.Run race](project_timeoutafter_iscompleted_shortcircuit_loses_to_taskrun_race.md) · [One Cobertura filename, several class nodes](project_cobertura_filename_maps_to_several_class_nodes.md)
- [msbuild file logger double-counts warnings](project_msbuild_filelogger_double_counts_each_warning.md) · [Reconciliation merge already tracks the feature docs](project_orchestrator_reconciliation_merge_tracks_feature_docs.md)
- [Seam sentences outlive a removed member](project_preflight_seam_sentences_outlive_a_removed_member.md) — after a decision removes an interface member, grep the spec for "Moq double of the .* interface"/"injectable"; keyword sweeps miss them
- [Explicit Compile items decide membership, not file presence](project_explicit_compile_items_decide_membership_not_file_presence.md) — a grep hit can be uncompiled
- [`git add -N -- .` defeats a "do not stage X" invariant](project_intent_to_add_span_defeats_do_not_stage_invariant.md) — it reads as diff plumbing, so a staging audit skips it
- [Hunk-header literal slides past a blank line](project_git_hunk_header_literal_slides_past_blank_line.md) — measured; assert numstat deleted=0, never `@@ -N,0 +M,`
- [Batch-budget hook discards out-of-root .ps1 writes](project_batch_budget_hook_discards_out_of_root_powershell_writes.md) — hook roots at session worktree; plan gates reading its state become unsatisfiable
- [BOM-bearing .cs files + pre-restore numstat gates](project_bom_bearing_cs_files_and_prerestore_numstat_head_gates.md)
- [CLAUDE.md differs per worktree; read the execution copy](project_claude_md_differs_between_worktrees_read_execution_copy.md)
- [CommandLine token kill hits own bash/pwsh shells](project_commandline_match_on_results_dir_token_kills_own_tool_shells.md)
- [CLI Rebuild omits .vsto/.manifest](project_commandline_rebuild_omits_vsto_manifests_addin_cannot_load.md)
- [git show in pwsh decodes ibm437 + keeps BOM](project_conservation_gate_git_show_decodes_ibm437_and_keeps_bom.md)
- [Dot-sourced coverage helpers: StrictMode $LASTEXITCODE throws](project_coverage_helpers_dotsource_strictmode_lastexitcode_throws.md)
- [Exactly-once literal vs pattern-containment clause](project_exactly_once_literal_clause_conflicts_with_pattern_containment_clause.md)
- [Fixed run-path coverage gate blind to sibling test file](project_fixed_run_path_coverage_gate_blind_to_sibling_test_file.md)
- [Inventory clause omits inherited promotion rename](project_footprint_inventory_clause_omits_inherited_promotion_rename.md)
- [Handoff AC count is a claim; measure the AC file](project_handoff_stated_ac_count_contradicts_the_ac_source_file.md)
- [Kill build-lock waiter by PID, not script name](project_killing_a_build_lock_waiter_by_script_name_hits_every_sibling.md)
- [Malformed pwsh payload surfaces as unrelated hook block](project_malformed_pwsh_payload_surfaces_as_unrelated_hook_block.md)
- [Mandatory [string[]] rejects a blank line](project_mandatory_string_array_param_rejects_blank_line_turning_red_into_binding_error.md)
- [Merge-base diff over branch-created file = one whole-file hunk](project_mergebase_diff_over_branch_created_file_is_whole_file_hunk.md)
- [Mid-plan commit breaks deletion staging/porcelain spans](project_midplan_commit_breaks_deletion_staging_and_porcelain_spans.md)
- [Minute-resolution timestamps can't be strictly increasing](project_minute_resolution_timestamp_cannot_be_strictly_increasing.md)
- [/m "N>" prefix zeroes anchored target counts](project_msbuild_parallel_log_node_prefix_defeats_anchored_target_counts.md)
- [Nested Import-Module -Force unloads session-wide](project_nested_import_module_force_unloads_session_wide.md)
- [Parent orchestrator hold-commits your branch mid-run](project_parent_orchestrator_hold_commits_your_branch_midrun.md)
- [Pester TotalCount includes filtered NotRun](project_pester_filtered_total_counts_notrun.md)
- [Auto-property with setter guard is not expressible](project_plan_mandated_autoproperty_with_setter_guard_is_not_expressible.md)
- [PoshQC format != Invoke-Formatter defaults](project_poshqc_format_rewrites_differ_from_invoke_formatter_defaults.md)
- [PoshQC strips BOM/CRLF only on rewrite](project_poshqc_format_strips_bom_and_crlf_only_when_it_rewrites.md)
- [PS double quotes keep both backslashes](project_powershell_double_quoted_backslash_defeats_msbuild_nonvacuity_grep.md)
- [4 PSScriptAnalyzer traps in new modules](project_psscriptanalyzer_traps_in_new_powershell_modules.md)
- [Param named $args makes msbuild gate vacuous](project_pwsh_function_param_named_args_makes_msbuild_gate_vacuous.md)
- [Nested quotes in "$( )" fail to parse](project_pwsh_nested_quotes_in_subexpression_fail_to_parse.md)
- [$Log/$log case collision flattens array](project_pwsh_param_name_case_collision_flattens_log_array.md)
- [Invoke-VersionReconciliation rewrites Reference version](project_reference_version_rewrite_when_assemblyversion_omitted.md)
- [Replaced-span numstat elides identical boundary lines](project_replacement_span_numstat_elides_identical_boundary_lines.md)
- [Finding's line right, description wrong](project_review_finding_line_number_right_description_wrong.md)
- [Splatting frees lines in ceiling-bound test files](project_splatting_is_the_line_budget_lever_for_ceiling_bound_test_files.md)
- [-WhatIf does not reach module ShouldProcess](project_whatif_does_not_reach_module_session_state.md)
- [WinForms control field installs SyncContext, deadlocks await](project_winforms_control_field_installs_synccontext_and_deadlocks_await.md)
- [Pinned Target Source prose carries census tokens](project_pinned_target_source_prose_carries_census_tokens.md) - comments and moved remarks trip count gates
