# Atomic Planner Memory Index

## Preflight revision seams (per-issue)

- [#468](project_468_preflight_revision_seams.md) — seam before red test; `[expect-fail]` on run tasks only; epic-child merge-base
- [#501 R1](untracked-file-and-linecount-gate-seams.md) — `git add -N` before grepping plan-created files; `(Get-Content).Count`, not `Measure-Object -Line`
- [#501 R3](project_501_r3_preflight_seams.md) — repo-wide 0-skipped gates unsatisfiable; BASELINE_FAILURE_SET subset; `Task.CompletedTask` singleton
- [#511 R1](project_511_r1_preflight_delta_seams.md) — mid-cycle evidence deletion; git-log scans post-commit; Start-Process for 20-min runs
- [#484](project_484_qfc_revision_seams.md) — ownership change sweeps plan→issue.md→spec.md (spec is the AC source); old-cardinal grep sweep
- [QfcItemController test-capacity squeeze](project_qfcitemcontroller_test_capacity_squeeze.md) — four owned test files hold only ~471 aggregate spare lines and `.csproj` edits are barred; budget in Phase 0, permit relocation, mandate DataRow/shared-arrange compaction
- [#494](project_494_threshold_reconciliation_plan_seams.md) — coverage runner throws before post-processing; reported-floor must not become hook-Blocking
- [#498](conditional-ladder-and-unowned-class-gates.md) — gate EVERY rung of a recorded-selector ladder incl. rung 1; scope failing-identifier clauses to owned test classes; 0/0 changed-line figure → NOT APPLICABLE
- [#503](project_503_ribbon_readiness_plan_seams.md) — RibbonViewer 487/500 forces a region move; 6+4 Compile entries; compile-time red + dossier
- [#505](project_505_toggle_state_guards_plan_seams.md) — runtime red (no dossier); raw cobertura to gitignored `coverage/`; manual-verification kind accepted
- [#512](project_512_toolchain_gate_fidelity_plan_seams.md) — same-line `/t:Build`+`Nullable=enable` grep; `-EnableNullable` no-op proved by EXIT 0
- [#553](project_553_ci_parallel_split_plan_seams.md) — workflow-only scope, no C# toolchain; no jq; pathspec anchoring
- [#614 store-root leak plan seams](project_614_store_root_leak_plan_seams.md) — AC25 net non-growth (3 over-limit files); E1 SelectRow out-of-root-only pinning; remediation C1: behavior-preserving seam phase reconciles fail-before with a signature change, resolver-in-guard beats inline try/catch on line budget + coverage, net48 IsNullOrWhiteSpace doesn't narrow (`archiveRoot!`)
- [#464 R3/R4](project_464_efc_controller_plan_seams.md) — additive-only file grows, budget a ceiling not a shrink; non-comment literal counts; a phase-N count must survive phases 1..N-1 deletions

## Plan-structure traps
- [Verify test provenance before planning a deletion](verify-test-provenance-before-planning-deletion.md) — in a revert plan, read the test at the pre-cycle commit; a two-arg call shape doesn't prove the cycle added it

- [Validator phase-heading constraint](plan-validator-phase-heading-constraint.md) — exact `### Phase N — <Title>`; nothing between N and the em dash
- [Validator task-ID sequential constraint](plan-validator-task-id-sequential-constraint.md) — digit-only, sequential by appearance; insertion forces full renumber
- [Planner may lack the MCP validator](project_planner_mcp_validator_not_in_tool_surface.md) — report VALIDATOR NOT RUN + structural self-check; never claim a pass
- [Fenced `#` comments look like headings](plan-fenced-powershell-comments-look-like-headings.md) — indent column-0 `#` inside code fences
- [One AC per check-off task](feedback_ac_checkoff_one_per_task.md) — preflight rejects batched AC check-offs
- [Terminal-phase planner traps](terminal-phase-planner-traps.md) — unowned "a follow-up issue should carry it"; artifacts written after the clean-tree commit task
- [Never plan a mid-plan halt on MCP availability](never-plan-a-mid-plan-halt-on-mcp-availability.md) — Phase 0 probe + record-blocker-and-continue
- [Thread granted discharges through consumers](thread-granted-discharges-through-consumers.md) — softening one task without its producer makes the discharge unreachable
- [Durable script copy into feature folder](durable-script-copy-into-feature-folder.md) — copy scratchpad scripts into `<FEATURE>/scripts/` first
- [Evidence path normalization](evidence-path-normalization.md) — normalize spec-named `evidence/coverage/` to `baseline/` + `qa-gates/`

## Acceptance-condition authoring

- [Acceptance edits must be false-before/true-after](acceptance-edits-must-be-false-before-true-after.md) — a clause already true at branch head is a no-op gate
- [Zero-hit grep gates need carve-outs](zero-hit-grep-gates-need-carveouts.md) — denial text and non-coverage numerals make "no hits" unsatisfiable
- [Single-numeral gates must name the role](single-numeral-gates-must-name-the-role.md) — count the *enforced* occurrence; enumerate doc/policy ones
- [Superseding a floor must name CLAUDE.md](superseding-a-coverage-floor-must-name-claude-md.md) — an enumeration omitting it implies its rank-1 floor survives
- [MCP promotion route seams](mcp-promotion-route-plan-seams.md) — separate bug entry point; `promotion_type`+`work_mode`; stage `docs/features/potential`; return shape undocumented
- [Wiring gates must be wiring-sensitive](feedback_wiring_gates_must_be_wiring_sensitive.md) — count floors deflate with the defect they guard
- [Research claims as acceptance clauses](research-claims-as-acceptance-clauses.md) — never encode an unmeasured third-party claim as a literal AC clause
- [Literal-call clauses block file-size tightening](literal-call-clauses-block-file-size-tightening.md) — pinning a call in 2+ places near a 500-line file is unsatisfiable
- [Enumeration variable must match its consumer](enumeration-variable-must-match-consumer.md) — `$kept` produced vs `@assemblies` splatted = zero-assembly vstest run reporting zero failures; same-payload re-execution + a count-parity floor
- [Diff gates need a commit task](diff-gates-need-a-commit-task.md) — `git diff <BASE>..HEAD` passes vacuously with no commit task
- [Never pin a HEAD SHA as a plan expectation](never-pin-head-sha-as-plan-expectation.md) — gate on tree invariants instead
- [Absolute counts in shared files go stale](absolute-counts-in-shared-files-go-stale.md) — lower-bound/baseline-relative for co-owned files; keep exact the count the task changes
- [.claude/agent-memory is tracked](agent-memory-is-tracked-scope-git-gates.md) — scope every diff/status/grep gate or it is unsatisfiable
- [Stale build output is not evidence of existence](stale-build-output-is-not-evidence-of-existence.md) — verify with `git ls-files`, not `obj/`

## C# toolchain and test mechanics

- [Phase 0 toolchain bootstrap](project_csharp_phase0_toolchain_bootstrap.md) — `dotnet tool run csharpier` works once the SDK is bootstrapped (global.json's missing .dotnet-sdk was the real blocker, not the manifest); mandatory NuGet restore
- [Agent worktrees need SDK + NuGet + analyzer backfill](agent-worktrees-need-sdk-and-nuget-bootstrap.md) — four Phase 0 steps; CS0006 is an error, not a warning
- [vstest scoped-run + csharpier 1.2.6 commands](reference_vstest_scoped_run_command.md) — vswhere + `/InIsolation` + `/TestCaseFilter`; csharpier needs a subcommand
- [CSharpier gate: format not pipe-files](csharpier-format-not-pipe-files-gate.md) — `pipe-files` is stdout-only and non-enforcing
- [CSharpier "Formatted N files" is processed count](csharpier-formatted-n-is-processed-count.md) — define rewritten-count via before/after SHA-256
- [Repo-wide csharpier format breaks zero-diff ACs](csharpier-repowide-format-breaks-zero-diff-acs.md) — scope the mutating pass to the plan's own path list
- [.csharpierignore scope](csharpierignore-scope-packages-config.md) — only `*.csproj`/`*.props`/`*.targets` are excluded; `packages.config` is NOT
- [`/Logger:trx` needs `/ResultsDirectory`](trx-needs-resultsdirectory.md) — plus a per-task `p#-t#` segment so `[expect-fail]` TRX cannot be mistaken
- [`[expect-fail]` needs a synchronous seam](expect-fail-needs-a-synchronous-seam.md) — async-void boundaries false-GREEN; re-run RED analysis after scoping `Times.Never()`
- [Invoke-MSTestWithCoverage.ps1](reference_invoke_mstest_with_coverage_script.md) — canonical full-suite Cobertura runner
- [Invoke-MSTest.ps1 single-SearchRoot defect](reference_invoke_mstest_single_searchroot_defect.md) — always pass `-SearchRoot .`
- [PoshQC MCP + msbuild facts](poshqc-mcp-and-msbuild-invocation-facts.md) — MCP returns no counts; pair unconditionally with direct runs
- [pwsh -Command payload quoting](pwsh-command-payload-quoting.md) — outer single quotes, inner doubles
- [Pester exits 0 on failing It blocks](pester-invoke-does-not-exit-nonzero.md) — scope every exit-code clause to a named channel
- [PowerShell gate observables](powershell-gate-observables.md) — no Invoke-Pester exit code; explicit `scan_folders`; aggregate-only `CoveragePercent`
- [Legacy csproj wiring](project_legacy_csproj_explicit_compile_include.md) — `Compile Include` + own `Reference`; ProjectReference gives no compile-time flow
- [Invoke-VSBuild rewrites csproj HintPaths](invoke-vsbuild-rewrites-csproj-hintpaths.md) — the wrapper runs Sync-PackageReferences over EVERY csproj; with a forbidden .csproj the build itself commits the scope violation — use vswhere-resolved MSBuild
- [Declaration-only seam task for fail-before](declaration-only-seam-task-for-fail-before.md) — tests citing not-yet-existing internals redden the whole assembly; order compile-clean tests first, open the fix phase with a no-behaviour seam task + whole-set assertion-time red run
- [net48 / nullable context mismatch](project_nullable_context_mismatch_prod_vs_test.md) — check `#nullable enable` in prod AND missing `<LangVersion>` in the test csproj
- [Worktree root breaks the `\.claude\` exclusion](worktree-root-breaks-dotclaude-exclusion.md) — assert a workspace-root prefix instead

## Coverage
- [Deletion-adjusted coverage no-regression gate](deletion-adjusted-coverage-no-regression-gate.md) — deleting covered lines makes `rate_post >= rate_base` unsatisfiable; gate on covered/valid counters

- [Verify test provenance before planning a deletion](verify-test-provenance-before-planning-deletion.md) — in a revert plan, read the test at the pre-cycle commit; a two-arg call shape doesn't prove the cycle added it
- [#489 PartN reroute amendment seams](project_489_partn_reroute_amendment_seams.md) — verify parent `partial` before continuation-file tasks (it wasn't); spec amendment notes shift all AC line citations (+13, renumber descending); re-grep rename-site lines after sibling growth
- [#614 store-root leak plan seams](project_614_store_root_leak_plan_seams.md) — AC25 net non-growth (3 over-limit files); E1 SelectRow out-of-root-only pinning; remediation C1: behavior-preserving seam phase reconciles fail-before with a signature change, resolver-in-guard beats inline try/catch on line budget + coverage, net48 IsNullOrWhiteSpace doesn't narrow (`archiveRoot!`)
- [Agent worktrees need SDK + NuGet + analyzer-backfill bootstrap](agent-worktrees-need-sdk-and-nuget-bootstrap.md) — no `.dotnet-sdk`, no `packages/`, and a clean restore still misses the skewed analyzer versions (CS0006, not a warning); three Phase 0 tasks
- [/Logger:trx needs /ResultsDirectory](trx-needs-resultsdirectory.md) — TRX lands in `TestResults\` relative to cwd; TRX-existence-under-evidence acceptance is unsatisfiable without it, and the clean-tree gate won't catch it
- [Per-task TRX subdirectory](trx-needs-resultsdirectory.md) — a shared `/ResultsDirectory:` makes "ten distinct TRX files" ambiguous once `[expect-fail]` runs deposit earlier TRX there; give each task a `p#-t#` segment
- [Spec corrections sweep sibling sections](feedback_spec_corrections_sweep_sibling_sections.md) — falsified-premise fixes must cover Scope/Out-of-scope/Rollout, not AC only; denial text must dodge closing-keyword scans (#511 R1 Part 6)
- [#511 R1 preflight delta seams](project_511_r1_preflight_delta_seams.md) — mid-cycle raw-evidence deletion breaks resolves gates; git-log scans post-commit only; absolute MSBuild path; Start-Process mechanic for 20-min runs; per-class coverage noise -0.50pp
- [CSharpier "Formatted N files" is processed count](csharpier-formatted-n-is-processed-count.md) — restart-on-rewrite loops keyed on it never terminate; define rewritten-count via before/after SHA-256
- [Terminal-phase planner traps](terminal-phase-planner-traps.md) — sweep the last phase for an unowned "a follow-up issue should carry it", artifacts written after the clean-tree commit task, and a false "clarification against the spec's wording"
- [#493 UiThread dispatcher plan seams](project_493_uithread_dispatcher_plan_seams.md) — signature-change fail-before gets a REAL red build by staging the two `<Compile Include>` lines; the coverage script IS the parallelized run; scope `SwapUiThreadDispatcher`/`UiThreadDispatcherGate` greps to `QuickFiler.Test/`
- [#553 CI parallel-split plan seams](project_553_ci_parallel_split_plan_seams.md) — workflow-only scope: no C# toolchain; Phase 0 snapshot for byte-identity; ruleset PUT + gh pr create orchestrator-gated; no jq (ConvertTo-Json -Depth 20); pathspec anchoring; BRANCH/SCRATCH conventions
- [#442 QuickFiler metrics plan seams](project_442_quickfiler_metrics_plan_seams.md) — commented-out code defeats zero-hit grep gates; an AC conjunct already green pre-fix; declare the seam BEFORE the red tests or they don't compile; Invoke-MSTestWithCoverage throws below 80%
- [#468 QfcCollectionController plan seams](project_468_qfc_collection_controller_plan_seams.md) — ToggleUnGroupConv is not COM-free drivable; MakeSpaceForItems never touches Size; a sign-defect seam must land carrying the defect; `LoadItemGroup(` needs the paren
- [Threshold conflict: CLAUDE.md vs general-unit-test.md](project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md) — 80/90 vs 85/75; repo-wide figure non-blocking, change-scoped gates blocking
- [JaCoCo hook, Cobertura also accepted](project_csharp_coverage_gate_jacoco_format.md) — follow the format the delta names
- [Async state machines split the denominator](async-state-machine-coverage-aggregation.md) — aggregate by `filename` or a >=90% gate fails for measurement reasons
- [Dead-code removal vs coverage exclusion](project_deadcode_removal_vs_coverage_exclusion.md) — shrink the denominator, never exclude
- [CLR-invoked private members](coverage-gate-clr-invoked-private-members.md) — never gate AssemblyResolve-style members at >=90%
- [Named coverage exception: verify the member body](named-coverage-exception-verify-member-body.md) — gap-closure goes BEFORE the clean-pass task
- [Enumerate condition outcomes before the case list](enumerate-condition-outcomes-before-case-list.md) — 2 outcomes per condition in every `||`/`&&` clause
- [#441 Cobertura arithmetic](project_441_cobertura_arithmetic_plan_seams.md) — two-file pin vs 500-line ceiling; StrictMode throws on any missing fixture attribute
- [#457 closure-filter](project_457_closure_filter_plan_seams.md) — the pipeline overwrites raw Cobertura in place; pre-merge insertion is a correctness constraint

## File-size and refactor mechanics

- [C# pure-move extraction pattern](csharp-pure-move-extraction-pattern.md) — keep the static-ctor install trigger; declare relocation-not-new-module
- [Re-scoping a plan after a sibling landed the fix](plan-rescope-after-sibling-landed-the-fix.md) — split the file's contiguous TAIL so upstream citations survive
- [#400 partial-class headroom placement](project_400_partial_class_headroom_placement.md) — put new cases in existing `.Part2.cs` partials
- [Post-format file-size audit](feedback_postformat_file_size_audit.md) — the 500-line audit goes AFTER the final csharpier format
- [Embedded-resource fail-proof needs a rebuild gate](embedded-resource-failproof-rebuild-gate.md) — edit → rebuild → assert bytes → `[expect-fail]`

## Domain seams (TaskMaster)

- [#445 keyboard-action](project_445_keyboard_action_plan_seams.md) — resolve WS at execution time; scope epic-child gates to owned test classes
- [#446 QuickFiler bug family](project_446_quickfiler_bug_family_plan_seams.md) — ScoringServiceFactory seam before COM-path tests; AC conflict → unchecked + REMEDIATION-REQUIRED; R3 unreachable coverage gate in an unbounded restart loop
- [#438 search-focus](project_438_search_focus_plan_seams.md) — additive interface overload broke 7 test files; dispatch the default path on the old overload
- [#424 QuickFiler deadline](project_424_quickfiler_deadline_plan_seams.md) — overload migration breaks loose-mock Setup/Verify; grep the old shape in ALL test files
- [#351 QuickFiler breadcrumb](project_351_quickfiler_breadcrumb_plan_seams.md) — JSON code in UtilitiesCS only; coordinator pattern
- [#349 EfcViewer breadcrumb](project_349_efcviewer_breadcrumb_plan_seams.md) — P0 halt-gate on the 9101 provider; mechanical swap only
- [#230 WinForms pump seam](project_230_winforms_pump_seam_plan_facts.md) — factory seam params before SaveParameters; CreateAsync awaited-tail faults
- [#211 startup-lifetime heartbeat](project_211_startup_lifetime_heartbeat_seam.md) — DispatcherTimer in ThisAddIn.cs; pure logic in StartupDiagnosticsProbe
- [#292 CurrentStoreContext](project_292_currentstorecontext_parallel_seam.md) — process-global static; scope-opening store test classes need `[DoNotParallelize]`
- [#307 F2 ScoCollection deletion gate](project_307_f2_scocollection_deletion_gate.md) — full first-party reference set incl. tests; ISubjectMapSco/IScoCollection boundary
- [#328 store exclusion](project_328_store_exclusion_seams.md) — near-limit files; new test `.cs` need csproj wiring; four inclusion surfaces lockstep
- [#295 WinForms STA exemptions](project_winforms_sta_refinement_exemption_rule.md) — remove HWND-only/PerformClick exemptions; keep dialog/Form/launcher
- [#295 STA control-identity pattern](project_sta_last_resort_control_identity_pattern.md) — companion interface + `*.StaTests.cs`; never construct a Form
- [Manager AsyncLazy shared seam](project_manager_asynclazy_shared_seam.md) — key-specific accessor, never retype the dictionary value
- [Folder predictor AF holder seam](project_folder_predictor_af_holder_seam.md) — Folder-only holder on IAppAutoFileObjects, not per-instance state
- [Dispatcher repro hang trap](dispatcher-repro-hang-trap.md) — use an owned pumping STA thread, not `Dispatcher.CurrentDispatcher` on a pooled worker

## Spec and artifact hygiene

- [Spec corrections sweep sibling sections](feedback_spec_corrections_sweep_sibling_sections.md) — cover Scope/Out-of-scope/Rollout, not AC only
- [Never embed absolute host paths](../_shared_no_absolute_host_paths.md) — use `<repo-root>` / `<user>` / `<host>`; vstest TRX names carry the account and host
