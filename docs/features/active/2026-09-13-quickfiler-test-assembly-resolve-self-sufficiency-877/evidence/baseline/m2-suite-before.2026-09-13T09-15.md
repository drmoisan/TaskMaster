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
