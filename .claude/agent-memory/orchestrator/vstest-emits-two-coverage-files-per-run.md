---
name: vstest-emits-two-coverage-files-per-run
description: A plan step that requires the '*.coverage' search under a vstest /ResultsDirectory to return exactly one file is unsatisfiable — /InIsolation emits the published attachment AND an in-run In\<machine>\ copy
metadata:
  type: project
---

`vstest.console.exe ... /EnableCodeCoverage /InIsolation /ResultsDirectory:<dir>` leaves **two**
`*.coverage` files under `<dir>`: the published attachment, and an in-run copy under
`In\<machine>\`. Their byte lengths differ slightly (observed 21356385 vs 21356197 on the same run).

So a plan task written as "locate the attachment with `Get-ChildItem -Path <dir> -Recurse -Filter
'*.coverage'`; that search must return exactly one file; if it returns zero or more than one, do not
convert an arbitrary member — record `COVERAGE_CAPTURE_BLOCKED`" can **never** pass. Observed on
issue #751, 2026-09-03: both the baseline capture (P0-T17) and the final-QC capture (P4-T12) recorded
`Rung: 3` / `COVERAGE_CAPTURE_BLOCKED` with an observed count of 2, so the plan produced no numeric
coverage pair at all and the Coverage Evidence Contract came out remediation-required.

**Why:** the "exactly one" precondition was written to stop an executor converting an arbitrary member
of an ambiguous set — a sound instinct. But the set is *always* ambiguous under `/InIsolation`, and
the guard was authored without observing a successful run's output directory. That is precisely the
defect class the atomic-plan-contract's "Observe a command's success-case output before asserting over
that output" rule exists to prevent, and it survived five preflight rounds because the reviewer read
the guard as conservative rather than as unsatisfiable.

**How to apply:** when reviewing or authoring any plan that converts a `.coverage` attachment, require
a *disambiguation rule* rather than a cardinality assertion — for example, exclude the `In\` subtree
and take the attachment published directly under the TRX's attachment folder, and state that rule in
the task text. Treat "must return exactly one" over a tool-generated output directory as a smell:
check what the tool actually writes there first. Related: [[preflight-catches-vacuous-gates]],
[[absence-from-failure-list-is-not-a-pass-gate]], [[csharp-coverage-denominator-two-figures]],
[[coverage-mode-raw-vs-processed-is-flake-sensitive]].
