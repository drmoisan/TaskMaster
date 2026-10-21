# AC6 verification — nullable annotations and comment (Issue #824, task P4-T3)

Timestamp: 2026-09-09T15-49

Command: five `Grep` invocations over `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`, the
last with `-n` and `output_mode: content`.

EXIT_CODE: 0

## Output Summary

| Pattern | Match count | Expected | Holds |
|---|---|---|---|
| `null!` | 0 | zero | yes |
| `populated by LoadOpCodes` | 0 | zero | yes |
| `never reassigned` | 2 | at least one | yes |
| `element mutation` | 2 | at least one | yes |
| `#nullable enable` | 1, at line 1 | a match at line 1 | yes |

All five expectations hold.

Notes on two of them:

- The `null!` count of 0 is a real observation rather than a pattern that could not match. The file
  still contains the `(OpCode)info1.GetValue(null)!` form inside the static constructor, which is a
  null-forgiving operator applied to a method result and does not contain the token `null!`. That
  form is deliberately retained: it guards the unbox that the `FieldType == typeof(OpCode)` check
  makes safe, and it is unrelated to the field suppressions AC6 requires deleted.
- `never reassigned` and `element mutation` each match twice, once in the XML documentation of each
  of the two opcode-table fields. AC6 requires at least one match, and per-field documentation is
  why there are two.

The replacement documentation states, for each field, that the table is published once by the static
constructor, is never reassigned, and that `readonly` prevents reassignment of the array reference
but does not prevent element mutation, so callers must treat the contents as read-only. The
three-line comment that previously asserted the tables were "populated by LoadOpCodes() before any
read" and "annotated null!" is gone, which the second row records.

## The CS86xx half of AC6

AC6 additionally requires that the nullable msbuild command complete with zero CS86xx diagnostics,
in particular no CS8618 on either field. That half is not discharged here; it is discharged by P5-T7
against the `/p:TreatWarningsAsErrors=true` gate. P2-T4 has already recorded a preliminary
observation of zero `: warning CS86[0-9][0-9]:` lines on a plain build, which is corroborating but
not the gate.
