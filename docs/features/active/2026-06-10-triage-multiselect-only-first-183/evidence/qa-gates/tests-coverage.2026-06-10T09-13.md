# Final QC — Tests with Coverage (Issue #183, AC5)

Timestamp: 2026-06-10T09-13

Command (canonical): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
Command (executed): `MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation /EnableCodeCoverage`
Coverage conversion: `Microsoft.CodeCoverage.Console.exe merge <.coverage> -f xml -o coverage-post.xml`

EXIT_CODE: 1 (one PRE-EXISTING unrelated failure, identical to baseline; see below)

## Output Summary

- Total tests: 3815 (baseline 3814, +1 = the new issue #183 regression test); Passed: 3814; Failed: 1.
- The single failing test is `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` — the SAME pre-existing UI-thread/dispatcher timing test that failed at baseline (P0-T6). It is unrelated to Triage_OlLogic and to issue #183. The fix introduces ZERO new test failures.
- The new regression test `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_WritesTriageUdfToEveryItem` PASSES, and the four pre-existing Triage tests PASS (see pass-after.2026-06-10T09-13.md).

### Coverage headline (post-change)
- First-party production assembly `UtilitiesCS.dll`: lines_covered=35047, lines_not_covered=5144 -> 87.20% line coverage (>= 80% repo gate).
- Production file `Triage_OlLogic.cs`: 116 covered / 55 not covered of 171 instrumented lines -> 67.84% (vs baseline 67.65%; slight increase from the +1 covered line in the changed method; uncovered remainder is the untouched `UnTrainSelectionAsync`).
- Changed method `TrainSelectionAsync` (`<TrainSelectionAsync>d__13.MoveNext`): 28 covered / 0 not covered -> 100% (baseline was 25/0). The added `HashSet<string>` gating and `mailItem.ConversationID ?? string.Empty` lines are all covered by the new and existing same-conversation tests.
- Whole instrumented process (incl. vendored/third-party modules): 54.14% (93590/172870) — informational only; not the policy gate.

Coverage XML: docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/coverage-post.xml
