# Epic Kickoff: review-residuals-2026-09-08

Planned by epic-planner on 2026-09-09T04-30. All child features are prepared: issues promoted,
active folders created, research complete, spec written, atomic plans approved, preflight
ALL CLEAR. Planning state: artifacts/orchestration/epic-planner-state.json (branch:
epic/review-residuals-2026-09-08-integration).

Preparation was completed across two sessions. The first session was killed by infrastructure with
all eight preparation children in flight; the second re-derived state from disk, merged the work
that had been committed, and relaunched eight children that imported their predecessors' partial
work verbatim rather than regenerating it. No feature was re-planned from scratch and no promotion
tool was re-run, so no duplicate issue was filed.

## Invocation Prompt

Run `/epic-run review-residuals-2026-09-08` to execute this epic, or paste the prompt below.

Use the epic-orchestrator subagent to execute the prepared epic at
docs/features/epics/review-residuals-2026-09-08/epic.md. The integration branch
epic/review-residuals-2026-09-08-integration already contains every prepared feature folder and
approved atomic plan; child features resume at atomic execution from their committed plan-path
rather than re-planning. Execute per the epic-orchestrate skill: wave-scheduled child orchestrator
runs, merge-on-green fan-in to the integration branch, and the final integration-to-main PR.

Launch every child atomic execution WITHOUT worktree isolation. pwsh is refused for
worktree-isolated agents in this environment and every plan in this epic is command-bearing, so an
isolated executor fails at its first command-bearing task on a false blocker. Feature 815 alone has
fifteen tasks that invoke pwsh, and feature 823 records this as constraint EH1. Give each child its
own working directory another way; do not restore isolation to obtain one.

Wave 0 is issues 813, 815, 817, 821, 823, 824 and 825. Wave 1 is issue 826 alone, which depends on
825 because both edit GetTableInViewAsync in
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs. That edge is load-bearing at
execution time, not merely declarative: feature 826's [P1-T1] re-measures Console.WriteLine
reachability against the post-825 tree and records a BRANCH: value selecting between two branches
its own task text authorizes, so 826 must not start before 825 has merged.

Read the Execution Preconditions and Follow-Ups To File After This Epic Merges sections of the epic
manifest before scheduling wave 0. They carry four further measured constraints, including a
contingency-path coverage-query mismatch in feature 821 and a known late-QC flake in feature 823
that halts safely and is recoverable by re-running.

## Verification Performed

Every feature's `PREFLIGHT: ALL CLEAR` was verified by reading the child's own
artifacts/orchestration/orchestrator-state.json on disk, not accepted from the child's summary.
Every fan-in was checked for deletions with a three-dot diff against the integration branch before
merging; all eight were additions-only. Every plan was confirmed to carry substantive phase headings
rather than the scaffold that new_active_feature_folder generates.

Preflight was not ceremony on this epic. It returned blocking defects on every feature, most of
which the MCP plan validator passed as ok, and the dominant class was acceptance conditions that
could never fail at execution: counts demanded from tool output that a successful run does not
print, a zero-match search defeated by pre-existing out-of-scope occurrences, an analyzer control
site that no project file compiles, and an unbounded QC restart loop keyed to a fixed baseline.

## Feature Summary

| issue_num | feature_folder | wave | complexity | plan-path |
| --- | --- | --- | --- | --- |
| 813 | 2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813 | 0 | C2 | docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/plan.2026-09-08T23-49.md |
| 815 | 2026-09-08-coverage-aggregation-double-counts-method-rows-815 | 0 | C3 | docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/plan.2026-09-08T23-49.md |
| 817 | 2026-09-08-utilitiescs-test-hygiene-residuals-817 | 0 | C2 | docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/plan.2026-09-08T23-50.md |
| 821 | 2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821 | 0 | C3 | docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/plan.2026-09-08T23-50.md |
| 823 | 2026-09-08-quickfiler-teardown-review-residuals-823 | 0 | C3 | docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/plan.2026-09-08T23-50.md |
| 824 | 2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824 | 0 | C3 | docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/plan.2026-09-08T23-51.md |
| 825 | 2026-09-08-etl-deadline-mechanics-follow-ups-825 | 0 | C3 | docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/plan.2026-09-08T23-51.md |
| 826 | 2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826 | 1 | C3 | docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/plan.2026-09-08T23-52.md |
