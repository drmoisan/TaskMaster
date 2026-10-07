# Sub-index: Test execution, isolation and coverage measurement

## Test execution & isolation
- [Long runs need a detached process](project_long_runs_need_detached_process.md) · [Tests must mock GUI](feedback_tests_must_mock_gui_no_visible_window.md)
- [Full-suite run hangs though baseline passed](project_full_suite_run_hangs_while_earlier_runs_idle.md) · [#511 is a test-host crash](project_511_is_a_testhost_crash_not_n_failing_tests.md)
- [WinFormsPumpHost tests are load-flaky](project_winformspumphost_tests_load_flaky.md) · [vstest /InIsolation + FilePathHelper](project_vstest_isolation_and_filepathhelper_serialization.md)
- [Invoke-MSTest.ps1 dies on one assembly](project_418_invoke_mstest_single_assembly_bug.md) · [Timed-out MSTest leaves a runner](project_timedout_mstest_leaves_detached_runner.md)
- [Sibling-worktree shared-tooling hazard](project_sibling_worktree_shared_tooling_hazard.md) · [Concurrent executor in one worktree](project_concurrent_executor_same_worktree.md)
- [Concurrent dotnet-coverage deadlock](project_concurrent_dotnet_coverage_deadlock_and_doccomment_retention_gate.md) · [UtilitiesCS.Test parallelism flakiness](project_utilitiescs_test_parallelism_flakiness.md)
- [[DoNotParallelize] overlaps the parallel bucket](project_mstest_donotparallelize_overlaps_parallel_bucket.md) · [log4net MemoryAppender shared per TYPE](project_log4net_memoryappender_shared_per_type_across_parallel_classes.md)
- [UiThread.Dispatcher static-swap race](project_uithread_dispatcher_static_swap_race.md) · [runsettings DataCollector default-enabled](project_runsettings_datacollector_default_enabled.md)
- [dotnet-coverage Deedle/FSharp breaks tests](project_dotnet_coverage_deedle_fsharp_instrumentation.md) · [DispatcherDelay hangs unit tests](project_dispatcherdelay_hangs_unit_tests.md)
- [ConfigController STA pump deadlock](project_configcontroller_sta_pump_deadlock.md)

## Coverage measurement
- [Exempt-forward extraction leaves the call site uncovered](project_exempt_forward_extraction_leaves_call_site_uncovered.md) · [Async state machine emits no `<method>`](project_async_state_machine_emits_no_method_element.md)
- [Reproduce the baseline's counting method](project_coverage_delta_reproduce_baseline_counting_method.md) · [First-party denominator (#197)](project_coverage_firstparty_denominator_method.md)
- [dotnet-coverage denominator nondeterminism](project_dotnet_coverage_denominator_nondeterminism.md) · [Failed run leaves RAW Cobertura](project_failed_coverage_run_leaves_raw_unprocessed_cobertura.md)
- [Runner throws before post-processing](project_coverage_runner_throws_before_postprocessing.md) · [Koverage post-processing shape](project_koverage_cobertura_postprocessing_shape.md)
- [C# canonical coverage conversion](project_csharp_canonical_coverage_artifact_conversion.md) · [Cobertura runsettings `<Attributes>` override](project_cobertura_runsettings_attributes_override.md)
- [Package rollup must use the repo helper](project_cobertura_package_rollup_must_use_repo_helper.md) · [Processed Cobertura names use backslashes](project_processed_cobertura_filenames_use_backslash.md)
- [Cobertura hits vs MS-coverage partial](project_changed_line_coverage_cobertura_vs_mscoverage_partial.md) · [QFC #227 coverage tooling](project_qfc227_coverage_tooling.md)
- [#398 test-split gate gotchas](project_398_test_split_gate_gotchas.md) · [ExcludeFromCodeCoverage on partial = CS0579](project_excludefromcodecoverage_partial_class_cs0579.md)
- Closed one-offs: [#400](project_400_completeopenasync_unreachable_recovery_catch.md), [Swordfish](project_swordfish_removal_epic_incidental_coverage_sideeffect.md), [#298](project_taskvis_scocollection_and_livebridge_exemptions.md), [#328](project_328_rebuild_threading_olobjectsproxy_conflict.md)
