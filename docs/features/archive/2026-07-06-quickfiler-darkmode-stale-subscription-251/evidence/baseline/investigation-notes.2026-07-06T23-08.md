# Investigation Notes (Issue #251)

Timestamp: 2026-07-06T23-28

Command: (source inspection via Read/Grep, no external command)

EXIT_CODE: 0

Output Summary:

(a) `IOlObjects` (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:11`) extends `INotifyPropertyChanged` and declares `bool DarkMode { get; set; }` (line 30). `IApplicationGlobals.Ol` (`UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs:11`) is `IOlObjects Ol { get; }` (get-only), directly mockable with `Mock<IOlObjects>` / `Mock<IApplicationGlobals>`.

(b) `QfcCollectionController` constructor (`QuickFiler/Controllers/QfcCollectionController.cs:29-52`) has 8 parameters:
  1. `IApplicationGlobals AppGlobals` — mockable interface.
  2. `IQfcFormViewer viewerInstance` — mockable interface; constructor dereferences `.L1v0L2L3v_TableLayout` (`TableLayoutPanel`) and `.L1v0L2_PanelMain` (`Panel`), both nullable-returning on a loose mock, only assigned to fields (no further dereference in the constructor).
  3. `QfEnums.InitTypeEnum InitType` — enum, constructed as a literal value (e.g., `QfEnums.InitTypeEnum.Sort`).
  4. `IFilerHomeController homeController` — mockable interface; constructor dereferences `.KeyboardHandler` (`IQfcKeyboardHandler`, `IFilerHomeController.cs:32`), mockable and settable on the mock.
  5. `IFilerFormController parent` — mockable interface, only assigned to a field.
  6. `CancellationTokenSource tokenSource` — concrete BCL type, constructed directly (`new CancellationTokenSource()`).
  7. `CancellationToken token` — concrete BCL struct, obtained directly (e.g., `tokenSource.Token`).
  8. `TlpCellStates tlpStates` — concrete `Dictionary<string, TlpCellSnapShotList>` subclass (`QuickFiler/Helper Classes/TlpCellSnapShot.cs:12-15`) with a public parameterless constructor and no virtual seam; constructed directly as `new TlpCellStates()`.
  The constructor also calls `SetupLightDark(_globals.Ol.DarkMode)` (line 51), which dereferences `AppGlobals.Ol.DarkMode` and subscribes `_globals.Ol.PropertyChanged += DarkMode_CheckedChanged` (line 2115).

(c) A loose `Mock<IQfcFormViewer>` returns `null` for `.L1v0L2L3v_TableLayout` and `.L1v0L2_PanelMain` by default (no `Setup` required); the constructor only assigns these to private fields (`_itemTlp`, `_itemPanel`) without further dereference, so no real `TableLayoutPanel`/`Panel`/WinForms control construction occurs during construction. `_itemGroups` (private `List<QfcItemGroup>`) is not assigned in the constructor and remains `null` until a separate initialization path runs; `RemoveControls()` (line 976-978) and `RemoveControlsAsync()` (line 1009-1011) both guard with `if (_itemGroups is not null)`, so `Cleanup()`/`CleanupAsync()` take the early-exit branch and require no `TableLayoutPanel` interaction in the regression test.

(d) Grep of all production `*.cs` files under `QuickFiler/` for `DarkMode_CheckedChanged` (excluding test/evidence files) confirms the only references to `QfcCollectionController.DarkMode_CheckedChanged` are within `QfcCollectionController.cs` itself: the subscribe at line 2115 (`SetupLightDark`) and the method definition at line 2118. `QfcFormController.EventHandlers.cs:22` and `QfcFormController.SetupDisposal.cs:84,212` declare and reference a separate, unrelated `DarkMode_CheckedChanged` method scoped to the different class `QfcFormController`, which is out of scope for this fix and already unsubscribes correctly.

(e) `QfcFormController.SetupDisposal.cs:208-213` implements the reference unsubscribe pattern to mirror in `QfcCollectionController`:
```
public void Cleanup()
{
    if (_globals?.Ol is not null)
    {
        _globals.Ol.PropertyChanged -= DarkMode_CheckedChanged;
    }
    ...
```

Current (pre-fix) `QfcCollectionController.cs` state confirmed by direct read:
- `CleanupAsync()` at lines 2152-2160: no unsubscribe before `_globals = null;` (line 2156).
- `Cleanup()` at lines 2162-2170: no unsubscribe before `_globals = null;` (line 2166).
- `DarkMode_CheckedChanged(object sender, EventArgs e)` at lines 2118-2130: dereferences `_globals.Ol.DarkMode` directly (line 2121) with no cleaned-up guard, matching the reported `NullReferenceException` at `QfcCollectionController.cs:2121`-equivalent behavior described in `issue.md`.
