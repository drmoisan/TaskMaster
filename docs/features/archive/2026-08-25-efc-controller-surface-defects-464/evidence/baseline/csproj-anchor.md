# Phase 0 — `QuickFiler.Test.csproj` insertion anchor

Timestamp: 2026-08-27T23-41
Task: [P0-T18]
Command: `sed -n '104,124p' QuickFiler.Test/QuickFiler.Test.csproj`, `sed -n '63p' QuickFiler.Test/QuickFiler.Test.csproj`, `grep -n 'Include="Controllers.Efc' QuickFiler.Test/QuickFiler.Test.csproj`, and `file` plus a BOM byte check, at `BASELINE_SHA` = `002335989830ba9f3ad802858ef0b794f6281750`
EXIT_CODE: 0

## Observed lines 104 through 124

```
104:    <Compile Include="Controllers\KaKeyTests.cs" />
105:    <Compile Include="Controllers\KaStringAsyncTests.cs" />
106:    <Compile Include="Controllers\FilerQueueTests.cs" />
107:    <Compile Include="Controllers\QfcQueueCoverageExpansionTests.cs" />
108:    <Compile Include="Controllers\QfcQueuePurePathsTests.cs" />
109:    <Compile Include="Controllers\EfcDataModelIssue614Tests.cs" />
110:    <Compile Include="Controllers\EfcDataModelTests.cs" />
111:    <Compile Include="Controllers\EfcFormControllerTests.cs" />
112:    <Compile Include="Controllers\EfcHomeControllerDependenciesTests.cs" />
113:    <Compile Include="Controllers\EfcHomeControllerDependenciesProductionFactoryTests.cs" />
114:    <Compile Include="Controllers\EfcHomeControllerExecuteMovesTests.cs" />
115:    <Compile Include="Controllers\EfcHomeControllerLifecycleTests.cs" />
116:    <Compile Include="Controllers\EfcHomeControllerMetricsTests.cs" />
117:    <Compile Include="Controllers\EfcHomeControllerTests.cs" />
118:    <Compile Include="Controllers\EmailSorterTests.cs" />
119:    <Compile Include="Controllers\BayesianPerformanceControllerTests.cs" />
120:    <Compile Include="Controllers\BayesianPerformanceController.TestSupport.cs" />
121:    <Compile Include="Controllers\EfcHomeControllerSeamTests.cs" />
122:    <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
123:    <Compile Include="Controllers\QfcCollectionControllerNavigationDigitsTests.cs" />
124:    <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
```

## Discrepancy, recorded and reported before Phase 1 begins

`[P0-T18]` states the expected anchor as "line 112 is the `Compile Include` entry for
`Controllers\EfcHomeControllerTests.cs` and line 113 is the entry for `Controllers\EmailSorterTests.cs`".

**Both are wrong on this base, and both deviate exactly as `plan-base-drift-addendum.2026-08-27T21-01.md`
predicted.**

| Anchor | Plan's expected line | Observed line |
|---|---|---|
| `Controllers\EfcHomeControllerTests.cs` | 112 | **117** |
| `Controllers\EmailSorterTests.cs` | 113 | **118** |

Line 112 is in fact `Controllers\EfcHomeControllerDependenciesTests.cs`.

The addendum is authoritative for where things are on disk, so the insertion point for `[P1-T2]` is
**immediately after line 117** and before line 118, not after line 112.

### Cause of the +5 offset

Five `Compile Include` entries now sit above `EfcHomeControllerTests.cs` that were not there when the
plan was authored. Four are the `EfcHomeController*` entries at `:112-115` reordered ahead of it, and one
is `Controllers\EfcDataModelIssue614Tests.cs` at `:109`, added by merged feature **#614**. That entry is
**not** named in `[P1-T2]`'s contiguous-region enumeration, which lists the region as beginning with
`Controllers\EfcDataModelTests.cs`.

**Consequence for `[P1-T2]`.** Its acceptance enumerates "delivered lines 105 through 116" as a specific
ordered list. That numbering and that list are both stale:

- the contiguous `Efc*` region on this base is `:109-117`, nine entries, not the eight the plan lists;
- after the three insertions it becomes `:109-120`, twelve entries, followed by
  `Controllers\EmailSorterTests.cs` at `:121`.

The **substantive** requirements of `[P1-T2]` are unaffected and are what will be verified: exactly three
added lines and zero deleted lines over the whole file; the three new entries contiguous, in the stated
alphabetical order, immediately after `Controllers\EfcHomeControllerTests.cs` and before
`Controllers\EmailSorterTests.cs`; no existing entry moved or reordered; CRLF and BOM preserved.

## Entry that must not be disturbed

`Controllers\EfcSelectionGuardTests.cs` sits at **`:63`**, far above the region, and is a separate merged
feature's entry. It is neither enumerated nor moved. `Controllers\EfcHomeControllerSeamTests.cs` sits at
`:121`, below `EmailSorterTests.cs` and outside the contiguous region; it is likewise neither enumerated
nor moved.

Complete list of `Controllers\Efc*` entries at baseline, for the record: `:63`, `:109`, `:110`, `:111`,
`:112`, `:113`, `:114`, `:115`, `:116`, `:117`, `:121` — eleven entries.

## Encoding facts that `[P1-T2]` must preserve

`file` reports: `XML 1.0 document, Unicode text, UTF-8 (with BOM) text, with very long lines (335), with
CRLF line terminators`. The first three bytes are `EF BB BF`. The insertion must preserve both the BOM
and CRLF line endings throughout, and must not normalise line endings anywhere in the file.

Output Summary: The insertion anchor deviates from the plan by +5 lines, exactly as the base-drift
addendum predicted. `Controllers\EfcHomeControllerTests.cs` is at :117 (not :112) and
`Controllers\EmailSorterTests.cs` at :118 (not :113); the extra entries are the reordered
EfcHomeController* group plus `Controllers\EfcDataModelIssue614Tests.cs` at :109 from merged #614.
The contiguous Efc* region is :109-117. `Controllers\EfcSelectionGuardTests.cs` at :63 must not be
disturbed. The file is UTF-8 with BOM and CRLF line endings.
