# Remote Tooling Probe — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-35-16
- Task: [P0-T13]
- Finding: R1
- `GH-AVAILABLE: true`

## Invocation 1 — `gh auth status`

EXIT_CODE: 0

Verbatim output:

```
github.com
  ✓ Logged in to github.com account drmoisan (keyring)
  - Active account: true
  - Git operations protocol: https
  - Token: ghp_************************************
  - Token scopes: 'delete:packages', 'gist', 'read:org', 'read:repo_hook', 'read:user', 'repo', 'workflow', 'write:packages'
```

The token value is masked by `gh` itself; no secret is recorded here.

`GH-AVAILABLE: true`. The deciding output is the line
`✓ Logged in to github.com account drmoisan (keyring)` together with exit code 0. The `workflow`
scope is present, which is what the [P6-T2] `gh workflow run` dispatch requires; the `repo` scope
is present, which is what authorises invocation 3 below.

## Invocation 2 — existing CI runs on this branch

```
gh run list --workflow=ci.yml --branch bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911 --limit 5 --json databaseId,headSha,conclusion,status
```

EXIT_CODE: 0

Verbatim output:

```
[]
```

Zero runs. This is the state R1 records: no workflow run exists against this branch at any commit,
so the green-run obligation is entirely outstanding and is discharged in Phase 6. The branch has
never been pushed, which is why the list is empty rather than stale.

## Invocation 3 — repository Actions secrets

```
gh api repos/drmoisan/TaskMaster/actions/secrets --jq ".secrets | length"
```

EXIT_CODE: 0

Verbatim output:

```
0
```

Recorded as the **integer 0**, not as an empty list. The distinction is load-bearing: an
authorised query returning zero proves absence, whereas a `403 FORBIDDEN` would have proven only
that the query could not see the answer. This query was authorised — the token carries the `repo`
scope and the call returned 200 with a countable body — so:

`CREDENTIAL-PRESENT: false`

Neither `DEPENDABOT_REPAIR_APP_ID` nor `DEPENDABOT_REPAIR_APP_PRIVATE_KEY` exists. This is the same
state the predecessor cycle recorded and is the evidenced basis for deferring AC18, AC19 and AC20
to issue #914. It is also the reason every Phase 3 task carries a **gate rule 20** residual clause:
the repair workflow cannot be executed from this state, because its first step mints an
installation token from two secrets that are not present.

## Invocation 4 — open pull requests

```
gh pr list --repo drmoisan/TaskMaster --state open --json number,headRefName --jq "length"
```

EXIT_CODE: 0

Verbatim output:

```
0
```

Zero open pull requests, so no open Dependabot pull request exists either. That is the second
missing precondition for AC18, AC19 and AC20, and it also means the `dependabot-repair` workflow's
`workflow_run` trigger has no branch to fire against.

## Summary of the Four Probes

| # | Invocation | EXIT_CODE | Result |
|---|---|---|---|
| 1 | `gh auth status` | 0 | authenticated, `workflow` and `repo` scopes present |
| 2 | `gh run list --workflow=ci.yml --branch <this branch>` | 0 | `[]`, zero runs |
| 3 | `gh api .../actions/secrets --jq ".secrets \| length"` | 0 | integer `0`, authorised, `CREDENTIAL-PRESENT: false` |
| 4 | `gh pr list --state open --jq "length"` | 0 | integer `0` |

All four outputs are quoted verbatim above rather than summarised.

## Consequences Recorded for Later Phases

- [P6-T2] takes its `GH-AVAILABLE: true` branch: it dispatches `CI` against the branch and polls
  for the conclusion. It may not record `EXIT_CODE: SKIPPED`.
- The four **gate rule 20** residuals that remain unverifiable until #914 are unchanged by this
  probe and are reproduced at [P5-T12] and [P6-T3].

## Output Summary

`GH-AVAILABLE: true`, authenticated with `workflow` and `repo` scopes. Zero CI runs on the branch.
Zero repository Actions secrets, from an authorised query, so `CREDENTIAL-PRESENT: false` is
proven rather than merely unobserved. Zero open pull requests.
