# P8-T3 — AC19: the required checks re-run and pass on the post-repair head SHA

DEFERRED: credential or fixture absent

Timestamp: 2026-09-20T02-39

Command:

```
gh api repos/drmoisan/TaskMaster/actions/secrets --jq '[.secrets[].name] | sort'
gh pr list --repo drmoisan/TaskMaster --state open --json number,headRefName,author --jq '[.[] | select(.author.login == "app/dependabot")] | length'
```

EXIT_CODE: 0

## Output Summary

The deferred branch is taken. AC19 remains unchecked and is carried into the P8-T5 follow-up issue.

## The measurement that selected this branch

Quoted from `evidence/other/p8-t1-credential-availability.2026-09-19T09-44.md`:

```
QUERY1-EXIT: 0
QUERY1-OUTPUT-BEGIN
[]
QUERY1-OUTPUT-END
QUERY2-LENGTH: 0
```

`CREDENTIAL-PRESENT: false` and `DEPENDABOT-PR-COUNT: 0`. The live branch requires both
`CREDENTIAL-PRESENT: true` and a count greater than 0, so the deferred branch is selected. That
branch is explicitly authorised by the plan for this task.

## What the live branch would have run

```
gh api repos/drmoisan/TaskMaster/rulesets/18572843 --jq '[.rules[] | select(.type == "required_status_checks") | .parameters.required_status_checks[].context] | sort'
gh api repos/drmoisan/TaskMaster/commits/<HEAD_SHA>/check-runs --jq '.check_runs[] | {name, status, conclusion, details_url}'
gh api repos/drmoisan/TaskMaster/actions/runs/<run_id> --jq '.event'
```

Acceptance would be that the required-check list is derived at run time from the ruleset and has a
length greater than 0; that for every member of that list a check run exists on the post-repair head
SHA; that each run's originating event resolves to `pull_request`; that each conclusion is
`success`; and that no run carries `action_required` as a conclusion or `waiting` as a status.

The required-check count is read from the ruleset rather than hard-coded because a hard-coded figure
would pass silently if the ruleset gained or lost a context, and the greater-than-zero length
assertion is what prevents an empty derived list from satisfying the per-member check vacuously.

This is the criterion that falsifies a wrong trigger or credential choice: a check sourced from the
`workflow_run` event, or one parked awaiting approval, fails it. It cannot be evaluated without a
live fixture, which is why it is written as an outcome assertion rather than as an inspection of the
workflow file.

## What has to happen before it can run

The credential provisioning in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md`,
then an open Dependabot pull request to act as the fixture, then one repair run on it.

AC19 remains **unchecked** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
