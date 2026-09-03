# Parallel Run Status: bugs-2026-09-02

Generated projection of artifacts/orchestration/parallel-orchestrator-state.json. Never hand-authored.

## Run

| field | value |
| --- | --- |
| parallel_slug | bugs-2026-09-02 |
| mode | open |
| max_concurrency | 16 |
| current_cohort | 2 |
| recolor_generation | 1 |
| last_updated | 2026-09-03T07-57 |
| next_step | Launch 584 and 707 immediately; both are edge-free under the corrected relation. Await 645 and 732; merging 645 releases 731, and 645 plus 732 release 736. |

## Items

| issue_num | feature_folder | cohort | state | merge_status | pr_url | merge_commit_sha |
| --- | --- | --- | --- | --- | --- | --- |
| 564 | docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/745 | 8be5a6aac3b5a82c86241fbbf989fd9118602c56 |
| 565 | docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565 | 1 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/753 | 87cb4df338322844abfa580abea14df77e738e5c |
| 584 | docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584 | 3 | in_flight | worktree_created | - | - |
| 645 | docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645 | 2 | in_flight | worktree_created | - | - |
| 707 | docs/features/active/2026-08-31-narrow-fileio2-retryable-exception-set-707 | 3 | in_flight | worktree_created | - | - |
| 729 | docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/746 | a679cd082819af6788cd0fb35f4366786fab87e3 |
| 730 | docs/features/active/2026-09-02-ci-build-infra-debt-730 | 1 | merged | worktree_removed | https://github.com/drmoisan/TaskMaster/pull/747 | 196561ca7a7f595bd88619e908e971b5636b6192 |
| 731 | docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731 | 3 | scheduled | not_started | - | - |
| 732 | docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732 | 2 | in_flight | worktree_created | - | - |
| 733 | docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/748 | b13d5b7b1a6dd0aa79d51d48a7156ee67377f9d0 |
| 735 | docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735 | 1 | merged | worktree_removed | https://github.com/drmoisan/TaskMaster/pull/749 | b01c37654b4cf0a470b09d565fcd5b76d2bcd758 |
| 736 | docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736 | 4 | scheduled | not_started | - | - |
| 737 | docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737 | 1 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/750 | 87233f867ad60c0a5c0d19b09cc121ae536d7ba1 |

### Item lifecycle timestamps

| issue_num | worktree_created_at | pr_opened_at | ci_green_at | merged_at | worktree_removed_at |
| --- | --- | --- | --- | --- | --- |
| 564 | 2026-09-02T18-10 | 2026-09-02T21-50 | 2026-09-02T22-00 | 2026-09-02T22-04 | - |
| 565 | 2026-09-03T01-30 | 2026-09-03T07-44 | 2026-09-03T07-50 | 2026-09-03T07-50 | - |
| 584 | 2026-09-03T07-57 | - | - | - | - |
| 645 | 2026-09-03T07-19 | - | - | - | - |
| 707 | 2026-09-03T07-57 | - | - | - | - |
| 729 | 2026-09-02T18-10 | 2026-09-03T00-38 | 2026-09-03T01-04 | 2026-09-03T01-05 | - |
| 730 | 2026-09-02T22-15 | 2026-09-03T00-45 | 2026-09-03T01-13 | 2026-09-03T01-14 | 2026-09-03T01-18 |
| 731 | - | - | - | - | - |
| 732 | 2026-09-03T07-19 | - | - | - | - |
| 733 | 2026-09-02T18-10 | 2026-09-03T01-06 | 2026-09-03T01-21 | 2026-09-03T01-21 | - |
| 735 | 2026-09-03T01-08 | 2026-09-03T01-27 | 2026-09-03T07-10 | 2026-09-03T07-11 | 2026-09-03T07-12 |
| 736 | - | - | - | - | - |
| 737 | 2026-09-03T01-08 | 2026-09-03T01-28 | 2026-09-03T07-17 | 2026-09-03T07-17 | - |

## Cohorts

| index | generation | item_keys |
| --- | --- | --- |
| 0 | 0 | 564, 729, 733 |
| 1 | 0 | 565, 730, 735, 737 |
| 2 | 0 | 645, 732 |
| 3 | 0 | 584, 736 |
| 4 | 0 | 707, 731 |
| 0 | 1 | 564, 729, 733 |
| 1 | 1 | 565, 730, 735, 737 |
| 2 | 1 | 645, 732 |
| 3 | 1 | 584, 707, 731 |
| 4 | 1 | 736 |

## Conflict Edges

| a | b | reason |
| --- | --- | --- |
| 564 | 730 | path_overlap |
| 565 | 729 | module_overlap |
| 565 | 733 | path_overlap |
| 645 | 731 | path_overlap |
| 645 | 736 | path_overlap |
| 729 | 731 | contract_dependency |
| 729 | 732 | path_overlap |
| 729 | 735 | path_overlap |
| 729 | 736 | path_overlap |
| 731 | 736 | path_overlap |
| 732 | 736 | path_overlap |
| 735 | 736 | path_overlap |

## Mutations


## Drift Events

