# P2-T7 — Solution analyzer build after the issue #486 fixes

Timestamp: 2026-08-28T00-30
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

`EXIT_CODE: 0`. `Build succeeded.` with `5 Warning(s)` and `0 Error(s)`. The warning count is
identical to the P0-T11 baseline of 5, and every one is the same pre-existing `System.Reactive`
`packages.config` advisory, one each for `UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel`,
`QuickFiler` and `TaskMaster`. There is no `CS` diagnostic of any kind, so the Phase 2 deletions
introduced no analyzer or compiler finding.

## Why this build is the proof that the designer wirings are gone

P2-T2 deleted both `MenuItem_CheckedChanged` overloads from
`QuickFiler/Viewers/ItemViewerExpanded.cs` and P2-T3 deleted the four
`CheckedChanged += new System.EventHandler(this.MenuItem_CheckedChanged);` statements from
`QuickFiler/Viewers/ItemViewerExpanded.Designer.cs`. Had any of the four `+=` statements survived,
it would reference a method that no longer exists and the compile would fail with `CS0103`. The
build reporting `0 Error(s)` is therefore a positive, falsifiable proof that the method deletion and
the wiring deletion landed in the same change, which is exactly the risk P2-T3 exists to control.

The same argument covers `QuickFiler/Viewers/ItemViewer.cs`, where P2-T4 deleted three members. Its
designer file `ItemViewer.Designer.cs` contains exactly one `+= new System` statement in the whole
file, at `:256`, and it is the `ParentChanged` wiring that Phase 4 removes — not a
`MenuItem_CheckedChanged` wiring — so no `CS0103` could arise from that file either, and none did.

## The one production addition also compiles

`CbxPictures_CheckedChanged` was added to `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`
at P2-T5 and subscribed in `WireIntentEvents` at P2-T6. Both the handler signature and the
subscription bind, so `IItemViewer.PicturesChanged` (`IItemViewer.cs:71`) and
`IItemViewer.PicturesChecked` (`:72`) are the surface the fix uses; no interface member was added or
changed.

## Diff state at this point

```
QuickFiler/Viewers/ItemViewerExpanded.cs           0 added,  22 deleted
QuickFiler/Viewers/ItemViewerExpanded.Designer.cs  0 added,   4 deleted
QuickFiler/Viewers/ItemViewer.cs                   0 added,  20 deleted
QuickFiler/Controllers/QfcItemController.EventHandlers.cs  5 added, 0 deleted
QuickFiler/Controllers/QfcItemController.EventWiring.cs    1 added, 0 deleted
```

Output Summary: The full-solution analyzer build **passes** after the issue #486 fixes:
`EXIT_CODE: 0`, `Build succeeded.`, `5 Warning(s)`, `0 Error(s)`, with the warning count equal to
the P0-T11 baseline and every warning the pre-existing `System.Reactive` `packages.config` advisory.
Zero `CS` diagnostics is the falsifiable proof that no `+=` statement survived any deleted method:
a stranded designer wiring would be `CS0103`. The two production additions — the
`CbxPictures_CheckedChanged` handler and its single subscription line in `WireIntentEvents` — bind
against the existing `IItemViewer.PicturesChanged` and `PicturesChecked` members with no interface
change.
