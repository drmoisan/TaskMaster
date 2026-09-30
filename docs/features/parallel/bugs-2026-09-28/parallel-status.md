# Parallel Run Status: bugs-2026-09-28

Generated projection of `artifacts/orchestration/parallel-orchestrator-state.json`. Do not edit by hand.

## Run

- parallel_slug: bugs-2026-09-28
- mode: open
- max_concurrency: 6
- current_cohort: 1
- recolor_generation: 0
- last_updated: 2026-09-30T08-22
- next_step: await cohort 1 children (929, 940, 942); 941 to launch only after its admission appears in the checkpoint

## Items

| issue_num | feature_folder | cohort | state | merge_status | pr_url | merge_commit_sha |
| --- | --- | --- | --- | --- | --- | --- |
| 882 | docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/934 | cca27280eef64b563be200445e434850de88bdc2 |
| 927 | docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/943 | 231e1c0b55105aeb626bf5a6e8d0266a567cacad |
| 928 | docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/938 | c4ff0e2be0bc9c51acc43dacd2cc5954a448676c |
| 929 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929 | 1 | in_flight | worktree_created |  |  |
| 930 | docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/935 | dcce3c8169f7ec528e4b706bba66931017d60794 |
| 931 | docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/939 | ddbab26a0149bf2ca5d0256e60686ad79e74d90c |
| 942 | docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942 | 1 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/946 | b305903e275b8abf58e8e65831c189f517568fe4 |
| 940 | docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940 | 1 | in_flight | worktree_created |  |  |

## Item Lifecycle Timestamps

| issue_num | scheduled_at | worktree_created_at | merged_at | worktree_removed_at |
| --- | --- | --- | --- | --- |
| 882 | 2026-09-29T08-46 | 2026-09-29T08-47 | 2026-09-29T09-58 |  |
| 927 | 2026-09-29T08-46 | 2026-09-29T08-47 | 2026-09-29T23-51 |  |
| 928 | 2026-09-29T08-46 | 2026-09-29T08-47 | 2026-09-29T11-40 |  |
| 929 | 2026-09-29T08-46 | 2026-09-30T07-05 |  |  |
| 930 | 2026-09-29T08-46 | 2026-09-29T08-47 | 2026-09-29T10-31 |  |
| 931 | 2026-09-29T08-46 | 2026-09-29T08-47 | 2026-09-29T20-45 |  |
| 942 | 2026-09-30T00-52 | 2026-09-30T07-05 | 2026-09-30T08-21 |  |
| 940 | 2026-09-30T01-30 | 2026-09-30T07-05 |  |  |

## Cohorts

| index | generation | item_keys |
| --- | --- | --- |
| 0 | 0 | 882, 927, 928, 930, 931 |
| 1 | 0 | 929, 940, 942 |

## Conflict Edges

| a | b | reason | all_reasons |
| --- | --- | --- | --- |
| 927 | 929 | path_overlap | path_overlap:.github/workflows/README.md |
| 931 | 940 | path_overlap | path_overlap:docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/** ~ docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t13-follow-up-handoff.2026-09-29T09-46.md |

## Mutations

| op | item_key | at | prior_state | new_state | disposition | recolor_generation |
| --- | --- | --- | --- | --- | --- | --- |
| add | 942 | 2026-09-30T00-52 |  | scheduled |  | 0 |
| add | 940 | 2026-09-30T01-30 |  | scheduled |  | 0 |

## Drift Events


## Mergeable Conflicts Resolved

