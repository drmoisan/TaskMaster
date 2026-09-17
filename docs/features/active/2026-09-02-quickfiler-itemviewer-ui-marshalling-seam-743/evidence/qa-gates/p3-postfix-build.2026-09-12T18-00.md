# Phase 3 — Post-fix analyzer Rebuild and Write Set diff (P3-T5)

Task: [P3-T5]
Both commands were run from the item worktree root. Command 1 ran inside one pwsh invocation via Set-Location with the Command Reference tool resolution prepended, while holding the shared machine build lock for item 743 (acquired immediately before, released immediately after). Command 2 was run as two consecutive `git -C <worktree>` invocations of the same subcommands, the form P1-T12 used. Outlook was closed.

## Command 1 — analyzer Rebuild on the post-fix tree

Timestamp: 2026-09-13T03-16
Command: `pwsh -Command '& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true'` (console output redirected to the ignored path `coverage\p3-t5-analyzer.log`)
EXIT_CODE: 0
Output Summary:
- `Build succeeded.`
- `    0 Warning(s)`
- `    0 Error(s)`
- `Time Elapsed 00:00:17.69`
- 20 `Done Building Project` lines in the log; no line of the form `error XXnnnn` or `warning XXnnnn` appears anywhere in the log.

Comparison against P0-T6: the transcribed error line `    0 Error(s)` is character-for-character identical to the P0-T6 baseline line `    0 Error(s)`; the warning line `    0 Warning(s)` is likewise identical.

## Source-compatibility verification (spec section 6.5, predicted in P1-T3)

The widening of `ResolveControlGroupsAsync` from the concrete viewer type to `IItemViewer` compiled both existing call sites unchanged: the production invocation `await ResolveControlGroupsAsync((ItemViewer)_itemViewer);` at line 216 of the Initialization controller partial and the test invocation `await controller.ResolveControlGroupsAsync(viewer).ConfigureAwait(false);` at line 448 of the ViewerSetup test file. Neither file appears in the diff list below, and the Rebuild that compiled both `QuickFiler` and `QuickFiler.Test` from clean reported `0 Error(s)`, so this is a verified outcome of the compile, not an assumption.

## Command 2 — stage and name-listing diff against the self-anchor

Timestamp: 2026-09-13T03-16
Command: `pwsh -Command 'git add -A -- QuickFiler QuickFiler.Test; git diff --cached --name-only refs/plan/issue-743-base -- QuickFiler QuickFiler.Test'`
EXIT_CODE: 0
Output Summary: seven names printed (verbatim list below), which are exactly the seven Write Set paths; all sit under the two project directories; the list contains neither `QuickFiler/Controllers/QfcItemController.Initialization.cs` nor `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`.

```
QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
QuickFiler/Viewers/IItemViewer.cs
QuickFiler/Viewers/ItemViewer.cs
```

Anchor: `refs/plan/issue-743-base` = c358b2d809ca58db0197eb10229f872f2e9a924e.
