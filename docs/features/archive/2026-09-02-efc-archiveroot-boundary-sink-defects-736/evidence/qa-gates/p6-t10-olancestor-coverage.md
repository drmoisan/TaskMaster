# P6-T10 — The config-construction line survived the finding-6 rewrite covered

Timestamp: 2026-09-04T02-15

Command: a `Select-String` over `QuickFiler/Controllers/EfcDataModel.cs` for the quoted `OlAncestor`
source literal, followed by an XML query over the P6-T6 Cobertura document reading each matched
line's `hits` with the same maximum-hits grouping over the pair (filename, number) that P6-T8 uses.

EXIT_CODE: 0

## Derivation, stated mechanically

1. Locate **every** line of `QuickFiler/Controllers/EfcDataModel.cs` matching the quoted source
   literal `OlAncestor = olAncestor,`.
2. Sort them ascending by line number.
3. Take the **first**, which is the occurrence inside the five-parameter `MoveToFolderAsync`
   overload and is the one finding 6's remedy is required to keep covered.
4. Read that line's `hits` from the P6-T6 Cobertura document against the backslash-spelled key
   `QuickFiler\Controllers\EfcDataModel.cs`, which is the key P6-T8's separator paragraph fixes for
   this file. A match against the forward-slash spelling used in this plan's prose returns zero rows.

The line numbers are re-derived after the change rather than assumed, because the formatting pass may
have shifted them. The Cobertura document read is the refreshed one produced by the P6-T6 execution
that followed P6-T13.

## The three matching lines

**Exactly three** lines of that file match the literal.

| Order | Post-change line | Enclosing member | Pre-change line (P0-T8) |
|---|---|---|---|
| first | **339** | the five-parameter `MoveToFolderAsync` overload, declared at line 303 | 339 |
| second | 380 | `OpenOlFolderAsync`, declared at line 363 | 366 |
| third | 404 | `OpenFsFolderAsync`, declared at line 388 | 390 |

The file order is unchanged from the pre-change order P0-T8 records — the five-parameter
`MoveToFolderAsync` overload, then `OpenOlFolderAsync`, then `OpenFsFolderAsync`. The first
occurrence did not move; the second and third shifted down by 14 lines, which is the size of the
`InvokeFilerAsync` seam D6 inserted between the first and the second. An earlier draft of this task
asserted a single matching line, which is not satisfiable in this file: the `EmailFilerConfig` object
initializer is written three times, once per public entry point, and D6 changes only the statements
that follow the initializer inside the `MoveToFolderAsync` overload.

## The measured `hits` on the first occurrence

| Line | `hits` | Greater than 0? |
|---|---|---|
| **339** | **1** | **yes** |

The line is covered. For the reader, the two siblings the remedy does not touch carry `hits="0"`:
line 380 in `OpenOlFolderAsync` and line 404 in `OpenFsFolderAsync`, neither of which is a changed
line and neither of which this item's tests exercise. Those two are outside this gate.

## The test method that reaches it

**`MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`**, declared at
`QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs:175`.

It is the single test that reaches line 339, and the reasoning is structural rather than inferred
from the coverage figure alone. Line 339 sits inside the `EmailFilerConfig` object initializer at
lines 332 through 341, which is reached only after the guard `if (!TryGetArchiveRoot(out var
olAncestor))` at line 327 falls through — that is, only when the archive root resolves. Of the eight
`MoveToFolderAsync` tests in that class, the two unresolvable cases (lines 48 and 71) and the two
early-return cases (lines 196 and 226) all return before line 332; the COM case (line 251) propagates
out of the archive-root read; and only the resolving case at line 175 continues into the initializer.
P5-T1 rewrote that test to call the shared `MoveAsync` helper directly and P5-T4 supplies the
`TestableEfcDataModel` override of `InvokeFilerAsync` that stops it at the filer seam, which is why
the initializer at 332-341 stays covered while the seam body at 359-361 does not.

Output Summary: exactly three lines of `QuickFiler/Controllers/EfcDataModel.cs` match the quoted
`OlAncestor` literal, at post-change lines 339, 380 and 404, enclosed in file order by the
five-parameter `MoveToFolderAsync` overload, `OpenOlFolderAsync`, and `OpenFsFolderAsync`. The first
occurrence, line 339, carries `hits="1"` in the refreshed P6-T6 Cobertura document under the key
`QuickFiler\Controllers\EfcDataModel.cs`, which is greater than 0, so the finding-6 rewrite preserved
coverage of the config-construction line. The single test reaching it is
`MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`.
