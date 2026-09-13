# Phase 0 — Pinned Source Facts

Timestamp: 2026-09-13T05-14
Task: [P0-T13]

Command: pwsh -Command '"TestMethodCount: " + @(Select-String -Path "UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs" -Pattern "TestMethod" -SimpleMatch -CaseSensitive).Count; "DataRowCount: " + @(Select-String -Path "UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs" -Pattern "DataRow" -SimpleMatch -CaseSensitive).Count; "CoverageAttributeCount: " + @(Select-String -Path "UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs" -Pattern "[ExcludeFromCodeCoverage]" -SimpleMatch -CaseSensitive).Count; "Part4Lines: " + (Get-Content -Path "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs").Count; "ProgressPackageTestsLines: " + (Get-Content -Path "UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs").Count; "ProgressPackageLines: " + (Get-Content -Path "UtilitiesCS/Threading/ProgressPackage.cs").Count; "SubjectMapOrchestrationLines: " + (Get-Content -Path "UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs").Count'
EXIT_CODE: 0

TestMethodCount: 9
DataRowCount: 0
CoverageAttributeCount: 4
Part4Lines: 347
ProgressPackageTestsLines: 120
ProgressPackageLines: 150
SubjectMapOrchestrationLines: 274

Output Summary: all seven values were measured and every one matches its expected value exactly. There is
no divergence to report on any of the seven.

## The Three Hard Expectations, All Met

- 9 test methods in `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`. Expected 9, observed 9.
- 0 data rows in the same file. Expected 0, observed 0. With no data rows, that class's executed-test count
  equals its method count, so the AC11 removal term for the UtilitiesCS test assembly is minus nine.
- 4 coverage-exclusion attributes in
  `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`. Expected 4, observed 4. This is
  the value P2-T12 compares against.

## The Four Line Counts, All Within Tolerance

The four line counts are recorded as measured and are the comparison base for P2-T17. Each observed value
equals its expected value exactly, so the one-line trailing-newline tolerance the task allows was not
needed:

| File | Expected | Observed |
| --- | --- | --- |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs | 347 | 347 |
| UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs | 120 | 120 |
| UtilitiesCS/Threading/ProgressPackage.cs | 150 | 150 |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs | 274 | 274 |

The 347-line figure on the Part4 test file matters for the file-size budget: the repository limit is 500
lines, so the two tests AC1 and AC2 add must fit within the remaining 153 lines. Splitting into a further
part file is out of scope because it would require editing the QuickFiler test project file, which this
delivery does not claim.

## The Two Compile Lines To Be Removed, Quoted Verbatim

Each is a single-line self-closing Compile item with four leading spaces. Quoted exactly as they appear:

`UtilitiesCS/UtilitiesCS.csproj` line 971:

```
    <Compile Include="Threading\ProgressTrackerAsync.cs" />
```

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 496:

```
    <Compile Include="Threading\ProgressTrackerAsync_Tests.cs" />
```

Both line numbers were verified by reading the files at those exact indices. Both match the expectation.

## The Neighbours That Must Not Be Touched

All four neighbours are Compile items and all four were read at their stated line numbers:

| Location | Content |
| --- | --- |
| UtilitiesCS/UtilitiesCS.csproj line 970 | `    <Compile Include="NewtonsoftHelpers\AppGlobalsConverter.cs" />` |
| UtilitiesCS/UtilitiesCS.csproj line 972 | `    <Compile Include="Threading\ProgressTrackerPane.cs" />` |
| UtilitiesCS.Test/UtilitiesCS.Test.csproj line 495 | `    <Compile Include="Threading\ProgressTrackerPane_Tests.cs" />` |
| UtilitiesCS.Test/UtilitiesCS.Test.csproj line 497 | `    <Compile Include="Threading\TaskPriority_Tests.cs" />` |

The two immediate neighbours in each file name the progress tracker pane and its test, and the task
priority test. None is the dormant tracker or its test, so an edit that removed a neighbour instead of the
target would be detectable by name as well as by the AC7 count comparison.

## Baseline Timing

Every file measured here is unmodified. Phase 1 has not started.
