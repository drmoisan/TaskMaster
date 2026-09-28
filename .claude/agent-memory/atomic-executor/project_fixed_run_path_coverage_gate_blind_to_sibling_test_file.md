---
name: fixed-run-path-coverage-gate-blind-to-sibling-test-file
description: A per-file coverage gate that fixes both the run path and the coverage path measures only the named test files, so a function tested in a sibling test file reads as uncovered and the gate fails on well-tested code
metadata:
  type: project
---

A new-code coverage gate that names its test files AND its coverage files explicitly (Pester
`Run.Path` = two test files, `CodeCoverage.Path` = two part files) measures the cross product of
exactly those. A function declared in a measured part file but exercised from a THIRD test file
outside `Run.Path` reads as fully uncovered.

**Why:** On #873 P7-T7 the projection part file read 82.5 percent against a floor of 90. Seven lines
were uncovered; five of them (187-194) were the entire body of `Test-RawCoverageDocumentRetained`,
which has three passing tests — in `Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`, which the
gate's fixed two-file run path excludes. The code was well tested; the measurement could not see it.

**How to apply:**
- Before concluding a per-file coverage shortfall is a real testing gap, list the uncovered line
  numbers and read those lines. Extract them with the JaCoCo `ci` attribute:
  `foreach ($l in $sf.SelectNodes('line')) { if ([int]$l.ci -eq 0) { $l.nr } }`.
- Attribute each uncovered region to one of: genuinely untested, or tested-but-out-of-run-path.
- Remediate only inside the Write Set. Prefer adding a test that closes a genuinely untested branch
  over duplicating a sibling file's tests into the measured file — a duplicate buys coverage but
  adds no assertion value. On #873 the cheapest correct fix was two tests: one for a second
  reconciliation equality that no test covered, one for a guard clause (empty parent directory) that
  the sibling file's three full-path tests could never reach.
- Budget lines: the measured test file is often already near the 500-line ceiling. Count first.
  `Invoke-MSTestWithCoverage.Projection.Tests.ps1` went 452 -> 495 for two tests.
- Remediating means the toolchain loop restarts from format. Re-run the earlier Phase 7 gates and
  append a `Pass 2` section to each artifact rather than overwriting pass 1; record the failing first
  measurement, see [[feedback_never_predict_an_observation_into_an_artifact]].

Related: [[project_coverage_firstparty_denominator_method]],
[[project_async_state_machine_emits_no_method_element]].
