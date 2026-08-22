# Atomic Executor Memory Index

## Plan validation & gates
- [Verify line citations with numbered output](feedback_verify_line_citations_with_numbered_output.md) — never hand-count from a `sed` window; a wrong #438 advisory got applied and corrupted 3 correct citations in plan + spec
- [Self-derived gate thresholds are blind](project_preflight_selfderived_gate_thresholds_are_blind.md) — a "count >= floor" gate whose floor comes from the runs it validates is deflation-blind + scope-incommensurable; use
- [Multi-pattern gates detach shared qualifiers](project_multipattern_gate_shared_qualifier_detachment.md) — rewriting one clause re-scopes the trailing allowlist to the last pattern only; restate the carve-out per pattern +
- [Merge-base diff gates need a commit cadence](project_preflight_mergebase_diff_gates_need_commit_cadence.md) — `<MERGE_BASE>..HEAD` gates are vacuous while HEAD == merge-base; plan an explicit commit task
- [Inserted plan tasks force renumbering](project_plan_task_ids_digit_only_forces_renumbering.md) — suffixed IDs (`P3-T5a`) fail validation; say "insert + renumber downstream", then verify defs-vs-mentions mechanically
- [Plan rationale clauses are evidence](project_418_plan_rationale_clauses_are_evidence.md) — #418 needed 3 preflight passes; all blockers were unmeasured world-state claims in prose, never in the fix
- [AC check-off "inline pointer" + artifacts/ tool-output paths](project_preflight_ac_checkoff_and_tooloutput_paths.md) — inline evidence pointer in an AC task violates the tracking skill; artifacts/*/coverage XML is a producer path
- [Exact-count gate vs remediation loop](project_exact_count_gate_vs_remediation_loop.md) — a pinned `TotalCount = 19` collides with an "add tests and restart" remediation path; use `B + N` and re-scan count
- [Tracked agent-memory breaks unscoped git gates](project_agent_memory_tracked_breaks_unscoped_git_gates.md) — `.claude/agent-memory/**` is tracked + dirty; every git diff/status gate needs an explicit pathspec
- [Absolute-zero gate on a sibling-owned assembly + pinned WS](project_preflight_absolute_zero_gate_on_sibling_owned_assembly.md) — `Failed 0` on an assembly a sibling owns is unsatisfiable; never pin an absolute WS in preparation mode
- [#418 500-line gate vs mandated plan content](project_418_500line_gate_vs_plan_content.md) — P1-T19 unsatisfiable (193 new lines into 146 headroom); per-block logging clauses block centralizing; delta = extract
- [#207 Hook() redesign breaks AppEventsTests](project_207_hook_redesign_breaks_appeventstests.md) — needs a plan revision, not a test weakening
- [ApplicationGlobalsTests.cs at 500-line ceiling](project_appglobalstests_at_500_line_ceiling.md) — exactly 500 lines; adding a Testable* override requires extraction first
- [#376 capstone scope-expansion layers](project_376_capstone_scope_expansion_layers.md) — 5 escalated layers resolved via the 3 authorized patterns
- [Swordfish F5 test misclassification](project_swordfish_f5_test_misclassification.md) — verify using/namespace before treating a removal as Swordfish-only

## Build / toolchain environment
- [pwsh/git/gh CLI gotchas](project_pwsh_git_gh_cli_gotchas.md) — jq NOT installed (only `gh --jq`); pwsh won't concatenate `$(git merge-base ...)..HEAD`; bare `packages.config`
- [Project Build/Test Env](project_build_test_env.md) — git-bash quirks (MSBuild switches, MSYS_NO_PATHCONV), csharpier v1 syntax, legacy csproj Compile includes, IVT for Moq
- [VS18 build/test toolchain paths](project_vs18_build_toolchain_paths.md) — use VS **18** full-framework msbuild.exe (not .dotnet-sdk, dies on binary resx MSB3822); nuget.exe restore
- [Repo-local SDK install + nullable Rebuild](project_repo_sdk_and_nullable_rebuild.md) — .dotnet-sdk install needs pwsh7; csharpier check/format subcommands; nullable debt scope NOT stable across sessions —
- [vstest TestCaseFilter OR-vs-pipe + fresh-worktree bootstrap](project_vstest_testcasefilter_or_operator_and_env_setup.md) — vstest rejects `OR`, needs `|`; fresh worktree needs restore + global `dotnet-coverage`
- [Analyzer version skew on fresh worktree](project_analyzer_version_skew_fresh_worktree.md) — analyzer build can fail CS0006 when csproj `<Analyzer Include>` HintPaths diverge from packages.config; compare the two
- [SecurityCodeScan incompatible with Roslyn 5.6](project_securitycodescan_roslyn56_incompat.md) — SecurityCodeScan.VS2019 5.6.7 throws CS8032/YamlDotNet under VS18, breaking TWAE; Meziantou/Roslynator need
- [Missing VSTO runtime breaks baseline gates](project_missing_vsto_runtime_breaks_baseline_gates.md) — HISTORICAL, not reproducing 2026-08-08; build before citing
- [New sln member surfaces MSB3277](project_new_sln_member_surfaces_msb3277_pin_divergence.md) — wiring an unbuilt legacy test project into the sln emits a new MSB3277 when packages.config pins diverge; fix in
- [Legacy csproj: no transitive compile refs](project_legacy_csproj_no_transitive_compile_refs.md) — non-SDK ProjectReference doesn't flow package types to csc (CS0012 despite copy-local DLL); tests need their own
- [sln/csproj edits: preserve CRLF](project_sln_csproj_edit_crlf_preserve.md) — git-bash `sed -i` strips CRLF from TaskMaster.sln (churn + BOM loss); use Edit or `perl -0777` w/ explicit `\r\n`
- [Incremental build makes a vacuous baseline](project_incremental_build_vacuous_baseline.md) — Invoke-VSBuild's /t:Build up-to-date check ignores /p: changes → EXIT 0 with 0 CoreCompile; add /t:Rebuild to enumerate
- [Nullable /t:Build gate is vacuous](project_nullable_build_gate_is_vacuous_incremental.md) — standard nullable gate passes without type-checking; isolated `/t:Rebuild` exposed 223 errors (no /p:OutputPath)
- [CSharpier 1.3.0 formats XML at 100 cols](project_csharpier_formats_xml_print_width.md) — a "reformatting churn" finding on an XML resource can be formatter-mandated; measure line length + run repo-wide
- [Evidence <TS> collision clobbers committed artifacts](project_evidence_timestamp_collision_clobbers_artifacts.md) — same-day remediation silently overwrites implementation evidence; a ` M` under evidence/ means clobber
- [csharpier pipe-files is a non-enforcing gate](project_csharpier_pipefiles_nonenforcing_gate.md) — use `csharpier check`/`format`; tests balloon past 500 lines under genuine format (size new files AFTER format)
- [Count-idiom pitfalls: csharpier + Measure-Object](project_count_idiom_pitfalls_csharpier_and_measureobject.md) — "Formatted N files" is a PROCESSED count (hash before/after for rewrites); `Measure-Object -Line` omits blank lines
- [PowerShell new files need UTF-8 BOM](powershell-bom-required.md) — PSScriptAnalyzer enforces PSUseBOMForUnicodeEncodedFile; prepend BOM after Write or restart the format loop
- [poshqc test MCP carries no verdict and no numbers](project_poshqc_pester_mcp_exit_minus1.md) — returns only {ok,tool,workspace_root,summary} - no exit code/counts, so any EXIT_CODE or count gate on it is vacuous
- [poshqc analyze exits 1 on a Warning](project_poshqc_analyze_exit1_on_warning.md) — "EXIT_CODE 0 with zero error-severity" is self-contradictory; Helpers.ps1 carries a pre-existing PSUseSingularNouns
- [BOM breaks grep ^ anchor](project_bom_grep_anchor_false_negative.md) — bash grep `^#nullable` misses BOM-prefixed files; use the Grep tool for opt-in classification, never bash grep
- [StrictMode Latest + missing XML attribute throws](project_pester_strictmode_xml_attribute_property_access.md) — a fixture missing `branch`/`complexity` throws PropertyNotFoundStrict; enumerate ALL bare `$node.attr` reads at once
- [Pester 5 result shape: no container .Tests, no -CI + -CodeCoverage](project_pester5_result_shape_container_tests_and_ci_codecoverage.md) — `$_.Tests.Count` renders 0 per file (use TotalCount); `-CI` and `-CodeCoverage` are different parameter sets
- [pwsh -Command needs single-quoted outer](project_pwsh_command_quoting_from_bash.md) — a double-quoted outer wrapper lets bash eat `$` → empty counts + inverted exit gates or a hard ParserError; `''` inside

- [Compile-time red needs body-level refs](project_compile_red_needs_body_level_references.md) — a missing type in a method SIGNATURE suppresses body binding, so an `[expect-fail]` task requiring N named CS0246s

## Test execution & isolation
- [Long runs need a detached process](project_long_runs_need_detached_process.md) — Bash `run_in_background` runners get killed after ~1h, taking `Start-Job` load generators with them; use `Start-Process -PassThru` + foreground `sleep 570` to advance real time
- [Tests must mock GUI; no visible window](feedback_tests_must_mock_gui_no_visible_window.md) — use headless seams (mocked viewers, injected show/focus delegates), never Form.Show/Application.Run
- [#511 is a test-host crash, not N failing tests](project_511_is_a_testhost_crash_not_n_failing_tests.md) — load-driven abort with `Total tests: Unknown` (no readable verdict); `/InIsolation` loop gave 0 failed; never gate on a
- [WinFormsPumpHost tests are load-flaky](project_winformspumphost_tests_load_flaky.md) — QfcItemController_InitializationTests fail with "window handle has been created"/60s timeouts when the box is
- [vstest /InIsolation + FilePathHelper serialization](project_vstest_isolation_and_filepathhelper_serialization.md) — Moq assemblies need /InIsolation (else STTE Setup FileNotFound); FilePathHelper.FilePath is "" default but null after
- [Invoke-MSTest.ps1 dies on a single test assembly](project_418_invoke_mstest_single_assembly_bug.md) — StrictMode + `.Count` on a scalar String throws before vstest runs; call vstest.console.exe directly with the script's
- [Timed-out MSTest leaves detached runner](project_timedout_mstest_leaves_detached_runner.md) — leaves a pwsh runner respawning testhosts → user.config hangs; kill the pwsh runner too, verify 0, rerun >=8min
- [Sibling-worktree shared-tooling hazard](project_sibling_worktree_shared_tooling_hazard.md) — a concurrent agent elsewhere crashes your testhost + clobbers /tmp logs; use the session scratchpad
- [Concurrent dotnet-coverage deadlock + doc-comment retention gate](project_concurrent_dotnet_coverage_deadlock_and_doccomment_retention_gate.md) — sibling worktree's coverage run hangs yours (CPU-delta test, kill only your chain); an XML doc quoting a pinned literal
- [Concurrent executor in same worktree](project_concurrent_executor_same_worktree.md) — two executors on one worktree corrupt shared files; detect via mtime progression during your own turn; STOP, don't
- [UtilitiesCS.Test parallelism flakiness](project_utilitiescs_test_parallelism_flakiness.md) — timing tests time out (~22s) under default parallelism + coverage; lower MSTest Workers to 4 via /Settings for a
- [MSTest [DoNotParallelize] overlaps the parallel bucket](project_mstest_donotparallelize_overlaps_parallel_bucket.md) — a [DoNotParallelize] null-baseline reader still sees parallel-bucket writers; mark every writer too
- [UiThread.Dispatcher static-swap race](project_uithread_dispatcher_static_swap_race.md) — two classes swapping the shared static deadlock on the parked dispatcher; serialize swap-to-restore with a
- [runsettings DataCollector default-enabled](project_runsettings_datacollector_default_enabled.md) — a declared Code Coverage `<DataCollector>` activates under CLI vstest without /collect; enabled="false" then breaks
- [dotnet-coverage Deedle/FSharp instrumentation breaks tests](project_dotnet_coverage_deedle_fsharp_instrumentation.md) — pass a module-exclude settings XML to dotnet-coverage (runsettings excludes don't propagate); pair with Workers=4
- [DispatcherDelay hangs unit tests](project_dispatcherdelay_hangs_unit_tests.md) — WaitAsync never completes in the pump-less MSTest host and hangs the whole assembly; drive coverage via dotnet-coverage
- [ConfigController STA pump deadlock](project_configcontroller_sta_pump_deadlock.md) — SaveAsync posts its continuation to the WinForms STA queue; an STA test must pump (DoEvents + Thread.Yield), not block

## Coverage measurement
- [Coverage delta: reproduce the baseline's counting method](project_coverage_delta_reproduce_baseline_counting_method.md) — Cobertura repeats lines under `<method>` AND class `<lines>`; deduped vs all-descendant give ~2x denominators
- [First-party coverage denominator method (#197)](project_coverage_firstparty_denominator_method.md) — production-only rate = per-`<line>` count across ALL deduped packages INCLUDING vendored Swordfish/SVGControl
- [dotnet-coverage denominator nondeterminism](project_dotnet_coverage_denominator_nondeterminism.md) — repo line-rate swings (47% vs 81%) from double-counted denominator; re-baseline via git-stash, trust per-class rates
- [Koverage Cobertura post-processing shape](project_koverage_cobertura_postprocessing_shape.md) — rewrites filenames with `\`, pre-merges per-file `<class>`, strips test packages; forward-slash queries match nothing
- [C# canonical coverage artifact conversion](project_csharp_canonical_coverage_artifact_conversion.md) — hook reads artifacts/csharp/coverage.xml as JaCoCo (85% floor); defer repo-wide to PR CI
- [Cobertura runsettings `<Attributes>` override](project_cobertura_runsettings_attributes_override.md) — a custom `<CodeCoverage>` block replaces the default `<Attributes>` excludes, silently disabling
- [Changed-line coverage: Cobertura hits vs MS-coverage partial](project_changed_line_coverage_cobertura_vs_mscoverage_partial.md) — null-guard throws read "partially covered" in MS.CodeCoverage XML but hits=1 in Cobertura; use Cobertura for >=90%
- [QFC #227 coverage tooling](project_qfc227_coverage_tooling.md) — vstest + Cobertura runsettings (Format under Configuration + attribute-exclude) is the reliable per-class path
- [#398 test-split gate gotchas](project_398_test_split_gate_gotchas.md) — pre-existing CS2002 duplicate Compile (latent, out of scope); /EnableCodeCoverage has no branch% + .coverage merges to
- [ExcludeFromCodeCoverage on partial class = CS0579](project_excludefromcodecoverage_partial_class_cs0579.md) — annotate a partial type ONCE, not both parts, or the build breaks with duplicate-attribute CS0579
- Closed one-offs (low reuse): [#400 dead recovery catch](project_400_completeopenasync_unreachable_recovery_catch.md), [Swordfish vendored-coverage side effect](project_swordfish_removal_epic_incidental_coverage_sideeffect.md), [#298 ScoCollection/live-bridge exemptions](project_taskvis_scocollection_and_livebridge_exemptions.md), [#328 OlObjectsProxy](project_328_rebuild_threading_olobjectsproxy_conflict.md)

## Nullable / C# language
- [Nullable per-file pragma gate mechanics](project_nullable_pragma_gate_mechanics.md) — CI's command (`/t:Rebuild`, TWAE, NO `/p:Nullable=enable`) genuinely passes EXIT 0; the 195+219 error population
- [CLAUDE.md nullable command != the CI gate](project_507_nullconditional_return_triggers_cs8603_under_genuine_nullable_check.md) — ci.yml omits `/p:Nullable=enable`; forced-flag CS86xx in an unannotated file is NOT a blocker - repro the CI command
- [Nullable remediation annotation patterns](project_nullable_remediation_annotation_patterns.md) — net481 has no post-condition attrs; struct `= default!`; `.ToString()!` for string cells; IsNullOrEmpty overload
- [Nullable annotation CS8632 scoping](project_nullable_annotation_cs8632_scoping.md) — `Type?` in nullable-disabled projects emits CS8632; wrap in `#nullable enable annotations`, not whole-file
- [init/record struct fails CS0518 on net48](project_record_struct_isexternalinit_netfx.md) — no IsExternalInit polyfill; use a ctor-initialized readonly struct with get-only props
- [CS8714 does not fire on net481](project_nullable_cs8714_not_on_net481.md) — net481 BCL lacks notnull, so `where TKey : notnull` is forward-looking only
- Nullable-epic per-issue notes (closed epic, low reuse): [#366 notnull cascade](project_366_notnull_cascades_beyond_wrapperscodictionary.md), [#366 4th file](project_366_scdictionary_constraint_cascades_to_fourth_file.md), [#366 CS8766](project_366_batch7_tnullable_return_cs8766.md), [#372](project_372_email_classifier_nullable_patterns.md), [#371](project_371_outlookobjects_nullable_lessons.md), [#375](project_375_residuals_nullable_gotchas.md)
- [Outlook `Action`/`Exception` ambiguity](project_outlook_action_ambiguity.md) — bare `Action` AND bare `Exception` are CS0104-ambiguous in Outlook-interop files; use

## Component-specific gotchas
- [WebView2 EndInit already creates child handles](project_webview2_endinit_creates_handles.md) — `new ItemViewer()` alone leaves BOTH WebView2 children handle-created, so a `IsHandleCreated == false` assertion is unsatisfiable; also 3 pre-existing UtilitiesCS.Test flakes (shared Console.Out) that break any all-assembly `failed == 0` gate
- [#349 breadcrumb WebView2 gotchas](project_349_breadcrumb_webview2_gotchas.md) — retyped Designer field breaks reflection-injected tests; aggregate async d__ classes for >=90%
- QuickFiler #227 cycle notes: [cycle-4 ToggleFocus](project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md), [cycle-3 Theme/FolderPredictor seam](project_theme_folderpredictor_seam_retrofit_gotchas.md)
- [ObjectListView TreeListView headless selection](project_objectlistview_treelistview_headless_selection.md) — selection needs a native handle; cache the node via SelectionChanged
- [QfcDatamodel BackgroundWorker async-void IsBusy race](project_qfc_backgroundworker_async_void_race.md) — IsBusy flips false instantly; assert WorkerSupportsCancellation
- [QfcItemController pump harness needs SaveParameters](project_qfcitemcontroller_pump_harness_needs_saveparameters.md) — SetField-only injection leaves `??=` factory defaults null (NRE in LoadFolderHandlerAsync)
- [TaskController (#297) unit-test gotchas](project_taskvisualization_taskcontroller_test_gotchas.md) — ApplyChanges hangs over Moq; get-only MailItem.TaskSubject throws; STA harness needs TableLayoutPanel parenting
- [ProjectEntry setter raw MessageBox](project_projectentry_setter_raw_messagebox.md) — the ProjectID setter uses RAW un-seamed MessageBox.Show and hangs STA tests; CompareTo tie-break needs a Moq
- [IApplicationGlobals member forces implementers](project_iapplicationglobals_member_forces_implementers.md) — adding a member breaks 7 hand-written test-double stubs beyond scope lock; Moq mocks auto-implement
- [TimeProvider seam gotchas](project_timeprovider_seam_gotchas.md) — Moq can't mock non-virtual GetLocalNow (use FakeTimeProvider); an optional TimeProvider param forces a Bcl.TimeProvider
- [ScoDictionaryNew needs TryAdd not Add](project_scodictionarynew_tryadd_not_add.md) — retargeting Sco* tests: `.Add(k,v)` won't compile (CS1061); the base exposes `.TryAdd`; swap in the same edit
- [FluentAssertions Equal(params) has no because](project_fluentassertions_equal_params_no_because.md) — a trailing reason on `.Equal(...)` becomes an extra expected element; use `.Equal(new[]{...})` or move the reason to
