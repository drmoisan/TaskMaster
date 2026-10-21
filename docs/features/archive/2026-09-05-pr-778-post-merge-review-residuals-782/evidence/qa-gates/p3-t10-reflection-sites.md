# QA Gate — AC5 Reflection-Site Reduction (P3-T10)

Timestamp: 2026-09-05T22-28

Command:

```powershell
$files = Get-ChildItem -Path . -Recurse -File -Filter '*.cs' |
    Where-Object { -not ($_.FullName.Contains('\obj\') -or $_.FullName.Contains('\bin\')) }
$files | ForEach-Object {
    $p = Resolve-Path -LiteralPath $_.FullName -Relative
    Get-Content -LiteralPath $_.FullName |
        Select-String -SimpleMatch '"_dispatcher"' |
        ForEach-Object { "$p : $($_.LineNumber) : $($_.Line.Trim())" }
}
```

```text
git diff --name-only pre-782-base..HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
git status --porcelain --untracked-files=all -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
```

1617 source files were scanned. Build output under `\obj\` and `\bin\` is excluded by an exact
path-segment test, matching the P0-T13 baseline method exactly so the before-figure and the
after-figure are produced by the same counting method.

The scan count is 3 higher than the 1614 the P0-T13 baseline recorded. Two of the three are
attributable to this delivery and were confirmed against the tree: `git diff --name-status
pre-782-base..HEAD -- '*.cs'` lists exactly one added file,
`UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs`, and the only scanned `.cs`
file not tracked by git is `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs`. The remaining
difference of one is not attributable from the evidence available here: `git ls-files -- '*.cs'`
reports 1616 tracked files at HEAD, which with the one untracked file accounts for all 1617 scanned,
so the difference lies in the baseline's own 1614 rather than in any file this delivery added or
removed. It is recorded rather than explained, and it does not affect this gate: the gate asserts a
match count, and both runs enumerate the same directory tree under the same exclusion rule.

The census searches the single-line token with its enclosing double quotes. The conjunction
`GetField("_dispatcher"` is deliberately not used, for the reason recorded in the P0-T13 baseline:
CSharpier wraps every acquisition so the two parts never share a line.

EXIT_CODE: 0

The PowerShell pipeline is composed entirely of cmdlets and sets no `$LASTEXITCODE`; it completed
without a terminating or non-terminating error, which is the success condition for this gate. Both
git spans exited 0 and each returned zero lines of output.

Output Summary:

## `"_dispatcher"` — exactly 2 lines, reduced from the 6 recorded in the P0-T13 baseline

| File | Line | Matched text |
|---|---|---|
| `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` | 117 | `"_dispatcher",` |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | 136 | `"_dispatcher",` |

The two survivors are exactly the two the acceptance condition names: the new `UtilitiesCS.Test`
install scope, and the pre-existing `QuickFiler.Test` fixture that a separate assembly must keep
because `UtilitiesCS/Properties/AssemblyInfo.cs` does not grant `InternalsVisibleTo` to
`QuickFiler.Test`.

## Before-figure from `evidence/baseline/p0-t13-reflection-census.md` — 6 lines

| File | Line | Disposition in this delivery |
|---|---|---|
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 128 | removed by P3-T3 |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | 422 | moved to `ProgressTracker_ReportAndViewerTests.cs` by the Phase 2 split, then removed by P3-T5 |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | 139 | removed by P3-T6 |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | 145 | removed by P3-T7 |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | 41 | removed by P3-T9 |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | 136 | retained — survivor |

Four of the five removed sites were replaced by a `using` over the shared
`UiThreadDispatcherScope`; the fifth, in `QuickFiler.Test`, was replaced by a read of the existing
`UiThreadDispatcherFixture` accessor in the same assembly.

## Surviving QuickFiler fixture was neither committed nor modified

```text
git diff --name-only pre-782-base..HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
<zero lines>

git status --porcelain --untracked-files=all -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
<zero lines>
```

Both spans are required. Phase 3 is not committed at the time this gate runs, so the diff alone
could not observe an uncommitted worktree modification; the porcelain span supplies that
observation, and the diff supplies the committed-history observation the porcelain span cannot.
