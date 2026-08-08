# Atomic Executor Memory Index

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

## Build / toolchain environment

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
- [csharpier pipe-files is a non-enforcing gate](project_csharpier_pipefiles_nonenforcing_gate.md) — use `check`/`format`; size new files AFTER format

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

## Nullable / C# language

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
- [Plan literals inherit research arithmetic errors](project_plan_literal_assertions_inherit_research_arithmetic.md) — recompute every quoted expected literal at preflight

## Additional entries

- [Preflight fix tasks inherit the round's own rules](project_preflight_fix_tasks_inherit_decomposition_rules.md) — audit NEW tasks against all invariants, not just the finding they close
- [Conditional split = three tasks](project_conditional_split_three_task_shape.md) — measure / split / register, each with an authorized NO ACTION branch; never bundled
