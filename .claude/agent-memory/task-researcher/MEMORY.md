# Task Researcher Memory Index

## Hygiene, feedback, references
- [no-absolute-host-paths](../_shared_no_absolute_host_paths.md) — use `<repo-root>`/`<user>`/`<host>`; rename vstest TRX before citing
- [exemption-audit-check-proven-techniques](feedback_exemption_audit_check_proven_techniques.md) — grep proven techniques + sibling consistency before IRREDUCIBLE
- [committed-cobertura-baselines](reference_committed_cobertura_baselines.md) — per-line coverage in docs/features/*/evidence/*.cobertura.xml; read, don't infer
- [net481-timeprovider-available](reference_net481_timeprovider_available.md) — FakeTimeProvider usable on net481; reject "net8+ only"
- [github-issue-search-without-gh](reference_github_issue_search_without_gh.md) — WebFetch github issues?q=... when gh absent; mark [V-web]

## Threading / test determinism
- [taskrun-getresult-inlines-900](project_taskrun_getresult_inlines_on_pool_thread_900.md) — Task.Run+GetResult on a pool thread INLINES (not reuse); MSTest 4.4 bodies run inside Task.Run; CheckAccess = Thread identity
- [pump-timeout-743](project_pump_timeout_743.md) — dispatcher-gate lead stale (#493); 9/19 pump tests skip the gate; TRX timestamps as instrument
- [uithread-dispatcher-restore-scope-493](project_uithread_dispatcher_restore_scope_493.md) — 2-lock split; never one semaphore for helper+fixture
- [winforms-pump-seam-230](project_winforms_pump_seam_230.md) — WinFormsPumpHost design; CreateAsync factory-seam gap
- [onedrive-timeout-test-determinism-253](project_onedrive_timeout_test_determinism_253.md) — TimeOutTask overload catches TimeoutException not TaskCanceled; DI-seam fix
- [unobserved-task-fault-670](project_unobserved_task_fault_670.md) — ViewerSetup.cs 499/500; IItemViewer.UiDispatcher is raw WPF Dispatcher
- [filerqueue-consumer-unsound-633](project_filerqueue_consumer_unsound_633.md) — Consumer orphaned-item race; BackGroundMove tests vacuous
- [terminal-hook-barrier-751](project_terminal_hook_barrier_751.md) — notify runs after terminal TrySet; `run.Terminal` is the barrier
- [analyzer-severity-and-runsettings-split](project_analyzer_severity_ceiling_and_runsettings_split.md) — MSTEST0032 only rule above suggestion; no .globalconfig
- [lock-recursion-coverage-317](project_lock_recursion_coverage_317.md) — deleted LockRecursionTests.cs is a restoration

## Coverage / Cobertura mechanics
- [cobertura-closure-exemption-457](project_cobertura_closure_exemption_457.md) — exempt members emit NO `<method>`; async `d__` machines are the trap
- [cobertura-root-attrs-raw-vs-postprocessed](project_cobertura_root_attrs_raw_vs_postprocessed.md) — raw root totals vs Koverage doubled sum; never compare
- [double-count-moves-counts-not-rates-815](project_cobertura_double_count_moves_counts_not_rates_815.md) — `.//line` doubles counters, moves pct <=0.46pp
- [coverage-threshold-reconciliation-494](project_coverage_threshold_reconciliation_494.md) — 85/75 is foreign leakage; gate evadable; +/-15pt spread
- [cobertura-perfile-attribution-contract](project_cobertura_perfile_attribution_contract.md) — `line-rate` attr inflated; recompute from `<lines>`
- [cobertura-line-double-count](project_cobertura_line_double_count.md) — lines-valid ~2x actual; recompute per-file
- [cobertura-exemption-branchrate-gotchas](project_cobertura_exemption_and_branchrate_gotchas.md) — method-level exclude does NOT exempt lambdas
- [qfc455-exclude-attribute-lambda-leak](project_qfc455_exclude_attribute_lambda_leak.md) — #441 inflation = per-method `<lines>` block
- [webview2-exemption-coverage-asymmetry](project_webview2_exemption_and_coverage_asymmetry.md) — class-level exclude hides nested lambdas, method-level not
- [partial-type-coverage-exclusion-456](project_partial_type_coverage_exclusion_456.md) — type-level exclusion hides Designer partials; de-exempting RAISES coverage
- [winforms-designer-coverage-mechanics](project_winforms_designer_coverage_mechanics.md) — one form construction auto-covers ~99% of Designer
- [itemviewer-partial-exemption-coupling](project_itemviewer_partial_exemption_coupling.md) — ItemViewer.cs:20 attr hides 6 partials + Designer
- [iqfcdatamodel-contract-436](project_iqfcdatamodel_contract_436.md) — Cobertura emits NO class element for interfaces/enums
- [quickfiler-interface-only-files-433](project_quickfiler_interface_only_files_433.md) — interface-only .cs absent; IQfcHomeController.cs exists twice
- [interface-only-ledger-bucket](project_quickfiler_per_file_coverage_interface_only_bucket.md) — add `interface-only` bucket; key harness on `filename`
- [quickfiler-percoverage-epic-136](project_quickfiler_percoverage_epic_136.md) — read per-file line-rate from committed Cobertura
- [quickfiler-coverage-ledger-432](project_quickfiler_coverage_ledger_432.md) — 121 files; "33 exemptions" really 40 usages/21 files
- [quickfiler-capstone-f16-measurement](project_quickfiler_capstone_f16_measurement.md) — `<sources>` discriminates raw vs post-processed (70.19/85.65)
- [f9-measurement-441-designer-inheritance](project_qfc_f9_measurement_441_and_designer_inheritance.md) — #441 corrupts per-file line-rate (6dp vs 16dp tell)

## Epic #136 per-file coverage children
- [qfc-helper-classes-f4-434](project_qfc_helper_classes_f4_434.md) — EmailMoveMonitor seam exists; explicit-Compile-Include csproj
- [qfc-theme-cluster-f4-434](project_qfc_theme_cluster_f4_434.md) — theme+layout tests-only; no colour getters
- [qfc-perfile-coverage-viewerqueue-434](project_qfc_perfile_coverage_viewerqueue_434.md) — method-group sites forbid optional-param seams
- [qfc-keyboard-coverage-430](project_qfc_keyboard_coverage_430.md) — UtilitiesCS grants no IVT to QuickFiler.Test; headless ItemViewer OK
- [qfc-keyboard-actions-430](project_qfc_keyboard_actions_430.md) — KaStringAsync has no async/timer
- [qfc-datamodel-coverage-436](project_qfc_datamodel_coverage_436.md) — type-scoped exclude hides 3 partials; remove last
- [efcdatamodel-coverage-436](project_efcdatamodel_coverage_436.md) — EmailFiler Sort/Open non-virtual; PackageItems(bool) dead
- [qfc-queueprocessing-436](project_qfc_queueprocessing_436.md) — zero COM deref; missing FakeTimeProvider fails silently
- [qfc-framebuilding-436](project_qfc_framebuilding_436.md) — Deedle not WinForms; DfDeedle dialogs behind IVT wall
- [qfc-explorer-controller-435](project_qfc_explorer_controller_435.md) — DynamicProxyGenAssembly2 IVT in QfcHighConfidencePreFilter.cs
- [qfc-form-controller-coverage-435](project_qfc_form_controller_coverage_435.md) — UndoConsumer `|| exit` busy-spins; RECONSTRUCTED, re-verify
- [qfc-form-controller-setup-disposal-435](project_qfc_form_controller_setup_disposal_435.md) — no new seams; Cleanup idempotent
- [duplicate-iqfcformcontroller-435](project_quickfiler_duplicate_iqfcformcontroller_435.md) — QuickFiler.Interfaces.IQfcFormController is dead code
- [qfc-home-controller-metrics-433](project_qfc_home_controller_metrics_433.md) — metrics consumer never runs; BlockingCollection OCE needs token
- [qfc-home-controller-iteration-433](project_qfc_home_controller_iteration_433.md) — #424 deadline leaked into 2-arg dequeue; Iterate dead
- [qfc-home-controller-coverage-433](project_qfc_home_controller_coverage_433.md) — LaunchAsync 0% structurally
- [efc-home-controller-deps-437](project_efc_home_controller_deps_437.md) — deps ~86-93%; Production* statics vs ClassLevel hazard
- [efc-home-controller-coverage-437](project_efc_home_controller_coverage_437.md) — Timing.cs reads no clock; dual default lambdas order-dependent
- [efc-item-controller-452](project_efc_item_controller_452.md) — IItemViewer covers ~70% of viewer
- [efc-form-controller-452](project_efc_form_controller_452.md) — ViewerQueueCore does NOT pool; #439 = namespace mismatch
- [qfc-item-controller-230-pump-seam](project_qfc_item_controller_230_pump_seam.md) — #230 root of 4 exemptions; 3/19 on DEAD members
- [qfc-item-controller-f10-coverage-453](project_qfc_item_controller_f10_coverage_453.md) — test files at 497/498 of 500
- [qfc-item-controller-f10-init-453](project_excludefromcodecoverage_lambda_leak.md) — 3/7 Initialization exemptions on DEAD members
- [qfc-conversation-seam-ratified-453](project_qfc_conversation_seam_ratified_453.md) — DoLoadConversationResolverCoreAsync exemption #227-ratified
- [qfc-collection-controller-454](project_qfc_collection_controller_454.md) — 12 unreachable members; `async public` defeats greps
- [quickfiler-test-sta-and-ivt](project_quickfiler_test_sta_and_ivt.md) — QuickFiler grants internals to QuickFiler.Test; manual STA infra exists
- [qfc-breadcrumb-dropdown-f13-455](project_qfc_breadcrumb_dropdown_f13_455.md) — async `throw;` makes catch brace unreachable
- [qfc455-reentrant-dispose-seam](project_qfc455_reentrant_dispose_seam.md) — disposal-callback reentrancy opens the async window
- [qfc-itemviewer-coverage-456](project_qfc_itemviewer_coverage_456.md) — class line-rate corrupt, branch-rate sound; STA attrs in MSTest 4.3.3
- [qfc-breadcrumb-lifecycle-f12-495](project_qfc_breadcrumb_lifecycle_f12_495.md) — `0/2` on `factory() ?? throw` means factory threw
- [qfc-breadcrumb-bridge-router-495](project_qfc_breadcrumb_bridge_router_495.md) — wrong router's branch-rate matches to 6 digits
- [breadcrumb-messenger-hub-495](project_breadcrumb_messenger_hub_495.md) — Component finalizer makes a branch GC-dependent
- [qfc-upgrade-lifetime-495](project_qfc_upgrade_lifetime_495.md) — `<class name>` can name a secondary type
- [qfc-item-controller-227-r2-denial](project_qfc_item_controller_227_r2_denial.md) — maintainer denied blanket exemption; per-member precedent
- [qfc227-headless-itemviewer](project_qfc227_headless_itemviewer_and_tlpcellsnapshot.md) — headless ItemViewer safe; target 24 -> 19

## QuickFiler / EFC defects and behaviour
- [qfc-high-confidence-dual-pipeline](project_qfc_high_confidence_dual_pipeline.md) — THREE pipelines; #233 gate LIVE, #169/#171 dormant
- [qfc424-startup-stall](project_qfc424_high_confidence_startup_stall.md) — gate scores serially; async-void BackgroundWorker; STA blocks parallel COM
- [qfc678-predictor-carry](project_qfc678_predictor_carry.md) — named producer DORMANT; PreScored never read
- [qfc791-deadline-cancel-teardown](project_qfc791_deadline_and_cancel_teardown.md) — empty-at-deadline superseded by #424/#608 ACs
- [qfc810-teardown-dropdown-residuals](project_qfc810_teardown_dropdown_residuals.md) — method-group blocks optional param (CS0123); files at 496/500
- [qfc823-review-residuals](project_qfc823_review_residuals.md) — R3 null-tolerance doc FALSE; R2 follow-up is #813
- [teardown-guard-enumeration-821](project_teardown_guard_enumeration_821.md) — 4th sharer is the 4th Cancel() SITE; enabler is SetCancellationTokenSource
- [qfc-lifecycle-disposal-731](project_qfc_lifecycle_disposal_731.md) — sharing EmailMoveMonitor DROPS actions; `volatile` = CS0420
- [qfc254-darkmode-stale-labels](project_qfc254_darkmode_stale_labels.md) — labels themed only in MailRead()-guarded branch
- [qfc254-residual-after-comexception-fix](project_qfc254_residual_after_comexception_fix.md) — historical; #269 real cause = Light-theme fore/back swap
- [qfc438-search-focus-steal](project_qfc438_search_focus_steal.md) — TWO focus-steal mechanisms; CancelSelector emits no SelectionChanged
- [qfc677-webview2-focus-hold](project_qfc677_webview2_focus_hold_outlook_keyboard.md) — WebView2 focus hold + _focusAnchor steal; fix = focus predicate
- [qfc680-menu-mode-keyboard-capture](project_qfc680_menu_mode_keyboard_capture.md) — ModalMenuFilter; AutoClose=false pre-Show only opt-out
- [qfc663-alt-chord-no-altf](project_qfc663_alt_chord_no_altf.md) — only Alt+M swallowed; Alt+F is EFC-only
- [qfc-keyboard-action-defects-444](project_qfc_keyboard_action_defects_444.md) — #468 removes duplicate registration; trigger Right/Down/Right
- [breadcrumb-navigation-defects-439-440](project_breadcrumb_navigation_defects_439_440_498_499.md) — fixing #439 regresses percentage join
- [issue-440-already-landed-via-498](project_issue_440_already_landed_via_498.md) — Efc/Qfc do NOT share BreadcrumbRow
- [qfc-item-controller-defects-484](project_qfc_item_controller_defects_484.md) — all 5 "Suspected Fix" sections wrong; verify callers first
- [webview2-host-initializer-defects-476](project_webview2_host_initializer_defects_476.md) — EfcViewerQueue not a pool; real WebView2 built in tests
- [qfc-efc-metrics-442](project_qfc_efc_metrics_442.md) — MoveAndIterate stopwatch race unfixable in owned files
- [qfc-collection-defects-468](project_qfc_collection_defects_468.md) — MovedMails param redundant; no log4net in QuickFiler.Test
- [qfc-collection-controller-defects-468](project_qfc_collection_controller_defects_468.md) — #474 "unrelated interfaces" FALSE
- [issue-469-already-fixed-residual-629](project_issue_469_already_fixed_residual_is_629.md) — `Initialized<T>` never memoizes
- [reflective-caller-closure-635](project_reflective_caller_closure_635.md) — removal was 13 members; `GetField(` is the reaching mechanism
- [selectrow-two-families-637](project_selectrow_two_families_637.md) — TWO SelectRow families; ButtonOK_Click does NOT rethrow
- [issue-656-bypass-path-does-not-exist](project_issue_656_bypass_path_does_not_exist.md) — premise FALSE; owner files at cap
- [efc614-store-root-stem-leak](project_efc614_store_root_stem_leak.md) — FolderConverterTests.cs:329 codifies a bug
- [efc736-archiveroot-boundary-sink](project_efc736_archiveroot_boundary_sink.md) — finding 6 cause FALSE; 6 sink sites not 4
- [banner-prefix-arity-662](project_banner_prefix_arity_662.md) — `/Tests:` and `/TestCaseFilter:` mutually exclusive
- [qfc-folder-tree-percentage-325](project_qfc_folder_tree_percentage_325.md) — 1 live viewer despite 9 dead CboFolders
- [qfc-breadcrumb-webview2-351](project_qfc_breadcrumb_webview2_351.md) — 9101 provider absent; bridge greenfield
- [folder-hierarchy-provider-350](project_folder_hierarchy_provider_350.md) — reuse snapshot infra; no new COM seam
- [efcviewer-breadcrumb-webview2-349](project_efcviewer_breadcrumb_webview2_349.md) — EfcViewer3 dead; unscaled ColumnHeader widths
- [folder-settings-persistence-797](project_folder_settings_persistence_797.md) — ThisAddIn_Shutdown never raised; a test asserts the AC8 bug
- [ilglobals-static-publication-824](project_ilglobals_static_publication_824.md) — BeSameAs only order-independent RED gate; CA2211 never fires
- [etl-deadline-followups-825](project_etl_deadline_followups_825.md) — read shipped package .xml to verify API

## Stores / watchdog / add-in lifecycle
- [store-runtime-reenable-263](project_store_runtime_reenable_263.md) — no per-store post-startup hookup seam
- [store-lockup-resilience-f4](project_store_lockup_resilience_f4_research.md) — AsyncLocal rejected (static volatile); MyBox no modeless path
- [stores-enum-stall-292](project_stores_enum_stall_292.md) — watchdog crash on null model; ThreadMonitor LIVE
- [storewrapper-dialog-287](project_storewrapper_dialog_287_state_inversion.md) — StoresUnavailable transient, ModelUnavailable permanent

## Ribbon, CI, toolchain, repo structure
- [ribbon-engine-readiness-503](project_ribbon_engine_readiness_503.md) — Ribbon layer coverage-excluded; 5 orphan onAction callbacks
- [ribbon-toggle-state-guards-505](project_ribbon_toggle_state_guards_505.md) — toggle vs command guard asymmetry
- [ribbon-engine-toggle-defects-735](project_ribbon_engine_toggle_defects_735.md) — 84 callback names (5 dead); ManagerAsyncLazy constructible
- [ci-parallel-split-553](project_ci_parallel_split_553.md) — 4 independent jobs; check names "caller / callee"
- [toolchain-gate-fidelity-512](project_toolchain_gate_fidelity_512.md) — AGENTS.md externally owned; ~1.2s vs ~17s = vacuity
- [console-out-and-rs0030-826](project_console_out_and_rs0030_promotion_826.md) — Directory.Build.props exists; CS0169/CS0414 is the hazard
- [dependabot-net481-340](project_dependabot_net481_340.md) — semver-major ignore, no fabricated ceilings
- [svgcontrol-test-unwired-418](project_svgcontrol_test_unwired_418.md) — STALE (in .sln since 2026-08-14); redirect topology historical
- [winforms-testability-epic-298](project_winforms_testability_epic_298.md) — #298 depends on #297; inverts #197 exemptions
- [tagcontroller-refactor-293](project_tagcontroller_refactor_293.md) — ITagViewer/IForm gaps; PrefixItem NotImplemented
- [swordfish-removal-epic-306](project_swordfish_removal_epic_306.md) — legacy flat JSON round-trips via ScoDictionaryNew
- [legacy-scodictionary-removal-315](project_legacy_scodictionary_removal_315.md) — delete SCODictionary_Tests, retarget 3 files
