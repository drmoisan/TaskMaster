# Orchestrator Memory Index

## Lifecycle, promotion, and routing

- [Evidence + lifecycle for every change](evidence-and-lifecycle-for-every-change.md) — promote to issue + active folder before ANY implementation, even 1-file fixes; evidence only under a feature folder
- [Small-path = minor-audit selection](small-path-minor-audit-selection.md) — 1-3 production-file bug = small path + minor-audit; no spec.md, AC lives in issue.md
- [potential_to_issue creates the GitHub issue](potential-to-issue-creates-github-issue.md) — the tool opens the issue itself; do not also `gh issue create`
- [potential_to_issue needs an absolute path](potential-to-issue-needs-absolute-path.md) — workspace-relative `potential_path` fails "not found" in a worktree
- [Promotion potential .md may not persist](promotion-potential-md-may-not-persist.md) — the promoted .md may be absent on disk; recreate for audit trail, not a failure
- [new_active_feature_folder date prefix](new-active-feature-folder-date-prefix.md) — standalone folders get the YYYY-MM-DD- prefix automatically; epic-child folders don't
- [MCP tools available to orchestrator](mcp-tools-available-to-orchestrator.md) — if a worker reports MCP tools unavailable, run them yourself; don't accept the block
- [Planner/executor lack the MCP validator](planner-executor-lack-mcp-validator.md) — run the plan and orchestrator-state validator gates from the orchestrator thread
- [Verify subagent capability claims](feedback_verify_subagent_capability_claims.md) — never relay "agent type not registered" unchecked; demand the verbatim error

## Checkpoint and validators

- [orchestrator-state flat keys + step-status enum](orchestrator-state-flat-keys-and-enum.md) — FLAT top-level variable keys; `in_progress` not `in-progress`; re-read fable_policy each run
- [orchestrator-state validator divergence](orchestrator-state-validator-divergence.md) — MCP check is stricter than the SubagentStop hook; conform to the canonical remediation_loop shape
- [Prep-child checkpoint: hook paths + receipt shape](prep-child-checkpoint-hook-paths-and-receipt-shape.md) — hooks hard-code `artifacts/orchestration/orchestrator-state.json`; mirror a child-scoped checkpoint or delegation AND termination block
- [Portable completion gate allows blocked child](portable-completion-gate-allows-blocked-child.md) — no Python validator here, so a blocked child can still terminate + open a PR
- [Model-routing scripts absent on epic integration base](model-routing-scripts-absent-on-epic-integration-base.md) — compute routing by hand; the MCP validator still gates

## Planning and preflight

- [Preflight catches what the plan validator cannot](preflight-catches-what-the-plan-validator-cannot.md) — MCP passed 3x on a plan with 8 blocking defects; budget 2-3 cycles above ~100 tasks. See also [preflight-forward-referencing-acceptances.md](preflight-forward-referencing-acceptances.md)
- [Preflight rounds need carried context](preflight-iterations-need-carried-context.md) — no SendMessage, so each round is a FRESH executor; carry established-facts + iteration history + the ALL CLEAR threshold
- [MCP plan validator: run it and observe](mcp-plan-validator-defective-em-dash.md) — em-dash and CRLF both PASS despite older notes; don't pre-normalize. See also [mcp-plan-validator-requires-lf.md](mcp-plan-validator-requires-lf.md) (superseded)
- [MCP plan validator Edit/Write pervasive-diff](mcp-plan-validator-editwrite-pervasive-diff.md) — an Edit can trigger "no canonical phase headings"; restore via cp/sed/perl; executor preflight is the real gate
- [Remediation-plan em-dash required](remediation-plan-em-dash-required.md) — only canonical `### Phase N — <Title>` passes; `(continued)` is rejected
- [Preparation-mode plans need repo-relative paths](preparation-mode-plans-need-repo-relative-paths.md) — prep plans execute later in a DIFFERENT worktree; forbid absolute paths up front
- [Prep-child upstream dependency must be non-halting](prep-child-upstream-dependency-must-be-nonhalting.md) — tell planner AND preflight the wave-0 artifact is an execution-time read
- [Epic-child plan Phase 0 paths are stale](feedback_plan_phase0_paths_are_stale_in_epic_children.md) — redirect the executor to the CURRENT worktree in the delegation prompt
- [Scope exclusions must be complete in the prd-feature prompt](scope-exclusions-must-be-complete-in-prd-prompt.md) — one omitted deferral made an AC assert a forbidden deletion; fix in preparation, never mid-execution
- [Verify brief constraints before propagating](feedback_verify_brief_constraints_before_propagating.md) — epic-child brief KEY CONSTRAINTS can be factually wrong; confirm-or-refute, then correct issue.md + spec.md
- [Remediation loop strict handoff](remediation-loop-strict-handoff.md) — atomic-planner -> atomic-executor -> feature-review only; five artifacts per cycle

## Coverage

