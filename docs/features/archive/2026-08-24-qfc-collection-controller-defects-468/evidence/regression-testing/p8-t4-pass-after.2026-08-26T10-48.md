# [P8-T4] Pass-after run for issue #470 defect 1

Timestamp: 2026-08-26T10-48

Command:

```
dotnet tool run csharpier check .                                       # EXIT_CODE 0, 1,524 files
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~PromoteFirstChild_WithNoMatchingChild_ReturnsMinusOneWithoutSubscripting|FullyQualifiedName~ToggleGroupConv_WithNoMatchingOriginal_DoesNotSubscriptWithMinusOne" `
    /Logger:"trx;LogFileName=p8-t4.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p8-t4
```

EXIT_CODE: 0

## Output Summary

`Test Run Successful. Total tests: 2  Passed: 2`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p8-t4/p8-t4.trx`:

```
total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Passed count is exactly `2` and failed count is exactly `0`, as the task's acceptance requires.

## Red-to-green mapping

| Test | Fail-before artifact | Pre-fix observation | Post-fix |
|---|---|---|---|
| `PromoteFirstChild_WithNoMatchingChild_ReturnsMinusOneWithoutSubscripting` | `p8-t1-fail-before.2026-08-26T10-45.md` | `ArgumentOutOfRangeException` at `QfcCollectionController.cs:1872` | passed |
| `ToggleGroupConv_WithNoMatchingOriginal_DoesNotSubscriptWithMinusOne` | `p8-t2-fail-before.2026-08-26T10-45.md` | same exception, reached via `ToggleGroupConv` at `:1535` | passed |

Both red states were recorded before P8-T3 was written.

## What P8-T3 changed

Three guards in `QuickFiler/Controllers/QfcCollectionController.cs`, all using the same early-return
idiom Phase 7 introduced:

1. **`PromoteFirstChild`** returns the sentinel `-1` when its own `FindIndex` misses, logging one
   warning, and leaves the caller's `ref int childCount` untouched because no child was promoted.
   Per D4 this is a sentinel return and not a typed throw: the member sits on the VSTO UI event
   path and the state is recoverable.
2. **`ToggleGroupConv(string)`** returns immediately when the promoted index is `-1`. With no
   original and no promotable child there is nothing to check and nothing to collapse.
3. **`ChangeConversationSilently(int, bool)`** returns without action when the index is outside the
   group-list bounds, treating a null list as out of bounds as well.

The third guard is not redundant with the second. The overload is `public` and reachable from
callers other than `ToggleGroupConv`; guarding only the caller would leave the same subscript
defect one call site away. It also covers the upper bound, not just the negative sentinel.

## Post-fix behaviour is silent, not merely non-throwing

Both members now return without touching the group list, so a conversation whose original has been
filed away is a no-op with one log line rather than an exception surfacing in Outlook. The
`childCount` assertion in the first test is what distinguishes "returned the sentinel" from
"returned the sentinel after consuming a child that does not exist".

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 13 substitutions. Post-sanitisation the
file contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
