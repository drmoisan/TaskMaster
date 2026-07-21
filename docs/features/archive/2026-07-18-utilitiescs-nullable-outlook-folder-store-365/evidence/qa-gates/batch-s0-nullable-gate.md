# Batch S0 Nullable Gate (P6-T3)

Timestamp: 2026-07-19T14-00

- csharpier format EXIT 0; full /t:Build EXIT 0.
- Scoped gate (UtilitiesCS Rebuild, TreatWarningsAsErrors, BuildProjectReferences=false): **zero CS86xx** for the
  3 Batch S0 files (AC1).

## Files remediated (3): IDisabledStoresViewer.cs, IStoreWrapperViewer.cs, DisabledStoreRow.cs
- IDisabledStoresViewer.BindRows kept non-nullable by contract; Dgv non-null; event-handler method senders
  set to `object?` (WinForms delegate shape, decided here for the S3/S4 concrete-viewer implementations).
- IStoreWrapperViewer WinForms control properties (Label/Button/ComboBox/CheckBox) kept non-null by
  post-construction contract; event-handler senders `object?`.
- DisabledStoreRow: `StoreIdentity Identity` is a value-type struct (no CS8618); `DisplayName`/`ScopeLabel`
  initialized `= string.Empty` for explicit non-null discipline; `IsFutureSession` bool.
- No post-condition attributes; no record/init.
