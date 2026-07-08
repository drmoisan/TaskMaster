# Baseline — Tests with Coverage (Issue #183)

Timestamp: 2026-06-10T09-13

Command (canonical): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
Command (executed, git-bash + Moq-isolation form):
`MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation /EnableCodeCoverage`
Coverage conversion: `Microsoft.CodeCoverage.Console.exe merge <.coverage> -f xml -o coverage-baseline.xml`

EXIT_CODE: 1 (full assembly — one PRE-EXISTING unrelated failure; see below)

## Output Summary

### Full UtilitiesCS.Test assembly
- Total tests: 3814; Passed: 3813; Failed: 1.
- The single failing test is `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` (a UI-thread/dispatcher timing test in the queue/dispatcher area). It is PRE-EXISTING on commit c8feca8c, unrelated to Triage_OlLogic and to issue #183. No change in this plan touches that code path. Recorded as a baseline-known failure.

### Targeted Triage_OlLogic tests (separate run, `/TestCaseFilter:FullyQualifiedName~Triage_OlLogic`)
- Total tests: 21; Passed: 21; Failed: 0.
- The four plan-named pre-existing tests all PASS at baseline:
  - `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_TotalEmailCountIncrementsOnce` — PASS
  - `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_MatchEmailCountIncrementsOnce` — PASS
  - `TrainSelectionAsync_WhenSelectionContainsMailItem_TrainsClassifierWithExpectedLabel` — PASS
  - `TrainSelectionAsync_WhenSelectionIsNull_SkipsWithoutThrowingOrTraining` — PASS

### Coverage headline (baseline)
- First-party production assembly `UtilitiesCS.dll`: lines_covered=35056, lines_not_covered=5134 -> 87.23% line coverage (>= 80% repo gate, measured against application code).
- Production file `Triage_OlLogic.cs`: 115 covered / 55 not covered of 170 instrumented lines -> 67.65%. Uncovered lines are concentrated in `UnTrainSelectionAsync` (0/25 MoveNext) and some `FilterView`/`StripFilter` branches, NOT in `TrainSelectionAsync`.
- Method under change `TrainSelectionAsync` (async state machine `MoveNext`): 25 covered / 0 not covered -> 100% at baseline (exercised by the existing same-conversation/single-item tests).

Note on the raw whole-process figure: the merged `.coverage` instruments every loaded module including vendored/third-party assemblies, yielding a raw whole-process line coverage of 54.11% (93515/172819). The policy 80% gate applies to first-party application code (`UtilitiesCS.dll` = 87.23%), not the whole instrumented process; the coverage-comparison task (P2-T5) uses the first-party and method-level figures.

Coverage XML: docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/baseline/coverage-baseline.xml
