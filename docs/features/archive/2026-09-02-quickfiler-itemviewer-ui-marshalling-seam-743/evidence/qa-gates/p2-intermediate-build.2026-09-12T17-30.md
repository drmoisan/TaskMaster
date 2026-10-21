# Phase 2 — Intermediate (defect-preserving) analyzer build and untouched-file gate (P2-T5)

Task: [P2-T5]
Both commands were run from the item worktree root. Command 1 ran inside one pwsh invocation via Set-Location with the Command Reference tool resolution prepended, while holding the shared machine build lock for item 743 (acquired immediately before, released immediately after; the acquire reported WAITING on item 839 for about 45 s before ACQUIRED). Command 2 was run as two consecutive `git -C <worktree>` invocations of the same subcommands, the form P1-T12 used. Outlook was closed.

## Command 1 — analyzer Rebuild on the intermediate tree (two casts retained)

Timestamp: 2026-09-13T02-58
Command: `pwsh -Command '& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true'` (console output redirected to the ignored path `coverage\p2-t5-analyzer.log`)
EXIT_CODE: 0
Output Summary:
- `Build succeeded.`
- `    0 Warning(s)`
- `    0 Error(s)`
- `Time Elapsed 00:00:18.49`
- 20 `Done Building Project` lines in the log; `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` exists after the run.
- No line of the form `error XXnnnn` or `warning XXnnnn` appears anywhere in the log. The Meziantou.Analyzer 3.0.203 folder installed under the ignored `packages/` directory during run B (P0-T6 artifact) was still present, so no CS0006 re-provisioning was needed.

Comparison against P0-T6: the transcribed error line `    0 Error(s)` is character-for-character identical to the P0-T6 baseline line `    0 Error(s)`; the warning line `    0 Warning(s)` is likewise identical.

## Command 2 — stage and name-listing diff against the self-anchor

Timestamp: 2026-09-13T02-59
Command: `pwsh -Command 'git add -A -- QuickFiler QuickFiler.Test; git diff --cached --name-only refs/plan/issue-743-base -- QuickFiler QuickFiler.Test'`
EXIT_CODE: 0
Output Summary: five names printed (verbatim list below); the list contains neither `QuickFiler/Controllers/QfcItemController.Initialization.cs` nor `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`.

```
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
QuickFiler/Viewers/IItemViewer.cs
QuickFiler/Viewers/ItemViewer.cs
```

The two fixture files appear because they were committed in P1-T12 after the anchor `refs/plan/issue-743-base` (c358b2d809ca58db0197eb10229f872f2e9a924e) was written; that is expected. The three production files are the P2-T1, P2-T2 and P2-T3 edits.
