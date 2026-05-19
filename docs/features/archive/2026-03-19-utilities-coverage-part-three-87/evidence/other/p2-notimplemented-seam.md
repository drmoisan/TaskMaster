# P2-T7 Evidence: NotImplementedDialog.cs Notification Seam

File: UtilitiesCS\Dialogs\NotImplementedDialog.cs
Seam Member: NotImplementedDialog.DisplayInvoker
Seam Type: internal static Func<MyBoxViewer, DialogResult>
Default Value: viewer => viewer.ShowDialog()

## Callsite Replaced

StopAtNotImplemented: `DisplayInvoker(_box)` replaces `_box.ShowDialog()`

## Effect

The seam allows tests to inject a non-modal stub that returns either
DialogResult.Yes (throw-exception path) or DialogResult.No (keep-running path),
enabling full coverage of StopAtNotImplemented without displaying any WinForms dialog.
