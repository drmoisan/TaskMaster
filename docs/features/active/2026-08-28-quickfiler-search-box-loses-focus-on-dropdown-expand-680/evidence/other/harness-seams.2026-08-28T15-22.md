# P1-T2 — Test Seam Confirmation (Issue #680)

Timestamp: 2026-08-28T15-22

All four seams were confirmed by reading the files. No file was modified by this task.

## (a) `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` — injected show delegate

Signature quoted verbatim (line 330):

```csharp
private static Harness CreateHarness(Action<ToolStripDropDown, Control, Point> show = null)
```

The harness's own show operation (lines 419-425) wraps the injected delegate and passes the live
`ToolStripDropDown` through to it:

```csharp
Action<ToolStripDropDown, Control, Point> show = (dropDown, owner, point) =>
{
    ShowCount++;
    _show?.Invoke(dropDown, owner, point);
    ShownOwner = owner;
    ShownLocation = point;
};
```

A test can therefore observe `dropDown.AutoClose` at the exact moment the show delegate runs, which
is the observation the P2-T1 host-seam tests depend on. The harness also exposes `ShowCount`,
`FocusPendingCount`, `FocusAnchorCount`, `CancelCount`, and `FactoryCount`, and the helpers
`Open(host, anchor, work, desired)` (3-parameter gesture open, line 342),
`Close(host, reason)` (line 352), and `Property<T>(host, property)` (line 380).

## (b) `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` — synchronous 4-parameter open

Signature quoted verbatim (line 222):

```csharp
private static bool OpenWithFocusIntent(
    object host,
    Rectangle anchor,
    Rectangle work,
    Size desired,
    bool takeFocus
) =>
    ((IBreadcrumbDropDownHost)host)
        .OpenAsync(anchor, work, desired, takeFocus)
        .GetAwaiter()
        .GetResult();
```

The `.GetAwaiter().GetResult()` drive makes this a synchronous seam, so an `[expect-fail]` assertion
surfaces on the MSTest thread rather than being swallowed by an unobserved task. The file declares
`public sealed partial class BreadcrumbDropDownHostTests` in namespace `QuickFiler.Test.Viewers`
and carries exactly four using directives: `System.Drawing`, `FluentAssertions`,
`Microsoft.VisualStudio.TestTools.UnitTesting`, `QuickFiler.Viewers`. Because it is a partial of the
primary test class it shares `Harness`, `CreateHarness`, `Open`, `Close`, and `Property<T>`, so
P2-T1 needs no new file and no `.csproj` entry. Current length: 234 lines.

## (c) `QuickFiler.Test/Controllers/QfcItemController.SearchFocusRegressionTests.cs` — headless controller pattern

Class `QfcItemController_SearchFocusRegressionTests` in namespace `QuickFiler.Controllers.Tests`.
The pattern P2-T6 mirrors is `BuildController` (line 252): construct a bare `HarnessController`, then
inject collaborators by reflection —

```csharp
HarnessController controller = new HarnessController();
QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
QfcItemControllerTestSupport.SetField(controller, "_folderHandler", folderHandler.Object);
```

with `Mock<IItemViewer>` supplying the viewer. Entirely headless: no WinForms control, window
handle, or message pump. Assertions are Moq `Verify(..., Times.Never())` / `Times.Once()` against
the viewer mock. This suite is one of the nine files pinned byte-unmodified by P4-T1.

## (d) `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` — wiring-test pattern

`WireIntentEvents_SubscribesEveryIntentEvent` at line 240 establishes the `VerifyAdd` pattern with a
mocked `IQfcKeyboardHandler`:

```csharp
var viewer = new Mock<IItemViewer>();
var kbd = new Mock<IQfcKeyboardHandler>();
var controller = new HarnessController();
QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", kbd.Object);
controller.WireIntentEvents();
viewer.VerifyAdd(v => v.SearchKeyDown += It.IsAny<KeyEventHandler>(), Times.Once());
```

The matching `VerifyRemove` half is `UnwireIntentEvents_DetachesPicturesChanged` at
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs:88` (currently 105 lines),
which is the file P2-T7 appends its two tests to.

Acceptance: satisfied — the artifact exists and quotes both helper signatures verbatim.
