# QA-04 Tests — CI Invocation Form (Cycle 2, Issue #292) — POST-FIX

Timestamp: 2026-07-09T17-45

## Authoritative pass/fail — CI-equivalent `/EnableCodeCoverage` invocation

Command: `vstest.console.exe <all 7 *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
EXIT_CODE: 0
Output Summary: `Test Run Successful.` Total tests 5141; Passed 5141; Failed 0.

## Numeric post-change coverage headline — reliable `dotnet-coverage collect` -> Cobertura path

Command: `dotnet-coverage collect --output <scratchpad>/p2t4.cobertura.xml --output-format cobertura --settings coverage.config -- vstest.console.exe <all 7 *.Test.dll> /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
EXIT_CODE: 0
Output Summary: Repository-wide (Cobertura root) line-rate **81.81%** (lines-covered 121618 /
lines-valid 148653); branch-rate 59.66%. All 5141 tests passed on the collection run. This reproduces the
P1-T3 post-fix figure (121618 / 148653) exactly. Above the 80% testable-denominator floor.
