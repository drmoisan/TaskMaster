# Item-1 sweep: both initializers in `ObsoleteBayesianClassifier_Tests.cs` (issue #826, [P5-T8])

Timestamp: 2026-09-09T19-36

This is the only file in the 33-file population carrying two live install statements, because it contains
two `[TestClass]` types each with its own initializer.

Command: both method bodies were read in full and confirmed to contain the install as their only
statement before anything was deleted. A `pwsh -NoProfile -Command` block carrying the plan's C2 preamble
branch guard then removed both methods with their attributes, braces, install statements and trailing
blank lines, asserting a match count of exactly **2** before writing. The path was then formatted with
`csharpier format` and gated.

EXIT_CODE: 0 (csharpier `format` reported `Formatted 1 files in 1115ms.` and exited 0)

## Bodies read before deletion

Both `[TestInitialize] public void TestInitialize()` methods contained the install as their only
statement, one in each `[TestClass]` type. Before the edit they sat at lines 58 to 62 and 473 to 477.

## Gate figures

| Measure | Observed | Required | Census |
|---|---|---|---|
| `TestInitialize` | 0 | 0 | 4 |
| `Console.SetOut(` | 0 | 0 | 2 |
| `DebugTextWriter` | 0 | 0 | 2 |
| `[TestClass]` | 2 | unchanged at 2 | 2 |

The `[TestClass]` count being unchanged at 2 is what proves only the two initializer methods went and
neither type declaration was damaged.

## Anchored numstat

```
0	12	UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs
```

Twelve removed lines and zero added: six per initializer, matching the per-method figure observed in
[P5-T7] for the same shape.

## Encoding preservation

The file carries no UTF-8 byte-order mark and was written back with a `UTF8Encoding` constructed from
that observed flag, so it did not gain one.

Output Summary: both initializer methods are deleted with their attributes and install statements, the
`TestInitialize`, `Console.SetOut(` and `DebugTextWriter` counts are all 0, and the `[TestClass]` count
is unchanged at 2. This file is one of the ten AC4 names.
