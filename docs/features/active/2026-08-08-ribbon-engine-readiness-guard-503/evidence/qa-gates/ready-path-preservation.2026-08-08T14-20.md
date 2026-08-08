# AC16 Ready-Path Preservation — Issue #503 (P5-T7)

Timestamp: 2026-08-08T14-20

Command (extracts the merge-base handler expressions for direct comparison against the post-change lambda bodies):
```
git show 003c5715055d7d1933db68a742531332756e30b2:TaskMaster/Ribbon/RibbonViewer.cs | sed -n '255,264p;303,310p;316,317p;325,326p'
```
Post-change bodies were taken from `<FEATURE>\evidence\qa-gates\lambda-deferral-audit.2026-08-08T14-18.md`, which extracted them verbatim from `TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs`.

EXIT_CODE: 0

## Output Summary — merge-base expression vs post-change lambda body

| # | Handler | Merge-base awaited expression | Post-change lambda body | Invoked expression text unchanged? |
|---|---|---|---|---|
| 1 | `TrainSpam_Click` | `Controller.SB.TrainAsync(Controller.OlSelection, true)` | `Controller.SB.TrainAsync(Controller.OlSelection, true)` | **Unchanged** |
| 2 | `TrainHam_Click` | `Controller.SB.TrainAsync(Controller.OlSelection, false)` | `Controller.SB.TrainAsync(Controller.OlSelection, false)` | **Unchanged** |
| 3 | `TestSpam_Click` | `((SpamBayes)Controller.Engines.InboxEngines[SpamBayes.GroupName].Engine).TestAsync(Controller.OlSelection)` | `((SpamBayes)Controller.Engines.InboxEngines[SpamBayes.GroupName].Engine).TestAsync(Controller.OlSelection)` | **Unchanged** |
| 4 | `TriageSetA_Click` | `_controller.Triage.OlLogic.TrainSelectionAsync("A")` | `_controller.Triage.OlLogic.TrainSelectionAsync("A")` | **Unchanged** |
| 5 | `TriageSetB_Click` | `_controller.Triage.OlLogic.TrainSelectionAsync("B")` | `_controller.Triage.OlLogic.TrainSelectionAsync("B")` | **Unchanged** |
| 6 | `TriageSetC_Click` | `_controller.Triage.OlLogic.TrainSelectionAsync("C")` | `_controller.Triage.OlLogic.TrainSelectionAsync("C")` | **Unchanged** |
| 7 | `ClearTriage_Click` | `_controller.Triage.OlLogic.UnTrainSelectionAsync()` | `_controller.Triage.OlLogic.UnTrainSelectionAsync()` | **Unchanged** |
| 8 | `FilterViewer_Click` | `_controller.Triage.OlLogic.FilterViewAsync()` | `_controller.Triage.OlLogic.FilterViewAsync()` | **Unchanged** |

Merge-base source, verbatim:

```
        public async void TrainSpam_Click(Office.IRibbonControl control) =>
            await Controller.SB.TrainAsync(Controller.OlSelection, true);

        public async void TrainHam_Click(Office.IRibbonControl control) =>
            await Controller.SB.TrainAsync(Controller.OlSelection, false);

        public async void TestSpam_Click(Office.IRibbonControl control) =>
            await (
                (SpamBayes)Controller.Engines.InboxEngines[SpamBayes.GroupName].Engine
            ).TestAsync(Controller.OlSelection);

        public async void TriageSetA_Click(Office.IRibbonControl control) =>
            await _controller.Triage.OlLogic.TrainSelectionAsync("A");

        public async void TriageSetB_Click(Office.IRibbonControl control) =>
            await _controller.Triage.OlLogic.TrainSelectionAsync("B");

        public async void TriageSetC_Click(Office.IRibbonControl control) =>
            await _controller.Triage.OlLogic.TrainSelectionAsync("C");

        public async void ClearTriage_Click(Office.IRibbonControl control) =>
            await _controller.Triage.OlLogic.UnTrainSelectionAsync();

        public async void FilterViewer_Click(Office.IRibbonControl control) =>
            await _controller.Triage.OlLogic.FilterViewAsync();
```

## Assessment

In all eight cases the invoked expression text is byte-identical to the merge-base. The only structural change is that the expression is now the body of a `Func<Task>` lambda handed to `Controller.RunEngineCommandAsync(<control id>, ...)` instead of being the direct operand of `await`. On the ready path the runner returns `action()` directly, so the awaited task is the same task the merge-base awaited, produced by the same expression, on the same thread. Once engines are loaded the behaviour is therefore identical to today's, which is exactly what R6 and AC16 require.

Corroborating runtime evidence: `RunAsync_WhenEngineReady_InvokesActionExactlyOnce` (the action runs exactly once, with zero notifications) and `RunAsync_WhenEngineReady_AwaitsActionToCompletion` (the returned task does not complete until the action's task completes).

Binary outcome: **PASS**.
