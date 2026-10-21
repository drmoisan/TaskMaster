# [P0-T22] Baseline of the four pre-existing navigation tests

Timestamp: 2026-08-27T09-45
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix|FullyQualifiedName~LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys|FullyQualifiedName~RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException|FullyQualifiedName~SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey"`
EXIT_CODE: 0

These four tests live in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, a file this
feature does not own and that upstream #468 `[P4-T5]` modifies. This task therefore records the
**observed** outcome rather than asserting an absolute figure. `[P2-T10]` compares against
`BaselineNavTestResults`, not against a hard-coded number.

`/InIsolation` is present. Omitting it produces empty-message, sub-millisecond assembly-load failures
in these Moq-based assemblies that are not real regressions.

## Summary (verbatim)

```
Test Run Successful.
Total tests: 4
     Passed: 4
```

```
BaselineNavTestCount = 4
```

## Per-test verdicts

```
BaselineNavTestResults:
  LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix = Passed [280 ms]
  LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys          = Passed [2 ms]
  RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException   = Passed [< 1 ms]
  SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey   = Passed [< 1 ms]
```

All four named tests were discovered and all four passed. No test matched the filter that is not one
of the four named.

## Acceptance evaluation

- The executed test count is recorded as `BaselineNavTestCount = 4`. PASS.
- The pass/fail verdict of each of the four named tests is recorded as `BaselineNavTestResults`. PASS.

Output Summary: 4 of 4 discovered; all four Passed; `BaselineNavTestCount = 4`;
`BaselineNavTestResults` records `Passed` for each of the four named tests.
