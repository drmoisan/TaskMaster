# P5-T7 — FolderConverter tests after D5a-D5g (#614; AC6-AC12 test halves)

Timestamp: 2026-08-26T17-45

Command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~FolderConverterIssue614Tests|FullyQualifiedName~FolderConverterTests|FullyQualifiedName~FolderConverter_Tests" "/Logger:trx;LogFileName=p5-t7.trx" "/ResultsDirectory:coverage\trx\p5-t7"`

(`$vstest` resolved via vswhere to the VS 18 Community `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.)

EXIT_CODE: 0

## Output Summary

- `Test Run Successful.` Total tests: 41; Passed: 41; Failed: 0; Skipped: 0.
- Per-class counts from the TRX: `FolderConverterIssue614Tests` 19 passed;
  `FolderConverterTests` 22 passed.
- AC11 spec correction verified: the updated `:329` assertion
  (`result["Remove illegal characters"]()...Should().Be("BadName")`) passes with the corrected
  per-character removal semantics. Every other pre-existing `FolderConverterTests` test is
  unedited and green, including
  `ToFsFolderpath_WithStringInputs_MapsOutlookBranchIntoFilesystemBranch`,
  `ToFsFolderpath_WhenMappedBranchContainsIllegalCharacters_ThrowsArgumentException`
  (which pins `.WithParameterName("fsPath")`, the constraint P5-T3 preserved), and all three
  `ResolveOlRoot_*` tests.
- New `FolderConverterIssue614Tests` coverage: AC6 dotted/bracketed/hyphenated filesystem roots
  and a dotted derived segment; AC7 invalid character, trailing dot, trailing space, and reserved
  device name, each with its legal counterpart (interior dot, interior space, `COM10`); AC8 a UNC
  ancestor and a sub-three-character ancestor, neither throwing `ArgumentOutOfRangeException` nor
  mangling; AC9 repeated ancestor substring stripped only at the prefix and a case-differing
  ancestor still matching; AC10 the thrown message containing neither `mailbox@example.com` nor
  the filesystem ancestor; AC12 the `Archive2` separator-boundary near-miss in `ResolveOlRoot`.

## Observed pre-existing condition (recorded, not changed)

The third filter alternation `FullyQualifiedName~FolderConverter_Tests` matched zero tests.
`UtilitiesCS.Test/OutlookExtensions/FolderConverter_Tests.cs` exists on disk but carries **no**
`<Compile Include>` item in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, so the class is not
compiled into `UtilitiesCS.Test.dll`. This was confirmed two ways: a search of the project file
returns no match for `FolderConverter_Tests`, and `vstest.console.exe /ListTests` over the built
assembly lists no test whose name contains `FolderConverter_Tests`.

This is a pre-existing orphan (these are non-SDK projects with no glob includes) and is **not**
caused by this change. Its single test asserts exactly the same string mapping as
`FolderConverterTests.ToFsFolderpath_WithStringInputs_MapsOutlookBranchIntoFilesystemBranch`,
which is compiled and green, so the behaviour it documents is covered. Wiring the orphan into the
project is deliberately NOT done here: it would add a compile item outside this change's in-scope
path set (plan P8-T2) and is unrelated to #614. Recorded for follow-up triage.

- Raw TRX (contains the machine account and host name) stays under the gitignored
  `coverage\trx\p5-t7\` tree.
