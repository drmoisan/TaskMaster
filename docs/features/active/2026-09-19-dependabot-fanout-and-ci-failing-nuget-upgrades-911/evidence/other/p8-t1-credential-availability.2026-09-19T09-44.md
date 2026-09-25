# P8-T1 — Credential and fixture availability

Timestamp: 2026-09-20T02-36

Commands:

```
gh api repos/drmoisan/TaskMaster/actions/secrets --jq '[.secrets[].name] | sort'
gh pr list --repo drmoisan/TaskMaster --state open --json number,headRefName,author --jq '[.[] | select(.author.login == "app/dependabot")]'
gh pr list --repo drmoisan/TaskMaster --state open --json number,headRefName,author --jq '[.[] | select(.author.login == "app/dependabot")] | length'
```

EXIT_CODE: 0

## Output Summary

```
QUERY1-EXIT: 0
QUERY1-OUTPUT-BEGIN
[]
QUERY1-OUTPUT-END
QUERY2-EXIT: 0
QUERY2-OUTPUT-BEGIN
[]
QUERY2-OUTPUT-END
QUERY2-LENGTH-EXIT: 0
QUERY2-LENGTH: 0
OPEN-PR-COUNT: 0
```

No command wrote to stderr; each of the three exited 0.

## Measurements

| Field | Value |
|---|---|
| `SECRETS-QUERY` | `200 OK`, empty name list |
| Sorted secret-name list | `[]` |
| `CREDENTIAL-PRESENT` | **false** |
| Verbatim Dependabot pull-request array | `[]` |
| `DEPENDABOT-PR-COUNT` | **0** |
| Open pull requests of any author, recorded as context | 0 |

**`CREDENTIAL-PRESENT: false` is derived from a successful query 1, not from a failed one.** The
secrets endpoint requires admin permission and answers HTTP 403 to a caller that lacks it; a 403
would have been recorded as `SECRETS-QUERY: 403 FORBIDDEN` with `CREDENTIAL-PRESENT: unknown`,
because a forbidden query and an empty list are different states and only the second proves
absence. Here the query returned exit 0 with an empty array, so the repository genuinely holds no
Actions secret at all — neither `DEPENDABOT_REPAIR_APP_ID` nor
`DEPENDABOT_REPAIR_APP_PRIVATE_KEY`.

`DEPENDABOT-PR-COUNT: 0` was read from the `| length` form of the same filtered expression, and the
unfiltered open-pull-request count is also 0, so the zero is not an artefact of the author filter.
The earlier `gh api "repos/drmoisan/TaskMaster/pulls?state=open" --jq '[.pull_requests?] | length'`
formulation is prohibited by the plan and was not used: `/pulls` returns a bare array, so
`.pull_requests?` yields empty on every possible repository state and that expression evaluates to
0 unconditionally.

## Branch selection for P8-T2, P8-T3 and P8-T4

The live branch is taken when and only when `CREDENTIAL-PRESENT: true` **and**
`DEPENDABOT-PR-COUNT` is greater than 0. Measured: `false` and `0`. **Both conditions fail
independently**, so the deferred branch is selected for all three tasks. The deferred branch is
explicitly authorised by the plan; it is not a skip, and each of the three tasks records its own
measurement, names the runbook and states that its criterion remains unchecked.
