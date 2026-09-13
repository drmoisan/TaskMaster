# M2 baseline: QuickFiler.Test alone, with runsettings, before any fix

Timestamp: 2026-09-13T09-15
Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:<scratch>/m2-pre /Logger:"trx;LogFileName=m2-pre.trx"`
EXIT_CODE: 0
Output Summary: Test Run Successful. Total tests: 1395, Passed: 1395, Failed: 0. Total time 12.2264 seconds. Runsettings in force: `Workers=0`, `Scope=ClassLevel`, unmodified. Head: c9590a8b7, before any fix.

## Significance

`UtilitiesCS.Test` was not part of this run, so its `[AssemblyInitialize]` never executed and the
`AssemblyResolve` handler it installs was never present. The three Deedle-using tests nevertheless
passed. This refutes the original attribution of the defect to sibling-assembly initialiser ordering: a
handler that is not installed cannot be the thing that satisfies the bind.

Read together with the M3 run at `evidence/regression-testing/m3-fail-before.2026-09-13T09-14.md`, which
fails, this localises the defect to ordering *within* `QuickFiler.Test`. The rescuer is the lazy
`SVGControl` handler installed from the `SVGControl.SvgRenderer` static constructor, reached through the
SVG-bearing QuickFiler viewers.

The suite outcome is order-dependent and therefore not a reliable repro. This run passing does not
contradict the defect report; it is the expected behaviour whenever an SVG-bearing class is scheduled
before the Deedle-using class.

## Prior-session observation, disagreeing

- A prior session ran the same command shape recorded in the headline row above, against the same
  assembly and the same runsettings file, and selected the same total of 1395 tests.
- That prior-session run observed 1392 passed with 3 FAILED and a non-zero exit.
- The run recorded in the headline row above observed 1395 passed with 0 failed and a zero exit.
- The prior-session run is the one the M-matrix in `issue.md` labels the FIRST observation; the headline
  row of this artifact records the later, second observation.
- The two observations disagree. Both selected 1395 tests, so the run shape was identical and only the
  outcome differed.
- M2 is therefore NONDETERMINISTIC across runs. It is a regression check only, not a discriminator for
  this fix. A passing M2 run cannot confirm the fix and a failing M2 run cannot refute it. The stable
  discriminator is the M3 single-class run with no runsettings file.
