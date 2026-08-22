# Epic Kickoff: quickfiler-suite-determinism-foundation

Planned by epic-planner on 2026-08-22T02-40. All four child features are prepared: active folders created against pre-existing open issues, research complete, spec written, atomic plans approved and validator-clean, preflight ALL CLEAR verified against each child's own on-disk checkpoint. Planning state: artifacts/orchestration/epic-planner-state.json (branch: epic/quickfiler-suite-determinism-foundation-integration).

## Invocation Prompt

Run `/epic-run quickfiler-suite-determinism-foundation` to execute this epic, or paste the prompt below.

Use the epic-orchestrator subagent to execute the prepared epic at docs/features/epics/quickfiler-suite-determinism-foundation/epic.md. The integration branch epic/quickfiler-suite-determinism-foundation-integration already contains every prepared feature folder and approved atomic plan; child features resume at atomic execution from their committed plan-path rather than re-planning. Execute per the epic-orchestrate skill: wave-scheduled child orchestrator runs in isolated worktrees, merge-on-green fan-in to the integration branch, and the final integration-to-main PR.

## Feature Summary

| issue_num | feature_folder | wave | complexity | plan-path |
| --- | --- | --- | --- | --- |
| 445 | 2026-08-07-quickfiler-keyboard-action-contract-defects-445 | 0 | C2 | docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/plan.2026-08-21T18-09.md |
| 449 | 2026-08-07-quickfiler-explorer-controller-latent-defects-449 | 0 | C3 | docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/plan.2026-08-21T18-09.md |
| 491 | 2026-08-07-quickfiler-test-form1-live-form-491 | 0 | C2 | docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/plan.2026-08-21T18-11.md |
| 511 | winformspumphost-suite-determinism-511 | 0 | C3 | docs/features/active/winformspumphost-suite-determinism-511/plan.2026-08-21T18-10.md |

## Additional Issues Closed

The 511 feature closes issue 571 as well as 511. Its spec reconciles the two, which are in tension rather than dependency order: 511's literal remedy would delete the tests 571 stabilizes. Plan task P6-T9 asserts a zero diff for both coverage-bearing files, so 511 cannot delete 571's coverage.

## Wave Structure

All four features are wave 0 with empty depends_on. The graph is intentionally edgeless: no child's fix changes a contract another child consumes. Ordering within the epic comes from a project-file region partition recorded in the manifest, not from dependency edges.

## Execution Notes

1. Every PreToolUse hook in this repository is currently inert. Each reads $toolInput.command while the payload nests the value at $toolInput.tool_input.command, so the property is always null and each hook returns permissionDecision allow. The epic wave barrier, the merge gate, and the worktree-removal gate provide no enforcement. Confirm every transition from git worktree list --porcelain, git branch, and gh pr view --json state,mergedAt,headRefOid.

2. vstest requires /InIsolation, together with /EnableCodeCoverage and /TestCaseFilter:"TestCategory!=LiveOutlook". Without it each assembly's app.config binding redirects are ignored and roughly 1,695 phantom failures appear with empty messages and sub-millisecond durations, surfacing as a Moq TypeInitializationException via System.Threading.Tasks.Extensions. That is a load failure, not a regression. Also exclude the .claude worktrees directory from recursive Test.dll discovery.

3. Line endings are not a gate. Three of the four plans are CRLF and one is LF, and all four pass the MCP plan validator. An earlier revision of this epic advised re-normalizing plans to LF before revalidating; that advice was based on an assumption since measured false. Re-normalization is harmless but unnecessary, and a validator failure at execution time should be diagnosed on its actual message.

4. No Python toolchain exists here. There is no scripts/dev_tools directory and no Poetry manifest, so any skill step naming poetry run python -m scripts.dev_tools is unrunnable by absence. Report it as such rather than fabricating a result or skipping silently. PowerShell equivalents live under .claude/lib.

5. Do not edit anything under .claude except .claude/agent-memory. The rest is push-down-owned and a sync overwrites it with no merge.

6. All four child issues are already open. A child must call only new_active_feature_folder; potential_to_issue has no idempotent path and would file duplicates.

7. A live repo-wide analyzer defect is in scope for 511's plan task P6-T20, which files it as its own issue during execution. All 16 first-party csproj files reference Meziantou.Analyzer 3.0.156 and Roslynator.Analyzers 4.16.0 in unconditional Analyzer Include items while packages.config pins 3.0.174 and 4.16.1. A cold CI cache or a fresh clone fails the analyzer build with error CS0006. The 511 plan works around it locally via P0-T10, which edits no tracked file.

## Preparation Provenance

Preparation ran twice. The first fan-out of four children was destroyed by a transient API 529 that killed all four plus at least three grandchild preflight validators; zero of four reached preflight clearance, and their uncommitted work was preserved to branches before relaunch. The second fan-out, scoped to preflight only, cleared all four. Across the two runs the preflight step found and corrected defects in every plan, including an undefined variable that would have made vstest run against zero assemblies and report zero failures, a workspace root pinned to a deleted worktree, and a deletion task whose literal line numbers had been invalidated by an earlier task in the same plan.
