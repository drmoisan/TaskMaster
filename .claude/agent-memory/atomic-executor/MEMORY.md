# Atomic Executor Memory Index

## Planning, preflight, and process

- [Inserted plan tasks force renumbering](project_plan_task_ids_digit_only_forces_renumbering.md) — suffixed IDs (`P3-T5a`) fail validation; say "insert + renumber downstream"
- [Preflight: probe mandated C# shapes with csc](project_preflight_csc_probe_for_mandated_csharp_shapes.md) — vswhere-resolved Roslyn csc on a scratchpad file proves a dictated construct compiles
- [Plan rationale clauses are evidence](project_418_plan_rationale_clauses_are_evidence.md) — preflight blockers are usually unmeasured world-state claims in prose, not the fix
- [#418 500-line gate vs mandated plan content](project_418_500line_gate_vs_plan_content.md) — per-block logging clauses can make a task unsatisfiable; delta = extract helpers
- [#376 capstone scope-expansion layers](project_376_capstone_scope_expansion_layers.md) — 5 escalated layers resolved via the 3 authorized patterns; stop-condition never fired
- [#207 Hook() redesign breaks AppEventsTests](project_207_hook_redesign_breaks_appeventstests.md) — needs a plan revision, not a test weakening
- [Concurrent executor in same worktree](project_concurrent_executor_same_worktree.md) — detect via mtime progression; STOP, don't stash/race
- [Sibling-worktree shared-tooling hazard](project_sibling_worktree_shared_tooling_hazard.md) — other worktrees crash your testhost via shared vstest; use the session scratchpad
- [Swordfish F5 test misclassification](project_swordfish_f5_test_misclassification.md) — verify using/namespace before treating a test removal as Swordfish-only
- [IApplicationGlobals member forces implementers](project_iapplicationglobals_member_forces_implementers.md) — breaks 7 hand-written stubs beyond scope lock; Moq auto-implements
- [ApplicationGlobalsTests.cs at 500-line ceiling](project_appglobalstests_at_500_line_ceiling.md) — exactly 500 lines; any added override must extract first
- [BOM breaks grep ^ anchor](project_bom_grep_anchor_false_negative.md) — use the Grep tool, never bash grep, for opt-in classification
- [sln/csproj edits: preserve CRLF](project_sln_csproj_edit_crlf_preserve.md) — git-bash `sed -i` strips CRLF; use Edit or `perl -0777`

## Build and toolchain

- [VS18 build/test toolchain paths](project_vs18_build_toolchain_paths.md) — VS 18 full-framework msbuild.exe, nuget restore, MSYS_NO_PATHCONV, csharpier v1 subcommands
- [Project Build/Test Env](project_build_test_env.md) — git-bash quirks, legacy csproj Compile includes, IVT for Moq, C# 7.3 in QuickFiler.Test
- [Repo-local SDK install + nullable Rebuild](project_repo_sdk_and_nullable_rebuild.md) — .dotnet-sdk needs pwsh7; nullable debt scope is NOT stable across sessions
- [Incremental build makes a vacuous baseline](project_incremental_build_vacuous_baseline.md) — /t:Build ignores /p: changes; add /t:Rebuild to enumerate diagnostics
- [Analyzer version skew on fresh worktree](project_analyzer_version_skew_fresh_worktree.md) — first analyzer build CS0006; nuget install old versions into packages/
- [SecurityCodeScan incompatible with Roslyn 5.6](project_securitycodescan_roslyn56_incompat.md) — CS8032/YamlDotNet breaks the TWAE gate
- [Missing VSTO runtime breaks baseline gates](project_missing_vsto_runtime_breaks_baseline_gates.md) — 4x CS0234 in ThisAddIn.Designer.cs; repo coverage reads ~25% not ~71%
- [New sln member surfaces MSB3277](project_new_sln_member_surfaces_msb3277_pin_divergence.md) — packages.config pin divergence; never fixable in a .cs file
- [Legacy csproj: no transitive compile refs](project_legacy_csproj_no_transitive_compile_refs.md) — CS0012 despite copy-local DLL; add own `<Reference>` + packages.config entry
- [csharpier pipe-files is a non-enforcing gate](project_csharpier_pipefiles_nonenforcing_gate.md) — use `csharpier check`/`format`; size new test files AFTER format
- [PowerShell new files need UTF-8 BOM](powershell-bom-required.md) — PSUseBOMForUnicodeEncodedFile; prepend BOM after Write
- [poshqc Pester MCP exits -1](project_poshqc_pester_mcp_exit_minus1.md) — run it for the record, pair with direct Invoke-Pester (pwsh7)

## Test execution and flakiness

- [vstest TestCaseFilter OR-vs-pipe + worktree bootstrap](project_vstest_testcasefilter_or_operator_and_env_setup.md) — `/TestCaseFilter` needs `|` not `OR`; fresh worktree bootstrap order
- [vstest /InIsolation + FilePathHelper serialization](project_vstest_isolation_and_filepathhelper_serialization.md) — Moq assemblies need /InIsolation
- [Invoke-MSTest.ps1 dies on a single test assembly](project_418_invoke_mstest_single_assembly_bug.md) — StrictMode + `.Count` on a scalar; call vstest.console.exe directly
- [Timed-out MSTest leaves detached runner](project_timedout_mstest_leaves_detached_runner.md) — kill the pwsh runner too, verify 0, rerun >=8min
- [UtilitiesCS.Test parallelism flakiness](project_utilitiescs_test_parallelism_flakiness.md) — lower MSTest Workers to 4 via /Settings for a deterministic gate
- [MSTest [DoNotParallelize] overlaps the parallel bucket](project_mstest_donotparallelize_overlaps_parallel_bucket.md) — mark every writer too
- [DispatcherDelay hangs unit tests](project_dispatcherdelay_hangs_unit_tests.md) — never completes in the pump-less MSTest host; hangs the whole assembly
- [ConfigController STA pump deadlock](project_configcontroller_sta_pump_deadlock.md) — STA tests must pump (DoEvents + Thread.Yield), not block on GetAwaiter()
- [QfcDatamodel BackgroundWorker async-void IsBusy race](project_qfc_backgroundworker_async_void_race.md) — assert WorkerSupportsCancellation instead
- [FluentAssertions Equal(params) has no because](project_fluentassertions_equal_params_no_because.md) — a trailing reason becomes an extra expected element

## Coverage measurement

- [Coverage delta: reproduce the baseline's counting method](project_coverage_delta_reproduce_baseline_counting_method.md) — deduped vs all-descendant give ~2x-different denominators
- [First-party coverage denominator method (#197)](project_coverage_firstparty_denominator_method.md) — per-`<line>` count across ALL deduped packages incl. vendored
- [dotnet-coverage denominator nondeterminism](project_dotnet_coverage_denominator_nondeterminism.md) — repo line-rate swings; re-baseline via git-stash, trust per-class rates
- [C# canonical coverage artifact conversion](project_csharp_canonical_coverage_artifact_conversion.md) — hook reads artifacts/csharp/coverage.xml as JaCoCo; convert feature Cobertura
- [Changed-line coverage: Cobertura vs MS-coverage](project_changed_line_coverage_cobertura_vs_mscoverage_partial.md) — use Cobertura per-line data for >=90% changed-line proofs
- [QFC #227 coverage tooling](project_qfc227_coverage_tooling.md) — vstest + Cobertura runsettings is the reliable numeric per-class path
- [Cobertura runsettings <Attributes> override](project_cobertura_runsettings_attributes_override.md) — a custom block silently disables [ExcludeFromCodeCoverage]; re-add it
- [runsettings DataCollector default-enabled](project_runsettings_datacollector_default_enabled.md) — a declared collector activates without /collect; enabled="false" then breaks /collect
- [dotnet-coverage Deedle/FSharp instrumentation](project_dotnet_coverage_deedle_fsharp_instrumentation.md) — pass a module-exclude settings XML; pair with Workers=4
- [ExcludeFromCodeCoverage on partial class = CS0579](project_excludefromcodecoverage_partial_class_cs0579.md) — annotate a partial type ONCE, not both parts
- [#398 test-split gate gotchas](project_398_test_split_gate_gotchas.md) — /EnableCodeCoverage has no branch%; use the Cobertura-runsettings variant
- [#400 CompleteOpenAsync unreachable recovery catch](project_400_completeopenasync_unreachable_recovery_catch.md) — dead code blocks >=90% test-only; escalate
- [Swordfish-removal incidental vendored-coverage drop](project_swordfish_removal_epic_incidental_coverage_sideeffect.md) — non-blocking side effect of deleting a ScoXxx wrapper
- [TaskVis #298 ScoCollection + live-bridge exemptions](project_taskvis_scocollection_and_livebridge_exemptions.md) — a default-factory live-form bridge must be method-level exempt

## Nullable epic

- [Nullable pragma-gate net481 mechanics](project_nullable_pragma_gate_net481_mechanics.md) — per-file gate can't hit EXIT 0; measure via prebuilt + BuildProjectReferences=false
- [Nullable per-file pragma gate mechanics](project_nullable_pragma_gate_mechanics.md) — solution-wide TWAE aborts on vendored SVGControl CS0649
- [Nullable epic: pragma gate + analyzer restore](project_nullable_epic_pragma_gate_and_analyzer_restore.md) — use a scoped TWAE with WarningsNotAsErrors; restore analyzer pkgs first
- [#364 nullable-gate pre-existing blockers](project_364_nullable_gate_preexisting_blockers.md) — the full-solution gate fails at baseline; verify via isolated UtilitiesCS build
- [Nullable remediation annotation patterns](project_nullable_remediation_annotation_patterns.md) — net481 has no post-condition attrs; `= default!`, `.ToString()!`, `x!.M()`
- [Nullable annotation CS8632 scoping](project_nullable_annotation_cs8632_scoping.md) — wrap in `#nullable enable annotations`, not whole-file
- [CS8714 does not fire on net481](project_nullable_cs8714_not_on_net481.md) — `where TKey : notnull` is forward-looking, not required
- [#366 notnull cascades past WrapperScoDictionary](project_366_notnull_cascades_beyond_wrapperscodictionary.md) — also forces the constraint onto ScoDictionaryConverter
- [#366 ScDictionary cascades to a 4th file](project_366_scdictionary_constraint_cascades_to_fourth_file.md) — beyond the 3-file waiver; STOP + re-escalate, don't widen
- [#366 Batch7 T? return triggers CS8766](project_366_batch7_tnullable_return_cs8766.md) — conform to `T` + justified `!`, don't edit the interface
- [#371 OutlookObjects nullable lessons](project_371_outlookobjects_nullable_lessons.md) — public-signature change regresses OTHER nullable files in the same assembly
- [#372 email-classifier nullable patterns](project_372_email_classifier_nullable_patterns.md) — post-ctor `null!`, `T?` factory returns, DTO `= null!` adds a coverage line
- [#375 residuals nullable gotchas](project_375_residuals_nullable_gotchas.md) — CS8644 fixed with a `#nullable disable` island; Rebuild cleans SVGControl.dll

## C# language and API gotchas

- [Outlook `Action`/`Exception` ambiguity](project_outlook_action_ambiguity.md) — bare `Action`/`Exception` are CS0104-ambiguous in interop files
- [init/record struct fails CS0518 on net48](project_record_struct_isexternalinit_netfx.md) — no IsExternalInit; use a readonly struct with get-only props
- [TimeProvider seam gotchas](project_timeprovider_seam_gotchas.md) — Moq can't mock non-virtual GetLocalNow; an optional param forces Bcl.TimeProvider on consumers
- [ScoDictionaryNew needs TryAdd not Add](project_scodictionarynew_tryadd_not_add.md) — `.Add(k,v)` won't compile (CS1061)

## Component-specific test gotchas

- [#349 breadcrumb WebView2 gotchas](project_349_breadcrumb_webview2_gotchas.md) — retyped Designer field breaks reflection-injected tests; inject a router
- [#227 cycle-4 ToggleFocus genuine-execution](project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md) — missing refs; use Activator.CreateInstance(field.FieldType)
- [Theme/FolderPredictor seam retrofit (#227 c3)](project_theme_folderpredictor_seam_retrofit_gotchas.md) — shared parameterless-ctor builder = silent regression
- [TaskController (#297) unit-test gotchas](project_taskvisualization_taskcontroller_test_gotchas.md) — ApplyChanges hangs over Moq; STA harness needs parenting + NavTips warmup
- [#328 Rebuild-threading breaks OlObjectsProxy](project_328_rebuild_threading_olobjectsproxy_conflict.md) — stub returns null for get_StoresWrapper; scope-lock the test doubles
- [ProjectEntry setter raw MessageBox](project_projectentry_setter_raw_messagebox.md) — the ProjectID setter is un-seamed; commits hang STA tests
- [ObjectListView TreeListView headless selection](project_objectlistview_treelistview_headless_selection.md) — needs a native handle; cache the node via SelectionChanged
