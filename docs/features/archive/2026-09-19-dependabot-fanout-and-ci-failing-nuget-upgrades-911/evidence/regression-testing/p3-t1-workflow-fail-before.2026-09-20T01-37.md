# Phase 3 Fail-Before — Four Workflow Assertions, All Red

- Timestamp: 2026-09-20T08-56-13
- Task: [P3-T1] **[expect-fail]**
- Findings: R3, R6, R7, R8
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/dependencies/DependabotConfig.Tests.ps1`,
  four invocations
- EXIT_CODE: 1 for all four
- ExpectedExitCode: 1

**Four failures is the fail-before evidence for the whole phase.** A passing assertion here would
mean the finding it encodes is not present in the workflow.

## Gate Rule 20 — Verification Route and Residual

`.github/workflows/dependabot-repair.yml` has never executed, and [P0-T13] established why: the
repository holds **zero** Actions secrets, so the workflow's first step cannot mint the
installation token it needs, and there are **zero** open pull requests for its `workflow_run`
trigger to fire against.

**Verified without a live run by these four tests:** the workflow's own text — which step carries
which condition, which quantity that condition reads, which literals the file contains and which
it does not. Each can fail, and all four do fail today.

**Unverifiable until the #914 credential exists:** everything about runtime behaviour. These tests
observe the file, not a run of it.

## Run 1 — `*R3- gates*`

```
PESTER Passed=0 Failed=1 Skipped=0 Executed=1 Total=16 NotRun=15
```

Failure message, verbatim:

```
Expected the actual value to be greater than 0, because the repair step must publish the write-set count as a step output, but got 0.
```

The workflow publishes `repair-count`, `beyond-known-weak`, `skip-count` and `report-path`. It
publishes **no** `written-count`, so the count of lines containing `written-count=` is 0.

The test's earlier guards passed before this clause failed: the commit step block was found
non-empty, and it carries exactly one `if:` line. The failure is therefore about the quantity the
gate reads, not about the test's ability to find the step.

## Run 2 — `*R6- guards*`

```
PESTER Passed=0 Failed=1 Skipped=0 Executed=1 Total=16 NotRun=15
```

Failure message, verbatim:

```
Expected 1, because the disclosure step must be guarded, but got 0.
```

The disclosure step carries **no** `if:` line at all. The step block was found non-empty, so the
zero is a property of the workflow and not of the parser.

## Run 3 — `*R7- counts*`

```
PESTER Passed=0 Failed=1 Skipped=0 Executed=1 Total=16 NotRun=15
```

Failure message, verbatim, first line:

```
Expected like wildcard '*Where-Object { $_ -ne 'Analyzer' }*' to match 'name: dependabot-repair
```

The file's filter today reads
`Where-Object { $_ -ne 'Analyzer' -and $_ -ne 'BindingRedirect' }`, so the single-clause fragment
is absent. The reachability comment is absent too.

## Run 4 — `*R8- derives*`

```
PESTER Passed=0 Failed=1 Skipped=0 Executed=1 Total=16 NotRun=15
```

Failure message, verbatim, first line:

```
Expected like wildcard '*steps.app-token.outputs.app-slug*' to match 'name: dependabot-repair
```

The commit step sets `user.name` and `user.email` to hand-written literals and reads nothing from
the token step's outputs.

## Acceptance

| Run | Filter | `Executed` | `Failed` | `EXIT_CODE` | Result |
|---|---|---|---|---|---|
| 1 | `*R3- gates*` | 1 | 1 | 1 | PASS |
| 2 | `*R6- guards*` | 1 | 1 | 1 | PASS |
| 3 | `*R7- counts*` | 1 | 1 | 1 | PASS |
| 4 | `*R8- derives*` | 1 | 1 | 1 | PASS |

Every `Executed` figure is `Passed + Failed + Skipped`. `Total` is 16 in all four and is recorded
as context only: it counts the whole file's `It` blocks and is invariant under the filter.

## A First Form of Two Tests Failed for the Wrong Reason

Recorded because a red that is not the intended red is worthless as fail-before evidence.

The first form of the `Get-WorkflowStepBlock` helper declared `[Parameter(Mandatory = $true)]
[string[]]$Line`. A mandatory `[string[]]` rejects an array containing a blank element, and a
workflow file is full of blank lines, so runs 1 and 2 failed with:

```
Cannot bind argument to parameter 'Line' because it is an empty string.
```

That is a binding error, not an assertion failure: both tests would have stayed red after the
Phase 3 edits and the fail-before-and-pass-after pair at [P3-T8] could never have closed.
`[AllowEmptyString()]` was added to the parameter and both runs then failed on their substantive
clauses, as shown above.

The pre-existing `Get-DependabotGroupKey` helper in the same file carries the same declaration and
works, because `.github/dependabot.yml` has no blank line inside the region it parses.

## Output Summary

Four filtered runs, four failures, exit 1 each, each with its failure message quoted. R3 fails on
the absent `written-count=` output, R6 on the absent disclosure guard, R7 on the two-clause filter
and the absent reachability comment, R8 on the absent `app-slug` reference. [P3-T8] records the
pass-after half for all four.
