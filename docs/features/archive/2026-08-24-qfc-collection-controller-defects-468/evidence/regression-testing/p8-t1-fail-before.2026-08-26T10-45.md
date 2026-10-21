# [P8-T1] [expect-fail] `PromoteFirstChild_WithNoMatchingChild_ReturnsMinusOneWithoutSubscripting`

Timestamp: 2026-08-26T10-45

Issue #470 defect 1 (a missing conversation original is used as a subscript).

Command:

```
dotnet tool run csharpier check .                                       # EXIT_CODE 0, 1,524 files
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~PromoteFirstChild_WithNoMatchingChild_ReturnsMinusOneWithoutSubscripting" `
    /Logger:"trx;LogFileName=p8-t1.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p8-t1
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

Build precondition: `EXIT_CODE 0`, 0 errors.

Test run: `Test Run Failed. Total tests: 1  Failed: 1`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p8-t1/p8-t1.trx`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `1`, as the task's acceptance requires.

## Acceptance: the observed failure is named `ArgumentOutOfRangeException`

```
Did not expect any exception because a missing original must be handled, not subscripted, but found
System.ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the
size of the collection.
   at System.ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument argument,
      ExceptionResource resource)
   ...
   at QuickFiler.Controllers.QfcCollectionController.PromoteFirstChild(String originalId,
      Int32& childCount) in <repo-root>\QuickFiler\Controllers\QfcCollectionController.cs:line 1872
```

Line 1872 is `var itemViewer = _itemGroups[indexOriginal].ItemViewer;`, the statement immediately
following the `FindIndex` lookup. The exception type is `ArgumentOutOfRangeException` because
`_itemGroups` is a `List<QfcItemGroup>` (`QfcCollectionController.cs:245`), whose indexer range-checks
and throws that type for a negative index.

## Arrangement

Two groups whose mocked `IQfcItemController` reports a `null` `ConvOriginID` and a `Mail.EntryID`
that does not match the requested identifier. `PromoteFirstChild("missing-original", ref childCount)`
is then called directly on an uninitialized controller.

Both mock members are configured explicitly. A loose mock returns `null` for `Mail`, and the
conversation lookups dereference `Mail.EntryID`; an unconfigured mock would fail with
`NullReferenceException` and mask the index defect this test targets.

The call is wrapped in a delegate that declares its own `int childCount` local, because C# does not
permit a `ref` parameter of the enclosing method to be used inside a lambda. The delegate copies the
post-call value out so the test can assert the count was not decremented.

## Note on the test file at this commit

`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs` was rewritten
before this run to a more compact documentation style, taking it from 461 to 333 lines with this
test included. No test name, arrangement, act, or assertion changed; only XML documentation prose
and the wording of `because:` reasons were shortened.

The reason is the repository's hard 500-line file cap. D12 assigns issue #470 defects 1, 2 and 3 to
this single file, and the remaining tests for P8-T2 and P9-T1 would have carried it past the cap.
Adding a sixth test file was rejected: D12 fixes the set at five files and P14-T11's acceptance
asserts exactly five consecutive `Compile Include` entries.

The six Phase 7 tests in the file are re-verified in full by the P8-T5 and P9-T4 suite runs.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 13 substitutions. Post-sanitisation the
file contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
