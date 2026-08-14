# Pre-Migration Green Run (Final Head) — Issue #553

- Timestamp: 2026-08-14T11-23 (local) / 2026-08-14T15:23Z (UTC)
- Task: [P5-T15]

Commands:

```
gh workflow run ci.yml --ref feature/ci-parallel-job-split-553
gh run watch 31813885124 --interval 20 --exit-status
gh api repos/drmoisan/TaskMaster/actions/runs/31813885124/jobs --jq '.jobs[] | {name, conclusion}'
gh pr view --json headRefOid
```

EXIT_CODE: 0 — `gh run watch --exit-status` returned 0.

## Run

- Run: [31813885124](https://github.com/drmoisan/TaskMaster/actions/runs/31813885124)
- Head SHA: **`df49d208efb56e19faee106556b723022939e5a2`**
- Run conclusion: **success**

This head is the [P5-T14] commit (`docs(553): record Phase 3-5 evidence, review
artifacts, and AC check-offs`) and is **the reference state for the ruleset
migration**. Phase 6 must not proceed against any other SHA without re-running
this confirmation and re-capturing the context names.

## Per-job conclusions and durations

| Job (check-run context) | Conclusion | Started | Completed | Duration |
| --- | --- | --- | --- | --- |
| `actionlint / actionlint` | success | 15:19:15Z | 15:19:45Z | 30s |
| `format-check / Verify formatting` | success | 15:19:15Z | 15:21:08Z | 113s |
| `build-nullable / Build with nullable warnings treated as errors` | success | 15:19:17Z | 15:22:30Z | 193s |
| `build-analyzers / Build with analyzers and code style enforcement` | success | 15:19:16Z | 15:22:32Z | 196s |
| `mstest-coverage / Run MSTest suite with coverage` | success | 15:19:15Z | 15:23:20Z | 245s |

**All five jobs `success` on the current head SHA.**

Pipeline wall clock: **245s** (15:19:15Z → 15:23:20Z), a 44.8% reduction against
the 444s baseline. This is the third green sample and is consistent with the
first (259s); see the addendum in
`ci-split-timing-comparison.2026-08-14T11-10.md`, which records it and reassesses
the 433s second sample as an outlier.

## Pull-request state

`gh pr view --json headRefOid` returns:

```
no pull requests found for branch "feature/ci-parallel-job-split-553"
```

This is expected and correct. [P3-T3] (pull-request creation) is
orchestrator-gated and remains deferred; no pull request exists. The
`modified-workflow-needs-green-run` obligation is satisfied by this
`workflow_dispatch` run against the branch head, which
`remediation-inputs.2026-08-14T10-21.md` finding B1 explicitly accepts as an
alternative to a pull-request run.

## Content of this head relative to the previous green run

The [P5-T14] commit changed only documentation and evidence, plus
`.github/workflows/README.md` (the F2 review-finding fix). **No workflow YAML
file changed** since run 31812508684, so this run re-confirms the same pipeline
definition on a newer tree. The byte-identity and structural verifications
recorded in Phases 1–2 remain valid without re-verification.

## Acceptance ([P5-T15])

- All five jobs `success` on the current head SHA.
- Artifact records run id `31813885124` and head SHA
  `df49d208efb56e19faee106556b723022939e5a2`.
- This head is the reference state for the ruleset migration.
