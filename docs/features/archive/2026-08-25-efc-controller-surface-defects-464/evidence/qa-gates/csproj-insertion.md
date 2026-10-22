# Phase 1 — `QuickFiler.Test.csproj` insertion

Timestamp: 2026-08-27T23-46
Task: [P1-T2]
Command: `git diff --numstat 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler.Test/QuickFiler.Test.csproj`; `git diff 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler.Test/QuickFiler.Test.csproj`; `sed -n '108,122p' QuickFiler.Test/QuickFiler.Test.csproj`; a raw-mode CRLF and BOM census
EXIT_CODE: 0

## Diff size

```
3       0       QuickFiler.Test/QuickFiler.Test.csproj
```

Exactly **3 added lines and 0 deleted lines**, as `[P1-T2]` requires. The complete set of changed lines
in the diff is:

```
+    <Compile Include="Controllers\EfcItemController.CleanupTests.cs" />
+    <Compile Include="Controllers\EfcItemControllerTests.cs" />
+    <Compile Include="Controllers\EfcViewerTests.cs" />
```

No existing line is added, removed, moved, or reordered.

## Delivered contiguous region

The three entries are contiguous and sit immediately after
`Controllers\EfcHomeControllerTests.cs` and immediately before `Controllers\EmailSorterTests.cs`, in the
alphabetical order the task states. Delivered lines 109 through 121:

```
109:    <Compile Include="Controllers\EfcDataModelIssue614Tests.cs" />
110:    <Compile Include="Controllers\EfcDataModelTests.cs" />
111:    <Compile Include="Controllers\EfcFormControllerTests.cs" />
112:    <Compile Include="Controllers\EfcHomeControllerDependenciesTests.cs" />
113:    <Compile Include="Controllers\EfcHomeControllerDependenciesProductionFactoryTests.cs" />
114:    <Compile Include="Controllers\EfcHomeControllerExecuteMovesTests.cs" />
115:    <Compile Include="Controllers\EfcHomeControllerLifecycleTests.cs" />
116:    <Compile Include="Controllers\EfcHomeControllerMetricsTests.cs" />
117:    <Compile Include="Controllers\EfcHomeControllerTests.cs" />
118:    <Compile Include="Controllers\EfcItemController.CleanupTests.cs" />
119:    <Compile Include="Controllers\EfcItemControllerTests.cs" />
120:    <Compile Include="Controllers\EfcViewerTests.cs" />
121:    <Compile Include="Controllers\EmailSorterTests.cs" />
```

### Recorded deviation from the task's stated line numbers

`[P1-T2]` says to insert "immediately after line 112" and describes the delivered region as "lines 105
through 116". Both figures are stale, for the reason `[P0-T18]` records in detail and
`plan-base-drift-addendum.2026-08-27T21-01.md` predicted: on this base
`Controllers\EfcHomeControllerTests.cs` is at `:117`, not `:112`.

The region is also one entry longer than the task's enumeration, because
`Controllers\EfcDataModelIssue614Tests.cs` at `:109` was added by merged feature **#614** and is not
named in the plan's list. It is inside the contiguous `Efc*` region, so it is enumerated above for
completeness; it is not moved and not modified.

Every **substantive** requirement of the task is met: three entries, contiguous, in the stated
alphabetical order, between the two named neighbours, with nothing else changed.

## Entries outside the region, neither enumerated nor moved

- `Controllers\EfcSelectionGuardTests.cs` remains at `:63`, unchanged.
- `Controllers\EfcHomeControllerSeamTests.cs` remains below `EmailSorterTests.cs`, outside the region.

## Ownership

All three inserted entries carry the `Controllers\Efc` prefix, which is the only prefix this feature
owns. No entry was added, reordered, or removed in any sibling-owned region: `Viewers\Breadcrumb*`
(#501), `Viewers\WebView2*` (#476), `Controllers\QfcItemController*` and `Viewers\ToolStrip*` (#489), the
eight `Controllers\QfcCollectionController*` entries from merged #444, and the two
`Controllers\QfcItemController.UiThreadDispatcherFixture*.cs` entries from merged #493 are all untouched —
which the three-line diff above proves exhaustively.

`git diff --name-only <BASELINE_SHA> -- QuickFiler/QuickFiler.csproj` produces **no output lines**:
`QuickFiler/QuickFiler.csproj` is untouched.

## Line endings and BOM

Raw-byte census of the delivered file: **496 CRLF sequences, 496 LF bytes**, so every line ending in the
file is CRLF and none is a bare LF. The UTF-8 BOM `EF BB BF` is still the first three bytes. The baseline
census was 493 CRLF / 493 LF with a BOM; the delta of exactly 3 is the three inserted lines. No line
ending was normalised anywhere in the file.

Output Summary: Exactly 3 added and 0 deleted lines. The three `Efc*` entries are contiguous,
alphabetical, and between `EfcHomeControllerTests.cs` and `EmailSorterTests.cs`. CRLF (496/496) and the
UTF-8 BOM are preserved, `EfcSelectionGuardTests.cs` at :63 is undisturbed, no sibling-owned region is
touched, and `QuickFiler/QuickFiler.csproj` is unmodified. The task's `:112` anchor and its "lines 105
through 116" enumeration are stale; the true anchor is `:117` and the delivered region is `:109-121`.
