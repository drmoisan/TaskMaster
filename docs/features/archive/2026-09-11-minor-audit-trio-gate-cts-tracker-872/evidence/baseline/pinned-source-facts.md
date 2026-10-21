# Phase 0 — Pinned Source Facts

Timestamp: 2026-09-13T15-06
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

Output Summary: all seven values were measured against the post-merge tree and all seven match their
expected values exactly. There is no divergence to report before Phase 1 begins.

## The Three Hard Expectations

- 9 test methods and 0 data rows in `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`. With
  zero data rows the executed-test count of that class equals its method count, so the AC11 removal
  term is exactly minus nine.
- 4 coverage attributes in `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`.
  This is the value P2-T12 compares against, so that no coverage attribute is added or removed by the
  AC5 change.

## The Four Line Counts

The four line counts are recorded as measured and are the comparison base for P2-T17. Each matches its
expected value exactly: 347, 120, 150 and 274. A difference of one line on any of the four would have
been attributable to the trailing-newline counting convention of the measuring tool and would not be a
divergence; a difference greater than one would be. No difference was observed.

## The Two Compile Lines To Be Removed

Both were located by their quoted text and their line numbers were then confirmed, rather than being
read from a line number alone. A line number is invalidated by any edit above it; the quoted text is
not.

`UtilitiesCS/UtilitiesCS.csproj` line 971, with four leading spaces:

```
    <Compile Include="Threading\ProgressTrackerAsync.cs" />
```

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 499, with four leading spaces:

```
    <Compile Include="Threading\ProgressTrackerAsync_Tests.cs" />
```

A text search for the first file name inside the production project file returns exactly one hit, at
line 971. A text search for the second file name inside the test project file returns exactly one hit,
at line 499.

## The Neighbours That Must Not Be Touched

Line 970 and line 972 of `UtilitiesCS/UtilitiesCS.csproj`:

```
    <Compile Include="NewtonsoftHelpers\AppGlobalsConverter.cs" />
    <Compile Include="Threading\ProgressTrackerPane.cs" />
```

Line 498 and line 500 of `UtilitiesCS.Test/UtilitiesCS.Test.csproj`:

```
    <Compile Include="Threading\ProgressTrackerPane_Tests.cs" />
    <Compile Include="Threading\TaskPriority_Tests.cs" />
```

## Re-Measurement Note, Per D15

This artifact overwrites a superseded capture. All seven pinned values are unchanged from the
superseded capture and from the plan's stated expectations, and each was re-measured against the
post-merge tree rather than carried forward.

The test-project citation is the one fact the merge moved. It was line 496 with neighbours 495 and 497
before the mandated reconciliation that merged the main branch carrying the fix for issue #877. That
fix inserted a three-line Compile element for the shared assembly resolver source higher in the same
item group, so every Compile item below it shifted down by exactly three lines and the citation moved
from 496 to 499. The observation above confirms the corrected citation: the quoted text resolves to
line 499 and its neighbours at 498 and 500 are the two items the plan names. The production-project
citation is unchanged by that fix and is confirmed to still be line 971.
