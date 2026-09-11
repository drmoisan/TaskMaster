---
name: 645-review-residuals
description: quickfiler-session-metrics-twelve-hour-time-format-645 (full-bug, hh:mm -> HH:mm) review outcome and residual findings
metadata:
  type: project
---

Closed 10/10 AC PASS, 1 blocking finding (evidence hygiene only, not code): the branch's own
committed `coverage-baseline.cobertura.xml` + `coverage-final.cobertura.xml`
(evidence/baseline + evidence/qa-gates, ~311k lines each) each leak the operator's account name
and absolute worktree path 2,007 times via `<class filename="...">` attributes — see
[[_shared_no_absolute_host_paths]]. Recommended remediation: redact both files and squash-merge
(a sanitizing commit alone leaves the original blob reachable in history, per
[[project_cobertura-substitution-leaves-blobs-in-history]]).

**Why:** the plan (v1.0) explicitly reasoned about avoiding a TRX host-name leak two tasks earlier
in the same phase, but the same discipline was not applied to the much larger Cobertura artifact —
awareness of one leak class doesn't generalize to a sibling leak class in the same evidence set.

**How to apply:** for any C# feature/bug branch that commits a raw Cobertura (or any coverage-tool
XML) report as evidence, grep it for the reviewing account name (`git grep -c "<account>" <file>`)
before accepting the coverage evidence — don't assume TRX-avoidance reasoning in the plan means the
Cobertura file is also clean.

The repo-wide C# coverage floor (23.8225%, Delta = 0.0000 pp baseline vs final) was correctly
carved out by the task as a pre-existing, non-blocking condition — matches the pattern in
[[project_build-ci-coverage-gate-fidelity-epic-outcome]] (80-vs-85 floor doc conflict, unreconciled;
this review cited whichever floor the artifact was compared against and reported FAIL/non-blocking
either way since Delta = 0).

AC9 ("full toolchain pass, no failures in final pass") was marked PASS-with-documented-deviation
rather than plain PASS, because the coverage wrapper script's own threshold assertion throws
(`EXIT_CODE: 1`) even though the underlying vstest run is fully green (1312/1312) — worth
distinguishing a script's own non-zero exit from the substantive test-run result it wraps when the
task has pre-authorized a specific carve-out.
