# Check-Run Context Names (Captured from the Final Head) — Issue #553

- Timestamp: 2026-08-14T11-23 (local) / 2026-08-14T15:23Z (UTC)
- Task: [P5-T16]
- Source head SHA: **`df49d208efb56e19faee106556b723022939e5a2`** (the [P5-T15]
  green run, run id 31813885124)

Command:

```
gh api repos/drmoisan/TaskMaster/commits/df49d208efb56e19faee106556b723022939e5a2/check-runs --jq '.check_runs[].name'
```

EXIT_CODE: 0

## Verbatim API output

```
mstest-coverage / Run MSTest suite with coverage
build-nullable / Build with nullable warnings treated as errors
format-check / Verify formatting
actionlint / actionlint
build-analyzers / Build with analyzers and code style enforcement
```

`total_count` = **5**. Every check-run on this commit is from the
`github-actions` app and concluded `success`; there are no third-party or stale
check-runs to filter out, so the captured list and the selected list are
identical.

## The five required-context strings, selected verbatim

These are the exact strings for the [P6-T2] PUT payload. They are copied
byte-for-byte from the API output above and must not be retyped or reordered by
hand.

```
actionlint / actionlint
build-analyzers / Build with analyzers and code style enforcement
build-nullable / Build with nullable warnings treated as errors
format-check / Verify formatting
mstest-coverage / Run MSTest suite with coverage
```

(Listed alphabetically here for readability; set membership is what matters, not
order.)

## Name-form confirmation

The observed form is `<caller job id> / <callee job name>`:

| Caller job id (in `ci.yml`) | Callee job `name:` | Resulting context |
| --- | --- | --- |
| `actionlint` | `actionlint` | `actionlint / actionlint` |
| `format-check` | `Verify formatting` | `format-check / Verify formatting` |
| `build-analyzers` | `Build with analyzers and code style enforcement` | `build-analyzers / Build with analyzers and code style enforcement` |
| `build-nullable` | `Build with nullable warnings treated as errors` | `build-nullable / Build with nullable warnings treated as errors` |
| `mstest-coverage` | `Run MSTest suite with coverage` | `mstest-coverage / Run MSTest suite with coverage` |

Note that the left-hand side is the caller's **job id**, not the caller job's
`name:`. In `ci.yml` each job's id and `name:` happen to be identical, so this
distinction is not observable here — but it is the reason the strings must be
captured rather than derived.

## Both current required contexts must be replaced

The `main` ruleset (id `18572843`) currently requires exactly two contexts:

| Current required context | Still reported on this head? |
| --- | --- |
| `actionlint` | **NO** — the actionlint job moved into `_actionlint.yml`, so it now reports as `actionlint / actionlint`. The bare name no longer exists. |
| `Format, build, analyze, and test` | **NO** — the monolithic job was decomposed; the name no longer exists. |

**Both** old contexts are therefore obsolete, not just the monolithic one. A PUT
that replaced only `Format, build, analyze, and test` would leave the bare
`actionlint` context required and permanently unreportable, blocking every merge
to `main`. The payload must contain exactly the five strings above and neither
old string.

## Handling note

This artifact is **not committed by Phase 5**. Committing it would advance the
branch head past `df49d208`, which is the reference state confirmed green by
[P5-T15] and the SHA these names were captured from. It is staged and committed
by [P7-T9].

The context names derive from workflow and job names, not from commit content, so
they remain valid as long as `ci.yml` and the callee job `name:` fields are
unchanged. If any of those change, re-run this capture against the new head
before performing the PUT.

## Acceptance ([P5-T16])

- Artifact exists with exactly five selected context strings, each copied
  verbatim from the API output.
- Captured from the final head SHA `df49d208efb56e19faee106556b723022939e5a2`,
  not reused from an earlier run.
