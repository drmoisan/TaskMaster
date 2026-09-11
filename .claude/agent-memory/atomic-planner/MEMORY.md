# Atomic Planner Memory Index

## Preflight revision seams (per-issue)

- [#826](project_826_factory_outside_try_reachability_seams.md) — a seam invoked OUTSIDE the `try` makes both catch bodies reachable; enumerate the seam's line vs the `try`'s, not the catch clauses
- [#815](project_815_coverage_aggregation_exposure_plan_seams.md) — `.//line` baseline already zero (needs a positive control); pinned nine-name allowlist zeroes the differential helper; test tree tighter than production; 500 lines legal, 501 not
- [#810 R1–R2](project_810_teardown_dropdown_residuals_plan_seams.md) — inherited paths must be a RULE not a list (executor writes agent-memory mid-run); never upper-bound a csharpier checked-file delta; `TokenSource` null before Cleanup too
- [#791](project_791_hc_deadline_cancel_teardown_plan_seams.md) — `QfcDatamodel` excluded from coverage; no shell var survives between tasks · [#781](project_781_excludefromcodecoverage_guard_plan_seams.md) — `[ExcludeFromCodeCoverage]` makes changed-line coverage unmeasurable
- [#736 R1–R5](project_736_efc_archiveroot_boundary_sink_plan_seams.md) — 485-line test file voids the Write Set; hardcoded floors; one shared `try`; name-vs-content gates; existence-only `*.log`
- [#731 R1–R5](project_731_lifecycle_disposal_plan_seams.md) — near-limit files force partial continuation; unbootstrapped worktree; `DebugType=full` leaks paths; `.//line` double-counts
- [#735 R1](project_735_evidence_content_sanitization_seams.md) — name-only sanitization can't fail; TRX leaks in content · [#752](project_752_relative_path_anchor_plan_seams.md) — GetRelativePath fix needs `(^|\\)`
- [#680](project_680_menu_mode_plan_seams.md) — HostTests.cs 499 not 500; "optional" fallback was load-bearing · [#678](project_678_carry_folder_predictor_plan_seams.md) — runner throws twice before writing
- [#670](project_670_webview_fault_boundary_plan_seams.md) — awaiter `IsCompleted` breaks "no pump"; merge by filename · [sanitisation](project_670_capture_time_sanitisation_seams.md) — vswhere path leaks via an *indirect* invoker
- [#677 R1–R8](project_677_keyboard_focus_leak_plan_seams.md) — ctor param REJECTED; never ambient SetSynchronizationContext · [#663](project_663_qfc_alt_chord_plan_seams.md) — defect-preserving seam: compile-red → runtime red
- [#662](project_662_banner_prefix_arity_plan_seams.md) · [R2](project_662_banner_prefix_revision_round_seams.md) · [R3](project_662_round3_trx_hygiene_and_verbatim_seams.md) — `AC5` prefixes `AC5b`; `*.trx` NOT gitignored
- [#656](project_656_closecompleted_guard_plan_seams.md) — no TestCaseFilter override; class nodes lack lines-valid · [#648](project_648_ungated_static_swap_plan_seams.md) — lines-valid equality unsatisfiable; use 5% tolerance
- [#644](project_644_ac16_referral_revision_seams.md) — named instrument prints no figure · [cycle 2](project_644_cycle2_sweep_gate_evasion_seams.md) — rewording out of a match set is evasion · [PA-7](project_644_pa7_redaction_plan_seams.md) — untracked artifact still enters main
- [#637 R6](project_637_r6_superseded_spec_claim_seams.md) — plan narrates spec edits it never performs · [R2–R5](project_637_selectrow_rooted_path_plan_seams.md) — broad operand hits 121 siblings; `-F` breaks regex
- [#635](project_635_reflective_caller_audit_plan_seams.md) — evidence-only audit inflates its own sweep · [#633](project_633_undo_handoff_plan_seams.md) — orphan window has no deterministic fail-before
- [#614](project_614_store_root_leak_plan_seams.md) — net non-growth AC; net48 `IsNullOrWhiteSpace` doesn't narrow · [#553](project_553_ci_parallel_split_plan_seams.md) — workflow-only; ruleset PUT orchestrator-gated
- [#512](project_512_toolchain_gate_fidelity_plan_seams.md) — same-line `/t:Build`+`Nullable=enable`; no-op proved by EXIT 0 · [#511 R1](project_511_r1_preflight_delta_seams.md) — mid-cycle evidence deletion
- [#505](project_505_toggle_state_guards_plan_seams.md) — runtime red; raw cobertura to gitignored `coverage/` · [#503](project_503_ribbon_readiness_plan_seams.md) — 487/500 forces a region move
- [#501 R1](untracked-file-and-linecount-gate-seams.md) — `git add -N` before grepping plan-created files · [R3](project_501_r3_preflight_seams.md) — repo-wide 0-skipped gates unsatisfiable
- [#498](conditional-ladder-and-unowned-class-gates.md) — gate every ladder rung; 0/0 → NOT APPLICABLE · [#494](project_494_threshold_reconciliation_plan_seams.md) — runner throws before post-processing
- [#484](project_484_qfc_revision_seams.md) — ownership sweeps plan→issue.md→spec.md (spec is the AC source) · [capacity squeeze](project_qfcitemcontroller_test_capacity_squeeze.md) — `.csproj` edits barred
- [#469 R1–R3](project_469_comment_accuracy_plan_seams.md) — a SWAP voids whole-file token gates; `AC1` prefixes `AC10` · [#468](project_468_preflight_revision_seams.md) — seam before red test
- [#464 R3/R4](project_464_efc_controller_plan_seams.md) — additive-only file grows; budget a ceiling, not a shrink · [#440 R1–R4](project_440_breadcrumb_left_arrow_plan_seams.md) — deletion voids a changed-line gate

## Plan-structure traps

- [Phase-heading constraint](plan-validator-phase-heading-constraint.md) — exact `### Phase N — <Title>` · [Task-ID constraint](plan-validator-task-id-sequential-constraint.md) — digit-only; insertion forces renumber
- [Planner may lack the MCP validator](project_planner_mcp_validator_not_in_tool_surface.md) — report VALIDATOR NOT RUN · [MCP unavailability](never-plan-a-mid-plan-halt-on-mcp-availability.md) — probe, never halt
- [Fenced `#` comments look like headings](plan-fenced-powershell-comments-look-like-headings.md) — indent column-0 `#` in fences
- [One AC per check-off task](feedback_ac_checkoff_one_per_task.md) — preflight rejects batched check-offs · [Terminal-phase traps](terminal-phase-planner-traps.md) — artifacts after the clean-tree commit
- [Verify test provenance before a deletion](verify-test-provenance-before-planning-deletion.md) — read the test at the pre-cycle commit
- [Reviewer enumeration may be narrow](reviewer-enumeration-may-be-deliberately-narrow.md) — "completing" a list can falsify it
- [Thread granted discharges through consumers](thread-granted-discharges-through-consumers.md) — softening one task strands its producer
- [Durable script copy](durable-script-copy-into-feature-folder.md) — copy into `<FEATURE>/scripts/` · [Evidence path normalization](evidence-path-normalization.md) — `coverage/` → `baseline/`+`qa-gates/`

## Acceptance-condition authoring

- [Edits must be false-before/true-after](acceptance-edits-must-be-false-before-true-after.md) — a clause already true is a no-op gate
- [Zero-hit greps need carve-outs](zero-hit-grep-gates-need-carveouts.md) — denial text unsatisfies "no hits" · [Single-numeral gates](single-numeral-gates-must-name-the-role.md) — count the *enforced* occurrence
- [Superseding a floor must name CLAUDE.md](superseding-a-coverage-floor-must-name-claude-md.md) — omission implies rank-1 survives
- [Wiring gates must be wiring-sensitive](feedback_wiring_gates_must_be_wiring_sensitive.md) — count floors deflate with the defect
- [Research claims as acceptance clauses](research-claims-as-acceptance-clauses.md) — never encode an unmeasured claim
- [Literal-call clauses block size tightening](literal-call-clauses-block-file-size-tightening.md) — unsatisfiable near 500 lines
- [Enumeration variable must match consumer](enumeration-variable-must-match-consumer.md) — mismatch = zero-assembly run
- [Diff gates need a commit task](diff-gates-need-a-commit-task.md) — unanchored `git diff` passes vacuously · [Never pin a HEAD SHA](never-pin-head-sha-as-plan-expectation.md) — gate on tree invariants
- [Empty-porcelain clauses are unsatisfiable](empty-porcelain-clause-is-unsatisfiable.md) — path-class clause + double amend · [Porcelain collapses dirs](porcelain-collapses-untracked-directories.md) — `--untracked-files=all`
- [Self-referential evidence enumeration](self-referential-evidence-enumeration.md) — bound the range at the capturing task
- [.claude/agent-memory is tracked](agent-memory-is-tracked-scope-git-gates.md) — scope every diff/status/grep gate · [Harness gitStatus](harness-git-status-may-describe-another-worktree.md) — may describe another worktree
- [.gitignore does not untrack an indexed path](gitignore-does-not-untrack-indexed-paths.md) — a force-added file stays tracked
- [Existence is not retention](existence-is-not-retention-gate-committed-artifacts.md) — add `git ls-files` + `git add -N` · [Stale build output](stale-build-output-is-not-evidence-of-existence.md) — not evidence of existence
- [Absolute counts in shared files go stale](absolute-counts-in-shared-files-go-stale.md) — lower-bound for co-owned files
- [Observation scope must match blast radius](observation-scope-must-match-blast-radius.md) — space, time, spelling · [Account-token pattern](runtime-derived-account-token-pattern.md) — derive at run time
- [MCP promotion route seams](mcp-promotion-route-plan-seams.md) — separate bug entry point; `promotion_type`+`work_mode`

## C# toolchain and test mechanics

- [Phase 0 toolchain bootstrap](project_csharp_phase0_toolchain_bootstrap.md) — csharpier works once the SDK is bootstrapped · [Worktree backfill](agent-worktrees-need-sdk-and-nuget-bootstrap.md) — CS0006 is an error
- [vstest scoped-run + csharpier commands](reference_vstest_scoped_run_command.md) — vswhere + `/InIsolation`; csharpier needs a subcommand
- [format, not pipe-files](csharpier-format-not-pipe-files-gate.md) — `pipe-files` is stdout-only · ["Formatted N" is a processed count](csharpier-formatted-n-is-processed-count.md) — a restart loop never ends
- [Repo-wide format breaks zero-diff ACs](csharpier-repowide-format-breaks-zero-diff-acs.md) — scope the pass · [.csharpierignore scope](csharpierignore-scope-packages-config.md) — NOT `packages.config`
- [.gitignore bracket classes defeat a literal grep](gitignore-bracket-classes-defeat-literal-grep.md) — `[Tt]est[Rr]esult*/`
- [`.trx` leaks host tokens in two casings](trx-carries-host-tokens-in-two-casings.md) — sweep content · [`/Logger:trx` needs `/ResultsDirectory`](trx-needs-resultsdirectory.md) — own subdir per task
- [`[expect-fail]` needs a synchronous seam](expect-fail-needs-a-synchronous-seam.md) — async-void boundaries false-GREEN
- [Invoke-MSTestWithCoverage.ps1](reference_invoke_mstest_with_coverage_script.md) · [defect](reference_invoke_mstest_single_searchroot_defect.md) — always pass `-SearchRoot .`
- [`Task "Csc"` needs detailed verbosity](msbuild-task-csc-literal-needs-detailed-verbosity.md) — use a detailed `/flp:` log · [PoshQC MCP](poshqc-mcp-and-msbuild-invocation-facts.md) — returns no counts
- [pwsh -Command payload quoting](pwsh-command-payload-quoting.md) — outer single quotes, inner doubles
- [Pester exits 0 on failing It blocks](pester-invoke-does-not-exit-nonzero.md) — scope exit-code clauses · [PowerShell gate observables](powershell-gate-observables.md) — explicit `scan_folders`
- [Legacy csproj wiring](project_legacy_csproj_explicit_compile_include.md) — `Compile Include` + own `Reference` · [Invoke-VSBuild rewrites HintPaths](invoke-vsbuild-rewrites-csproj-hintpaths.md) — use vswhere MSBuild
- [Declaration-only seam for fail-before](declaration-only-seam-task-for-fail-before.md) — missing internals redden the whole assembly
- [net48 / nullable context mismatch](project_nullable_context_mismatch_prod_vs_test.md) — check `#nullable enable`, `<LangVersion>`
- [Worktree root breaks the `\.claude\` exclusion](worktree-root-breaks-dotclaude-exclusion.md) — assert a workspace-root prefix

## Coverage

- [Repo-wide Cobertura line-rate is nondeterministic](repo-wide-cobertura-line-rate-is-nondeterministic.md) — branch on `lines-valid` comparability
- [Deletion-adjusted no-regression gate](deletion-adjusted-coverage-no-regression-gate.md) — gate on counters; shrink, never exclude
- [Threshold conflict](project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md) — CLAUDE.md 80/90 vs rules 85/75 · [JaCoCo hook](project_csharp_coverage_gate_jacoco_format.md) — Cobertura also accepted
- [Async state machines split the denominator](async-state-machine-coverage-aggregation.md) — aggregate by `filename` · [CLR-invoked private members](coverage-gate-clr-invoked-private-members.md) — never gate at >=90%
- [Named exception: verify the member body](named-coverage-exception-verify-member-body.md) — gap-closure precedes the clean pass
- [Enumerate condition outcomes first](enumerate-condition-outcomes-before-case-list.md) — 2 outcomes per `||`/`&&`
- [#441 Cobertura arithmetic](project_441_cobertura_arithmetic_plan_seams.md) — two-file pin vs ceiling · [#457 closure-filter](project_457_closure_filter_plan_seams.md) — pipeline overwrites raw Cobertura
- [#489 PartN reroute amendments](project_489_partn_reroute_amendment_seams.md) — verify parent `partial` · [#493 UiThread](project_493_uithread_dispatcher_plan_seams.md) — stage `<Compile Include>` for a red build
- [#442 QuickFiler metrics](project_442_quickfiler_metrics_plan_seams.md) — commented-out code defeats zero-hit greps · [#468](project_468_qfc_collection_controller_plan_seams.md) — a sign-defect seam must land carrying it
- [Spec corrections sweep sibling sections](feedback_spec_corrections_sweep_sibling_sections.md) — cover Scope/Out-of-scope/Rollout

## File-size and refactor mechanics

- [C# pure-move extraction pattern](csharp-pure-move-extraction-pattern.md) — keep the static-ctor install trigger · [#400 headroom placement](project_400_partial_class_headroom_placement.md) — use existing `.Part2.cs`
- [Re-scope after a sibling landed the fix](plan-rescope-after-sibling-landed-the-fix.md) — split the contiguous tail
- [Post-format file-size audit](feedback_postformat_file_size_audit.md) — runs after the final format · [Embedded-resource rebuild gate](embedded-resource-failproof-rebuild-gate.md) — edit → rebuild → assert

## Domain seams (TaskMaster)

- [#445 keyboard-action](project_445_keyboard_action_plan_seams.md) — resolve WS at execution time · [#446 QuickFiler bug family](project_446_quickfiler_bug_family_plan_seams.md) — ScoringServiceFactory seam first
- [#438 search-focus](project_438_search_focus_plan_seams.md) — additive overload broke 7 test files · [#424 deadline](project_424_quickfiler_deadline_plan_seams.md) — overload breaks loose-mock Setup/Verify
- [#351 QuickFiler breadcrumb](project_351_quickfiler_breadcrumb_plan_seams.md) — JSON in UtilitiesCS only · [#349 EfcViewer](project_349_efcviewer_breadcrumb_plan_seams.md) — P0 halt-gate on the 9101 provider
- [#230 WinForms pump seam](project_230_winforms_pump_seam_plan_facts.md) — factory seam params before SaveParameters · [#211 heartbeat](project_211_startup_lifetime_heartbeat_seam.md) — DispatcherTimer in ThisAddIn.cs
- [#292 CurrentStoreContext](project_292_currentstorecontext_parallel_seam.md) — process-global static; `[DoNotParallelize]` · [#307 deletion gate](project_307_f2_scocollection_deletion_gate.md) — full reference set incl. tests
- [#328 store exclusion](project_328_store_exclusion_seams.md) — near-limit files; new test `.cs` need csproj wiring
- [#295 WinForms STA exemptions](project_winforms_sta_refinement_exemption_rule.md) — dialog/Form/launcher only · [control-identity pattern](project_sta_last_resort_control_identity_pattern.md) — companion interface, never a Form
- [Manager AsyncLazy shared seam](project_manager_asynclazy_shared_seam.md) — key-specific accessor · [Folder predictor AF holder](project_folder_predictor_af_holder_seam.md) — Folder-only holder
- [Dispatcher repro hang trap](dispatcher-repro-hang-trap.md) — use an owned pumping STA thread

## Spec and artifact hygiene

- [Never embed absolute host paths](../_shared_no_absolute_host_paths.md) — use `<repo-root>` / `<user>` / `<host>`
- [PowerShell batch budget caps helpers](powershell-batch-budget-caps-plan-authored-helpers.md) — 3-path cap; ONE fixed gitignored helper
- [#731 R6–R12](project_731_r6_coverage_runner_bypass_seams.md) — runner self-blocks in an agent worktree; porcelain and name-status are disjoint
- [#751 R3](project_751_r3_detached_launch_seams.md) — detached launch: reset `$LASTEXITCODE`, sentinels, pid poll, TRX witness · [R2](project_751_sync_barrier_revision_seams.md) — `nuget restore` for packages.config
- [Two-run gates need a measured/confirming split](two-run-gates-need-a-measured-vs-confirming-split.md) — name one run measured
- [Verify citations in the ASSIGNED worktree](verify-citations-in-the-assigned-worktree.md) — sibling worktrees diverge
- [#647 R1–R2](project_647_fileio2_retry_plan_seams.md) — vstest needs explicit `/Settings:`; ExpectedExitCode keys on THIS RUN
- [SubagentStop hook gotchas](validate-planner-output-hook-line-anchored-gotchas.md) — the review record must also be plain text in chat
- [#797](project_797_folder_settings_persistence_plan_seams.md) - coverage runner TestCaseFilter hard-coded; harness private-nested; R1: unbootstrapped worktree, msbuild not on PATH, hook path regex needs a separator; R2: a generic-type logger has no closed-type attach point, ClassLevel runsettings voids exact log-event counts; R3: an admitted red baseline voids every sibling exit-0 demand
- [#798 R1](project_798_qfc_column_timeout_plan_seams.md) - frozen write set vs an 882-line member; a declared-but-unwired seam observes 0 invocations; a paren anchor misses generic overloads
- [#821 R1](project_821_parentcleanup_double_release_plan_seams.md) - Read over-counts by one trailing line; Select-String default case-insensitivity defeats a first-char-case swap gate; a member with an untested pre-existing catch voids a whole-member coverage gate
- [#825 R1-R6](project_825_etl_deadline_mechanics_plan_seams.md) - a latch ArmingBarrier encodes a FIXED timer order; the Csc task line never names a project; green vstest prints no Failed/Skipped line; the runner appends a test-dll exclusion so the test dll is NOT in the denominator
- [Self-invalidating comment citations](project_825_etl_deadline_mechanics_plan_seams.md) - a comment inserted ABOVE the line range it cites moves that range; permanent source, survives the merge
- [Planners amend ACs, never executors](acceptance-criteria-are-amended-by-planners-not-executors.md) - plan a read-only verification task instead
- [ExcludeFromCodeCoverage voids per-file coverage rows](excludefromcodecoverage-voids-per-file-coverage-rows.md) - no class element at all, so a hits-row demand is unsatisfiable
