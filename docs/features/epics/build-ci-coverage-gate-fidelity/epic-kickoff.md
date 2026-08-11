# Epic Kickoff: build-ci-coverage-gate-fidelity

Planned by epic-planner on 2026-08-11T02-10. All child features are prepared: active folders created against pre-existing issues, research complete, spec/user-story written, atomic plans approved, preflight ALL CLEAR. Planning state: artifacts/orchestration/epic-planner-state.json (branch: epic/build-ci-coverage-gate-fidelity-integration).

## Invocation Prompt

Run `/epic-run build-ci-coverage-gate-fidelity` to execute this epic, or paste the prompt below.

Use the epic-orchestrator subagent to execute the prepared epic at docs/features/epics/build-ci-coverage-gate-fidelity/epic.md. The integration branch epic/build-ci-coverage-gate-fidelity-integration already contains every prepared feature folder and approved atomic plan; child features resume at atomic execution from their committed plan-path rather than re-planning. Execute per the epic-orchestrate skill: wave-scheduled child orchestrator runs in isolated worktrees, merge-on-green fan-in to the integration branch, and the final integration-to-main PR.

Nine issues, five features, three waves. The critical path is 441 to 457 to 494.

## Feature Summary

| issue_num | feature_folder | wave | complexity | plan-path |
| --- | --- | --- | --- | --- |
| 441 | 2026-08-10-cobertura-coverage-arithmetic-441 | 0 | C3 | docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/plan.2026-08-10T14-07.md |
| 512 | 2026-08-10-csharp-toolchain-gate-fidelity-512 | 0 | C3 | docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/plan.2026-08-10T14-08.md |
| 394 | 2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394 | 0 | C1 | docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/plan.2026-08-10T14-09.md |
| 457 | 2026-08-10-excludefromcodecoverage-nested-lambdas-457 | 1 | C3 | docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/plan.2026-08-10T14-08.md |
| 494 | 2026-08-10-coverage-threshold-policy-reconciliation-494 | 2 | C3 | docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/plan.2026-08-10T14-10.md |

## Additional Issues Closed Per Feature

The manifest is keyed by one primary `issue_num` per feature, but three features close more than one issue because the fixes edit the same expressions or the same documentation sites:

- Feature 441 also closes 478 (both correct the rate recomputation inside `Merge-CoberturaClassesByFilename`).
- Feature 512 also closes 492, 509 and 522 (all four edit overlapping lines of the same three governance files).

## Execution Authorization Required

Two child features must edit documents that the `policy-compliance-order` skill places under a hard constraint ("Do NOT modify policy documents under `.claude/rules/`"):

- `csharp-toolchain-gate-fidelity-512` edits `CLAUDE.md`, `.claude/rules/csharp.md`, and `.claude/skills/csharp-qa-gate/SKILL.md`.
- `coverage-threshold-policy-reconciliation-494` edits `CLAUDE.md` § UT2, `.claude/rules/general-unit-test.md`, and `.claude/rules/quality-tiers.md`.

These edits are the substance of issues 494, 509 and 522 — the defect *is* that the governance documents are wrong. **Running this epic constitutes the authorization to apply them.** Each child's plan is scoped to the specific sites its issues enumerate, the two children own disjoint regions of `CLAUDE.md` (the toolchain command block versus § UT2, verified 84 lines apart), and no policy requirement may be relaxed, weakened, or deleted to make a gate pass. Preparation modified no governance file; verified by `git status -- CLAUDE.md .claude/rules` returning empty on each child branch.

## Notes for epic-orchestrator

1. **Re-normalize each plan to LF before running the plan validator.** The plans are committed as LF, but `core.autocrlf=true` materializes them as CRLF in a fresh worktree, and the MCP plan validator rejects that. Every one of the five preparations hit this. It is a line-ending artifact of the checkout, not a defect in the plan.
2. **Feature 494's `P0-T3` probes `potential_to_issue` availability, and the atomic-executor session does not expose that tool.** Expect the `PROMOTION MCP UNAVAILABLE` branch to be live: the FU-A and FU-C follow-up promotions fall to `epic-orchestrator` after execution, and AC10 stays unchecked until then.
3. **Feature 441 carries three non-blocking advisories** in its child checkpoint. The main one is that `P7-T20` does not itself verify AC-16's narrative-`Timestamp` clause; it is satisfiable, but the executor must add the field if missing.
4. **A model-routing hook conflicts with concurrent child isolation.** `.claude/hooks/enforce-model-routing-receipt.ps1` hardcodes `artifacts/orchestration/orchestrator-state.json` and cannot see a child-scoped checkpoint, so it denies every delegation when only a child-scoped file exists. Concurrent children currently have to mirror the file inside their own isolated worktree. Worth its own issue.
5. **Coordinate merge order with epic #136.** Feature 441 changes every coverage figure in the repository, which invalidates the committed baselines that the 21 unmerged branches of the QuickFiler per-file coverage epic gate on. No file conflicts with this epic, but decide before landing either on `main` whether #136 merges first or re-baselines afterward.

## Scope Exclusion

**Issue 513 is not in this epic and cannot be fixed from this repository.** The misclassifying code is `extensions/drm-copilot/src/lib/pr-context/collector-output.ts` in the separate `drm-copilot` governance repository; TaskMaster consumes `collect_pr_context` as an installed MCP extension and contains only two hook files that invoke it. It must be filed and fixed upstream.

## Preparation Quality Note

Across the five features, the atomic-executor preflight gate rejected more than 60 defects over 16 iterations, and the recurring class was a gate that cannot fail or cannot pass: an `[expect-fail]` proof resting on a Pester exit code that is provably 0 on failure; fixtures that would throw under `Set-StrictMode` while the failure-count assertion still passed; a branch-coverage figure Pester cannot produce; a finding table required to match a baseline the same plan perturbs. That is the same defect class this epic exists to remove, found inside the epic's own plans. Two earlier runs reported clearance that no artifact could corroborate; re-running the gate rather than trusting those reports is what surfaced 13 of those defects.
