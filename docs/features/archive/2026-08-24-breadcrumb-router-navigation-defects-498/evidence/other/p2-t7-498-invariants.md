# P2-T7 — #498 Invariants That Must Survive

Timestamp: 2026-08-26T09-24

Command: `git diff --stat <BASELINE_COMMIT> -- UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs; git hash-object UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs; wc -l UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs; sed -n '190,229p' UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs; grep -n "OnHostMessageReceived" QuickFiler/Controllers/BreadcrumbBridgeRouter.cs; grep -n "catch" QuickFiler/Controllers/BreadcrumbBridgeRouter.cs; sed -n '287,299p' QuickFiler/Controllers/BreadcrumbBridgeRouter.cs; grep -n "segmentDoubleClick for row" QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`

EXIT_CODE: 0

## Output Summary

All three invariant checks HOLD.

| # | Check | File and range inspected | Result |
|---:|---|---|:--:|
| 1 | `CollapseAfter`, its XML contract and its `ArgumentOutOfRangeException` throw are byte-identical to the `P0-T16` state | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:190-229` (whole file 361 lines) | HOLDS |
| 2 | `OnHostMessageReceived` has exactly one catch clause and it is `catch (BreadcrumbMessageException)` | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:287-298` | HOLDS |
| 3 | The new `Error` log site exists in the `SegmentDoubleClick` arm | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:254-258` | HOLDS |

### Check 1 — `BreadcrumbRow.CollapseAfter` unmodified

`git diff --stat` against the feature's baseline commit reports **no output at all** for
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`, meaning the file is byte-identical to its
baseline state; its blob id is `369b7dfbb484d7d92405c3f5c98da979cf47c07a`. The file measures 361
lines, exactly the figure recorded for it in
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/baseline/p0-t16-ownership-and-line-counts.md`
(row 7, "delta: none").

Within that file the three cited regions are intact:

- `:190-199` — the XML documentation contract, including the
  `<exception cref="ArgumentOutOfRangeException">` clause stating that the parameter "is outside the
  segment list of a suggestion row".
- `:200-229` — the `CollapseAfter` body, in the documented order: banner/pseudo short-circuit at
  `:202-205`, the range check and its `throw` together at `:207-214`, leaf-index no-op, idempotent no-op, then
  the state assignment.
- `:207-214` — the `throw new ArgumentOutOfRangeException(...)` itself.

The throw contract still holds when `CollapseAfter` is called directly. The `P2-T3` RED run is the
positive proof that this throw was reachable before the fix, and the `P2-T4` fix was applied
entirely in the router, never in the row. This satisfies the AC-2 unmodified-row clause.

### Check 2 — no broad catch at the async void host-message boundary

`OnHostMessageReceived` now occupies `:287-298`. It relocated from its pre-fix `:266-277` purely
because `P2-T4` added 21 lines above it; the body is otherwise unchanged. It contains exactly one
`catch` clause:

```
            catch (BreadcrumbMessageException)
```

at `:293`, with its original comment. `grep -n "catch"` over the whole primary file returns four
hits, and the other three are accounted for and are not at this boundary:

- `:246` — a source comment inside the new `SegmentDoubleClick` arm that contains the word
  `catch (BreadcrumbMessageException)` in prose. It is not a clause.
- `:415` — `catch (OperationCanceledException)`, pre-existing, inside `ExpandLeafAsync`.
- `:421` — `catch (Exception ex)`, pre-existing, inside `ExpandLeafAsync`, at the provider I/O
  boundary with its own logging.

No broad `catch (Exception)` was added at the `async void` host-message boundary. This satisfies the
AC-3 no-broad-catch clause. The AC-3 wording cites this method at its pre-fix coordinates
`:266-277`; the method is the same method at `:287-298`.

### Check 3 — the rejected index is logged at `Error`

The `SegmentDoubleClick` arm now carries, at `:254-258`:

```
                        log.Error(
                            $"Inbound segmentDoubleClick for row '{row.RowId}' carries segment index "
                                + $"'{requestedIndex}', which is outside the valid range "
                                + $"[0, {row.Segments.Count - 1}]; rejected without a transition."
                        );
```

It uses the file's existing `log4net` static field declared at `:21`, matching the established
pattern at `:235` (unknown-row rejection) and at the arrow-key default arm. Together with check 1
this satisfies AC-2.

### Line-number shift recorded for downstream phases

`P2-T4` inserted 21 lines into `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, which grew from
410 to 431 lines. Everything at or above the `SegmentDoubleClick` arm keeps its coordinates;
everything below `:247` shifts by **+21**.

- **Phase 3 is unaffected.** Every citation `P3-T3` uses — `BindRowsAsync` at `:92-138`, the
  `_selectedRowId = null;` statement at `:136`, the `SelectedFolderPath` property at `:59`, the
  `SelectedFolderPathChanged` event at `:62`, and `SelectFirstRow` at `:192-199` — sits above the
  fix and is unchanged.
- **Phase 6 citations below the arm shift by +21**, for example `HandleArrowKeyAsync` `:304-339`
  becomes `:325-360` and `ExpandLeafAsync` `:364-408` becomes `:385-429`. This is recorded here so
  that a later phase resolves those members by name and re-derives the range rather than trusting
  the pre-fix figure.

Satisfies AC-2 and the AC-3 no-broad-catch clause.
