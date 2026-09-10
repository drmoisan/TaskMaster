# Item-1 sweep: six initializer methods deleted outright (issue #826, [P5-T7])

Timestamp: 2026-09-09T19-35

Command: each method body was read in full and confirmed to contain the install as its **only**
statement before anything was deleted. A `pwsh -NoProfile -Command` block carrying the plan's C2 preamble
branch guard then removed the attribute, the signature, the braces, the install statement and the
trailing blank line as one unit, asserting a per-file match count of 1 before writing. The touched paths
were then formatted with `csharpier format` and gated with `-SimpleMatch` counts and an anchored
`git diff --numstat`.

EXIT_CODE: 0 (csharpier `format` reported `Formatted 6 files in 3685ms.` and exited 0)

## Bodies read before deletion — the install was the only statement in each

| File | Initializer | Body observed before the edit |
|---|---|---|
| `VBFunctions.Test/ComputerInfo_Test.cs` | `[TestInitialize] public void Initialize()` | install only |
| `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs` | `[TestInitialize] public void TestInitialize()` | install only |
| `UtilitiesCS.Test/Extensions/Frexp_Test.cs` | `[TestInitialize] public void TestInitialize()` | install only |
| `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs` | `[TestInitialize] public void TestInitialize()` | install only |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs` | `[TestInitialize] public void TestInitialize()` | install only |
| `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs` | `[TestInitialize] public void TestInitialize()` | install only |

Note that `VBFunctions.Test/ComputerInfo_Test.cs` names its method `Initialize()` rather than
`TestInitialize()`. The deletion matched on the `[TestInitialize]` attribute plus a
`public void <identifier>()` signature rather than on a fixed method name, so the naming difference did
not cause a miss. That file's census `TestInitialize` count was 1, the attribute alone, and it is now 0.

None of these six is one of the three comment-only cases; those are handled by [P5-T9].

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `TestInitialize` across the six paths | 0 | 0 |
| `Console.SetOut(` across the six paths | 0 | 0 |
| `DebugTextWriter` across the six paths | 0 | 0 |

## Anchored numstat

```
0	6	TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs
0	6	UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs
0	6	UtilitiesCS.Test/Extensions/Frexp_Test.cs
0	6	UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs
0	6	UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs
0	6	VBFunctions.Test/ComputerInfo_Test.cs
```

Six removed lines and zero added lines in every file: the `[TestInitialize]` attribute, the signature,
the opening brace, the install statement, the closing brace and the trailing blank line. The uniformity
across all six is itself a check that the deletion matched the intended shape in each.

## Encoding preservation

Five of the six files carry a UTF-8 byte-order mark and one does not. Each was written back with a
`UTF8Encoding` constructed from its own observed flag, so none gained or lost one.

Output Summary: all six initializer methods are deleted together with their attributes and install
statements, and the `TestInitialize`, `Console.SetOut(` and `DebugTextWriter` counts are all 0 across the
six paths. These six are six of the ten files AC4 names.
