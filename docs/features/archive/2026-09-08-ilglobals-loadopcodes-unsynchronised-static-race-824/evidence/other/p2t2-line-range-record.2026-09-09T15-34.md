# P2-T2 static-constructor line-range record (Issue #824)

Timestamp: 2026-09-09T15-34

This is the completion note P2-T2 requires and the record P4-T1 and P6-T1 cite. It fixes the four
line numbers observed in `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` immediately after
the P2-T2 edit and before P2-T3 adds its using directive.

## The four line numbers

| Locator | Line |
|---|---|
| Static constructor declaration, `static ILGlobals()` | 138 |
| Closing brace of the reflection loop | 165 |
| Assignment `singleByteOpCodes = singleTable;` | 166 |
| Assignment `multiByteOpCodes = multiTable;` | 167 |
| Closing brace of the static constructor | 168 |

Both assignment line numbers, 166 and 167, are greater than the reflection loop's closing brace at
165 and less than the static constructor's closing brace at 168. This is the mechanical form of the
requirement that the constructor "assigns each field exactly once after the loop".

## Grep results at this point in the run

| Pattern | Count |
|---|---|
| `static ILGlobals\(\)` | 1 |
| `Invalid OpCode\.` | 1 |
| `(singleByteOpCodes\|multiByteOpCodes)\s*=` | 2, at lines 166 and 167 |

The count of one for `Invalid OpCode.` confirms the original loop was deleted from `LoadOpCodes()`
rather than duplicated: two reflection loops would have produced two throws, two pairs of field
assignments, and a match count of four on the third pattern.

## Local names

The locals are named `singleTable` and `multiTable`. They are deliberately not named
`singleByteOpCodes` or `multiByteOpCodes`: a local of either of those names would shadow the field
it must assign, would force the assignment into the `ILGlobals.singleByteOpCodes = ...` form, and
would raise the third pattern's match count from two to four, which would then fail the
repository-wide P4-T1 sweep whose acceptance requires exactly two matching lines.

## Behaviour preserved from the original loop

- the `info1.FieldType == typeof(OpCode)` guard;
- the `num2 < 0x100` single-byte / multi-byte split;
- the `(num2 & 0xff00) != 0xfe00` classification;
- `throw new Exception("Invalid OpCode.")`.

The only substantive change is that the element writes target the locals `singleTable` and
`multiTable` rather than the fields, and each field is assigned once after the loop completes.

## Note on line-number stability

These numbers are superseded as soon as P2-T3 adds the `using System.Runtime.CompilerServices;`
directive, which shifts every line below it by one, and again by the P5-T1 format pass. P4-T1 and
P5-T13 therefore re-derive all three bounding line numbers at the time they run and record any
difference against the values above, rather than comparing against a frozen upper bound.
