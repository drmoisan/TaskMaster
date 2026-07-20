# P2-T20 — Full Solution-Wide Rebuild Gate (Post Layer 1/Layer 2 Fixes)

Timestamp: 2026-07-20T02-20

Command: `MSBuild.exe TaskMaster.sln -t:Rebuild -m -p:Configuration=Debug "-p:Platform=Any CPU" -p:TreatWarningsAsErrors=true`

EXIT_CODE: 1

## Outcome: (b) non-zero EXIT_CODE — a newly-surfaced project/diagnostic layer found; proceed to P2-T21

## Per-project build status (19 project nodes)

Succeeded: `Tags.Test`, `TaskVisualization`, `Tags`, `ToDoModel`, `ToDoModel.Test`, `TaskTree`,
`VBFunctions`, `SVGControl`, `TaskTree.Test`, `VBFunctions.Test`, `UtilitiesCS`,
`TaskVisualization.Test`.

FAILED: `QuickFiler.csproj` (15 own diagnostics — the real newly-surfaced layer), plus 4
cascading failures with zero own diagnostics (`TaskMaster.csproj`, `QuickFiler.Test.csproj`,
`UtilitiesCS.Test.csproj`, `TaskMaster.Test.csproj` — confirmed via grep that none of these 4
have their own `error` lines; each fails only because it references `QuickFiler.csproj`, whose
build did not produce an output assembly).

## QuickFiler.csproj diagnostic breakdown (15 total, 0 Warning(s))

- **CS0108** (4 occurrences) — `QuickFiler/Viewers/IItemViewer.cs` lines 119-122:
  `InvokeRequired`, `Invoke(Delegate)`, `BeginInvoke(Delegate)`, `Height` each hide an inherited
  member (`ISynchronizeInvoke.InvokeRequired`, `IControl.Invoke`, `IControl.BeginInvoke`,
  `IControl.Height`) without the `new` keyword.
- **CS0618** (8 occurrences) — obsolete `System.Linq.Async` `AsyncEnumerable` extension method
  usage (`SelectAwait`, `SelectAwaitWithCancellation`, `ForEachAwaitAsync`, `ForEachAsync`,
  `ForEachAwaitWithCancellationAsync`) across `QuickFiler/Controllers/QfcQueue.cs` (1),
  `QuickFiler/Helper Classes/ConversationResolver.cs` (1),
  `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (2),
  `QuickFiler/Controllers/QfcDatamodel.cs` (1), `QuickFiler/Controllers/QfcCollectionController.cs`
  (3).
- **CS8600** (2 occurrences) — `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` lines 296 and
  341: "Converting null literal or possible null value to non-nullable type."

## Next step

All 15 diagnostics fall into diagnostic classes already covered by the three authorized
remediation patterns (CS0618 -> narrow pragma bracket, matching the pattern already used ~11
times this session including P2-T17/P2-T18; CS8600 -> nullable annotation/guard-clause, matching
the pattern used throughout Phase 2's batches; CS0108 -> a new diagnostic class not previously
encountered this session, requiring a judgment call on whether adding the `new` keyword is a
behavior-preserving pattern-2/pattern-1-adjacent fix or a genuine API-shape change — evaluated in
P2-T21/P2-T22). Proceeding to P2-T21 to re-grep and scan `QuickFiler.csproj` (the only project
with its own diagnostics) as the Layer 3 baseline.
