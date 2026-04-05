# P2-T1: InputBox Dialog-Invoker Seam

Timestamp: 2026-03-27T09-40
File: UtilitiesCS\Dialogs\InputBox.cs
Seam Member Name: DialogInvoker

## Details

Added internal static property:
```csharp
internal static Func<InputBoxViewer, DialogResult> DialogInvoker { get; set; } =
    viewer => viewer.ShowDialog();
```

The production `ShowDialog` method now calls `DialogInvoker(viewer)` instead of `viewer.ShowDialog()` directly. Tests inject a controlled delegate to avoid real modal display.
