# [P8-T2] [expect-fail] `ToggleGroupConv_WithNoMatchingOriginal_DoesNotSubscriptWithMinusOne`

Timestamp: 2026-08-26T10-45

Issue #470 defect 1, driven end to end through the string overload of `ToggleGroupConv`.

Command:

```
dotnet tool run csharpier check .                                       # EXIT_CODE 0, 1,524 files
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~ToggleGroupConv_WithNoMatchingOriginal_DoesNotSubscriptWithMinusOne" `
    /Logger:"trx;LogFileName=p8-t2.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p8-t2
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

Build precondition: `EXIT_CODE 0`, 0 errors.

Test run: `Test Run Failed. Total tests: 1  Failed: 1`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p8-t2/p8-t2.trx`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `1`, as the task's acceptance requires.

## Recorded failure and call chain

```
Did not expect any exception because a conversation whose original is gone must be a no-op on the
UI event path, not an ArgumentOutOfRangeException raised into VSTO, but found
System.ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the
size of the collection.
   at System.ThrowHelper.ThrowArgumentOutOfRangeException(...)
   at QuickFiler.Controllers.QfcCollectionController.PromoteFirstChild(String originalId,
      Int32& childCount) in <repo-root>\QuickFiler\Controllers\QfcCollectionController.cs:line 1872
   at QuickFiler.Controllers.QfcCollectionController.ToggleGroupConv(String originalId)
      in <repo-root>\QuickFiler\Controllers\QfcCollectionController.cs:line 1535
```

The chain is the point of this test. `ToggleGroupConv(string)` at `:1535` calls `PromoteFirstChild`
only because its own `FindIndex` already returned `-1`, and `PromoteFirstChild` then subscripts with
its own `-1` at `:1872`. This is the caller-side reachability proof that P8-T1's direct call cannot
give: the defect is not merely present on a private path, it is reachable from the public member
that the item controller's collapse action invokes.

## Arrangement

A single group whose mocked item controller reports a `null` `ConvOriginID` and a `Mail.EntryID` of
`other-1`, and a request to toggle `missing-original`. Consequences inside `ToggleGroupConv`:

- `childCount` is `0`, because no group's `ConvOriginID` matches. The collapse branch guarded by
  `childCount > 0` is therefore never entered, which is what keeps this test free of the WinForms
  work in `ToggleGroupConv(int, int)`.
- `indexOriginal` is `-1`, because no group's `Mail.EntryID` matches.
- Control therefore reaches `PromoteFirstChild`, and after the fix it must also survive the
  subsequent `ChangeConversationSilently(indexOriginal, true)` call, which is the second negative
  subscript on this path.

## Two subscripts, one test

The recorded pre-fix failure is raised by the first of the two negative subscripts on this path, so
the second one is unreached in this TRX. Both are guarded by P8-T3:

| Site | Statement | Guard added |
|---|---|---|
| `PromoteFirstChild` | `_itemGroups[indexOriginal].ItemViewer` | return `-1` when the lookup misses |
| `ToggleGroupConv(string)` | `ChangeConversationSilently(indexOriginal, true)` | return when the promoted index is `-1` |
| `ChangeConversationSilently(int, bool)` | `_itemGroups[indexOriginal]` | return when the index is outside the list bounds |

The third guard is defence in depth for the same class of caller, per D4.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 14 substitutions. Post-sanitisation the
file contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
