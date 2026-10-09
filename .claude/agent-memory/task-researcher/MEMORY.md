# Task Researcher Memory Index

## Sub-indexes
- [epic136-percoverage-children](index_epic136_percoverage_children.md) — 35 QuickFiler/EFC per-file coverage child notes (430-495, 227)
- [quickfiler-efc-defects](index_quickfiler_efc_defects.md) — 40 QuickFiler/EFC/SortEmail defect and behaviour notes

## Hygiene, feedback, references
- [no-absolute-host-paths](../_shared_no_absolute_host_paths.md) — use `<repo-root>`/`<user>`/`<host>`; rename TRX before citing
- [exemption-audit-proven-techniques](feedback_exemption_audit_check_proven_techniques.md) — grep proven techniques + sibling consistency before IRREDUCIBLE
- [measure-item-worktree](feedback_measure_item_worktree_not_session_worktree.md) — read facts from the ITEM worktree; uniform off-by-N = stale tree
- [committed-cobertura-baselines](reference_committed_cobertura_baselines.md) — per-line coverage in docs/features/*/evidence/*.cobertura.xml
- [net481-timeprovider](reference_net481_timeprovider_available.md) — FakeTimeProvider usable on net481; reject "net8+ only"
- [github-issue-search-without-gh](reference_github_issue_search_without_gh.md) — WebFetch issues?q=...; mark [V-web]
- [nuget-flatcontainer-nuspec](reference_nuget_flatcontainer_nuspec_webfetch.md) — packages/ has no .nuspec; fetch api.nuget.org flat-container nuspec

## Dependencies / Dependabot / packages.config
- [dependabot-repair-script-source-985](project_dependabot_repair_workflow_run_script_source_985.md) — repair runs branch's script; YAML edits can't green-run; redirect invariant is a union
- [binding-redirect-drift-973](project_binding_redirect_drift_and_missing_asyncenumerable_973.md) — NuGet rewrites only installer's config; AsyncEnumerable aliased
- [graph-usings-973](project_graph_usings_removable_and_models_name_collisions_973.md) — all 6 Graph usings removable; Models name collisions
- [ixnet-v7-lib-ref-clash](project_ixnet_v7_lib_ref_clash_packages_config.md) — lib/net48 public AsyncEnumerable; global BCL ref = CS0121; use `<Aliases>`
- [fsharp-hintpath-skew-895](project_fsharp_core_hintpath_skew_895.md) — 15 output dirs; ns2.1>2.0 ranking; ToDoModel.Test gap
- [dependabot-net481-340](project_dependabot_net481_340.md) — semver-major ignore, no fabricated ceilings
- [svgcontrol-test-418](project_svgcontrol_test_unwired_418.md) — STALE (in .sln since 2026-08-14)

## Threading / test determinism
- [taskrun-getresult-inlines-900](project_taskrun_getresult_inlines_on_pool_thread_900.md) — Task.Run+GetResult on a pool thread INLINES
- [taskrun-triage-931](project_taskrun_triage_931.md) — 2/22 Task.Run sites thread-dependent; OpenRead returns FileStream
- [prime-marker-944](project_prime_marker_register_before_start_944.md) — ContinueWith(None) never inlines; TCS marker before start
- [engine-toggle-prime-log-order-942](project_engine_toggle_prime_fault_log_order_942.md) — CompletePrime removes marker BEFORE log
- [engine-toggle-fault-suppression-948](project_engine_toggle_fault_suppression_948.md) — pressed cache never clears; first fault can be NRE
- [transactiongate-probe-882](project_transactiongate_parallel_safe_probe_882.md) — zero-bound probes assert only failure while holding
- [pump-timeout-743](project_pump_timeout_743.md) — dispatcher-gate lead stale; TRX timestamps as instrument
- [uithread-restore-493](project_uithread_dispatcher_restore_scope_493.md) — never one semaphore for helper+fixture
- [winforms-pump-seam-230](project_winforms_pump_seam_230.md) — WinFormsPumpHost; CreateAsync factory gap
- [onedrive-timeout-253](project_onedrive_timeout_test_determinism_253.md) — catches TimeoutException not TCE
- [unobserved-task-fault-670](project_unobserved_task_fault_670.md) — ViewerSetup.cs 499/500; raw WPF Dispatcher
- [filerqueue-consumer-633](project_filerqueue_consumer_unsound_633.md) — orphaned-item race; BackGroundMove tests vacuous
- [terminal-hook-barrier-751](project_terminal_hook_barrier_751.md) — `run.Terminal` is the barrier
- [analyzer-severity-runsettings](project_analyzer_severity_ceiling_and_runsettings_split.md) — MSTEST0032 only rule above suggestion
- [lock-recursion-317](project_lock_recursion_coverage_317.md) — deleted LockRecursionTests.cs is a restoration
- [gettableinviewasync-838](project_gettableinviewasync_null_contract_838.md) — `maxAttempts:1` = TWO attempts; OCE is not TCE

## Coverage / Cobertura mechanics
- [cobertura-closure-exemption-457](project_cobertura_closure_exemption_457.md) — exempt members emit NO `<method>`; async `d__` trap
- [cobertura-root-attrs](project_cobertura_root_attrs_raw_vs_postprocessed.md) — raw vs post-processed root totals; never compare
- [double-count-815](project_cobertura_double_count_moves_counts_not_rates_815.md) — `.//line` doubles counters, pct <=0.46pp
- [coverage-threshold-494](project_coverage_threshold_reconciliation_494.md) — 85/75 is foreign leakage; gate evadable
- [cobertura-perfile-attribution](project_cobertura_perfile_attribution_contract.md) — `line-rate` attr inflated; recompute
- [cobertura-line-double-count](project_cobertura_line_double_count.md) — lines-valid ~2x; recompute per-file
- [cobertura-exemption-gotchas](project_cobertura_exemption_and_branchrate_gotchas.md) — method-level exclude misses lambdas
- [qfc455-lambda-leak](project_qfc455_exclude_attribute_lambda_leak.md) — #441 inflation = per-method `<lines>` block
- [webview2-exemption-asymmetry](project_webview2_exemption_and_coverage_asymmetry.md) — class-level exclude hides lambdas
- [partial-type-exclusion-456](project_partial_type_coverage_exclusion_456.md) — type-level exclusion hides Designer partials
- [winforms-designer-coverage](project_winforms_designer_coverage_mechanics.md) — one construction covers ~99% of Designer
- [itemviewer-partial-coupling](project_itemviewer_partial_exemption_coupling.md) — ItemViewer.cs:20 hides 6 partials
- [iqfcdatamodel-contract-436](project_iqfcdatamodel_contract_436.md) — no class element for interfaces/enums
- [interface-only-files-433](project_quickfiler_interface_only_files_433.md) — interface-only .cs absent from Cobertura
- [interface-only-bucket](project_quickfiler_per_file_coverage_interface_only_bucket.md) — `interface-only` ledger bucket
- [percoverage-epic-136](project_quickfiler_percoverage_epic_136.md) — read per-file line-rate from committed Cobertura
- [coverage-ledger-432](project_quickfiler_coverage_ledger_432.md) — 121 files; 40 usages/21 files
- [capstone-f16](project_quickfiler_capstone_f16_measurement.md) — `<sources>` discriminates raw vs post-processed
- [f9-measurement-441](project_qfc_f9_measurement_441_and_designer_inheritance.md) — #441 corrupts per-file line-rate

## Stores / watchdog / add-in lifecycle
- [store-runtime-reenable-263](project_store_runtime_reenable_263.md) — no per-store hookup seam
- [store-lockup-f4](project_store_lockup_resilience_f4_research.md) — AsyncLocal rejected; MyBox no modeless
- [stores-enum-stall-292](project_stores_enum_stall_292.md) — watchdog crash on null model
- [storewrapper-dialog-287](project_storewrapper_dialog_287_state_inversion.md) — issue inverts the two states

## Ribbon, CI, toolchain, repo structure
- [ribbon-readiness-503](project_ribbon_engine_readiness_503.md) — Ribbon coverage-excluded; 5 orphan onAction callbacks
- [ribbon-toggle-guards-505](project_ribbon_toggle_state_guards_505.md) — toggle vs command guard asymmetry
- [ribbon-toggle-defects-735](project_ribbon_engine_toggle_defects_735.md) — 84 callbacks (5 dead); ManagerAsyncLazy constructible
- [ci-parallel-split-553](project_ci_parallel_split_553.md) — 4 independent jobs; check names "caller / callee"
- [toolchain-gate-fidelity-512](project_toolchain_gate_fidelity_512.md) — ~1.2s vs ~17s = vacuity
- [console-out-rs0030-826](project_console_out_and_rs0030_promotion_826.md) — CS0169/CS0414 hazard; IDE0005 silent
- [host-identifier-sweep-602](project_host_identifier_sweep_602.md) — PQ setting no var substitution; rg-vs-git deltas
- [evidence-identity-sweep-927](project_evidence_identity_hygiene_sweep_927.md) — classify raw evidence by XML root
- [push-down-claude-dir-149](project_push_down_claude_dir.md) — #149 pushDownClaudeDir research (2026-04-16)
- [winforms-testability-298](project_winforms_testability_epic_298.md) — #298 depends on #297; inverts #197 exemptions
- [tagcontroller-293](project_tagcontroller_refactor_293.md) — ITagViewer/IForm gaps; PrefixItem NotImplemented
- [swordfish-removal-306](project_swordfish_removal_epic_306.md) — legacy JSON round-trips via ScoDictionaryNew
- [legacy-scodictionary-315](project_legacy_scodictionary_removal_315.md) — delete SCODictionary_Tests, retarget 3 files
