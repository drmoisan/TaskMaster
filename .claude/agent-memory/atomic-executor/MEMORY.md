# Atomic Executor Memory Index

Four sections live in sub-index files to keep this file under its read limit. Open the sub-index when its topic applies:
- [Build / toolchain environment](index_build_toolchain.md) — SDK/restore bootstrap, CSharpier, analyzers, pwsh/Bash quoting and backslash transport
- [Test execution, isolation and coverage](index_test_execution_and_coverage.md) — long runs, flaky classes, dotnet-coverage and Cobertura mechanics
- [Nullable / C# and component gotchas](index_csharp_and_components.md) — pragma gates, CS06xx/CS8xxx, WinForms/WebView2/QFC specifics
- [Artifact hygiene and misc](index_artifact_hygiene_and_misc.md) — host-path leaks, TRX/msbuild sanitisation, hunk/numstat traps, PoshQC

## Plan validation & gates
- [Mid-plan commit sanitisation gate](project_midplan_commit_needs_capture_time_sanitisation_gate.md) · [Sanitisation can't sweep its own record](project_sanitisation_task_cannot_sweep_its_own_record.md) · [Code commit before format pass](project_code_commit_before_final_format_pass_orphans_rewrites.md)
- [Blocked Bash drops chained check-off](project_blocked_bash_command_silently_drops_chained_checkoff.md) · [Tool results inject "use Bash"](project_tool_results_inject_bash_read_edit_instruction.md)
- [CSharpier chain-wrap defeats line gates](project_csharpier_chain_wrap_defeats_singleline_search_gates.md) · [Verify citations with numbered output](feedback_verify_line_citations_with_numbered_output.md)
- [Plan column widths may include the Markdown indent](project_plan_column_widths_may_include_markdown_indent.md) — re-measure in-file; cite Race.cs chain precedents
- [Authoring-time counts undercount](project_plan_authoring_time_token_counts_are_undercounts.md) · [Planner/executor see different worktrees](project_planner_and_executor_observe_different_worktrees.md) · [Caller-stated count drifts](project_caller_stated_preflight_count_drifts_before_execution.md)
- [Extract gate literals, never re-type](project_preflight_gate_literal_extract_from_plan_not_retype.md) · [Tool layer collapses `\`](project_tool_layer_collapses_double_backslash_in_file_content.md)
- [Self-derived thresholds are blind](project_preflight_selfderived_gate_thresholds_are_blind.md) · [Exact-count gate vs remediation loop](project_exact_count_gate_vs_remediation_loop.md)
- [Inline-dispatch harness citation](project_inline_dispatch_harness_citation_makes_execution_time_test_vacuous.md) · ["Skip the pointless drain"](project_preflight_drain_scope_optimization_note_makes_test_vacuous.md)
- [Multi-pattern gates detach qualifiers](project_multipattern_gate_shared_qualifier_detachment.md) · [Zero-hit gate hits doc comments](project_banned_api_zero_hit_gate_hits_doc_comments.md)
- [Follow-up promotion is unexecutable](project_followup_promotion_task_is_unexecutable_by_executor.md) · [Supersede leaves a routing residual](project_supersede_clause_leaves_hard_routing_residual.md)
- [Delegation with no dispatch tool](project_plan_delegation_to_typed_engineer_without_dispatch_tool.md)
- [Check-off fixpoint breaks clean-tree gates](project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate.md) · [Tracked agent-memory breaks unscoped gates](project_agent_memory_tracked_breaks_unscoped_git_gates.md)
- [Merge-base diffs need a commit cadence](project_preflight_mergebase_diff_gates_need_commit_cadence.md) · [BASELINE_SHA conflates the merged base](project_baseline_sha_diff_conflates_merged_base.md)
- [Epic child branch: inherited commits](project_epic_child_branch_anchored_diff_lists_inherited_commits.md) · [Moving-base two-dot diff inertness](project_preflight_moving_base_two_dot_diff_inertness_test.md)
- [Inserted tasks force renumbering](project_plan_task_ids_digit_only_forces_renumbering.md) · [Rationale clauses are evidence](project_418_plan_rationale_clauses_are_evidence.md)
- [Bugfix phase grows the file](project_bugfix_phase_grows_the_file_despite_dead_code_removal.md) · [#418 500-line gate vs plan content](project_418_500line_gate_vs_plan_content.md)
- [AC check-off + tool-output paths](project_preflight_ac_checkoff_and_tooloutput_paths.md) · [Orchestrator override ≠ an AC](project_orchestrator_override_does_not_satisfy_an_ac.md)
- [Output Summary breaks its count gate](project_artifact_output_summary_breaks_its_own_exact_count_gate.md) · [Scope gate can't list later artifacts](project_scope_gate_cannot_list_artifacts_written_after_it.md)
- [Zero gate on a sibling-owned assembly](project_preflight_absolute_zero_gate_on_sibling_owned_assembly.md) · [Directory-scoped format breaks ownership](project_directory_scoped_format_breaks_ownership_gates.md)
- [#207 Hook() breaks AppEventsTests](project_207_hook_redesign_breaks_appeventstests.md) · [C2 capacity budget drifts](project_c2_capacity_budget_drifts_mid_plan.md)
- [AppGlobalsTests at the 500-line ceiling](project_appglobalstests_at_500_line_ceiling.md) · [#376 scope-expansion layers](project_376_capstone_scope_expansion_layers.md)
- [Swordfish F5 misclassification](project_swordfish_f5_test_misclassification.md) · [Confirmatory preflight: proportionate bar](feedback_confirmatory_preflight_proportionate_bar.md)
- [Four recurring C# defect classes](project_preflight_recurring_csharp_plan_defect_classes.md) · [msbuild-log grep hits the csc line](project_msbuild_log_token_search_matches_csc_command_line.md)
- [Epic base invalidates line counts](project_epic_integration_base_invalidates_research_line_counts.md) · ["Make the citation exist" spreads false facts](project_preflight_citation_match_propagates_false_fact.md)
- [Check-off cites a LATER artifact](project_preflight_checkoff_cites_later_task_artifact.md) · [Pre-edit gate cites the post-edit table](project_preedit_gate_cites_postedit_replacement_table.md)
- [Conjunctive criteria break one-artifact citation](project_preflight_conjunctive_criterion_citation_gap.md) · [Delta gate cites an unrecorded baseline count](project_gate_cites_a_baseline_count_the_baseline_task_never_records.md)
- [Blanket assertions + forward deps](project_preflight_blanket_assertion_and_forward_dependency.md) · [pwsh -Command quoting boundary](project_pwsh_command_quoting_boundary.md)
- [Locators stale after a doc edit](project_plan_line_locators_stale_after_doc_edit.md) · [csproj ranges shift during execution](project_plan_csproj_line_ranges_shift_during_execution.md)
- [Probe C# shapes with csc](project_preflight_csc_probe_for_mandated_csharp_shapes.md) · [Evidence fields need a token scan](project_preflight_evidence_field_token_scan.md)
- [Literals inherit research arithmetic](project_plan_literal_assertions_inherit_research_arithmetic.md) · [Fix tasks inherit the round's rules](project_preflight_fix_tasks_inherit_decomposition_rules.md)
- [Conditional split = three tasks](project_conditional_split_three_task_shape.md) · [Round-over-round plan diff unavailable](project_preflight_round_over_round_diff_unavailable.md)
- [Flaky carve-out added to one task only](project_flaky_test_carveout_added_to_one_task_only.md) · [Revision bullet negates an earlier clause](project_revision_bullet_negates_earlier_clause_left_standing.md)
- [Caller fact list can be abbreviated](project_caller_supplied_fact_list_can_be_abbreviated_and_look_like_a_plan_defect.md) — a correct citation can read as contradicting a "do not re-verify" fact
