# Task Researcher Memory Index

## Hygiene, feedback, references
- [no-absolute-host-paths](../_shared_no_absolute_host_paths.md) — use `<repo-root>`/`<user>`; rename TRX before citing
- [exemption-audit-proven-techniques](feedback_exemption_audit_check_proven_techniques.md) — grep proven techniques before IRREDUCIBLE
- [measure-item-worktree](feedback_measure_item_worktree_not_session_worktree.md) — measure the item worktree, not the session one
- [committed-cobertura-baselines](reference_committed_cobertura_baselines.md) — per-line coverage in docs/features/*/evidence
- [net481-timeprovider](reference_net481_timeprovider_available.md) — FakeTimeProvider usable on net481
- [issue-search-without-gh](reference_github_issue_search_without_gh.md) — WebFetch issues?q=...; mark [V-web]

## Threading / test determinism
- [taskrun-getresult-inlines-900](project_taskrun_getresult_inlines_on_pool_thread_900.md) — Task.Run+GetResult on pool thread INLINES
- [prime-marker-944](project_prime_marker_register_before_start_944.md) — ContinueWith(None) never inlines; TCS marker before start
- [pump-timeout-743](project_pump_timeout_743.md) — dispatcher-gate lead stale; TRX timestamps as instrument
- [uithread-restore-493](project_uithread_dispatcher_restore_scope_493.md) — never one semaphore for helper+fixture
- [winforms-pump-seam-230](project_winforms_pump_seam_230.md) — WinFormsPumpHost; CreateAsync factory gap
- [onedrive-timeout-253](project_onedrive_timeout_test_determinism_253.md) — catches TimeoutException not TCE
- [unobserved-task-fault-670](project_unobserved_task_fault_670.md) — ViewerSetup.cs 499/500; raw WPF Dispatcher
- [filerqueue-consumer-633](project_filerqueue_consumer_unsound_633.md) — orphaned-item race; vacuous tests
- [terminal-hook-barrier-751](project_terminal_hook_barrier_751.md) — `run.Terminal` is the barrier
- [analyzer-severity-runsettings](project_analyzer_severity_ceiling_and_runsettings_split.md) — MSTEST0032 only rule above suggestion
- [lock-recursion-317](project_lock_recursion_coverage_317.md) — deleted LockRecursionTests.cs is a restoration
- [gettableinviewasync-838](project_gettableinviewasync_null_contract_838.md) — `maxAttempts:1` = TWO attempts

## Coverage / Cobertura mechanics
- [closure-exemption-457](project_cobertura_closure_exemption_457.md) — exempt members emit no `<method>`; `d__` trap
- [root-attrs-raw-vs-post](project_cobertura_root_attrs_raw_vs_postprocessed.md) — never compare raw vs Koverage totals
- [double-count-815](project_cobertura_double_count_moves_counts_not_rates_815.md) — doubles counters, pct <=0.46pp
- [threshold-reconciliation-494](project_coverage_threshold_reconciliation_494.md) — 85/75 foreign leakage; gate evadable
- [perfile-attribution](project_cobertura_perfile_attribution_contract.md) — recompute from `<lines>`
- [line-double-count](project_cobertura_line_double_count.md) — lines-valid ~2x actual
- [exemption-branchrate-gotchas](project_cobertura_exemption_and_branchrate_gotchas.md) — method exclude leaks lambdas
- [qfc455-lambda-leak](project_qfc455_exclude_attribute_lambda_leak.md) — #441 inflation = per-method `<lines>`
- [webview2-exemption-asymmetry](project_webview2_exemption_and_coverage_asymmetry.md) — class-level hides lambdas
- [partial-type-exclusion-456](project_partial_type_coverage_exclusion_456.md) — de-exempting designer RAISES coverage
- [designer-coverage](project_winforms_designer_coverage_mechanics.md) — one construction covers ~99% Designer
- [itemviewer-partial-coupling](project_itemviewer_partial_exemption_coupling.md) — one attr hides 6 partials
- [iqfcdatamodel-436](project_iqfcdatamodel_contract_436.md) — no class element for interfaces/enums
- [interface-only-files-433](project_quickfiler_interface_only_files_433.md) — IQfcHomeController.cs exists twice
- [interface-only-bucket](project_quickfiler_per_file_coverage_interface_only_bucket.md) — `interface-only` ledger bucket
- [percoverage-epic-136](project_quickfiler_percoverage_epic_136.md) — read per-file rate from Cobertura
- [coverage-ledger-432](project_quickfiler_coverage_ledger_432.md) — 40 usages/21 files, not 33
- [capstone-f16](project_quickfiler_capstone_f16_measurement.md) — `<sources>` discriminates raw vs post
- [f9-441-designer](project_qfc_f9_measurement_441_and_designer_inheritance.md) — 6dp vs 16dp tell

## Epic #136 per-file coverage children
- [helper-classes-434](project_qfc_helper_classes_f4_434.md) — EmailMoveMonitor seam exists
- [theme-cluster-434](project_qfc_theme_cluster_f4_434.md) — no colour getters
- [viewerqueue-434](project_qfc_perfile_coverage_viewerqueue_434.md) — method-group sites forbid optional params
- [keyboard-430](project_qfc_keyboard_coverage_430.md) — no IVT to QuickFiler.Test; headless ItemViewer OK
- [keyboard-actions-430](project_qfc_keyboard_actions_430.md) — KaStringAsync has no async/timer
- [datamodel-436](project_qfc_datamodel_coverage_436.md) — type exclude hides 3 partials
- [efcdatamodel-436](project_efcdatamodel_coverage_436.md) — Sort/Open non-virtual
- [queueprocessing-436](project_qfc_queueprocessing_436.md) — missing FakeTimeProvider fails silently
- [framebuilding-436](project_qfc_framebuilding_436.md) — Deedle; dialogs behind IVT wall
- [explorer-controller-435](project_qfc_explorer_controller_435.md) — DynamicProxy IVT in QfcHighConfidencePreFilter.cs
- [form-controller-435](project_qfc_form_controller_coverage_435.md) — UndoConsumer busy-spins; re-verify
- [form-controller-disposal-435](project_qfc_form_controller_setup_disposal_435.md) — Cleanup idempotent
- [duplicate-iqfcformcontroller-435](project_quickfiler_duplicate_iqfcformcontroller_435.md) — Interfaces copy dead
- [home-metrics-433](project_qfc_home_controller_metrics_433.md) — metrics consumer never runs
- [home-iteration-433](project_qfc_home_controller_iteration_433.md) — Iterate dead; deadline leak
- [home-coverage-433](project_qfc_home_controller_coverage_433.md) — LaunchAsync 0% structurally
- [efc-home-deps-437](project_efc_home_controller_deps_437.md) — Production* statics vs ClassLevel
- [efc-home-coverage-437](project_efc_home_controller_coverage_437.md) — Timing.cs reads no clock
- [efc-item-controller-452](project_efc_item_controller_452.md) — IItemViewer covers ~70%
- [efc-form-controller-452](project_efc_form_controller_452.md) — ViewerQueueCore does NOT pool
- [item-controller-230-seam](project_qfc_item_controller_230_pump_seam.md) — #230 root of 4 exemptions
- [item-controller-f10-453](project_qfc_item_controller_f10_coverage_453.md) — test files at 497/498
- [f10-init-453](project_excludefromcodecoverage_lambda_leak.md) — 3/7 exemptions on DEAD members
- [conversation-seam-453](project_qfc_conversation_seam_ratified_453.md) — #227-ratified exemption
- [collection-controller-454](project_qfc_collection_controller_454.md) — `async public` defeats greps
- [test-sta-and-ivt](project_quickfiler_test_sta_and_ivt.md) — manual STA infra exists
- [breadcrumb-dropdown-455](project_qfc_breadcrumb_dropdown_f13_455.md) — async `throw;` brace unreachable
- [reentrant-dispose-455](project_qfc455_reentrant_dispose_seam.md) — reentrancy opens async window
- [itemviewer-456](project_qfc_itemviewer_coverage_456.md) — line-rate corrupt, branch-rate sound
- [breadcrumb-lifecycle-495](project_qfc_breadcrumb_lifecycle_f12_495.md) — `0/2` on `?? throw` = factory threw
- [bridge-router-495](project_qfc_breadcrumb_bridge_router_495.md) — wrong router matches 6 digits
- [messenger-hub-495](project_breadcrumb_messenger_hub_495.md) — finalizer makes branch GC-dependent
- [upgrade-lifetime-495](project_qfc_upgrade_lifetime_495.md) — `<class name>` can name secondary type
- [item-controller-227-denial](project_qfc_item_controller_227_r2_denial.md) — per-member precedent
- [headless-itemviewer-227](project_qfc227_headless_itemviewer_and_tlpcellsnapshot.md) — target 24 -> 19

## QuickFiler / EFC defects and behaviour
- [high-confidence-pipelines](project_qfc_high_confidence_dual_pipeline.md) — THREE pipelines; #233 gate LIVE
- [startup-stall-424](project_qfc424_high_confidence_startup_stall.md) — serial scoring; async-void worker
- [predictor-carry-678](project_qfc678_predictor_carry.md) — PreScored never read
- [deadline-teardown-791](project_qfc791_deadline_and_cancel_teardown.md) — superseded by #424/#608 ACs
- [teardown-dropdown-810](project_qfc810_teardown_dropdown_residuals.md) — CS0123; files at 496/500
- [review-residuals-823](project_qfc823_review_residuals.md) — follow-up is #813
- [teardown-guard-821](project_teardown_guard_enumeration_821.md) — 4th sharer is 4th Cancel() site
- [lifecycle-disposal-731](project_qfc_lifecycle_disposal_731.md) — sharing monitor DROPS actions
- [darkmode-labels-254](project_qfc254_darkmode_stale_labels.md) — themed only in MailRead() branch
- [qfc254-residual](project_qfc254_residual_after_comexception_fix.md) — #269 = Light-theme swap
- [qfc254-ambient](project_qfc254_ambient_inheritance_mechanism.md) — SUPERSEDED; ambient notes accurate
- [search-focus-438](project_qfc438_search_focus_steal.md) — TWO focus-steal mechanisms
- [webview2-focus-677](project_qfc677_webview2_focus_hold_outlook_keyboard.md) — fix = focus predicate
- [menu-mode-680](project_qfc680_menu_mode_keyboard_capture.md) — AutoClose=false pre-Show only
- [alt-chord-663](project_qfc663_alt_chord_no_altf.md) — only Alt+M swallowed
- [keyboard-defects-444](project_qfc_keyboard_action_defects_444.md) — trigger Right/Down/Right
- [breadcrumb-nav-439-440](project_breadcrumb_navigation_defects_439_440_498_499.md) — #439 fix regresses join
- [issue-440-via-498](project_issue_440_already_landed_via_498.md) — Efc/Qfc don't share BreadcrumbRow
- [item-controller-484](project_qfc_item_controller_defects_484.md) — all 5 Suspected Fix wrong
- [webview2-init-476](project_webview2_host_initializer_defects_476.md) — EfcViewerQueue not a pool
- [metrics-442](project_qfc_efc_metrics_442.md) — stopwatch race unfixable in owned files
- [collection-defects-468](project_qfc_collection_defects_468.md) — no log4net in QuickFiler.Test
- [collection-controller-468](project_qfc_collection_controller_defects_468.md) — #474 premise FALSE
- [issue-469-fixed-629](project_issue_469_already_fixed_residual_is_629.md) — `Initialized<T>` never memoizes
- [reflective-closure-635](project_reflective_caller_closure_635.md) — `GetField(` reaches 13 members
- [selectrow-637](project_selectrow_two_families_637.md) — TWO SelectRow families
- [issue-656-no-bypass](project_issue_656_bypass_path_does_not_exist.md) — premise FALSE
- [store-root-stem-614](project_efc614_store_root_stem_leak.md) — test codifies a bug
- [archiveroot-sink-736](project_efc736_archiveroot_boundary_sink.md) — 6 sink sites not 4
- [banner-prefix-662](project_banner_prefix_arity_662.md) — `/Tests:` vs `/TestCaseFilter:` exclusive
- [folder-tree-pct-325](project_qfc_folder_tree_percentage_325.md) — 1 live viewer
- [breadcrumb-wv2-351](project_qfc_breadcrumb_webview2_351.md) — bridge greenfield
- [folder-hierarchy-350](project_folder_hierarchy_provider_350.md) — reuse snapshot infra
- [efcviewer-breadcrumb-349](project_efcviewer_breadcrumb_webview2_349.md) — EfcViewer3 dead
- [folder-settings-797](project_folder_settings_persistence_797.md) — ThisAddIn_Shutdown never raised
- [ilglobals-824](project_ilglobals_static_publication_824.md) — BeSameAs only RED gate
- [etl-deadline-825](project_etl_deadline_followups_825.md) — read package .xml to verify API

## Stores / watchdog / add-in lifecycle
- [store-reenable-263](project_store_runtime_reenable_263.md) — no per-store hookup seam
- [store-lockup-f4](project_store_lockup_resilience_f4_research.md) — AsyncLocal rejected
- [stores-enum-stall-292](project_stores_enum_stall_292.md) — watchdog crash on null model
- [storewrapper-dialog-287](project_storewrapper_dialog_287_state_inversion.md) — state inversion

## Ribbon, CI, toolchain, repo structure
- [ribbon-readiness-503](project_ribbon_engine_readiness_503.md) — 5 orphan onAction callbacks
- [ribbon-toggle-guards-505](project_ribbon_toggle_state_guards_505.md) — toggle vs command asymmetry
- [ribbon-toggle-defects-735](project_ribbon_engine_toggle_defects_735.md) — 84 callbacks (5 dead)
- [ci-parallel-split-553](project_ci_parallel_split_553.md) — check names "caller / callee"
- [toolchain-fidelity-512](project_toolchain_gate_fidelity_512.md) — ~1.2s vs ~17s = vacuity
- [console-out-rs0030-826](project_console_out_and_rs0030_promotion_826.md) — CS0169/CS0414 hazard
- [fsharp-hintpath-895](project_fsharp_core_hintpath_skew_895.md) — 15 output dirs; ns2.1>2.0 ranking
- [dependabot-340](project_dependabot_net481_340.md) — semver-major ignore
- [svgcontrol-test-418](project_svgcontrol_test_unwired_418.md) — STALE; in .sln since 2026-08-14
- [winforms-epic-298](project_winforms_testability_epic_298.md) — depends on #297
- [tagcontroller-293](project_tagcontroller_refactor_293.md) — PrefixItem NotImplemented
- [swordfish-306](project_swordfish_removal_epic_306.md) — round-trips via ScoDictionaryNew
- [scodictionary-315](project_legacy_scodictionary_removal_315.md) — delete tests, retarget 3 files
- [push-down-claude-dir-149](project_push_down_claude_dir.md) — pushDownClaudeDir research
- [host-identifier-sweep-602](project_host_identifier_sweep_602.md) — rg-vs-git deltas
