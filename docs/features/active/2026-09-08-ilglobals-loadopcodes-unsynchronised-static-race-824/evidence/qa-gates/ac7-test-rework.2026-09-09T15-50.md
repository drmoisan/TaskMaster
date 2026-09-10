# AC7 verification — the four LoadOpCodes_* tests reworked (Issue #824, task P4-T4)

Timestamp: 2026-09-09T15-50

Command: `Grep` with `-n` for `ILGlobals\.LoadOpCodes\(\)` and a `Grep` for the four old test names,
both over `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs`, plus a `Grep` with
`-n` for the `Length` assertions.

EXIT_CODE: 0

## Output Summary

### The single surviving invocation

```
50:            ILGlobals.LoadOpCodes();
```

Exactly one match. It lies inside the body of `LoadOpCodes_DoesNotRepublishPublishedTables`, whose
declaration is at line 43 and whose body is bounded by lines 44 and 69, so line 50 is the Act of
that test and of no other. That is the AC2 gate, which is what AC7 requires the one remaining
invocation to be.

### The four old test names

A `Grep` for
`LoadOpCodes_Initializes_SingleByteOpCodes|LoadOpCodes_Initializes_MultiByteOpCodes|LoadOpCodes_PopulatesKnownSingleByteOpCodes|LoadOpCodes_PopulatesKnownOpCode_Ret`
returns **zero** matches. Two were renamed and two were deleted.

### The two renamed publication tests

| Test | Declaration line | Present exactly once |
|---|---|---|
| `SingleByteOpCodes_IsPublishedWithFullLength` | 17 | yes |
| `MultiByteOpCodes_IsPublishedWithFullLength` | 29 | yes |

Their assertions, reproduced verbatim with line numbers:

```
20:            ILGlobals.singleByteOpCodes.Should().NotBeNull();
21:            ILGlobals.singleByteOpCodes.Length.Should().Be(0x100);
32:            ILGlobals.multiByteOpCodes.Should().NotBeNull();
33:            ILGlobals.multiByteOpCodes.Length.Should().Be(0x100);
```

The `Length` assertion required by this task is present for each: `.Length.Should().Be(0x100)` at
line 21 for the single-byte table and at line 33 for the multi-byte table. The not-null assertions
previously at :18 and :29 of the baseline file are preserved at lines 20 and 32.

Neither renamed test calls `ILGlobals.LoadOpCodes()`. Their Act sections were removed, so the first
read of the static field in the Assert is what triggers publication, which is the property the
renamed tests now describe. This is what removes the mis-attribution AC7 names: before the rework a
reader would reasonably conclude that `LoadOpCodes()` is what populates the tables, which is the
mental model that produced the defect.

### The two deleted spot checks

`LoadOpCodes_PopulatesKnownSingleByteOpCodes` asserted `singleByteOpCodes[0x00] == OpCodes.Nop` and
`LoadOpCodes_PopulatesKnownOpCode_Ret` asserted `singleByteOpCodes[0x2A] == OpCodes.Ret`. Both are
deleted. Their coverage is subsumed by `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes`, which
asserts the correct entry for every opcode `System.Reflection.Emit.OpCodes` declares, including
those two.
