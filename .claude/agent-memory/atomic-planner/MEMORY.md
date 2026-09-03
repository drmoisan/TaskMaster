# Atomic Planner Memory Index

## Preflight revision seams (per-issue)

- [#468](project_468_preflight_revision_seams.md) — seam before red test; `[expect-fail]` on run tasks only
- [#501 R1](untracked-file-and-linecount-gate-seams.md) — `git add -N` before grepping plan-created files
- [#501 R3](project_501_r3_preflight_seams.md) — repo-wide 0-skipped gates unsatisfiable; scope to BASELINE_FAILURE_SET
- [#511 R1](project_511_r1_preflight_delta_seams.md) — mid-cycle evidence deletion; `Start-Process` for 20-min runs
- [#484](project_484_qfc_revision_seams.md) — ownership sweeps plan→issue.md→spec.md (spec is the AC source)
- [QfcItemController test-capacity squeeze](project_qfcitemcontroller_test_capacity_squeeze.md) — 4 owned files, ~471 spare lines, `.csproj` edits barred
- [#494](project_494_threshold_reconciliation_plan_seams.md) — runner throws before post-processing; floor must not become Blocking
- [#498](conditional-ladder-and-unowned-class-gates.md) — gate every ladder rung incl. rung 1; 0/0 → NOT APPLICABLE
- [#503](project_503_ribbon_readiness_plan_seams.md) — RibbonViewer 487/500 forces a region move; compile-red + dossier
- [#505](project_505_toggle_state_guards_plan_seams.md) — runtime red (no dossier); raw cobertura to gitignored `coverage/`
- [#512](project_512_toolchain_gate_fidelity_plan_seams.md) — same-line `/t:Build`+`Nullable=enable`; no-op proved by EXIT 0
- [#553](project_553_ci_parallel_split_plan_seams.md) — workflow-only, no C# toolchain; ruleset PUT orchestrator-gated
- [#614](project_614_store_root_leak_plan_seams.md) — AC25 net non-growth; net48 `IsNullOrWhiteSpace` doesn't narrow
- [#464 R3/R4](project_464_efc_controller_plan_seams.md) — additive-only file grows; budget a ceiling, not a shrink
- [#677 R1–R8](project_677_keyboard_focus_leak_plan_seams.md) — ctor param REJECTED (reflection-arity tests); never ambient SetSynchronizationContext
- [#635](project_635_reflective_caller_audit_plan_seams.md) — evidence-only audit: tracked plan inflates its own sweep
- [#440 R1–R4](project_440_breadcrumb_left_arrow_plan_seams.md) — deletion-only change voids a diff-derived changed-line gate
- [#637 R6](project_637_r6_superseded_spec_claim_seams.md) — plan narrates spec edits it never performs; format drift outside pathspec
- [#637 R2–R5](project_637_selectrow_rooted_path_plan_seams.md) — broad operand hits 121 sibling files; blanket `-F` breaks regex
- [#469 R1–R3](project_469_comment_accuracy_plan_seams.md) — defect-number SWAP voids whole-file token gates; `AC1` prefixes `AC10`
- [#644](project_644_ac16_referral_revision_seams.md) — named instrument prints no figure; referral task pins UNCHECKED
- [#644 cycle 2](project_644_cycle2_sweep_gate_evasion_seams.md) — rewording out of a detector's match set is gate evasion
- [#644 PA-7](project_644_pa7_redaction_plan_seams.md) — untracked audit artifact still enters main; name-status diff blind to it
- [#633](project_633_undo_handoff_plan_seams.md) — orphan window has no deterministic fail-before; record synchronously
- [#648](project_648_ungated_static_swap_plan_seams.md) — lines-valid equality UNSATISFIABLE, use 5% tolerance
- [#656](project_656_closecompleted_guard_plan_seams.md) — no TestCaseFilter override in either wrapper; class nodes lack lines-valid
- [#662](project_662_banner_prefix_arity_plan_seams.md) · [R2](project_662_banner_prefix_revision_round_seams.md) · [R3](project_662_round3_trx_hygiene_and_verbatim_seams.md) — `AC5` prefixes `AC5b`; `*.trx` is NOT gitignored
- [#663](project_663_qfc_alt_chord_plan_seams.md) — defect-preserving seam turns compile-red into runtime red
- [#678](project_678_carry_folder_predictor_plan_seams.md) — runner throws twice before writing; fail-closed `CreateGate` lookup
- [#670](project_670_webview_fault_boundary_plan_seams.md) — awaiter `IsCompleted` breaks "no pump" test; Cobertura merge by filename
- [#670 capture-time sanitisation](project_670_capture_time_sanitisation_seams.md) — a vswhere-resolved path leaks via an *indirect* invoker
- [#680](project_680_menu_mode_plan_seams.md) — HostTests.cs 499 not 500; "optional" fallback was load-bearing
- [#735 R1](project_735_evidence_content_sanitization_seams.md) — name-only sanitization gate can't fail; TRX `runUser=`/`computerName=` leak in content; csproj "between" clause self-contradictory

## Plan-structure traps

- [Verify test provenance before a deletion](verify-test-provenance-before-planning-deletion.md) — read the test at the pre-cycle commit
- [Validator phase-heading constraint](plan-validator-phase-heading-constraint.md) — exact `### Phase N — <Title>`
- [Validator task-ID sequential constraint](plan-validator-task-id-sequential-constraint.md) — digit-only; insertion forces renumber
- [Planner may lack the MCP validator](project_planner_mcp_validator_not_in_tool_surface.md) — report VALIDATOR NOT RUN / COMMIT NOT RUN
- [Fenced `#` comments look like headings](plan-fenced-powershell-comments-look-like-headings.md) — indent column-0 `#` inside code fences
- [One AC per check-off task](feedback_ac_checkoff_one_per_task.md) — preflight rejects batched AC check-offs
- [Terminal-phase planner traps](terminal-phase-planner-traps.md) — unowned follow-ups; artifacts written after the clean-tree commit
- [Reviewer enumeration may be deliberately narrow](reviewer-enumeration-may-be-deliberately-narrow.md) — "completing" an omitted list can falsify its trailing predicate
- [MCP unavailability seams](never-plan-a-mid-plan-halt-on-mcp-availability.md) — never a halt task; Phase 0 probe + record-and-continue
- [Thread granted discharges through consumers](thread-granted-discharges-through-consumers.md) — softening one task without its producer strands it
- [Durable script copy into feature folder](durable-script-copy-into-feature-folder.md) — copy scratchpad scripts into `<FEATURE>/scripts/` first
- [Evidence path normalization](evidence-path-normalization.md) — normalize `evidence/coverage/` to `baseline/` + `qa-gates/`

## Acceptance-condition authoring

- [Acceptance edits must be false-before/true-after](acceptance-edits-must-be-false-before-true-after.md) — a clause already true is a no-op gate
- [Zero-hit grep gates need carve-outs](zero-hit-grep-gates-need-carveouts.md) — denial text and non-coverage numerals unsatisfy "no hits"
- [Single-numeral gates must name the role](single-numeral-gates-must-name-the-role.md) — count the *enforced* occurrence, not doc/policy ones
- [Superseding a floor must name CLAUDE.md](superseding-a-coverage-floor-must-name-claude-md.md) — omission implies its rank-1 floor survives
- [MCP promotion route seams](mcp-promotion-route-plan-seams.md) — separate bug entry point; `promotion_type`+`work_mode`
- [Wiring gates must be wiring-sensitive](feedback_wiring_gates_must_be_wiring_sensitive.md) — count floors deflate with the guarded defect
- [Research claims as acceptance clauses](research-claims-as-acceptance-clauses.md) — never encode an unmeasured third-party claim
- [Literal-call clauses block file-size tightening](literal-call-clauses-block-file-size-tightening.md) — unsatisfiable near a 500-line file
- [Enumeration variable must match its consumer](enumeration-variable-must-match-consumer.md) — mismatch = zero-assembly run, zero reported failures
- [Diff gates need a commit task](diff-gates-need-a-commit-task.md) — `git diff <BASE>..HEAD` passes vacuously with no commit
- [Never pin a HEAD SHA as a plan expectation](never-pin-head-sha-as-plan-expectation.md) — gate on tree invariants instead
- [Harness gitStatus may describe another worktree](harness-git-status-may-describe-another-worktree.md) — measure inside the target worktree
- [Absolute counts in shared files go stale](absolute-counts-in-shared-files-go-stale.md) — lower-bound/baseline-relative for co-owned files
- [.claude/agent-memory is tracked](agent-memory-is-tracked-scope-git-gates.md) — scope every diff/status/grep gate
- [.gitignore does not untrack an indexed path](gitignore-does-not-untrack-indexed-paths.md) — a force-added file stays tracked
- [Stale build output is not evidence of existence](stale-build-output-is-not-evidence-of-existence.md) — verify with `git ls-files`, not `obj/`
- [Observation scope must match blast radius](observation-scope-must-match-blast-radius.md) — space, time, spelling must all match
- [Run-time-derived account-token pattern](runtime-derived-account-token-pattern.md) — `Split-Path -Leaf $env:USERPROFILE`; self-exempt

## C# toolchain and test mechanics

- [Phase 0 toolchain bootstrap](project_csharp_phase0_toolchain_bootstrap.md) — csharpier works once the SDK is bootstrapped; mandatory restore
- [Agent worktrees need SDK + NuGet + analyzer backfill](agent-worktrees-need-sdk-and-nuget-bootstrap.md) — CS0006 is an error, not a warning
- [vstest scoped-run + csharpier 1.2.6 commands](reference_vstest_scoped_run_command.md) — vswhere + `/InIsolation`; csharpier needs a subcommand
- [CSharpier gate: format not pipe-files](csharpier-format-not-pipe-files-gate.md) — `pipe-files` is stdout-only, non-enforcing
- [CSharpier "Formatted N files" is processed count](csharpier-formatted-n-is-processed-count.md) — a restart-on-rewrite loop keyed on it never terminates
- [Repo-wide csharpier format breaks zero-diff ACs](csharpier-repowide-format-breaks-zero-diff-acs.md) — scope the pass to the plan's own paths
- [.csharpierignore scope](csharpierignore-scope-packages-config.md) — only `*.csproj`/`*.props`/`*.targets`; `packages.config` is NOT
- [.gitignore bracket classes defeat a literal grep](gitignore-bracket-classes-defeat-literal-grep.md) — `[Tt]est[Rr]esult*/` ignores `TestResults/`
- [`/Logger:trx` needs `/ResultsDirectory` AND `LogFileName`](trx-needs-resultsdirectory.md) — own subdir per task or names collide
- [`[expect-fail]` needs a synchronous seam](expect-fail-needs-a-synchronous-seam.md) — async-void boundaries false-GREEN
- [Invoke-MSTestWithCoverage.ps1](reference_invoke_mstest_with_coverage_script.md) · [single-SearchRoot defect](reference_invoke_mstest_single_searchroot_defect.md) — always pass `-SearchRoot .`
- [`Task "Csc"` needs detailed verbosity](msbuild-task-csc-literal-needs-detailed-verbosity.md) — use a detailed `/flp:` log
- [PoshQC MCP + msbuild facts](poshqc-mcp-and-msbuild-invocation-facts.md) — MCP returns no counts; pair with direct runs
- [pwsh -Command payload quoting](pwsh-command-payload-quoting.md) — outer single quotes, inner doubles
- [Pester exits 0 on failing It blocks](pester-invoke-does-not-exit-nonzero.md) — scope every exit-code clause to a named channel
- [PowerShell gate observables](powershell-gate-observables.md) — no Invoke-Pester exit code; explicit `scan_folders`
- [Legacy csproj wiring](project_legacy_csproj_explicit_compile_include.md) — `Compile Include` + own `Reference`
- [Invoke-VSBuild rewrites csproj HintPaths](invoke-vsbuild-rewrites-csproj-hintpaths.md) — use vswhere-resolved MSBuild instead
- [Declaration-only seam task for fail-before](declaration-only-seam-task-for-fail-before.md) — tests citing not-yet-existing internals redden the whole assembly
- [net48 / nullable context mismatch](project_nullable_context_mismatch_prod_vs_test.md) — check `#nullable enable` and missing `<LangVersion>`
- [Worktree root breaks the `\.claude\` exclusion](worktree-root-breaks-dotclaude-exclusion.md) — assert a workspace-root prefix instead

## Coverage

- [Deletion-adjusted coverage no-regression gate](deletion-adjusted-coverage-no-regression-gate.md) — gate on covered/valid counters; shrink the denominator, never exclude (`project_deadcode_removal_vs_coverage_exclusion.md`)
- [#489 PartN reroute amendment seams](project_489_partn_reroute_amendment_seams.md) — verify parent `partial`; amendments shift AC line citations
- [Spec corrections sweep sibling sections](feedback_spec_corrections_sweep_sibling_sections.md) — cover Scope/Out-of-scope/Rollout, not AC only
- [#493 UiThread dispatcher plan seams](project_493_uithread_dispatcher_plan_seams.md) — stage the `<Compile Include>` lines for a real red build
- [#442 QuickFiler metrics plan seams](project_442_quickfiler_metrics_plan_seams.md) — commented-out code defeats zero-hit grep gates
- [#468 QfcCollectionController plan seams](project_468_qfc_collection_controller_plan_seams.md) — a sign-defect seam must land carrying the defect
- [Threshold conflict: CLAUDE.md vs general-unit-test.md](project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md) — 80/90 vs 85/75
- [JaCoCo hook, Cobertura also accepted](project_csharp_coverage_gate_jacoco_format.md) — follow the format the delta names
- [Async state machines split the denominator](async-state-machine-coverage-aggregation.md) — aggregate by `filename`
- [CLR-invoked private members](coverage-gate-clr-invoked-private-members.md) — never gate AssemblyResolve-style members at >=90%
- [Named coverage exception: verify the member body](named-coverage-exception-verify-member-body.md) — gap-closure precedes the clean-pass task
- [Enumerate condition outcomes before the case list](enumerate-condition-outcomes-before-case-list.md) — 2 outcomes per `||`/`&&`
- [#441 Cobertura arithmetic](project_441_cobertura_arithmetic_plan_seams.md) — two-file pin vs 500-line ceiling; StrictMode throws
- [#457 closure-filter](project_457_closure_filter_plan_seams.md) — the pipeline overwrites raw Cobertura in place

## File-size and refactor mechanics

- [C# pure-move extraction pattern](csharp-pure-move-extraction-pattern.md) — keep the static-ctor install trigger; relocation not new module
- [Re-scoping a plan after a sibling landed the fix](plan-rescope-after-sibling-landed-the-fix.md) — split the contiguous tail
- [#400 partial-class headroom placement](project_400_partial_class_headroom_placement.md) — new cases in existing `.Part2.cs` partials
- [Post-format file-size audit](feedback_postformat_file_size_audit.md) — 500-line audit runs after final csharpier format
- [Embedded-resource fail-proof needs a rebuild gate](embedded-resource-failproof-rebuild-gate.md) — edit → rebuild → assert bytes

## Domain seams (TaskMaster)

- [#445 keyboard-action](project_445_keyboard_action_plan_seams.md) — resolve WS at execution time; scope epic-child gates
- [#446 QuickFiler bug family](project_446_quickfiler_bug_family_plan_seams.md) — ScoringServiceFactory seam before COM-path tests
- [#438 search-focus](project_438_search_focus_plan_seams.md) — additive overload broke 7 test files
- [#424 QuickFiler deadline](project_424_quickfiler_deadline_plan_seams.md) — overload migration breaks loose-mock Setup/Verify
- [#351 QuickFiler breadcrumb](project_351_quickfiler_breadcrumb_plan_seams.md) — JSON code in UtilitiesCS only; coordinator pattern
- [#349 EfcViewer breadcrumb](project_349_efcviewer_breadcrumb_plan_seams.md) — P0 halt-gate on the 9101 provider
- [#230 WinForms pump seam](project_230_winforms_pump_seam_plan_facts.md) — factory seam params before SaveParameters
- [#211 startup-lifetime heartbeat](project_211_startup_lifetime_heartbeat_seam.md) — DispatcherTimer in ThisAddIn.cs
- [#292 CurrentStoreContext](project_292_currentstorecontext_parallel_seam.md) — process-global static; needs `[DoNotParallelize]`
- [#307 F2 ScoCollection deletion gate](project_307_f2_scocollection_deletion_gate.md) — full first-party reference set incl. tests
- [#328 store exclusion](project_328_store_exclusion_seams.md) — near-limit files; new test `.cs` need csproj wiring
- [#295 WinForms STA exemptions](project_winforms_sta_refinement_exemption_rule.md) — keep dialog/Form/launcher only; control-identity pattern: companion interface + `*.StaTests.cs`, never a Form (`project_sta_last_resort_control_identity_pattern.md`)
- [Manager AsyncLazy shared seam](project_manager_asynclazy_shared_seam.md) — key-specific accessor, never retype the dictionary value
- [Folder predictor AF holder seam](project_folder_predictor_af_holder_seam.md) — Folder-only holder on IAppAutoFileObjects
- [Dispatcher repro hang trap](dispatcher-repro-hang-trap.md) — use an owned pumping STA thread

## Spec and artifact hygiene

- [Never embed absolute host paths](../_shared_no_absolute_host_paths.md) — use `<repo-root>` / `<user>` / `<host>`
