# Item-1 sweep: six `UtilitiesCS.Test` EmailIntelligence files (issue #826, [P5-T2])

Timestamp: 2026-09-09T19-27

Command: each initializer body was read in full before anything was deleted, then a
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard removed exactly the single
install statement from each file, asserting a per-file match count of 1 before writing. The touched paths
were then formatted with `csharpier format` and gated with `-SimpleMatch` counts and an anchored
`git diff --numstat`.

EXIT_CODE: 0 (csharpier `format` reported `Formatted 6 files in 3931ms.` and exited 0)

## Risk 5 discharge — initializer bodies read before deleting

All six initializers do other work, so only the single install line was removed in each:

| File | Statements the initializer retains |
|---|---|
| `EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs` | `_mockGlobals` construction, `Triage` construction with an object initializer, `Triage_OlLogic` construction |
| `EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs` | `this.mockRepository` construction and three mock-folder constructions |
| `EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs` | `this.mockRepository`, `SetupMockGlobals()`, `SetupMockMail()` and a commented line that is **not** the method's only body and is therefore left in place |
| `EmailIntelligence/Bayesian/BayesianSerializationHelper_Tests.cs` | `_mockRepository`, two `Create<...>()` assignments, `SetupAllProperties()` and an `_appDataRoot` assignment |
| `EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs` | `_mockRepository`, two `Create<...>()` assignments, `SetupAllProperties()` and a `SetupGet` chain |
| `EmailIntelligence/Bayesian/BayesianClassifierTests.cs` | `this.mockRepository` with `CallBase = true`, four token-fixture constructions and a `CorpusSub` construction |

None of these six is in the AC4 delete-the-method group.

## Sibling file deliberately not touched

`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs` was not opened
or edited. Its `Console.SetOut` occurrence is commented out, installs nothing, and is one of the two
files AC1 expects to still match after the change. Its post-change `Console.SetOut(` count is **1**, as
required, and it does not appear in the anchored numstat below.

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `Console.SetOut(` across the six edited paths | 0 | 0 |
| `DebugTextWriter` across the six edited paths | 0 | 0 |
| `Console.SetOut(` in `BayesianClassifierTests_UnfinishedStubs.cs` | 1 | 1 |

## Anchored numstat

```
0	1	UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests.cs
0	2	UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs
0	2	UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianSerializationHelper_Tests.cs
0	1	UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs
0	1	UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs
0	1	UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs
```

`BayesianClassifierTests_UnfinishedStubs.cs` is absent from this list, confirming it was not modified.

### Recorded note — two files show 2 removed lines rather than 1

`BayesianPerformanceMeasurement_Tests.cs` and `BayesianSerializationHelper_Tests.cs` each report 0 added
and 2 removed. The second removed line in each is the blank line that followed the install statement and
became a leading blank line inside the method body once the statement went; CSharpier collapsed it during
the mandatory format pass. No statement was lost. The diff for both files is:

```
         [TestInitialize]
         public void TestInitialize()
         {
-            Console.SetOut(new DebugTextWriter());
-
             _mockRepository = new MockRepository(MockBehavior.Loose);
             _mockGlobals = _mockRepository.Create<IApplicationGlobals>();
             _mockGlobals.SetupAllProperties();
```

Recorded rather than absorbed, because a 2-removed figure where 1 was expected would otherwise read as an
over-deletion.

## Encoding preservation

Three of the six files carry a UTF-8 byte-order mark and three do not. Each file was written back with a
`UTF8Encoding` constructed from its own observed mark flag, so no file gained or lost one. The
zero-added-line numstat confirms it.

Output Summary: all six install statements removed with their initializer methods and every other
statement intact, `Console.SetOut(` and `DebugTextWriter` both 0 across the six paths, and the
commented-out sibling stub file untouched at a count of 1.
