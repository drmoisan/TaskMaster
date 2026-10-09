# Atomic Executor Memory Index

- Sub-index: [issue-specific lessons, C# coverage mechanics, component C# gotchas](index_issue_specific_and_component_gotchas.md)

## Plan validation & gates
- [Mid-plan sanitisation](project_midplan_commit_needs_capture_time_sanitisation_gate.md) · [Sanitisation self-sweep](project_sanitisation_task_cannot_sweep_its_own_record.md) · [Blocked Bash drops check-off](project_blocked_bash_command_silently_drops_chained_checkoff.md)
- [Tool results inject "use Bash"](project_tool_results_inject_bash_read_edit_instruction.md) · [CSharpier chain-wrap](project_csharpier_chain_wrap_defeats_singleline_search_gates.md) · [Numbered citations](feedback_verify_line_citations_with_numbered_output.md)
- [Authoring counts undercount](project_plan_authoring_time_token_counts_are_undercounts.md) · [Planner/executor worktrees](project_planner_and_executor_observe_different_worktrees.md) · [Caller count drifts](project_caller_stated_preflight_count_drifts_before_execution.md)
- [Extract gate literals](project_preflight_gate_literal_extract_from_plan_not_retype.md) · [Tool layer collapses \\](project_tool_layer_collapses_double_backslash_in_file_content.md) · [Self-derived thresholds](project_preflight_selfderived_gate_thresholds_are_blind.md)
- [Exact count vs remediation](project_exact_count_gate_vs_remediation_loop.md) · [Inline-dispatch citation](project_inline_dispatch_harness_citation_makes_execution_time_test_vacuous.md) · [Drain note vacuous](project_preflight_drain_scope_optimization_note_makes_test_vacuous.md)
- [Qualifier detachment](project_multipattern_gate_shared_qualifier_detachment.md) · [Zero-hit doc comments](project_banned_api_zero_hit_gate_hits_doc_comments.md) · [Follow-up promotion](project_followup_promotion_task_is_unexecutable_by_executor.md)
- [Supersede residual](project_supersede_clause_leaves_hard_routing_residual.md) · [No dispatch tool](project_plan_delegation_to_typed_engineer_without_dispatch_tool.md) · [Check-off fixpoint](project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate.md)
- [Tracked agent-memory](project_agent_memory_tracked_breaks_unscoped_git_gates.md) · [Merge-base cadence](project_preflight_mergebase_diff_gates_need_commit_cadence.md) · [BASELINE_SHA merged base](project_baseline_sha_diff_conflates_merged_base.md)
- [Epic inherited commits](project_epic_child_branch_anchored_diff_lists_inherited_commits.md) · [Moving-base diff](project_preflight_moving_base_two_dot_diff_inertness_test.md) · [Renumbering](project_plan_task_ids_digit_only_forces_renumbering.md)
- [Bugfix grows file](project_bugfix_phase_grows_the_file_despite_dead_code_removal.md) · [Write drops BOM](project_write_tool_drops_utf8_bom_edit_prefix_restores_it.md)
- [AC check-off paths](project_preflight_ac_checkoff_and_tooloutput_paths.md) · [Override is not an AC](project_orchestrator_override_does_not_satisfy_an_ac.md) · [Output Summary count](project_artifact_output_summary_breaks_its_own_exact_count_gate.md)
- [Scope gate vs later artifacts](project_scope_gate_cannot_list_artifacts_written_after_it.md) · [Sibling-owned zero gate](project_preflight_absolute_zero_gate_on_sibling_owned_assembly.md) · [Dir-scoped format](project_directory_scoped_format_breaks_ownership_gates.md)
- [Proportionate bar](feedback_confirmatory_preflight_proportionate_bar.md) · [4 C# defect classes](project_preflight_recurring_csharp_plan_defect_classes.md)
- [Scratch reconstruction + cached CSharpier](project_preflight_scratch_reconstruction_with_cached_csharpier.md) (verify census rows post-format; FailedTestName blind to aborted; stray gate misses pwsh runner)
- [msbuild log csc line](project_msbuild_log_token_search_matches_csc_command_line.md) · [Epic base line counts](project_epic_integration_base_invalidates_research_line_counts.md) · [Citation-match false fact](project_preflight_citation_match_propagates_false_fact.md)
- [Cites LATER artifact](project_preflight_checkoff_cites_later_task_artifact.md) · [Pre-edit vs post-edit table](project_preedit_gate_cites_postedit_replacement_table.md) · [Conjunctive criteria](project_preflight_conjunctive_criterion_citation_gap.md)
- [Unrecorded baseline count](project_gate_cites_a_baseline_count_the_baseline_task_never_records.md) · [Blanket + forward deps](project_preflight_blanket_assertion_and_forward_dependency.md) · [pwsh quoting boundary](project_pwsh_command_quoting_boundary.md)
- [Stale locators](project_plan_line_locators_stale_after_doc_edit.md) · [csproj ranges shift](project_plan_csproj_line_ranges_shift_during_execution.md) · [csc probe](project_preflight_csc_probe_for_mandated_csharp_shapes.md) · [Evidence token scan](project_preflight_evidence_field_token_scan.md)
- [Research arithmetic](project_plan_literal_assertions_inherit_research_arithmetic.md) · [Fix tasks inherit rules](project_preflight_fix_tasks_inherit_decomposition_rules.md) · [Conditional split](project_conditional_split_three_task_shape.md)
- [No round diff](project_preflight_round_over_round_diff_unavailable.md) · [Flaky carve-out](project_flaky_test_carveout_added_to_one_task_only.md) · [Bullet negates clause](project_revision_bullet_negates_earlier_clause_left_standing.md)
- [Mid-run main merge re-anchor](project_midrun_main_merge_reanchor_scope_phrasing.md) (an "in pass 2" scope misses post-loop tasks; restart basis not restated in the conventions)
- [Multi-line trx MESSAGE; Markdown-indent width](project_trx_message_multiline_and_markdown_indent_width.md) (Moq Verify splits reason and count onto two lines; recount wrap claims without the 4-space indent)
- [Ambient drainable SyncContext is vacuous](project_ambient_drainable_synccontext_is_vacuous_without_async_continuations.md) (same-thread SetResult inlines; needs RunContinuationsAsynchronously)
- [Glob blind under .claude/worktrees](project_glob_tool_blind_under_claude_worktrees.md) (empty Glob in an item worktree is not absence; use Read/Grep)
- [FullName .claude skip + DisplayName census](project_preflight_fullname_claude_exclusion_and_displayname_census.md) (FullName `*\.claude\*` filter empties counts in item worktrees; DisplayName rows inflate name tokens)
- [Stall probe treats a failure as a stall](project_stall_probe_clear_rule_treats_a_failure_as_a_stall.md) (fast shell-icon failure picks DIRECT, so a runner-verbatim AC becomes unreachable)
- [pwsh payload hook containment](project_phrase_count_payload_trips_promotion_gh_issue_hook.md) (pwsh is a wrapper: issue+new+any "gh" substring is refused; never merge payloads)
- [Restart re-enters pre-commit census; closed class lists](project_restart_loop_reenters_precommit_census_and_closed_classification_lists.md) (follow restart arrows post-commit and into pass 2; map every census line)
- [gh/CI-log traps](project_gh_ci_log_and_download_gotchas.md) (createdAt trips pr-author hook; ANSI in Pester log; gh download never overwrites) · [Abbreviated caller facts](project_caller_supplied_fact_list_can_be_abbreviated_and_look_like_a_plan_defect.md)

- [MSTest summary absent on failure](project_mstest_runner_summary_absent_on_failure_voids_flaky_carveout.md) (flaky carve-out keyed on summary never fires; read the trx)
- [Generic seam over embedded interop = CS1769](project_generic_seam_over_embedded_interop_type_cs1769.md) (Func<Attachment,...> seams compile in UtilitiesCS, fail at every test call site)
## Sub-indexes (open when the topic is in scope)
- [Test isolation + C# coverage](index_test_isolation_and_coverage.md) — vstest/MSTest hangs, flakiness, Cobertura/dotnet-coverage/Koverage
- [C# nullable + component gotchas](index_csharp_nullable_and_component_gotchas.md) — net481 pragma gates, CS86xx, WebView2, QFC, TimeProvider
- [pwsh, git and gate mechanics](index_pwsh_git_and_gate_mechanics_misc.md) — pwsh parse traps, PoshQC/Pester, numstat/hunk gates, hooks
- [Mid-plan commit sanitisation gate](project_midplan_commit_needs_capture_time_sanitisation_gate.md) · [Sanitisation can't sweep its own record](project_sanitisation_task_cannot_sweep_its_own_record.md)
Four sections live in sub-index files to keep this file under its read limit. Open the sub-index when its topic applies:
- [Build / toolchain environment](index_build_toolchain.md) — SDK/restore bootstrap, CSharpier, analyzers, pwsh/Bash quoting and backslash transport
- [Test execution, isolation and coverage](index_test_execution_and_coverage.md) — long runs, flaky classes, dotnet-coverage and Cobertura mechanics
- [Nullable / C# and component gotchas](index_csharp_and_components.md) — pragma gates, CS06xx/CS8xxx, WinForms/WebView2/QFC specifics
- [Artifact hygiene and misc](index_artifact_hygiene_and_misc.md) — host-path leaks, TRX/msbuild sanitisation, hunk/numstat traps, PoshQC
- [Overflow index: ~170 moved entries](index_overflow_entries.md) — plan-gate defect classes, build/test env, hygiene, pwsh/git pitfalls, component gotchas
