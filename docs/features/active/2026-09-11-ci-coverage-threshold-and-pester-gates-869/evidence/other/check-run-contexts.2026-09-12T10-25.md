# Check-run context names (P8-T5)

Timestamp: 2026-09-14T20-46

Status: PENDING LIVE RUN

## The command prescribed by the workflow README

`.github/workflows/README.md` prescribes this query in step 2 of its branch-protection rename procedure, and forbids hand-writing the context strings:

```
gh api repos/drmoisan/TaskMaster/commits/<head-sha>/check-runs --jq '.check_runs[].name'
```

## What was run, and against what

Head SHA queried: `1e2550623b561040f3dd8e93f44f7bb9347608cb`

That SHA is the branch head at the time of this task, obtained with `git -C "<repo-root>" rev-parse HEAD`, and it was pushed to `origin` before the query so the SHA would be resolvable by the API.

Command: `gh api repos/drmoisan/TaskMaster/commits/1e2550623b561040f3dd8e93f44f7bb9347608cb/check-runs --jq '.check_runs[].name'`
EXIT_CODE: 0
Returned list of names: **empty**.

Corroborating query: `gh api repos/drmoisan/TaskMaster/commits/1e2550623b561040f3dd8e93f44f7bb9347608cb/check-runs --jq '.total_count'`
Output: `0`

An earlier attempt against the same SHA before the push returned HTTP 422 `No commit found for SHA`, which confirms the later queries were run against a SHA the API could resolve and that the empty result is a genuine absence of check runs rather than an unresolvable reference.

## Why no run exists at execution time

Command: `gh pr list --repo drmoisan/TaskMaster --head bug/ci-coverage-threshold-and-pester-gates-869 --state all --json number,state,headRefName`
EXIT_CODE: 0
Output: `[]`

No pull request exists for this branch. `.github/workflows/ci.yml` declares its triggers as `push` restricted to the branches `main` and `development`, `pull_request` restricted to the same two branches, and `workflow_dispatch`. A push to this feature branch therefore matches no trigger, and with no pull request open the `pull_request` trigger has not fired either. No CI run has been produced against this head SHA, which is why the returned list is empty.

Opening the pull request is outside this executor's scope: PR authoring is handled separately, and this executor was directed not to run any `gh pr` command that creates or edits one.

## Exact command to run once the run completes

Once the pull request is open and its run has completed, re-run, substituting the then-current head SHA:

```
gh api repos/drmoisan/TaskMaster/commits/<head-sha>/check-runs --jq '.check_runs[].name'
```

## Predicted string, labelled as predicted and not confirmed

**Predicted, not confirmed:**

```
pester / Run Pester suite with coverage
```

The prediction is derived mechanically from the convention the README states, `<caller job id> / <callee job name>`, applied to values that are verifiable in the tree today:

- the caller job id is `pester`, from the job key added to `.github/workflows/ci.yml` by P7-T5;
- the callee job name is `Run Pester suite with coverage`, from the `name:` of the single job inside `.github/workflows/_pester.yml` written by P7-T3.

Whether the returned list contains the predicted string is therefore **not yet determined**, because the returned list is empty. It cannot be stated to contain it and it cannot be stated not to contain it.

The five existing contexts are expected to continue reporting under unchanged names, because this delivery changes no existing job key and no existing job name. The C# threshold assertion was placed inside the existing `mstest-coverage` callee precisely so that it adds no context of its own.

## Consequence for the acceptance criteria

Acceptance criterion AC-23 requires the actual context name to be captured from a live run and recorded. That capture has not been made. AC-23 is therefore **left unchecked**, with this artifact as the recorded reason, per the authorized branch of this task and of P9-T27. The criterion line in the specification is left byte-identical.
