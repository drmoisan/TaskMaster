# Phase 3 Pass-After — Six Runs, All Green

- Timestamp: 2026-09-20T09-00-29
- Task: [P3-T8]
- Findings: R3, R6, R7, R8
- Command: CMD-PESTER-FILTERED, six invocations
- EXIT_CODE: 0 for all six

## The Six Runs

| # | File | Filter | `Passed` | `Failed` | `Executed` | `Total` | `NotRun` | `EXIT_CODE` |
|---|---|---|---|---|---|---|---|---|
| 1 | `DependabotConfig.Tests.ps1` | `*R3- gates*` | 1 | 0 | **1** | 17 | 16 | **0** |
| 2 | `DependabotConfig.Tests.ps1` | `*R6- guards*` | 1 | 0 | **1** | 17 | 16 | **0** |
| 3 | `DependabotConfig.Tests.ps1` | `*R7- counts*` | 1 | 0 | **1** | 17 | 16 | **0** |
| 4 | `DependabotConfig.Tests.ps1` | `*R8- derives*` | 1 | 0 | **1** | 17 | 16 | **0** |
| 5 | `Repair-PackageManifestConsistency.Tests.ps1` | `*R3- reports a non-zero write count*` | 1 | 0 | **1** | 31 | 30 | **0** |
| 6 | `DependabotConfig.Tests.ps1` | `*R6- replaces rather than appends*` | 1 | 0 | **1** | 17 | 16 | **0** |

Six runs, `Executed=1` and `Passed=1` each, exit 0 each. Every `Executed` figure is
`Passed + Failed + Skipped`; `Total` and `NotRun` are context only.

## Fail-Before and Pass-After, Per Finding

| Finding | Assertion | [P3-T1] failure message | [P3-T8] |
|---|---|---|---|
| **R3** | `R3- gates the commit step on the write-set count rather than the repair count` | `Expected the actual value to be greater than 0, because the repair step must publish the write-set count as a step output, but got 0.` | **Passed** |
| **R6** | `R6- guards the disclosure step and replaces a delimited block` | `Expected 1, because the disclosure step must be guarded, but got 0.` | **Passed** |
| **R7** | `R7- counts beyond-known-weak repairs with the analyzer exclusion alone` | `Expected like wildcard '*Where-Object { $_ -ne 'Analyzer' }*' to match 'name: dependabot-repair` | **Passed** |
| **R8** | `R8- derives the commit identity from the token step outputs` | `Expected like wildcard '*steps.app-token.outputs.app-slug*' to match 'name: dependabot-repair` | **Passed** |

All four workflow findings have a red-before and a green-after against the same assertion, the
same file and the same filter.

## The Two Behavioural Assertions

Runs 5 and 6 are not part of the fail-before pair and do not claim to be.

**Run 5, `R3- reports a non-zero write count`.** It tests the composition root, which already
returned `WrittenPath` correctly before this cycle; the defect was the workflow's choice of
gating quantity. It is the load-bearing half of R3 because it exhibits the run in which
`RepairCount` is 0 and the write-set count is 1 — the silent-discard case. It was green before
[P3-T2] and is green after it, and [P3-T3] records that plainly.

**Run 6, `R6- replaces rather than appends`.** It applies the workflow's own literal pattern to a
synthetic body and asserts one block results. Its containment assertion binds it to the
workflow's expression, so it would have failed had [P3-T4] written a different pattern.

## Gate Rule 20 — Verification Route and Residual

**Verified without a live run:** the workflow's text, by four named assertions each shown red
before the corresponding edit and green after it; and two composed behaviours, the write-set
quantity and the strip-then-append expression, by unit assertion over in-memory inputs.

**Unverifiable until the #914 credential exists**, unchanged by these six runs:

1. that `actions/create-github-app-token@v3` publishes an `app-slug` output;
2. that the resolved bot user id produces a commit whose `author.login` ends `[bot]`;
3. that the push causes the required checks to re-run on the post-repair head SHA;
4. that the disclosure edit produces exactly one block on a real pull-request body.

Six green filtered runs do not touch any of the four. The workflow has still never executed.

## Output Summary

Six filtered runs, all `Executed=1 Passed=1` at exit 0. Every one of the four workflow findings
is tabulated with its [P3-T1] failure message beside its pass, which is the fail-before and
pass-after pair for the whole phase.
