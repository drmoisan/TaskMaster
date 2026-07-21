# P2-T21 — Layer 3 Baseline: QuickFiler.csproj (Only Project P2-T20 Reported as Newly-Surfaced)

Timestamp: 2026-07-20T02-25

Command: `MSBuild.exe QuickFiler/QuickFiler.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true`

EXIT_CODE: 1

## Scope note

P2-T20's rebuild reported 5 FAILED projects: `QuickFiler.csproj` (15 own diagnostics),
`TaskMaster.csproj`, `QuickFiler.Test.csproj`, `UtilitiesCS.Test.csproj`, `TaskMaster.Test.csproj`
(these 4 fail solely as cascading dependency failures on `QuickFiler.csproj`'s missing output
assembly — grep-confirmed zero own `error` lines for each of the 4 in the P2-T20 log). Per this
task's own instruction ("named in P2-T20's output"), only `QuickFiler.csproj` is the specific
project with genuinely newly-surfaced diagnostics; the 4 cascading projects will be
re-scanned/re-rebuilt after `QuickFiler.csproj` is fixed (in the P2-T23 final gate re-run) rather
than scanned independently now, since they currently have no diagnostics of their own to
enumerate.

## Current file list and per-diagnostic-code counts (14 unique diagnostic sites)

- **CS0108** (4 sites) — `QuickFiler/Viewers/IItemViewer.cs`:
  - line 119: `InvokeRequired` hides `ISynchronizeInvoke.InvokeRequired`
  - line 120: `Invoke(Delegate)` hides `IControl.Invoke(Delegate)`
  - line 121: `BeginInvoke(Delegate)` hides `IControl.BeginInvoke(Delegate)`
  - line 122: `Height` hides `IControl.Height`
- **CS0618** (8 sites) — obsolete `System.Linq.Async` `AsyncEnumerable` extension-method usage:
  - `QuickFiler/Controllers/QfcQueue.cs(393,29)` — `SelectAwait`
  - `QuickFiler/Helper Classes/ConversationResolver.cs(180,33)` — `SelectAwaitWithCancellation`
  - `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs(215,38)` — `SelectAwait`
  - `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs(220,39)` — `SelectAwait`
  - `QuickFiler/Controllers/QfcDatamodel.cs(415,23)` — `ForEachAwaitWithCancellationAsync`
  - `QuickFiler/Controllers/QfcCollectionController.cs(763,19)` — `ForEachAsync`
  - `QuickFiler/Controllers/QfcCollectionController.cs(822,19)` — `ForEachAsync`
  - `QuickFiler/Controllers/QfcCollectionController.cs(2200,19)` — `ForEachAwaitAsync`
- **CS8600** (2 sites) — `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`:
  - line 296: "Converting null literal or possible null value to non-nullable type."
  - line 341: "Converting null literal or possible null value to non-nullable type."

This is the authoritative current diagnostic list for this layer, filtered from this task's own
fresh isolated rebuild (not carried over unchanged from P2-T20, though the counts match).
