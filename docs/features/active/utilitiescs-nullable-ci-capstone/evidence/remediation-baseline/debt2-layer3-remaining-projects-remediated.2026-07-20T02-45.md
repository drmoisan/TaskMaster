# P2-T22 — Layer 3 Remediation: QuickFiler.csproj (14 unique diagnostic sites)

Timestamp: 2026-07-20T02-45

## Remediation applied, per diagnostic

| File | Line(s) | Code | Pattern applied | Rationale |
|---|---|---|---|---|
| `QuickFiler/Viewers/IItemViewer.cs` | 119-122 | CS0108 (x4) | Narrow pragma bracket | Four interface members deliberately re-declared for mockability (existing comment above them documents this); adding `new` or restructuring the interface hierarchy is an API-shape change, out of scope per AC7 |
| `QuickFiler/Controllers/QfcQueue.cs` | 393-412 (statement) | CS0618 (`SelectAwait` x2 in one statement, 1 reported site) | Narrow pragma bracket | Migrating to the new `Select` overload is a call-shape change; suppression preserves exact behavior |
| `QuickFiler/Helper Classes/ConversationResolver.cs` | 180-196 (statement) | CS0618 (`SelectAwaitWithCancellation`) | Narrow pragma bracket | Same as above |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 215-223 (two statements) | CS0618 (`SelectAwait` x2) | Narrow pragma bracket | Same as above |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 415-434 (statement) | CS0618 (`ForEachAwaitWithCancellationAsync`) | Narrow pragma bracket | Replacing with `await foreach` is a control-flow change; suppression preserves exact behavior |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 763-766 (statement) | CS0618 (`ForEachAsync`) | Narrow pragma bracket | Same as above |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 822-825 (statement) | CS0618 (`ForEachAsync`) | Narrow pragma bracket | Same as above |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2200-2203 (statement) | CS0618 (`ForEachAwaitAsync`) | Narrow pragma bracket | Same as above |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 296 | CS8600 | Nullable annotation (`FolderTreeNodeKey` -> `FolderTreeNodeKey?`) | Local variable is already checked `if (key == null)` immediately afterward; declaring it nullable accurately reflects existing behavior with zero control-flow change. File already has `#nullable enable` at line 1. |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 341 | CS8600 | Nullable annotation (`FolderTreeNodeKey` -> `FolderTreeNodeKey?`) | Same as above |

All fixes fall within the three authorized patterns (nullable annotation for the 2 CS8600 sites;
narrow pragma bracket with rationale comment for the 4 CS0108 sites and 8 CS0618 sites). No
diagnostic required a behavior change; no escalation was necessary.

## Verification

Command: `MSBuild.exe QuickFiler/QuickFiler.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Output Summary: Build succeeded, 0 Warning(s), 0 Error(s). All 14 previously-reported diagnostic
sites (CS0108 x4, CS0618 x8 unique call sites, CS8600 x2) no longer appear.
