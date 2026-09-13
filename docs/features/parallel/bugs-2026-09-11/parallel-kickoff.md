# Parallel Kickoff: bugs-2026-09-11

Planned by parallel-planner on 2026-09-13T04:01:56Z. All items are prepared: promoted, active folders created,
research complete, spec and user-story written, atomic plans approved, preflight ALL CLEAR, blast
radii declared and V1/V2-clear. Planning state:
artifacts/orchestration/parallel-planner-state.json (run branch: parallel/bugs-2026-09-11-plan).

Thirteen items, four generation-0 cohorts, eighteen conflict edges over seventy-eight pairs. The
five potential entries were promoted to issues 869, 870, 871, 872 and 873. Base commit 2405a829d.

## Invocation Prompt

Run `/parallel-run bugs-2026-09-11` to execute this run, or paste the prompt below.

Use the parallel-orchestrator subagent to execute the prepared run whose manifest is
docs/features/parallel/bugs-2026-09-11/parallel.md on the plan-home branch parallel/bugs-2026-09-11-plan. Each item
resumes at atomic execution from its committed plan-path on its own pushed feature branch rather
than re-planning, and each item opens its own pull request against main.

## Execution Constraints

Each of these was verified during preparation and several will silently break the run if ignored.

1. Launch every execution child NON-ISOLATED. Under Agent-tool worktree isolation the Bash tool
   refuses pwsh and every other opaque executable, and delegates get Bash disabled outright. Six
   independent children confirmed this. parallel-orchestrator must create each worktree itself with
   git worktree add and pass the path to a non-isolated child.
2. Grant msbuild and dotnet Bash permissions to the execution child. The atomic-executor agent
   definition grants neither, and every C# gate needs both.
3. Pass the absolute worktree root explicitly. git rev-parse --show-toplevel is not a usable
   fallback: from the session root it resolves to the session root, not the item worktree.
4. STAGGER every gate step that invokes msbuild or vstest. Item 743 exists because pump-hosted
   QuickFiler tests expire at PumpTimeoutMs under concurrent build load, so concurrent gates
   reproduce that defect inside the executors' own gates. This is a CPU constraint, independent of
   max_concurrency and of any model-quota throttle.
5. Close Outlook, never kill it, before any rebuild gate, or build output stays locked. Item 792's
   AC-U5 needs a live Outlook afterwards and is a MANUAL human verification.
6. Local vstest needs the .claude worktree exclusion, /InIsolation, and a TestCaseFilter excluding
   the four UtilitiesCS.Test shell-icon classes that stall the test host on this machine.
7. Per-item orchestrator checkpoints do NOT travel with the branches, because artifacts/ is
   gitignored. Each execution child must seed its own before its first git add.
8. Restore the Parallel mode: true marker for EXECUTION delegations only. Item 816 verified that
   including it during preparation makes the pre-implementation gate evaluate against the
   parallel-orchestrator checkpoint and deny with checkpoint-absent.

## Open Decisions

1. ORDERING. Item 602 conflicts with all twelve other items and is placed in cohort 0, so it
   executes FIRST. Its scope note requires it to land AFTER item 873, which delivers half of its
   acceptance criteria. compute_cohorts is Welsh-Powell and colours the highest-degree vertex first,
   so the most contended item is scheduled earliest. The intake handoff assumed the opposite. No
   permitted planner action can move it. Withdrawing 602 costs the run nothing in parallelism, since
   it occupies a cohort alone either way, and yields three cohorts instead of four.
2. TRX FILENAME LEAK. Items 743, 838, 871 and 872 pass /Logger:trx with no LogFileName= across
   seventeen command spans, so vstest emits a filename carrying the account and host tokens. Item
   816 is the only item that authored LogFileName correctly. The fix is mechanical and
   acceptance-condition-neutral. Relying on item 602 to sweep the leak afterwards does not work,
   because 602 currently runs first.

## Item Summary

