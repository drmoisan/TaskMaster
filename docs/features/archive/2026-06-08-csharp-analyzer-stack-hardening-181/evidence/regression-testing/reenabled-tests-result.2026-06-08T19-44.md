# Re-Enabled Regression Tests — Result (Cycle 3) — NEW FINDING

Timestamp: 2026-06-08T19-44

Command: vstest.console.exe <7 first-party Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx
TRX: docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/trx/DanMoisan_MEGALODON4_2026-06-08_19_57_49_net481.trx

EXIT_CODE: 1

## Per-Test Outcome (the two re-enabled regression tests)

| Test | File | Ran? | Outcome |
|---|---|---|---|
| Constructor_WithOutlookItem_ShouldInitializeProperties | ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs | Yes (not Skipped) | Passed |
| People_Deserialize_CanDeserializePatternCorrectly | ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs | Yes (not Skipped) | **Failed** |

## Failure Detail (People_Deserialize_CanDeserializePatternCorrectly)

```
Assert.AreEqual failed. Expected string length 11 but was 0.
  'expected' expression: '"pplkey.json"'
  'actual'   expression: 'people.Config.Disk.FileName'
```

This is a genuine assertion/behavior failure (the deserialized object's
`Config.Disk.FileName` is empty where the test expects `"pplkey.json"`), not a
timing/flaky failure. It is the regression the `[Ignore("ProductionBugSuspected")]`
marker previously suppressed; commit 0883d0f7 re-enabled it.

## Classification — NEW FINDING (Scope-Change Escalation)

Per the plan's Scope-Change Escalation Rule (remediation-plan lines 95-101) and the
executor directive, a FAILING re-enabled regression test is a new finding OUTSIDE this
formatting-only cycle. Actions taken:
- Execution HALTED at P2-T6 BEFORE commit/push.
- The test was NOT re-ignored, weakened, or skipped.
- No analyzer-config, vendored-project, or `.claude/rules/` change was made.
- The formatting fix (P1-T1) remains applied in the working tree but is NOT committed.

This requires a follow-up remediation cycle per `remediation-handoff-atomic-planner`.

## Full Test Run Headline

- Total tests: 4064. Passed: 4054. Failed: 10.
- Of the 10 failures, `People_Deserialize_CanDeserializePatternCorrectly` is the
  re-enabled regression test and is the blocking new finding.
- The other 9 failures are listed for context in the full-test-coverage evidence; they
  are candidate documented flaky wall-clock-timer / path-dependent tests and are NOT the
  reason for this halt (the halt is driven solely by the re-enabled regression test).
