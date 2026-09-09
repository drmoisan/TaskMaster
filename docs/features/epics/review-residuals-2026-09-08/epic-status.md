# Epic Status: review-residuals-2026-09-08

Generated projection of `artifacts/orchestration/epic-orchestrator-state.json`. Do not hand-edit;
`epic-orchestrator` regenerates this file at epic kickoff, at every `merge_status` transition, at
every wave transition, and at final integration-PR completion. The checkpoint JSON is the durable,
machine-authoritative source; `epic.md` is the human-authored manifest and narrative.

- Last updated: 2026-09-09T13:46:46Z
- Integration branch: `epic/review-residuals-2026-09-08-integration` at `e708cf02`
- Current wave: 0
- Epic manifest: `docs/features/epics/review-residuals-2026-09-08/epic.md`
- Epic kickoff: `docs/features/epics/review-residuals-2026-09-08/epic-kickoff.md`
- Integration PR: not yet opened

## Feature Status

| issue_num | feature_folder | wave | merge_status | pr_url | merge_commit_sha | worktree_created_at | pr_opened_at | merge_confirmed_at | worktree_removed_at |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 813 | `2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813` | 0 | worktree_created | — | — | 2026-09-09T13:46:00Z | — | — | — |
| 815 | `2026-09-08-coverage-aggregation-double-counts-method-rows-815` | 0 | worktree_created | — | — | 2026-09-09T13:46:00Z | — | — | — |
| 817 | `2026-09-08-utilitiescs-test-hygiene-residuals-817` | 0 | worktree_created | — | — | 2026-09-09T13:46:00Z | — | — | — |
| 821 | `2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821` | 0 | worktree_created | — | — | 2026-09-09T13:46:00Z | — | — | — |
| 823 | `2026-09-08-quickfiler-teardown-review-residuals-823` | 0 | worktree_created | — | — | 2026-09-09T13:46:00Z | — | — | — |
| 824 | `2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824` | 0 | worktree_created | — | — | 2026-09-09T13:46:00Z | — | — | — |
| 825 | `2026-09-08-etl-deadline-mechanics-follow-ups-825` | 0 | worktree_created | — | — | 2026-09-09T13:46:00Z | — | — | — |
| 826 | `2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826` | 1 | not_started | — | — | — | — | — | — |

## Wave Layering

Computed by longest-path layering over the `depends_on` edges in the manifest frontmatter.

- **Wave 0** (no dependencies): 813, 815, 817, 821, 823, 824, 825.
- **Wave 1**: 826, which depends on 825. Both edit `GetTableInViewAsync` in
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`. The edge is load-bearing at
  execution time, not merely declarative: 826's `[P1-T1]` re-measures `Console.WriteLine`
  reachability against the post-825 tree and records a `BRANCH:` value selecting between two
  branches its own task text authorizes.

## Execution Deviations

Three deviations from the `epic-orchestrate` skill defaults are in force for this run. Each is
recorded in full, with its authority, under `execution_deviations` in the epic checkpoint.

1. **Child worktree isolation is disabled.** A worktree-isolated agent's Bash calls to any non-git
   executable are refused by the isolation filter, and both `orchestrator` and `atomic-executor`
   are granted only `Bash(pwsh *)` with no PowerShell tool. Every plan in this epic is
   command-bearing, so an isolated child would fail at its first toolchain command on a false
   blocker. Each child is instead given an explicitly created worktree, addressed by absolute path.
2. **Effective wave concurrency is one.** A non-isolated child inherits the session working
   directory, and both `enforce-model-routing-receipt.ps1` and `validate-orchestrator-output.ps1`
   resolve `artifacts/orchestration/orchestrator-state.json` as a bare cwd-relative literal and fail
   closed. Concurrent non-isolated children would share one copy of that checkpoint. Wave 0
   therefore runs one feature at a time; the wave barrier for wave 1 is unchanged.
3. **Execution branches carry an `-exec` suffix.** Each of the eight preparation branch names is
   still checked out in a framework-locked preparation worktree, so `git worktree add` cannot reuse
   them. Every preparation branch was verified to be fully contained in the integration branch
   before renaming, so no preparation work is stranded.

## Preparation Provenance

All eight features were prepared by `epic-planner` and committed to the integration branch: issue,
active feature folder, research, `spec.md`, an approved atomic plan, and a recorded
`PREFLIGHT: ALL CLEAR` verified against each child's own checkpoint on disk. `user-story.md` is
absent for every feature, which is correct for `full-bug` work mode. Children resume at atomic
execution from their committed `plan-path` and do not re-run promotion, research, or planning.
