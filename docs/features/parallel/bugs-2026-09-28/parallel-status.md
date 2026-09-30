# Parallel Run Status: bugs-2026-09-28

Generated projection of `artifacts/orchestration/parallel-orchestrator-state.json`. Do not edit by hand.

## Run

- parallel_slug: bugs-2026-09-28
- mode: open
- max_concurrency: 6
- current_cohort: 1
- recolor_generation: 2
- last_updated: 2026-09-30T11-43
- next_step: 940 running under AC8 ruling; 945 held on 940 merge; 941 not admitted

## Items

| issue_num | feature_folder | cohort | state | merge_status | pr_url | merge_commit_sha |
| --- | --- | --- | --- | --- | --- | --- |
| 882 | docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/934 | cca27280eef64b563be200445e434850de88bdc2 |
| 927 | docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/943 | 231e1c0b55105aeb626bf5a6e8d0266a567cacad |
| 928 | docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/938 | c4ff0e2be0bc9c51acc43dacd2cc5954a448676c |
| 929 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929 | 1 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/949 | 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817 |
| 930 | docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/935 | dcce3c8169f7ec528e4b706bba66931017d60794 |
| 931 | docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931 | 0 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/939 | ddbab26a0149bf2ca5d0256e60686ad79e74d90c |
| 942 | docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942 | 1 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/946 | b305903e275b8abf58e8e65831c189f517568fe4 |
| 940 | docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940 | 1 | in_flight | worktree_created |  |  |
| 945 | docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945 | 2 | scheduled | not_started |  |  |
| 944 | docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944 | 2 | merged | merged | https://github.com/drmoisan/TaskMaster/pull/954 | 829ad415db99bf13b70a7766fe85dfa0d521f706 |

## Item Lifecycle Timestamps

| issue_num | scheduled_at | worktree_created_at | merged_at | worktree_removed_at |
| --- | --- | --- | --- | --- |
| 882 | 2026-09-29T08-46 | 2026-09-29T08-47 | 2026-09-29T09-58 |  |
| 927 | 2026-09-29T08-46 | 2026-09-29T08-47 | 2026-09-29T23-51 |  |
| 928 | 2026-09-29T08-46 | 2026-09-29T08-47 | 2026-09-29T11-40 |  |
| 929 | 2026-09-29T08-46 | 2026-09-30T07-05 | 2026-09-30T10-33 |  |
| 930 | 2026-09-29T08-46 | 2026-09-29T08-47 | 2026-09-29T10-31 |  |
| 931 | 2026-09-29T08-46 | 2026-09-29T08-47 | 2026-09-29T20-45 |  |
| 942 | 2026-09-30T00-52 | 2026-09-30T07-05 | 2026-09-30T08-21 |  |
| 940 | 2026-09-30T01-30 | 2026-09-30T07-05 |  |  |
| 945 | 2026-09-30T08-49 |  |  |  |
| 944 | 2026-09-30T09-08 | 2026-09-30T09-12 | 2026-09-30T11-43 |  |

## Cohorts

| index | generation | item_keys |
| --- | --- | --- |
| 0 | 0 | 882, 927, 928, 930, 931 |
| 1 | 0 | 929, 940, 942 |
| 0 | 1 | 882, 927, 928, 930, 931 |
| 1 | 1 | 929, 940, 942 |
| 2 | 1 | 945 |
| 0 | 2 | 882, 927, 928, 930, 931 |
| 1 | 2 | 929, 940, 942 |
| 2 | 2 | 944, 945 |

## Conflict Edges

| a | b | reason | all_reasons |
| --- | --- | --- | --- |
| 927 | 929 | path_overlap | path_overlap:.github/workflows/README.md |
| 931 | 940 | path_overlap | path_overlap:docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/** ~ docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t13-follow-up-handoff.2026-09-29T09-46.md |
| 940 | 945 | path_overlap | path_overlap:UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs |
| 927 | 944 | path_overlap | path_overlap:scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 |
| 942 | 944 | path_overlap | path_overlap:TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs; path_overlap:TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs; path_overlap:TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs; path_overlap:TaskMaster.Test/TaskMaster.Test.csproj; path_overlap:TaskMaster/Ribbon/EngineToggleStateCoordinator.cs; path_overlap:TaskMaster/Ribbon/RibbonController.EngineCommands.cs |

## Mutations

| op | item_key | at | prior_state | new_state | disposition | recolor_generation |
| --- | --- | --- | --- | --- | --- | --- |
| add | 942 | 2026-09-30T00-52 |  | scheduled |  | 0 |
| add | 940 | 2026-09-30T01-30 |  | scheduled |  | 0 |
| add | 945 | 2026-09-30T08-49 |  | scheduled |  | 1 |
| add | 944 | 2026-09-30T09-08 |  | scheduled |  | 2 |

## Drift Events


## Mergeable Conflicts Resolved

