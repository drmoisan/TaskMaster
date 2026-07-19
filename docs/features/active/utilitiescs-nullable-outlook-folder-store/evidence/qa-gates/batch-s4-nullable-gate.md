# Batch S4 Nullable Gate (P11-T3)

Timestamp: 2026-07-19T16-25

- csharpier format EXIT 0; full /t:Build EXIT 0.
- Scoped gate: **zero CS86xx** for the 2 Batch S4 files (AC1).

## Files remediated (2): StoreWrapperViewer.cs, DisabledStoresViewer.cs
- `StoreWrapperViewer.Controller` (StoreWrapperController) nullable-by-design, matching the existing
  `Controller?.` guard usage; all event-handler senders `object?` (WinForms `EventHandler` shape and the
  IStoreWrapperViewer interface set in S0). No new runtime guards introduced (AC4).
- `DisabledStoresViewer.Dgv_CellContentClick(object? sender, ...)`; `Dgv` DataGridView property left backed by
  the oblivious Designer field; `Controller` set in the ctor.
- Cross-batch consistency: StoreWrapperController's `DisplayName_SelectedValueChanged` and
  `ExcludeStore_CheckedChanged` senders widened to `object?` so the viewer's `object?`-forwarding compiles.
- The two Designer-generated siblings `StoreWrapperViewer.Designer.cs` and `DisabledStoresViewer.Designer.cs`
  were **NOT** pragma-annotated (verified they still start with `namespace`, no `#nullable enable`), per repo
  convention. No post-condition attributes; no record/init.
