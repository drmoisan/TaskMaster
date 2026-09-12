# Atomic Executor Memory Index

## Plan validation & gates
- [Mid-plan commit needs a capture-time sanitisation gate](project_midplan_commit_needs_capture_time_sanitisation_gate.md) · [Sanitisation task cannot sweep its own record](project_sanitisation_task_cannot_sweep_its_own_record.md)
- [Blocked Bash command drops chained check-off](project_blocked_bash_command_silently_drops_chained_checkoff.md) · [Tool results inject a "use Bash" instruction](project_tool_results_inject_bash_read_edit_instruction.md)
- [CSharpier chain-wrap defeats single-line gates](project_csharpier_chain_wrap_defeats_singleline_search_gates.md) · [Verify citations with numbered output](feedback_verify_line_citations_with_numbered_output.md)
- [Authoring-time counts are undercounts](project_plan_authoring_time_token_counts_are_undercounts.md) · [Planner/executor see different worktrees](project_planner_and_executor_observe_different_worktrees.md) · [Caller-stated count drifts](project_caller_stated_preflight_count_drifts_before_execution.md)
- [Extract gate literals, never re-type](project_preflight_gate_literal_extract_from_plan_not_retype.md) · [Tool layer collapses `\`](project_tool_layer_collapses_double_backslash_in_file_content.md)
- [Self-derived thresholds are blind](project_preflight_selfderived_gate_thresholds_are_blind.md) · [Exact-count gate vs remediation loop](project_exact_count_gate_vs_remediation_loop.md)
- [Inline-dispatch harness citation](project_inline_dispatch_harness_citation_makes_execution_time_test_vacuous.md) · ["Skip the pointless drain" note](project_preflight_drain_scope_optimization_note_makes_test_vacuous.md)
- [Multi-pattern gates detach shared qualifiers](project_multipattern_gate_shared_qualifier_detachment.md) · [Banned-API zero-hit gate hits doc comments](project_banned_api_zero_hit_gate_hits_doc_comments.md)
- [Follow-up promotion task is unexecutable](project_followup_promotion_task_is_unexecutable_by_executor.md) · [Supersede clause leaves a routing residual](project_supersede_clause_leaves_hard_routing_residual.md)
- [Delegation to csharp-typed-engineer with no dispatch tool](project_plan_delegation_to_typed_engineer_without_dispatch_tool.md)
- [Plan check-off fixpoint breaks clean-tree gates](project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate.md) · [Tracked agent-memory breaks unscoped git gates](project_agent_memory_tracked_breaks_unscoped_git_gates.md)
- [Merge-base diff gates need a commit cadence](project_preflight_mergebase_diff_gates_need_commit_cadence.md) · [BASELINE_SHA conflates the merged base](project_baseline_sha_diff_conflates_merged_base.md)
- [Epic child branch: anchored diff lists inherited commits](project_epic_child_branch_anchored_diff_lists_inherited_commits.md) — footprint gates unsatisfiable; use `git diff HEAD`
- [Moving-base two-dot diff needs an inertness test](project_preflight_moving_base_two_dot_diff_inertness_test.md)
- [Inserted tasks force renumbering](project_plan_task_ids_digit_only_forces_renumbering.md) · [Rationale clauses are evidence](project_418_plan_rationale_clauses_are_evidence.md)
- [Bugfix phase grows the file anyway](project_bugfix_phase_grows_the_file_despite_dead_code_removal.md) · [#418 500-line gate vs mandated plan content](project_418_500line_gate_vs_plan_content.md)
- [AC check-off + artifacts/ tool-output paths](project_preflight_ac_checkoff_and_tooloutput_paths.md) · [Orchestrator override does not satisfy an AC](project_orchestrator_override_does_not_satisfy_an_ac.md)
- [Output Summary breaks its own count gate](project_artifact_output_summary_breaks_its_own_exact_count_gate.md) · [Scope gate cannot list artifacts written after it](project_scope_gate_cannot_list_artifacts_written_after_it.md)
- [Absolute-zero gate on a sibling-owned assembly](project_preflight_absolute_zero_gate_on_sibling_owned_assembly.md) · [Directory-scoped format breaks ownership gates](project_directory_scoped_format_breaks_ownership_gates.md)
- [#207 Hook() redesign breaks AppEventsTests](project_207_hook_redesign_breaks_appeventstests.md) · [C2 capacity budget drifts mid-plan](project_c2_capacity_budget_drifts_mid_plan.md)
- [ApplicationGlobalsTests.cs at 500-line ceiling](project_appglobalstests_at_500_line_ceiling.md) · [#376 capstone scope-expansion layers](project_376_capstone_scope_expansion_layers.md)
- [Swordfish F5 test misclassification](project_swordfish_f5_test_misclassification.md) · [Confirmatory preflight: proportionate bar](feedback_confirmatory_preflight_proportionate_bar.md)
- [Four recurring C# plan defect classes](project_preflight_recurring_csharp_plan_defect_classes.md) · [msbuild-log grep matches the csc command line](project_msbuild_log_token_search_matches_csc_command_line.md)
- [Epic base invalidates research line counts](project_epic_integration_base_invalidates_research_line_counts.md) · ["Make the citation exist" propagates false facts](project_preflight_citation_match_propagates_false_fact.md)
- [Check-off cites an artifact a LATER task writes](project_preflight_checkoff_cites_later_task_artifact.md) · [Pre-edit gate cites the post-edit table](project_preedit_gate_cites_postedit_replacement_table.md)
- [Conjunctive criteria break the one-artifact citation rule](project_preflight_conjunctive_criterion_citation_gap.md)

## Build / toolchain environment
- [pwsh/git/gh CLI gotchas](project_pwsh_git_gh_cli_gotchas.md) · [Project Build/Test Env](project_build_test_env.md) — no jq; MSYS_NO_PATHCONV
- [VS18 toolchain paths](project_vs18_build_toolchain_paths.md) · [Repo-local SDK + nullable Rebuild](project_repo_sdk_and_nullable_rebuild.md)
- [Fresh worktree needs SDK install + `/p:RestorePackagesConfig=true`](project_fresh_worktree_needs_sdk_and_packages_config_restore.md) — plain `/t:Restore` restores nothing
- [Start-Process -ArgumentList strips quoting](project_startprocess_arglist_array_strips_quoting.md) · [Relative paths in pwsh hit the wrong worktree](project_relative_path_in_pwsh_dotnet_io_hits_wrong_worktree.md)
- [QuickFiler.Test coverage hang](project_quickfiler_test_coverage_hang_and_build_flags.md) · [Dot-sourcing clobbers $CoverageOutput](project_dotsourcing_invoke_mstest_clobbers_coverageoutput_param.md)
- [vstest TestCaseFilter: `|` not OR](project_vstest_testcasefilter_or_operator_and_env_setup.md) · [Test file name != partial class name](project_test_file_name_vs_partial_class_name.md)
- [Analyzer HintPath skew breaks all four gates](project_analyzer_hintpath_skew_breaks_all_four_gates.md) · [Analyzer version skew on fresh worktree](project_analyzer_version_skew_fresh_worktree.md)
- [New sln member surfaces MSB3277](project_new_sln_member_surfaces_msb3277_pin_divergence.md) · [Legacy csproj: no transitive compile refs](project_legacy_csproj_no_transitive_compile_refs.md)
- [sln/csproj edits: preserve CRLF](project_sln_csproj_edit_crlf_preserve.md) · [Incremental build makes a vacuous baseline](project_incremental_build_vacuous_baseline.md) · [Nullable /t:Build is vacuous](project_nullable_build_gate_is_vacuous_incremental.md)
- [CSharpier skips *.Designer.cs by filename](project_csharpier_skips_designer_cs_by_filename.md)
- [.gitignore `*.log` blocks msbuild-log evidence](project_gitignore_star_log_blocks_committed_msbuild_log_evidence.md) — `git add -N` discriminates
- [csharpier pipe-files is non-enforcing](project_csharpier_pipefiles_nonenforcing_gate.md) · [Count-idiom pitfalls](project_count_idiom_pitfalls_csharpier_and_measureobject.md) · [New .cs force a format-loop restart](project_new_cs_files_guarantee_a_format_loop_restart.md)
- [PowerShell new files need UTF-8 BOM](powershell-bom-required.md) · [BOM breaks grep ^](project_bom_grep_anchor_false_negative.md) · [StrictMode + missing XML attribute throws](project_pester_strictmode_xml_attribute_property_access.md)
- [Pester 5 helper must live in BeforeAll](project_pester5_helper_function_must_live_in_beforeall.md)
- [`-NoExecute` unreachability premise is often false](project_noexecute_early_return_premise_hides_reachable_entry_point_tests.md)
- [poshqc test MCP carries no verdict](project_poshqc_pester_mcp_exit_minus1.md) · [poshqc analyze exits 1 on a Warning](project_poshqc_analyze_exit1_on_warning.md) · [Pester 5 result shape](project_pester5_result_shape_container_tests_and_ci_codecoverage.md)
- [Bash heredoc collapses `\\`](project_bash_heredoc_collapses_doubled_backslashes.md) · [Unquoted backslash redirects output](project_unquoted_backslash_in_bash_arg_silently_redirects_output.md) · [Doubled backslash de-doubles](project_doubled_backslash_dedoubles_bash_to_native_exe.md)
- [Recursive delete: both idioms blocked](project_recursive_delete_idioms_blocked_use_dotnet_api.md)
- [pwsh -Command quoting](project_pwsh_command_quoting_from_bash.md) · [pwsh -File binds a list as ONE string](project_pwsh_file_array_param_from_bash.md)
- [Compile-time red needs body-level refs](project_compile_red_needs_body_level_references.md) · [Cross-task shell-variable splat gates](project_cross_task_shell_variable_splat_gate.md)
- [Evidence <TS> collision clobbers artifacts](project_evidence_timestamp_collision_clobbers_artifacts.md) · [Shared artifact + floating <ts>](project_shared_evidence_artifact_floating_ts.md)

## Test execution & isolation
- [Long runs need a detached process](project_long_runs_need_detached_process.md) — background runners die at ~1h
- [Tests must mock GUI; no visible window](feedback_tests_must_mock_gui_no_visible_window.md)
- [Full-suite run hangs though the baseline passed](project_full_suite_run_hangs_while_earlier_runs_idle.md) — sample testhost CPU
- [WinFormsPumpHost tests are load-flaky](project_winformspumphost_tests_load_flaky.md) · [#511 is a test-host crash](project_511_is_a_testhost_crash_not_n_failing_tests.md)
- [vstest /InIsolation + FilePathHelper](project_vstest_isolation_and_filepathhelper_serialization.md) · [Invoke-MSTest.ps1 dies on one assembly](project_418_invoke_mstest_single_assembly_bug.md)
- [Timed-out MSTest leaves a detached runner](project_timedout_mstest_leaves_detached_runner.md) · [Sibling-worktree shared-tooling hazard](project_sibling_worktree_shared_tooling_hazard.md)
- [Concurrent dotnet-coverage deadlock](project_concurrent_dotnet_coverage_deadlock_and_doccomment_retention_gate.md) · [Concurrent executor in same worktree](project_concurrent_executor_same_worktree.md)
- [UtilitiesCS.Test parallelism flakiness](project_utilitiescs_test_parallelism_flakiness.md) · [[DoNotParallelize] overlaps the parallel bucket](project_mstest_donotparallelize_overlaps_parallel_bucket.md)
- [log4net MemoryAppender is shared per TYPE](project_log4net_memoryappender_shared_per_type_across_parallel_classes.md) · [UiThread.Dispatcher static-swap race](project_uithread_dispatcher_static_swap_race.md)
- [runsettings DataCollector default-enabled](project_runsettings_datacollector_default_enabled.md) · [dotnet-coverage Deedle/FSharp breaks tests](project_dotnet_coverage_deedle_fsharp_instrumentation.md)
- [DispatcherDelay hangs unit tests](project_dispatcherdelay_hangs_unit_tests.md) · [ConfigController STA pump deadlock](project_configcontroller_sta_pump_deadlock.md)

## Coverage measurement
- [Exempt-forward extraction leaves call site uncovered](project_exempt_forward_extraction_leaves_call_site_uncovered.md)
- [Reproduce the baseline's counting method](project_coverage_delta_reproduce_baseline_counting_method.md) — deduped vs all-descendant
- [Async state machine emits no `<method>` element](project_async_state_machine_emits_no_method_element.md)
- [First-party coverage denominator (#197)](project_coverage_firstparty_denominator_method.md) · [dotnet-coverage denominator nondeterminism](project_dotnet_coverage_denominator_nondeterminism.md)
- [Failed run leaves RAW Cobertura](project_failed_coverage_run_leaves_raw_unprocessed_cobertura.md) · [runner throws before post-processing](project_coverage_runner_throws_before_postprocessing.md) · [Koverage post-processing shape](project_koverage_cobertura_postprocessing_shape.md)
- [C# canonical coverage artifact conversion](project_csharp_canonical_coverage_artifact_conversion.md) · [Cobertura runsettings `<Attributes>` override](project_cobertura_runsettings_attributes_override.md)
- [Package rollup must use the repo helper](project_cobertura_package_rollup_must_use_repo_helper.md)
- [Processed Cobertura filenames use backslashes](project_processed_cobertura_filenames_use_backslash.md) — forward-slash match returns zero rows
- [Cobertura hits vs MS-coverage partial](project_changed_line_coverage_cobertura_vs_mscoverage_partial.md) · [QFC #227 coverage tooling](project_qfc227_coverage_tooling.md)
- [#398 test-split gate gotchas](project_398_test_split_gate_gotchas.md) · [ExcludeFromCodeCoverage on partial = CS0579](project_excludefromcodecoverage_partial_class_cs0579.md)
- Closed one-offs: [#400](project_400_completeopenasync_unreachable_recovery_catch.md), [Swordfish](project_swordfish_removal_epic_incidental_coverage_sideeffect.md), [#298](project_taskvis_scocollection_and_livebridge_exemptions.md), [#328](project_328_rebuild_threading_olobjectsproxy_conflict.md)

## Nullable / C# language
- [Nullable per-file pragma gate mechanics](project_nullable_pragma_gate_mechanics.md) — CI passes EXIT 0 without it
- [CLAUDE.md nullable command != the CI gate](project_507_nullconditional_return_triggers_cs8603_under_genuine_nullable_check.md)
- [Nullable remediation annotation patterns](project_nullable_remediation_annotation_patterns.md) · [Nullable annotation CS8632 scoping](project_nullable_annotation_cs8632_scoping.md)
- [init/record struct fails CS0518 on net48](project_record_struct_isexternalinit_netfx.md) · [CS8714 does not fire on net481](project_nullable_cs8714_not_on_net481.md)
- [Outlook `Action`/`Exception` ambiguity](project_outlook_action_ambiguity.md) — CS0104
- [CS1769 forces reflection for Outlook-returning APIs](project_cs1769_forces_reflection_for_outlook_returning_apis.md) — `Task<Outlook.X>` cannot be awaited from the test assembly
- Nullable-epic (closed): [#366a](project_366_notnull_cascades_beyond_wrapperscodictionary.md), [#366b](project_366_scdictionary_constraint_cascades_to_fourth_file.md), [#366c](project_366_batch7_tnullable_return_cs8766.md), [#371](project_371_outlookobjects_nullable_lessons.md), [#372](project_372_email_classifier_nullable_patterns.md), [#375](project_375_residuals_nullable_gotchas.md)

## Component-specific gotchas
- [WebView2 EndInit already creates child handles](project_webview2_endinit_creates_handles.md) · [#349 breadcrumb WebView2 gotchas](project_349_breadcrumb_webview2_gotchas.md)
- QFC #227: [cycle-4 ToggleFocus](project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md) · [cycle-3 seam](project_theme_folderpredictor_seam_retrofit_gotchas.md)
- [ObjectListView TreeListView headless selection](project_objectlistview_treelistview_headless_selection.md) · [QfcDatamodel BackgroundWorker async-void race](project_qfc_backgroundworker_async_void_race.md)
- [QfcItemController harness needs SaveParameters](project_qfcitemcontroller_pump_harness_needs_saveparameters.md) · [TaskController (#297) test gotchas](project_taskvisualization_taskcontroller_test_gotchas.md)
- [ProjectEntry setter raw MessageBox](project_projectentry_setter_raw_messagebox.md) · [IApplicationGlobals member forces implementers](project_iapplicationglobals_member_forces_implementers.md)
- [TimeProvider seam gotchas](project_timeprovider_seam_gotchas.md) · [Initializer.GetOrLoad discards setter injection](project_initializer_getorload_discards_injection_when_dependency_null.md)
- [ScoDictionaryNew needs TryAdd not Add](project_scodictionarynew_tryadd_not_add.md) · [FluentAssertions Equal(params) has no because](project_fluentassertions_equal_params_no_because.md)

## Artifact hygiene
- [Never embed absolute host paths](../_shared_no_absolute_host_paths.md) · [Never predict an observation](feedback_never_predict_an_observation_into_an_artifact.md) — placeholder, observe, append
- [Evidence <TS> drifts ahead of write time](project_evidence_timestamp_labels_drift_ahead_of_write_time.md) · [Probe literal trips the NEXT sweep](project_selftest_probe_literal_trips_the_next_sweep_pass.md)
- [TRX sanitisation is case-insensitive](project_trx_sanitisation_must_be_case_insensitive.md) · [TRX/msbuild need a sanitisation micro-action](project_vstest_trx_evidence_needs_sanitisation_task.md) · [MSBuild logs leak TWO roots](project_msbuild_log_has_two_absolute_path_leak_classes.md)
- [Deploy_ dir leaks tokens on FAILING runs only](project_mstest_deploy_dir_leaks_tokens_on_failing_runs.md) · [vstest leaves TWO .coverage files](project_vstest_emits_two_coverage_files_per_run.md)
- [PS budget hook blocks scratch .ps1](project_powershell_scratch_script_budget_hook_blocks_helpers.md) · [Plan-mandated .ps1 + frozen porcelain gate](project_plan_mandated_ps1_helpers_collide_with_budget_cap_and_frozen_porcelain_gate.md)
- [2nd pass must not qualify schema fields](project_appending_a_second_pass_must_not_qualify_schema_fields.md)
- [Bash resets cwd; use `env -C`](project_bash_cwd_resets_use_env_dash_c.md) · [global.json cwd-search vs no-cd discipline](project_dotnet_global_json_cwd_search_vs_bash_discipline.md)
- [pwsh -File starts in the SESSION root](project_pwsh_file_starts_in_session_root_needs_workingdirectory.md) · [pwsh stdin is a REPL](project_pwsh_stdin_repl_mode_and_nonascii_mangling.md) · [Isolation guard refuses pwsh from Bash](project_worktree_isolation_guard_refuses_pwsh_from_bash.md)
- [Changed-line branch gate invalidated by the fix](project_changed_line_coverage_branch_gate_invalidated_by_the_fix.md) · [ExpectedExitCode keyed off the baseline](project_expectedexitcode_declared_from_baseline_not_observed_run.md)
- [CSharpier forces a blank line before a comment](project_csharpier_requires_blank_line_before_comment_breaking_numstat_bounds.md)
- [ExcludeFromCodeCoverage misses `this`-capturing lambdas](project_excludefromcodecoverage_misses_this_capturing_lambdas.md) · [Koverage -RepoRoot needs native separators](project_koverage_reporoot_needs_native_separators.md)
- [FakeTimeProvider zero due time fires at creation](project_faketimeprovider_zero_duetime_fires_at_creation.md) · [BeEmpty names only the first item](project_fluentassertions_beempty_names_only_first_item.md)
- [Reflective property read escapes a member grep](project_reflective_property_read_escapes_member_expression_grep.md)
- [Green run prints no Failed/Skipped line](project_vstest_success_run_prints_no_failed_or_skipped_line.md) · [Preparation mode flips anchored-diff membership](project_preparation_mode_flips_anchored_diff_gate_membership.md)
- [git grep -c with an empty pattern is the line-count oracle](project_git_grep_c_empty_is_the_allowlisted_line_count_oracle.md) - Read renders a phantom trailing line; exact-count gates hard-stop on the off-by-one
- [Contingency fallback orphans downstream hard-coded paths](project_contingency_fallback_orphans_downstream_hardcoded_paths.md) - the citation sweep passes; only the assumed branch is wrong
- [Stale-citation gate literal is per-comment](project_stale_citation_gate_literal_must_match_the_comments_legitimate_citations.md) - a blanket gate is unsatisfiable when the comment legitimately cites an unmoved file
- [Revision bullet negates an earlier clause left standing](project_revision_bullet_negates_earlier_clause_left_standing.md) - read the WHOLE task
- [TimeoutAfter IsCompleted short-circuit loses to the Task.Run race](project_timeoutafter_iscompleted_shortcircuit_loses_to_taskrun_race.md)
- [One Cobertura filename maps to several class nodes](project_cobertura_filename_maps_to_several_class_nodes.md) - nested types repeat it; the line element doubles per node
- [msbuild file logger double-counts every warning](project_msbuild_filelogger_double_counts_each_warning.md) - inline + summary; a whole-log count gate fails at exactly 2x
- [Reconciliation merge already tracks the feature docs](project_orchestrator_reconciliation_merge_tracks_feature_docs.md) - an untracked-folder baseline gate is false on arrival
- [Round-over-round plan diff is unavailable](project_preflight_round_over_round_diff_unavailable.md) - the plan's only commit predates every round; substitute a full re-read and say so
- [Flaky-test carve-out added to one task only](project_flaky_test_carveout_added_to_one_task_only.md) - siblings running the same suite keep the stop rule; the row may be stranded
## Plan structure, preflight, execution protocol

- [Preflight: blanket assertions + forward-phase deps](project_preflight_blanket_assertion_and_forward_dependency.md) — the two recurring blockers to check mechanically
- [pwsh -Command quoting boundary](project_pwsh_command_quoting_boundary.md) — outer double quotes let bash/PowerShell eat `$var` and `$(...)` before pwsh sees them; execute plan commands verbatim during preflight
- [Inserted plan tasks force renumbering](project_plan_task_ids_digit_only_forces_renumbering.md) — suffixed IDs (`P3-T5a`) fail validation; insert + renumber downstream
- [Plan rationale clauses are evidence](project_418_plan_rationale_clauses_are_evidence.md) — unmeasured world-state claims in prose block preflight, not the fix
- [Line locators go stale after a doc edit](project_plan_line_locators_stale_after_doc_edit.md) — a revision that expands spec.md shifts every later citation; verify each cited line
- [csproj line ranges shift during execution](project_plan_csproj_line_ranges_shift_during_execution.md) — earlier tasks adding Compile entries invalidate a later task's cited block range; cite blocks by name
- [#418 500-line gate vs mandated plan content](project_418_500line_gate_vs_plan_content.md) — unsatisfiable size gate; delta = extract pure helpers to a new file
- [#376 capstone scope-expansion layers](project_376_capstone_scope_expansion_layers.md) — 5 escalated layers resolved via the 3 authorized patterns
- [#207 Hook() redesign breaks AppEventsTests](project_207_hook_redesign_breaks_appeventstests.md) — needs a plan revision, not a test weakening
- [#400 CompleteOpenAsync unreachable recovery catch](project_400_completeopenasync_unreachable_recovery_catch.md) — dead code can't reach >=90%; escalate, don't force
- [Concurrent executor in same worktree](project_concurrent_executor_same_worktree.md) — two executors corrupt shared files; STOP, don't stash/race
- [Sibling-worktree shared-tooling hazard](project_sibling_worktree_shared_tooling_hazard.md) — other worktrees crash your testhost via shared vstest/dotnet-coverage
- [ApplicationGlobalsTests.cs at 500-line ceiling](project_appglobalstests_at_500_line_ceiling.md) — exactly 500 lines; extract before adding an override


- [Project Build/Test Env](project_build_test_env.md) — git-bash quirks, MSBuild switches, csharpier v1, legacy csproj includes, IVT, C# 7.3
- [VS18 build/test toolchain paths](project_vs18_build_toolchain_paths.md) — use VS 18 full-framework msbuild.exe, nuget.exe restore, MSYS_NO_PATHCONV
- [Repo-local SDK install + nullable Rebuild](project_repo_sdk_and_nullable_rebuild.md) — .dotnet-sdk needs pwsh7; nullable debt scope not stable across sessions
- [vstest TestCaseFilter OR-vs-pipe + fresh-worktree bootstrap](project_vstest_testcasefilter_or_operator_and_env_setup.md) — needs `|` not `OR`; full bootstrap order
- [Analyzer version skew on fresh worktree](project_analyzer_version_skew_fresh_worktree.md) — first analyzer build CS0006; nuget install old versions into packages/
- [SecurityCodeScan incompatible with Roslyn 5.6](project_securitycodescan_roslyn56_incompat.md) — CS8032/YamlDotNet breaks the TWAE gate
- [Incremental build makes a vacuous baseline](project_incremental_build_vacuous_baseline.md) — /t:Build ignores /p: changes; add a /t:Rebuild
- [Missing VSTO runtime breaks baseline gates](project_missing_vsto_runtime_breaks_baseline_gates.md) — CS0234 in ThisAddIn.Designer.cs skews repo coverage
- [sln/csproj edits: preserve CRLF](project_sln_csproj_edit_crlf_preserve.md) — `sed -i` strips CRLF; use Edit or perl -0777
- [New sln member surfaces MSB3277](project_new_sln_member_surfaces_msb3277_pin_divergence.md) — packages.config pin divergence; never fixable in a .cs file
- [Legacy csproj: no transitive compile refs](project_legacy_csproj_no_transitive_compile_refs.md) — tests need their own `<Reference>` + packages.config entry
- [BOM breaks grep ^ anchor](project_bom_grep_anchor_false_negative.md) — use the Grep tool, never bash grep, for anchored classification
- [PowerShell new files need UTF-8 BOM](powershell-bom-required.md) — PSUseBOMForUnicodeEncodedFile; prepend BOM after Write
- [poshqc Pester MCP exits -1](project_poshqc_pester_mcp_exit_minus1.md) — pair with a direct Invoke-Pester run for the numeric proof
- [csharpier pipe-files is a non-enforcing gate](project_csharpier_pipefiles_nonenforcing_gate.md) — use `check`/`format`; size AFTER format; post-deletion tolerances open downward

## Test execution and coverage measurement

- [Invoke-MSTest.ps1 dies on a single test assembly](project_418_invoke_mstest_single_assembly_bug.md) — scalar `.Count` throws; call vstest.console.exe directly
- [Timed-out MSTest leaves detached runner](project_timedout_mstest_leaves_detached_runner.md) — kill the pwsh runner too, verify 0, then rerun
- [UtilitiesCS.Test parallelism flakiness](project_utilitiescs_test_parallelism_flakiness.md) — lower MSTest Workers to 4 via /Settings
- [dotnet-coverage Deedle/FSharp instrumentation breaks tests](project_dotnet_coverage_deedle_fsharp_instrumentation.md) — pass a module-exclude settings XML
- [dotnet-coverage denominator nondeterminism](project_dotnet_coverage_denominator_nondeterminism.md) — re-baseline via git-stash, trust per-class rates
- [Coverage delta: reproduce the baseline's counting method](project_coverage_delta_reproduce_baseline_counting_method.md) — deduped vs all-descendant denominators differ ~2x
- [First-party coverage denominator method (#197)](project_coverage_firstparty_denominator_method.md) — per-`<line>` count across ALL deduped packages
- [C# canonical coverage artifact conversion](project_csharp_canonical_coverage_artifact_conversion.md) — convert feature Cobertura to JaCoCo at artifacts/csharp/coverage.xml
- [Changed-line coverage: Cobertura hits vs MS-coverage partial](project_changed_line_coverage_cobertura_vs_mscoverage_partial.md) — use Cobertura per-line data
- [QFC #227 coverage tooling](project_qfc227_coverage_tooling.md) — vstest + Cobertura runsettings is the reliable per-class numeric path
- [Cobertura runsettings <Attributes> override](project_cobertura_runsettings_attributes_override.md) — a custom block silently disables [ExcludeFromCodeCoverage]
- [runsettings DataCollector default-enabled](project_runsettings_datacollector_default_enabled.md) — a declared collector activates without /collect
- [#398 test-split gate gotchas](project_398_test_split_gate_gotchas.md) — /EnableCodeCoverage lacks branch%; use the Cobertura-runsettings variant
- [vstest /InIsolation + FilePathHelper serialization](project_vstest_isolation_and_filepathhelper_serialization.md) — Moq assemblies need /InIsolation
- [ExcludeFromCodeCoverage on partial class = CS0579](project_excludefromcodecoverage_partial_class_cs0579.md) — annotate a partial type once, not both parts
- [Swordfish-removal epic: incidental vendored-coverage side effect](project_swordfish_removal_epic_incidental_coverage_sideeffect.md) — expected, non-blocking

## Test authoring gotchas

- [MSTest [DoNotParallelize] overlaps the parallel bucket](project_mstest_donotparallelize_overlaps_parallel_bucket.md) — mark every writer too
- [FluentAssertions Equal(params) has no because](project_fluentassertions_equal_params_no_because.md) — a trailing reason becomes an extra expected element
- [DispatcherDelay hangs unit tests](project_dispatcherdelay_hangs_unit_tests.md) — never completes in the pump-less MSTest host
- [ConfigController STA pump deadlock](project_configcontroller_sta_pump_deadlock.md) — an STA test must pump, not block on GetAwaiter().GetResult()
- [TimeProvider seam gotchas](project_timeprovider_seam_gotchas.md) — use FakeTimeProvider; an optional param forces Bcl.TimeProvider on consumers
- [IApplicationGlobals member forces implementers](project_iapplicationglobals_member_forces_implementers.md) — breaks 7 hand-written stubs beyond scope lock
- [ObjectListView TreeListView headless selection](project_objectlistview_treelistview_headless_selection.md) — needs a native handle; cache via SelectionChanged
- [QfcDatamodel BackgroundWorker async-void IsBusy race](project_qfc_backgroundworker_async_void_race.md) — assert WorkerSupportsCancellation instead
- [TaskController (#297) unit-test gotchas](project_taskvisualization_taskcontroller_test_gotchas.md) — ApplyChanges hangs over Moq; STA harness needs parenting
- [Theme/FolderPredictor seam retrofit gotchas (#227 cycle-3)](project_theme_folderpredictor_seam_retrofit_gotchas.md) — shared test-double builder causes silent regression
- [#227 cycle-4 ToggleFocus genuine-execution gotchas](project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md) — missing refs; use Activator.CreateInstance
- [#349 breadcrumb WebView2 gotchas](project_349_breadcrumb_webview2_gotchas.md) — retyped Designer field breaks reflection-injected tests
- [#328 Rebuild-threading breaks OlObjectsProxy](project_328_rebuild_threading_olobjectsproxy_conflict.md) — stub returns null for get_StoresWrapper
- [ProjectEntry setter raw MessageBox](project_projectentry_setter_raw_messagebox.md) — un-seamed MessageBox.Show hangs STA tests
- [TaskVisualization #298 ScoCollection + live-bridge exemptions](project_taskvis_scocollection_and_livebridge_exemptions.md) — method-level exempt the live-form bridge
- [Swordfish F5 test misclassification](project_swordfish_f5_test_misclassification.md) — verify using/namespace before calling a removal Swordfish-only
- [ScoDictionaryNew needs TryAdd not Add](project_scodictionarynew_tryadd_not_add.md) — `.Add(k,v)` is CS1061; base exposes `.TryAdd`


- [Nullable per-file pragma gate mechanics](project_nullable_pragma_gate_mechanics.md) — verify via isolated UtilitiesCS build with BuildProjectReferences=false
- [Nullable pragma-gate net481 mechanics](project_nullable_pragma_gate_net481_mechanics.md) — per-file gate cannot hit EXIT 0; measure scoped CS86xx
- [Nullable epic: pragma gate + analyzer restore](project_nullable_epic_pragma_gate_and_analyzer_restore.md) — scoped TWAE with WarningsNotAsErrors
- [#364 nullable-gate pre-existing blockers](project_364_nullable_gate_preexisting_blockers.md) — full-solution TWAE fails at baseline
- [Nullable remediation annotation patterns](project_nullable_remediation_annotation_patterns.md) — net481 has no post-condition attrs; `= default!`, `.ToString()!`
- [#371 OutlookObjects nullable lessons](project_371_outlookobjects_nullable_lessons.md) — public-signature changes regress other nullable files
- [#375 residuals nullable gotchas](project_375_residuals_nullable_gotchas.md) — CS8644 fixed with a `#nullable disable` island
- [#372 email-classifier nullable patterns](project_372_email_classifier_nullable_patterns.md) — `null!` post-ctor props, `T?` factories, `.Class!` cascade
- [CS8714 does not fire on net481](project_nullable_cs8714_not_on_net481.md) — `where TKey : notnull` is forward-looking, not required
- [#366 notnull cascades past WrapperScoDictionary](project_366_notnull_cascades_beyond_wrapperscodictionary.md) — same constraint needed on ScoDictionaryConverter
- [#366 ScDictionary constraint cascades to a 4th file](project_366_scdictionary_constraint_cascades_to_fourth_file.md) — STOP + re-escalate, don't widen
- [#366 Batch7 T? return triggers CS8766](project_366_batch7_tnullable_return_cs8766.md) — conform to `T` + justified `!`, don't edit the interface
- [Nullable annotation CS8632 scoping](project_nullable_annotation_cs8632_scoping.md) — wrap in `#nullable enable annotations`, not whole-file
- [init/record struct fails CS0518 on net48](project_record_struct_isexternalinit_netfx.md) — no IsExternalInit; use a readonly struct with get-only props
- [Outlook `Action`/`Exception` ambiguity](project_outlook_action_ambiguity.md) — use `System.Action`/`System.Exception` in interop files

## Additional entries

- [Preflight: probe mandated C# shapes with csc](project_preflight_csc_probe_for_mandated_csharp_shapes.md) — vswhere-resolved Roslyn csc on a scratchpad file proves a dictated construct compiles
- [Preflight evidence fields need a token scan](project_preflight_evidence_field_token_scan.md) — prose "the `git diff ...` command" omits literal `Command:`; scan tokens + order, non-blocking
- [Plan literals inherit research arithmetic errors](project_plan_literal_assertions_inherit_research_arithmetic.md) — recompute every quoted literal, line-count projection, and "all N sites" count at preflight


- [Preflight fix tasks inherit the round's own rules](project_preflight_fix_tasks_inherit_decomposition_rules.md) — audit NEW tasks against all invariants, not just the finding they close
- [Conditional split = three tasks](project_conditional_split_three_task_shape.md) — measure / split / register, each with an authorized NO ACTION branch; never bundled
