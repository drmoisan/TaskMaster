# Phase 0 — Baseline Test-Existence Filter (Issue #244)

Timestamp: 2026-07-06T11-50

Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~InitEmailQueue_ZeroBatchSize OR FullyQualifiedName~InitEmailQueue_PositiveBatchSize"

EXIT_CODE: 1

Output Summary: "No test matches the given testcase filter ... in QuickFiler.Test.dll" — 0 matching tests found, confirming the regression tests (`InitEmailQueue_ZeroBatchSize_*`, `InitEmailQueue_PositiveBatchSize_*`) do not exist yet at baseline. vstest additionally emitted an informational "Incorrect format for TestCaseFilter" diagnostic about the `OR` syntax, but the run still completed and correctly reported zero matches, so the baseline test-existence signal (0 matches) is confirmed either way.

Tooling note: a probe against two known-existing test names confirmed this vstest 18.7.0 build rejects the literal `OR` keyword in `/TestCaseFilter` (0 matches with the "Incorrect format" diagnostic even for tests that exist) and requires the `|` operator instead (confirmed: `FullyQualifiedName~A|FullyQualifiedName~B` correctly matched and ran both known tests). Subsequent tasks (P1-T2, P1-T3, P1-T5, P2-T1) that reuse this filter substitute `|` for `OR` in the literal command actually executed, while targeting the identical set of test names specified by the plan. This is a boolean-operator syntax correction only, not a change in test scope.

