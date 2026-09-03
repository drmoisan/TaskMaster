# Parallel Run Status: bugs-2026-09-02

Generated projection of artifacts/orchestration/parallel-orchestrator-state.json. Never hand-authored.

## Run

| field | value |
| --- | --- |
| parallel_slug | bugs-2026-09-02 |
| mode | open |
| max_concurrency | 16 |
| current_cohort | 0 |
| recolor_generation | 0 |
| last_updated | 2026-09-03T01-08 |
| next_step | Bring pull request 747 (item 730) up to date against main a679cd08, re-confirm its checks green on the NEW head, and merge it. Then do the same for 748 (item 733). Await 735 and 737, create their pull requests from the parent on READY FOR PR, and re-evaluate the barrier after every merge. |

## Items

| issue_num | feature_folder | cohort | state | merge_status | pr_url | merge_commit_sha |
| --- | --- | --- | --- | --- | --- | --- |
| 564 | docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/745 | 8be5a6aac3b5a82c86241fbbf989fd9118602c56 |
| 565 | docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565 | 1 | scheduled | not_started | - | - |
| 584 | docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584 | 3 | scheduled | not_started | - | - |
| 645 | docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645 | 2 | scheduled | not_started | - | - |
| 707 | docs/features/active/2026-08-31-narrow-fileio2-retryable-exception-set-707 | 4 | scheduled | not_started | - | - |
| 729 | docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/746 | a679cd082819af6788cd0fb35f4366786fab87e3 |
| 730 | docs/features/active/2026-09-02-ci-build-infra-debt-730 | 1 | in_flight | ci_green | https://github.com/drmoisan/TaskMaster/pull/747 | - |
| 731 | docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731 | 4 | scheduled | not_started | - | - |
| 732 | docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732 | 2 | scheduled | not_started | - | - |
| 733 | docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733 | 0 | in_flight | pr_open | https://github.com/drmoisan/TaskMaster/pull/748 | - |
| 735 | docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735 | 1 | in_flight | worktree_created | - | - |
| 736 | docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736 | 3 | scheduled | not_started | - | - |
| 737 | docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737 | 1 | in_flight | worktree_created | - | - |

### Item lifecycle timestamps

| issue_num | worktree_created_at | pr_opened_at | ci_green_at | merged_at | worktree_removed_at |
| --- | --- | --- | --- | --- | --- |
| 564 | 2026-09-02T18-10 | 2026-09-02T21-50 | 2026-09-02T22-00 | 2026-09-02T22-04 | - |
| 565 | - | - | - | - | - |
| 584 | - | - | - | - | - |
| 645 | - | - | - | - | - |
| 707 | - | - | - | - | - |
| 729 | 2026-09-02T18-10 | 2026-09-03T00-38 | 2026-09-03T01-04 | 2026-09-03T01-05 | - |
| 730 | 2026-09-02T22-15 | 2026-09-03T00-45 | 2026-09-03T01-04 | - | - |
| 731 | - | - | - | - | - |
| 732 | - | - | - | - | - |
| 733 | 2026-09-02T18-10 | 2026-09-03T01-06 | - | - | - |
| 735 | 2026-09-03T01-08 | - | - | - | - |
| 736 | - | - | - | - | - |
| 737 | 2026-09-03T01-08 | - | - | - | - |

## Cohorts

| index | generation | item_keys |
| --- | --- | --- |
| 0 | 0 | 564, 729, 733 |
| 1 | 0 | 565, 730, 735, 737 |
| 2 | 0 | 645, 732 |
| 3 | 0 | 584, 736 |
| 4 | 0 | 707, 731 |

## Conflict Edges

| a | b | reason |
| --- | --- | --- |
| 564 | 730 | path_overlap |
| 565 | 729 | module_overlap |
| 565 | 733 | path_overlap |
| 584 | 707 | module_overlap |
| 584 | 729 | module_overlap |
| 584 | 732 | module_overlap |
| 584 | 737 | module_overlap |
| 645 | 729 | module_overlap |
| 645 | 731 | path_overlap |
| 645 | 736 | path_overlap |
| 645 | 737 | module_overlap |
| 707 | 729 | module_overlap |
| 707 | 732 | module_overlap |
| 707 | 737 | module_overlap |
| 729 | 731 | module_overlap |
| 729 | 732 | path_overlap |
| 729 | 735 | path_overlap |
| 729 | 736 | path_overlap |
| 729 | 737 | module_overlap |
| 731 | 736 | path_overlap |
| 731 | 737 | module_overlap |
| 732 | 735 | module_overlap |
| 732 | 736 | path_overlap |
| 732 | 737 | module_overlap |
| 735 | 736 | path_overlap |
| 736 | 737 | module_overlap |

## Mutations


## Drift Events

