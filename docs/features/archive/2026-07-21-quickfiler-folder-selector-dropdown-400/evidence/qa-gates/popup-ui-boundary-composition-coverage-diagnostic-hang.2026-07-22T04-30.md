# Popup UI-boundary composition coverage diagnostic hang

Timestamp: 2026-07-22T04:30:53.4359498Z

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-popup-ui-boundary-composition.2026-07-22T04-27.cobertura.xml`

EXIT_CODE: -1

Output Summary: This first full-repository P5-T27 attempt is diagnostic only and is not accepted as coverage evidence. It discovered 8 test assemblies and began the non-LiveOutlook suite, but encountered pre-P6/P7 failures including pending-open close and subfolder-activation contracts and then stopped making progress in the known no-pump lifecycle harness. After more than three minutes with `vstest.console` PID 67812 fixed at CPU 0.61 while child `testhost` PID 61372 remained active, the exact owned process tree was verified by command line and terminated: testhost 61372, vstest 67812, and its `dotnet-coverage` parent 66260. All three reported `RUNNING_AFTER=False`. The wrapper then exited -1 with `MSTest with coverage failed with exit code -1`. No Cobertura file was produced. P5-T27 remains open and must be rerun after the blocking P5 ownership correction and the planned P6 lifecycle harness correction.

Known observed failures included `ToggleAndEscapeWhileOpenIsPending_EachClosesHostExactlyOnce`, `AutomaticSelectorCloseWhileOpenIsPending_ClosesHostExactlyOnce`, and the not-yet-implemented subfolder activation follow-up cases. These are not treated as P5 composition acceptance evidence.
