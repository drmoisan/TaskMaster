# Phase 0 — Baseline Coverage Document (Issue #797)

Timestamp: 2026-09-07T09-21

Command: `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Coverage -FilterName All -OutputPath coverage/plan797-baseline/coverage.cobertura.xml -ResultsDirectory coverage/plan797-trx/baseline-coverage`

The helper invoked `dotnet-coverage collect --output <path> --output-format cobertura --settings
<derived> -- <vstest.console.exe> <two assemblies> /Settings:<cli runsettings> /InIsolation
/TestCaseFilter:<All filter> /Logger:trx /ResultsDirectory:<results dir>`.

EXIT_CODE: 0

ExpectedExitCode: 0

The clean branch applies. The collected run reproduced no failure at all: its results file records
5237 total, 5237 executed, 5237 passed, 0 failed, so the coverage collection had no inner test-run
exit code to propagate.

## Output Summary

LINES_COVERED=44426 LINES_VALID=83466 BRANCHES_COVERED=10877 BRANCHES_VALID=24323 BASELINE_LINE_PERCENT=53.23 BASELINE_ASSEMBLY_SCOPE=UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll,TaskMaster.Test/bin/Debug/TaskMaster.Test.dll

The document-level branch rate, recorded as a non-asserted observation, is 44.72 percent.

Both test assemblies were excluded from instrumentation by the derived coverage settings: the helper
reads the canonical coverage.config in memory, retains every existing third-party module exclusion,
and adds exactly one further module exclusion matching any module name ending in `.Test.dll`. The
canonical settings file was not written. The denominator therefore holds production code only.

No field above carries the text UNVERIFIED or any placeholder; every value is a concrete integer or a
two-decimal number read from the Cobertura document root.

This step records values and asserts no threshold. The repository's own 80 percent assertion is
written against a full-suite denominator, and this run's denominator is narrower, so the comparison
belongs in P5-T7.

Output Summary: Baseline coverage collected cleanly. 44426 of 83466 lines covered, giving a
document-level line rate of 53.23 percent; 10877 of 24323 branches covered. The collected test run was
green at 5237 passed and 0 failed.
