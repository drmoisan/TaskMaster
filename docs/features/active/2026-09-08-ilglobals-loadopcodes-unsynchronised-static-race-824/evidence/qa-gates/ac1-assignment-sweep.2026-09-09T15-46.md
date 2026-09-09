# AC1 repository-wide assignment sweep (Issue #824, task P4-T1)

Timestamp: 2026-09-09T15-46

Command: `Grep` with `-n` and `output_mode: content`, `head_limit: 0`, for the pattern
`(singleByteOpCodes|multiByteOpCodes)\s*=` over `*.cs` across the whole worktree.

EXIT_CODE: 0

## Output Summary

Exactly **two** matching lines were returned, both in
`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`, reproduced verbatim with their line
numbers:

```
UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:167:            singleByteOpCodes = singleTable;
UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:168:            multiByteOpCodes = multiTable;
```

No match exists anywhere else in the repository, including
`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` and every test file. Every other
reference to either field is a read.

## Bounds, re-derived by reading the file at the time this task ran

| Bound | Line |
|---|---|
| Static constructor declaration, `static ILGlobals()` | 139 |
| Closing brace of the reflection loop | 166 |
| Closing brace of the static constructor | 169 |

Both matched assignment lines, 167 and 168, fall after the reflection loop's closing brace at 166
and before the static constructor's closing brace at 169. This is the mechanical form of AC1's
requirement that each field be assigned exactly once **after** the loop rather than before it.

The static constructor's declaration line is re-derived so that its closing brace is *identified*
rather than chosen: the brace at 169 is the one closing the block opened at 140, which is the block
of the declaration at 139.

## Comparison with the P2-T2 record

Source: `evidence/other/p2t2-line-range-record.2026-09-09T15-34.md`.

| Bound | P2-T2 value | Re-derived here | Difference |
|---|---|---|---|
| Static constructor declaration | 138 | 139 | +1 |
| Closing brace of the reflection loop | 165 | 166 | +1 |
| Closing brace of the static constructor | 168 | 169 | +1 |
| Assignment, `singleByteOpCodes` | 166 | 167 | +1 |
| Assignment, `multiByteOpCodes` | 167 | 168 | +1 |

Every locator moved down by exactly one line. The cause is P2-T3, which added the
`using System.Runtime.CompilerServices;` directive above all of them and shifted every line below it
by one. P2-T2 records a counterpart for the loop's closing brace and the constructor's closing brace
but not for the constructor's declaration line, so the difference statement for that first bound is
made against the value this task derived rather than against a prior record of it.

This uniform +1 shift is exactly why the plan requires the bounds to be re-derived rather than
frozen: a comparison against P2-T2's upper bound of 168 would have rejected the assignment at line
168 on a line offset rather than on the property AC1 states.

P5-T13 re-runs this gate after the P5-T1 format pass, which can shift the numbers again.

## Assessment

AC1's assignment-site invariant holds: both opcode-table fields are assigned exactly twice in total
across the entire repository, once each, both inside the static constructor of `ILGlobals`, and both
after the reflection loop has completed.

The compiler corroborates this independently. Both fields are `public static readonly`, so any
assignment outside the static constructor is CS0198, an error rather than a warning. P2-T4 recorded
zero lines matching `: error [A-Z]+[0-9]+:`, so no such assignment exists anywhere the compiler can
see.
