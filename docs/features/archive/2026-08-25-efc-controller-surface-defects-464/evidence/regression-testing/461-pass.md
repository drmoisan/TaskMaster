# Phase 3 — pass-after evidence for the #461 dead-handler removal

Timestamp: 2026-08-28T00-23
Task: [P3-T4]
Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU`, then `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ConversationResolverPropertyChanged_IsAbsentFromEfcItemControllerMetadata|FullyQualifiedName~PopulateConversation_AssignsSetTopicThreadToConversationResolverUpdateUi" "/Logger:trx;LogFileName=461-pass.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p3-t4`, both under `pwsh -NoProfile`
EXIT_CODE: 0

Build exit code: 0.

## Counters

```
total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0"
inProgress="0" pending="0"
```

Total executed: **2**. Failed: **0**.

## The two results, both `Passed`

| Test | Outcome | Duration |
|---|---|---|
| `ConversationResolverPropertyChanged_IsAbsentFromEfcItemControllerMetadata` | Passed | 30 ms |
| `PopulateConversation_AssignsSetTopicThreadToConversationResolverUpdateUi` | Passed | 838 ms |

The first was **red** in `[P3-T2]` against the identical assertion, so the transition is attributable to
`[P3-T3]`'s deletion of the handler and nothing else.

The second was green before and after by design. It is the live-route control: `PopulateConversation`
still assigns `SetTopicThread` to `ConversationResolver.UpdateUI`, and the delegate is still bound to the
controller instance. Its remaining green after the deletion is the evidence that removing the dead
handler cost no behaviour — the intended effect was already being delivered by this route, which is the
finding that made removal the correct remedy instead of retargeting the `nameof` guard.

## TRX artifact

`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p3-t4/461-pass.trx`

Sanitised in place: worktree paths replaced with `<repo-root>`, account and machine names replaced with
`<user>` and `<host>`; a case-insensitive search for either returns zero matches. The `/InIsolation`
`Deploy_*` scratch tree was removed.

Output Summary: 2 of 2 executed and passed, 0 failed, vstest exit code 0. The absence assertion flipped
from red in [P3-T2] to green here; the live-route control stayed green throughout.
