# [P1-T6] #444 duplicate-guard test — GREEN after the fix

Timestamp: 2026-08-27T09-45
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~KbdActionsRemainingBranchesTests.EnumerableConstructor_WhenSeedContainsDuplicateSourceAndStoredKey_ThrowsArgumentException"`
EXIT_CODE: 0

The filter is byte-identical to `[P1-T3]`'s. The only change between the two runs is `[P1-T4]`'s edit
to `QuickFiler/Controllers/KbdActions.cs`.

## Result (verbatim)

```
Passed EnumerableConstructor_WhenSeedContainsDuplicateSourceAndStoredKey_ThrowsArgumentException [95 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
```

| Measure | Value |
| --- | --- |
| Total | 1 |
| Passed | **1** |
| Failed | **0** |

The constructor now throws `ArgumentException` whose message contains the literal fragment
`already exists`, satisfying the test's `.WithMessage("*already exists*")` assertion.

## Acceptance evaluation

- The run reports `Passed: 1` and `Failed: 0`. PASS.

Output Summary: 1 test run, 1 passed, 0 failed; the #444 pass-after state is captured; paired with
`[P1-T3]`'s RED run by the dossier at `[P1-T17]`.
