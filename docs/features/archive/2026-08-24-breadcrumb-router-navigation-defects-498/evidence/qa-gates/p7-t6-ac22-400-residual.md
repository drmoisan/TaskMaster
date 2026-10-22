# P7-T6 — #400 Residual Selector Contract (AC-22)

Timestamp: 2026-08-26T11-06

Command (as written in `P7-T6`, run first): `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbStateModelSequenceTests" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/trx/p7-t6"; "EXIT_CODE: $LASTEXITCODE"'`

Command (name-resolved, the run of record): identical except `"/TestCaseFilter:FullyQualifiedName~BreadcrumbStateModelTests"`

Second command: `git status --porcelain --untracked-files=all -- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`

EXIT_CODE: 0

## Output Summary

**PASS at the primary acceptance condition (`failed 0`). No degradation was used or available.**

### Name resolution: file name versus class name

The filter named by the task, `FullyQualifiedName~BreadcrumbStateModelSequenceTests`, matched **no
test**: vstest reported `No test matches the given testcase filter`. The cause is a naming discrepancy
that pre-dates this feature and is not caused by it.

`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` exists and carries 12
`[TestMethod]` members, but at line 16 it declares
`public sealed partial class BreadcrumbStateModelTests` — the FILE is named `...SequenceTests.cs` while
the TYPE it contributes to is `BreadcrumbStateModelTests`. Its own header comment states the reason:
"Split from BreadcrumbStateModelTests.cs so each file stays under the 500-line limit". A
fully-qualified-name filter selects by type, not by file, so no filter spelled
`BreadcrumbStateModelSequenceTests` can ever select these tests.

Verified: `grep -rln "partial class BreadcrumbStateModelTests" UtilitiesCS.Test/` returns exactly two
files — `BreadcrumbStateModelSequenceTests.cs` and `BreadcrumbStateModelTests.cs` — so the partial type
spans those two files and no others.

Resolution applied, per this plan's instruction to resolve every member BY NAME: the run of record uses
`FullyQualifiedName~BreadcrumbStateModelTests`, which selects the whole partial type and therefore
every test the named file contributes. The substring does not collide with
`BreadcrumbStateModelSelectorTests` (the literal `BreadcrumbStateModelTests` does not occur inside
`BreadcrumbStateModelSelectorTests`), which `P7-T5` covers separately.

### Test result

`Test Run Successful.` Counts read from the TRX `<ResultSummary><Counters>` element at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/trx/p7-t6/results.trx`:

| Metric | Value |
|---|---:|
| total | 32 |
| executed | 32 |
| passed | **32** |
| **failed** | **0** |

Total time 2.2472 seconds.

All twelve `[TestMethod]` members declared in `BreadcrumbStateModelSequenceTests.cs` were confirmed
present in the TRX with `outcome="Passed"`, each appearing exactly once:

| # | Method | TRX outcome |
|---:|---|---|
| 1 | `Sequence_CollapseReExpandCollapse_TransitionsDeterministically` | Passed |
| 2 | `Sequence_ExpandListSubfoldersThenCollapse_ClearsTheList` | Passed |
| 3 | `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges` | Passed |
| 4 | `RightArrow_OnCollapsedRow_ReExpandsBeforeLeafExpansion` | Passed |
| 5 | `Arrows_WithNoSelection_AreUnhandled` | Passed |
| 6 | `SelectSubfolder_OutOfRangeIndex_Throws` | Passed |
| 7 | `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses` | Passed |
| 8 | `AddSuggestionRow_NullSegmentInChain_Throws` | Passed |
| 9 | `Clear_RemovesRowsAndSelection` | Passed |
| 10 | `ReplaceRows_NullRows_Throws` | Passed |
| 11 | `ReplaceRows_PreservesSelectionWhenIndexStillValid` | Passed |
| 12 | `ReplaceRows_ClearsSelectionWhenIndexBeyondNewCount` | Passed |

These are the #400 residual state-transition sequences the criterion is about: collapse / re-expand,
subfolder listing and clearing, Left and Right arrow handled/unhandled outcomes, out-of-range subfolder
rejection, and the #398 atomic `ReplaceRows` selection-preservation behavior. Every one of them
continues to pass after this feature's `BreadcrumbStateModel` changes and its
`BreadcrumbStateModel.Row.cs` split.

### Read-only confirmation

`git status --porcelain --untracked-files=all -- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`
produced **no output**, and
`git diff --name-only 61edc19befcf6c4e95b5acd32542f2dcdab41b78 HEAD -- <that path>` is likewise empty, so
the file is unmodified both in the working tree and across the whole feature change set.

### Degradation status

The `P0-T15` `BASELINE_FAILURE_SET` is EMPTY, so the conditional degradation is **unavailable** and the
gate stands at its primary condition `failed 0`, which was met absolutely.

### Consequence recorded for `P8-T5`

`P8-T5` lists `BreadcrumbStateModelSequenceTests` among four test classes "this plan does NOT write",
excluded from its second clause. That exclusion was written against the file name. Because the type is
actually `BreadcrumbStateModelTests` — which IS one of the six owned test classes — the tests in this
file are in practice governed by the STRICTER of the two clauses in `P8-T5`, not the weaker one. That is
a tightening, not a weakening, of the gate, and the run above shows the stricter condition is met.

**AC-22 disposition: SATISFIED.**
