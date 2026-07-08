# P5-T1 — MyBox.DialogInvoker Seam Verification

- Timestamp: 2026-06-14T15-10
- Command: Read UtilitiesCS/Dialogs/MyBox.cs (lines 28-43); confirm settable internal seam defaulting to real dialog.
- EXIT_CODE: 0

## Output Summary

PASS. The settable internal dialog-invoker seam exists and no new MyBox seam is required.

- File: `UtilitiesCS/Dialogs/MyBox.cs`
- Declaration (lines 39-43):
  `internal static Func<MyBoxViewer, DialogResult> DialogInvoker { get => _dialogInvoker.Value ?? RealDialogInvoker; set => _dialogInvoker.Value = value; }`
- Backing store (lines 28-29): `private static readonly AsyncLocal<Func<MyBoxViewer, DialogResult>> _dialogInvoker` — per-async-flow storage so parallel test classes do not contaminate each other.
- Default (lines 31-32): `RealDialogInvoker = viewer => viewer.ShowDialog();` — when `_dialogInvoker.Value` is unset, the getter returns the real dialog. Production behavior is unchanged.

Verification result: the seam is internal, both get and set are present, and it defaults to the real dialog when unset. No new MyBox production seam is needed. The only UtilitiesCS production change required is the `[assembly: InternalsVisibleTo("ToDoModel.Test")]` attribute (P5-T2) so `ToDoModel.Test` can set the internal `DialogInvoker`. The change does not exceed an assembly attribute; no flag-and-stop.

`ProjectEntry.SetProjectId`/`ChangeId` route their dialogs through `MyBox.ShowDialog`, which dispatches through `DialogInvoker`, so the malformed-ID and change-confirmation branches are reachable by injecting a stub `DialogInvoker`. (Note: the legacy `ProjectID` property setter uses raw `MessageBox.Show` and is not in scope for P5-T3, which targets `SetProjectId`/`ChangeId`/`CompareTo`.)
