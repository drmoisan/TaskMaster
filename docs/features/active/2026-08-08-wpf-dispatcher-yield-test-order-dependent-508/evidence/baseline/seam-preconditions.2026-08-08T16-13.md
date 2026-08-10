# Seam Preconditions Confirmation

Timestamp: 2026-08-08T16-13

Task: [P0-T5]

Four preconditions required by the plan's `## Design Decision — Seam Shape` section, each verified
against the working tree at HEAD `003c5715`.

## Precondition 1 — `InternalsVisibleTo("UtilitiesCS.Test")`

CONFIRMED at `UtilitiesCS/Properties/AssemblyInfo.cs:19`.

```csharp
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]   // line 18
[assembly: InternalsVisibleTo("UtilitiesCS.Test")]           // line 19
[assembly: InternalsVisibleTo("ToDoModel.Test")]             // line 20
```

Consequence: an `internal` seam constructor on `WpfDispatcherYield` is reachable from
`UtilitiesCS.Test` without widening the public API surface. This is what allows the strongest
possible answer to AC4 ("no public surface change beyond the explicit parameterless constructor").

## Precondition 2 — `<LangVersion>Latest</LangVersion>`

CONFIRMED at `UtilitiesCS.Test/UtilitiesCS.Test.csproj:18`.

```xml
<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>   <!-- line 17 -->
<LangVersion>Latest</LangVersion>                          <!-- line 18 -->
```

Consequence: nullable annotations (`Func<Dispatcher?>`) and `#nullable enable` are usable in the
test project. Target framework is `v4.8.1`, so no `init` accessor / `record struct` (no
`IsExternalInit` on net481) — not needed by this plan's shape.

## Precondition 3 — `#nullable enable` in peer files under `UtilitiesCS.Test/OutlookObjects/Folder/`

CONFIRMED. Seven peer files in that directory already open with `#nullable enable`:

```
UtilitiesCS.Test\OutlookObjects\Folder\FolderBreadcrumbRouterSelectionConcurrencyTests.cs
UtilitiesCS.Test\OutlookObjects\Folder\BreadcrumbSubfolderSelectorSessionTests.cs
UtilitiesCS.Test\OutlookObjects\Folder\BreadcrumbStateModelSelectorTests.cs
UtilitiesCS.Test\OutlookObjects\Folder\BreadcrumbSelectionSessionTests.cs
UtilitiesCS.Test\OutlookObjects\Folder\BreadcrumbSelectorMessagesTests.cs
UtilitiesCS.Test\OutlookObjects\Folder\BreadcrumbRenderProjectionSelectorTests.cs
UtilitiesCS.Test\OutlookObjects\Folder\BreadcrumbDuplicateIdentityTests.cs
```

Consequence: the per-file `#nullable enable` opt-in that P1-T8 adds matches established practice in
this exact directory; it is not a novel pattern.

## Precondition 4 — the two `new WpfDispatcherYield()` call sites

CONFIRMED. Repository-wide grep for `new WpfDispatcherYield()` across `**/*.cs`:

```
TaskMaster\AppGlobals\AppOlObjects.FolderTreeService.cs:365:                new WpfDispatcherYield()
UtilitiesCS.Test\OutlookObjects\Folder\WpfDispatcherYieldTests.cs:16:            var dispatcherYield = new WpfDispatcherYield();
UtilitiesCS.Test\OutlookObjects\Folder\WpfDispatcherYieldTests.cs:31:            var dispatcherYield = new WpfDispatcherYield();
UtilitiesCS.Test\OutlookObjects\Folder\OutlookFolderTreeServiceConcurrencyTests.cs:55:                        new WpfDispatcherYield()
```

Two of the four hits are inside the in-scope test file itself and are replaced by the Phase 1
rewrite. The two out-of-scope call sites are exactly the two the plan names:

- `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:365` (production)
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs:55` (test)

Consequence: adding a seam constructor removes the implicit parameterless constructor, so P1-T3's
explicit `public WpfDispatcherYield()` is mandatory. Both out-of-scope call sites must compile
unchanged with zero edits.

## Supporting reference — `StaDispatcherHost` precedent

`UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderYieldTests.cs:118-147` contains the
pumping STA host pattern that P0-T12 and P1-T9 reuse: an STA thread that captures
`Dispatcher.CurrentDispatcher`, signals an `AutoResetEvent`, calls `Dispatcher.Run()`, and disposes
via `BeginInvokeShutdown(DispatcherPriority.Send)` + `Join()`. This is a genuinely pumping
dispatcher, which is required because `InvokeAsync(..., DispatcherPriority.Background, ...)` never
completes against a non-pumping dispatcher.

Output Summary: PASS. All four seam preconditions confirmed at the exact file/line locations the
plan names: `InternalsVisibleTo("UtilitiesCS.Test")` at `AssemblyInfo.cs:19`,
`<LangVersion>Latest</LangVersion>` at `UtilitiesCS.Test.csproj:18`, seven peer files already using
`#nullable enable` in the same directory, and exactly two out-of-scope `new WpfDispatcherYield()`
call sites. The `StaDispatcherHost` precedent at `FolderTreeSnapshotBuilderYieldTests.cs:118-147`
was also confirmed for reuse.
