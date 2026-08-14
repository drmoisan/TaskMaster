# `test-results` Artifact Continuity — Issue #553

- Timestamp: 2026-08-14T11-10 (local) / 2026-08-14T15:10Z (UTC)
- Task: [P4-T5]

Command:

```
gh api repos/drmoisan/TaskMaster/actions/runs/31812508684/artifacts --jq '.artifacts[] | {name, size_in_bytes}'
```

(run id from [P4-T4])

EXIT_CODE: 0

## Output Summary

```json
{"expired":false,"name":"test-results","size_in_bytes":8246282}
```

- Artifact name: **`test-results`** — exactly the pre-split name, unchanged.
- Size: **8,246,282 bytes** (~7.9 MiB) — non-zero.
- Expired: `false`.
- It is the **only** artifact produced by the run, confirming that no incidental
  build-output artifact was introduced by the split. The adopted topology shares
  no files between jobs, so this upload targets workflow storage only and is not
  cross-job file transfer.

## Continuity across runs

| Run | Head | Artifact | Size |
| --- | --- | --- | --- |
| 31809697953 (first green) | `0b016c81` | `test-results` | 8,247,182 bytes |
| 31812508684 (post-probe green) | `ad28ea81` | `test-results` | 8,246,282 bytes |

Both runs produce a single `test-results` artifact of ~8.25 MB. The 900-byte
difference between runs is expected: the payload contains `.trx` logs whose
embedded timestamps, run GUIDs, and machine names differ per run. Size stability
to within 0.01% indicates the same test population and the same coverage payload.

## Why this matters

The upload step was transplanted byte-identically from the monolith (verified in
`byte-identity.2026-08-14T09-54.md`, block `upload-step`, SHA-256
`894b0ce75a70c838...`) and retains:

- `if: always()`, so results upload even when the gate fails;
- `name: test-results`, the same artifact name any downstream consumer expects;
- the same two paths, `TestResults/**/*.trx` and `TestResults/**/*.coverage`;
- `if-no-files-found: warn`.

Because the artifact now originates from `_mstest-coverage.yml` rather than the
monolithic `quality-gates` job, this check confirms the relocation did not break
production of the pipeline's only coverage-bearing output. The `if: always()`
behaviour was also exercised in practice: the [P4-T3] probe run
(31811867381), in which the MSTest gate failed, still completed its upload step.

## Acceptance ([P4-T5])

- An artifact named exactly `test-results` exists on the [P4-T4] green run with
  non-zero size (8,246,282 bytes).
- Spec seeded-condition checkbox 6 ("Test results and coverage artifacts continue
  to upload with the same names") is checked off with this artifact as the
  evidence pointer.
