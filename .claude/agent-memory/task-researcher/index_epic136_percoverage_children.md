---
name: index-epic136-percoverage-children
description: Sub-index of task-researcher memories for QuickFiler/EFC per-file coverage epic #136 child items (430-495, 227); open when researching coverage of a QuickFiler or EFC class
metadata:
  type: reference
---

Sub-index split out of MEMORY.md (2026-10-09) to keep the root index under its read limit. One line per memory file.

- [qfc-helper-classes-434](project_qfc_helper_classes_f4_434.md) — EmailMoveMonitor seam; explicit Compile Includes
- [qfc-theme-cluster-434](project_qfc_theme_cluster_f4_434.md) — theme+layout tests-only; no colour getters
- [qfc-viewerqueue-434](project_qfc_perfile_coverage_viewerqueue_434.md) — method-group sites forbid optional params
- [qfc-keyboard-coverage-430](project_qfc_keyboard_coverage_430.md) — no IVT UtilitiesCS->QuickFiler.Test; headless ItemViewer OK
- [qfc-keyboard-actions-430](project_qfc_keyboard_actions_430.md) — KaStringAsync has no async/timer
- [qfc-datamodel-436](project_qfc_datamodel_coverage_436.md) — type-scoped exclude hides 3 partials; remove last
- [efcdatamodel-436](project_efcdatamodel_coverage_436.md) — EmailFiler Sort/Open non-virtual; PackageItems(bool) dead
- [qfc-queueprocessing-436](project_qfc_queueprocessing_436.md) — zero COM deref; missing FakeTimeProvider fails silently
- [qfc-framebuilding-436](project_qfc_framebuilding_436.md) — Deedle not WinForms; DfDeedle dialogs behind IVT wall
- [qfc-explorer-controller-435](project_qfc_explorer_controller_435.md) — DynamicProxyGenAssembly2 IVT in QfcHighConfidencePreFilter.cs
- [qfc-form-controller-435](project_qfc_form_controller_coverage_435.md) — UndoConsumer busy-spins; RECONSTRUCTED, re-verify
- [qfc-form-setup-disposal-435](project_qfc_form_controller_setup_disposal_435.md) — no new seams; Cleanup idempotent
- [duplicate-iqfcformcontroller-435](project_quickfiler_duplicate_iqfcformcontroller_435.md) — QuickFiler.Interfaces copy is dead code
- [qfc-home-metrics-433](project_qfc_home_controller_metrics_433.md) — metrics consumer never runs; OCE needs token
- [qfc-home-iteration-433](project_qfc_home_controller_iteration_433.md) — #424 deadline leaked into 2-arg dequeue; Iterate dead
- [qfc-home-coverage-433](project_qfc_home_controller_coverage_433.md) — LaunchAsync 0% structurally
- [efc-home-deps-437](project_efc_home_controller_deps_437.md) — Production* statics vs ClassLevel hazard
- [efc-home-coverage-437](project_efc_home_controller_coverage_437.md) — Timing.cs reads no clock; default lambdas order-dependent
- [efc-item-controller-452](project_efc_item_controller_452.md) — IItemViewer covers ~70% of viewer; WpfUiDispatcher ctor internal
- [efc-form-controller-452](project_efc_form_controller_452.md) — ViewerQueueCore does NOT pool; #439 = namespace mismatch
- [qfc-item-230-pump-seam](project_qfc_item_controller_230_pump_seam.md) — #230 root of 4 exemptions; 3/19 on DEAD members
- [qfc-item-f10-453](project_qfc_item_controller_f10_coverage_453.md) — test files at 497/498 of 500
- [qfc-item-f10-init-453](project_excludefromcodecoverage_lambda_leak.md) — 3/7 Initialization exemptions on DEAD members
- [qfc-conversation-seam-453](project_qfc_conversation_seam_ratified_453.md) — exemption #227-ratified
- [qfc-collection-controller-454](project_qfc_collection_controller_454.md) — 12 unreachable members; `async public` defeats greps
- [quickfiler-test-sta-ivt](project_quickfiler_test_sta_and_ivt.md) — QuickFiler grants internals to QuickFiler.Test; manual STA infra
- [qfc-dropdown-f13-455](project_qfc_breadcrumb_dropdown_f13_455.md) — async `throw;` makes catch brace unreachable
- [qfc455-reentrant-dispose](project_qfc455_reentrant_dispose_seam.md) — disposal-callback reentrancy opens async window
- [qfc-itemviewer-456](project_qfc_itemviewer_coverage_456.md) — line-rate corrupt, branch-rate sound; STA attrs in MSTest 4.3.3
- [qfc-breadcrumb-lifecycle-495](project_qfc_breadcrumb_lifecycle_f12_495.md) — `0/2` on `factory() ?? throw` = factory threw
- [qfc-bridge-router-495](project_qfc_breadcrumb_bridge_router_495.md) — wrong router's branch-rate matches to 6 digits
- [breadcrumb-messenger-hub-495](project_breadcrumb_messenger_hub_495.md) — Component finalizer makes a branch GC-dependent
- [qfc-upgrade-lifetime-495](project_qfc_upgrade_lifetime_495.md) — `<class name>` can name a secondary type
- [qfc-item-227-r2-denial](project_qfc_item_controller_227_r2_denial.md) — maintainer denied blanket exemption; per-member precedent
- [qfc227-headless-itemviewer](project_qfc227_headless_itemviewer_and_tlpcellsnapshot.md) — headless ItemViewer safe; target 24 -> 19
