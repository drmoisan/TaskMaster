# Phase 1 — pass-after evidence for the #459 / #466 structural removals

Timestamp: 2026-08-28T00-12
Task: [P1-T12]
Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU`, then `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~IsAbsentFromEfcItemControllerMetadata|FullyQualifiedName~ToggleExpansion_IsAbsentAtEveryArity|FullyQualifiedName~AsyncExpansionPath_OnOffOn_LeavesCharActionsKeysUnchanged" "/Logger:trx;LogFileName=459-466-structural-pass.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p1-t12`, both under `pwsh -NoProfile`
EXIT_CODE: 0

Build exit code: 0.

## Counters

TRX `<Counters>`, verbatim:

```
total="6" executed="6" passed="6" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0"
inProgress="0" pending="0"
```

Total executed: **6**. Failed: **0**.

## The six distinct results, all `Passed`

| # | Test | Outcome | Duration |
|---|---|---|---|
| 1 | `RegisterActions_IsAbsentFromEfcItemControllerMetadata` | Passed | 30 ms |
| 2 | `ToggleExpansion_IsAbsentAtEveryArity` | Passed | 4 ms |
| 3 | `InitializeWebView_IsAbsentFromEfcItemControllerMetadata` | Passed | < 1 ms |
| 4 | `SevenParameterConstructor_IsAbsentFromEfcItemControllerMetadata` | Passed | 4 ms |
| 5 | `SelectorsCtrlsField_IsAbsentFromEfcItemControllerMetadata` | Passed | < 1 ms |
| 6 | `AsyncExpansionPath_OnOffOn_LeavesCharActionsKeysUnchanged` | Passed | 1 s |

Results 1 through 5 were red in `[P1-T5]` against the same assertions and the same binaries, so the
red-to-green transition is attributable to the Phase 1 deletions and to nothing else.

## Recorded deviation — one arrange line added to `[P1-T11]`'s test

`AsyncExpansionPath_OnOffOn_LeavesCharActionsKeysUnchanged` did not complete on first run: the test host
hung indefinitely and had to be killed after 600 seconds. The cause was isolated empirically rather than
guessed, with three staged probes that were removed once they had answered the question:

| Probe | Result |
|---|---|
| `L1h0L2hv3h_TlpBodyToggle.ColumnStyles[n].Width = ...` on a fresh `ItemViewer` | Passed, 878 ms |
| `L0v2h2_WebView2.Visible = true` on a fresh `ItemViewer` | Passed, 871 ms |
| `TopicThread.Visible = true` on a fresh `ItemViewer` | **hung**, killed at 70 s |

Setting `UseOverlays = false`, `EmptyListMsg = null`, or `VirtualMode = false` on the list first did not
help; all three still hung. A fourth probe established the mechanism exactly: on a freshly constructed
`QuickFiler.ItemViewer`, `viewer.IsHandleCreated` is already **True** — the WebView2 children force the
parent's handle during `EndInit` — while `viewer.TopicThread.IsHandleCreated` is **False**. A visibility
write against a child that is still parented to a handle-created parent therefore reaches
`Control.CreateControl()`, and creating a `BrightIdeasSoftware.FastObjectListView` handle never completes
in a test host with no message pump. The same probe toggled `Visible` four times on a **parentless**
`FastObjectListView` with no delay at all.

The remedy is one arrange line, added with an explanatory comment:

```csharp
viewer.TopicThread = new BrightIdeasSoftware.FastObjectListView();
```

`ItemViewer.TopicThread` is a settable property of that exact type, so the substitute is a real control
of the real type; it simply has no parent, so no native handle can be created by a visibility write.

Everything `[P1-T11]` specifies is unchanged by this line: the controller is still produced by
`FormatterServices.GetUninitializedObject`; a real headless `QuickFiler.ItemViewer` is still injected into
`_itemViewer` and is never shown; the `SynchronizationContext` is still saved and restored in `finally`;
the registry is still a real `KbdActions<char, KaChar, Action<char>>` seeded with two distinct entries and
held in a local; the mock is still `MockBehavior.Strict` with `CharActions` as its only set-up member; the
pre-state is still captured from the local and never through the mock; the dispatched bodies are still
invoked directly by reflection in the order **On, Off, On**; and `VerifyNoOtherCalls()` is still asserted.
No assertion was weakened and no prohibited construct was introduced — no `Thread.Sleep`, no `Task.Delay`,
no `Form`, no message pump, no temporary file.

## TRX artifact

`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p1-t12/459-466-structural-pass.trx`

Sanitised in place: absolute worktree paths replaced with `<repo-root>`, account and machine names
replaced with `<user>` and `<host>`; a case-insensitive search for either now returns zero matches. The
`Deploy_*` scratch tree that `/InIsolation` created was removed, including a nested directory named for
the machine.

Output Summary: 6 of 6 tests executed and passed, 0 failed, vstest exit code 0. Results 1-5 were red in
[P1-T5] against identical assertions, so the transition is attributable to the Phase 1 deletions. The
sixth test required one arrange line — substituting a parentless `FastObjectListView` for the viewer's
list control — because creating that control's native handle never completes in a pump-less host; the
cause was isolated by four staged probes and no assertion was weakened.
