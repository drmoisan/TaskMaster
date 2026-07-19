# Maintainer Flags — Pre-Existing Conditions (flagged, not fixed)

- Timestamp: 2026-07-19T10-50
- Feature: utilitiescs-nullable-outlook-mailitem-item (#371)

This artifact records pre-existing conditions surfaced during nullable remediation that are, by
plan and spec scope, flagged for future work rather than fixed in this annotation-only child.

## Flag 1 (P3-T6): `dynamic item` in `OlToDoTable.EnsureItemValues` is invisible to nullable-flow analysis

- File: `UtilitiesCS/OutlookObjects/Table/OlToDoTable.cs`, method `EnsureItemValues`.
- Line: `dynamic item = itemObj;` (followed by `item.PropertyAccessor`, `item.EntryID`, `item.Save()`).
- Condition: member access through a `dynamic` reference is not analyzed by the C# nullable-flow
  analyzer; the compiler cannot verify null-safety through the `dynamic` call sites. The
  `#nullable enable` pragma therefore cannot prove or enforce null-safety for the values that flow
  out of `item.*`.
- Decision: FLAGGED, NOT FIXED. Converting `dynamic item = itemObj;` to a typed access pattern
  (e.g., casting `itemObj` to a concrete Outlook item interface or `OutlookItem`) would be a
  behavior-risk refactor (the loop deliberately accepts heterogeneous Outlook item types and relies
  on late binding), which is out of scope for this annotation-only remediation. The `dynamic item =
  itemObj;` line is left byte-unchanged. Surrounding locals (`itemObj`, `entryId`, `value`) were
  annotated nullable where the compiler can see them; the `dynamic`-sourced values remain outside
  nullable analysis by design.
- Recommended follow-up: open a separate issue to evaluate replacing the `dynamic` access with a
  typed `IOutlookItem`/reflection-wrapper path if stronger null guarantees are later required.

## Flag 2 (P4-T2): `OutlookItem.cs` exceeds the 500-line file-size limit (pre-existing)

- File: `UtilitiesCS/OutlookObjects/Item/OutlookItem.cs`.
- Condition: the file was already 503 lines before this remediation, exceeding the repo 500-line
  file-size limit. This is a pre-existing condition, not introduced by #371.
- Effect of this remediation: annotation-only work (the `#nullable enable` pragma line plus `?`/`!`
  annotations) adds ONE line, moving the file to 504 lines — further over 500, not under it.
- Decision: FLAGGED, NOT FIXED. Splitting `OutlookItem.cs` into multiple files would be a refactor,
  which is out of scope for this annotation-only child (and would itself be a partial-class-group
  change requiring its own review). The file is left intact at 504 lines.
- Recommended follow-up: open a separate issue to split `OutlookItem.cs` (e.g., separating the
  predefined-property surface from the reflection-helper internals) to bring it under the 500-line
  limit.

## Note: `OutlookItem`-family unconstrained-generic `T?` contract (deliberate, not a defect)

- The `OutlookItem`/`OutlookItemExtensions`/`OutlookItemTry`/`OutlookItemTryGet`/`OutlookItemFlaggableTry`
  reflection-wrapper family uses `TryGet<T>`/`TryCall<T>`/`GetPropertyValueIfExists<T>` helpers that
  return `default(T)` on a swallowed exception. These were given an explicit unconstrained `T?`
  return/`out T?` contract (per spec and research Section 7). This propagates to the try/catch-
  swallowing decorators' reference-type public members, which are annotated nullable to reflect that
  they genuinely can return `default(T)`/null on failure. Value-type members are unaffected
  (unconstrained `T?` is a no-op for value types at runtime). The decorator seams
  (`OutlookItemTry`/`OutlookItemTryGet`/`OutlookItemFlaggableTry` over `IOutlookItem`/
  `IOutlookItemFlaggable`) are preserved exactly; the out-of-scope interfaces remain oblivious and
  are not cross-blocked. `ItemType`/`_type`/`_item` reflection derefs that require a constructed
  wrapper use a justified `!` (preserving the original NRE-caught-by-surrounding-try behavior);
  error-log-string derefs use `?.` (defensive, avoids a secondary crash during error logging).
