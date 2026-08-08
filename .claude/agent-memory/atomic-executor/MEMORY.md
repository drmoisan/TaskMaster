# Atomic Executor Memory Index

## Plan / preflight validation

- [Inserted plan tasks force renumbering](project_plan_task_ids_digit_only_forces_renumbering.md) — suffixed IDs fail validation; say "insert + renumber downstream", verify defs vs mentions
- [Preflight evidence fields need a token scan](project_preflight_evidence_field_token_scan.md) — prose "the `git diff ...` command" omits literal `Command:`; scan tokens + order, non-blocking
- [Plan rationale clauses are evidence](project_418_plan_rationale_clauses_are_evidence.md) — preflight blockers hide in unmeasured world-state claims in prose, not in the fix
- [500-line gate vs mandated plan content](project_418_500line_gate_vs_plan_content.md) — per-block logging clauses block centralizing; delta = extract pure helpers to a new file
- [#376 capstone scope-expansion layers](project_376_capstone_scope_expansion_layers.md) — 5 escalated layers resolved via the 3 authorized patterns; stop-condition never triggered
- [#207 Hook() redesign breaks AppEventsTests](project_207_hook_redesign_breaks_appeventstests.md) — out-of-scope test asserts superseded ordering; needs a plan revision, not a weakened test
- [#400 CompleteOpenAsync unreachable catch](project_400_completeopenasync_unreachable_recovery_catch.md) — dead code can't reach >=90% test-only; escalate; 17-class gate deadlocks testhost
- [Concurrent executor in same worktree](project_concurrent_executor_same_worktree.md) — two executors corrupt shared files; detect via mtime progression; STOP, don't stash/race
- [Sibling-worktree shared-tooling hazard](project_sibling_worktree_shared_tooling_hazard.md) — other worktrees crash your testhost via shared vstest; use session scratchpad
- [ApplicationGlobalsTests.cs at 500-line ceiling](project_appglobalstests_at_500_line_ceiling.md) — exactly 500 lines; any plan adding an override must extract first

## Build / toolchain environment

- [Project Build/Test Env](project_build_test_env.md) — git-bash MSBuild dash-switches, MSYS_NO_PATHCONV, csharpier v1, legacy csproj Compile includes, IVT, C# 7.3
- [VS18 build/test toolchain paths](project_vs18_build_toolchain_paths.md) — use VS 18 full-framework msbuild (not Core SDK); nuget restore; dotnet-coverage needs `--`
- [Repo-local SDK install + nullable Rebuild](project_repo_sdk_and_nullable_rebuild.md) — .dotnet-sdk needs pwsh7; nullable debt scope is NOT stable across sessions, re-verify
- [vstest TestCaseFilter pipe + worktree bootstrap](project_vstest_testcasefilter_or_operator_and_env_setup.md) — `/TestCaseFilter` needs `|` not `OR`; fresh worktree needs SDK + tool restore first
- [Analyzer version skew on fresh worktree](project_analyzer_version_skew_fresh_worktree.md) — first analyzer build fails CS0006; nuget install the old versions into gitignored packages/
- [SecurityCodeScan incompatible with Roslyn 5.6](project_securitycodescan_roslyn56_incompat.md) — 5.6.7 throws CS8032/YamlDotNet under VS18, breaking the TWAE gate
- [Missing VSTO runtime breaks baseline gates](project_missing_vsto_runtime_breaks_baseline_gates.md) — absent Office Tools v4.0.Utilities => CS0234; test projects never build, coverage reads low
- [Incremental build makes a vacuous baseline](project_incremental_build_vacuous_baseline.md) — /t:Build up-to-date checks ignore /p: changes; add a /t:Rebuild to enumerate diagnostics
- [csharpier pipe-files is a non-enforcing gate](project_csharpier_pipefiles_nonenforcing_gate.md) — use `csharpier check`/`format`; size new test files AFTER format, not by hand count
- [sln/csproj edits: preserve CRLF](project_sln_csproj_edit_crlf_preserve.md) — git-bash `sed -i` strips CRLF; use Edit tool or `perl -0777` with explicit `\r\n`
- [New sln member surfaces MSB3277](project_new_sln_member_surfaces_msb3277_pin_divergence.md) — packages.config pin divergence; only fixable in packages.config/.csproj, never a .cs file
- [Legacy csproj: no transitive compile refs](project_legacy_csproj_no_transitive_compile_refs.md) — tests naming a third-party type need their own `<Reference>` + packages.config entry
- [PowerShell new files need UTF-8 BOM](powershell-bom-required.md) — PSScriptAnalyzer enforces PSUseBOMForUnicodeEncodedFile; prepend BOM after Write
- [poshqc Pester MCP exits -1](project_poshqc_pester_mcp_exit_minus1.md) — run it for the record, pair with direct Invoke-Pester (pwsh7) for the numeric proof
- [BOM breaks grep ^ anchor](project_bom_grep_anchor_false_negative.md) — bash grep `^#nullable` misses BOM-prefixed files; use the Grep tool for classification

## Coverage measurement

- [Coverage delta: reproduce the baseline's method](project_coverage_delta_reproduce_baseline_counting_method.md) — Cobertura repeats lines; deduped vs all-descendant give ~2x denominators
- [First-party coverage denominator method](project_coverage_firstparty_denominator_method.md) — per-`<line>` count across ALL deduped packages including vendored ones
- [C# canonical coverage artifact conversion](project_csharp_canonical_coverage_artifact_conversion.md) — hook reads artifacts/csharp/coverage.xml as JaCoCo; convert feature Cobertura
- [dotnet-coverage denominator nondeterminism](project_dotnet_coverage_denominator_nondeterminism.md) — repo line-rate swings from a double-counted denominator; trust per-class rates
- [Changed-line coverage: Cobertura vs MS-coverage](project_changed_line_coverage_cobertura_vs_mscoverage_partial.md) — guard lines read "partial" in MS XML but hits=1 in Cobertura; use Cobertura
- [QFC #227 coverage tooling](project_qfc227_coverage_tooling.md) — vstest + Cobertura runsettings is the reliable numeric per-class path; .coverage not convertible here
- [Cobertura runsettings `<Attributes>` override](project_cobertura_runsettings_attributes_override.md) — a custom `<CodeCoverage>` block silently disables [ExcludeFromCodeCoverage]
- [runsettings DataCollector default-enabled](project_runsettings_datacollector_default_enabled.md) — a declared collector activates without /collect; enabled="false" then breaks /collect
- [ExcludeFromCodeCoverage on partial class = CS0579](project_excludefromcodecoverage_partial_class_cs0579.md) — annotate a partial type ONCE, not both parts
- [#398 test-split gate gotchas](project_398_test_split_gate_gotchas.md) — /EnableCodeCoverage has no branch%; use the Cobertura-runsettings variant + Workers=4
- [Swordfish-removal: incidental coverage side effect](project_swordfish_removal_epic_incidental_coverage_sideeffect.md) — deleting a ScoXxx wrapper drops incidental vendored coverage; non-blocking

## Test execution hazards

- [Timed-out MSTest leaves detached runner](project_timedout_mstest_leaves_detached_runner.md) — kill the pwsh runner too, verify 0, then rerun >=8min
- [UtilitiesCS.Test parallelism flakiness](project_utilitiescs_test_parallelism_flakiness.md) — lower MSTest Workers to 4 via /Settings runsettings for a deterministic green gate
- [dotnet-coverage Deedle/FSharp instrumentation](project_dotnet_coverage_deedle_fsharp_instrumentation.md) — pass a module-exclude settings XML to dotnet-coverage; pair with Workers=4
- [vstest /InIsolation + FilePathHelper](project_vstest_isolation_and_filepathhelper_serialization.md) — Moq assemblies need /InIsolation; FilePath is "" default but null after deserialize
- [DispatcherDelay hangs unit tests](project_dispatcherdelay_hangs_unit_tests.md) — WaitAsync never completes in the pump-less MSTest host and hangs the whole assembly
- [ConfigController STA pump deadlock](project_configcontroller_sta_pump_deadlock.md) — STA tests must pump (DoEvents + Thread.Yield), not block on GetAwaiter().GetResult()
- [MSTest [DoNotParallelize] overlaps parallel bucket](project_mstest_donotparallelize_overlaps_parallel_bucket.md) — a null-baseline reader still sees parallel writers; mark every writer too
- [Invoke-MSTest.ps1 dies on one test assembly](project_418_invoke_mstest_single_assembly_bug.md) — StrictMode `.Count` on a scalar String throws; call vstest.console.exe directly
- [FluentAssertions Equal(params) has no because](project_fluentassertions_equal_params_no_because.md) — a trailing reason becomes an extra expected element; use `.Equal(new[]{...})`

## C# / test authoring patterns

- [Outlook `Action`/`Exception` ambiguity](project_outlook_action_ambiguity.md) — bare `Action`/`Exception` are CS0104-ambiguous in interop files; qualify with `System.`
- [init/record struct fails CS0518 on net48](project_record_struct_isexternalinit_netfx.md) — no IsExternalInit; use a constructor-initialized readonly struct with get-only props
- [TimeProvider seam gotchas](project_timeprovider_seam_gotchas.md) — Moq can't mock non-virtual GetLocalNow; an optional TimeProvider param forces a `<Reference>` (CS0012)
- [IApplicationGlobals member forces implementers](project_iapplicationglobals_member_forces_implementers.md) — a new member breaks hand-written stubs beyond scope lock; Moq auto-implements
- [ObjectListView TreeListView headless selection](project_objectlistview_treelistview_headless_selection.md) — selection needs a native handle; cache the node via SelectionChanged
- [ProjectEntry setter raw MessageBox](project_projectentry_setter_raw_messagebox.md) — the ProjectID setter uses an un-seamed MessageBox.Show and hangs STA tests
- [QfcDatamodel BackgroundWorker async-void race](project_qfc_backgroundworker_async_void_race.md) — IsBusy flips false instantly; assert WorkerSupportsCancellation instead
- [ScoDictionaryNew needs TryAdd not Add](project_scodictionarynew_tryadd_not_add.md) — `.Add(k,v)` won't compile (CS1061); the base exposes `.TryAdd`
- [TaskController (#297) unit-test gotchas](project_taskvisualization_taskcontroller_test_gotchas.md) — ApplyChanges hangs over Moq; STA harness needs TableLayoutPanel parenting + warmup
- [#349 breadcrumb WebView2 gotchas](project_349_breadcrumb_webview2_gotchas.md) — a retyped Designer field breaks reflection-injected tests; inject a router
- [#227 cycle-4 ToggleFocus genuine-execution](project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md) — use Activator.CreateInstance(field.FieldType); ToggleFocus sees Invoke twice
- [Theme/FolderPredictor seam retrofit gotchas](project_theme_folderpredictor_seam_retrofit_gotchas.md) — a shared parameterless-ctor test-double builder causes silent regression
- [#328 Rebuild-threading breaks OlObjectsProxy](project_328_rebuild_threading_olobjectsproxy_conflict.md) — proxy stubs only get_App; return null for get_StoresWrapper; scope-lock the doubles
- [TaskVis #298 ScoCollection + live-bridge exemptions](project_taskvis_scocollection_and_livebridge_exemptions.md) — a default-factory live-form bridge needs a method-level exemption
- [Swordfish F5 test misclassification](project_swordfish_f5_test_misclassification.md) — verify using/namespace before treating a test removal as Swordfish-only

## Nullable epic

- [Nullable remediation annotation patterns](project_nullable_remediation_annotation_patterns.md) — net481 has no post-condition attrs; `= default!`, `.ToString()!`, `x!.M()`
- [Nullable per-file pragma gate mechanics](project_nullable_pragma_gate_mechanics.md) — verify via isolated `UtilitiesCS.csproj -t:Rebuild -p:BuildProjectReferences=false`
- [Nullable pragma-gate net481 mechanics](project_nullable_pragma_gate_net481_mechanics.md) — the per-file gate can't hit EXIT 0; measure CS86xx with SVGControl prebuilt
- [Nullable epic: pragma gate + analyzer restore](project_nullable_epic_pragma_gate_and_analyzer_restore.md) — use scoped TWAE with `/p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168`
- [#364 nullable-gate pre-existing blockers](project_364_nullable_gate_preexisting_blockers.md) — the full-solution pragma-only TWAE gate already fails at baseline
- [Nullable annotation CS8632 scoping](project_nullable_annotation_cs8632_scoping.md) — wrap in `#nullable enable annotations`/`restore annotations`, not whole-file
- [CS8714 does not fire on net481](project_nullable_cs8714_not_on_net481.md) — `where TKey : notnull` is forward-looking, not required
- [#371 OutlookObjects nullable lessons](project_371_outlookobjects_nullable_lessons.md) — keep public tuples non-null with `!` at null sites; lazy-field CS8618 → `Lazy<T>?`
- [#372 email-classifier nullable patterns](project_372_email_classifier_nullable_patterns.md) — post-ctor `null!`; factory returns `T?`; DTO `= null!` adds a coverage line
- [#375 residuals nullable gotchas](project_375_residuals_nullable_gotchas.md) — CS8644 fixed with a `#nullable disable` island; full Rebuild cleans SVGControl.dll
- [#366 notnull cascades past WrapperScoDictionary](project_366_notnull_cascades_beyond_wrapperscodictionary.md) — the constraint also lands on ScoDictionaryConverter
- [#366 ScDictionary cascades to a 4th file](project_366_scdictionary_constraint_cascades_to_fourth_file.md) — beyond the 3-file waiver; STOP and re-escalate, don't widen
- [#366 Batch7 `T?` return triggers CS8766](project_366_batch7_tnullable_return_cs8766.md) — conform to `T` with a justified `!`; don't edit the null-oblivious interface

## Additional entries

- [Plan literals inherit research arithmetic errors](project_plan_literal_assertions_inherit_research_arithmetic.md) — recompute every quoted expected literal at preflight
