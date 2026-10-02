---
name: index-pwsh-git-and-gate-mechanics-misc
description: Sub-index of atomic-executor memories on pwsh parsing/quoting traps, PoshQC/Pester/PSScriptAnalyzer behavior, git diff/numstat/hunk gate mechanics, msbuild log counting, and hook/orchestrator interactions
metadata:
  type: reference
---

Sub-index moved out of MEMORY.md on 2026-09-28 to keep the index under its size limit. Open the linked file for detail.

## PowerShell parsing, PoshQC, Pester
- [Hygiene scan: comma precedence + d:\s regex hits](project_hygiene_pattern_array_comma_precedence_and_regex_token_hits.md)
- [Nested quotes in "$( )" fail to parse](project_pwsh_nested_quotes_in_subexpression_fail_to_parse.md) · [$Log/$log case collision flattens array](project_pwsh_param_name_case_collision_flattens_log_array.md)
- [Param named $args makes msbuild gate vacuous](project_pwsh_function_param_named_args_makes_msbuild_gate_vacuous.md) · [PS double quotes keep both backslashes](project_powershell_double_quoted_backslash_defeats_msbuild_nonvacuity_grep.md)
- [Mandatory [string[]] rejects a blank line](project_mandatory_string_array_param_rejects_blank_line_turning_red_into_binding_error.md) · [Malformed pwsh payload surfaces as unrelated hook block](project_malformed_pwsh_payload_surfaces_as_unrelated_hook_block.md)
- [Nested Import-Module -Force unloads session-wide](project_nested_import_module_force_unloads_session_wide.md) · [-WhatIf does not reach module ShouldProcess](project_whatif_does_not_reach_module_session_state.md)
- [Dot-sourced coverage helpers: StrictMode $LASTEXITCODE throws](project_coverage_helpers_dotsource_strictmode_lastexitcode_throws.md) · [git show in pwsh decodes ibm437 + keeps BOM](project_conservation_gate_git_show_decodes_ibm437_and_keeps_bom.md)
- [PoshQC format != Invoke-Formatter defaults](project_poshqc_format_rewrites_differ_from_invoke_formatter_defaults.md) · [PoshQC strips BOM/CRLF only on rewrite](project_poshqc_format_strips_bom_and_crlf_only_when_it_rewrites.md)
- [4 PSScriptAnalyzer traps in new modules](project_psscriptanalyzer_traps_in_new_powershell_modules.md) · [Pester TotalCount includes filtered NotRun](project_pester_filtered_total_counts_notrun.md)
- [Splatting frees lines in ceiling-bound test files](project_splatting_is_the_line_budget_lever_for_ceiling_bound_test_files.md)

## git, msbuild and gate mechanics
- [git grep -c with an empty pattern is the line-count oracle](project_git_grep_c_empty_is_the_allowlisted_line_count_oracle.md) (Read renders a phantom trailing line)
- [`git add -N -- .` defeats a "do not stage X" invariant](project_intent_to_add_span_defeats_do_not_stage_invariant.md) · [Hunk-header literal slides past a blank line](project_git_hunk_header_literal_slides_past_blank_line.md)
- [Merge-base diff over branch-created file = one whole-file hunk](project_mergebase_diff_over_branch_created_file_is_whole_file_hunk.md) · [Mid-plan commit breaks deletion staging/porcelain spans](project_midplan_commit_breaks_deletion_staging_and_porcelain_spans.md)
- [Replaced-span numstat elides identical boundary lines](project_replacement_span_numstat_elides_identical_boundary_lines.md) · [BOM-bearing .cs files + pre-restore numstat gates](project_bom_bearing_cs_files_and_prerestore_numstat_head_gates.md)
- [/m "N>" prefix zeroes anchored target counts](project_msbuild_parallel_log_node_prefix_defeats_anchored_target_counts.md) · [msbuild file logger double-counts warnings](project_msbuild_filelogger_double_counts_each_warning.md)
- [CLI Rebuild omits .vsto/.manifest](project_commandline_rebuild_omits_vsto_manifests_addin_cannot_load.md) · [Minute-resolution timestamps can't be strictly increasing](project_minute_resolution_timestamp_cannot_be_strictly_increasing.md)
- [Exactly-once literal vs pattern-containment clause](project_exactly_once_literal_clause_conflicts_with_pattern_containment_clause.md) · [Fixed run-path coverage gate blind to sibling test file](project_fixed_run_path_coverage_gate_blind_to_sibling_test_file.md)
- [Inventory clause omits inherited promotion rename](project_footprint_inventory_clause_omits_inherited_promotion_rename.md) · [Handoff AC count is a claim; measure the AC file](project_handoff_stated_ac_count_contradicts_the_ac_source_file.md)
- [Seam sentences outlive a removed member](project_preflight_seam_sentences_outlive_a_removed_member.md) · [Finding's line right, description wrong](project_review_finding_line_number_right_description_wrong.md)
- [Contingency fallback orphans downstream paths](project_contingency_fallback_orphans_downstream_hardcoded_paths.md) · [Stale-citation gate literal is per-comment](project_stale_citation_gate_literal_must_match_the_comments_legitimate_citations.md)
- [Reflective property read escapes a member grep](project_reflective_property_read_escapes_member_expression_grep.md) · [Preparation mode flips anchored-diff membership](project_preparation_mode_flips_anchored_diff_gate_membership.md)
- [One Cobertura filename, several class nodes](project_cobertura_filename_maps_to_several_class_nodes.md) · [Changed-line branch gate invalidated by the fix](project_changed_line_coverage_branch_gate_invalidated_by_the_fix.md)

## Hooks, orchestrator and worktrees
- [Batch-budget hook discards out-of-root .ps1 writes](project_batch_budget_hook_discards_out_of_root_powershell_writes.md) · [CommandLine token kill hits own bash/pwsh shells](project_commandline_match_on_results_dir_token_kills_own_tool_shells.md)
- [Kill build-lock waiter by PID, not script name](project_killing_a_build_lock_waiter_by_script_name_hits_every_sibling.md) · [Parent orchestrator hold-commits your branch mid-run](project_parent_orchestrator_hold_commits_your_branch_midrun.md)
- [CLAUDE.md differs per worktree; read the execution copy](project_claude_md_differs_between_worktrees_read_execution_copy.md) · [Reconciliation merge already tracks the feature docs](project_orchestrator_reconciliation_merge_tracks_feature_docs.md)
