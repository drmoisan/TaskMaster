---
name: index-csharp-nullable-and-component-gotchas
description: Sub-index of atomic-executor memories on nullable/C# language gates (net481 pragma, CS86xx, CS0518) and component-specific gotchas (WebView2, QFC, ObjectListView, TimeProvider, FluentAssertions)
metadata:
  type: reference
---

Sub-index moved out of MEMORY.md on 2026-09-28 to keep the index under its size limit. Open the linked file for detail.

## Nullable / C# language
- [Per-file pragma gate mechanics](project_nullable_pragma_gate_mechanics.md) · [net481 pragma-gate mechanics](project_nullable_pragma_gate_net481_mechanics.md) · [Epic pragma gate + analyzer restore](project_nullable_epic_pragma_gate_and_analyzer_restore.md)
- [#364 pre-existing blockers](project_364_nullable_gate_preexisting_blockers.md) · [CLAUDE.md command != the CI gate](project_507_nullconditional_return_triggers_cs8603_under_genuine_nullable_check.md)
- [Remediation annotation patterns](project_nullable_remediation_annotation_patterns.md) · [CS8632 scoping](project_nullable_annotation_cs8632_scoping.md) · [CS8714 not on net481](project_nullable_cs8714_not_on_net481.md)
- [init/record struct = CS0518 on net48](project_record_struct_isexternalinit_netfx.md) · [Outlook `Action`/`Exception` CS0104](project_outlook_action_ambiguity.md) · [CS1769 forces reflection](project_cs1769_forces_reflection_for_outlook_returning_apis.md)
- Nullable-epic (closed): [#366a](project_366_notnull_cascades_beyond_wrapperscodictionary.md), [#366b](project_366_scdictionary_constraint_cascades_to_fourth_file.md), [#366c](project_366_batch7_tnullable_return_cs8766.md), [#371](project_371_outlookobjects_nullable_lessons.md), [#372](project_372_email_classifier_nullable_patterns.md), [#375](project_375_residuals_nullable_gotchas.md)

## Component-specific gotchas
- [WebView2 EndInit creates child handles](project_webview2_endinit_creates_handles.md) · [#349 breadcrumb WebView2](project_349_breadcrumb_webview2_gotchas.md)
- QFC #227: [cycle-4 ToggleFocus](project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md) · [cycle-3 seam](project_theme_folderpredictor_seam_retrofit_gotchas.md)
- [ObjectListView headless selection](project_objectlistview_treelistview_headless_selection.md) · [BackgroundWorker async-void race](project_qfc_backgroundworker_async_void_race.md)
- [QfcItemController needs SaveParameters](project_qfcitemcontroller_pump_harness_needs_saveparameters.md) · [TaskController (#297)](project_taskvisualization_taskcontroller_test_gotchas.md)
- [ProjectEntry setter raw MessageBox](project_projectentry_setter_raw_messagebox.md) · [IApplicationGlobals member forces implementers](project_iapplicationglobals_member_forces_implementers.md)
- [TimeProvider seam gotchas](project_timeprovider_seam_gotchas.md) · [GetOrLoad discards setter injection](project_initializer_getorload_discards_injection_when_dependency_null.md)
- [ScoDictionaryNew needs TryAdd](project_scodictionarynew_tryadd_not_add.md) · [Equal(params) has no because](project_fluentassertions_equal_params_no_because.md) · [BeEmpty names only the first item](project_fluentassertions_beempty_names_only_first_item.md)
- [WinForms control field installs SyncContext, deadlocks await](project_winforms_control_field_installs_synccontext_and_deadlocks_await.md)
- [Invoke-VersionReconciliation rewrites Reference version](project_reference_version_rewrite_when_assemblyversion_omitted.md)
- [TimeoutAfter IsCompleted loses to the Task.Run race](project_timeoutafter_iscompleted_shortcircuit_loses_to_taskrun_race.md) · [FakeTimeProvider zero due time fires at creation](project_faketimeprovider_zero_duetime_fires_at_creation.md)
- [Auto-property with setter guard is not expressible](project_plan_mandated_autoproperty_with_setter_guard_is_not_expressible.md) · [Explicit Compile items decide membership](project_explicit_compile_items_decide_membership_not_file_presence.md)
