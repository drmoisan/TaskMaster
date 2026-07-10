# Phase 0 — Precondition: IForm.cs (P0-T3)

Timestamp: 2026-07-09T22-00

- File: UtilitiesCS/Interfaces/IWinForm/IForm.cs
- Declaration: `public interface IForm : IContainerControl, IScrollableControl`
  - `IContainerControl` resolves to `System.Windows.Forms.IContainerControl`
    (the repo-local interface is named `IContainerControlLocal`).
  - `IScrollableControl` resolves to `UtilitiesCS.Interfaces.IWinForm.IScrollableControl`,
    which derives from `UtilitiesCS.Interfaces.IWinForm.IControl`.
- `IControl` (UtilitiesCS/Interfaces/IWinForm/IControl.cs) extends
  `IComponent, IDropTarget, ISynchronizeInvoke, IWin32Window, IDisposable, IBindableComponent`.

Form-level / Control-level members ITaskViewer inherits via the base chain (confirmed present):
- From IForm: `AcceptButton`, `CancelButton`, `DialogResult`, `ShowDialog()`.
- From IControl (via IScrollableControl): `InvokeRequired` (ISynchronizeInvoke),
  `Invoke(Delegate)`, `Hide()`, `Dispose()` (IDisposable), `Focus()`, `Controls`,
  `Visible`, `Enabled`, `BackColor`.

Note (implementation consequence): `IControl` declares only `object Invoke(Delegate method)`,
not `Invoke(Action)`. Controller re-invoke one-liners that pass a lambda
(`_viewer.Invoke(() => ...)`) require an explicit delegate cast (`(Action)(...)`) after
the retarget to `ITaskViewer`. Recorded here for P3-T1/P3-T3.

AC: interface path, base interfaces, and inherited Form-level member surface CONFIRMED.
