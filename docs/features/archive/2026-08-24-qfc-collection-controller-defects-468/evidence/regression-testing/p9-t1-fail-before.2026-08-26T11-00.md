# [P9-T1] [expect-fail] `SetVisualDigits_WithNullItemController_SkipsTheGroupWithoutThrowing`

Timestamp: 2026-08-26T11-00

Issue #470 defect 3 (inconsistent guarding inside the `SetVisualDigits` group loop).

Command:

```
dotnet tool run csharpier check .                                       # EXIT_CODE 0, 1,524 files
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~SetVisualDigits_WithNullItemController_SkipsTheGroupWithoutThrowing" `
    /Logger:"trx;LogFileName=p9-t1.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p9-t1
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

Build precondition: `EXIT_CODE 0`, 0 errors.

Test run: `Test Run Failed. Total tests: 1  Failed: 1`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p9-t1/p9-t1.trx`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `1`, as the task's acceptance requires.

## Acceptance: the inner exception is `NullReferenceException`

```
Expected captured to be <null> because issue #470 defect 3 requires a group with no controller or
no viewer to be skipped rather than dereferenced, but found System.NullReferenceException: Object
reference not set to an instance of an object.
   at QuickFiler.Controllers.QfcCollectionController.<>c__DisplayClass38_1.<SetVisualDigits>b__1(
      QfcItemGroup grp) in <repo-root>\QuickFiler\Controllers\QfcCollectionController.cs:line 145
   at QuickFiler.Controllers.QfcCollectionController.SetVisualDigits(Int32 digits)
      in <repo-root>\QuickFiler\Controllers\QfcCollectionController.cs:line 143
```

The variable asserted on is the **inner** exception. `MethodInfo.Invoke` wraps every failure from
the target in `TargetInvocationException`; the test catches that wrapper and asserts on
`wrapper.InnerException`, so the recorded message names `NullReferenceException` directly rather
than the reflection wrapper. That unwrapping is what P9-T1's acceptance requires.

Line 145 is `grp.ItemController.ItemNumberDigits = digits;` — the first statement of the loop body,
inside the `ForEach` lambda that line 143 dispatches.

## Pre-fix source state

```
_itemGroups.ForEach(grp =>
{
    grp.ItemController.ItemNumberDigits = digits;
    grp.ItemViewer.LblItemNumber.Text =
        grp.ItemController?.ItemNumber.ToString(format) ?? 0.ToString(format);
});
```

The inconsistency is visible in three consecutive lines. `grp.ItemController` is dereferenced
unguarded on the first line and then null-conditionally on the third, while `grp.ItemViewer` is
dereferenced unguarded on the second. The null-conditional operator on line three is therefore
unreachable protection: if `grp.ItemController` were null, line one has already thrown.

## Arrangement, and why it has two groups

The list holds two groups, both with a null `ItemViewer`:

1. `new QfcItemGroup()` — the default group P9-T1 specifies: both `ItemController` and `ItemViewer`
   are null. This is the group that raises the recorded pre-fix `NullReferenceException`.
2. `new QfcItemGroup { ItemController = liveController.Object }` — a mocked controller with a null
   viewer.

The second group is present from the outset, rather than being added after the fix, so that the
committed test is byte-for-byte the test that produced this red state. It serves two purposes:

- It makes a controller-only guard visibly insufficient, which is precisely what P9-T2's task text
  warns about: guard only the first dereference and execution reaches `grp.ItemViewer.LblItemNumber`
  on the next line with the same arrangement, and throws again.
- It carries P9-T3's additional assertion. Because every group's viewer is null, any attempt to
  write viewer text would throw, so completing without an exception already proves no viewer text
  was written; `VerifySet(item => item.ItemNumberDigits = It.IsAny<int>(), Times.Never())` on the
  live controller adds that the group was skipped *before* the controller write rather than after
  it.

`ItemViewer` is a concrete WinForms `UserControl`, not an interface, so it cannot be mocked and must
not be constructed in a unit test. The null-viewer arrangement plus the completion argument is the
strongest observation available without a GUI.

The loaded-email guard `EmailsLoaded > 0` passes because `EmailsLoaded` is `_itemGroups?.Count ?? 0`,
which is `2` here. The controller is allocated through
`QfcCollectionControllerTestSupport.CreateUninitializedController()`, which injects `_digits = 1`;
without that, per D14, the `Digits` getter would set `_digitRefreshNeeded` and route other code into
this same WinForms-bound path.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 13 substitutions. Post-sanitisation the
file contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
