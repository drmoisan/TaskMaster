# Final QC — Scope Guards (P9-T8)

Timestamp: 2026-07-19T22-03

## Over-limit in-scope files NOT split

Each of the five pre-existing >500-line in-scope files remains a single file (line counts grew
only from added `#nullable enable` pragma + annotation lines; none was split, which would be an
out-of-scope refactor):

| File | Lines (current) | Pre-existing baseline |
|---|---|---|
| Observable/ObservableDictionary.cs | 836 | 834 |
| NewSmartSerializable/SmartSerializable.cs | 613 | 596 |
| Serializable/SerializableList.cs | 584 | 575 |
| NewSmartSerializable/SmartSerializableBase.cs | 545 | 534 |
| Locking/Observable/LinkedList/LockingObservableLinkedList.cs | 528 | 522 |

The pre-existing >500-line condition is flagged for a separate future issue per the plan Scope
Invariants; this annotation-only child did not split any file.

## No record / init / record struct conversion

- `grep -rEn "\brecord\s+(struct\s+)?[A-Z]|\{\s*get;\s*init;\s*\}" UtilitiesCS/ReusableTypeClasses`
  count: **0**.
- No type was converted to `record`, `record struct`, or `{ get; init; }` (all of which fail
  CS0518 on net481, which lacks `IsExternalInit`). AC3/AC5 scope compliance confirmed.
