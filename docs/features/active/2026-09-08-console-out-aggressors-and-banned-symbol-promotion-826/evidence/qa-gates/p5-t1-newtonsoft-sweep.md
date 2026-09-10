# Item-1 sweep: six `UtilitiesCS.Test/NewtonsoftHelpers` files (issue #826, [P5-T1])

Timestamp: 2026-09-09T19-25

Command: each initializer body was read in full before anything was deleted, then a
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard removed exactly the single
install statement from each file, asserting a match count of 1 per file before writing. The touched paths
were then formatted and gated:

```
& $dotnet tool run csharpier format @files
@(Select-String -LiteralPath $files -CaseSensitive -SimpleMatch "Console.SetOut(").Count
@(Select-String -LiteralPath $files -CaseSensitive -SimpleMatch "DebugTextWriter").Count
@(Select-String -LiteralPath <each file> -CaseSensitive -SimpleMatch "TestInitialize").Count
git diff --numstat $Base -- $files
```

EXIT_CODE: 0

## Risk 5 discharge — initializer bodies read before deleting

Each of the six `[TestInitialize]` methods does other work, so only the single install line was removed
in each. The bodies observed before the edit were:

| File | Statements the initializer retains |
|---|---|
| `WrapperScoDictionaryTest.cs` | `mockRepository = new MockRepository(MockBehavior.Strict);` plus two `mockRepository.Create<...>()` assignments |
| `WrapperScDictionaryTest.cs` | `mockRepository = new MockRepository(MockBehavior.Strict);` plus two `mockRepository.Create<...>()` assignments |
| `ScoDictionaryConverterTests.cs` | `this.mockRepository = new MockRepository(MockBehavior.Strict);`, a `Create<...>()` assignment and an `ApplicationGlobals` construction |
| `ScDictionaryConverter_Tests.cs` | `mockRepository = new MockRepository(MockBehavior.Loose);`, a `Create<...>()` assignment and an `ApplicationGlobals` construction |
| `PeopleScoConverter_Tests.cs` | `mockRepository = new MockRepository(MockBehavior.Loose);`, a `Create<...>()` assignment and an `ApplicationGlobals` construction |
| `FilePathHelperConverterTests.cs` | `this.mockRepository = new MockRepository(MockBehavior.Loose);` plus two `this.mockRepository.Create<...>()` assignments |

None of these six files is in the AC4 delete-the-method group.

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `Console.SetOut(` across the six paths | 0 | 0 |
| `DebugTextWriter` across the six paths | 0 | 0 |

Per-file `TestInitialize` counts, compared against the [P0-T5] census:

| File | Census | Now | Unchanged |
|---|---|---|---|
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs` | 2 | 2 | yes |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs` | 2 | 2 | yes |
| `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs` | 2 | 2 | yes |
| `UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs` | 2 | 2 | yes |
| `UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs` | 2 | 2 | yes |
| `UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs` | 2 | 2 | yes |

## Anchored numstat

```
0	1	UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs
0	1	UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs
0	1	UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs
0	1	UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs
0	1	UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs
0	1	UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs
```

Exactly one removed line and zero added lines per file, which is what proves only the install statement
went and nothing was reflowed. The csharpier pass ran before this measurement, so the figures are
post-format.

## Encoding preservation

Three of the six files carry a UTF-8 byte-order mark and three do not. The edit read each file's raw
bytes, detected the mark, and wrote the file back with a `UTF8Encoding` constructed with that same flag,
so no file gained or lost a mark. The zero-added-line numstat confirms this: an encoding change would
have shown as a modified first line.

Output Summary: all six install statements removed, six initializer methods retained intact with their
mock construction, `Console.SetOut(` and `DebugTextWriter` counts both 0 across the six paths, and
`TestInitialize` counts unchanged from the census at 2 per file.
