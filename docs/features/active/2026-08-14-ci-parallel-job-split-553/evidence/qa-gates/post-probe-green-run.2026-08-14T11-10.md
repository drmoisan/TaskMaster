# Post-Revert Green Run — Issue #553

- Timestamp: 2026-08-14T11-10 (local) / 2026-08-14T15:10Z (UTC)
- Task: [P4-T4]

Commands:

```
gh workflow run ci.yml --ref feature/ci-parallel-job-split-553
gh run watch 31812508684 --interval 20 --exit-status
gh api repos/drmoisan/TaskMaster/actions/runs/31812508684/jobs --jq '.jobs[] | {name, conclusion}'
```

EXIT_CODE: 0 — `gh run watch --exit-status` returned 0, which it does only for a
successful run.

## Run

- Run: [31812508684](https://github.com/drmoisan/TaskMaster/actions/runs/31812508684)
- Head SHA: `ad28ea81e85ed09399feb4275828d00efeccc790`
- Run conclusion: **success**

The head is the third revert commit, so this run exercises the branch in its
probe-free state: all three seeded violations have been introduced, observed, and
cancelled by their reverts.

Note on trigger: the final revert push did not itself start a run, because
`ci.yml` triggers only on `push` to `[main, development]` and on `pull_request`
against them. The run was started with
`gh workflow run ci.yml --ref feature/ci-parallel-job-split-553`, and its
`head_sha` was verified to equal the branch tip before watching.

## Per-job conclusions and durations

| Job (check-run context) | Conclusion | Started | Completed | Duration |
| --- | --- | --- | --- | --- |
| `actionlint / actionlint` | success | 15:02:37Z | 15:03:11Z | 34s |
| `format-check / Verify formatting` | success | 15:02:38Z | 15:05:03Z | 145s |
| `build-nullable / Build with nullable warnings treated as errors` | success | 15:02:37Z | 15:05:29Z | 172s |
| `build-analyzers / Build with analyzers and code style enforcement` | success | 15:02:38Z | 15:06:03Z | 205s |
| `mstest-coverage / Run MSTest suite with coverage` | success | 15:02:38Z | 15:09:50Z | 432s |

**All five job conclusions are `success`.**

Pipeline wall clock: **433s** (earliest start 15:02:37Z, latest completion
15:09:50Z). This is materially slower than the 259s measured on the first green
run (31809697953) and the difference is analysed in
`ci-split-timing-comparison.2026-08-14T11-10.md`; it is runner variance, not a
structural change. The workflow files are byte-identical between the two runs.

## Significance

1. **The branch is green in its probe-free state.** Every seeded fault was
   demonstrated and then removed; the pipeline returns to green on the reverted
   tree, which is what makes the probes safe to have run on this branch.
2. **This run satisfies `modified-workflow-needs-green-run` for the current
   head.** [P5-T15] re-confirms it on the final pre-migration head after the
   Phase 5 evidence commit, and [P5-T16] captures the required context names from
   that final head.
3. **All five jobs again started within one second of each other**, confirming
   the concurrent scheduling is stable across runs.

## Acceptance ([P4-T4])

- Artifact records run id `31812508684`, head SHA
  `ad28ea81e85ed09399feb4275828d00efeccc790`, and all five job conclusions
  `success`.
