# Phase 1 — fail-before evidence for the #459 / #466 structural removals

Timestamp: 2026-08-27T23-45
Task: [P1-T5] [expect-fail]
Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU`, then `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~IsAbsentFromEfcItemControllerMetadata|FullyQualifiedName~ToggleExpansion_IsAbsentAtEveryArity" "/Logger:trx;LogFileName=459-466-structural-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p1-t5`, both under `pwsh -NoProfile`
EXIT_CODE: 1
ExpectedExitCode: 1

A failing run is the intended outcome of this task. It is the fail-before evidence that every member
Phase 1 deletes is still present at this point.

The preceding build exited 0, so the five red results are genuine assertion failures and not a compile
failure.

## Counters

TRX `<Counters>`, verbatim:

```
total="5" executed="5" passed="0" failed="5" error="0" timeout="0" aborted="0" inconclusive="0"
passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0"
inProgress="0" pending="0"
```

Total executed: **5**, which is greater than zero, so the filter matched real tests. This satisfies the
plan's non-vacuity rule.

## The five results, all `Failed`

| # | Test | Outcome | Failure message |
|---|---|---|---|
| 1 | `RegisterActions_IsAbsentFromEfcItemControllerMetadata` | Failed | Expected `registerActions` to be `<null>` ... but found `EfcItemController.RegisterActions`. |
| 2 | `ToggleExpansion_IsAbsentAtEveryArity` | Failed | Expected `synchronousOverloads` to be empty ... but found at least one item `{EfcItemController.ToggleExpansion}`. |
| 3 | `InitializeWebView_IsAbsentFromEfcItemControllerMetadata` | Failed | Expected `initializeWebView` to be `<null>` ... but found `EfcItemController.InitializeWebView`. |
| 4 | `SevenParameterConstructor_IsAbsentFromEfcItemControllerMetadata` | Failed | Expected `parameterCounts {6, 7, 5}` to not contain `7` ... |
| 5 | `SelectorsCtrlsField_IsAbsentFromEfcItemControllerMetadata` | Failed | Expected `selectorsCtrls` to be `<null>` ... but found `System.Collections.Generic.List1[System.Windows.Forms.Control]`. |

Each message names the member the assertion found, so each red result is direct evidence that the
specific member is still declared. Result 4 additionally enumerates the observed constructor arities as
`{6, 7, 5}`, confirming that the seven-parameter overload exists alongside the six- and five-parameter
overloads that must survive.

## TRX artifact

`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p1-t5/459-466-structural-fail.trx`

Sanitised in place: absolute worktree paths replaced with `<repo-root>`, and the account name, machine
name and deployment-root string replaced with `<user>` and `<host>`. A case-insensitive search for either
name now returns zero matches. The empty `Deploy_*` scratch directory that `/InIsolation` created
alongside the TRX was removed; it is runner scratch, not evidence.

Output Summary: 5 of 5 tests executed and failed, exactly as required for the fail-before step. Each
failure message names the still-present member, establishing that `RegisterActions`,
`ToggleExpansion`, `InitializeWebView`, the seven-parameter constructor and `_selectorsCtrls` all exist
before Phase 1's deletions.
