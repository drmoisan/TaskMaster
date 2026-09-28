---
name: per-file-coverage-gate-excludes-sibling-test-file
description: A per-file coverage task that fixes the test-file list under-reports any function whose tests live in a sibling test file, forcing a needless remediation inside the final QA loop
metadata:
  type: feedback
---

A per-file coverage acceptance task that names a FIXED set of test files, and measures
coverage of production files exercised by more than that set, under-reports. The uncovered
lines it reports are not untested — they are tested from a test file the fixed run path
excludes.

**Why:** On #873 P7-T7 the task fixed the run to two test files and two production files.
`Invoke-MSTestWithCoverage.Projection.ps1` measured 82.50 percent and failed the 90 gate.
Five of its seven uncovered lines were the body of `Test-RawCoverageDocumentRetained`, which
is well covered — but from `Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`, outside the
fixed pair. The gate was measuring run-path scope, not test adequacy. The executor closed it
by adding two genuinely new tests to the in-scope test file, which is a correct repair but
cost a full restart of the toolchain loop from task 1.

**How to apply:** When reviewing a plan that carries a per-file coverage gate, check that the
fixed test-file list is a superset of every test file that exercises the measured production
files. If it is not, either widen the run path or state the expected shortfall in the task
text. Do this at preflight — once the gate fails in the final QA loop, every preceding gate
has to be re-run. Related: [[csharp-coverage-denominator-two-figures]],
[[feedback_repowide_coverage_run_full_suite]].

Note the healthy pattern the executor used: it recorded BOTH the failing 82.50 measurement
and the passing 92.50 one in the same artifact rather than overwriting the failure. Preserve
that; a coverage artifact showing only the passing figure hides the remediation.
