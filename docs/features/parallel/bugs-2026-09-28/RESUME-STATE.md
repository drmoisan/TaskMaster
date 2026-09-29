# Resume State: bugs-2026-09-28

SUPERSEDED 2026-09-29T03:55Z: the hold ended, all six items reached PREFLIGHT: ALL CLEAR, and the run
is planned. The authoritative artifacts are parallel.md and parallel-kickoff.md in this folder. The
content below is kept only as the record of the state at the hold.

Written by parallel-planner at 2026-09-29T01:20Z under the coordinator's commit-and-hold order
(forecast: all accounts exhausted about 01:40Z, no usable account until about 02:20Z). This file is
the durable copy of the planner checkpoint, which lives in the gitignored
artifacts/orchestration/parallel-planner-state.json and does not survive a lost session.

Run parameters: mode open, max_concurrency 6 (coordinator ruling), base origin/main
177b6d78e1b2408e5aedbd794cef3aad6b7fb372, plan-home branch parallel/bugs-2026-09-28-plan,
model_budget.fable_policy preferred. Issues 932 (upstream drm-copilot tracker) and 933 (refactor)
are deliberately excluded.

## Item state at the hold

| issue | work mode | branch | pushed head | preparation state |
| --- | --- | --- | --- | --- |
| 928 | minor-audit | bug/coverage-runner-scoped-threshold-and-format-928 | 4aa748fcd | preflight ALL CLEAR (4 rounds); child terminated; radius not yet derived |
| 929 | minor-audit | bug/package-manifest-consistency-residuals-929 | 62448256a | child running; plan authored, preflight in progress |
| 930 | minor-audit | bug/csharp-latent-hazards-uithread-ilglobals-comments-930 | 31ce569cc | child running; plan authored, preflight in progress |
| 931 | full-bug | bug/tests-depend-on-uncontrolled-environment-931 | 74ab72cdd | child running; spec committed, planning in progress |
| 882 | full-bug | bug/quickfiler-transactiongate-permit-leak-unexcluded-882 | 055bbec84 | child running; local merge of origin/main (98bbf3545) NOT pushed; spec and plan revisions uncommitted |
| 927 | full-bug | bug/evidence-and-identity-hygiene-sweep-927 | 177b6d78e (no item commit) | child running; feature folder exists only as untracked files in its worktree |

Plan paths known so far: 928
docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/plan.2026-09-28T19-45.md;
882 docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/plan.2026-09-13T18-24.md.

## Uncommitted work at the hold (child worktrees under .claude/worktrees)

- agent-a02e6a7cf1d674eb9 (927): untracked feature folder docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927, staged promoted record, agent-memory edits. Highest loss risk.
- agent-a78053755ec29e605 (882): unpushed merge commit 98bbf3545, modified spec.md and plan, agent-memory edits.
- agent-a36f285b19663c25b (931), agent-a74dcedbc13b789fd (929), agent-ab7f72be619adf22f (930), agent-afae372769aa6f4ed (928): agent-memory edits only.

The planner could not relay the commit-now order to running children: SendMessage is disabled in
this session. Children launched after the phase-boundary directive (929, 930, 931) commit at every
phase boundary; 882 and 927 were launched earlier and commit only at their terminal step.

## Relaunch recipe after the resume notice

1. For each item whose child terminated before reporting, first check liveness separately (worktree
   lock plus HEAD and index mtimes); an unchanged ref is not proof of death.
2. Relaunch a preparation-mode orchestrator for the item with the same kickoff line, pointing it at
   the surviving worktree's absolute path so it can Read and reuse uncommitted research, spec and
   plan text, re-verifying every citation. Require plain fast-forward pushes only.
3. When all six are preflight-clear: derive declared radii (hand-append scripts/vscode paths for
   928, the space-bearing ILGlobals path for 930, the app.config path for 929, per-folder globs for
   927), validate V1-V3, seed cohorts, run the parity check, write the manifest and kickoff.
