# Atomic Executor Memory Index

## Plan validation & gates
- [Mid-plan commit needs a capture-time sanitisation gate](project_midplan_commit_needs_capture_time_sanitisation_gate.md) — a final whole-tree sweep cannot reach an earlier commit
- [Blocked Bash command drops chained check-off](project_blocked_bash_command_silently_drops_chained_checkoff.md) — aborts the WHOLE command
- [CSharpier chain-wrap defeats single-line search gates](project_csharpier_chain_wrap_defeats_singleline_search_gates.md) — zero-hit gate
- [Verify line citations with numbered output](feedback_verify_line_citations_with_numbered_output.md) — never hand-count
- [Plan "observed while authoring" counts are undercounts](project_plan_authoring_time_token_counts_are_undercounts.md) — measure; 5 vs 7, 36 vs 37
- [Planner/executor see different worktrees](project_planner_and_executor_observe_different_worktrees.md) · [Caller-stated preflight count drifts](project_caller_stated_preflight_count_drifts_before_execution.md) — claims don't travel; 42 vs 48
- [Extract gate literals from the plan, never re-type](project_preflight_gate_literal_extract_from_plan_not_retype.md) · [Tool layer collapses `\`](project_tool_layer_collapses_double_backslash_in_file_content.md)
- [Self-derived gate thresholds are blind](project_preflight_selfderived_gate_thresholds_are_blind.md) · [Exact-count gate vs remediation loop](project_exact_count_gate_vs_remediation_loop.md) — a pinned total blocks the fix
- [Inline-dispatch harness citation makes a test vacuous](project_inline_dispatch_harness_citation_makes_execution_time_test_vacuous.md) · ["Skip the pointless drain" note](project_preflight_drain_scope_optimization_note_makes_test_vacuous.md)
- [Multi-pattern gates detach shared qualifiers](project_multipattern_gate_shared_qualifier_detachment.md) · [Banned-API zero-hit gate hits doc comments](project_banned_api_zero_hit_gate_hits_doc_comments.md)
- [Follow-up promotion task is unexecutable](project_followup_promotion_task_is_unexecutable_by_executor.md) · [Supersede clause leaves a routing residual](project_supersede_clause_leaves_hard_routing_residual.md)
- [Plan check-off fixpoint breaks clean-tree gates](project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate.md) · [Tracked agent-memory breaks unscoped git gates](project_agent_memory_tracked_breaks_unscoped_git_gates.md)
- [Merge-base diff gates need a commit cadence](project_preflight_mergebase_diff_gates_need_commit_cadence.md) · [BASELINE_SHA diff conflates the merged base](project_baseline_sha_diff_conflates_merged_base.md) — use `<base>..HEAD`
- [Moving-base two-dot diff needs an inertness test](project_preflight_moving_base_two_dot_diff_inertness_test.md) — only blocking if the delta actually hits those paths
- [Inserted plan tasks force renumbering](project_plan_task_ids_digit_only_forces_renumbering.md) · [Plan rationale clauses are evidence](project_418_plan_rationale_clauses_are_evidence.md) — blockers hide as prose
- [Bugfix phase grows the file anyway](project_bugfix_phase_grows_the_file_despite_dead_code_removal.md) · [#418 500-line gate vs mandated plan content](project_418_500line_gate_vs_plan_content.md)
- [AC check-off + artifacts/ tool-output paths](project_preflight_ac_checkoff_and_tooloutput_paths.md) · [Orchestrator override does not satisfy an AC](project_orchestrator_override_does_not_satisfy_an_ac.md)
- [Artifact Output Summary breaks its own count gate](project_artifact_output_summary_breaks_its_own_exact_count_gate.md) · [Sanitisation task cannot sweep its own record](project_sanitisation_task_cannot_sweep_its_own_record.md)
- [Absolute-zero gate on a sibling-owned assembly](project_preflight_absolute_zero_gate_on_sibling_owned_assembly.md) · [Directory-scoped format breaks ownership gates](project_directory_scoped_format_breaks_ownership_gates.md)
- [#207 Hook() redesign breaks AppEventsTests](project_207_hook_redesign_breaks_appeventstests.md) · [C2 capacity budget drifts mid-plan](project_c2_capacity_budget_drifts_mid_plan.md)
- [ApplicationGlobalsTests.cs at 500-line ceiling](project_appglobalstests_at_500_line_ceiling.md) · [#376 capstone scope-expansion layers](project_376_capstone_scope_expansion_layers.md) — 5 layers, 3 authorized
- [Swordfish F5 test misclassification](project_swordfish_f5_test_misclassification.md) · [Confirmatory preflight: proportionate bar](feedback_confirmatory_preflight_proportionate_bar.md)
- [Four recurring C# plan defect classes](project_preflight_recurring_csharp_plan_defect_classes.md) · [msbuild-log grep matches the csc command line](project_msbuild_log_token_search_matches_csc_command_line.md)
- [Epic integration base invalidates research line counts](project_epic_integration_base_invalidates_research_line_counts.md) · ["Make the citation exist" propagates false facts](project_preflight_citation_match_propagates_false_fact.md)
- [Check-off cites an artifact a LATER task writes](project_preflight_checkoff_cites_later_task_artifact.md) · [Pre-edit gate cites the post-edit table](project_preedit_gate_cites_postedit_replacement_table.md)
- [Conjunctive criteria break the one-artifact citation rule](project_preflight_conjunctive_criterion_citation_gap.md) — "cites exactly one"

## Build / toolchain environment
- [pwsh/git/gh CLI gotchas](project_pwsh_git_gh_cli_gotchas.md) — no jq; pwsh won't concatenate `$(git merge-base`
- [Project Build/Test Env](project_build_test_env.md) — git-bash MSBuild switches, MSYS_NO_PATHCONV, csharpier v1
- [VS18 build/test toolchain paths](project_vs18_build_toolchain_paths.md) · [Repo-local SDK install + nullable Rebuild](project_repo_sdk_and_nullable_rebuild.md) — .dotnet-sdk needs pwsh7
- [Start-Process -ArgumentList strips quoting](project_startprocess_arglist_array_strips_quoting.md) · [Relative paths in pwsh hit the wrong worktree](project_relative_path_in_pwsh_dotnet_io_hits_wrong_worktree.md)
- [QuickFiler.Test coverage hang + build flags](project_quickfiler_test_coverage_hang_and_build_flags.md) — testhost can hang
- [vstest TestCaseFilter needs `|` not OR](project_vstest_testcasefilter_or_operator_and_env_setup.md) · [Test file name != partial class name](project_test_file_name_vs_partial_class_name.md)
- [Analyzer HintPath skew breaks all four gates](project_analyzer_hintpath_skew_breaks_all_four_gates.md) · [Analyzer version skew on fresh worktree](project_analyzer_version_skew_fresh_worktree.md) — CS0006
- [SecurityCodeScan incompatible with Roslyn 5.6](project_securitycodescan_roslyn56_incompat.md) · [Missing VSTO runtime breaks baseline gates](project_missing_vsto_runtime_breaks_baseline_gates.md) — HISTORICAL
- [New sln member surfaces MSB3277](project_new_sln_member_surfaces_msb3277_pin_divergence.md) · [Legacy csproj: no transitive compile refs](project_legacy_csproj_no_transitive_compile_refs.md) — CS0012
- [sln/csproj edits: preserve CRLF](project_sln_csproj_edit_crlf_preserve.md) — `sed -i` strips CRLF; use Edit
- [Incremental build makes a vacuous baseline](project_incremental_build_vacuous_baseline.md) · [Nullable /t:Build gate is vacuous](project_nullable_build_gate_is_vacuous_incremental.md) — use `/t:Rebuild`
- [CSharpier skips *.Designer.cs by filename](project_csharpier_skips_designer_cs_by_filename.md) · [CSharpier 1.3.0 formats XML at 100 cols](project_csharpier_formats_xml_print_width.md)
- [.gitignore `*.log` blocks committed msbuild-log evidence](project_gitignore_star_log_blocks_committed_msbuild_log_evidence.md) — exists-on-disk gate passes, commit lacks it; `git add -N` is the tracked-status discriminator
- [csharpier pipe-files is a non-enforcing gate](project_csharpier_pipefiles_nonenforcing_gate.md) · [Count-idiom pitfalls: csharpier + Measure-Object](project_count_idiom_pitfalls_csharpier_and_measureobject.md)
- [New .cs files guarantee a format-loop restart](project_new_cs_files_guarantee_a_format_loop_restart.md) · [PowerShell new files need UTF-8 BOM](powershell-bom-required.md)
- [poshqc test MCP carries no verdict/numbers](project_poshqc_pester_mcp_exit_minus1.md) · [poshqc analyze exits 1 on a Warning](project_poshqc_analyze_exit1_on_warning.md)
- [BOM breaks grep ^; grep also strips CR](project_bom_grep_anchor_false_negative.md) · [StrictMode + missing XML attribute throws](project_pester_strictmode_xml_attribute_property_access.md)
- [Pester 5 result shape](project_pester5_result_shape_container_tests_and_ci_codecoverage.md) — use TotalCount; `-CI`
- [Bash heredoc collapses `\\` to `\`](project_bash_heredoc_collapses_doubled_backslashes.md) · [Unquoted backslash in a bash ARG redirects output](project_unquoted_backslash_in_bash_arg_silently_redirects_output.md)
- [Doubled backslash de-doubles bash->native exe](project_doubled_backslash_dedoubles_bash_to_native_exe.md) — `[\\/]` becomes forward-slash-only
- [Recursive delete: both idioms blocked](project_recursive_delete_idioms_blocked_use_dotnet_api.md) — use `[System.IO.Directory]::Delete(p,$true)`
- [pwsh -Command quoting + backtick stripping](project_pwsh_command_quoting_from_bash.md) · [pwsh -File binds a list as ONE string](project_pwsh_file_array_param_from_bash.md)
- [Compile-time red needs body-level refs](project_compile_red_needs_body_level_references.md) · [Cross-task shell-variable splat gates](project_cross_task_shell_variable_splat_gate.md)
- [Evidence <TS> collision clobbers artifacts](project_evidence_timestamp_collision_clobbers_artifacts.md) · [Shared evidence artifact + floating <ts>](project_shared_evidence_artifact_floating_ts.md)

## Test execution & isolation
- [Long runs need a detached process](project_long_runs_need_detached_process.md) — background runners die at ~1h
- [Tests must mock GUI; no visible window](feedback_tests_must_mock_gui_no_visible_window.md) — headless seams, never a real window
- [Full-suite run hangs though the baseline passed](project_full_suite_run_hangs_while_earlier_runs_idle.md) — sample testhost CPU to prove hang vs slow
- [WinFormsPumpHost tests are load-flaky](project_winformspumphost_tests_load_flaky.md) · [#511 is a test-host crash, not N failures](project_511_is_a_testhost_crash_not_n_failing_tests.md)
- [vstest /InIsolation + FilePathHelper](project_vstest_isolation_and_filepathhelper_serialization.md) · [Invoke-MSTest.ps1 dies on one assembly](project_418_invoke_mstest_single_assembly_bug.md)
- [Timed-out MSTest leaves a detached runner](project_timedout_mstest_leaves_detached_runner.md) · [Sibling-worktree shared-tooling hazard](project_sibling_worktree_shared_tooling_hazard.md)
- [Concurrent dotnet-coverage deadlock](project_concurrent_dotnet_coverage_deadlock_and_doccomment_retention_gate.md) · [Concurrent executor in same worktree](project_concurrent_executor_same_worktree.md) — kill only your own
- [UtilitiesCS.Test parallelism flakiness](project_utilitiescs_test_parallelism_flakiness.md) · [[DoNotParallelize] overlaps the parallel bucket](project_mstest_donotparallelize_overlaps_parallel_bucket.md)
- [log4net MemoryAppender is shared per TYPE](project_log4net_memoryappender_shared_per_type_across_parallel_classes.md) · [UiThread.Dispatcher static-swap race](project_uithread_dispatcher_static_swap_race.md)
- [runsettings DataCollector default-enabled](project_runsettings_datacollector_default_enabled.md) · [dotnet-coverage Deedle/FSharp breaks tests](project_dotnet_coverage_deedle_fsharp_instrumentation.md)
- [DispatcherDelay hangs unit tests](project_dispatcherdelay_hangs_unit_tests.md) · [ConfigController STA pump deadlock](project_configcontroller_sta_pump_deadlock.md) — STA test must pump

## Coverage measurement
- [Exempt-forward extraction leaves call site uncovered](project_exempt_forward_extraction_leaves_call_site_uncovered.md) — >=90% gate unsatisfiable
- [Reproduce the baseline's counting method](project_coverage_delta_reproduce_baseline_counting_method.md) — deduped vs all-descendant
- [Async state machine emits no `<method>` element](project_async_state_machine_emits_no_method_element.md) — `.//line` double-counts
- [First-party coverage denominator (#197)](project_coverage_firstparty_denominator_method.md) · [dotnet-coverage denominator nondeterminism](project_dotnet_coverage_denominator_nondeterminism.md) — 47% vs 81%
- [Failed/red coverage run leaves RAW Cobertura](project_failed_coverage_run_leaves_raw_unprocessed_cobertura.md) · [runner throws before post-processing](project_coverage_runner_throws_before_postprocessing.md) · [Koverage post-processing shape](project_koverage_cobertura_postprocessing_shape.md) — not comparable with a processed one
- [C# canonical coverage artifact conversion](project_csharp_canonical_coverage_artifact_conversion.md) · [Cobertura runsettings `<Attributes>` override](project_cobertura_runsettings_attributes_override.md)
- [Cobertura hits vs MS-coverage partial](project_changed_line_coverage_cobertura_vs_mscoverage_partial.md) · [QFC #227 coverage tooling](project_qfc227_coverage_tooling.md)
- [#398 test-split gate gotchas](project_398_test_split_gate_gotchas.md) · [ExcludeFromCodeCoverage on partial = CS0579](project_excludefromcodecoverage_partial_class_cs0579.md)
- Closed one-offs: [#400](project_400_completeopenasync_unreachable_recovery_catch.md), [Swordfish](project_swordfish_removal_epic_incidental_coverage_sideeffect.md), [#298](project_taskvis_scocollection_and_livebridge_exemptions.md), [#328](project_328_rebuild_threading_olobjectsproxy_conflict.md)

## Nullable / C# language
- [Nullable per-file pragma gate mechanics](project_nullable_pragma_gate_mechanics.md) — CI passes EXIT 0 without it
- [CLAUDE.md nullable command != the CI gate](project_507_nullconditional_return_triggers_cs8603_under_genuine_nullable_check.md) — forced-flag CS86xx
- [Nullable remediation annotation patterns](project_nullable_remediation_annotation_patterns.md) · [Nullable annotation CS8632 scoping](project_nullable_annotation_cs8632_scoping.md)
- [init/record struct fails CS0518 on net48](project_record_struct_isexternalinit_netfx.md) · [CS8714 does not fire on net481](project_nullable_cs8714_not_on_net481.md)
- [Outlook `Action`/`Exception` ambiguity](project_outlook_action_ambiguity.md) — CS0104 in Outlook-interop files
- Nullable-epic (closed): [#366a](project_366_notnull_cascades_beyond_wrapperscodictionary.md), [#366b](project_366_scdictionary_constraint_cascades_to_fourth_file.md), [#366c](project_366_batch7_tnullable_return_cs8766.md), [#371](project_371_outlookobjects_nullable_lessons.md), [#372](project_372_email_classifier_nullable_patterns.md), [#375](project_375_residuals_nullable_gotchas.md)

## Component-specific gotchas
- [WebView2 EndInit already creates child handles](project_webview2_endinit_creates_handles.md) · [#349 breadcrumb WebView2 gotchas](project_349_breadcrumb_webview2_gotchas.md)
- QFC #227: [cycle-4 ToggleFocus](project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md) · [cycle-3 seam](project_theme_folderpredictor_seam_retrofit_gotchas.md)
- [ObjectListView TreeListView headless selection](project_objectlistview_treelistview_headless_selection.md) · [QfcDatamodel BackgroundWorker async-void race](project_qfc_backgroundworker_async_void_race.md)
- [QfcItemController harness needs SaveParameters](project_qfcitemcontroller_pump_harness_needs_saveparameters.md) · [TaskController (#297) unit-test gotchas](project_taskvisualization_taskcontroller_test_gotchas.md)
- [ProjectEntry setter raw MessageBox](project_projectentry_setter_raw_messagebox.md) · [IApplicationGlobals member forces implementers](project_iapplicationglobals_member_forces_implementers.md)
- [TimeProvider seam gotchas](project_timeprovider_seam_gotchas.md) · [Initializer.GetOrLoad discards setter injection](project_initializer_getorload_discards_injection_when_dependency_null.md)
- [ScoDictionaryNew needs TryAdd not Add](project_scodictionarynew_tryadd_not_add.md) · [FluentAssertions Equal(params) has no because](project_fluentassertions_equal_params_no_because.md)

## Artifact hygiene
- [Never embed absolute host paths](../_shared_no_absolute_host_paths.md) — no account or machine name in ANY artifact
- [Never predict an observation into an artifact](feedback_never_predict_an_observation_into_an_artifact.md) — placeholder, commit, observe, append
- [Evidence <TS> labels drift ahead of write time](project_evidence_timestamp_labels_drift_ahead_of_write_time.md) — call `date`, don't increment
- [Self-test probe literal trips the NEXT sweep pass](project_selftest_probe_literal_trips_the_next_sweep_pass.md) — describe probes, never quote them
- [TRX sanitisation must be case-insensitive](project_trx_sanitisation_must_be_case_insensitive.md) · [TRX/msbuild evidence needs a sanitisation micro-action](project_vstest_trx_evidence_needs_sanitisation_task.md)
- [PowerShell budget hook blocks scratch .ps1 helpers](project_powershell_scratch_script_budget_hook_blocks_helpers.md) — cap counts others
