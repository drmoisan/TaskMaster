# Phase 3 completion notes (Issue #824)

Timestamp: 2026-09-09T15-42

Two Phase 3 tasks require a completion note but name no artifact of their own. Both notes are
recorded here so they are durable rather than confined to a transcript.

## P3-T1 — the AC4 test is a supporting test, not a gate

`OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` **passes on the unfixed tree as well as on the
fixed tree**, because a single-threaded test observes a fully populated table either way: by the
time the assertions run, either the class's own earlier activity or a prior test class has completed
a `LoadOpCodes()` call. It is therefore **not a gate for this defect and must not be reported or
relied upon as evidence that the race is fixed.**

Its value is different and real. It replaces the two weak spot checks deleted by P3-T3 with an
exhaustive assertion over every opcode `System.Reflection.Emit.OpCodes` declares, so it would catch
a fix that publishes the tables safely but fills them wrongly. The statement is also carried in the
test's own XML documentation comment in the source file, so a future reader encounters it at the
test rather than only in this artifact.

The deterministic gates for this defect are AC2
(`LoadOpCodes_DoesNotRepublishPublishedTables`) and AC3 (`SingleByteOpCodes_FieldIsInitOnly`,
`MultiByteOpCodes_FieldIsInitOnly`), whose fail-before / pass-after evidence is recorded under
`evidence/regression-testing/`.

## P3-T3 — the single surviving `ILGlobals.LoadOpCodes()` call site

After the two spot-check tests were deleted, a `Grep` with `-n` for `ILGlobals\.LoadOpCodes\(\)`
over `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` returns exactly one match:

```
50:            ILGlobals.LoadOpCodes();
```

| Locator | Line |
|---|---|
| Enclosing method declaration, `public void LoadOpCodes_DoesNotRepublishPublishedTables()` | 43 |
| Opening brace of that method body | 44 |
| The single `ILGlobals.LoadOpCodes()` invocation | 50 |
| Closing brace of that method body | 69 |

Line 50 lies strictly inside the body bounded by lines 44 and 69, so the surviving invocation is the
Act of `LoadOpCodes_DoesNotRepublishPublishedTables` and of no other test. This is the mechanical
form of AC7's requirement that the one remaining `LoadOpCodes()` invocation be the Act of the AC2
gate.

A `Grep` for `LoadOpCodes_PopulatesKnownSingleByteOpCodes|LoadOpCodes_PopulatesKnownOpCode_Ret` over
that file returns zero matches, confirming both spot checks were deleted rather than renamed.

These line numbers are superseded by the P5-T1 format pass; P4-T4 and P5-T13 re-derive them.
