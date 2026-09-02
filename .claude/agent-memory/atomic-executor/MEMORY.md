# Atomic Executor Memory Index

## Plan validation & gates
- [Blocked Bash drops chained check-off](project_blocked_bash_command_silently_drops_chained_checkoff.md) — aborts the chain
- [CSharpier chain-wrap defeats zero-hit gates](project_csharpier_chain_wrap_defeats_singleline_search_gates.md) — goes green early
- [Verify citations with numbered output](feedback_verify_line_citations_with_numbered_output.md) — never hand-count
- ["Observed while authoring" counts undercount](project_plan_authoring_time_token_counts_are_undercounts.md) — measure; 5v7, 36v37
- [Planner vs executor worktrees](project_planner_and_executor_observe_different_worktrees.md) — status doesn't travel
- [Extract gate literals, never retype](project_preflight_gate_literal_extract_from_plan_not_retype.md) — quoting drift
- [Tool layer collapses `\` in files](project_tool_layer_collapses_double_backslash_in_file_content.md) — in heredocs
- [Self-derived thresholds are blind](project_preflight_selfderived_gate_thresholds_are_blind.md) — derived from itself
- [Inline-dispatch harness voids a test](project_inline_dispatch_harness_citation_makes_execution_time_test_vacuous.md) — check dispatch mode
- [Multi-pattern gates detach qualifiers](project_multipattern_gate_shared_qualifier_detachment.md) — detaches silently
- [Follow-up promotion, unexecutable](project_followup_promotion_task_is_unexecutable_by_executor.md) — trips on "promotion"
- [Check-off fixpoint breaks the gate](project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate.md) — flip boxes pre-commit
- [Merge-base diff needs commit cadence](project_preflight_mergebase_diff_gates_need_commit_cadence.md) — vacuous at HEAD==base
- [Moving-base two-dot diff: test inertness](project_preflight_moving_base_two_dot_diff_inertness_test.md) — only if delta hits paths
- [BASELINE_SHA diff conflates the base](project_baseline_sha_diff_conflates_merged_base.md) — use `<base>..HEAD`
- [Inserted tasks force renumbering](project_plan_task_ids_digit_only_forces_renumbering.md) — no suffixed IDs
- [Plan rationale clauses are evidence](project_418_plan_rationale_clauses_are_evidence.md) — blockers hide as prose
- [Bugfix phase grows the file](project_bugfix_phase_grows_the_file_despite_dead_code_removal.md) — net-reduction runs low
- [Banned-API zero-hit hits comments](project_banned_api_zero_hit_gate_hits_doc_comments.md) — measure stripped hits
- [AC check-off + tool-output paths](project_preflight_ac_checkoff_and_tooloutput_paths.md) — pointer vs. policy
- [Exact-count gate vs remediation loop](project_exact_count_gate_vs_remediation_loop.md) — pinned totals go stale
- [Output Summary breaks its own count gate](project_artifact_output_summary_breaks_its_own_exact_count_gate.md) — restates; breaks it
- [Tracked agent-memory breaks gates](project_agent_memory_tracked_breaks_unscoped_git_gates.md) — needs a pathspec
- [Absolute-zero gate vs sibling assembly](project_preflight_absolute_zero_gate_on_sibling_owned_assembly.md) — unsat under flakes
- [#418 500-line gate vs plan content](project_418_500line_gate_vs_plan_content.md) — 193 lines into 146
- [#207 Hook() redesign breaks AppEventsTests](project_207_hook_redesign_breaks_appeventstests.md) — ordering superseded
- [C2 capacity budget drifts mid-plan](project_c2_capacity_budget_drifts_mid_plan.md) — planning-time baseline
- [ApplicationGlobalsTests.cs at 500 lines](project_appglobalstests_at_500_line_ceiling.md) — extraction first
- [#376 capstone scope-expansion layers](project_376_capstone_scope_expansion_layers.md) — 5 layers, 3 authorized
- [Swordfish F5 test misclassification](project_swordfish_f5_test_misclassification.md) — check using/namespace
- [Confirmatory preflight](feedback_confirmatory_preflight_proportionate_bar.md) — block only regressions
- [Dir-scoped format breaks ownership](project_directory_scoped_format_breaks_ownership_gates.md) — rewrites locked files
- [Supersede clause leaves a residual](project_supersede_clause_leaves_hard_routing_residual.md) — misses a 3rd spot
- [Four recurring C# plan defect classes](project_preflight_recurring_csharp_plan_defect_classes.md) — omitted policy, +3
- [msbuild-log grep matches csc command line](project_msbuild_log_token_search_matches_csc_command_line.md) — byte-exact grep
- [Epic base invalidates line counts](project_epic_integration_base_invalidates_research_line_counts.md) — was 489, not 365
- ["Make citation exist" propagates false facts](project_preflight_citation_match_propagates_false_fact.md) — copies B's error to A
- [Check-off cites a LATER artifact](project_preflight_checkoff_cites_later_task_artifact.md) — unsat. in plan order
- ["Skip drain" voids a negative test](project_preflight_drain_scope_optimization_note_makes_test_vacuous.md) — claim unverified
- [Sanitisation can't sweep its own record](project_sanitisation_task_cannot_sweep_its_own_record.md) — one residual: its record
- [Conjunctive criteria break one-artifact](project_preflight_conjunctive_criterion_citation_gap.md) — breaks the AC
- [Pre-edit gate cites its own table](project_preedit_gate_cites_postedit_replacement_table.md) — asserts pre-edit strings
- [Caller-stated count drifts pre-execution](project_caller_stated_preflight_count_drifts_before_execution.md) — 42 vs 48 measured
- [Orchestrator override does not satisfy an AC](project_orchestrator_override_does_not_satisfy_an_ac.md) — proceeds; AC unsat.

## Build / toolchain environment
- [pwsh/git/gh CLI gotchas](project_pwsh_git_gh_cli_gotchas.md) — no jq; can't concat `..HEAD`
- [Project Build/Test Env](project_build_test_env.md) — MSBuild switches
- [Start-Process -ArgumentList strips quoting](project_startprocess_arglist_array_strips_quoting.md) — loses platform quote
- [VS18 build/test toolchain paths](project_vs18_build_toolchain_paths.md) — full-framework msbuild
- [Repo-local SDK install + nullable Rebuild](project_repo_sdk_and_nullable_rebuild.md) — needs pwsh7
- [QuickFiler.Test coverage hang](project_quickfiler_test_coverage_hang_and_build_flags.md) — testhost can hang
- [vstest TestCaseFilter needs `|` not OR](project_vstest_testcasefilter_or_operator_and_env_setup.md) — worktree needs SDK+NuGet
- [Test file name != partial class name](project_test_file_name_vs_partial_class_name.md) — filter matches 0
- [HintPath/version skew](project_analyzer_hintpath_skew_breaks_all_four_gates.md) / [fresh-worktree](project_analyzer_version_skew_fresh_worktree.md) — CS0006 from divergence
- [SecurityCodeScan vs Roslyn](project_securitycodescan_roslyn56_incompat.md) — CS8032/YamlDotNet
- [Missing VSTO runtime](project_missing_vsto_runtime_breaks_baseline_gates.md) — HISTORICAL; re-verify
- [New sln member surfaces MSB3277](project_new_sln_member_surfaces_msb3277_pin_divergence.md) — config divergence
- [Legacy csproj: no transitive refs](project_legacy_csproj_no_transitive_compile_refs.md) — CS0012, needs a Ref
- [sln/csproj edits: preserve CRLF](project_sln_csproj_edit_crlf_preserve.md) — sed strips CRLF
- [Relative paths hit wrong worktree](project_relative_path_in_pwsh_dotnet_io_hits_wrong_worktree.md) — Set-Location ≠ .
- [Incremental build: vacuous baseline](project_incremental_build_vacuous_baseline.md) / [nullable instance](project_nullable_build_gate_is_vacuous_incremental.md) — /t:Build ignores /p:; use `/t:Rebuild`
- [CSharpier skips *.Designer.cs by filename](project_csharpier_skips_designer_cs_by_filename.md) — generated-file skip
- [CSharpier 1.3.0 formats XML at 100 cols](project_csharpier_formats_xml_print_width.md) — "churn" is formatter-driven
- [Evidence <TS> collision clobbers files](project_evidence_timestamp_collision_clobbers_artifacts.md) — same-day <TS> collides
- [Evidence <TS> labels drift ahead of clock](project_evidence_timestamp_labels_drift_ahead_of_write_time.md) — beat the clock
- [csharpier pipe-files: non-enforcing gate](project_csharpier_pipefiles_nonenforcing_gate.md) — use `check`, not pipe
- [Count-idiom: csharpier + Measure-Object](project_count_idiom_pitfalls_csharpier_and_measureobject.md) — "Formatted N" ≠ rewritten
- [New .cs files force a restart](project_new_cs_files_guarantee_a_format_loop_restart.md) — Write emits LF not CRLF
- [PowerShell new files need UTF-8 BOM](powershell-bom-required.md) — PSUseBOMForUnicodeEncodedFile
- [poshqc test MCP: no verdict/numbers](project_poshqc_pester_mcp_exit_minus1.md) — use Invoke-Pester for counts
- [poshqc analyze exits 1 on Warning](project_poshqc_analyze_exit1_on_warning.md) — "0+0" self-contradicts
- [BOM breaks grep ^; grep also strips CR](project_bom_grep_anchor_false_negative.md) — ripgrep is safe
- [StrictMode + missing XML attr throws](project_pester_strictmode_xml_attribute_property_access.md) — enumerate ALL cases
- [Pester 5 result shape](project_pester5_result_shape_container_tests_and_ci_codecoverage.md) — use TotalCount; `-CI`
- [Bash heredoc collapses `\\` to `\`](project_bash_heredoc_collapses_doubled_backslashes.md) — even in a heredoc
- [Unquoted bash-ARG backslash redirects](project_unquoted_backslash_in_bash_arg_silently_redirects_output.md) — silent wrong path
- [Recursive delete: both idioms blocked](project_recursive_delete_idioms_blocked_use_dotnet_api.md) — use `Directory::Delete`
- [Doubled backslash de-doubles to native exe](project_doubled_backslash_dedoubles_bash_to_native_exe.md) — → forward-slash-only
- [pwsh -Command quoting + backtick stripping](project_pwsh_command_quoting_from_bash.md) — single-quote OUTER
- [pwsh -File binds a list as ONE string](project_pwsh_file_array_param_from_bash.md) — `-Tokens` binds ONE string
- [Compile-time red needs body refs](project_compile_red_needs_body_level_references.md) — a signature hides more
- [Cross-task variable splat gates](project_cross_task_shell_variable_splat_gate.md) — needs same-payload rerun
- [Shared evidence + floating <ts>](project_shared_evidence_artifact_floating_ts.md) — can split at rollover

## Test execution & isolation
- [Long runs need a detached process](project_long_runs_need_detached_process.md) — die at ~1h
- [Tests must mock GUI; no visible window](feedback_tests_must_mock_gui_no_visible_window.md) — headless seams only
- [#511 is a test-host crash, not N failures](project_511_is_a_testhost_crash_not_n_failing_tests.md) — `Unknown`, not N
- [WinFormsPumpHost tests are load-flaky](project_winformspumphost_tests_load_flaky.md) — handle/timeout flaky
- [vstest /InIsolation + FilePathHelper](project_vstest_isolation_and_filepathhelper_serialization.md) — needs /InIsolation
- [Invoke-MSTest.ps1 dies on one assembly](project_418_invoke_mstest_single_assembly_bug.md) — `.Count` on a scalar
- [Timed-out MSTest leaves a runner](project_timedout_mstest_leaves_detached_runner.md) — kill it
- [Sibling-worktree](project_sibling_worktree_shared_tooling_hazard.md) / [deadlock](project_concurrent_dotnet_coverage_deadlock_and_doccomment_retention_gate.md) — kill your own chain
- [Concurrent executor in same worktree](project_concurrent_executor_same_worktree.md) — detect via mtime
- [UtilitiesCS.Test parallelism flakes](project_utilitiescs_test_parallelism_flakiness.md) — lower MSTest Workers
- [[DoNotParallelize] overlaps the bucket](project_mstest_donotparallelize_overlaps_parallel_bucket.md) — mark every writer
- [log4net MemoryAppender per TYPE](project_log4net_memoryappender_shared_per_type_across_parallel_classes.md) — cross-class bleed
- [UiThread.Dispatcher static-swap race](project_uithread_dispatcher_static_swap_race.md) — serialize swap-restore
- [runsettings DataCollector default-on](project_runsettings_datacollector_default_enabled.md) — activates w/o /collect
- [dotnet-coverage Deedle/FSharp breaks](project_dotnet_coverage_deedle_fsharp_instrumentation.md) — pass module-exclude
- [DispatcherDelay hangs unit tests](project_dispatcherdelay_hangs_unit_tests.md) — WaitAsync never completes
- [ConfigController STA pump deadlock](project_configcontroller_sta_pump_deadlock.md) — must pump (DoEvents)

## Coverage measurement
- [Exempt-forward leaves call uncovered](project_exempt_forward_extraction_leaves_call_site_uncovered.md) — >=90% unsat.
- [Reproduce baseline's counting method](project_coverage_delta_reproduce_baseline_counting_method.md) — deduped vs all-descendant
- [Async state machine emits no `<method>`](project_async_state_machine_emits_no_method_element.md) — union empty; double-counts
- [First-party denom. (#197)](project_coverage_firstparty_denominator_method.md) / [conversion](project_csharp_canonical_coverage_artifact_conversion.md) — reads JaCoCo
- [dotnet-coverage denominator nondeterminism](project_dotnet_coverage_denominator_nondeterminism.md) — double-counts; use git-stash
- [Koverage Cobertura post-processing shape](project_koverage_cobertura_postprocessing_shape.md) — pass = processed
- [Cobertura runsettings `<Attributes>` override](project_cobertura_runsettings_attributes_override.md) — disables excludes
- [Cobertura hits vs MS-coverage partial](project_changed_line_coverage_cobertura_vs_mscoverage_partial.md) — hit=1 in Cobertura
- [QFC #227 coverage tooling](project_qfc227_coverage_tooling.md) — vstest+Cobertura reliable
- [#398 test-split gate gotchas](project_398_test_split_gate_gotchas.md) — latent CS2002; no branch%
- [ExcludeFromCodeCoverage on partial = CS0579](project_excludefromcodecoverage_partial_class_cs0579.md) — annotate type once
- Closed one-offs: [#400](project_400_completeopenasync_unreachable_recovery_catch.md), [Swordfish](project_swordfish_removal_epic_incidental_coverage_sideeffect.md), [#298](project_taskvis_scocollection_and_livebridge_exemptions.md), [#328](project_328_rebuild_threading_olobjectsproxy_conflict.md)

## Nullable / C# language
- [Nullable pragma mechanics](project_nullable_pragma_gate_mechanics.md) / [cmd≠CI gate](project_507_nullconditional_return_triggers_cs8603_under_genuine_nullable_check.md) — CS86xx isn't a blocker
- [Nullable remediation annotation patterns](project_nullable_remediation_annotation_patterns.md) — no post-condition attr
- [Nullable annotation CS8632 scoping](project_nullable_annotation_cs8632_scoping.md) — wrap in `enable annotations`
- [init/record struct fails CS0518 on net48](project_record_struct_isexternalinit_netfx.md) — use a readonly struct
- [CS8714 does not fire on net481](project_nullable_cs8714_not_on_net481.md) — `notnull` doesn't fire
- Nullable epic (closed): [#366a](project_366_notnull_cascades_beyond_wrapperscodictionary.md), [#366b](project_366_scdictionary_constraint_cascades_to_fourth_file.md), [#366c](project_366_batch7_tnullable_return_cs8766.md), [#371](project_371_outlookobjects_nullable_lessons.md), [#372](project_372_email_classifier_nullable_patterns.md), [#375](project_375_residuals_nullable_gotchas.md)
- [Outlook `Action`/`Exception` ambiguity](project_outlook_action_ambiguity.md) — CS0104 in interop files

## Component-specific gotchas
- [WebView2 EndInit creates handles](project_webview2_endinit_creates_handles.md) / [#349 breadcrumb](project_349_breadcrumb_webview2_gotchas.md) — `new ItemViewer()`; Designer field breaks it
- QFC227: [cycle-4 ToggleFocus](project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md), [cycle-3 seam](project_theme_folderpredictor_seam_retrofit_gotchas.md)
- [ObjectListView headless selection](project_objectlistview_treelistview_headless_selection.md) — cache via SelectionChanged
- [QfcDatamodel BackgroundWorker async-void race](project_qfc_backgroundworker_async_void_race.md) — IsBusy flips false early
- [QfcItemController needs SaveParameters](project_qfcitemcontroller_pump_harness_needs_saveparameters.md) — `??=` defaults stay null
- [TaskController (#297) unit-test gotchas](project_taskvisualization_taskcontroller_test_gotchas.md) — ApplyChanges hangs over Moq
- [ProjectEntry setter raw MessageBox](project_projectentry_setter_raw_messagebox.md) — un-seamed MessageBox.Show
- [IApplicationGlobals forces implementers](project_iapplicationglobals_member_forces_implementers.md) — breaks 7
- [TimeProvider seam gotchas](project_timeprovider_seam_gotchas.md) — can't mock GetLocalNow
- [Initializer.GetOrLoad discards injection](project_initializer_getorload_discards_injection_when_dependency_null.md) — returns default(T)
- [ScoDictionaryNew needs TryAdd not Add](project_scodictionarynew_tryadd_not_add.md) — `.Add(k,v)` won't compile
- [FluentAssertions Equal(params) has no because](project_fluentassertions_equal_params_no_because.md) — becomes extra element

## Artifact hygiene
- [Never embed absolute host paths](../_shared_no_absolute_host_paths.md) — no account/machine name
- [Self-test probe trips the sweep pass](project_selftest_probe_literal_trips_the_next_sweep_pass.md) — describe, don't quote
- [TRX sanitisation needs case-insensitivity](project_trx_sanitisation_must_be_case_insensitive.md) — `storage=` is lower-case
- [TRX/msbuild evidence needs sanitisation](project_vstest_trx_evidence_needs_sanitisation_task.md) — no plan budgets it
- [PS budget hook blocks scratch helpers](project_powershell_scratch_script_budget_hook_blocks_helpers.md) — cap counts sessions