- [Cobertura line-rate attribute is wrong](cobertura-line-rate-attribute-is-wrong.md) — #441 + #478 both corrupt emitted line-rate/branch-rate and it distorts BOTH ways, so a file can falsely PASS. Recompute from deduplicated class-level `<line>` nodes keyed on `filename=`. Supersedes [cobertura-line-rate-double-counts.md](cobertura-line-rate-double-counts.md) and [cobertura-per-file-rates-corrupted-441.md](cobertura-per-file-rates-corrupted-441.md)
- [Repo-wide coverage: run the FULL suite](feedback_repowide_coverage_run_full_suite.md) — run ALL *.Test.dll together; a single-assembly run reports a false-low number (20.21% vs 81.19%)
- [Repo-wide coverage authority exception](feedback_repowide_coverage_authority_exception.md) — pre-existing repo-wide shortfall + passing change-scope gates → surface an authority-scoped exception, don't auto-cycle
- [feature-review 85% coverage floor trap](feature-review-coverage-85-floor-trap.md) — do NOT generate artifacts/csharp/coverage.xml at 80-85%; the hook hard-codes 85% and forces a false FAIL
- [ExcludeFromCodeCoverage partial-type trap](excludefromcodecoverage-partial-type-trap.md) — the attribute on one partial suppresses the whole type incl. *.Designer.cs; removal is usually net-positive
- [No coverage exemption when purpose is testability](feedback_no_coverage_exemption_when_purpose_is_testability.md) — plan real seams instead; contrast #223 which was ratified
- [Check for a prior ratified exemption boundary](check-for-prior-ratified-exemption-boundary.md) — search docs/features/archive BEFORE planning coverage work; a ratified boundary overrides an epic's instruction
- [Verify reducibility before accepting exemption count](feedback_verify_reducibility_before_accepting_exemption_count.md) — cross-check the residual against proven in-repo techniques
- [Whole-repo CI gate is not out-of-scope](whole-repo-ci-gate-not-out-of-scope.md) — a pre-existing repo-wide csharpier/lint failure blocks the PR's required check; fix it

## Epics

- [Epic children need full lifecycle + PRs](feedback_epic_children_require_full_lifecycle_and_prs.md) — maintainer rejected the executor-driver shortcut and direct --no-ff merges
- [Epic child PRs get no CI](project_epic_child_prs_no_ci.md) — ci.yml triggers only on main/development, so child→integration CI-green is vacuous; merge on blocking_count==0
- [Epic self-merge step9 gate sequencing](epic-mode-pr-merge-gate-sequencing.md) — merge gate needs epic_mode:true + step9_status "passed"; flip to "verified" after merge, before Stop
- [Epic-child self-merge: step9 passed vs verified](epic-child-self-merge-step9-passed-vs-verified.md) — the MCP validator REJECTS "passed"; keep it for the merge, flip after, revalidate
- [Epic-child PR-gate gotchas](epic-child-pr-gate-gotchas.md) — isolated-worktree collect_pr_context writes to the wrong checkout; the hook reads nested epic_context.integration_branch
- [Agent-worktree hooks resolve to agent cwd](agent-worktree-hooks-resolve-to-agent-cwd.md) — in `.claude/worktrees/agent-<id>` hooks read the agent worktree; do NOT copy the checkpoint to session root
- [Child-orchestrator PR hook reads SESSION ROOT](child-orchestrator-pr-hook-reads-session-root.md) — the contrasting named-worktree case; stage body/receipt/checkpoint at session root
- [Unplanned epic-child worktree mechanics](unplanned-epic-child-worktree-mechanics.md) — cross-worktree delegation works via absolute paths; C# tools need explicit paths (not on PATH)
- [Epic-child stale local integration ref](project_epic_child_stale_local_integration_ref.md) — `git fetch` and branch from `origin/<branch>`, never the bare local name
- [Re-fetch integration before declaring prep done](refetch-integration-before-declaring-prep-done.md) — siblings share .git and origin refs move silently; re-fetch, rebase, absorb, re-preflight
- [Parallel preparation children share one worktree](parallel-preparation-children-shared-worktree.md) — use a child-scoped checkpoint path + pathspec-scoped commit; don't revert a sibling's write
- [Subagent limit is session-wide](subagent-limit-shared-across-epic-children.md) — the concurrent-subagent cap is global across siblings; keep fan-out to 2-4, wait and retry, never do the step in-thread. See also [parallel-prep-children-subagent-saturation.md](parallel-prep-children-subagent-saturation.md), [parallel-children-share-subagent-limit.md](parallel-children-share-subagent-limit.md)
- [Epic-child agent-memory merge conflicts](epic-child-agent-memory-merge-conflicts.md) — child→integration PRs conflict solely on shared `.claude/agent-memory/*/MEMORY.md`; resolve by union, re-verify. See also [parallel-epic-children-conflict-on-agent-memory-index.md](parallel-epic-children-conflict-on-agent-memory-index.md), [epic-child-rebase-shared-memory-conflict.md](epic-child-rebase-shared-memory-conflict.md)
- [Parallel epic children name collisions](parallel-epic-children-name-collisions.md) — siblings coin identical type names; CS0101/CS0104 surface only at rebase; rename YOUR types
- [Epic generic-constraint cascades across children](epic-generic-constraint-cascades-multiple-children.md) — a ratified `where TKey : notnull` emits CS8714 in every unconstrained consumer; enumerate ALL consumers first
- [Epic-child nullable fan-in debt is deferred](project_epic_child_nullable_fanin_debt_deferred.md) — cross-child CS86xx fan-in is the Wave-2 capstone's job; don't over-remediate sibling files

