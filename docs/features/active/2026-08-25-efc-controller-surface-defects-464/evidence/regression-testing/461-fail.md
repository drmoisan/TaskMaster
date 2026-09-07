# Phase 3 — fail-before evidence for the #461 dead-handler removal

Timestamp: 2026-08-28T00-22
Task: [P3-T2] [expect-fail]
Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU`, then `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ConversationResolverPropertyChanged_IsAbsentFromEfcItemControllerMetadata|FullyQualifiedName~PopulateConversation_AssignsSetTopicThreadToConversationResolverUpdateUi" "/Logger:trx;LogFileName=461-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p3-t2`, both under `pwsh -NoProfile`
EXIT_CODE: 1
ExpectedExitCode: 1

A failing run is the intended outcome of this task. Build exit code was 0.

## Counters

```
total="2" executed="2" passed="1" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0"
inProgress="0" pending="0"
```

Total executed: **2**, which is greater than zero, satisfying the non-vacuity rule.

## The two results

| Test | Outcome | Duration |
|---|---|---|
| `ConversationResolverPropertyChanged_IsAbsentFromEfcItemControllerMetadata` | **Failed** | 128 ms |
| `PopulateConversation_AssignsSetTopicThreadToConversationResolverUpdateUi` | Passed | 811 ms |

Failure message of the red result, verbatim:

> Expected `handler` to be `<null>` because the handler guards on a property name the resolver never
> raises and #461 closes it by removal, but found `EfcItemController.ConversationResolverPropertyChanged`.

This is the fail-before evidence `[P3-T2]` requires: the dead handler is still declared on the type.

## Why the second result is green here

`PopulateConversation_AssignsSetTopicThreadToConversationResolverUpdateUi` is a **live-route** control,
not a fail-before assertion. It pins the surviving path that already delivers the intended behaviour:
`PopulateConversation` assigns `SetTopicThread` to `ConversationResolver.UpdateUI`, and the resolver
invokes that delegate on the UI thread after a background load. That route exists **before** this feature
touches anything, which is precisely the finding that makes removal — rather than retargeting the guard —
the correct remedy for #461. Its being green both before and after the deletion is the positive
behaviour-preservation control required by `spec.md` risk R-3 mitigation 6; a red result here would mean
the deletion had cost real behaviour.

## TRX artifact

`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p3-t2/461-fail.trx`

Sanitised in place: worktree paths replaced with `<repo-root>`, account and machine names replaced with
`<user>` and `<host>`; a case-insensitive search for either returns zero matches. The `/InIsolation`
`Deploy_*` scratch tree was removed.

Output Summary: 2 executed, 1 failed, 1 passed, vstest exit code 1 as expected. The failure names
`EfcItemController.ConversationResolverPropertyChanged` as still declared; the live-route control
confirms `PopulateConversation` already installs `SetTopicThread` as the resolver's `UpdateUI` delegate.
