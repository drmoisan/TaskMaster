# Item-1 sweep: three comment-only initializers deleted with their orphaned comment (issue #826, [P5-T9])

Timestamp: 2026-09-09T19-38

Command: each method body was read in full before anything was deleted, confirming that the install plus
one commented line formed the whole body. A `pwsh -NoProfile -Command` block carrying the plan's C2
preamble branch guard then removed the attribute, the signature, the braces, the install statement, the
orphaned comment and the trailing blank line as one unit, asserting a per-file match count of 1 before
writing. The touched paths were then formatted with `csharpier format` and gated.

EXIT_CODE: 0 (csharpier `format` reported `Formatted 3 files in 2466ms.` and exited 0)

## Bodies read before deletion

| File | Body observed before the edit |
|---|---|
| `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs` | install, then `//this.mockRepository = new MockRepository(MockBehavior.Loose);` |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs` | install, then `//this.mockRepository = new MockRepository(MockBehavior.Strict);` |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` | install, then `//this.mockRepository = new MockRepository(MockBehavior.Loose) { CallBase = true };` |

Only the comment inside the deleted method body was removed in each file. Comments outside the method
were left alone; in particular `BayesianClassifierGroupTests.cs` retains its
`//private MockRepository mockRepository;` declaration comment, which sits above the method and does not
match the residue token.

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `TestInitialize` across the three paths | 0 | 0 |
| `Console.SetOut(` across the three paths | 0 | 0 |
| `DebugTextWriter` across the three paths | 0 | 0 |
| `mockRepository = new MockRepository` across the three paths | 0 | 0 |

The `mockRepository = new MockRepository` gate is capable of failing rather than vacuous: before this
task each of the three files carried exactly one match, and that single match was the commented residue
this task deletes. That was verified per file immediately before the edit, with a count of 1 observed in
each. The surviving `//private MockRepository mockRepository;` declaration comment does not match the
token, so leaving it in place does not make the gate unsatisfiable.

## Anchored numstat

```
0	7	UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs
0	7	UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs
0	7	UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs
```

Seven removed lines and zero added in every file: the six-line shape of [P5-T7] plus the orphaned
comment.

## Encoding preservation

All three files carry a UTF-8 byte-order mark and were written back with a `UTF8Encoding` constructed
from that observed flag, so none lost it.

Output Summary: all three initializer methods are deleted together with their attributes, install
statements and orphaned comments. `TestInitialize`, `Console.SetOut(`, `DebugTextWriter` and the
commented `mockRepository = new MockRepository` residue are all 0 across the three paths. These three are
the last three of the ten files AC4 names.
