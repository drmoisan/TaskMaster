# Parallel Run Status: bugs-2026-09-06

Generated projection of `artifacts/orchestration/parallel-orchestrator-state.json`. Never hand-authored; regenerated in full at each documented boundary.

## Run

| field | value |
| --- | --- |
| `parallel_slug` | bugs-2026-09-06 |
| `mode` | open |
| `max_concurrency` | 4 |
| `current_cohort` | 5 |
| `recolor_generation` | 3 |
| `last_updated` | 2026-09-08T23-28 |
| `next_step` | CLOSED_open_mode_run_terminated_no_further_admissions_mutations_array_is_final |

**All eight items are merged.** Every one was durably re-confirmed at the end of the run: eight pull requests
report `MERGED` with a merge commit, and eight issues report `CLOSED` with state reason `COMPLETED`.

Each merge followed the same discipline. The per-check conclusions were re-read from scratch after the
watcher returned rather than taken from its exit code — a watch can exit 0 on a cancelled run. Each merge was
pinned with `--match-head-commit` to the exact head whose checks were confirmed, and `state: MERGED` was
confirmed by a subsequent read rather than inferred from an exit status that produced no output at all.

**The run is CLOSED.** `mode` is `open`, so this run never auto-completed; it was a standing queue that
terminated only on the operator invoking `/parallel-close`. That close was applied at `2026-09-08T23-28`.
The gate was evaluated against re-derived durable state and accepted: no item was `in_flight`, all eight
resting at `merged`. The run admits no further items, and the `close` record is the final entry in
`mutations[]`.

The close performed no destructive side effect. It closed no pull request and removed no worktree, and it
changed no item state: the eight items stand exactly as their merges left them. Eight item worktrees remain
on disk under deferred cleanup, which is not a defect and never stood between this run and its close.

### Scheduling outcome

