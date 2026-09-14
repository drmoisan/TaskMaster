# Check-run context names (P8-T5)

Timestamp: 2026-09-14T20-46

Status: CAPTURED FROM A LIVE RUN

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


## Live capture (AC-23 discharged)

Timestamp: 2026-09-14T19-40

The bootstrap condition described above has cleared: pull request 897 is open against `main` for
this branch, all six of its check runs have completed, and the head SHA is therefore resolvable
with check runs attached. The prescribed query was re-run by the coordinator, unmodified, against
that head.

Head SHA queried: `df4c247df90176e17bf0ac5e247290113d06ba32`

Command: `gh api repos/drmoisan/TaskMaster/commits/df4c247df90176e17bf0ac5e247290113d06ba32/check-runs --jq '.check_runs[].name'`
EXIT_CODE: 0
Returned list of names:

```
build-nullable / Build with nullable warnings treated as errors
mstest-coverage / Run MSTest suite with coverage
actionlint / actionlint
format-check / Verify formatting
build-analyzers / Build with analyzers and code style enforcement
pester / Run Pester suite with coverage
```

Corroborating query: `gh api repos/drmoisan/TaskMaster/commits/df4c247df90176e17bf0ac5e247290113d06ba32/check-runs --jq '.total_count'`
Output: `6`

## Outcome for the prediction

The predicted string `pester / Run Pester suite with coverage` **IS PRESENT** in the returned list.
The prediction was derived mechanically from the `<caller job id> / <callee job name>` convention
the workflow README states, and the live capture confirms it. It is no longer labelled predicted.

Exactly ONE context was added. The five pre-existing contexts — `actionlint / actionlint`,
`format-check / Verify formatting`, `build-analyzers / Build with analyzers and code style
enforcement`, `build-nullable / Build with nullable warnings treated as errors`, and
`mstest-coverage / Run MSTest suite with coverage` — all report under unchanged names, confirming
that placing the C# threshold assertion inside the existing `mstest-coverage` callee added no
context of its own.

The context strings above were taken from the output of the prescribed `gh api` query and were not
hand-written, and were not transcribed from `gh pr checks`, which is a different command producing
a differently-derived list.

## Consequence for the acceptance criteria

AC-23 is now **satisfied and checked off**. It was never unsatisfiable: it required only that a
pull-request run exist, which is a bootstrap condition rather than a structural impossibility, and
it was discharged the moment that condition cleared. This is deliberately distinguished from the
criteria elsewhere in this run that were disclosed rather than discharged — AC16 on item 742, AC19
on item 879, and AC22 on item 871 — each of which could not be satisfied at all.
