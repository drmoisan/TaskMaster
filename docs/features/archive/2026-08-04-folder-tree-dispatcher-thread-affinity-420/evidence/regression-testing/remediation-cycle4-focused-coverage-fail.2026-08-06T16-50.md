Timestamp: 2026-08-06T16-50
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/regression-testing/remediation-cycle4-focused-coverage.cobertura.xml`.
EXIT_CODE: 1
Output Summary: The exact eight-assembly coverage wrapper completed in 54.5998 seconds and produced `remediation-cycle4-focused-coverage.cobertura.xml` (17,446,553 bytes, written 2026-08-06T16:49:33). VSTest reported 6,149 total tests, 6,148 passed, and 1 failed. The wrapper removed its generated effective settings file in its `finally` block. No coverage runner remained after completion.

The equivalent eight-assembly non-coverage VSTest command completed successfully immediately afterward: 6,149/6,149 passed. The wrapper does not emit a TRX or diagnostic log, so the identity of the coverage-context-only failure was not retained in the wrapper output. The fresh Cobertura XML exists but is non-green coverage evidence; P5-T46 remains unchecked and no coverage threshold or denominator assertion is made from this run.
