# CI Toolchain Run at Head — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-28-41
- Task: [P6-T2]
- Finding: R1, **Blocking**
- EXIT_CODE: 0

## Branch Taken

[P0-T13] recorded **`GH-AVAILABLE: true`**, so this task takes the **dispatch** branch. The
branch is named explicitly here, as the acceptance requires.

`CI-DISPATCH: performed.` Neither branch records `EXIT_CODE: SKIPPED`.

## Dispatch

```
gh workflow run CI --ref bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911
```

Verbatim output:

```
https://github.com/drmoisan/TaskMaster/actions/runs/35513025198
```

Exit code **0**.

`.github/workflows/ci.yml:8` declares `workflow_dispatch` alongside `push` and `pull_request`,
which is what makes this route available.

## Poll to Completion

```
gh run list --workflow=ci.yml --branch bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911 --limit 5 --json databaseId,headSha,conclusion,status,event
```

Polled at 60-second intervals. Exit code **0** on every invocation.

```
POLL 09:17:06 status=in_progress conclusion=
POLL 09:18:07 status=in_progress conclusion=
POLL 09:19:07 status=in_progress conclusion=
POLL 09:20:08 status=in_progress conclusion=
POLL 09:21:08 status=in_progress conclusion=
POLL 09:22:09 status=in_progress conclusion=
POLL 09:23:09 status=in_progress conclusion=
POLL 09:24:10 status=completed conclusion=success
```

Final list output, verbatim:

```json
[{"conclusion":"success","databaseId":35513025198,"event":"workflow_dispatch","headSha":"de9a00106c951a073c1ac33a4cf5223e24563cd8","status":"completed"}]
```

## The Run

| Field | Value |
|---|---|
| `databaseId` | **35513025198** |
| `headSha` | **`de9a00106c951a073c1ac33a4cf5223e24563cd8`** |
| `event` | `workflow_dispatch` |
| `status` | `completed` |
| `conclusion` | **`success`** |

| Clause | Required | Measured | Result |
|---|---|---|---|
| Recorded `headSha` equals `H1` | `de9a0010...` | **`de9a0010...`** | PASS |
| `conclusion` | `success` | **`success`** | PASS |
| All six jobs recorded individually | yes | **yes**, below | PASS |

## Per-Job Conclusions, Verbatim

```
gh run view 35513025198 --json jobs
```

```json
{"conclusion":"success","name":"build-analyzers / Build with analyzers and code style enforcement","status":"completed"}
{"conclusion":"success","name":"format-check / Verify formatting","status":"completed"}
{"conclusion":"success","name":"mstest-coverage / Run MSTest suite with coverage","status":"completed"}
{"conclusion":"success","name":"build-nullable / Build with nullable warnings treated as errors","status":"completed"}
{"conclusion":"success","name":"pester / Run Pester suite with coverage","status":"completed"}
{"conclusion":"success","name":"actionlint / actionlint","status":"completed"}
```

| # | Job | Conclusion |
|---|---|---|
| 1 | `actionlint` | **success** |
| 2 | `format-check` | **success** |
| 3 | `build-analyzers` | **success** |
| 4 | `build-nullable` | **success** |
| 5 | `mstest-coverage` | **success** |
| 6 | `pester` | **success** |

**Six of six green.** No job failed, so this task does not return the cycle to Phase 5.

## What This Run Does and Does Not Establish

This is a **`workflow_dispatch`** run, not a **`pull_request`** run. The `event` field records
the difference and this artifact states it rather than implying otherwise.

**What it establishes.** The six CI gates execute green against the exact commit `H1`, on a
clean runner checkout. That matters most for the four gates this branch changed:

- `pester` runs the suite this cycle extended, against the widened scan and coverage paths, on a
  machine that is not this one;
- `build-analyzers` and `build-nullable` run the pinned NuGet CLI against a cold package cache,
  which is the condition issue #898 was about;
- `actionlint` validates the six workflow files, including the four regions Phase 3 rewrote;
- `format-check` runs the manifest-pinned CSharpier against the `.csharpierignore` scope this
  change widened.

**What it does not establish.** It is not the PR-context run. The `modified-workflow-needs-green-run`
rule the review cited demands a green run at the exact commit being merged, and the merge head
is not yet known: [P6-T4] produces `H2` and a pull request may carry further commits.

**The authoritative discharge of R1 remains the PR-context `CI` run at the merge head**, which
the orchestrator records at pull-request time. This run is the evidence that the six gates pass
at head today, recorded so the result is known before the pull request is opened rather than
after.

[P6-T5] closes the gap between `H1` and `H2` by requiring the intervening diff to be
documentation only.

## Gate Rule 20 — Verification Route and Residual

**Verified by a live run:** the six CI gates at `H1`.

**Still unverifiable until the #914 credential exists:** everything about
`.github/workflows/dependabot-repair.yml` at run time. That workflow is **not** one of the six
jobs above. It triggers on `workflow_run` completion for a branch under `dependabot/`, and it
mints an installation token from two secrets the repository does not hold. A green `CI` run does
not execute it. The four residuals at [P5-T12] are unchanged by this run.

## Output Summary

`CI` dispatched against the branch and polled to completion. Run **35513025198**,
`headSha` equal to `H1`, `event` `workflow_dispatch`, `conclusion` **success**, all six jobs
green and recorded individually. The authoritative R1 discharge remains the PR-context run at
the merge head.
