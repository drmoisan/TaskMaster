# P5-T6 — Post-change Coverage Document (Issue #797)

Timestamp: 2026-09-07T10-08

Command: `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Coverage -FilterName All -OutputPath coverage/plan797-final/coverage.cobertura.xml -ResultsDirectory coverage/plan797-trx/p5-coverage`

The helper invoked `dotnet-coverage collect --output <path> --output-format cobertura --settings
<derived> -- <vstest.console.exe> <two assemblies> /Settings:<cli runsettings> /InIsolation
/TestCaseFilter:<All filter> /Logger:trx /ResultsDirectory:<results dir>`, identically to the Phase 0
baseline run apart from the output path.

EXIT_CODE: 0

ExpectedExitCode: 0

The clean branch applies. The collected run reproduced no failure: its results file records 5262
total, 5262 executed, 5262 passed, 0 failed, so there was no inner test-run exit code to propagate
and no pre-existing failure to name.

## Output Summary

LINES_COVERED=44489 LINES_VALID=83537 BRANCHES_COVERED=10928 BRANCHES_VALID=24371 POSTCHANGE_LINE_PERCENT=53.26 POSTCHANGE_ASSEMBLY_SCOPE=UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll,TaskMaster.Test/bin/Debug/TaskMaster.Test.dll

The document-level branch rate, recorded as a non-asserted observation, is 44.84 percent, against
44.72 percent at the baseline.

Both test assemblies were excluded from instrumentation by the derived coverage settings: the helper
reads the canonical coverage.config in memory, retains every existing third-party module exclusion,
and adds exactly one further module exclusion matching any module name ending in `.Test.dll`. The
canonical settings file was not written. The denominator therefore holds production code only.

No field above carries the text UNVERIFIED or any placeholder; every value is a concrete integer or a
two-decimal number read from the Cobertura document root.

The assembly scope is the same two test assemblies P0-T10 recorded, and both documents were produced
by the same helper with no post-processing, so they are comparable with each other. The binding
comparison is made in P5-T7 against the Phase 0 baseline, not here.

Output Summary: Post-change coverage collected cleanly. 44489 of 83537 lines covered, giving a
document-level line rate of 53.26 percent; 10928 of 24371 branches covered. The collected test run was
green at 5262 passed and 0 failed.
