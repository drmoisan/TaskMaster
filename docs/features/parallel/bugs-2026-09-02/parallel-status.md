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
| last_updated | 2026-09-04T09-15 |
| next_step | Every non-withdrawn item in this run (15 of 15) has reached merge_status merged or worktree_removed. Issue 584 was confirmed still open post-merge and closed directly with a link to PR 778, mirroring the 731/732 precedent. Per mode open this run does not auto-complete and remains a standing queue; it terminates only via /parallel-close, which is out of scope for this driver and has not been invoked. |

## Items

| issue_num | feature_folder | cohort | state | merge_status | pr_url | merge_commit_sha |
| --- | --- | --- | --- | --- | --- | --- |
| 564 | docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/745 | 8be5a6aac3b5a82c86241fbbf989fd9118602c56 |
| 565 | docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565 | 1 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/753 | 87cb4df338322844abfa580abea14df77e738e5c |
| 584 | docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584 | 3 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/778 | 1c3b210cab966b56a51c9648cd19c6f27b8d8e0b |
| 645 | docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645 | 2 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/755 | 495b012929675f59dd4dea082a0694c0f5a27369 |
| 707 | docs/features/active/2026-08-31-narrow-fileio2-retryable-exception-set-707 | 3 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/756 | 35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e |
| 729 | docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/746 | a679cd082819af6788cd0fb35f4366786fab87e3 |
| 730 | docs/features/active/2026-09-02-ci-build-infra-debt-730 | 1 | merged | worktree_removed | https://github.com/drmoisan/TaskMaster/pull/747 | 196561ca7a7f595bd88619e908e971b5636b6192 |
| 731 | docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731 | 3 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/775 | 66749143601aedb816c679b911f1042ffa3e86a5 |
| 732 | docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732 | 2 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/754 | f8414ee979e1884c4a93703523509d4f45e89151 |
| 733 | docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/748 | b13d5b7b1a6dd0aa79d51d48a7156ee67377f9d0 |
| 735 | docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735 | 1 | merged | worktree_removed | https://github.com/drmoisan/TaskMaster/pull/749 | b01c37654b4cf0a470b09d565fcd5b76d2bcd758 |
| 736 | docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736 | 4 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/777 | c7078d4170c708a6124573b28a81ec7bcc001cb6 |
| 737 | docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737 | 1 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/750 | 87233f867ad60c0a5c0d19b09cc121ae536d7ba1 |
| 751 | docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751 | 2 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/758 | 8642d42ce4562241152c8c67f6cad372fcfded46 |
| 752 | docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752 | 2 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/776 | c12c4c34c227cede1690bcb5705e309ce9fa8796 |

### Item lifecycle timestamps

| issue_num | worktree_created_at | pr_opened_at | ci_green_at | merged_at | worktree_removed_at |
| --- | --- | --- | --- | --- | --- |
| 564 | 2026-09-02T18-10 | 2026-09-02T21-50 | 2026-09-02T22-00 | 2026-09-02T22-04 | - |
| 565 | 2026-09-03T01-30 | 2026-09-03T07-44 | 2026-09-03T07-50 | 2026-09-03T07-50 | - |
| 584 | 2026-09-03T07-57 | 2026-09-04T08-10 | 2026-09-04T09-10 | 2026-09-04T08-16 | - |
| 645 | 2026-09-03T07-19 | 2026-09-03T14-40 | 2026-09-03T15-15 | 2026-09-03T15-55 | - |
| 707 | 2026-09-03T07-57 | 2026-09-03T14-42 | 2026-09-03T15-15 | 2026-09-03T15-55 | - |
| 729 | 2026-09-02T18-10 | 2026-09-03T00-38 | 2026-09-03T01-04 | 2026-09-03T01-05 | - |
| 730 | 2026-09-02T22-15 | 2026-09-03T00-45 | 2026-09-03T01-13 | 2026-09-03T01-14 | 2026-09-03T01-18 |
| 731 | 2026-09-03T16-00 | 2026-09-03T20-15 | 2026-09-03T21-05 | 2026-09-03T21-08 | - |
| 732 | 2026-09-03T07-19 | 2026-09-03T07-59 | 2026-09-03T08-10 | 2026-09-03T08-11 | - |
| 733 | 2026-09-02T18-10 | 2026-09-03T01-06 | 2026-09-03T01-21 | 2026-09-03T01-21 | - |
| 735 | 2026-09-03T01-08 | 2026-09-03T01-27 | 2026-09-03T07-10 | 2026-09-03T07-11 | 2026-09-03T07-12 |
| 736 | 2026-09-03T21-20 | 2026-09-04T05-00 | 2026-09-04T05-15 | 2026-09-04T06-36 | - |
| 737 | 2026-09-03T01-08 | 2026-09-03T01-28 | 2026-09-03T07-17 | 2026-09-03T07-17 | - |
| 751 | 2026-09-03T16-30 | 2026-09-03T19-20 | 2026-09-03T20-05 | 2026-09-03T19-38 | - |
| 752 | 2026-09-03T15-00 | 2026-09-04T04-08 | 2026-09-04T04-45 | 2026-09-04T04-29 | - |

Worktree removal for items 645 and 707 was attempted (non-forced `git worktree remove`) and deferred: both worktrees carry modified/untracked files and removal was not forced. `merge_status: merged` still satisfies open-mode completion.

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
| 2 | 1 | 645, 732, 752, 751 |
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
| 565 | 752 | path_overlap |
| 729 | 752 | module_overlap |
| 733 | 752 | path_overlap |
| 729 | 751 | path_overlap |

## Mutations

| op | item_key | at | prior_state | new_state | disposition | recolor_generation |
| --- | --- | --- | --- | --- | --- | --- |
| add | 752 | 2026-09-03T14-55 | - | scheduled | - | 1 |
| add | 751 | 2026-09-03T16-25 | - | scheduled | - | 1 |

## Drift Events

