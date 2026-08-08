# AC10 Lambda-Deferral Audit — Issue #503 (P5-T6)

Timestamp: 2026-08-08T14-18

Command (extracts every `RunEngineCommandAsync` invocation with its full argument list from the rewritten handlers):
```
awk '/RunEngineCommandAsync\(/{p=1} p{print NR": "$0} /^\s*\);\s*$/{if(p){p=0; print "---"}}' TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs
```

EXIT_CODE: 0

## Output Summary — the eight rewritten handler bodies, verbatim

| # | Handler | Control id argument | `Func<Task>` lambda body | Engine dereference inside the lambda only? |
|---|---|---|---|---|
| 1 | `TrainSpam_Click` | `"TrainSpam"` | `() => Controller.SB.TrainAsync(Controller.OlSelection, true)` | **Yes** — `Controller.SB` is inside the lambda |
| 2 | `TrainHam_Click` | `"TrainHam"` | `() => Controller.SB.TrainAsync(Controller.OlSelection, false)` | **Yes** |
| 3 | `TestSpam_Click` | `"TestSpam"` | `() => ((SpamBayes)Controller.Engines.InboxEngines[SpamBayes.GroupName].Engine).TestAsync(Controller.OlSelection)` | **Yes** — the `InboxEngines[...]` indexer is inside the lambda |
| 4 | `TriageSetA_Click` | `"TriageSetA"` | `() => _controller.Triage.OlLogic.TrainSelectionAsync("A")` | **Yes** — `_controller.Triage` is inside the lambda |
| 5 | `TriageSetB_Click` | `"TriageSetB"` | `() => _controller.Triage.OlLogic.TrainSelectionAsync("B")` | **Yes** |
| 6 | `TriageSetC_Click` | `"TriageSetC"` | `() => _controller.Triage.OlLogic.TrainSelectionAsync("C")` | **Yes** |
| 7 | `ClearTriage_Click` | `"ClearTriage"` | `() => _controller.Triage.OlLogic.UnTrainSelectionAsync()` | **Yes** |
| 8 | `FilterViewer_Click` | `"FilterTriageGroup"` | `() => _controller.Triage.OlLogic.FilterViewAsync()` | **Yes** |

Verbatim extraction:

```
84:             await Controller.RunEngineCommandAsync(
85:                 "TrainSpam",
86:                 () => Controller.SB.TrainAsync(Controller.OlSelection, true)
87:             );
90:             await Controller.RunEngineCommandAsync(
91:                 "TrainHam",
92:                 () => Controller.SB.TrainAsync(Controller.OlSelection, false)
93:             );
96:             await Controller.RunEngineCommandAsync(
97:                 "TestSpam",
98:                 () =>
99:                     (
100:                         (SpamBayes)Controller.Engines.InboxEngines[SpamBayes.GroupName].Engine
101:                     ).TestAsync(Controller.OlSelection)
102:             );
142:             await Controller.RunEngineCommandAsync(
143:                 "TriageSetA",
144:                 () => _controller.Triage.OlLogic.TrainSelectionAsync("A")
145:             );
148:             await Controller.RunEngineCommandAsync(
149:                 "TriageSetB",
150:                 () => _controller.Triage.OlLogic.TrainSelectionAsync("B")
151:             );
154:             await Controller.RunEngineCommandAsync(
155:                 "TriageSetC",
156:                 () => _controller.Triage.OlLogic.TrainSelectionAsync("C")
157:             );
164:             await Controller.RunEngineCommandAsync(
165:                 "ClearTriage",
166:                 () => _controller.Triage.OlLogic.UnTrainSelectionAsync()
167:             );
176:             await Controller.RunEngineCommandAsync(
177:                 "FilterTriageGroup",
178:                 () => _controller.Triage.OlLogic.FilterViewAsync()
179:             );
```

## Assessment

Each of the eight handlers passes exactly two arguments to `RunEngineCommandAsync`: a **string literal** control id and a **lambda**. In every case the only argument evaluated before `RunEngineCommandAsync` is entered is the string literal. `Controller` is a plain field-backed property read that touches no engine. No engine dereference — `Controller.SB`, `_controller.Triage`, or `Controller.Engines.InboxEngines[...]` — appears anywhere except inside a lambda body.

Consequence: when `IsCommandEnabled(controlId)` is false, `EngineGatedCommandRunner.RunAsync` returns `Task.CompletedTask` without ever invoking the lambda, so no engine is dereferenced. That is exactly what converts the `NullReferenceException` (handlers 1, 2, 4-8) and the `KeyNotFoundException` (handler 3) into a no-op.

`FilterViewer_Click` correctly uses control id `FilterTriageGroup`, matching the ribbon XML element id rather than the handler name.

Corroborating runtime evidence: `RunAsync_WhenEngineNotReady_DoesNotThrowNullReferenceException` and `RunAsync_WhenEngineNotReady_DoesNotThrowKeyNotFoundException` both assert that the supplied lambda is never entered when the gate is closed (recorded in `<FEATURE>\evidence\regression-testing\pass-after-503.2026-08-08T13-32.md`).

Binary outcome: **PASS**.
