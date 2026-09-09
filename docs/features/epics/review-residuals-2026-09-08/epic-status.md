# Epic Status: review-residuals-2026-09-08

Generated projection of `artifacts/orchestration/epic-orchestrator-state.json`. Do not hand-edit;
`epic-orchestrator` regenerates this file at epic kickoff, at every `merge_status` transition, at
every wave transition, and at final integration-PR completion. The checkpoint JSON is the durable,
machine-authoritative source; `epic.md` is the human-authored manifest and narrative.

- Last updated: 2026-09-09T16:22:54Z
- Integration branch: `epic/review-residuals-2026-09-08-integration` at `89bdfe06`
- Current wave: 0 (3 of 7 merged; 821 executing)
- Epic manifest: `docs/features/epics/review-residuals-2026-09-08/epic.md`
- Epic kickoff: `docs/features/epics/review-residuals-2026-09-08/epic-kickoff.md`
- Integration PR: not yet opened

## Feature Status

| issue_num | feature_folder | wave | merge_status | pr_url | merge_commit_sha | worktree_created_at | pr_opened_at | merge_confirmed_at | worktree_removed_at |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 813 | `2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813` | 0 | merged | [#827](https://github.com/drmoisan/TaskMaster/pull/827) | `48cd004b` | 2026-09-09T13:46:00Z | 2026-09-09T14:30:00Z | 2026-09-09T14:34:01Z | — |
| 815 | `2026-09-08-coverage-aggregation-double-counts-method-rows-815` | 0 | merged | [#829](https://github.com/drmoisan/TaskMaster/pull/829) | `732b84d3` | 2026-09-09T13:46:00Z | 2026-09-09T15:30:00Z | 2026-09-09T15:34:59Z | — |
| 817 | `2026-09-08-utilitiescs-test-hygiene-residuals-817` | 0 | merged | [#830](https://github.com/drmoisan/TaskMaster/pull/830) | `89bdfe06` | 2026-09-09T13:46:00Z | 2026-09-09T16:17:00Z | 2026-09-09T16:21:05Z | — |
| 821 | `2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821` | 0 | worktree_created (executing) | — | — | 2026-09-09T13:46:00Z | — | — | — |
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

## CI Gating

`.github/workflows/ci.yml` triggers `pull_request` only on branches `[main, development]`. Every
child pull request in this epic is based on the integration branch, so it receives **zero** CI runs
and merge-on-green would silently become merge-on-nothing. Two compensations are in force, and no
workflow file is edited to obtain a trigger:

- Each child treats an absence of checks as an absence of evidence rather than as green, records the
  zero-run fact as its `ci_gate` evidence, and proves its own tree with the full four-step C#
  toolchain locally before merging.
- The epic parent dispatches `gh workflow run ci.yml --ref epic/review-residuals-2026-09-08-integration`
  after each fan-in, because a per-child run on a head that predates a sibling's merge never gates
  the integrated tree.

| run | after merge of | head_sha | conclusion |
| --- | --- | --- | --- |
| [34364775706](https://github.com/drmoisan/TaskMaster/actions/runs/34364775706) | 813 | `48cd004b` | success |
| [34371580397](https://github.com/drmoisan/TaskMaster/actions/runs/34371580397) | 815 | `732b84d3` | success |
| [34376281522](https://github.com/drmoisan/TaskMaster/actions/runs/34376281522) | 817 | `89bdfe06` | pending |

## Deferred Worktree Removals

`enforce-parallel-worktree-removal-gate.ps1` demands a matching **parallel** checkpoint `items[]`
record. This is an epic run, which keeps its per-feature records in the epic checkpoint's
`features[]`, so the parallel gate has no jurisdiction here yet fails closed and denies every
removal. The epic-specific gate would have allowed each of these: the feature is merged and the
worktree is clean. Removal is deferred rather than forced, and no parallel checkpoint is fabricated
to satisfy a gate that does not govern this run. Leftover worktrees are listed at epic completion
for reclamation via `scripts/bash/cleanup-worktrees.sh`.

| issue_num | worktree_path | first denied at |
| --- | --- | --- |
| 813 | `C:/Users/DanMoisan/repos/TaskMaster-wt/rr0908-813` | 2026-09-09T14:37:00Z |
| 815 | `C:/Users/DanMoisan/repos/TaskMaster-wt/rr0908-815` | 2026-09-09T15:40:00Z |
| 817 | `C:/Users/DanMoisan/repos/TaskMaster-wt/rr0908-817` | pending retry |

## Preparation Provenance

All eight features were prepared by `epic-planner` and committed to the integration branch: issue,
active feature folder, research, `spec.md`, an approved atomic plan, and a recorded
`PREFLIGHT: ALL CLEAR` verified against each child's own checkpoint on disk. `user-story.md` is
absent for every feature, which is correct for `full-bug` work mode. Children resume at atomic
execution from their committed `plan-path` and do not re-run promotion, research, or planning.
