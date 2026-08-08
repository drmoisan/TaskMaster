# Atomic Executor Memory Index

## Planning / preflight

- [Inserted plan tasks force renumbering](project_plan_task_ids_digit_only_forces_renumbering.md) — IDs must be digit-only; phrase deltas as "insert + renumber downstream"
- [Preflight fix tasks inherit the round's own rules](project_preflight_fix_tasks_inherit_decomposition_rules.md) — audit NEW tasks against all invariants, not just the finding they close
- [Plan rationale clauses are evidence](project_418_plan_rationale_clauses_are_evidence.md) — unmeasured world-state claims in prose block preflight, not the fix itself
- [#418 500-line gate vs mandated plan content](project_418_500line_gate_vs_plan_content.md) — unsatisfiable size task; delta = extract pure helpers to a new file
- [Conditional split = three tasks](project_conditional_split_three_task_shape.md) — measure / split / register, each with an authorized NO ACTION branch; never bundled
- [#376 capstone scope-expansion layers](project_376_capstone_scope_expansion_layers.md) — 5 escalated layers resolved via the 3 authorized patterns
- [Concurrent executor in same worktree](project_concurrent_executor_same_worktree.md) — detect via mtime progression; STOP, don't stash/race
- [Sibling-worktree shared-tooling hazard](project_sibling_worktree_shared_tooling_hazard.md) — concurrent agents clobber shared vstest/dotnet-coverage and /tmp logs

## Build / toolchain

- [Project Build/Test Env](project_build_test_env.md) — git-bash quirks, MSBuild switches, csharpier v1, legacy csproj, IVT, C# 7.3
- [VS18 build/test toolchain paths](project_vs18_build_toolchain_paths.md) — full-framework msbuild.exe, nuget restore, MSYS_NO_PATHCONV, dotnet-coverage `--`
- [Repo-local SDK install + nullable Rebuild](project_repo_sdk_and_nullable_rebuild.md) — .dotnet-sdk needs pwsh7; nullable debt scope not stable across sessions
- [Analyzer version skew on fresh worktree](project_analyzer_version_skew_fresh_worktree.md) — first analyzer build CS0006; nuget install old versions into packages/
- [SecurityCodeScan incompatible with Roslyn 5.6](project_securitycodescan_roslyn56_incompat.md) — CS8032/YamlDotNet breaks the TWAE gate
- [Incremental build makes a vacuous baseline](project_incremental_build_vacuous_baseline.md) — /t:Build ignores /p: changes; add /t:Rebuild to enumerate diagnostics
- [Missing VSTO runtime breaks baseline gates](project_missing_vsto_runtime_breaks_baseline_gates.md) — CS0234 in ThisAddIn.Designer.cs; test projects never build
- [New sln member surfaces MSB3277](project_new_sln_member_surfaces_msb3277_pin_divergence.md) — packages.config pin divergence; never fixable in a .cs file
- [sln/csproj edits: preserve CRLF](project_sln_csproj_edit_crlf_preserve.md) — use Edit tool or perl -0777, not git-bash sed -i
- [Legacy csproj: no transitive compile refs](project_legacy_csproj_no_transitive_compile_refs.md) — tests naming a third-party type need their own `<Reference>`
- [BOM breaks grep ^ anchor](project_bom_grep_anchor_false_negative.md) — use the Grep tool, never bash grep, for opt-in classification
- [PowerShell new files need UTF-8 BOM](powershell-bom-required.md) — PSUseBOMForUnicodeEncodedFile; prepend BOM after Write
- [poshqc Pester MCP exits -1](project_poshqc_pester_mcp_exit_minus1.md) — pair with direct Invoke-Pester (pwsh7) for the numeric proof

## Nullable remediation

- [Nullable per-file pragma gate mechanics](project_nullable_pragma_gate_mechanics.md) — verify via isolated UtilitiesCS build with BuildProjectReferences=false
- [Nullable pragma-gate net481 mechanics](project_nullable_pragma_gate_net481_mechanics.md) — EXIT 0 unreachable; measure CS86xx with SVGControl prebuilt
- [Nullable epic: pragma gate + analyzer restore](project_nullable_epic_pragma_gate_and_analyzer_restore.md) — scoped TWAE with WarningsNotAsErrors; restore mismatched analyzers first
- [#364 nullable-gate pre-existing blockers](project_364_nullable_gate_preexisting_blockers.md) — full-solution gate fails at baseline
- [Nullable remediation annotation patterns](project_nullable_remediation_annotation_patterns.md) — net481 has no post-condition attrs; `= default!`; `.ToString()!`
- [Nullable annotation CS8632 scoping](project_nullable_annotation_cs8632_scoping.md) — wrap `Type?` in `#nullable enable annotations`
- [CS8714 does not fire on net481](project_nullable_cs8714_not_on_net481.md) — `where TKey : notnull` is forward-looking, not required
- [#371 OutlookObjects nullable lessons](project_371_outlookobjects_nullable_lessons.md) — public-signature changes regress other files in the same assembly
- [#372 email-classifier nullable patterns](project_372_email_classifier_nullable_patterns.md) — `null!` post-ctor; `T?` factory returns; `.Class!` cascade
- [#375 residuals nullable gotchas](project_375_residuals_nullable_gotchas.md) — CS8644 fixed with a `#nullable disable` island on the class-decl line
- [init/record struct fails CS0518 on net48](project_record_struct_isexternalinit_netfx.md) — use a constructor-initialized readonly struct

## Coverage measurement

- [C# canonical coverage artifact conversion](project_csharp_canonical_coverage_artifact_conversion.md) — hook reads artifacts/csharp/coverage.xml as JaCoCo; convert Cobertura
- [Coverage delta: reproduce the baseline's counting method](project_coverage_delta_reproduce_baseline_counting_method.md) — deduped vs all-descendant give ~2x-different denominators
- [First-party coverage denominator method (#197)](project_coverage_firstparty_denominator_method.md) — per-`<line>` across ALL deduped packages including vendored
- [dotnet-coverage denominator nondeterminism](project_dotnet_coverage_denominator_nondeterminism.md) — re-baseline via git-stash; trust per-class rates
- [Changed-line coverage: Cobertura hits vs MS-coverage partial](project_changed_line_coverage_cobertura_vs_mscoverage_partial.md) — use Cobertura per-line data for >=90% proofs
- [QFC #227 coverage tooling](project_qfc227_coverage_tooling.md) — vstest + Cobertura runsettings is the reliable per-class numeric path
- [Cobertura runsettings `<Attributes>` override](project_cobertura_runsettings_attributes_override.md) — a custom block silently disables [ExcludeFromCodeCoverage]
- [runsettings DataCollector default-enabled](project_runsettings_datacollector_default_enabled.md) — a declared collector activates without /collect
- [ExcludeFromCodeCoverage on partial class = CS0579](project_excludefromcodecoverage_partial_class_cs0579.md) — annotate a partial type once, not both parts
- [csharpier pipe-files is a non-enforcing gate](project_csharpier_pipefiles_nonenforcing_gate.md) — use `check`/`format`; size new test files AFTER format
- [dotnet-coverage Deedle/FSharp instrumentation breaks tests](project_dotnet_coverage_deedle_fsharp_instrumentation.md) — pass a module-exclude settings XML; pair with Workers=4

## Test execution

- [vstest /InIsolation + FilePathHelper serialization](project_vstest_isolation_and_filepathhelper_serialization.md) — Moq assemblies need /InIsolation
- [vstest TestCaseFilter OR-vs-pipe + fresh-worktree bootstrap](project_vstest_testcasefilter_or_operator_and_env_setup.md) — use `|` not `OR`; bootstrap SDK + tools first
- [Invoke-MSTest.ps1 dies on a single test assembly](project_418_invoke_mstest_single_assembly_bug.md) — StrictMode + `.Count` on a scalar; call vstest.console.exe directly
- [Timed-out MSTest leaves detached runner](project_timedout_mstest_leaves_detached_runner.md) — kill the pwsh runner too, verify 0, rerun >=8min
- [UtilitiesCS.Test parallelism flakiness](project_utilitiescs_test_parallelism_flakiness.md) — lower MSTest Workers to 4 via /Settings
- [MSTest [DoNotParallelize] overlaps the parallel bucket](project_mstest_donotparallelize_overlaps_parallel_bucket.md) — mark every writer too
- [#398 test-split gate gotchas](project_398_test_split_gate_gotchas.md) — pre-existing CS2002; /EnableCodeCoverage has no branch%; Workers=4 needed
- [FluentAssertions Equal(params) has no because](project_fluentassertions_equal_params_no_because.md) — a trailing reason becomes an extra expected element

## Test authoring / seams

- [TimeProvider seam gotchas](project_timeprovider_seam_gotchas.md) — Moq can't mock non-virtual GetLocalNow; use FakeTimeProvider
- [DispatcherDelay hangs unit tests](project_dispatcherdelay_hangs_unit_tests.md) — never completes in the pump-less MSTest host
- [ConfigController STA pump deadlock](project_configcontroller_sta_pump_deadlock.md) — STA tests must pump, not block on GetAwaiter().GetResult()
- [ObjectListView TreeListView headless selection](project_objectlistview_treelistview_headless_selection.md) — cache the node via SelectionChanged
- [QfcDatamodel BackgroundWorker async-void IsBusy race](project_qfc_backgroundworker_async_void_race.md) — assert WorkerSupportsCancellation instead
- [IApplicationGlobals member forces implementers](project_iapplicationglobals_member_forces_implementers.md) — breaks 7 hand-written stubs beyond scope lock
- [Outlook `Action`/`Exception` ambiguity](project_outlook_action_ambiguity.md) — use `System.Action`/`System.Exception`
- [ProjectEntry setter raw MessageBox](project_projectentry_setter_raw_messagebox.md) — the ProjectID setter bypasses the MyBox seam and hangs STA tests
- [TaskController (#297) unit-test gotchas](project_taskvisualization_taskcontroller_test_gotchas.md) — ApplyChanges hangs over Moq; STA harness needs parenting + warmup
- [Theme/FolderPredictor seam retrofit gotchas](project_theme_folderpredictor_seam_retrofit_gotchas.md) — shared parameterless-ctor builders cause silent regressions
- [#227 cycle-4 ToggleFocus genuine-execution gotchas](project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md) — use Activator.CreateInstance(field.FieldType) for missing refs
- [#349 breadcrumb WebView2 gotchas](project_349_breadcrumb_webview2_gotchas.md) — retyped Designer fields break reflection-injected tests
- [#207 Hook() redesign breaks AppEventsTests](project_207_hook_redesign_breaks_appeventstests.md) — needs a plan revision, not a test weakening
- [ApplicationGlobalsTests.cs at 500-line ceiling](project_appglobalstests_at_500_line_ceiling.md) — extract before adding any override
- [#400 CompleteOpenAsync unreachable recovery catch](project_400_completeopenasync_unreachable_recovery_catch.md) — dead code can't reach >=90%; 17-class gate deadlocks testhost
- [#328 Rebuild-threading breaks OlObjectsProxy](project_328_rebuild_threading_olobjectsproxy_conflict.md) — proxy stubs only get_App; return null for get_StoresWrapper

## Swordfish / collections migration

- [ScoDictionaryNew needs TryAdd not Add](project_scodictionarynew_tryadd_not_add.md) — base exposes `.TryAdd`; swap in the same edit
- [#366 notnull cascades past WrapperScoDictionary](project_366_notnull_cascades_beyond_wrapperscodictionary.md) — same one-line constraint on ScoDictionaryConverter
- [#366 ScDictionary constraint cascades to a 4th file](project_366_scdictionary_constraint_cascades_to_fourth_file.md) — STOP + re-escalate, don't widen
- [#366 Batch7 T? return triggers CS8766](project_366_batch7_tnullable_return_cs8766.md) — conform to `T` + justified `!`, don't edit the interface
- [Swordfish F5 test misclassification](project_swordfish_f5_test_misclassification.md) — verify using/namespace before treating a removal as Swordfish-only
- [Swordfish-removal epic: incidental vendored-coverage side effect](project_swordfish_removal_epic_incidental_coverage_sideeffect.md) — non-blocking, expect on F1/F2/F4/F5
- [TaskVisualization #298 ScoCollection + live-bridge exemptions](project_taskvis_scocollection_and_livebridge_exemptions.md) — default-factory live-form bridges need method-level exemption