| issue_num | feature_folder | cohort | complexity | branch | plan-path |
| --- | --- | --- | --- | --- | --- |
| 602 | docs/features/active/2026-09-12-host-identifier-leakage-sweep-602 | 0 | C3 | bug/host-identifier-leakage-sweep-602 | docs/features/active/2026-09-12-host-identifier-leakage-sweep-602/plan.2026-09-12T16-14.md |
| 583 | docs/features/active/kastringasync-keyequals-contains-offset-583 | 1 | C2 | bug/kastringasync-keyequals-contains-offset-583 | docs/features/active/kastringasync-keyequals-contains-offset-583/plan.2026-09-12T10-25.md |
| 743 | docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743 | 1 | C4 | bug/quickfiler-itemviewer-ui-marshalling-seam-743 | docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/plan.2026-09-12T13-23.md |
| 838 | docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838 | 1 | C3 | bug/gettableinviewasync-returns-null-on-timeout-838 | docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md |
| 839 | docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839 | 1 | C3 | bug/createcancellationtoken-has-no-production-caller-839 | docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/plan.2026-09-12T22-14.md |
| 871 | docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871 | 1 | C3 | bug/qfcqueue-enqueue-path-lacks-injectable-seams-871 | docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md |
| 872 | docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872 | 1 | C3 | bug/minor-audit-trio-gate-cts-tracker-872 | docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/plan.2026-09-12T10-26.md |
| 873 | docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873 | 1 | C3 | bug/test-evidence-projection-convention-and-identity-leak-tooling-873 | docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/plan.2026-09-12T10-26.md |
| 742 | docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742 | 2 | C2 | bug/quickfiler-date-time-format-missing-invariant-culture-742 | docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/plan.2026-09-12T16-09.md |
| 816 | docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816 | 2 | C3 | bug/uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816 | docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/plan.2026-09-12T13-23.md |
| 869 | docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869 | 2 | C3 | bug/ci-coverage-threshold-and-pester-gates-869 | docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/plan.2026-09-12T10-25.md |
| 870 | docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870 | 2 | C2 | bug/claude-md-coverage-thresholds-and-toolchain-command-corrections-870 | docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/plan.2026-09-12T10-25.md |
| 792 | docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792 | 3 | C4 | bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792 | docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/plan.2026-09-12T13-21.md |

## Integrity

planning_commit: b1a40f80c612b97e1a566834ad90e881b1ac848d

| plan-path | plan-hash |
| --- | --- |
| docs/features/active/kastringasync-keyequals-contains-offset-583/plan.2026-09-12T10-25.md | d498baedd72de17a1945205b6b3195ce80e55e9e |
| docs/features/active/2026-09-12-host-identifier-leakage-sweep-602/plan.2026-09-12T16-14.md | c12bbc2e8b9116edfdca507106a6a229ac74ca29 |
| docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/plan.2026-09-12T16-09.md | 1839f6490d84475a228862667869a8071724a07b |
| docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/plan.2026-09-12T13-23.md | 6a43290d55dc92ea315a4b018cff18cf9c6507b9 |
| docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/plan.2026-09-12T13-21.md | 66f5de8cad9d18959ed09cded7b7ccf9227cce5e |
| docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/plan.2026-09-12T13-23.md | 3a4998656ef92e1d44ac2115636a2bacc14a427e |
| docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md | 9f6d30eb5090542925648470ff675c695b9b6108 |
| docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/plan.2026-09-12T22-14.md | b0de08b701d7ad0e84ac1c8cd8c0ab2352ab44f4 |
| docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/plan.2026-09-12T10-25.md | 2e5acc065f3e90de609af0d973bacb4b160c87ed |
| docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/plan.2026-09-12T10-25.md | 8f776743f1b09931525aee87a5f4536b0b29ff98 |
| docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md | ebc428fee89f488992ae003b3860e2fde46d0e3c |
| docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/plan.2026-09-12T10-26.md | cb19b03f5711f63840abb0b717a895766c49c44a |
| docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/plan.2026-09-12T10-26.md | 7f54632d44aa1dbc8c9b7d57862932f7ea00dfc0 |
