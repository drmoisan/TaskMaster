# Orchestrator Memory Index

## Lifecycle, promotion, checkpoint
- [Evidence + lifecycle for every change](evidence-and-lifecycle-for-every-change.md) · [Small-path = minor-audit](small-path-minor-audit-selection.md) — 1-3 prod files = small path
- [potential_to_issue creates the issue](potential-to-issue-creates-github-issue.md) · [needs an absolute path](potential-to-issue-needs-absolute-path.md) · [maps sections by heading](potential-to-issue-keeps-only-summary-section.md)
- [Promotion potential .md may not persist](promotion-potential-md-may-not-persist.md) · [new_active_feature_folder date prefix](new-active-feature-folder-date-prefix.md)
- [Verify an issue is open in SUBSTANCE](verify-issue-still-open-in-substance.md) — the residual may already be its own issue
- [Footprint AC forbids on-branch follow-up promotion](footprint-ac-forbids-onbranch-followup-promotion.md)
- [orchestrator-state.json is TRACKED in git](orchestrator-state-json-is-tracked-in-git.md) — skip-worktree BEFORE first write
- [Bootstrapping the first checkpoint write](bootstrapping-orchestrator-state-json-first-write.md) — Write tool can't create it
- [Checkpoint gate exact key names](checkpoint-gate-exact-key-requirements.md) — pre-impl wants `lifecycle_ready`
- [Flat keys + step-status enum](orchestrator-state-flat-keys-and-enum.md) · [validator divergence](orchestrator-state-validator-divergence.md) · [`completed` write-locks](step-status-completed-write-locks-checkpoint.md)
- [Completion-gate receipt shapes](completion-gate-receipt-shapes.md) · [namespaces + owner race](checkpoint-receipt-namespaces-and-owner-race.md) — 8-key receipts; MCP DOES accept `agents`; extra keys ignored
- [blocked_reason enum can't express a substantive halt](blocked-reason-enum-cannot-express-substantive-halt.md) · [Removing a halt needs branch propagation](removing-a-halt-requires-branch-propagation.md)
- [Shared checkpoint: never read-modify-write](shared-checkpoint-read-modify-write-corrupts.md) · [Resumed child shares your worktree](resumed-child-orchestrator-shares-worktree.md)
- [MCP tools available to orchestrator](mcp-tools-available-to-orchestrator.md) — run them yourself if a worker can't
- [Run the real hook, not MCP](run-orchestration-hook-gates-locally.md) — the MCP validator disagrees on the bug route

## PR authoring and CI gate
- [pr-author is a skill, not an agent](pr-author-hook-blocks-gh-in-this-repo.md) · [exact checkpoint schema](pr-author-hook-exact-checkpoint-schema.md) — agents must be a LIST; `relativeFile` required
- [PR readiness gate bars ANY recorded override](pr-readiness-gate-bars-any-recorded-override.md) — step8 must not be `pending`
- [PR-creation checkpoint shape](pr-creation-readiness-exact-requirements.md) — Invoke-OrchestratorStatePreflight's exact demand
- [pr_context.summary.txt is unreliable](pr-context-summary-unreliable-gh-and-classification.md) · [top-N-churn truncation kills the coverage gate](pr-context-top-n-churn-truncation-kills-coverage-gate.md)
- [collect_pr_context lands in main checkout](collect-pr-context-lands-in-main-checkout.md) · [receipt staleness is mtime vs created_at](pr-author-receipt-staleness-is-mtime-vs-created-at.md)
- [Closing keyword fires inside a negation](closing-keyword-fires-inside-negation.md) — `does NOT fix #511` still closes it
- [Commit everything before the S9 CI gate](feedback_commit_before_ci_gate.md) · [Commit review artifacts + step8 preflight](feedback_commit_review_artifacts_and_step8_preflight.md)
- [External actor can merge your PR mid-run](external-actor-can-merge-your-child-pr-midrun.md) · [Migration posture before PR gate](feedback_migration_not_just_patch.md)
- [Whole-repo CI gate is not out-of-scope](whole-repo-ci-gate-not-out-of-scope.md) · [Flaky CI: PhysicalFileInfoAdapter](project_flaky_ci_physicalfileinfoadapter_test.md)
- [Local C# gate for a non-C# change](local-csharp-gate-for-a-non-csharp-change.md) — mirror the 2.1GB bootstrap from a sibling worktree; evidence XML already ignored
- [Executor-blocked AC may be orchestrator-dischargeable](executor-blocked-ac-may-be-orchestrator-dischargeable.md) — check your own tool surface; integration-base PRs don't auto-close
- [`gh issue create` is hook-blocked](gh-issue-create-blocked-by-promotion-mcp-hook.md) — no file-free promotion route; settle the commit target BEFORE the PR merges

## Verification discipline
- [My own negative claims need a scoped search](my-own-negative-claims-need-a-scoped-search.md) — I overturned a correct spec on a grep of the wrong file
- [My relayed delta needs the same satisfiability check](my-relayed-delta-must-pass-the-same-satisfiability-check.md) — a `docs/`-scoped grep matches the plan's own prose; trust an evidenced refusal
- [A subagent's correction can be FALSE](subagent-self-reported-correction-can-be-false.md) · [cites the wrong checkout's gitStatus](subagent-cites-harness-gitstatus-of-wrong-checkout.md) · [Verify capability claims](feedback_verify_subagent_capability_claims.md)
- [Reconcile plan numbers against your own measurements](reconcile-plan-numbers-against-your-own-measurements.md) · [Epic kickoff facts need independent measurement](epic-kickoff-facts-need-independent-measurement.md)
- [Verify repro before bugfix cycle](feedback_verify_repro_before_bugfix_cycle.md) · [Git-blame regressions first](feedback_gitblame_regressions_before_novel_hypothesis.md)
- [Re-verify ground truth after a user mid-cycle commit](feedback_reverify_ground_truth_after_user_midcycle_commit.md) · [Evidence timestamps can be synthesized](evidence-timestamps-can-be-synthesized.md)
- [Stale base anchor passes ancestry vacuously](stale-base-anchor-passes-ancestry-vacuously.md) — compare against origin/main
- [Epic child: never anchor a plan on origin/main](epic-child-plan-must-not-anchor-on-origin-main.md) — the merge base sits behind every merged sibling
- [Three-dot diff degenerates on an ancestor base](three-dot-diff-degenerates-on-ancestor-base.md) — bills sibling merges to your footprint
- [Merging main invalidates the plan's base anchor](merging-main-invalidates-plan-base-anchor.md) — re-anchor to the merge commit
- [Stale-figure sweep by changed-file set](stale-figure-sweep-by-changed-file-set.md) · [Verify reducibility before accepting an exemption count](feedback_verify_reducibility_before_accepting_exemption_count.md)
- [Piped command's `$?` is the LAST segment](piped-command-exit-code-is-the-last-segment.md) — suspect the measurement before overwriting a memory it contradicts

## Plans, preflight, delegation
- [Preflight catches vacuous gates](preflight-catches-vacuous-gates.md) · [converges on verbatim delta text](preflight-converges-on-verbatim-delta-text.md) · [may exceed the 2-round target](preflight-rounds-exceed-target-legitimately.md)
- [Preflight sibling-invalidation cascade](preflight-sibling-invalidation-cascade.md) · [sweep ordering + citation arity](preflight-sweep-task-ordering-and-citation-arity.md) · [defect-trend scope confound](preflight-defect-trend-scope-confound.md)
- [Absence from a failure list isn't a pass](absence-from-failure-list-is-not-a-pass-gate.md) — pair with a discovery-count control
- [Count gate broken by the plan's own new names](plan-token-count-gate-broken-by-own-mandated-identifiers.md) — unsatisfiable, not vacuous
- [Convergence signal is systematically optimistic](convergence-signal-is-systematically-optimistic.md) — budget 3 rounds
- [Multi-location fact residuals drive rounds](multi-location-fact-residuals-drive-preflight-rounds.md) · [Apply EVERY part of a multi-part delta](apply-every-part-of-a-multipart-delta.md)
- [atomic-planner has no MCP validator tool](atomic-planner-lacks-mcp-validator-tool.md) · [validator requires LF](mcp-plan-validator-requires-lf.md) · [em-dash is version-dependent](mcp-plan-validator-defective-em-dash.md) · [Edit/Write pervasive-diff](mcp-plan-validator-editwrite-pervasive-diff.md)
- [Select-String pattern quoting in plans](select-string-pattern-quoting-in-plans.md) — `\|` is a LITERAL pipe; use `\x5C` / `\x22`
- [expect-fail tests break substring scoped-run gates](expect-fail-tests-break-substring-scoped-run-gates.md) · [Revert plans must check test provenance](revert-plans-must-check-test-provenance.md)
- [One executor per worktree](one-executor-per-worktree.md) · [Agent() cannot course-correct a running subagent](agent-tool-cannot-course-correct-running-subagent.md)
- [No SendMessage tool: relaunch with a resume brief](no-sendmessage-relaunch-with-resume-brief.md) — never a placeholder prompt
- [Dead subagent's work may already be complete on disk](dead-subagent-work-may-be-complete-on-disk.md) — diff before relaunching
- [Remediation loop strict handoff](remediation-loop-strict-handoff.md) · [Remediation-plan em-dash required](remediation-plan-em-dash-required.md)
- [Get-BlastRadius over-includes citations, omits gitignored writes](get-blastradius-overincludes-citations-omits-gitignored-writes.md)
- [Model-routing hook reads the canonical path only](model-routing-hook-reads-canonical-path-only.md) · [use the portable PS modules](model-routing-scripts-absent-on-epic-integration-base.md) · [feature-review is fable only under `preferred`](model-routing-feature-review-is-always-fable.md)
- [Forward the planner's handoff records to preflight](forward-planner-handoff-records-to-preflight.md) — or they're reported missing
- [Reading-only preflight cannot clear a plan](preflight-without-build-access-cannot-clear-a-plan.md) — 7 rounds missed a missing NuGet restore
- [A session may have NO Agent tool](orchestrator-session-may-lack-agent-tool.md) — block, never implement the plan yourself
- [PowerShell batch budget caps plan helper scripts](powershell-batch-budget-caps-plan-helper-scripts.md) · [tracked + carries stale paths](powershell-batch-budget-is-tracked-and-carries-stale-paths.md) — put throwaway helpers in the session scratchpad
- [Get-PlanPaths truncates spaced paths](get-planpaths-truncates-paths-containing-spaces.md)

## Coverage
- [C# coverage has two denominators](csharp-coverage-denominator-two-figures.md) · [lines-covered is nondeterministic](coverage-lines-covered-is-nondeterministic.md) · [#457 coverage moved UP](project_457_coverage_moved_up_not_down.md)
- [Raw Cobertura branch-rate is 13pts low](branch-coverage-denominator-trap-raw-cobertura.md) — line-only checks pass while branch sits below the 75% floor
- [feature-review 85% floor trap](feature-review-coverage-85-floor-trap.md) · [JaCoCo not Cobertura for evidence](jacoco-not-cobertura-for-evidence.md) · [PoshQC drops coverage.xml at repo root](poshqc-test-drops-coverage-xml-at-repo-root.md)
- [Repo-wide coverage: run the FULL suite](feedback_repowide_coverage_run_full_suite.md) · [authority exception](feedback_repowide_coverage_authority_exception.md) · [No exemption when the purpose is testability](feedback_no_coverage_exemption_when_purpose_is_testability.md)
- [Convert Cobertura to JaCoCo BEFORE commit](cobertura-substitution-must-happen-precommit.md) — intercept before the executor's commit
- [Coverage mode raw-vs-processed is flake-sensitive](coverage-mode-raw-vs-processed-is-flake-sensitive.md) — re-measure in a detached worktree
- [Post-processed Cobertura = zero exit, NOT a test result](cobertura-postprocessing-is-a-zero-exit-proxy-not-a-test-result.md) — no .trx exists; re-run the gate
- [[ExcludeFromCodeCoverage] is INVISIBLE, not 0%](excludefromcodecoverage-invisible-to-coverage-gates.md) — per-file hits-row gates unsatisfiable; METHOD-level leaks
- [vstest emits TWO .coverage files per run](vstest-emits-two-coverage-files-per-run.md) — need a disambiguation rule

## C# toolchain and tests
- [C# agent worktree needs three bootstrap steps](csharp-agent-worktree-needs-three-bootstrap-steps.md) · [analyzer packages.config quirks](csharp-analyzer-packages-config-quirks.md) · [direct-csproj build facts](csharp-direct-csproj-build-facts.md)
- [Analyzer gate is vacuous without /t:Rebuild](msbuild-analyzer-gate-vacuous-without-rebuild.md) · [which non-vacuity pattern to count](msbuild-non-vacuity-which-pattern-to-count.md) · [a successful msbuild prints "error" 35 times](msbuild-success-output-contains-error.md)
- [`suggestion` diagnostics never reach the msbuild log](suggestion-severity-diagnostics-invisible-to-msbuild.md) — certify a SARIF channel with a control
- [MSB3021-only failure = testhost lock](msbuild-msb3021-only-means-test-host-lock.md) · [net48 nominal record DOES compile](net48-nominal-record-compiles-only-init-fails.md) — only `init` fails CS0518
- [Aggregate vstest crash: isolate per assembly](vstest-aggregate-crash-isolate-per-assembly.md) · [bare vstest omits the LiveOutlook filter](bare-vstest-omits-liveoutlook-filter.md)
- [Tests must not trigger UX or a live worker](feedback_tests_must_not_trigger_ux_or_live_worker.md) · [Banned API in a touched file is in scope](feedback_banned_api_in_touched_file_in_scope.md)
- [VSTO startup STA threading directive](feedback_vsto_startup_sta_threading_directive.md) · [STA controls ratified as last resort](feedback_sta_controls_last_resort_ratified.md)
- [WebView2 EndInit creates handles at construction](webview2-endinit-creates-handles-at-construction.md) · [Store-lockup watchdog null-model hazard](project_store_lockup_watchdog_null_model_hazard.md)

## Tooling quirks (Bash / pwsh / hooks)
- [NEVER `cd X && ...` or grep/sed/cat via Bash](feedback_no_cd_or_non_allowlisted_bash_segments.md) — only `git *`, `pwsh *`, `poetry run *` + 3 lib scripts; EVERY segment must match
- [Bash tool rejects complex commands in isolated worktrees](bash-tool-rejects-complex-commands-in-isolated-worktree.md) · [mangles MSBuild switches](bash-tool-mangles-msbuild-switches.md) · [collapses `\` before sed](bash-tool-collapses-double-backslash-in-sed.md)
- [Bash eats `$` in a double-quoted pwsh -Command](bash-expands-dollar-in-double-quoted-pwsh-command.md) — single-quote outside; import modules by ABSOLUTE path
- [pwsh double-quoted -Command is refused in a worktree](pwsh-double-quoted-command-refused-in-worktree.md) · [grep-count wrapper leaks $LASTEXITCODE](grep-count-wrapper-does-not-clear-lastexitcode.md) · [CR-pattern grep falsely reports 100% CRLF](grep-cr-empty-pattern-false-crlf.md)
- [Worktree isolation blocks pwsh — the SANDBOX, not the agent type](worktree-isolation-blocks-pwsh-per-agent-type.md) — launch execution WITHOUT isolation
- [Hooks pattern-match Bash command TEXT](hooks-pattern-match-bash-command-text.md) · [Promotion hook matches commit-message text](promotion-hook-matches-commit-message-text.md)
- [validate-bash blocks --force-with-lease too](validate-bash-blocks-force-with-lease-too.md) — delete-and-repush instead
- [Agent-worktree hooks resolve to agent cwd](agent-worktree-hooks-resolve-to-agent-cwd.md) · [Child-orchestrator PR hook reads SESSION ROOT](child-orchestrator-pr-hook-reads-session-root.md) · [Pre-impl gate reads a SIBLING's checkpoint](preimplementation-gate-reads-sibling-checkpoint.md)
- [feature-folder-order hook is work-mode-blind](feature-folder-order-hook-is-workmode-blind.md) · [PRD_FEATURE_BLOCKED false positive](prd-feature-hook-parses-prompt-paths.md) · [picks the LONGEST active path](prd-feature-hook-picks-longest-active-path.md) · [blocks reused prep-worktree topology](prd-feature-hook-blocks-reused-prep-worktree-topology.md)
- [Hard-lock MCP needs an absolute target](mcp-hardlock-and-review-mirror-quirks.md) · [check-ignore false negative on a directory glob](check-ignore-false-negative-on-directory-glob.md)
- [Edit tool CRLF-ifies LF markdown](edit-tool-crlf-ifies-lf-markdown.md) · [feature-review + `git -C` hangs forever](feature-review-git-c-form-hangs-unattended.md) — ban Bash there, paste the diff

## Artifact hygiene
- [Never embed absolute host paths](../_shared_no_absolute_host_paths.md) — a CONVENTION, not a codified rule · [Angle-bracket redaction breaks TRX XML](angle-bracket-redaction-breaks-trx-xml.md) · [.gitignore does NOT cover *.trx](gitignore-does-not-cover-trx.md)
- [No helper scripts under evidence/](feedback_no_helper_scripts_under_evidence.md) · [feature-review edits SHARED .git/info/exclude](feature-review-edits-shared-git-info-exclude.md)
- [Session-root shims are deleted by siblings](session-root-shims-are-deleted-by-siblings.md) · [Parent session can commit into your worktree](parent-session-can-commit-into-child-worktree.md)
- [Honor the user's per-cycle folder layout](feedback_verify_flat_artifact_layout_after_executor.md) · [Agent-worktree discovery + evidence hygiene](project_agent_worktree_discovery_and_evidence_hygiene.md)

## Epic and parallel orchestration
- [Epic children need full lifecycle + PRs](feedback_epic_children_require_full_lifecycle_and_prs.md) · [Epic child PRs get no CI](project_epic_child_prs_no_ci.md) · [PR-gate gotchas](epic-child-pr-gate-gotchas.md)
- [Epic self-merge step9 gate sequencing](epic-mode-pr-merge-gate-sequencing.md) · [step9 passed vs verified](epic-child-self-merge-step9-passed-vs-verified.md) · [Portable completion gate is FULL parity](portable-completion-gate-allows-blocked-child.md)
- [Epic-child stale local integration ref](project_epic_child_stale_local_integration_ref.md) · [plan Phase 0 paths are stale](feedback_plan_phase0_paths_are_stale_in_epic_children.md) · [Prepared child invalidated by a sibling merge](prepared-epic-child-invalidated-by-sibling-merge.md)
- [Epic manifest's ALL CLEAR table is not evidence](epic-manifest-all-clear-table-is-not-evidence.md) — grep the child folder; 1 round found 7 blocking defects incl. an inverted derivation
- [Epic-child rebase shared-memory conflict](epic-child-rebase-shared-memory-conflict.md) · [agent-memory merge conflicts](epic-child-agent-memory-merge-conflicts.md) · [Parallel children conflict on the memory index](parallel-epic-children-conflict-on-agent-memory-index.md)
- [Child cwd is the session root](preparation-child-cwd-is-session-root-not-item-worktree.md) — mirror the WHOLE folder; execution mode too
- [Resume brief's "already in your worktree" can be false](resume-brief-worktree-contents-premise-can-be-false.md) — Glob first; repair with `merge --ff-only`, which creates no branch
- [Unplanned epic-child worktree mechanics](unplanned-epic-child-worktree-mechanics.md) · [Parallel preparation children share one worktree](parallel-preparation-children-shared-worktree.md)
- [Parallel epic children name collisions](parallel-epic-children-name-collisions.md) · [generic-constraint cascades across children](epic-generic-constraint-cascades-multiple-children.md) · [Absolute-zero gate on a sibling-owned assembly](absolute-zero-gate-on-sibling-owned-assembly.md)
- [Epic-child nullable fan-in debt is deferred](project_epic_child_nullable_fanin_debt_deferred.md) · [Spec backticks widen the blast radius](spec-backticks-widen-blast-radius.md)
- [Epic #295 winforms testability](project_epic_295_winforms_testability.md) · [Swordfish: clean collection premise is false](project_swordfish_removal_false_clean_collection.md) · [F5 ScoDictionary blocker RESOLVED](project_swordfish_epic_f5_blocked_on_old_scodictionary.md)

## Repo layout
- [VS Code extension location](project_extension_location.md) · [Verify package.json before vsce work](feedback_vsce_verify_package_location.md) · [Repo root is source of truth for the codex bundle](feedback_repo_root_is_source_of_truth.md)
- [CLAUDE.md nullable command != CI gate — RESOLVED by #540](project_claudemd_nullable_command_diverges_from_ci.md)
- [System.IO relative paths escape the worktree](system-io-relative-paths-escape-the-worktree.md) - Set-Location does NOT move Environment.CurrentDirectory; bit 3x in one run, incl. a write into another worktree
- [new_active_feature_folder receipt under-reports artifacts](new-active-feature-folder-receipt-underreports-artifacts.md) - it scaffolds spec.md + plan.<ts>.md too; not a peer agent, and that plan file IS your canonical plan-path
- [prd-feature STOP hooks are work-mode-blind](prd-feature-stop-hooks-are-workmode-blind.md) - demands an existing user-story.md on a full-bug, and any digit in an AC forces 11 exact labels into the RESEARCH file
- [task-researcher filename regex is strict](task-researcher-filename-regex-is-strict.md) - the name must be <ts>-<slug>-research.md or SubagentStop blocks; the orchestrator suggested name can trap it
- [#751: 5-round preflight for novel infra](project_issue_751_five_round_preflight_detached_launch_convention.md) - budget more rounds when a plan invents execution infra
- [A delegate may have no Bash tool](delegate-may-lack-bash-tool-verify-its-git-claims.md) - it cannot verify its own git claims; check them yourself
- [Coverage seam workaround for .claude worktrees](coverage-seam-workaround-for-claude-worktrees.md) - dot-source TWO files, explicit -TestAssembly
- [isolation worktree spawn param kills the toolchain](isolation-worktree-spawn-param-kills-toolchain.md) - pwsh refused; isolation-dependent, NOT agent-type dependent
- [An analyzer control site can be UNCOMPILED](analyzer-control-site-can-be-uncompiled-not-just-commented.md) - legacy csproj have explicit Compile items, no wildcard; verify live code AND a Compile Include entry
- [Don't elect reviewer-declined optional changes](do-not-elect-reviewer-declined-optional-changes.md) - the substitute value was itself a defect; cost 2 rounds. Bar additive edits to keep a delta narrow
- [Byte-exact copy via git plumbing](byte-exact-copy-via-git-plumbing.md) - `hash-object -w` plus `cat-file blob >`; a SHA compare proves identity and keeps LF
- [An imported checkpoint's recorded pass is not evidence](imported-checkpoint-recorded-pass-is-not-evidence.md) - a predecessor's `model_routing_preflight: pass` re-validated as 22 errors
- [Self-anchor the diff base in epic children](self-anchor-the-diff-base-in-epic-children.md) - a base fixed by the parent degenerates once a sibling merges; derive it in the child at P0
- [Re-fetch integration before declaring prep done](refetch-integration-before-declaring-prep-done.md) — epic-planner publishes binding child directives mid-run; siblings share .git so origin refs move silently. Re-fetch, rebase, diff the manifest, absorb, re-preflight
- [Store-lockup watchdog null-model hazard](project_store_lockup_watchdog_null_model_hazard.md) — #260 watchdog is live; new startup COM attribution scopes need a responder phase-branch that returns before the disable-service write, or the watchdog thread crashes on the null store model
- [VS Code extension location](project_extension_location.md) — the extension lives at `extensions/drm-copilot/`, not at the repo root.
- [Verify package.json before vsce work](feedback_vsce_verify_package_location.md) — in multi-package repos, never assume the repo root is the publishable extension; locate it first.
- [Repo root is source of truth for codex bundle](feedback_repo_root_is_source_of_truth.md) — when repo `.codex/`, `.agents/`, `AGENTS.md` differ from bundled copies, update the bundle to match.
- [Evidence + lifecycle for every change](evidence-and-lifecycle-for-every-change.md) — evidence only under a feature folder; promote to issue + active folder before ANY implementation, even 1-file tooling fixes
- [Small-path = minor-audit selection](small-path-minor-audit-selection.md) — 1-3 production-file bug = small path + minor-audit, no spec.md, AC lives in issue.md
- [MCP tools available to orchestrator](mcp-tools-available-to-orchestrator.md) — if a worker reports MCP gate/lifecycle tools unavailable, run them from the orchestrator yourself, don't accept the block
- [potential_to_issue creates the GitHub issue](potential-to-issue-creates-github-issue.md) — the promotion tool opens the GitHub issue itself; do not also gh issue create
- [potential_to_issue needs an absolute path](potential-to-issue-needs-absolute-path.md) — in a worktree, workspace-relative potential_path fails "not found"; pass the absolute path from the new_potential_entry receipt
- [Promotion potential .md may not persist](promotion-potential-md-may-not-persist.md) — MCP promotion creates the issue + populates active issue.md, but the potential/promoted .md may be absent on disk; recreate for audit trail, don't treat as failure
- [Remediation loop strict handoff](remediation-loop-strict-handoff.md) — remediation cycles run atomic-planner -> atomic-executor -> feature-review only; no direct typed-engineer worker calls; five required artifacts per cycle
- [Remediation-plan em-dash required](remediation-plan-em-dash-required.md) — the plan validator rejects `### Phase N (continued) — <Title>`; only canonical `### Phase N — <Title>` passes
- [new_active_feature_folder date prefix](new-active-feature-folder-date-prefix.md) — standalone feature folders get the YYYY-MM-DD- prefix automatically; epic-child folders don't (git mv those)
- [orchestrator-state validator divergence](orchestrator-state-validator-divergence.md) — MCP orchestrator-state check is stricter than the real SubagentStop hook; conform to the canonical schema's remediation_loop shape
- [orchestrator-state flat keys + step-status enum](orchestrator-state-flat-keys-and-enum.md) — validator needs FLAT top-level variable keys (not nested under "variables") and enum step statuses (in_progress not in-progress); fable_policy "available" => C3->opus
- [C# analyzer packages.config quirks](csharp-analyzer-packages-config-quirks.md) — non-SDK analyzer wiring needs manual roslyn subfolder selection; SecurityCodeScan.VS2019 breaks Roslyn 5.6 via CS8032 (can't be silenced by editorconfig)
- [Whole-repo CI gate is not out-of-scope](whole-repo-ci-gate-not-out-of-scope.md) — a pre-existing repo-wide csharpier/lint failure blocks the PR's required check (AC6); fix it, don't defer it
- [Honor user's per-cycle folder layout](feedback_verify_flat_artifact_layout_after_executor.md) — #181 user committed a per-cycle folder layout (<ts>-remediation/, <ts>-audit/); follow it. Only revert UNDIRECTED agent relocations, not the user's own reorg
- [Repo-wide coverage authority exception](feedback_repowide_coverage_authority_exception.md) — sole blocking finding = pre-existing repo-wide coverage shortfall + change-scope gates pass → surface authority-scoped exception, don't auto-cycle
- [Repo-wide coverage: run the FULL suite](feedback_repowide_coverage_run_full_suite.md) — measure repo-wide C# coverage by running ALL *.Test.dll together (like ci.yml); a single-assembly run reports a false-low number (20.21% vs true 81.19%). Verify before treating as a blocker
- [No coverage exemption when purpose is testability](feedback_no_coverage_exemption_when_purpose_is_testability.md) — for testability-purpose work, maintainer denies [ExcludeFromCodeCoverage] ratification; plan real interface/adapter seams instead (contrast #223 which was ratified)
- [Verify reducibility before accepting exemption count](feedback_verify_reducibility_before_accepting_exemption_count.md) — don't trust a delivered residual count at face value; cross-check against proven in-repo techniques, and confirm de-exempted tests execute the delegate, not just verify it was called
- [Migration posture before PR gate](feedback_migration_not_just_patch.md) — before step 10, report integration/migration posture (reachable in prod? persisted? old path retired?), not just a clean audit
- [Verify repro before bugfix cycle](feedback_verify_repro_before_bugfix_cycle.md) — for a latent/worked-around defect, ground-truth its reachability on HEAD before a red-before-green cycle; a correct workaround can make it unreproducible
- [VSTO startup STA threading directive](feedback_vsto_startup_sta_threading_directive.md) — minimize STA reliance, always pump, gate COM hookups on Outlook readiness, offload only non-COM compute; residual in scope only if the add-in causes it
- [Banned API in touched file is in scope](feedback_banned_api_in_touched_file_in_scope.md) — when a fix modifies a production file, remediate any banned API (DateTime.Now/UtcNow, Random.Shared, Thread.Sleep, Task.Delay) found in it; don't defer
- [Re-verify ground truth after user mid-cycle commit](feedback_reverify_ground_truth_after_user_midcycle_commit.md) — if the user rebases/commits mid-cycle, re-probe line counts/merge-base/csproj and re-plan before executing a preflighted plan
- [pr_context.summary.txt unreliable for gh + file classification](pr-context-summary-unreliable-gh-and-classification.md) — verify gh with `which gh`/`gh auth status` and author the PR body from the real diff, not the summary's "gh unavailable"/"0 core-logic changes" lines
- [pr-author agent unavailable; run skill in-thread](pr-author-hook-blocks-gh-in-this-repo.md) — Agent(pr-author) type not registered here; author body+SHA256 receipt in-thread and the hook permits gh pr create; also carries the verified checkpoint schema + step-status enum
- [Tests must not trigger UX or a live worker](feedback_tests_must_not_trigger_ux_or_live_worker.md) — never start a real BackgroundWorker/form in unit tests (pops MessageBox, touches COM); seam the worker body and inject an inert delegate
- [Commit everything before the S9 CI gate](feedback_commit_before_ci_gate.md) — a post-gate commit (even docs/memory) moves the head SHA and forces an S9 re-run; finish all commits, then run S9 once
- [Git-blame regressions before novel hypotheses](feedback_gitblame_regressions_before_novel_hypothesis.md) — for a "was working, now wrong" symptom, git-blame the exact lines and diff the responsible refactor commit first; re-confirm the exact symptom; treat a failed fix's mechanism as refuted (#269 fore/back swap from #236 refactor)
- [Flaky CI: PhysicalFileInfoAdapter test](project_flaky_ci_physicalfileinfoadapter_test.md) — PhysicalFileInfoAdapter_..._MirrorFileInfo intermittently fails CI opening real TaskMaster.sln; re-run failed job first, fix only if deterministic
- [Commit review artifacts + step8 preflight nuances](feedback_commit_review_artifacts_and_step8_preflight.md) — commit feature-review's artifacts before rebase/PR; set step8_status non-pending for PR preflight; ff-only merge to update main in a linked worktree
- [Epic self-merge step9 gate sequencing](epic-mode-pr-merge-gate-sequencing.md) — merge gate needs top-level epic_mode:true + step9_status "passed"; completion gate rejects "passed" → flip to "verified" after merge, before Stop
- [feature-review 85% coverage floor trap](feature-review-coverage-85-floor-trap.md) — do NOT generate artifacts/csharp/coverage.xml at 80-85% repo-wide; hook hard-codes 85% and forces a false FAIL (supersedes the stale "generate coverage.xml" note)
- [MCP plan validator em-dash behavior is version-dependent](mcp-plan-validator-defective-em-dash.md) — PASSED em-dash+LF plans on 2026-07-10 (F1/F2/F5 preps; contradicts older "rejects em-dash" finding); run it and observe, keep em-dash, executor preflight is still the substantive gate
- [collect_pr_context lands in main checkout](collect-pr-context-lands-in-main-checkout.md) — in a worktree, copy pr_context.* into the worktree before gh pr create; checkpoint + pr_* artifacts are gitignored (local-only)
- [MCP plan validator Edit/Write pervasive-diff](mcp-plan-validator-editwrite-pervasive-diff.md) — validator rejects an LF/no-BOM plan after an Edit/Write with "no canonical phase headings"; restore via git-bash cp/sed/perl, fold oversized bullets, executor preflight is the real gate
- [MCP plan validator CRLF: now ACCEPTED](mcp-plan-validator-requires-lf.md) — SUPERSEDED 2026-08-07: validator now passes CRLF plans (every committed plan is CRLF here via `* text=auto`+autocrlf). Don't pre-normalize or treat post-checkout CRLF as a risk; run it and observe
- [Swordfish epic: clean collection premise is false](project_swordfish_removal_false_clean_collection.md) — manifest claims a clean ConcurrentObservableCollection "already in repo" but it doesn't exist; F4 used List<T>, F2 must actually create it
- [Epic child PRs get no CI](project_epic_child_prs_no_ci.md) — ci.yml triggers only on PRs to main/development; child→integration PRs run zero checks by design, so CI-green is vacuous and merge proceeds on blocking_count==0
- [Child-orchestrator PR hook reads SESSION ROOT](child-orchestrator-pr-hook-reads-session-root.md) — when session cwd != feature worktree, pr-author hook + collect_pr_context resolve against session root; stage body/receipt/checkpoint there and backup-swap-restore the epic checkpoint around gh pr create
- [Agent-worktree hooks resolve to agent cwd](agent-worktree-hooks-resolve-to-agent-cwd.md) — in a .claude/worktrees/agent-<id> isolated worktree, PreToolUse+SubagentStop hooks read the agent worktree (proven by merge succeeding); do NOT copy checkpoint to session root (clobbers siblings). Contrast the named-worktree case
- [Epic #295 winforms testability](project_epic_295_winforms_testability.md) — design-phase-only mandate (research→spec→plan→preflight, then STOP); children 293/296/297/298; 298 depends on 297
- [STA controls ratified as last resort](feedback_sta_controls_last_resort_ratified.md) — unshown WinForms controls on STA OK only after seams, in dedicated *.StaTests.cs files; Forms/popups/pumps still banned
- [Epic children need full lifecycle + PRs](feedback_epic_children_require_full_lifecycle_and_prs.md) — maintainer rejected executor-driver shortcut AND direct --no-ff child merges; child PRs mandatory even with vacuous CI
- [Verify subagent capability claims](feedback_verify_subagent_capability_claims.md) — never relay "agent type not registered" without checking .claude/agents frontmatter; demand verbatim error; blocked state, not silent fallback
- [Epic-child plan Phase 0 paths are stale](feedback_plan_phase0_paths_are_stale_in_epic_children.md) — epic-child plans cite the planning worktree's absolute paths for P0 policy reads; redirect the executor to the CURRENT worktree's files in the delegation prompt
- [Unplanned epic-child worktree mechanics](unplanned-epic-child-worktree-mechanics.md) — cross-worktree delegation works via absolute paths; atomic-executor runs C# tools via pwsh with explicit paths (vstest/csharpier not on PATH); collect_pr_context + PR/merge hooks resolve against session root
- [Epic generic-constraint cascades across children](epic-generic-constraint-cascades-multiple-children.md) — a ratified `where TKey : notnull` on a base type emits CS8714 in EVERY nullable-enabled unconstrained consumer; consumers span sibling children, so a one-file waiver estimate undercounts; enumerate ALL consumers first, child re-escalates rather than self-widening
- [Parallel epic children name collisions](parallel-epic-children-name-collisions.md) — siblings coin identical type names in shared namespaces; CS0101/CS0104 surface only at rebase; rename YOUR types, rerun toolchain, no re-review
- [Model-routing scripts absent on epic integration base](model-routing-scripts-absent-on-epic-integration-base.md) — on a PR-head-based epic integration branch the compute_complexity_floor/resolve_delegation_model/validate_orchestrator_state scripts may be missing; compute routing by hand, MCP validator still gates
- [Swordfish epic F5 ScoDictionary blocker (RESOLVED)](project_swordfish_epic_f5_blocked_on_old_scodictionary.md) — F5 (#308) once WI-0-halted on the OLD ScoDictionary Swordfish base (ScoDictionaryNew was a decoy); #315/PR #316 deleted it, F5 then completed. Lesson: grep the OLD class base + using, not just the *New replacement
- [Epic-child stale local integration ref](project_epic_child_stale_local_integration_ref.md) — local branch ref for the epic integration branch can be stale in an agent worktree; `git fetch` and branch from `origin/<branch>`, not the bare local name, before trusting a Phase-0 gate
- [Portable completion gate allows blocked child](portable-completion-gate-allows-blocked-child.md) — TaskMaster has no Python validator, so orchestrator SubagentStop uses the portable PS path (no require-complete); a blocked child can terminate + open PR by setting blocked_reason=none, steps 5-8 non-pending, halt in next_step/custom fields
- [Epic-child rebase shared-memory conflict](epic-child-rebase-shared-memory-conflict.md) — rebase child onto advanced integration tip; only conflict is shared agent-memory MEMORY.md index (union it); prove disjoint source + zero sibling-symbol refs => no rebuild; regenerate pr_context.summary.txt from real diff
- [Epic-child PR-gate gotchas](epic-child-pr-gate-gotchas.md) — isolated-worktree collect_pr_context writes to wrong checkout; hook reads nested epic_context.integration_branch; ci.yml only triggers on main/development so integration-base PRs merge on CLEAN
- [Parallel preparation children share one worktree](parallel-preparation-children-shared-worktree.md) — prep-mode epic children can run concurrently in ONE dir/index/checkpoint; use a child-scoped checkpoint path + pathspec-scoped commit; don't revert the sibling's canonical checkpoint write
- [Parallel epic children conflict on agent-memory index](parallel-epic-children-conflict-on-agent-memory-index.md) — late child PR conflicts ONLY on .claude/agent-memory/<agent>/MEMORY.md (each executor appends an index line); resolve by union, re-run build gate, PR flips to CLEAN
- [Epic-child self-merge: step9 passed vs verified](epic-child-self-merge-step9-passed-vs-verified.md) — enforce-epic-merge-gate.ps1 needs on-disk step9_status "passed"+epic_mode:true; MCP validator REJECTS the "passed" enum; keep "passed" for the merge, flip to "verified" after, revalidate
- [Epic-child agent-memory merge conflicts](epic-child-agent-memory-merge-conflicts.md) — parallel children's child->integration PR shows CONFLICTING solely on shared .claude/agent-memory/*/MEMORY.md index files; resolve by union, then re-verify post-merge before push
- [Prep-child checkpoint: hook paths + receipt shape](prep-child-checkpoint-hook-paths-and-receipt-shape.md) — SubagentStop + model-routing PreToolUse hooks hard-code artifacts/orchestration/orchestrator-state.json, so a child-scoped checkpoint alone blocks delegation AND termination; mirror it. delegation_receipts needs 7 keys per entry once non-empty
- [Scope exclusions must be complete in the prd-feature prompt](scope-exclusions-must-be-complete-in-prd-prompt.md) — omitting one deferred item made spec AC-7 assert a deletion the plan was forbidden to make; fix the requirement in preparation, never via a mid-execution spec-amendment task

## Additional entries

- [Verify brief constraints before propagating](feedback_verify_brief_constraints_before_propagating.md) — epic-child brief KEY CONSTRAINTS can be factually wrong (F11-consumer and WinForms claims both refuted in #436); have researchers confirm-or-refute, then correct issue.md + spec.md
- [Epic-child nullable fan-in debt is deferred](project_epic_child_nullable_fanin_debt_deferred.md) — per-child #nullable gate is scoped to the child's own branch; cross-child CS86xx fan-in accumulates on integration (tip already carried 15 pre-merge) and is the Wave-2 capstone's job — merge on blocking_count==0+CLEAN, don't over-remediate sibling files
- [Planner/executor lack the MCP validator](planner-executor-lack-mcp-validator.md) — atomic-planner and atomic-executor have file-tools only; run the plan + orchestrator-state validator gates from the orchestrator thread yourself
- [Subagent limit is session-wide](subagent-limit-shared-across-epic-children.md) — parallel epic children compete for the 20-agent pool; keep fan-out to 2-4, relaunch after rejection, and a held slot proves your agent is alive not dead


- [Parallel prep children saturate the subagent cap](parallel-prep-children-subagent-saturation.md) — 8 concurrent epic prep children exhaust the 20-subagent limit; wait and retry, never do the delegated step in-thread
- [Prep-child upstream dependency must be non-halting](prep-child-upstream-dependency-must-be-nonhalting.md) — tell planner AND preflight that the concurrently-prepared wave-0 artifact is an execution-time read; otherwise the child halts at WI-0 on a legitimately-absent file
- [Preparation-mode plans need repo-relative paths](preparation-mode-plans-need-repo-relative-paths.md) — prep-mode child plans execute later in a DIFFERENT worktree; forbid absolute paths up front, and tell preflight that upstream outputs are legitimately absent
- [Preflight finds forward-referencing acceptances](preflight-forward-referencing-acceptances.md) — budget 3 preflight rounds; name "no acceptance may reference state a later task establishes" explicitly; MCP validator passes right through it


- [Preflight catches what the plan validator cannot](preflight-catches-what-the-plan-validator-cannot.md) — MCP validator passed 3x on a plan with 8 blocking execution defects; budget 2-3 preflight cycles above ~100 tasks, and check the six recurring defect classes
- [Cobertura line-rate attribute is wrong](cobertura-line-rate-attribute-is-wrong.md) — #441 + #478 both corrupt it; recompute from deduplicated class-level `<line>` nodes. Per-file attribution DOES survive partial splits; #424's raw-vs-post-processed baseline is a like-for-like trap
