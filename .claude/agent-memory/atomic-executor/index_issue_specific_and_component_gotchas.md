---
name: index-issue-specific-and-component-gotchas
description: Sub-index of issue-specific (mostly closed) lessons and component-specific C# gotchas, split out of MEMORY.md to keep the main index under its size limit
metadata:
  type: reference
---

Sub-index. Each line points to one memory file in this folder.

## Issue-specific lessons (mostly closed issues)
- [#207 Hook() breaks AppEventsTests](project_207_hook_redesign_breaks_appeventstests.md)
- [C2 capacity budget drifts](project_c2_capacity_budget_drifts_mid_plan.md)
- [AppGlobalsTests at the 500-line ceiling](project_appglobalstests_at_500_line_ceiling.md)
- [#376 scope-expansion layers](project_376_capstone_scope_expansion_layers.md)
- [Swordfish F5 misclassification](project_swordfish_f5_test_misclassification.md)
- [#418 rationale clauses are evidence](project_418_plan_rationale_clauses_are_evidence.md)
- [#418 500-line gate vs plan content](project_418_500line_gate_vs_plan_content.md)
- [#418 Invoke-MSTest.ps1 dies on one assembly](project_418_invoke_mstest_single_assembly_bug.md)
- [#511 is a test-host crash](project_511_is_a_testhost_crash_not_n_failing_tests.md)
- [#398 test-split gate gotchas](project_398_test_split_gate_gotchas.md)
- [#364 nullable pre-existing blockers](project_364_nullable_gate_preexisting_blockers.md)
- [QFC #227 coverage tooling](project_qfc227_coverage_tooling.md)
- Coverage one-offs: [#400](project_400_completeopenasync_unreachable_recovery_catch.md), [Swordfish](project_swordfish_removal_epic_incidental_coverage_sideeffect.md), [#298](project_taskvis_scocollection_and_livebridge_exemptions.md), [#328](project_328_rebuild_threading_olobjectsproxy_conflict.md)
- Nullable epic: [#366a](project_366_notnull_cascades_beyond_wrapperscodictionary.md), [#366b](project_366_scdictionary_constraint_cascades_to_fourth_file.md), [#366c](project_366_batch7_tnullable_return_cs8766.md), [#371](project_371_outlookobjects_nullable_lessons.md), [#372](project_372_email_classifier_nullable_patterns.md), [#375](project_375_residuals_nullable_gotchas.md)

## C# coverage measurement
- [Exempt-forward extraction leaves the call site uncovered](project_exempt_forward_extraction_leaves_call_site_uncovered.md)
- [Async state machine emits no method element](project_async_state_machine_emits_no_method_element.md)
- [Reproduce the baseline's counting method](project_coverage_delta_reproduce_baseline_counting_method.md)
- [First-party denominator (#197)](project_coverage_firstparty_denominator_method.md)
- [dotnet-coverage denominator nondeterminism](project_dotnet_coverage_denominator_nondeterminism.md)
- [Failed run leaves RAW Cobertura](project_failed_coverage_run_leaves_raw_unprocessed_cobertura.md)
- [Runner throws before post-processing](project_coverage_runner_throws_before_postprocessing.md)
- [Koverage post-processing shape](project_koverage_cobertura_postprocessing_shape.md)
- [C# canonical coverage conversion](project_csharp_canonical_coverage_artifact_conversion.md)
- [Cobertura runsettings Attributes override](project_cobertura_runsettings_attributes_override.md)
- [Package rollup must use the repo helper](project_cobertura_package_rollup_must_use_repo_helper.md)
- [Processed Cobertura names use backslashes](project_processed_cobertura_filenames_use_backslash.md)
- [Cobertura hits vs MS-coverage partial](project_changed_line_coverage_cobertura_vs_mscoverage_partial.md)
- [ExcludeFromCodeCoverage on partial = CS0579](project_excludefromcodecoverage_partial_class_cs0579.md)

## Component-specific C# gotchas
- [WebView2 EndInit creates child handles](project_webview2_endinit_creates_handles.md)
- [#349 breadcrumb WebView2](project_349_breadcrumb_webview2_gotchas.md)
- [QFC #227 cycle-4 ToggleFocus](project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md)
- [QFC #227 cycle-3 seam](project_theme_folderpredictor_seam_retrofit_gotchas.md)
- [ObjectListView headless selection](project_objectlistview_treelistview_headless_selection.md)
- [BackgroundWorker async-void race](project_qfc_backgroundworker_async_void_race.md)
- [QfcItemController needs SaveParameters](project_qfcitemcontroller_pump_harness_needs_saveparameters.md)
- [TaskController (#297)](project_taskvisualization_taskcontroller_test_gotchas.md)
- [ProjectEntry setter raw MessageBox](project_projectentry_setter_raw_messagebox.md)
- [IApplicationGlobals member forces implementers](project_iapplicationglobals_member_forces_implementers.md)
- [TimeProvider seam gotchas](project_timeprovider_seam_gotchas.md)
- [GetOrLoad discards setter injection](project_initializer_getorload_discards_injection_when_dependency_null.md)
- [ScoDictionaryNew needs TryAdd](project_scodictionarynew_tryadd_not_add.md)
- [FluentAssertions Equal(params) has no because](project_fluentassertions_equal_params_no_because.md)
- [FluentAssertions BeEmpty names only the first item](project_fluentassertions_beempty_names_only_first_item.md)
- [WinForms control field installs SyncContext](project_winforms_control_field_installs_synccontext_and_deadlocks_await.md)
- [Outlook Action/Exception CS0104](project_outlook_action_ambiguity.md)
- [CS1769 forces reflection](project_cs1769_forces_reflection_for_outlook_returning_apis.md)