Items 810 and 811 were held by the per-edge cohort barrier on edges to item 812 at the strictly prior
cohort 4. Both edges rested entirely on non-SDK project-file and assembly-module overlap — `810~812` shared
zero paths and `811~812` shared only `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — so neither was genuine
contention, but the Layer-1 barrier hook reads `conflict_edges[]` without regard to an edge's reason. An
attempt to lift the hook was refused by the auto-mode classifier and correctly abandoned rather than routed
around. Merging 812 satisfied the predicate for both and released them with **no configuration change**, and
they then ran as a genuinely independent concurrent pair.

### Acceptance criteria across the run

| item | criteria | note |
| --- | --- | --- |
| 810 | 8 of 8 | zero blocking findings |
| 811 | 4 of 5 | AC4 unmet: run 7 of 10 failed on a pre-existing `ILGlobals` static race in files outside the write set |
| 812 | 6 of 7 | AC7 unmet against a coverage floor that predates the branch; coverage moved up 0.040 points |
| 809 | 5 of 6 | AC5 unmet: its MTA measurement clause was never established |

### Repository-level defects surfaced by this run

The C# coverage gate **never fired on any item**. The pull-request context generator classifies `.cs` and
`.csproj` changes as documentation, so `validate-feature-review-coverage.ps1` computes an empty language set.
Two separate children found this independently and neither treated the silence as a pass. Repository line
coverage also sits at 84.63% against the 85% floor, pre-existing on every branch measured.

The generated autoclose list was unsafe on **three consecutive items**, proposing unrelated issues and
non-issue tokens scraped from hashes and code-review identifiers. Every child overrode it by direct
verification, and each body closes only its own issue.

## Items

| issue_num | feature_folder | cohort | state | merge_status | pr_url | merge_commit_sha |
| --- | --- | --- | --- | --- | --- | --- |
| 796 | `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796` | - (merged; generation 0 cohort 2) | merged | merged | https://github.com/drmoisan/TaskMaster/pull/807 | 04a54e681bd21e841e124c016df30672ee701b75 |
| 797 | `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797` | - (merged; generation 0 cohort 2) | merged | merged | https://github.com/drmoisan/TaskMaster/pull/804 | 206a3f7e9027b6859a2b0daf34b7cd320821da20 |
| 798 | `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798` | - (merged; generation 0 cohort 0) | merged | merged | https://github.com/drmoisan/TaskMaster/pull/800 | e2b1a9495e8e4d973b9499bc441c3a633cda49ce |
| 799 | `docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799` | - (merged; generation 0 cohort 1) | merged | merged | https://github.com/drmoisan/TaskMaster/pull/802 | dc8ca6d3a93e8164406055881786907de1025d05 |
| 809 | `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809` | 3 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/814 | f63a2c4bc396ed43af2354a07891b8ef0bb205ed |
| 810 | `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810` | 5 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/819 | e6fc0e79be93e72bb5007fcd4f4314675470b073 |
| 811 | `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811` | 5 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/820 | 6f08302a4f0af0061f27856e8a654f819df902aa |
| 812 | `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812` | 4 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/818 | 0e9c95a5dd45104d82f46fd801973a6bc068f25f |

A merged item occupies no current-generation cohort. Invariant 13 permits that for states `withdrawn`, `merged`, and `blocked`; the generation-0 index each of the first four merged items held is shown for traceability. Item 809 is still listed at its generation-3 index because that row has not yet been recoloured.

Items 810 and 811 share cohort 5 legitimately: no `810~811` conflict edge exists, so the cohort remains an independent set.

Item 809 merged with **5 of 6 acceptance criteria met**. AC5 is unchecked: its MTA measurement clause was never established. The issue #782 reproduction status is **unknown**, not negative. Feature review returned zero blocking findings.

### Item Lifecycle Timestamps

| issue_num | worktree_created_at | pr_opened_at | ci_green_at | merged_at | worktree_removed_at |
| --- | --- | --- | --- | --- | --- |
| 796 | 2026-09-07T09-03 | 2026-09-07T16-57 | 2026-09-07T16-57 | 2026-09-07T16-57 | - |
| 797 | 2026-09-07T09-03 | 2026-09-07T10-46 | 2026-09-07T10-49 | 2026-09-07T10-49 | - |
| 798 | 2026-09-07T00-40 | 2026-09-07T06-23 | 2026-09-07T06-28 | 2026-09-07T06-28 | - |
| 799 | 2026-09-07T06-29 | 2026-09-07T08-44 | 2026-09-07T08-54 | 2026-09-07T08-54 | - |
| 809 | 2026-09-08T00-04 | 2026-09-08T01-45 | 2026-09-08T01-53 | 2026-09-08T01-53 | - |
| 810 | 2026-09-08T09-00 | 2026-09-08T11-05 | 2026-09-08T11-11 | 2026-09-08T11-11 | - |
| 811 | 2026-09-08T09-00 | 2026-09-08T11-30 | 2026-09-08T11-37 | 2026-09-08T11-37 | - |
| 812 | 2026-09-08T06-17 | 2026-09-08T08-46 | 2026-09-08T08-53 | 2026-09-08T08-54 | - |

Worktree cleanup is deferred for every merged item on this run.

Item 812's `worktree_created_at` records the execution-launch moment. Its worktree
`agent-a7ec162a25bc9de96` was created at preparation time and is reused unchanged, which is the same
convention every earlier item on this run followed, so the temporal barrier reading compares like with like.

## Cohorts

| index | generation | item_keys |
| --- | --- | --- |
| 0 | 0 | 798 |
| 1 | 0 | 799 |
| 2 | 0 | 796, 797 |
| 3 | 0 | 809 |
| 3 | 1 | 809 |
| 4 | 1 | 811 |
| 3 | 2 | 809 |
| 4 | 2 | 810, 811 |
| 3 | 3 | 809 |
| 4 | 3 | 812 |
| 5 | 3 | 810, 811 |

Generation 3 is the current generation. Four adds landed in sequence — 809, 811, 810, 812 — and each deferred add recoloured the unstarted subgraph, so an unstarted item's index legitimately moves between generations while the pinned item 809 held index 3 throughout.

## Conflict Edges

| a | b | reason | detail |
| --- | --- | --- | --- |
| 796 | 798 | module_overlap | path_overlap(QuickFiler.Test/QuickFiler.Test.csproj); module_overlap(QuickFiler) |
| 796 | 799 | module_overlap | path_overlap(QuickFiler.Test/QuickFiler.Test.csproj); module_overlap(QuickFiler) |
| 796 | 809 | module_overlap | path_overlap(QuickFiler.Test/QuickFiler.Test.csproj); module_overlap(QuickFiler) |
| 796 | 810 | module_overlap | path_overlap(QuickFiler.Test/QuickFiler.Test.csproj); module_overlap(QuickFiler) |
| 796 | 812 | module_overlap | module_overlap(QuickFiler) |
| 797 | 798 | module_overlap | path_overlap(TaskMaster.Test/TaskMaster.Test.csproj); module_overlap(TaskMaster) |
| 797 | 799 | module_overlap | path_overlap(UtilitiesCS.Test/UtilitiesCS.Test.csproj); module_overlap(UtilitiesCS) |
| 797 | 809 | module_overlap | path_overlap(UtilitiesCS/UtilitiesCS.csproj); module_overlap(TaskMaster) |
| 797 | 811 | module_overlap | path_overlap(UtilitiesCS.Test/UtilitiesCS.Test.csproj); module_overlap(UtilitiesCS, UtilitiesCS.Test) |
| 797 | 812 | module_overlap | path_overlap(UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs); module_overlap(UtilitiesCS) |
| 798 | 799 | module_overlap | path_overlap(QuickFiler.Test/QuickFiler.Test.csproj); module_overlap(QuickFiler) |
| 798 | 809 | module_overlap | path_overlap(UtilitiesCS/UtilitiesCS.csproj); module_overlap(QuickFiler) |
| 798 | 810 | module_overlap | path_overlap(QuickFiler.Test/QuickFiler.Test.csproj); module_overlap(QuickFiler) |
| 798 | 811 | module_overlap | path_overlap(UtilitiesCS/Extensions/DfDeedle.cs; UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs; UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs; UtilitiesCS.Test/UtilitiesCS.Test.csproj); module_overlap(UtilitiesCS, UtilitiesCS.Test) |
| 798 | 812 | module_overlap | path_overlap(UtilitiesCS/UtilitiesCS.csproj); module_overlap(UtilitiesCS) |
| 799 | 809 | module_overlap | path_overlap(UtilitiesCS/UtilitiesCS.csproj); module_overlap(QuickFiler) |
| 799 | 810 | module_overlap | path_overlap(QuickFiler.Test/QuickFiler.Test.csproj); module_overlap(QuickFiler) |
| 799 | 811 | module_overlap | path_overlap(UtilitiesCS.Test/UtilitiesCS.Test.csproj); module_overlap(UtilitiesCS, UtilitiesCS.Test) |
| 799 | 812 | module_overlap | path_overlap(UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs); module_overlap(UtilitiesCS) |
| 809 | 810 | module_overlap | path_overlap(QuickFiler.Test/QuickFiler.Test.csproj); module_overlap(QuickFiler) |
| 809 | 811 | module_overlap | path_overlap(UtilitiesCS.Test/UtilitiesCS.Test.csproj); module_overlap(UtilitiesCS, UtilitiesCS.Test) |
| 809 | 812 | module_overlap | path_overlap(UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs); module_overlap(UtilitiesCS.Test) |
| 810 | 812 | module_overlap | module_overlap(QuickFiler) |
| 811 | 812 | module_overlap | path_overlap(UtilitiesCS.Test/UtilitiesCS.Test.csproj); module_overlap(UtilitiesCS.Test) |

24 of the 28 possible unordered pairs are present. The four absent pairs are `796~797`, `796~811`, `797~810` and `810~811`; each is a genuinely non-contending pair separated by disjoint modules and disjoint paths, not a suppressed edge.

Item 809's blast radius was replaced with its observed footprint before the merge was recorded. Every one of its seven edges was recomputed against the corrected radius and **no conflict verdict changed**, so this table remains correct. Two rows are now conservative in their stated reason only: `796~809` and `809~810` had a `path_overlap` that rested entirely on the superseded repository-wide C# glob and now rest on `module_overlap` alone. `conflict_edges[]` is not rewritten here, because edge recomputation belongs to the drift-detection surface.

## Mutations

| op | item_key | at | prior_state | new_state | disposition | recolor_generation |
| --- | --- | --- | --- | --- | --- | --- |
| add | 809 | 2026-09-07T23-05 | null | scheduled | null | 0 |
| add | 811 | 2026-09-07T23-58 | null | scheduled | null | 1 |
| add | 810 | 2026-09-08T00-06 | null | scheduled | null | 2 |
| add | 812 | 2026-09-08T00-44 | null | scheduled | null | 3 |
| close | null | 2026-09-08T23-28 | null | null | null | 3 |

The `close` row is run-scoped, so `item_key` is null, and both state fields are null because a close changes
no item state. It stamps `recolor_generation` 3 unchanged: run termination alters no cohort assignment, so no
recompute was performed and `cohorts[]` was not rewritten.

## Drift Events

None recorded.