## PR, CI, and merge

- [pr-author agent unavailable; run skill in-thread](pr-author-hook-blocks-gh-in-this-repo.md) — author body + SHA256 receipt in-thread and the hook permits `gh pr create`
- [pr_context.summary.txt unreliable](pr-context-summary-unreliable-gh-and-classification.md) — verify gh with `gh auth status`; author from the real diff, not the summary's claims
- [collect_pr_context lands in main checkout](collect-pr-context-lands-in-main-checkout.md) — copy pr_context.* into the worktree before `gh pr create`; checkpoint + pr_* are gitignored
- [Commit everything before the S9 CI gate](feedback_commit_before_ci_gate.md) — a post-gate commit moves the head SHA and forces an S9 re-run
- [Commit review artifacts + step8 preflight nuances](feedback_commit_review_artifacts_and_step8_preflight.md) — commit feature-review artifacts before rebase/PR; ff-only merge in a linked worktree
- [Migration posture before PR gate](feedback_migration_not_just_patch.md) — report integration/migration posture, not just a clean audit
- [Flaky CI: PhysicalFileInfoAdapter test](project_flaky_ci_physicalfileinfoadapter_test.md) — intermittent; re-run the failed job first, fix only if deterministic

## C#, tests, and toolchain

- [Fresh worktree: NuGet restore + csharpier v1](fresh-worktree-nuget-restore-and-csharpier-v1.md) — packages/ is absent and msbuild won't restore packages.config; `csharpier .` is v0 syntax that fails against pinned 1.2.6
- [C# analyzer packages.config quirks](csharp-analyzer-packages-config-quirks.md) — manual roslyn subfolder selection; SecurityCodeScan.VS2019 breaks Roslyn 5.6 via CS8032
- [Tests must not trigger UX or a live worker](feedback_tests_must_not_trigger_ux_or_live_worker.md) — seam the worker body and inject an inert delegate
- [STA controls ratified as last resort](feedback_sta_controls_last_resort_ratified.md) — unshown controls on STA only after seams, in dedicated *.StaTests.cs; Forms/popups/pumps still banned
- [Banned API in touched file is in scope](feedback_banned_api_in_touched_file_in_scope.md) — remediate DateTime.Now/UtcNow, Random.Shared, Thread.Sleep, Task.Delay in files you modify
- [VSTO startup STA threading directive](feedback_vsto_startup_sta_threading_directive.md) — minimize STA reliance, always pump, gate COM hookups on Outlook readiness

## Debugging and verification discipline

- [Git-blame regressions before novel hypotheses](feedback_gitblame_regressions_before_novel_hypothesis.md) — for "was working, now wrong", blame the exact lines and diff the responsible refactor first
- [Verify repro before bugfix cycle](feedback_verify_repro_before_bugfix_cycle.md) — a correct workaround can make a latent defect unreproducible; ground-truth on HEAD first
- [Re-verify ground truth after user mid-cycle commit](feedback_reverify_ground_truth_after_user_midcycle_commit.md) — re-probe line counts/merge-base/csproj and re-plan before executing a preflighted plan
- [Honor user's per-cycle folder layout](feedback_verify_flat_artifact_layout_after_executor.md) — revert only UNDIRECTED agent relocations, never the user's own reorg

## Project context

- [Store-lockup watchdog null-model hazard](project_store_lockup_watchdog_null_model_hazard.md) — new startup COM attribution scopes need a responder phase-branch before the disable-service write
- [Epic #295 winforms testability](project_epic_295_winforms_testability.md) — design-phase-only mandate; children 293/296/297/298; 298 depends on 297
- [Swordfish epic: clean collection premise is false](project_swordfish_removal_false_clean_collection.md) — the "already in repo" ConcurrentObservableCollection doesn't exist; F2 must create it
- [Swordfish epic F5 ScoDictionary blocker (RESOLVED)](project_swordfish_epic_f5_blocked_on_old_scodictionary.md) — grep the OLD class base + using, not just the *New replacement
- [VS Code extension location](project_extension_location.md) — the extension lives at `extensions/drm-copilot/`, not the repo root
- [Verify package.json before vsce work](feedback_vsce_verify_package_location.md) — locate the publishable extension first in multi-package repos
- [Repo root is source of truth for codex bundle](feedback_repo_root_is_source_of_truth.md) — update the bundle to match repo `.codex/`, `.agents/`, `AGENTS.md`
