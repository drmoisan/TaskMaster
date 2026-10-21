Timestamp: 2026-09-03T11-09

Coverage delta verification for scripts/vscode/Invoke-MSTestWithCoverage.ps1:

- [P0-T7] baseline Percent=90.09 (MainScriptCommands=111 Executed=100)
- [P4-T3] final Percent=90.09 (MainScriptCommands=111 Executed=100)

The post-change Percent= (90.09) is greater than or equal to the baseline Percent= (90.09): no
regression (delta = 0.00 percentage points; identical MainScriptCommands total, confirming the
statement reorder did not add or remove any measurable commands).

Name-check: the swapped lines (the Set-Content and Assert-CoberturaLineCoverageThreshold calls,
at drifted-anchor lines 342/344) are among the commands counted as executed in the [P4-T3] run.
Both tests that exercise them are present in that run's Output Summary with Result=Passed:
- Invoke-MSTestWithCoverageMain.collects and post-processes coverage on the fully mocked main happy path
  (pre-existing test; per [P3-T1]'s pass-after-run artifact, this test's Set-Content and
  Assert-CoberturaLineCoverageThreshold call sites are both invoked exactly once on the happy
  path).
- Invoke-MSTestWithCoverageMain.persists the post-processed Cobertura document before the
  threshold assertion can throw on a sub-threshold run (the new [P1-T1] test; exercises the
  Set-Content call site on the sub-threshold/throwing path).

Outcome: PASS (post-change percent is not lower than baseline).
