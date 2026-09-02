# Atomic Planner Memory Index

## Preflight revision seams (per-issue)

- [#468](project_468_preflight_revision_seams.md) — seam before red test; `[expect-fail]` on run tasks only; epic-child merge-base
- [#501 R1](untracked-file-and-linecount-gate-seams.md) — `git add -N` before grepping plan-created files; `(Get-Content).Count`
- [#501 R3](project_501_r3_preflight_seams.md) — repo-wide 0-skipped gates unsatisfiable; BASELINE_FAILURE_SET subset
- [#511 R1](project_511_r1_preflight_delta_seams.md) — mid-cycle evidence deletion; post-commit git-log scans; per-class coverage noise
- [#484](project_484_qfc_revision_seams.md) — ownership sweeps plan→issue.md→spec.md (spec is the AC source); old-cardinal grep sweep
- [QfcItemController capacity squeeze](project_qfcitemcontroller_test_capacity_squeeze.md) — ~471 spare lines across 4 owned test files, `.csproj` edits barred
- [#494](project_494_threshold_reconciliation_plan_seams.md) — coverage runner throws before post-processing; reported floor stays non-Blocking
- [#498](conditional-ladder-and-unowned-class-gates.md) — gate EVERY ladder rung incl. rung 1; 0/0 changed-line figure → NOT APPLICABLE
- [#503](project_503_ribbon_readiness_plan_seams.md) — RibbonViewer 487/500 forces a region move; 6+4 Compile entries; compile-time red
- [#505](project_505_toggle_state_guards_plan_seams.md) — runtime red (no dossier); raw cobertura to gitignored `coverage/`
- [#512](project_512_toolchain_gate_fidelity_plan_seams.md) — same-line `/t:Build`+`Nullable=enable` grep; `-EnableNullable` no-op via EXIT 0
- [#553](project_553_ci_parallel_split_plan_seams.md) — workflow-only scope; no jq; pathspec anchoring; ruleset PUT orchestrator-gated
- [#614](project_614_store_root_leak_plan_seams.md) — net non-growth AC; seam phase reconciles fail-before with a signature change
- [#464 R3/R4](project_464_efc_controller_plan_seams.md) — additive-only growth is a ceiling not a shrink; phase-N counts must survive earlier deletions
- [#677 R1–R8](project_677_keyboard_focus_leak_plan_seams.md) — ctor param REJECTED (reflection-arity tests); typed harness for compile-red
- [#635](project_635_reflective_caller_audit_plan_seams.md) — evidence-only audit: the scan hits its own pattern list; pathspec breadth inflates counts
- [#440 R1–R4](project_440_breadcrumb_left_arrow_plan_seams.md) — deletion-only change voids a diff-derived changed-line gate; `(Rebuild target(s))`
- [#637 R6](project_637_r6_superseded_spec_claim_seams.md) — plan narrates spec edits it never performs; stale-site list short by 3
- [#637 R2–R5](project_637_selectrow_rooted_path_plan_seams.md) — broad `docs/features/active` operand; blanket `-F` breaks regex; CRLF round-trip
- [#469 R1–R3](project_469_comment_accuracy_plan_seams.md) — number SWAP voids whole-file token gates; `- [x] AC1` prefixes `AC10`
- [#644](project_644_ac16_referral_revision_seams.md) — AC's named instrument prints no figure; substitute noise exceeds the delta
- [#644 cycle 2](project_644_cycle2_sweep_gate_evasion_seams.md) — rewording out of a detector's match set is gate evasion; SHA-256 pair
- [#644 PA-7](project_644_pa7_redaction_plan_seams.md) — untracked audit artifact still enters main; name-status diff blind to it
- [#648](project_648_ungated_static_swap_plan_seams.md) — lines-valid equality UNSATISFIABLE, use 5% tolerance; `git tag` not idempotent
- [#656](project_656_closecompleted_guard_plan_seams.md) — no TestCaseFilter override in either wrapper; wrapper writes no TRX; class nodes lack lines-covered/lines-valid; TestResults\ must be created first
- [#662](project_662_banner_prefix_arity_plan_seams.md) — `AC5` is a prefix of `AC5b`; `("===")` not a substring of `("====")`
- [#662 R3](project_662_round3_trx_hygiene_and_verbatim_seams.md) — `*.trx` is NOT gitignored; sweep case-insensitively; exclude the plan file from the sweep's own zero-hit gate
- [#662 R2](project_662_banner_prefix_revision_round_seams.md) — `'*.xml'` scope gate hits the plan's own cobertura evidence; loop-restart needs a baseline-relative failure test
- [#663](project_663_qfc_alt_chord_plan_seams.md) — defect-preserving seam turns compile-red into runtime red; derive named-test outcomes from totals
- [#670](project_670_capture_time_sanitisation_seams.md) — a script that echoes a vswhere-resolved path leaks via an *indirect* invoker; gate sanitisation at capture time
- [#680](project_680_menu_mode_plan_seams.md) — HostTests.cs 499 not 500; TRX 5-shape identifiers, `grep -a`; exact line arithmetic

## Plan-structure traps

- [Verify test provenance before a deletion](verify-test-provenance-before-planning-deletion.md) — read the test at the pre-cycle commit
- [Validator phase-heading constraint](plan-validator-phase-heading-constraint.md) — exact `### Phase N — <Title>`
- [Validator task-ID sequential constraint](plan-validator-task-id-sequential-constraint.md) — digit-only, sequential; insertion forces renumber
- [Planner may lack the MCP validator](project_planner_mcp_validator_not_in_tool_surface.md) — report VALIDATOR NOT RUN / COMMIT NOT RUN
- [Fenced `#` comments look like headings](plan-fenced-powershell-comments-look-like-headings.md) — indent column-0 `#` inside code fences
- [One AC per check-off task](feedback_ac_checkoff_one_per_task.md) — preflight rejects batched AC check-offs
- [Terminal-phase planner traps](terminal-phase-planner-traps.md) — unowned follow-ups; artifacts written after the clean-tree commit task
- [A reviewer's enumeration may be deliberately narrow](reviewer-enumeration-may-be-deliberately-narrow.md) — omitted IDs are often bound by a sibling clause; "completing" the list can falsify its trailing predicate
- [Never plan a mid-plan halt on MCP availability](never-plan-a-mid-plan-halt-on-mcp-availability.md) — Phase 0 probe + record-and-continue
- [Thread granted discharges through consumers](thread-granted-discharges-through-consumers.md) — softening one task strands its discharge
- [Durable script copy into feature folder](durable-script-copy-into-feature-folder.md) — copy scratchpad scripts into `<FEATURE>/scripts/`
- [Evidence path normalization](evidence-path-normalization.md) — normalize `evidence/coverage/` to `baseline/` + `qa-gates/`

## Acceptance-condition authoring

- [Acceptance edits must be false-before/true-after](acceptance-edits-must-be-false-before-true-after.md) — a clause already true is a no-op gate
- [Zero-hit grep gates need carve-outs](zero-hit-grep-gates-need-carveouts.md) — denial text makes "no hits" unsatisfiable
- [Single-numeral gates must name the role](single-numeral-gates-must-name-the-role.md) — count the enforced occurrence
- [Superseding a floor must name CLAUDE.md](superseding-a-coverage-floor-must-name-claude-md.md) — omission implies its rank-1 floor survives
- [MCP promotion route seams](mcp-promotion-route-plan-seams.md) — bug entry point; `promotion_type`+`work_mode`; stage `docs/features/potential`
- [Wiring gates must be wiring-sensitive](feedback_wiring_gates_must_be_wiring_sensitive.md) — count floors deflate with the defect they guard
- [Research claims as acceptance clauses](research-claims-as-acceptance-clauses.md) — never encode an unmeasured third-party claim
- [Literal-call clauses block file-size tightening](literal-call-clauses-block-file-size-tightening.md) — unsatisfiable near a 500-line file
- [Enumeration variable must match its consumer](enumeration-variable-must-match-consumer.md) — mismatch = zero-assembly run reporting zero failures
- [Diff gates need a commit task](diff-gates-need-a-commit-task.md) — `git diff <BASE>..HEAD` passes vacuously with no commit task
- [Never pin a HEAD SHA as a plan expectation](never-pin-head-sha-as-plan-expectation.md) — gate on tree invariants instead
- [Harness gitStatus may describe another worktree](harness-git-status-may-describe-another-worktree.md) — measure inside the target worktree
- [Absolute counts in shared files go stale](absolute-counts-in-shared-files-go-stale.md) — baseline-relative for co-owned files
- [.claude/agent-memory is tracked](agent-memory-is-tracked-scope-git-gates.md) — scope every diff/status/grep gate
- [.gitignore does not untrack an indexed path](gitignore-does-not-untrack-indexed-paths.md) — a force-added file stays tracked; verify against the index
- [Stale build output is not evidence of existence](stale-build-output-is-not-evidence-of-existence.md) — verify with `git ls-files`, not `obj/`
- [Observation scope must match blast radius](observation-scope-must-match-blast-radius.md) — space, time, and spelling of the observation
- [Run-time-derived account-token pattern](runtime-derived-account-token-pattern.md) — `Split-Path -Leaf $env:USERPROFILE`; self-exempt

## C# toolchain and test mechanics

- [Phase 0 toolchain bootstrap](project_csharp_phase0_toolchain_bootstrap.md) — SDK bootstrap unblocks csharpier; mandatory NuGet restore
- [Agent worktrees need SDK + NuGet + analyzer backfill](agent-worktrees-need-sdk-and-nuget-bootstrap.md) — four Phase 0 steps; CS0006 is an error
- [vstest scoped-run + csharpier 1.2.6 commands](reference_vstest_scoped_run_command.md) — vswhere + `/InIsolation` + `/TestCaseFilter`
- [CSharpier gate: format not pipe-files](csharpier-format-not-pipe-files-gate.md) — `pipe-files` is stdout-only and non-enforcing
- [CSharpier "Formatted N files" is processed count](csharpier-formatted-n-is-processed-count.md) — define rewritten-count via before/after SHA-256
- [Repo-wide csharpier format breaks zero-diff ACs](csharpier-repowide-format-breaks-zero-diff-acs.md) — scope the mutating pass to owned paths
- [.csharpierignore scope](csharpierignore-scope-packages-config.md) — only `*.csproj`/`*.props`/`*.targets` excluded; `packages.config` is NOT
- [.gitignore bracket classes defeat a literal grep](gitignore-bracket-classes-defeat-literal-grep.md) — `[Tt]est[Rr]esult*/` does ignore `TestResults/`
- [`/Logger:trx` needs `/ResultsDirectory`](trx-needs-resultsdirectory.md) — give each run task its own `p#-t#` subdirectory
- [`[expect-fail]` needs a synchronous seam](expect-fail-needs-a-synchronous-seam.md) — async-void boundaries false-GREEN
- [Invoke-MSTestWithCoverage.ps1](reference_invoke_mstest_with_coverage_script.md) — canonical full-suite Cobertura runner
- [Invoke-MSTest.ps1 single-SearchRoot defect](reference_invoke_mstest_single_searchroot_defect.md) — always pass `-SearchRoot .`
- [`Task "Csc"` needs detailed verbosity](msbuild-task-csc-literal-needs-detailed-verbosity.md) — use a detailed `/flp:` log or an output-assembly timestamp
- [PoshQC MCP + msbuild facts](poshqc-mcp-and-msbuild-invocation-facts.md) — MCP returns no counts; pair with direct runs
- [pwsh -Command payload quoting](pwsh-command-payload-quoting.md) — outer single quotes, inner doubles
- [Pester exits 0 on failing It blocks](pester-invoke-does-not-exit-nonzero.md) — scope every exit-code clause to a named channel
- [PowerShell gate observables](powershell-gate-observables.md) — no Invoke-Pester exit code; explicit `scan_folders`
- [Legacy csproj wiring](project_legacy_csproj_explicit_compile_include.md) — `Compile Include` + own `Reference`
- [Invoke-VSBuild rewrites csproj HintPaths](invoke-vsbuild-rewrites-csproj-hintpaths.md) — use vswhere-resolved MSBuild instead
- [Declaration-only seam task for fail-before](declaration-only-seam-task-for-fail-before.md) — seam task before the whole-set red run
- [net48 / nullable context mismatch](project_nullable_context_mismatch_prod_vs_test.md) — check `#nullable enable` and missing `<LangVersion>`
- [Worktree root breaks the `\.claude\` exclusion](worktree-root-breaks-dotclaude-exclusion.md) — assert a workspace-root prefix instead

## Coverage

- [Deletion-adjusted coverage no-regression gate](deletion-adjusted-coverage-no-regression-gate.md) — gate on covered/valid counters
- [#489 PartN reroute amendment seams](project_489_partn_reroute_amendment_seams.md) — verify parent `partial`; amendments shift AC line citations
- [Spec corrections sweep sibling sections](feedback_spec_corrections_sweep_sibling_sections.md) — cover Scope/Out-of-scope/Rollout, not AC only
- [#493 UiThread dispatcher plan seams](project_493_uithread_dispatcher_plan_seams.md) — stage the two `<Compile Include>` lines for a real red build
- [#442 QuickFiler metrics plan seams](project_442_quickfiler_metrics_plan_seams.md) — commented-out code defeats zero-hit grep gates
- [#468 QfcCollectionController plan seams](project_468_qfc_collection_controller_plan_seams.md) — a sign-defect seam must land carrying the defect
- [Threshold conflict: CLAUDE.md vs general-unit-test](project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md) — 80/90 vs 85/75
- [JaCoCo hook, Cobertura also accepted](project_csharp_coverage_gate_jacoco_format.md) — follow the format the delta names
- [Async state machines split the denominator](async-state-machine-coverage-aggregation.md) — aggregate by `filename`
- [Dead-code removal vs coverage exclusion](project_deadcode_removal_vs_coverage_exclusion.md) — shrink the denominator, never exclude
- [CLR-invoked private members](coverage-gate-clr-invoked-private-members.md) — never gate AssemblyResolve-style members at >=90%
- [Named coverage exception: verify the member body](named-coverage-exception-verify-member-body.md) — gap-closure before the clean-pass task
- [Enumerate condition outcomes before the case list](enumerate-condition-outcomes-before-case-list.md) — 2 outcomes per condition
- [#441 Cobertura arithmetic](project_441_cobertura_arithmetic_plan_seams.md) — two-file pin vs 500-line ceiling; StrictMode throws
- [#457 closure-filter](project_457_closure_filter_plan_seams.md) — the pipeline overwrites raw Cobertura in place

## File-size and refactor mechanics

- [C# pure-move extraction pattern](csharp-pure-move-extraction-pattern.md) — keep the static-ctor install trigger
- [Re-scoping after a sibling landed the fix](plan-rescope-after-sibling-landed-the-fix.md) — split the file's contiguous TAIL
- [#400 partial-class headroom placement](project_400_partial_class_headroom_placement.md) — put new cases in existing `.Part2.cs` partials
- [Post-format file-size audit](feedback_postformat_file_size_audit.md) — the 500-line audit goes AFTER the final csharpier format
- [Embedded-resource fail-proof needs a rebuild gate](embedded-resource-failproof-rebuild-gate.md) — edit → rebuild → assert bytes

## Domain seams (TaskMaster)

- [#445 keyboard-action](project_445_keyboard_action_plan_seams.md) — resolve WS at execution time; scope epic-child gates
- [#446 QuickFiler bug family](project_446_quickfiler_bug_family_plan_seams.md) — ScoringServiceFactory seam before COM-path tests
- [#438 search-focus](project_438_search_focus_plan_seams.md) — additive overload broke 7 test files; keep the old overload's default path
- [#424 QuickFiler deadline](project_424_quickfiler_deadline_plan_seams.md) — overload migration breaks loose-mock Setup/Verify
- [#351 QuickFiler breadcrumb](project_351_quickfiler_breadcrumb_plan_seams.md) — JSON code in UtilitiesCS only; coordinator pattern
- [#349 EfcViewer breadcrumb](project_349_efcviewer_breadcrumb_plan_seams.md) — P0 halt-gate on the 9101 provider
- [#230 WinForms pump seam](project_230_winforms_pump_seam_plan_facts.md) — factory seam params before SaveParameters
- [#211 startup-lifetime heartbeat](project_211_startup_lifetime_heartbeat_seam.md) — DispatcherTimer in ThisAddIn.cs
- [#292 CurrentStoreContext](project_292_currentstorecontext_parallel_seam.md) — process-global static; needs `[DoNotParallelize]`
- [#307 F2 ScoCollection deletion gate](project_307_f2_scocollection_deletion_gate.md) — full first-party reference set incl. tests
- [#328 store exclusion](project_328_store_exclusion_seams.md) — near-limit files; new test `.cs` need csproj wiring
- [#295 WinForms STA exemptions](project_winforms_sta_refinement_exemption_rule.md) — keep dialog/Form/launcher exemptions only
- [#295 STA control-identity pattern](project_sta_last_resort_control_identity_pattern.md) — companion interface + `*.StaTests.cs`
- [Manager AsyncLazy shared seam](project_manager_asynclazy_shared_seam.md) — key-specific accessor, never retype the dictionary value
- [Folder predictor AF holder seam](project_folder_predictor_af_holder_seam.md) — Folder-only holder on IAppAutoFileObjects
- [Dispatcher repro hang trap](dispatcher-repro-hang-trap.md) — use an owned pumping STA thread

## Spec and artifact hygiene

- [Never embed absolute host paths](../_shared_no_absolute_host_paths.md) — use `<repo-root>` / `<user>` / `<host>`
