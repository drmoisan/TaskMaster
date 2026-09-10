# Epic Status: review-residuals-2026-09-08

Generated projection of `artifacts/orchestration/epic-orchestrator-state.json`. Do not hand-edit;
`epic-orchestrator` regenerates this file at epic kickoff, at every `merge_status` transition, at
every wave transition, and at final integration-PR completion. The checkpoint JSON is the durable,
machine-authoritative source; `epic.md` is the human-authored manifest and narrative.

- Last updated: 2026-09-10T00:20:00Z
- Integration branch: `epic/review-residuals-2026-09-08-integration` at `5eaf33dc`
- Current wave: 1 COMPLETE. All eight features merged.
- Integration-to-`main` pull request: pending the final integrated-tree CI gate
- Epic manifest: `docs/features/epics/review-residuals-2026-09-08/epic.md`
- Epic kickoff: `docs/features/epics/review-residuals-2026-09-08/epic-kickoff.md`
- Integration PR: not yet opened

## Feature Status

| issue_num | feature_folder | wave | merge_status | pr_url | merge_commit_sha | worktree_created_at | pr_opened_at | merge_confirmed_at | worktree_removed_at |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 813 | `2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813` | 0 | merged | [#827](https://github.com/drmoisan/TaskMaster/pull/827) | `48cd004b` | 2026-09-09T13:46:00Z | 2026-09-09T14:30:00Z | 2026-09-09T14:34:01Z | — |
| 815 | `2026-09-08-coverage-aggregation-double-counts-method-rows-815` | 0 | merged | [#829](https://github.com/drmoisan/TaskMaster/pull/829) | `732b84d3` | 2026-09-09T13:46:00Z | 2026-09-09T15:30:00Z | 2026-09-09T15:34:59Z | — |
| 817 | `2026-09-08-utilitiescs-test-hygiene-residuals-817` | 0 | merged | [#830](https://github.com/drmoisan/TaskMaster/pull/830) | `89bdfe06` | 2026-09-09T13:46:00Z | 2026-09-09T16:17:00Z | 2026-09-09T16:21:05Z | — |
| 821 | `2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821` | 0 | merged | [#831](https://github.com/drmoisan/TaskMaster/pull/831) | `d636b0f2` | 2026-09-09T13:46:00Z | 2026-09-09T17:33:00Z | 2026-09-09T17:37:20Z | — |
| 823 | `2026-09-08-quickfiler-teardown-review-residuals-823` | 0 | merged | [#832](https://github.com/drmoisan/TaskMaster/pull/832) | `553f874a` | 2026-09-09T13:46:00Z | 2026-09-09T18:43:00Z | 2026-09-09T18:47:53Z | — |
| 824 | `2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824` | 0 | merged | [#833](https://github.com/drmoisan/TaskMaster/pull/833) | `96fd3dd8` | 2026-09-09T13:46:00Z | 2026-09-09T20:20:00Z | 2026-09-09T20:24:46Z | — |
| 825 | `2026-09-08-etl-deadline-mechanics-follow-ups-825` | 0 | merged | [#834](https://github.com/drmoisan/TaskMaster/pull/834) | `049c1427` | 2026-09-09T13:46:00Z | 2026-09-09T21:50:00Z | 2026-09-09T22:02:23Z | — |
| 826 | `2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826` | 1 | merged | [#835](https://github.com/drmoisan/TaskMaster/pull/835) | `219804ef` | 2026-09-09T22:15:00Z | 2026-09-10T00:10:00Z | 2026-09-10T00:15:01Z | — |

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
| [34376281522](https://github.com/drmoisan/TaskMaster/actions/runs/34376281522) | 817 | `89bdfe06` | success |
| [34384355056](https://github.com/drmoisan/TaskMaster/actions/runs/34384355056) | 821 | `d636b0f2` | success |
| [34391748802](https://github.com/drmoisan/TaskMaster/actions/runs/34391748802) | 823 | `553f874a` | success |
| [34401152781](https://github.com/drmoisan/TaskMaster/actions/runs/34401152781) | 824 | `96fd3dd8` | success |

| [34410792972](https://github.com/drmoisan/TaskMaster/actions/runs/34410792972) | 825 | `aff8285d` | success |
| [34420677042](https://github.com/drmoisan/TaskMaster/actions/runs/34420677042) | 826 | `219804ef` | pending |

Six concluded integration runs, six successes. The seventh is the first to cover the complete
wave-0 tree with all seven features merged.

## Wave 1 Barrier

Wave 1 was not opened until dependency 825 was durably confirmed merged, verified from
`gh pr view 834` (state MERGED, merge commit `049c1427`, merged at 2026-09-09T22:02:23Z) and by
confirming that commit is an ancestor of the wave-1 worktree HEAD — not from a completion
notification. The MCP epic-checkpoint validation, which had reported an
`EPIC_WAVE_BARRIER_VIOLATION` line for 826 throughout wave 0 exactly as expected while its
dependency was unmerged, returned `ok: true` with no findings once 825 flipped to merged.

Feature 825's fourth gate, unresolved at merge time, was subsequently confirmed green on evidence
by an independent re-run at the same head: 7213 tests, 7213 passed, 85.67% line and 79.84% branch
coverage, clearing the 85% and 75% floors.

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
| 821 | `C:/Users/DanMoisan/repos/TaskMaster-wt/rr0908-821` | pending retry |
| 823 | `C:/Users/DanMoisan/repos/TaskMaster-wt/rr0908-823` | pending retry |
| 824 | `C:/Users/DanMoisan/repos/TaskMaster-wt/rr0908-824` | pending retry |
| 825 | `C:/Users/DanMoisan/repos/TaskMaster-wt/rr0908-825` | pending retry |

## Redundant Resume of Feature 825

Recorded because the audit trail should show it rather than hide it. The first 825 orchestrator
emitted a task-notification carrying a narrative progress report — its pull request open, the test
gate still running — instead of the agreed bounded return shape. Treating that as a stop, the parent
re-derived durable state (pull request open, worktree clean at the pull-request head), found no
`vstest.console`, `testhost`, `CodeCoverage` or `datacollector` process alive, and double-sampled the
coverage file twenty seconds apart with identical mtime and length. Every probe read as idle, so a
resume agent was launched.

The original agent was not idle. It merged twenty-three seconds after that ground-truth read, while
the resume prompt was being composed. The flaw is specific: process scans and file-mtime sampling
measure the **workload**, not the **agent**. An agent between tool calls after a long test run is
indistinguishable from a dead one by those probes.

The resulting double-delegation is bounded. The single-instance side effect at this step is the
merge, and it is not repeatable — the pull request was already merged, so the resume agent could not
merge it again. No second pull request, no second issue, no branch mutation. Wave 1 was deliberately
held until the redundant agent exited, preserving the effective-concurrency-of-one invariant that
exists because non-isolated children share one session-scoped orchestrator checkpoint.

## Delivered Scope, and What Is Explicitly Not Closed

This epic closes exactly eight issues: 813, 815, 817, 821, 823, 824, 825 and 826. No other issue is
resolved by it.

`collect_pr_context` emits a much longer "author asserted" auto-close list — including #181, #230,
#441, #478, #498, #602, #670, #677, #780, #797, #798, #799, #801, #805, #809, #810, #811, #812 and
#814 — by scraping issue numbers from commit messages and feature documentation. Those are prose
references, not deliveries. They must not be turned into closing keywords.

The sharpest case is **#181, which remains open and must stay open.** Feature 826 was authorized to
promote `dotnet_diagnostic.RS0030.severity` from `suggestion` to `warning`, but the authorization
explicitly did not extend to breaking the build to obtain it. The feature took the documented
fallback: it delivered the reachable subset by extending `BannedSymbols.txt` from seven entries to
fifteen — adding `CancelAfter` (both overloads), the two deadline `CancellationTokenSource`
constructors, and all four timed `WaitOne` overloads — and left `RS0030` at `suggestion` with an
in-file comment recording the blocking precondition.

That comment also corrects a figure carried in the epic manifest. The manifest cited roughly 143
pre-existing usages; 826 re-measured at implementation time and found 153 **textual** hits
(`DateTime.Now` 53, `DateTime.UtcNow` 20, `Random.Shared` 5, `Thread.Sleep` 15, `Task.Delay` 60),
noting that textual hits are not diagnostics and the earlier figure was a diagnostic count, so the
two are not comparable. Promotion stays blocked because toolchain step 3 promotes every warning to a
build error.

This is the epic's Non-Goal 5 and its NFR set being honoured rather than worked around: no threshold
was lowered, no severity weakened, and no production file added to a coverage exclusion list to make
a gate pass.

## Preparation Provenance

All eight features were prepared by `epic-planner` and committed to the integration branch: issue,
active feature folder, research, `spec.md`, an approved atomic plan, and a recorded
`PREFLIGHT: ALL CLEAR` verified against each child's own checkpoint on disk. `user-story.md` is
absent for every feature, which is correct for `full-bug` work mode. Children resume at atomic
execution from their committed `plan-path` and do not re-run promotion, research, or planning.
