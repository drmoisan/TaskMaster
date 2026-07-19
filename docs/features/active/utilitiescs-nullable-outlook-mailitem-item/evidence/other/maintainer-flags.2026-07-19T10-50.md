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
