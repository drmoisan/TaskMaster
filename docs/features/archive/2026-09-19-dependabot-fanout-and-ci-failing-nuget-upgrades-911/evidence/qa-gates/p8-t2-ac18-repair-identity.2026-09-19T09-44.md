# P8-T2 — AC18: the repair commit is pushed under the GitHub App identity

DEFERRED: credential or fixture absent

Timestamp: 2026-09-20T02-38

Command:

```
gh api repos/drmoisan/TaskMaster/actions/secrets --jq '[.secrets[].name] | sort'
gh pr list --repo drmoisan/TaskMaster --state open --json number,headRefName,author --jq '[.[] | select(.author.login == "app/dependabot")] | length'
```

EXIT_CODE: 0

## Output Summary

The deferred branch is taken. AC18 remains unchecked and is carried into the P8-T5 follow-up issue.

## The measurement that selected this branch

Quoted from `evidence/other/p8-t1-credential-availability.2026-09-19T09-44.md`:

```
QUERY1-EXIT: 0
QUERY1-OUTPUT-BEGIN
[]
QUERY1-OUTPUT-END
QUERY2-LENGTH: 0
```

| Field | Value | Live-branch requirement |
|---|---|---|
| `CREDENTIAL-PRESENT` | false | true |
| `DEPENDABOT-PR-COUNT` | 0 | greater than 0 |

Both fail, and each fails on its own. The live branch is taken when and only when both hold, so the
deferred branch is selected. That branch is explicitly authorised by the plan for this task.

## What the live branch would have run

After a repair run on the fixture Dependabot pull request:

```
gh api repos/drmoisan/TaskMaster/pulls/<PR> --jq '.head.sha'
gh api repos/drmoisan/TaskMaster/commits/<HEAD_SHA> --jq '.author.login'
```

Acceptance would be that the recorded head SHA differs from the pre-repair SHA, and that the
recorded login ends with `[bot]` and is not `github-actions[bot]`. That is what distinguishes a push
made under the App installation identity from a fallback to the default Actions token, which is the
failure mode AC19 would otherwise diagnose only indirectly.

## What has to happen before it can run

A repository admin must create the GitHub App, install it on `drmoisan/TaskMaster` with contents
and pull-requests write, and store `DEPENDABOT_REPAIR_APP_ID` and
`DEPENDABOT_REPAIR_APP_PRIVATE_KEY` as repository secrets, following
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md`.
A Dependabot pull request must then be open to serve as the fixture; the repository currently has
none open, and none of any author.

AC18 remains **unchecked** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
