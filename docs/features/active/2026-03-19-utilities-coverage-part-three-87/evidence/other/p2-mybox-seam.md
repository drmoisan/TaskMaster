# P2-T4 Evidence: MyBox.cs Dialog-Invoker Seam

File: UtilitiesCS\Dialogs\MyBox.cs
Seam Member: MyBox.DialogInvoker
Seam Type: internal static Func<MyBoxViewer, DialogResult>
Default Value: viewer => viewer.ShowDialog()

## Callsites Replaced

Overload 1 (DelegateButton): `DialogInvoker(_viewer)`
Overload 2 (BoxIcon + ActionButton): `DialogInvoker(viewer)`
Overload 3 (generic FunctionButtonGroup): `DialogInvoker(viewer)`
Overload 4 (MessageBoxIcon + ActionButton): `DialogInvoker(viewer)`

## Effect

The seam allows tests to inject a non-modal stub for DialogInvoker,
replacing the blocking viewer.ShowDialog() call with a deterministic
return value, enabling full coverage without displaying any WinForms dialog.
