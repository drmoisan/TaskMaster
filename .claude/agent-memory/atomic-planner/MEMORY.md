# Atomic Planner Memory Index

- [pwsh -Command quoting in plan tasks](pwsh-command-quoting-in-plan-tasks.md) — outer SINGLE quotes, inner double; outer-double is eaten by the calling Bash/PowerShell and fakes a Phase 0 halt

- [Dead-code removal vs coverage exclusion](project_deadcode_removal_vs_coverage_exclusion.md) — coverage gate blocked by unreachable dead prod code → plan removal (shrink denominator), never exclusion/carve-out/forced-rethrow
- [Dead code retained: residual to the ledger](deadcode-retained-residual-to-ledger.md) — when deletion is routed out of scope, measure the real rate and request ratification; never exclude, never reflect-invoke private statics to inflate
- [Coverage gate on CLR-invoked private members](coverage-gate-clr-invoked-private-members.md) — never gate AssemblyResolve-style private members at >=90%; split newly-added vs changed per the AC's own wording
- [Nullable context mismatch: prod vs test](project_nullable_context_mismatch_prod_vs_test.md) — check `#nullable enable` in the prod file AND missing `<LangVersion>` (C# 7.3) in the test csproj; adding `<LangVersion>latest</LangVersion>` is never "one property"
- [C# pure-move extraction pattern](csharp-pure-move-extraction-pattern.md) — moving members out of a 497/500-line file: keep the static-ctor install trigger, route testable members to an existing covered class, declare relocation-not-new-module
- [Research claims as acceptance clauses](research-claims-as-acceptance-clauses.md) — never encode an unmeasured third-party null-vs-throw claim as a literal AC clause; #418 §1.4 empty-bytes claim cost a revision pass
- [Enumerate condition outcomes before the case list](enumerate-condition-outcomes-before-case-list.md) — 100% branch-rate tasks: 2 outcomes per condition in every ||/&& clause; a null-pairing guard needs all four orderings, not three
- [Size test fixtures by measured lines-per-test](test-fixture-sizing-lines-per-test.md) — 17 methods is the sub-500 ceiling for mock-heavy QuickFiler.Test/Controllers; cap 16/12, split via `.PartN.cs` partials with no repeated [TestClass]
- [Per-phase size gates need a scoped csharpier](per-phase-size-gates-need-scoped-csharpier.md) — an interim 500-line gate measured before CSharpier runs is not load-bearing; same for any "byte-identical after CSharpier" clause
- [Named coverage exception: verify the member body](named-coverage-exception-verify-member-body.md) — read the member before writing "untestable branch"; put gap-closure BEFORE the toolchain-clean-pass task; pin line-rate vs branch-rate
- [Re-derive plan aggregate claims after every delta](plan-aggregate-claims-must-be-rederived-after-deltas.md) — stale branch/file counts and acceptance that only one arm can satisfy; validator never cross-checks prose against the task list

## Additional entries

- [Partial-class seam: declare and consume in the same phase](partial-class-seam-declaration-and-consumption-same-phase.md) — per-file phases must not split a seam's declaration from its consumption across two partials; dead code fails the analyzer build and blocks the earlier phase's coverage gate
- [Seam default CS0236 + intermediate consumers](csharp-seam-default-cs0236-and-intermediate-consumers.md) — a delegate-seam default capturing `_field` in a property initializer is CS0236 (use backing field + lazy default); extraction rewires must check lines BETWEEN the call sites for pre-transform consumers
- [AC source sweep: Definition of Done](ac-source-sweep-definition-of-done.md) — every AC needs a verification task, and spec.md's `## Definition of Done` checkboxes are AC-source too (`## Seeded Test Conditions` are not)
- [Plan self-consistency sweeps](plan-self-consistency-sweeps.md) — pre-preflight: preamble-vs-task reachability, post-format size check on ALL new test files, and a named input for every "demonstrated against a concrete file" gate

## Additional entries

- [#453 QfcItemController plan seams](project_453_qfcitemcontroller_plan_seams.md) — AC-8 caps de-exemption at 15 and overrides 3 research artifacts; AC-4 = tests-first ordering not bundling; FlagTasks ctor touches COM; #441 false pass
- [Stale-worktree guard must be repo-relative](stale-worktree-guard-must-be-repo-relative.md) — absolute `\.claude\` match flags the executing agent worktree's own DLLs and can never pass after a build; anchor to `(Resolve-Path .)`
- [Baseline-relative gates; vacuous diff comparators](baseline-relative-toolchain-gates-and-vacuous-diff-comparators.md) — never assert absolute exit 0 for solution-wide CMD-ANALYZE/CMD-NULLABLE; `<merge-base>..HEAD` proves nothing when the plan never commits
- [Seam shape: cardinality and mutability](seam-shape-must-match-target-cardinality-and-mutability.md) — per-element targets need a stateless facade (target as 1st param); reassigned fields need accessor delegates, never a readonly snapshot; re-attribute downstream tasks after narrowing a facade
- [Task counts must be mechanical and recorded](task-counts-must-be-mechanical-and-recorded.md) — count `^- \[ \] \[P\d+-T\d+\]` matches, record per-phase totals in the plan header; line-vs-unique-ID divergence means a duplicate ID

## Additional entries

- [Verify line spans and computed literals](verify-line-spans-and-computed-literals.md) — replace-L<a>-L<b> spans swallow still-referenced field decls; recompute asserted literals incl. format rounding (`##0.00` rounds away from zero → "0.67" not "0.66")
- [Never assert .Method.Name on a lambda-valued delegate](never-assert-method-name-on-lambda-valued-delegate.md) — classify each Production* default as named-method vs lambda before writing a ".Method.Name identity only" clause; lambdas need NotBeSameAs(sentinel)
- [Verify caller-supplied citation corrections](verify-caller-supplied-citation-corrections.md) — preflight "fix this line number" deltas are themselves sometimes off-by-one; re-read the source before transcribing one into a validated plan

- [#452 F9 EFC form/item/viewer plan seams](project_452_efc_form_item_viewer_plan_seams.md) — shared Phase 1 before per-file phases; viewer→form→item order; DEC-1 Form-construction IN/OUT task lists; IEfcFormViewer forward-member coverage trap
- [#437 EfcHomeController plan seams](project_437_efc_home_controller_plan_seams.md) — EFC test files at 459-476 lines force new files; MoveFailureMessageAction=MessageBox.Show CI hang; ClassLevel parallelism + Production* statics; Timing.cs has no clock
- [Coverage Evidence Path Normalization](evidence-path-normalization.md) — specs sometimes name evidence/coverage/; normalize to canonical baseline/ + qa-gates/
- [Stale build output is not evidence of existence](stale-build-output-is-not-evidence-of-existence.md) — obj/ cache filenames outlive tear-down commits; verify project/source files with git ls-files or a glob before writing an existence claim into acceptance text
- [Never pin a HEAD SHA as a plan expectation](never-pin-head-sha-as-plan-expectation.md) — record HEAD, gate on tree invariants (clean porcelain + no .cs/.csproj/packages.config/app.config diff vs the baseline-capture sha)
- [.csharpierignore scope: packages.config is NOT exempt](csharpierignore-scope-packages-config.md) — only *.csproj/*.props/*.targets are excluded; justify single-line package entries by character width, never by formatter exemption
- [CSharpier gate: format not pipe-files](csharpier-format-not-pipe-files-gate.md) — formatting tasks must use `csharpier format` + scoped `csharpier check` exit 0; `pipe-files` is stdout-only/non-enforcing and masked a 500-line overflow in #400
- [#400 partial-class headroom placement](project_400_partial_class_headroom_placement.md) — put new coverage cases in existing `.Part2.cs` `[TestClass] partial` files to keep the 17-class filter/count assertions stable
- [Manager AsyncLazy shared seam](project_manager_asynclazy_shared_seam.md) — Globals.AF.Manager is shared across all classifier subsystems; use a key-specific accessor, never retype the dictionary value for one key
- [Folder predictor AF holder seam](project_folder_predictor_af_holder_seam.md) — #177 F1: route flag-on LCPPN predictor through a Folder-only holder on IAppAutoFileObjects (globals.AF), not per-instance OlFolderClassifierGroup state
- [CRLF plans validate — do not normalize](crlf-plans-validate-do-not-normalize.md) — verified: all six epic-child plans returned ok:true as pure CRLF; never add an LF-normalization step (the csproj CRLF rule is unrelated and stays)
- [Plan validator phase-heading constraint](plan-validator-phase-heading-constraint.md) — MCP plan validator requires exact `### Phase N — <Title>`; no tokens between Phase N and em-dash; H1 title line is exempt
- [Plan validator task-ID sequential constraint](plan-validator-task-id-sequential-constraint.md) — task IDs must be digit-only and sequential-by-appearance; mid-phase insertion forces renumbering all later tasks + cross-refs
- [Legacy csproj wiring](project_legacy_csproj_explicit_compile_include.md) — fold `Compile Include` into the creating task (never a batched entry task); own `Reference` needed, ProjectReference gives no compile-time flow (CS0012)
- [Preflight delta with colliding task IDs](preflight-delta-colliding-task-ids.md) — when two findings pin the same ID, keep the ID cited by literal replacement text, place the reordered task adjacent, and report the deviation

## Additional entries

- [Decomposition must cover newly-inserted tasks](decomposition-must-cover-newly-inserted-tasks.md) — a revision that decomposes bundled measure/split/register tasks must also fix the ones it itself inserts, and re-derive the Decision record's enumeration
- [C# Phase 0 toolchain bootstrap](project_csharp_phase0_toolchain_bootstrap.md) — .dotnet-sdk/ absent + no dotnet tool restore + no dotnet-coverage; make it [P0-T1] or all csharpier/coverage tasks fail
- [#211 startup-lifetime heartbeat seam](project_211_startup_lifetime_heartbeat_seam.md) — Phase 3.3 [startup-lifetime-heartbeat] DispatcherTimer in ThisAddIn.cs (exempt), pure logic in StartupDiagnosticsProbe; AC15
- [#292 CurrentStoreContext parallel seam](project_292_currentstorecontext_parallel_seam.md) — process-global static; scope-opening store test classes must be [DoNotParallelize] or they pollute reader-baseline tests under UtilitiesCS.Test ClassLevel parallelization
- [WinForms STA-refinement exemption rule](project_winforms_sta_refinement_exemption_rule.md) — epic #295 STA refinement: remove HWND-only default-body + PerformClick-wiring exemptions via dedicated *.StaTests.cs; keep dialog/Form/launcher exemptions
- [STA last-resort control-identity plan pattern](project_sta_last_resort_control_identity_pattern.md) — epic #295: measure control-identity partials via companion interface (real Label/Control) + *.StaTests.cs ([STATestClass], MSTest 4.2.2); never construct Form; handle/pump residue stays method-level exempt
- [#307 F2 ScoCollection deletion gate](project_307_f2_scocollection_deletion_gate.md) — full first-party ScoCollection/ScoStack reference set incl. tests beyond spec §7; ISubjectMapSco/IScoCollection F5 boundary; FS/Prompt seams live in ScoCollection.cs
- [#328 store-exclusion seams](project_328_store_exclusion_seams.md) — StoresWrapper(469)/TreeOfToDoItems(481) near 500-limit, ToDoEvents(594) pre-existing over-limit; new test .cs need csproj wiring; four inclusion surfaces lockstep; adopted persisted StoreWrapper.StoreId
- [C# coverage gate expects JaCoCo](project_csharp_coverage_gate_jacoco_format.md) — validate-feature-review-coverage.ps1 reads artifacts/csharp/coverage.xml as JaCoCo, not Cobertura; plan a conversion scoped to first-party
- [Durable script copy into feature folder](durable-script-copy-into-feature-folder.md) — copy scratchpad-supplied scripts into `<FEATURE>/scripts/` before referencing them in plan tasks (session-scoped temp paths aren't durable)
- [#351 QuickFiler breadcrumb plan seams](project_351_quickfiler_breadcrumb_plan_seams.md) — JSON code in UtilitiesCS only (QuickFiler lacks Newtonsoft); P2-T1 blocked-if-9101-absent; evidence/repro/ rejected; coordinator pattern
- [Invoke-MSTestWithCoverage.ps1 canonical coverage runner](reference_invoke_mstest_with_coverage_script.md) — full-suite *.Test.dll → Cobertura XML via dotnet-coverage+vstest /InIsolation; cite for baseline/final-QC coverage tasks
- [Invoke-MSTest.ps1 single-SearchRoot defect](reference_invoke_mstest_single_searchroot_defect.md) — scalar `.Count` under StrictMode throws when one assembly matches; always cite `-SearchRoot .`
- [Literal-call clauses block file-size tightening](literal-call-clauses-block-file-size-tightening.md) — clauses pinning a call in 2+ places + a near-500-line file = unsatisfiable; plan the type split up front (no waiver for .cs)
- [Coverage threshold conflict: CLAUDE.md vs general-unit-test.md](project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md) — 80/90 vs 85/75; repo baseline 70.19/58.30 → repo-wide figure reported non-blocking on fixes (#424 precedent), change-scoped gates blocking
- [Planner may lack the MCP plan validator](project_planner_mcp_validator_not_in_tool_surface.md) — file-only tool surface (no Bash/no mcp__drm-copilot__*); never claim the gate passed, report VALIDATOR NOT RUN + structural self-check
- [#430 QuickFiler keyboard plan seams](project_430_quickfiler_keyboard_plan_seams.md) — K1 mandatory (QuickFiler.Test absent from UtilitiesCS InternalsVisibleTo); R2 Option A; amended AC9 two-csproj allowance; ItemViewer needs an ambient SynchronizationContext
- [#432 coverage-ledger plan seams](project_432_coverage_ledger_plan_seams.md) — 121-file disjoint partition arithmetic; classification vs disposition are orthogonal axes; entry fn returns ExitCode (never calls exit); zero-line files can never be `testable`
- [#349 breadcrumb plan seams](project_349_efcviewer_breadcrumb_plan_seams.md) — P0-T6 halt-gate on 9101 provider; evidence/repro/ authorized; EfcViewer3 mechanical swap only; Newtonsoft in UtilitiesCS only
- [#455 F13 breadcrumb/WebView2 plan seams](project_455_f13_breadcrumb_webview_plan_seams.md) — separate-type (never partial) exemption extraction; method-level attrs leak lambdas, type-level don't; 8/11 files already pass
- [#456 F14 ItemViewer plan seams](project_456_f14_itemviewer_plan_seams.md) — ControlHost not on IBreadcrumbDropDownHost; S1 orphans Linq+Drawing usings; D5 overrides 3 research STA homes; AC9 forbids the D11 deletion
- [#136 wave-1 non-halting F1 dependency](project_136_wave1_nonhalting_f1_dependency.md) — F1 ledger/harness absent at planning time is by design; write it as an execution-time read, never a preflight-evaluable gate
- [#433 F7 QfcHomeController plan seams](project_433_f7_qfchomecontroller_plan_seams.md) — partial split before seams (487+15>500); `:133`/`:136` viewer/scheduler coupling; 5 frozen #424 test files; QuickFiler.Test.csproj wiring
- [#424 QuickFiler deadline plan seams](project_424_quickfiler_deadline_plan_seams.md) — 12s const; Part2 partial no-[TestClass]; overload migration breaks loose-mock Setup/Verify (Issue218 "dormant" misclassification); grep old overload shape in ALL test files
- [PoshQC MCP measurement limits](reference_poshqc_mcp_measurement_limits.md) — no file list/count, no BRANCH counter, hooks-only coverage allow-list (scripts/vscode/ never instrumented); never clause `EXIT_CODE: 0` for analyze or test
- [vstest scoped-run + csharpier 1.2.6 commands](reference_vstest_scoped_run_command.md) — vswhere-resolved vstest.console.exe + /InIsolation + /TestCaseFilter (join `|`); every run task needs an explicit command; csharpier needs format/check subcommands
