# Atomic Planner Memory Index

## Preflight revision seams (per issue; newest first)

- [#985 R1](project_985_r1_scratchpad_identity_and_double_write_seams.md) — never spell the scratchpad (encoded name holds the account); sweep diff + untracked; reflowed tag forces a double write
- [#985 R0](project_985_r0_rehearsal_fidelity_and_hygiene_path_seams.md) — merge-into-Dependabot rehearsal needs a simulated nuget update; hygiene guard flags Users paths in tracked plans; Invoke-VSBuild rewrites csproj
- [#973 R1](project_973_r1_last_task_artifact_only_commit_and_measured_line_length_seams.md) — last task commits its artifact alone; dossier cites no later result; measure line length with `^.{N,}`; Glob zero-gate needs a positive control
- [#973 cycle 1](project_973_cycle1_premise_correction_refscan_and_tautology_fold_seams.md) — false-premise AC fixed by a refscan + positive control; a "tautology" may be an analyzer workaround; commit-per-task flip-then-commit
- [#964 cycle 1](project_964_cycle1_test_only_coverage_gap_remediation_seams.md) — test-only coverage-gap plan: dossier not fail-before, data-row trx prefix match, commit-task check-off circularity
- [Seams #882–#973](index_preflight_seams_900_to_973.md) — sub-index: #968, #964 R0-R4, #959, #956, #953, #952, #950, #948, #947, #945, #940, #931, #930, #929, #928, #927, #944, #942, #882, #973 R0-R6
- [Legacy seams #136–#900](index_legacy_preflight_seams_pre_900.md) — sub-index of every per-issue seam note for #900 and earlier (#900, #895, #873, #871, #839 … #136)

## Plan structure and revision mechanics

- [Phase heading](plan-validator-phase-heading-constraint.md) · [Task IDs](plan-validator-task-id-sequential-constraint.md) · [Task counts](task-counts-must-be-mechanical-and-recorded.md) · [Colliding IDs](preflight-delta-colliding-task-ids.md)
- [Validator may be absent](project_planner_mcp_validator_not_in_tool_surface.md) · [Never halt on MCP](never-plan-a-mid-plan-halt-on-mcp-availability.md) · [Hook gotchas](validate-planner-output-hook-line-anchored-gotchas.md)
- [Fenced `#` looks like heading](plan-fenced-powershell-comments-look-like-headings.md) · [CRLF plans validate](crlf-plans-validate-do-not-normalize.md) · [Evidence path normalization](evidence-path-normalization.md)
- [One AC per check-off](feedback_ac_checkoff_one_per_task.md) · [Terminal-phase traps](terminal-phase-planner-traps.md) · [Planners amend ACs](acceptance-criteria-are-amended-by-planners-not-executors.md)
- [AC source incl. DoD](ac-source-sweep-definition-of-done.md) · [Spec corrections sweep siblings](feedback_spec_corrections_sweep_sibling_sections.md) · [Self-consistency sweeps](plan-self-consistency-sweeps.md)
- [Re-derive aggregates after deltas](plan-aggregate-claims-must-be-rederived-after-deltas.md) · [Decomposition covers inserted tasks](decomposition-must-cover-newly-inserted-tasks.md)
- [Verify in ASSIGNED worktree](verify-citations-in-the-assigned-worktree.md) · [Caller corrections can be off](verify-caller-supplied-citation-corrections.md) · [Line spans + literals](verify-line-spans-and-computed-literals.md)
- [Test provenance before deletion](verify-test-provenance-before-planning-deletion.md) · [Narrow reviewer lists](reviewer-enumeration-may-be-deliberately-narrow.md) · [Thread discharges](thread-granted-discharges-through-consumers.md)
- [Re-scope after sibling fix](plan-rescope-after-sibling-landed-the-fix.md) · [Durable script copy](durable-script-copy-into-feature-folder.md) · [MCP promotion route](mcp-promotion-route-plan-seams.md)

## Acceptance-condition authoring

- [False-before/true-after](acceptance-edits-must-be-false-before-true-after.md) · [Zero-hit carve-outs](zero-hit-grep-gates-need-carveouts.md) · [Single-numeral role](single-numeral-gates-must-name-the-role.md)
- [Floor supersede names CLAUDE.md](superseding-a-coverage-floor-must-name-claude-md.md) · [Wiring-sensitive gates](feedback_wiring_gates_must_be_wiring_sensitive.md) · [No research claims](research-claims-as-acceptance-clauses.md)
- [Literal-call vs size](literal-call-clauses-block-file-size-tightening.md) · [Enumeration var = consumer](enumeration-variable-must-match-consumer.md) · [Diff gates need a commit](diff-gates-need-a-commit-task.md)
- [Never pin HEAD SHA](never-pin-head-sha-as-plan-expectation.md) · [Empty porcelain unsatisfiable](empty-porcelain-clause-is-unsatisfiable.md) · [Porcelain collapses dirs](porcelain-collapses-untracked-directories.md)
- [Self-referential evidence](self-referential-evidence-enumeration.md) · [agent-memory is tracked](agent-memory-is-tracked-scope-git-gates.md) · [Harness gitStatus](harness-git-status-may-describe-another-worktree.md)
- [.gitignore never untracks](gitignore-does-not-untrack-indexed-paths.md) · [Existence not retention](existence-is-not-retention-gate-committed-artifacts.md) · [Stale build output](stale-build-output-is-not-evidence-of-existence.md)
- [Absolute counts go stale](absolute-counts-in-shared-files-go-stale.md) · [Scope = blast radius](observation-scope-must-match-blast-radius.md) · [Account token](runtime-derived-account-token-pattern.md)
- [Measured vs confirming run](two-run-gates-need-a-measured-vs-confirming-split.md) · [Absence gate on carrying file](absence-gate-must-target-the-file-that-carries-it.md)
- [No .Method.Name on lambdas](never-assert-method-name-on-lambda-valued-delegate.md) · [Baseline-relative gates](baseline-relative-toolchain-gates-and-vacuous-diff-comparators.md)
- [Repo-relative stale-worktree guard](stale-worktree-guard-must-be-repo-relative.md) · [Seam shape](seam-shape-must-match-target-cardinality-and-mutability.md)

## C# toolchain and test mechanics

- [Phase 0 bootstrap](project_csharp_phase0_toolchain_bootstrap.md) · [Worktree SDK/NuGet](agent-worktrees-need-sdk-and-nuget-bootstrap.md) · [vstest + csharpier cmds](reference_vstest_scoped_run_command.md)
- [format not pipe-files](csharpier-format-not-pipe-files-gate.md) · ["Formatted N" processed count](csharpier-formatted-n-is-processed-count.md) · [Repo-wide format vs zero-diff](csharpier-repowide-format-breaks-zero-diff-acs.md)
- [.csharpierignore scope](csharpierignore-scope-packages-config.md) · [.gitignore bracket classes](gitignore-bracket-classes-defeat-literal-grep.md) · [TRX host tokens](trx-carries-host-tokens-in-two-casings.md)
- [TRX needs ResultsDirectory](trx-needs-resultsdirectory.md) · [expect-fail sync seam](expect-fail-needs-a-synchronous-seam.md) · [Declaration-only seam](declaration-only-seam-task-for-fail-before.md)
- [Invoke-MSTestWithCoverage](reference_invoke_mstest_with_coverage_script.md) · [SearchRoot defect](reference_invoke_mstest_single_searchroot_defect.md) · [Csc needs detailed](msbuild-task-csc-literal-needs-detailed-verbosity.md)
- [PoshQC MCP facts](poshqc-mcp-and-msbuild-invocation-facts.md) · [PoshQC limits](reference_poshqc_mcp_measurement_limits.md) · [PS gate observables](powershell-gate-observables.md)
- [pwsh payload quoting](pwsh-command-payload-quoting.md) · [pwsh quoting in tasks](pwsh-command-quoting-in-plan-tasks.md) · [Pester exit 0](pester-invoke-does-not-exit-nonzero.md)
- [Legacy csproj wiring](project_legacy_csproj_explicit_compile_include.md) · [Invoke-VSBuild HintPaths](invoke-vsbuild-rewrites-csproj-hintpaths.md) · [Nullable mismatch](project_nullable_context_mismatch_prod_vs_test.md)
- [Worktree root vs `\.claude\`](worktree-root-breaks-dotclaude-exclusion.md) · [CS0236 seam default](csharp-seam-default-cs0236-and-intermediate-consumers.md) · [Partial seam same phase](partial-class-seam-declaration-and-consumption-same-phase.md)
- [PS batch budget](powershell-batch-budget-caps-plan-authored-helpers.md) · [Fixture sizing](test-fixture-sizing-lines-per-test.md) · [Per-phase size needs csharpier](per-phase-size-gates-need-scoped-csharpier.md)

## Coverage

- [Repo-wide rate nondeterministic](repo-wide-cobertura-line-rate-is-nondeterministic.md) · [Deletion-adjusted gate](deletion-adjusted-coverage-no-regression-gate.md) · [Threshold conflict](project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md)
- [JaCoCo hook](project_csharp_coverage_gate_jacoco_format.md) · [Async state machines](async-state-machine-coverage-aggregation.md) · [CLR-invoked privates](coverage-gate-clr-invoked-private-members.md)
- [Named exception: read body](named-coverage-exception-verify-member-body.md) · [Condition outcomes first](enumerate-condition-outcomes-before-case-list.md) · [EFCC voids rows](excludefromcodecoverage-voids-per-file-coverage-rows.md)
- [Dead code: remove not exclude](project_deadcode_removal_vs_coverage_exclusion.md) · [Dead code retained to ledger](deadcode-retained-residual-to-ledger.md)

## File-size and refactor

- [Pure-move extraction](csharp-pure-move-extraction-pattern.md) · [Post-format size audit](feedback_postformat_file_size_audit.md) · [Embedded-resource rebuild](embedded-resource-failproof-rebuild-gate.md)

## Domain seams (TaskMaster)

- [#445](project_445_keyboard_action_plan_seams.md) · [#446](project_446_quickfiler_bug_family_plan_seams.md) · [#438](project_438_search_focus_plan_seams.md) · [#424](project_424_quickfiler_deadline_plan_seams.md)
- [#351](project_351_quickfiler_breadcrumb_plan_seams.md) · [#349](project_349_efcviewer_breadcrumb_plan_seams.md) · [#230](project_230_winforms_pump_seam_plan_facts.md) · [#211](project_211_startup_lifetime_heartbeat_seam.md)
- [#292](project_292_currentstorecontext_parallel_seam.md) · [#307](project_307_f2_scocollection_deletion_gate.md) · [#328](project_328_store_exclusion_seams.md) · [Dispatcher hang](dispatcher-repro-hang-trap.md)
- [WinForms STA exemptions](project_winforms_sta_refinement_exemption_rule.md) · [Control-identity](project_sta_last_resort_control_identity_pattern.md)
- [Manager AsyncLazy](project_manager_asynclazy_shared_seam.md) · [Folder predictor holder](project_folder_predictor_af_holder_seam.md)

## Artifact hygiene

- [No absolute host paths](../_shared_no_absolute_host_paths.md) — use `<repo-root>` / `<user>` / `<host>`; the CI hygiene guard flags drive-rooted Users paths in any tracked file (see #985 R0)
